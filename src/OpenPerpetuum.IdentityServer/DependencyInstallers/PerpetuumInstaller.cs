using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenPerpetuum.Core.DataServices.Context;
using OpenPerpetuum.Core.DataServices.CQRS;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Foundation.Utilities;
using OpenPerpetuum.IdentityServer.Configuration;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenPerpetuum.IdentityServer.DependencyInstallers
{
	internal static class Types
	{
		public static bool IsGenericType(this Type type) => type.GetTypeInfo().IsGenericType;
		public static bool IsAbstract(this Type type) => type.GetTypeInfo().IsAbstract;
		public static bool IsGenericTypeDefinition(this Type type) => type.GetTypeInfo().IsGenericTypeDefinition;
		public static Type BaseType(this Type type) => type.GetTypeInfo().BaseType;
		internal static bool IsConcreteType(Type serviceType) =>
			!serviceType.IsAbstract() && !serviceType.IsArray && serviceType != typeof(object) &&
			!typeof(Delegate).IsAssignableFrom(serviceType);
		internal static bool ServiceIsAssignableFromImplementation(Type service, Type implementation)
		{
			if (service.IsAssignableFrom(implementation))
			{
				return true;
			}

			if (implementation.IsGenericType() && implementation.GetGenericTypeDefinition() == service)
			{
				return true;
			}

			// PERF: We don't use LINQ to prevent unneeded memory allocations.
			// Unfortunately we can't prevent memory allocations while calling GetInterfaces() :-(
			foreach (Type interfaceType in implementation.GetInterfaces())
			{
				if (IsGenericImplementationOf(interfaceType, service))
				{
					return true;
				}
			}

			// PERF: We don't call GetBaseTypes(), to prevent memory allocations.
			Type baseType = implementation.BaseType() ?? (implementation != typeof(object) ? typeof(object) : null);

			while (baseType != null)
			{
				if (IsGenericImplementationOf(baseType, service))
				{
					return true;
				}

				baseType = baseType.BaseType();
			}

			return false;
		}
		private static bool IsVariantVersionOf(this Type type, Type otherType) =>
			type.IsGenericType()
			&& otherType.IsGenericType()
			&& type.GetGenericTypeDefinition() == otherType.GetGenericTypeDefinition()
			&& type.IsAssignableFrom(otherType);

		private static bool IsGenericImplementationOf(Type type, Type serviceType) =>
			type == serviceType
			|| serviceType.IsVariantVersionOf(type)
			|| (type.IsGenericType() && type.GetGenericTypeDefinition() == serviceType);
	}

	public static class PerpetuumInstaller
	{
		

		public static void RegisterPerpetuumApiTypes(this IServiceCollection container)
		{
			IEnumerable<Assembly> asm = AssemblyLoader.Instance.RuntimeAssemblies;
			// CAUTION: WE ARE USING THE DEFAULT DI CONTAINER HERE BECAUSE IDENTITYSERVER4 REQUIRES IT
			foreach (Type t in GetTypesToRegister(typeof(ICommandHandler<>), asm))
				container.AddScoped(t);

			foreach (Type t in GetTypesToRegister(typeof(IQueryHandler<,>), asm))
				container.AddScoped(t);
			// Manually wire the processors. These are being auto-wired to allow for inaccessible class registration
			container.AddScoped<ICommandProcessor, BasicCommandProcessor>();
			container.AddScoped<IQueryProcessor, BasicQueryProcessor>();
			container.AddScoped<IIdGeneratorService, IdGeneratorService>();
			container.AddSingleton<IGenericContext, GenericContext>();
			container.AddSingleton((sp) => GetPerpetuumDatabases(sp));
			container.AddScoped<ICoreContext, CoreContext>();
		}

		private static IEnumerable<Type> GetTypesToRegister(Type serviceType, IEnumerable<Assembly> assemblies, bool includeGenericTypeDefinitions = false)
		{
			var types =
				from assembly in assemblies.Distinct()
				where !assembly.IsDynamic
				from type in assembly.GetTypes()
				where Types.IsConcreteType(type)
				where includeGenericTypeDefinitions || !type.IsGenericTypeDefinition()
				where Types.ServiceIsAssignableFromImplementation(serviceType, type)
				select type;

			return types.ToArray();
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
