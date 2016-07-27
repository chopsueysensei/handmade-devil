using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;



namespace Onirika.Utils
{
	public interface IAppDomain
	{
		Assembly LoadAssembly( string assemblyPath );
	}

	public interface IAssemblyLoader
	{
		IAppDomain CreateAssemblyDomain( string friendlyName );
		void UnloadAssemblyDomain( IAppDomain assemblyDomain );
	}



	public static class ModuleManager
	{
		public static IAssemblyLoader AssemblyLoader { get; set; }

		static Dictionary<Type, IAppDomain> _typeMap = new Dictionary<Type, IAppDomain>();


		public static T LoadModule<T>( string assemblyPath ) where T : class
		{
			IAppDomain asmDomain = AssemblyLoader.CreateAssemblyDomain( assemblyPath );
			Assembly asm = asmDomain.LoadAssembly( assemblyPath );

			var types = GetLoadableTypes( asm );

			foreach( Type type in types )
			{
				if( typeof(T).IsAssignableFrom( type ) )
				{
					_typeMap.Add( typeof(T), asmDomain );
					return Activator.CreateInstance( type ) as T;
				}
			}

			return null;
		}

		public static void UnloadModule<T>()
		{
			AssemblyLoader.UnloadAssemblyDomain( _typeMap[typeof(T) ]);
		}

		/// Load assembly directly, without the possibility of reloading later
		public static T LinkModule<T>( string assemblyString ) where T : class
		{
			Assembly moduleAsm = Assembly.Load( assemblyString );

			foreach (Type type in moduleAsm.GetTypes())
			{
				if( typeof(T).IsAssignableFrom( type ) )
					return Activator.CreateInstance( type ) as T;
			}

			return null;
		}

		private static IEnumerable<Type> GetLoadableTypes( Assembly assembly )
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");
			
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
				return e.Types.Where(t => t != null);
			}
		}
	}
}

