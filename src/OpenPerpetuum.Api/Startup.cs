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
using OpenPerpetuum.Core.SharedIdentity.Configuration;
using System;
using static OpenPerpetuum.Core.SharedIdentity.Configuration.IdentityConfig;

namespace OpenPerpetuum.Api
{
	public class Startup
    {
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
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var openIdConnectConfig = Configuration.GetSection("OpenIdConnect").Get<OpenIdConnectConfiguration>();

			services.Configure<OpenIdConnectConfiguration>(options => Configuration.GetSection("OpenIdConnect").Bind(options));
			services.Configure<DataProviderConfiguration>(options => Configuration.GetSection("DataProviders").Bind(options));

			services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
				.AddIdentityServerAuthentication(options =>
				{
					options.Authority = openIdConnectConfig.IdentityServer;
					options.ApiName = "OPAPI";
					options.RequireHttpsMetadata = !openIdConnectConfig.AllowInsecureHttp;
					options.Validate();
				});

			services.AddAuthorization(options =>
			{
				options.AddPolicy(Scopes.Registration, builder =>
				{
					builder.RequireScope(Scopes.Registration);
				});
			});
			
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
#if DEBUG
				options.SerializerSettings.Formatting = Formatting.Indented;
#else
				options.SerializerSettings.Formatting = Formatting.None;
#endif
			});

			services.AddMemoryCache();
			services.AddDistributedMemoryCache();

			services.RegisterPerpetuumApiTypes();
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
	}
}
