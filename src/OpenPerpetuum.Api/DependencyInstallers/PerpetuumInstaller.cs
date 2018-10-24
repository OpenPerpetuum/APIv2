using Microsoft.Extensions.Options;
using OpenPerpetuum.Api.Configuration;
using OpenPerpetuum.Core.DataServices.Context;
using OpenPerpetuum.Core.DataServices.CQRS;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Foundation.Processing;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenPerpetuum.Api.DependencyInstallers
{
	public static class PerpetuumInstaller
	{
		public static void RegisterPerpetuumApiTypes(this Container container)
		{
			IEnumerable<Assembly> asm = AssemblyLoader.Instance.RuntimeAssemblies;
			container.Register(typeof(ICommandHandler<>), asm);
			// Example of how to add decorators to the handlers via IoC -> container.RegisterDecorator(typeof(ICommandHandler<>), typeof(Extension.DecoratedHandler<>));
			container.Register(typeof(IQueryHandler<,>), asm);

			// Manually wire the processors. These are being auto-wired to allow for inaccessible class registration
			container.Register<ICommandProcessor, BasicCommandProcessor>();
			container.Register<IQueryProcessor, BasicQueryProcessor>();
			container.Register<IIdGeneratorService, IdGeneratorService>();
			container.Register<IGenericContext, GenericContext>();
			container.RegisterSingleton(() => GetPerpetuumDatabases(container));
			container.Register<ICoreContext, CoreContext>();
		}

		private static IDataContext GetPerpetuumDatabases(IServiceProvider container)
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
					var providerType = providers.Single(ti => ti.ToFriendlyName().StartsWith(dbConfig.Type));
					provider = Activator.CreateInstance(providerType.AsType(), new object[] { dbConfig.ProviderName, dbConfig.Username, dbConfig.Password, dbConfig.Server, dbConfig.DefaultDatabase }) as IDatabaseProvider;
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
