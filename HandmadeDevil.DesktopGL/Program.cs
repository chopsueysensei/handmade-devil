using HandmadeDevil.Core;
using System;

namespace HandmadeDevil.DesktopGL
{
    public class GameWrapper : MarshalByRefObject, IGameWrapper
    {
        private GameModule game;

        public GameWrapper( GameModule game )
        {
            this.game = game;
        }

        public byte[] RetrieveGameStateAndExit()
        {
            game.isPaused = true;
            var state = game.SerializeGameStateBinary();
            game.Exit();

            return state;
        }
    }



    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            HandmadeGame game = null;

#if DEBUG
            byte[] stateData = AppDomain.CurrentDomain.GetData( DomainDefs.DataKey_GameState ) as byte[];
            game = new HandmadeGame( stateData );

            // Create a wrapper around the game instance and make it available from current AppDomain
            var wrapper = new GameWrapper(game);
            AppDomain.CurrentDomain.SetData( DomainDefs.DataKey_GameWrapper, wrapper );
#else
            game = new HandmadeGame();
#endif

            using( game )
                game.Run();
        }
    }
}
