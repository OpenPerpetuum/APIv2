using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.Extensions.Options;
using OpenPerpetuum.Api.Authorisation;
using OpenPerpetuum.Api.Configuration;
using OpenPerpetuum.Api.DependencyInstallers;
using OpenPerpetuum.Core.DataServices;
using OpenPerpetuum.Core.Extensions;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

			services
				.AddEntityFrameworkInMemoryDatabase()
				.AddDbContext<ApplicationContext>(options => options.UseInMemoryDatabase(nameof(ApplicationContext)));

            var openIdConnectConfig = Configuration.Get<OpenIdConnectConfiguration>();

			services.Configure<OpenIdConnectConfiguration>(options => Configuration.GetSection("OpenIdConnect").Bind(options));
			services.Configure<DataProviderConfiguration>(options => Configuration.GetSection("DataProviders").Bind(options));

			services.AddAuthentication(sharedOptions =>
			{
				sharedOptions.DefaultScheme = "ServerCookie";
			})
			.AddCookie("ServerCookie", options =>
			{
				options.Cookie.Name = CookieAuthenticationDefaults.CookiePrefix + "ServerCookie";
				options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
				options.LoginPath = new PathString("/authorisation/login");
				options.LogoutPath = new PathString("/authorisation/logout");
			})
			.AddOAuthValidation()
			.AddOpenIdConnectServer(config =>
			{
				config.ProviderType = typeof(AuthorisationProvider);
				config.AuthorizationEndpointPath = openIdConnectConfig.AuthorisationPath;
				config.LogoutEndpointPath = openIdConnectConfig.LogoutPath;
				config.TokenEndpointPath = openIdConnectConfig.TokenPath;
				config.UserinfoEndpointPath = openIdConnectConfig.UserInfoPath;

#if DEBUG // Development stuff
				config.ApplicationCanDisplayErrors = true;
				config.AllowInsecureHttp = openIdConnectConfig.AllowInsecureHttp;
#endif
			});

			services.AddScoped<AuthorisationProvider>();

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

			services.AddMvc(setupAction =>
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
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddDistributedMemoryCache();

            services.EnableSimpleInjectorCrossWiring(container);
            services.UseSimpleInjectorAspNetRequestScoping(container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
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
			app.UseSession();
			app.UseStaticFiles();
			app.UseMvc();

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

			// Cross-Wiring
			// The HTTP Context Accessor requires special wiring. Don't enable unless we *really* need it. Bad Practice.
			// container.CrossWire<IHttpContextAccessor>(app);
			// The CacheClient requires access to the HTTP Context Accessor.
			// container.CrossWire<ICacheClient>(app);

			container.CrossWire<IOptions<DataProviderConfiguration>>(app);
			container.CrossWire<ILoggerFactory>(app);
            container.CrossWire<IDistributedCache>(app);
            
            // Singleton Registrations
            container.RegisterInstance<Func<IViewBufferScope>>(() => app.GetRequestService<IViewBufferScope>());
            container.RegisterInstance(typeof(IServiceProvider), container); // Self registration; basically enables witchcraft...

			// Add Middleware here!
			// Note that the order in which you enable them in Configure/ConfigureServices is important!

			// Add "Other Stuff" here! (I typically use Dependency Installers rather than list all my deps here)
			container.RegisterPerpetuumApiTypes();
        }

        // This should stop the API from returning 302 redirects (attempts to present you a login page) for 401 Unauthorised
        private static Func<RedirectContext<CookieAuthenticationOptions>, Task> ReplaceRedirector(HttpStatusCode statusCode, Func<RedirectContext<CookieAuthenticationOptions>, Task> existingRedirector) =>
            context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    context.Response.StatusCode = (int)statusCode;
                    return Task.CompletedTask;
                }
                return existingRedirector(context);
            };
    }
}
