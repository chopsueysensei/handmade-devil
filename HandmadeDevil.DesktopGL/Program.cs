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

        public IGameState RetrieveGameStateAndExit()
        {
//            var state = game.gameState;
            IGameState state = null;
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
            var game = new HandmadeGame();

#if !RELEASE
            // Create a wrapper around the game instance and make it available from current AppDomain
            var wrapper = new GameWrapper(game);
            AppDomain.CurrentDomain.SetData( "GameWrapper", wrapper );
#endif

            using( game )
                game.Run();
        }
    }
}
