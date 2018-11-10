using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using OpenPerpetuum.Core.Extensions;
using OpenPerpetuum.Core.SharedIdentity.Configuration;
using OpenPerpetuum.IdentityServer.Configuration;
using OpenPerpetuum.IdentityServer.DependencyInstallers;
using OpenPerpetuum.IdentityServer.Stores;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using System;

namespace OpenPerpetuum.IdentityServer
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
			services.AddMvc();
			services.EnableSimpleInjectorCrossWiring(container);
			services.UseSimpleInjectorAspNetRequestScoping(container);
			
			services.AddOptions();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			var openIdConnectConfig = Configuration.GetSection("OpenIdConnect").Get<OpenIdConnectConfiguration>();

			services.Configure<OpenIdConnectConfiguration>(options => Configuration.GetSection("OpenIdConnect").Bind(options));
			services.Configure<DataProviderConfiguration>(options => Configuration.GetSection("DataProviders").Bind(options));

			services.RegisterPerpetuumApiTypes();
			services.AddSingleton<ICache<Client>, InMemoryCache<Client>>();

			services.AddScoped<ClientStore>();

			services.AddIdentityServer()
				.AddDeveloperSigningCredential()
				.AddInMemoryApiResources(IdentityConfig.GetApiResources())
				.AddInMemoryIdentityResources(IdentityConfig.GetIdentityResources())
				.AddClientStoreCache<CachingClientStore<ClientStore>>();

			
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

			container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
			container.AutoCrossWireAspNetComponents(app);
			//InitialiseContainer(app, loggerFactory);
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

			app.UseStaticFiles();

			app.UseIdentityServer();

			app.UseMvcWithDefaultRoute();
		}

		private void InitialiseContainer(IApplicationBuilder app, ILoggerFactory loggerFactory)
		{
			var logger = loggerFactory.CreateLogger("ContainerStartup");
			logger.LogInformation("Starting container initialisation");

			container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
			
			// CrossWire Magic
			container.AutoCrossWireAspNetComponents(app);

			// Singleton Registrations
			container.RegisterInstance<Func<IViewBufferScope>>(() => app.GetRequestService<IViewBufferScope>());
			container.RegisterInstance(typeof(IServiceProvider), container); // Self registration; basically enables witchcraft...

			// Add Middleware here!
			// Note that the order in which you enable them in Configure/ConfigureServices is important!

			// Add "Other Stuff" here! (I typically use Dependency Installers rather than list all my deps here)
			//container.RegisterPerpetuumApiTypes();
		}
	}
}
