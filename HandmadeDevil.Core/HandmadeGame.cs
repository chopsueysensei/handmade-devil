using HandmadeDevil.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace HandmadeDevil.DesktopGL
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class HandmadeGame : GameModule
    {
        static readonly bool FixedTimestep = false;


        ///
        /// GAME CONFIG
        ///
        GameConfig _cfg;


        ///
        /// RESOURCES
        ///
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        Texture2D _backBuffer;
        SpriteFont _monoFont;


        ///
        /// OTHER STATE
        ///
        uint _framesAccum;
        double _lastFPSUpdateSeconds;
        string _lastFPS;
        // ???
        Viewport _viewport;
        UInt32[] _drawBuffer;



        public HandmadeGame() : base(null)
        {
            _graphics = new GraphicsDeviceManager( this );
            // Relative to platform assembly!
            Content.RootDirectory = "Content";

            if( !FixedTimestep )
            {
                IsFixedTimeStep = false;
                _graphics.SynchronizeWithVerticalRetrace = false;
            }

            _framesAccum = 0;
            _lastFPSUpdateSeconds = 0.0;
            _lastFPS = "0";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // Init game config
            _cfg = new GameConfig( null );

            // Init graphics
            _viewport = _graphics.GraphicsDevice.Viewport;
            _spriteBatch = new SpriteBatch( GraphicsDevice );

            // TODO Resizing!
            _drawBuffer = new UInt32[_viewport.Width * _viewport.Height];
            _backBuffer = new Texture2D( _graphics.GraphicsDevice, _viewport.Width, _viewport.Height );
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _monoFont = Content.Load<SpriteFont>( "Inconsolata" );
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update( GameTime gameTime )
        {
            base.Update( gameTime );

            if( GamePad.GetState( PlayerIndex.One ).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown( Keys.Escape ) )
                Exit();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw( GameTime gameTime )
        {
            base.Draw( gameTime );
            GraphicsDevice.Clear( Color.CornflowerBlue );

            _framesAccum++;
            var elapsed = gameTime.TotalGameTime.TotalSeconds - _lastFPSUpdateSeconds;

            if( elapsed >= 1.0 )
            {
                _lastFPS = _framesAccum.ToString();
                _lastFPSUpdateSeconds = gameTime.TotalGameTime.TotalSeconds - (elapsed-1.0);
                _framesAccum = 0;
            }

//            _gameInstance.RenderVideo( _drawBuffer, _viewport.Width, _viewport.Height );

            // Suuuuuper slow
            // TODO Try drawing pixel-sized colored textures directly?
            _backBuffer.SetData<UInt32>( _drawBuffer );

            _spriteBatch.Begin();
            _spriteBatch.Draw( _backBuffer, position: Vector2.Zero );
            _spriteBatch.DrawString( _monoFont, _lastFPS, _cfg.DebugPanelPos, Color.White );
            _spriteBatch.End();
        }
    }
}
