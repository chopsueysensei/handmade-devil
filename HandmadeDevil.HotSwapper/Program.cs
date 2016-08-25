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

        static byte[] gameStateBuffer = null;



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

            // Main reload loop
            while( _reloadAssembly )
            {
                _reloadAssembly = false;

                var t = new Thread( GameThread );
                t.Start();

                // Continuously check thread is still alive (game has not exited) and assembly hasn't been updated in the meantime
                while( t.IsAlive && !CheckAssemblyUpdated() )
                    Thread.Sleep( 1000 );

                if( t.IsAlive )
                {
                    // If assembly was updated, obtain last game state and make it terminate
                    var wrapper = (_gameDomain.GetData( DomainDefs.DataKey_GameWrapper ) as IGameWrapper);
                    gameStateBuffer = wrapper.RetrieveGameStateAndExit();

                    _reloadAssembly = true;
                }
            }

            // http://www.drdobbs.com/windows/launcher-mastering-your-own-domain/184405853
        }

        static bool CheckAssemblyUpdated()
        {
            bool res = false;

            // Fetch game EXE and check last write time
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

            // Pass existing game state (if any)
            _gameDomain.SetData( DomainDefs.DataKey_GameState, gameStateBuffer );
            // Block thread while executing game's assembly
            _gameDomain.ExecuteAssembly( _gameAsmPath );

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
    }
}
