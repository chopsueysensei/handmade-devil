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
            var state = game.gameState;
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

            // Create a wrapper around the game instance and make it available from current AppDomain
            // TODO Disable for final build
            var wrapper = new GameWrapper(game);
            AppDomain.CurrentDomain.SetData( "GameWrapper", wrapper );

            using( game )
                game.Run();
        }
    }
}
