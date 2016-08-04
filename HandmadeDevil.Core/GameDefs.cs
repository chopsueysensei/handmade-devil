using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.Serialization;



namespace HandmadeDevil.Core
{
    public abstract class GameModule : Game
    {
        public interface IPlatformConfig
        {
            Vector2 DebugPanelPos { get; }
            int SampleRate { get; }
            int LatencySamples { get; }
            int BytesPerSample { get; }
            int AudioBufferLenBytes { get; }
        }

        public interface IGameInput
        {
            KeyboardState keyboardState { get; set; }
            MouseState mouseState { get; set; }
            // TODO Generalize for several players
            GamePadCapabilities gamePadCaps { get; set; }
            GamePadState gamePadState { get; set; }
        }

        public interface IGameState
        { }


        // Provided by the platform
        public IPlatformConfig platformConfig;
        // Initialized by the game, updated by the platform on every frame
        public IGameInput gameInput;
        // Initialized by the game, can be overwriten by the platform on game reload
        public IGameState gameState;



        public GameModule() { }

//        public abstract void Update( GameTime gameTime );
//        public abstract void RenderVideo( UInt32[] videoBuffer, int width, int height );
//        public abstract void RenderAudio( byte[] audioBuffer );

        public delegate void UpdateDelegate( GameTime gameTime );
        public event UpdateDelegate OnUpdate;

        protected override void Update( GameTime gameTime )
        {
            base.Update( gameTime );

            if( OnUpdate != null )
                OnUpdate( gameTime );
        }

        public virtual void Terminate()
        {
            Exit();
        }
    }


	public class GameInput : GameModule.IGameInput
	{
		public KeyboardState		keyboardState	{ get; set; }
		public MouseState			mouseState		{ get; set; }
		// TODO Generalize for several players
		public GamePadCapabilities	gamePadCaps		{ get; set; }
		public GamePadState			gamePadState	{ get; set; }
	}

    [DataContract]
	public class GameState : GameModule.IGameState
	{
        [DataMember]
		public int		xOffset;
        [DataMember]
		public int		yOffset;
        [DataMember]
		public double	time;
	}
    

    public interface IGameWrapper
    {
        GameModule.IGameState RetrieveGameStateAndExit();
    }
}