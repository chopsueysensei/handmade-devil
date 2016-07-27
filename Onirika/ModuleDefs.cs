using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;



namespace Onirika.Utils
{
	public abstract class GameModule
	{
		public interface IPlatformConfig
		{
			Vector2		DebugPanelPos			{ get; }
			int			SampleRate				{ get; }
			int			LatencySamples			{ get; }
			int			BytesPerSample			{ get; }
			int			AudioBufferLenBytes		{ get; }
		}

		public interface IGameInput
		{
			KeyboardState		keyboardState	{ get; set; }
			MouseState			mouseState		{ get; set; }
			// TODO Generalize for several players
			GamePadCapabilities	gamePadCaps		{ get; set; }
			GamePadState		gamePadState	{ get; set; }
		}

		public interface IGameState
		{}


		// Provided by the platform
		public IPlatformConfig platformConfig;
		// Initialized by the game, updated by the platform on every frame
		public IGameInput gameInput;
		// Initialized by the game, can be overwriten by the platform on game reload
		public IGameState gameState;



		public GameModule() {}

		public abstract void Update();
		public abstract void RenderVideo( UInt32[] videoBuffer, int width, int height );
		public abstract void RenderAudio( byte[] audioBuffer );
	}
}

