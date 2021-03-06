﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenPerpetuum.Core.DataServices.Context;
using OpenPerpetuum.Core.DataServices.CQRS;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Foundation.Utilities;
using OpenPerpetuum.IdentityServer.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenPerpetuum.IdentityServer.DependencyInstallers
{
	public static class PerpetuumInstaller
	{
		
		public static void RegisterPerpetuumApiTypes(this IServiceCollection container, IConfiguration configuration)
		{
			IEnumerable<Assembly> asm = AssemblyLoader.Instance.RuntimeAssemblies;
			Type commandHandlerType = typeof(ICommandHandler<>);
			Type queryHandlerType = typeof(IQueryHandler<,>);

			container.Scan(scan => scan
				.FromAssemblies(asm)
				.AddClasses(classes => classes.AssignableTo(commandHandlerType))
					.AsImplementedInterfaces()
					.WithTransientLifetime()
				.AddClasses(classes => classes.AssignableTo(queryHandlerType))
					.AsImplementedInterfaces()
					.WithTransientLifetime());

			// Manually wire the processors. These are being auto-wired to allow for inaccessible class registration
			container.AddScoped<ICommandProcessor, BasicCommandProcessor>();
			container.AddScoped<IQueryProcessor, BasicQueryProcessor>();
			container.AddScoped<IIdGeneratorService, IdGeneratorService>();
			container.AddSingleton<IGenericContext, GenericContext>();
			container.AddSingleton((sp) => GetPerpetuumDatabases(sp, configuration));
			container.AddScoped<ICoreContext, CoreContext>();
		}

		private static IDataContext GetPerpetuumDatabases(IServiceProvider container, IConfiguration configuration)
		{
			IEnumerable<Assembly> asm = AssemblyLoader.Instance.RuntimeAssemblies;
			var providers = asm.SelectMany(a => a.DefinedTypes).Where(type => typeof(IDatabaseProvider).IsAssignableFrom(type.AsType()));

			IDataContext dataContext = new DataContext();
			var databaseConfigurations = container.GetService(typeof(IOptions<DataProviderConfiguration>)) as IOptions<DataProviderConfiguration>;

			foreach (var dbConfig in databaseConfigurations.Value.Databases)
			{
				IDatabaseProvider provider;
				try
				{
					var providerType = providers.Single(ti => ti.Name.StartsWith(dbConfig.Type));
                    var connectionString = configuration.GetConnectionString(dbConfig.ConnectionId);
					provider = Activator.CreateInstance(providerType.AsType(), new object[] { dbConfig.ProviderName, connectionString }) as IDatabaseProvider;
				}
				catch (InvalidOperationException ioe)
				{
					throw new InvalidOperationException($"Could not find a single DB provider of type \"{dbConfig.ProviderName}\"", ioe);
				}
				catch (Exception exc)
				{
					throw new InvalidOperationException($"Unable to activate an instance of type \"{dbConfig.ProviderName}\". Please see inner exception details.", exc);
				}
				dataContext.AddDataContext(provider);
			}

			return dataContext;
		}

		
	}
}
