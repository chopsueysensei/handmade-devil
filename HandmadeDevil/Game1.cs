﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HandmadeDevil
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        static readonly bool            FixedTimestep = false;
        static readonly Vector2         DebugPanelPos = new Vector2( 10f, 10f );



        ///
        /// RESOURCES
        ///
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D _backBuffer;
        SpriteFont _monoFont;


        ///
        /// GAME STATE
        ///
        uint _framesAccum;
        double _lastFPSUpdateSeconds;
		int _xOffset, _yOffset;
		// ???
        UInt32[] _drawBuffer;
        

        ///
        /// AUX
        ///
        string _lastFPS;
        Viewport _viewport;
        // TODO Publish this in a static App struct
        KeyboardState _keyboardState;
        MouseState _mouseState;
        // TODO Generalize for several players
        GamePadCapabilities _gamePadCaps;
        GamePadState _gamePadState;



        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            if( !FixedTimestep )
            {
                IsFixedTimeStep = false;
                graphics.SynchronizeWithVerticalRetrace = false;
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _framesAccum = 0;
            _lastFPSUpdateSeconds = 0.0;
            _lastFPS = "0";
			_xOffset = 0;
			_yOffset = 0;

            _viewport = graphics.GraphicsDevice.Viewport;

            // TODO Resizing?
            _drawBuffer = new UInt32[ _viewport.Width*_viewport.Height ];
			_backBuffer = new Texture2D( graphics.GraphicsDevice, _viewport.Width, _viewport.Height );

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            _monoFont = Content.Load<SpriteFont>( "Inconsolata" );
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            this.Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _keyboardState = Keyboard.GetState();
            _mouseState = Mouse.GetState();
            _gamePadCaps = GamePad.GetCapabilities( PlayerIndex.One );
            _gamePadState = GamePad.GetState( PlayerIndex.One );

            if (_gamePadState.Buttons.Back == ButtonState.Pressed || _keyboardState.IsKeyDown(Keys.Escape))
                Exit();

			_xOffset++;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _framesAccum++;
            var elapsed = gameTime.TotalGameTime.TotalSeconds - _lastFPSUpdateSeconds;

            if( elapsed >= 1.0 )
            {
                _lastFPS = _framesAccum.ToString();
                _lastFPSUpdateSeconds = gameTime.TotalGameTime.TotalSeconds - (elapsed-1.0);
                _framesAccum = 0;
            }

			int i = 0;
			for( int y = 0; y < _viewport.Height; ++y )
				for( int x = 0; x < _viewport.Width; ++x )
					_drawBuffer[i++] = (UInt32)(
						(0xFF<<24)
						| (((byte) (x+_xOffset))<<16) | (((byte) (y+_yOffset))<<8) );

            // Suuuuuper slow
			// TODO Try drawing pixel-sized colored textures directly?
			_backBuffer.SetData<UInt32>( _drawBuffer );

            spriteBatch.Begin();
			spriteBatch.Draw( _backBuffer, position: Vector2.Zero );
            spriteBatch.DrawString( _monoFont, _lastFPS, DebugPanelPos, Color.White );
            spriteBatch.End();

            base.Draw( gameTime );
        }
    }
}
