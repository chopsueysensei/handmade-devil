using Onirika.Utils;
using Microsoft.Xna.Framework.Input;



namespace HandmadeDevil.Core
{
	public class GameInput : GameModule.IGameInput
	{
		public KeyboardState		keyboardState	{ get; set; }
		public MouseState			mouseState		{ get; set; }
		// TODO Generalize for several players
		public GamePadCapabilities	gamePadCaps		{ get; set; }
		public GamePadState			gamePadState	{ get; set; }
	}


	public class GameState : GameModule.IGameState
	{
		public int		xOffset;
		public int		yOffset;
		public double	time;
	}
}