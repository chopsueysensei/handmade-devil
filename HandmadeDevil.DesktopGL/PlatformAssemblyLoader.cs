using System;
using System.IO;
using System.Reflection;

namespace HandmadeDevil.DesktopGL
{
    public class PlatformAssemblyLoader : MarshalByRefObject
    {
        public Assembly LoadFrom( string path )
        {
            if( !File.Exists( path ) )
                throw new ArgumentException( "path '" + path + "' does not exist" );

            return Assembly.LoadFrom( path );
        }
    }
}
