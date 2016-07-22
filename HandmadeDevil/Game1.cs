using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenTK.Audio;

namespace HandmadeDevil
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
		///
		/// PLATFORM CONFIG
		///
		// TODO move this to a file)
        static readonly bool            FixedTimestep = false;
        static readonly Vector2         DebugPanelPos = new Vector2( 10f, 10f );
		static readonly int				SampleRate = 48000;
		static readonly int				LatencySamples = 1024;
		static readonly int				BytesPerSample = 2 * 2;		// 16 bit stereo
		static readonly int				AudioBufferLenBytes = LatencySamples * BytesPerSample;


		///
		/// GAME STATE
		///
		uint _framesAccum;
		double _lastFPSUpdateSeconds;
		int _xOffset, _yOffset;
		// ???
		UInt32[] _drawBuffer;
		byte[] _audioBuffer;



        ///
        /// RESOURCES
        ///
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D _backBuffer;
        SpriteFont _monoFont;
        AudioContext _audioContext;
		DynamicSoundEffectInstance _audioInstance;
        

        ///
        /// AUX
        ///
        string _lastFPS;
        Viewport _viewport;
        // TODO Publish this in a static App struct?
        KeyboardState _keyboardState;
        MouseState _mouseState;
        // TODO Generalize for several players
        GamePadCapabilities _gamePadCaps;
        GamePadState _gamePadState;
		double _time;



        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
			_time = 0.0;

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

            _audioContext = new AudioContext();
			_audioBuffer = new byte[AudioBufferLenBytes];
			_audioInstance = new DynamicSoundEffectInstance( SampleRate,  Microsoft.Xna.Framework.Audio.AudioChannels.Stereo );
            _audioInstance.BufferNeeded += OnAudioBufferNeeded;

			_audioInstance.Play();

            base.Initialize();
        }

        void OnAudioBufferNeeded( object sender, EventArgs e )
        {
            RenderAudioBuffer();
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

            _audioInstance.Dispose();
            _audioContext.Dispose();
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

			// FIXME Review that custom class... ¬¬
            //while( _audioInstance.PendingBufferCount < 2 )
            //    RenderAudioBuffer();

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

		private void RenderAudioBuffer()
		{
			const int Freq = 440;
            const int Period = 10000;
            const short Amp = 15000;

            /*
			for( int i = 0; i < AudioBufferLenBytes; i += BytesPerSample )
			{
				double sample = Math.Sin( 2 * Math.PI * Freq * _time );
				// Left channel
				Int16 lSample = Sample( sample );
				ToByteArray( lSample, _audioBuffer, i );

				// Right channel
				Int16 rSample = Sample( sample );
				ToByteArray( rSample, _audioBuffer, i+2 );

				_time += 1.0 / SampleRate;
			}
             * */

            bool up = true;
            int p = 0;

            for( int i = 0; i < AudioBufferLenBytes; i += BytesPerSample )
            {
                ToByteArray( (short)(up ? Amp : -Amp), _audioBuffer, i );
                ToByteArray( (short)(up ? Amp : -Amp), _audioBuffer, i+2 );

                p++;
                if( p == Period )
                {
                    up = !up;
                    p = 0;
                }
            }

			_audioInstance.SubmitBuffer( _audioBuffer );
		}

		private Int16 Sample( double sample )
		{
			Int16 result = 0;

			sample = (sample > 1.0) ? 1.0 : (sample < -1.0 ? -1.0 : sample);
			if( sample > .0 )
				result = (Int16)( sample * Int16.MaxValue );
			else
				result = (Int16)( -sample * Int16.MinValue );

			return result;
		}

		private void ToByteArray( Int16 sample, byte[] buffer, int index )
		{
			if( BitConverter.IsLittleEndian )
			{
				buffer[index] = (byte)sample;
				buffer[index+1] = (byte)(sample >> 8);
			}
			else
			{
				buffer[index] = (byte)(sample >> 8);
				buffer[index+1] = (byte)sample;
			}
		}
    }
}
