using IdentityModel;
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
using OpenPerpetuum.Api.Authorisation;
using OpenPerpetuum.Api.Configuration;
using OpenPerpetuum.Api.DependencyInstallers;
using OpenPerpetuum.Core.Extensions;
using OpenPerpetuum.Core.Killboard;
using OpenPerpetuum.Core.SharedIdentity.Authorisation;
using OpenPerpetuum.Core.SharedIdentity.Authorisation.Policy;
using OpenPerpetuum.Core.SharedIdentity.Configuration;
using System;

namespace OpenPerpetuum.Api
{
	public class ApiStartup
    {
        private IConfiguration Configuration { get; }

        public ApiStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var openIdConnectConfig = Configuration.GetSection("OpenIdConnect").Get<OpenIdConnectConfiguration>();

			services.Configure<OpenIdConnectConfiguration>(options => Configuration.GetSection("OpenIdConnect").Bind(options));
			services.Configure<DataProviderConfiguration>(options => Configuration.GetSection("DataProviders").Bind(options));
			services.Configure<TestData>(options => Configuration.GetSection("TestData").Bind(options));

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

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
				.AddIdentityServerAuthentication(options =>
				{
					options.Authority = openIdConnectConfig.IdentityServer;
					options.ApiName = IdentityConfig.API_Name;
					options.RequireHttpsMetadata = openIdConnectConfig.RequireHttpsMetadata;
					options.Validate();
				});

			services.AddAuthorization(options =>
			{
				options.AddPolicy(Scopes.Registration, builder =>
				{
					builder.RequireScope(Scopes.Registration);
				});
                options.AddPolicy(Scopes.ExternalKillboard, builder =>
                {
                    builder.RequireAssertion((context) => { return true; });
                });
                options.AddPolicy("RequiresLogin", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(JwtClaimTypes.Subject);
                });
            });

            services.AddTransient<IAuthorizationHandler, RequiresPermissionHandler>();
            services.AddSingleton<IAuthorizationPolicyProvider, RequiresPermissionPolicyProvider>();
            
			services.AddMemoryCache();
			// services.AddDistributedMemoryCache(); -- No Redis implementation yet

			services.RegisterPerpetuumApiTypes(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            ILogger startupLog = loggerFactory.CreateLogger("Startup");

			bool isDevMode = false, isHsts = false, isHttps = false;

            if (env.IsDevelopment())
            {
                isDevMode = true;
                app.UseDeveloperExceptionPage();
				//app.UseCors("development");
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
