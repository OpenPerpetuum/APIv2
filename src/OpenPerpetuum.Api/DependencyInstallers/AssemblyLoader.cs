using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace OpenPerpetuum.Api.DependencyInstallers
{
	internal sealed class AssemblyLoader
	{
		private static readonly Lazy<AssemblyLoader> lazy = new Lazy<AssemblyLoader>(() => new AssemblyLoader());
		public static AssemblyLoader Instance {  get { return lazy.Value; } }

		private readonly List<Assembly> runtimeAssemblies = new List<Assembly>();
		private AssemblyLoader()
		{
			var platform = Environment.OSVersion.Platform.ToString();
			runtimeAssemblies = DependencyContext.Default.GetRuntimeAssemblyNames(platform).Where(an => an.Name.StartsWith("OpenPerpetuum")).Select(Assembly.Load).ToList();
		}

		public ReadOnlyCollection<Assembly> RuntimeAssemblies => runtimeAssemblies.AsReadOnly();

	}
}
