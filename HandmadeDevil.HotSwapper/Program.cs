using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using HandmadeDevil.Core;
using System.Threading;

namespace HandmadeDevil.HotSwapper
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        static readonly string RelSolutionDir = "../../../../..";
        static readonly string GamePrjName = "HandmadeDevil.DesktopGL";
        static readonly string RelGameOutDir = GamePrjName + "/bin/DesktopGL/x86";
#if DEBUG
        static readonly string PrjConfig = "Debug";
#else
        static readonly string PrjConfig = "Release";
#endif

        static string _gamePrjOutDir;
        static string _gameAsmPath;
        static DateTime _gameAsmWriteTime;
        static bool _reloadAssembly;
        static AppDomain _gameDomain;
        static GameModule _gameInstance;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            // Look for game executable and associated resources in the main game build output dir.
            string solutionDir = Path.GetFullPath( RelSolutionDir );

            _gamePrjOutDir = Path.Combine( solutionDir, Path.Combine( RelGameOutDir, PrjConfig ) );
            _gameAsmPath = Path.Combine( _gamePrjOutDir, GamePrjName + ".exe" );
            _gameAsmWriteTime = new FileInfo( _gameAsmPath ).LastWriteTime;
            _reloadAssembly = true;


            while( _reloadAssembly )
            {
                _reloadAssembly = false;

                var t = new Thread( GameThread );
                t.Start();

                while( t.IsAlive && !CheckAssemblyUpdated() )
                    Thread.Sleep( 1000 );

                if( t.IsAlive )
                {
                    _reloadAssembly = true;
                    var wrapper = (_gameDomain.GetData( "GameWrapper" ) as IGameWrapper);
                    var gameState = wrapper.RetrieveGameStateAndExit();
                }

//                AppDomain.Unload( _gameDomain );
//                // Just in case
//                t.Abort();
            }

            // http://www.drdobbs.com/windows/launcher-mastering-your-own-domain/184405853

            // http://stackoverflow.com/questions/17225276/create-custom-appdomain-and-add-assemblies-to-it/17324102#17324102
            // http://stackoverflow.com/questions/658498/how-to-load-an-assembly-to-appdomain-with-all-references-recursively
            // http://stackoverflow.com/questions/2100296/how-can-i-switch-net-assembly-for-execution-of-one-method/2101048#2101048
        }

        static bool CheckAssemblyUpdated()// GameTime gameTime )
        {
            bool res = false;

            // Fetch game EXE and see if it's been updated
            var newWriteTime = new FileInfo( _gameAsmPath ).LastWriteTime;
            if( newWriteTime != _gameAsmWriteTime )
            {
                _gameAsmWriteTime = newWriteTime;
                res = true;
            }
            
            return res;
        }

        [STAThread]
        static void GameThread()
        {
            _gameDomain = SetupAppDomain( _gamePrjOutDir, GamePrjName );
//            _gameInstance = LoadModule<GameModule>( _gameAsmPath );
            _gameDomain.ExecuteAssembly( _gameAsmPath );

//            if( _gameInstance == null )
//                throw new Exception( "Could not load game assembly at '" + _gameAsmPath + "'!" );
//
//            using( _gameInstance )
//            {
////                _gameInstance.OnUpdate += CheckAssemblyUpdated;
//                _gameInstance.Run();
//            }
//
//            _gameInstance = null;
            Console.WriteLine( "GameThread exiting." );
        }

        static AppDomain SetupAppDomain( string assemblyBasePath, string assemblyName )
        {
            var domainSetup = new AppDomainSetup();
            // Enable Shadow Copy to avoid file locking errors
            domainSetup.ApplicationBase = assemblyBasePath;
            domainSetup.ShadowCopyFiles = "true";
            domainSetup.ShadowCopyDirectories = assemblyBasePath;

            return AppDomain.CreateDomain( assemblyName + new Random().Next( 1000 ), null, domainSetup );
        }

//        static T LoadModule<T>( string assemblyPath ) where T : class
//        {
//            // CreateInstanceAndUnwrap a MarshalByRefObject proxy loader that will instantiate the game class
//            var assemblyLoader = (PlatformAssemblyLoader)_gameDomain.CreateInstanceAndUnwrap(
//                typeof( PlatformAssemblyLoader ).Assembly.FullName,
//                typeof( PlatformAssemblyLoader ).FullName
//            );
//            // Load an assembly in the LoadFrom context
//            var asm = assemblyLoader.LoadFrom( assemblyPath );
//
//            Type gameType = null;
//            // Create game instance
//            foreach( Type type in asm.GetTypes() )
//            {
//                if( typeof( T ).IsAssignableFrom( type ) )
//                {
//                    gameType = type;
//                    break;
//                }
//            }
//
//            return Activator.CreateInstance( gameType ) as T;
//        }
    }
}
