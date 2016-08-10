using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.Serialization;



namespace HandmadeDevil.Core
{
    public class GameInput
    {
        public KeyboardState keyboardState { get; set; }
        public MouseState mouseState { get; set; }
        // TODO Generalize for several players
        public GamePadCapabilities gamePadCaps { get; set; }
        public GamePadState gamePadState { get; set; }
    }


    public interface IGameState
    { }


    /// <summary>
    /// This class must be serializable so it can be copied across AppDomains.
    /// Mark all attributes to preserve after reload with [DataMember].
    /// NOTE: Any non-primitive members must be themselves [DataContract] instances.
    /// NOTE: Any references should point to the game state itself or to persistent entities.
    /// TODO Look into this persistent pointers thing!
    /// </summary>
    [DataContract]
    public class GameState : IGameState
    {
        [DataMember]
        public int xOffset;
        [DataMember]
        public int yOffset;
        [DataMember]
        public double time;
    }


    public interface IGameWrapper
    {
        IGameState RetrieveGameStateAndExit();
    }


    public abstract class GameModule : Game
    {
        // Initialized by the game, can be overwriten by the platform on game reload
        public IGameState gameState;

        public GameModule( IGameState initialState )
        {
            gameState = initialState ?? new GameState();
        }
    }
}