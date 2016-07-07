using System;
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
        Texture2D _frontBuffer;
        SpriteFont _monoFont;


        ///
        /// GAME STATE
        ///
        uint _framesAccum;
        double _lastFPSUpdateSeconds;
        UInt32[] _backBuffer;
        

        ///
        /// AUX
        ///
        string _lastFPS;
        Viewport _viewport;




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
            _viewport = graphics.GraphicsDevice.Viewport;

            // TODO Resizing?
            _backBuffer = new UInt32[ _viewport.Width*_viewport.Height ];
            _frontBuffer = new Texture2D( graphics.GraphicsDevice, _viewport.Width, _viewport.Height );

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            for( int i = 0; i < _viewport.Width*_viewport.Height; ++i )
                _backBuffer[i] = 0xFF0000FF;

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

            // Suuuuuper slow
            _frontBuffer.SetData<UInt32>( _backBuffer );

            spriteBatch.Begin();
            spriteBatch.Draw( _frontBuffer, position: Vector2.Zero );
            spriteBatch.DrawString( _monoFont, _lastFPS, DebugPanelPos, Color.White );
            spriteBatch.End();

            base.Draw( gameTime );
        }
    }
}
