using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Newtonsoft.Json;
using OpenPerpetuum.Api.Configuration;
using OpenPerpetuum.Api.DependencyInstallers;
using OpenPerpetuum.Core.Extensions;
using OpenPerpetuum.Core.Foundation.SharedConfiguration;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;
using System;

namespace OpenPerpetuum.Api
{
	public class Startup
    {
        private readonly Container container = new Container();
        private IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services, IHostingEnvironment env)
        {
            services.AddOptions();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var openIdConnectConfig = Configuration.GetSection("OpenIdConnect").Get<OpenIdConnectConfiguration>();

			services.Configure<OpenIdConnectConfiguration>(options => Configuration.GetSection("OpenIdConnect").Bind(options));
			services.Configure<DataProviderConfiguration>(options => Configuration.GetSection("DataProviders").Bind(options));

			services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
				.AddIdentityServerAuthentication(options =>
				{
					options.Authority = openIdConnectConfig.IdentityServer;
					options.ApiName = "OPAPI"
				})

			services.AddAuthorization(options =>
			{
				options.AddPolicy(OpenPerpetuumScopes.Registration, builder =>
				{
					builder.RequireScope(OpenPerpetuumScopes.Registration);
				});
			});
			
			services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(container));
			services.AddSingleton<IViewComponentActivator>(new SimpleInjectorViewComponentActivator(container));

			services.AddCors(options =>
			{
				options.AddPolicy("development", policy =>
				{
					policy
					.AllowAnyOrigin()
					.AllowCredentials()
					.AllowAnyHeader()
					.AllowAnyMethod();
				});
			});

			services.AddSession(sessionOptions =>
			{
				sessionOptions.Cookie.Name = ".OpenPerpetuum.APIv2.Session";
				sessionOptions.Cookie.HttpOnly = true;
#if RELEASE
				sessionOptions.IdleTimeout = TimeSpan.FromHours(2);
				sessionOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				sessionOptions.Cookie.SameSite = SameSiteMode.Strict;
#else
				sessionOptions.IdleTimeout = TimeSpan.FromHours(24);
				sessionOptions.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
				sessionOptions.Cookie.SameSite = SameSiteMode.None;				
#endif
			});

			services.AddMvcCore(setupAction =>
			{
#if DEBUG // Don't cache in debug mode
				setupAction.CacheProfiles.Add(
					key: "Never",
					value: new CacheProfile
					{
						Duration = -1,
						Location = ResponseCacheLocation.None,
						NoStore = true
					});
#endif
			})
			.SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
			.AddAuthorization()
			.AddJsonFormatters()
			.AddJsonOptions(options =>
			{
				options.SerializerSettings.Formatting = env.IsDevelopment() ? Formatting.Indented : Formatting.None;
			});

			services.AddMemoryCache();
			services.AddDistributedMemoryCache();

            services.EnableSimpleInjectorCrossWiring(container);
            services.UseSimpleInjectorAspNetRequestScoping(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            // Event log is only support on Windows systems
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                loggerFactory.AddEventLog(new EventLogSettings
                {
                    Filter = new Func<string, LogLevel, bool>((inString, inLevel) => { return inLevel >= LogLevel.Warning; }),
                    LogName = "Application",
                    MachineName = Environment.MachineName,
                    SourceName = "OP APIv2"
                }); // Only log warnings or above in the event log, regardless of debug settings.
            }

            ILogger startupLog = loggerFactory.CreateLogger("Startup");

            InitialiseContainer(app, loggerFactory);
            container.Verify();

			// Ensure all requests are scoped for the container
			app.Use(async (context, next) =>
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    await next();
                }
            });

			bool isDevMode = false, isHsts = false, isHttps = false;

            if (env.IsDevelopment())
            {
                isDevMode = true;
                app.UseDeveloperExceptionPage();
				app.UseCors("development");
            }
            else
            {
                isHsts = true;
                app.UseHsts();
				app.UseHttpsRedirection();
				isHttps = true;
            }            

            startupLog.LogInformation($"********************\n      Development mode: {isDevMode.ToEnabledString()}\n      HSTS mode:\t{isHsts.ToEnabledString()}\n      HTTPS mode:\t{isHttps.ToEnabledString()}\n      ********************");

			app.UseAuthentication();
			app.UseStaticFiles();
			app.UseMvc();
			//CancellationTokenSource tokenSource = new CancellationTokenSource();
			//Task.Run(() => RunPeriodically(() => PopulateApplications(container.GetRequiredService<IMemoryCache>(), container.GetRequiredService<IQueryProcessor>()), TimeSpan.FromMinutes(3), tokenSource.Token));

			//applicationLifetime.ApplicationStopping.Register(() => tokenSource.Cancel());

			UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), "pack", -1); // Don't fail with Azure packed claims

            startupLog.LogInformation("Initialisation complete");
        }

        private void InitialiseContainer(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger("ContainerStartup");
            logger.LogInformation("Starting container initialisation");

            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
			// MVC Autowire
			container.RegisterMvcControllers(app);
            container.RegisterMvcViewComponents(app);

			// CrossWire Magic
			container.AutoCrossWireAspNetComponents(app);

			// Singleton Registrations
			container.RegisterInstance<Func<IViewBufferScope>>(() => app.GetRequestService<IViewBufferScope>());
            container.RegisterInstance(typeof(IServiceProvider), container); // Self registration; basically enables witchcraft...

			// Add Middleware here!
			// Note that the order in which you enable them in Configure/ConfigureServices is important!

			// Add "Other Stuff" here! (I typically use Dependency Installers rather than list all my deps here)
			container.RegisterPerpetuumApiTypes();
		}
	}
}
