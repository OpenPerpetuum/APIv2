using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenPerpetuum.Core.DataServices;
using OpenPerpetuum.Core.Extensions;
using OpenPerpetuum.IdentityServer.Configuration;
using OpenPerpetuum.IdentityServer.DependencyInstallers;
using OpenPerpetuum.IdentityServer.Stores;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace OpenPerpetuum.IdentityServer
{
    public class IdentityStartup
	{
		private IConfiguration Configuration { get; }

		public IdentityStartup(IConfiguration config)
		{
            Configuration = config;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddOptions();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.Configure<DataProviderConfiguration>(options => Configuration.GetSection("DataProviders").Bind(options));
            var dataProviders = Configuration.GetSection("DataProviders").Get<DataProviderConfiguration>();
            DatabaseProviderConfiguration apiDataProvider = dataProviders.Databases.SingleOrDefault(db => string.Equals(db.ProviderName, "API"));

			services.RegisterPerpetuumApiTypes(Configuration);
			services.AddSingleton<ICache<Client>, InMemoryCache<Client>>();
            services.AddSingleton<ICache<Resource>, InMemoryCache<Resource>>();

            var identityConnectionString = Configuration.GetConnectionString(apiDataProvider.ConnectionId);
            var migrationsAssembly = typeof(IdentityStartup).GetTypeInfo().Assembly.GetName().Name; // The migration assembly is where the database update instructions exist

            services.AddIdentityServer()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                      b.UseSqlServer(identityConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                      b.UseSqlServer(identityConnectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                    options.EnableTokenCleanup = true;
                })
                .AddInMemoryCaching()
                .AddClientStoreCache<IdentityServer4.EntityFramework.Stores.ClientStore>()
                .AddResourceStoreCache<IdentityServer4.EntityFramework.Stores.ResourceStore>()
                .AddConfigurationStoreCache()
#if DEBUG
                .AddDeveloperSigningCredential();
#else
                .AddSigningCredential(GetCertificate(signingCertificate));
#endif
        }

        private X509Certificate2 GetCertificate(string certPath)
        {
            if (string.IsNullOrEmpty(certPath))
                throw new ArgumentException("Certificate path cannot be empty!", "certPath");

            var cert = new X509Certificate2(certPath);

            return cert;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
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

            // Perform the EF initialisation
            InitialiseSqlDatabase(app);

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
		}

        private void InitialiseSqlDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();

                if (!context.Clients.Any())
                {
                    foreach (var client in IdentityServerConfiguration.GetClients(Configuration))
                        context.Clients.Add(client.ToEntity());

                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in IdentityServerConfiguration.GetIdentityResources())
                        context.IdentityResources.Add(resource.ToEntity());

                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in IdentityServerConfiguration.GetApiResources())
                        context.ApiResources.Add(resource.ToEntity());

                    context.SaveChanges();
                }
            }
        }
    }
}
