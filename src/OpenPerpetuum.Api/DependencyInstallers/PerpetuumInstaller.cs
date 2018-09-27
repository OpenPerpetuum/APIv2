using OpenPerpetuum.Core.DataServices.Context;
using OpenPerpetuum.Core.DataServices.CQRS;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Foundation.Processing;
using SimpleInjector;
using System.Collections.Generic;
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
			container.Register<IDataContext, DataContext>();
			container.Register<ICoreContext, CoreContext>();
		}
	}
}
