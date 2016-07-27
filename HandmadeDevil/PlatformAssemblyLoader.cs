using System;
using Onirika.Utils;
using System.Reflection;
using System.IO;

namespace HandmadeDevil
{
	public class PlatformAppDomain : IAppDomain
	{
		internal AppDomain appDomain { get; }

		public PlatformAppDomain( AppDomain appDomain )
		{
			this.appDomain = appDomain;
		}

		public Assembly LoadAssembly( string assemblyPath )
		{
			return appDomain.Load( File.ReadAllBytes( assemblyPath ) );
		}
	}

	public class PlatformAssemblyLoader : IAssemblyLoader
	{
		private AppDomainSetup _domainSetup;

		public PlatformAssemblyLoader()
		{
			_domainSetup = new AppDomainSetup();
			// Enable Shadow Copy to avoid file locking errors
			_domainSetup.ShadowCopyFiles = "true";
		}

		public IAppDomain CreateAssemblyDomain( string friendlyName )
		{
			var gameDomain = AppDomain.CreateDomain( friendlyName, null, _domainSetup );
			return new PlatformAppDomain( gameDomain );
		}

		public void UnloadAssemblyDomain( IAppDomain assemblyDomain )
		{
			AppDomain.Unload( (assemblyDomain as PlatformAppDomain).appDomain );
		}
	}
}

