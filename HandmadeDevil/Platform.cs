using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenTK.Audio;
using System.IO;
using System.Reflection;
using Onirika.Utils;

namespace HandmadeDevil
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Platform : Game
    {
		static readonly bool FixedTimestep = false;


		// TODO Do this the other way around! (copy Core DLL to Platform output dir on each build)
		static readonly string SolutionBaseDir = "dev/workshop/HandmadeDevil";
		static readonly string CoreDLLRelDir = "HandmadeDevil.Core/bin/Debug";



		///
		/// PLATFORM CONFIG
		///
		PlatformConfig _cfg;


		///
		/// PLATFORM STATE
		///
        string _gameAsmName;
		string _gameDLLSrcPath;
        //string _gameDLLDestPath;
		DateTime _gameDLLWriteTime;
		GameModule _gameInstance;

		uint _framesAccum;
		double _lastFPSUpdateSeconds;
		string _lastFPS;
		// ???
		Viewport _viewport;
		UInt32[] _drawBuffer;
		byte[] _audioBuffer;


        ///
        /// RESOURCES
        ///
		GraphicsDeviceManager _graphics;
		SpriteBatch _spriteBatch;
        Texture2D _backBuffer;
        SpriteFont _monoFont;
        AudioContext _audioContext;
		DynamicSoundEffectInstance _audioInstance;



        public Platform()
        {
			_graphics = new GraphicsDeviceManager(this);
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

			// Init platform config
			_cfg = new PlatformConfig( null );

			// Init graphics
			_viewport = _graphics.GraphicsDevice.Viewport;
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO Resizing!
			_drawBuffer = new UInt32[ _viewport.Width * _viewport.Height ];
			_backBuffer = new Texture2D( _graphics.GraphicsDevice, _viewport.Width, _viewport.Height );

			// Init audio
			_audioContext = new AudioContext();
			_audioBuffer = new byte[_cfg.AudioBufferLenBytes];
			_audioInstance = new DynamicSoundEffectInstance( _cfg.SampleRate,  Microsoft.Xna.Framework.Audio.AudioChannels.Stereo );
			_audioInstance = new DynamicSoundEffectInstance( 48000,  Microsoft.Xna.Framework.Audio.AudioChannels.Stereo );
			_audioInstance.Play();

            //var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            //var assemblyPath = Path.Combine( Path.Combine( homePath, SolutionBaseDir ), CoreDLLRelDir );
            var assemblyPath = AppDomain.CurrentDomain.BaseDirectory;

			Console.WriteLine( "Loading game assembly from " + assemblyPath );
            _gameAsmName = "HandmadeDevil.Core";
            _gameDLLSrcPath = Path.Combine( assemblyPath, _gameAsmName + ".dll" );

#if DEBUG
			// Provide an assembly loader for this platform
			ModuleManager.AssemblyLoader = new PlatformAssemblyLoader( assemblyPath );
            //_gameDLLDestPath = "HandmadeDevil.Core.Temp.dll";
            //File.Copy( _gameDLLSrcPath, _gameDLLDestPath, true );

			_gameInstance = ModuleManager.LoadModule<GameModule>( _gameAsmName );

			if( _gameInstance == null )
				throw new Exception( "Could not load game assembly at '" + _gameDLLSrcPath + "'!" );

			_gameDLLWriteTime = new FileInfo( _gameDLLSrcPath ).LastWriteTime;
#else
			// Link 'statically' (no reloads allowed)
            _gameInstance = ModuleManager.LinkModule<GameModule>( _gameDLLName );
#endif

			// Pass platform configuration to the game
			_gameInstance.platformConfig = _cfg;
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
		/// (Unload any non ContentManager content here).
        /// </summary>
        protected override void UnloadContent()
        {
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
			base.Update(gameTime);

#if DEBUG
			// Fetch game DLL and see if it's been updated
			var newWriteTime = new FileInfo( _gameDLLSrcPath ).LastWriteTime;
			if( newWriteTime != _gameDLLWriteTime )
			{
                ModuleManager.UnloadModule<GameModule>();
                //File.Copy( _gameDLLSrcPath, _gameDLLDestPath, true );
			    _gameInstance = ModuleManager.LoadModule<GameModule>( _gameAsmName );

			    if( _gameInstance == null )
				    throw new Exception( "Could not load game assembly at '" + _gameDLLSrcPath + "'!" );

				_gameDLLWriteTime = newWriteTime;
			}
#endif

			var input = _gameInstance.gameInput;
			input.keyboardState	= Keyboard.GetState();
			input.mouseState	= Mouse.GetState();
			input.gamePadCaps	= GamePad.GetCapabilities( PlayerIndex.One );
			input.gamePadState	= GamePad.GetState( PlayerIndex.One );

			if( input.gamePadState.Buttons.Back == ButtonState.Pressed
				|| input.keyboardState.IsKeyDown(Keys.Escape) )
                Exit();

			_gameInstance.Update();

			while( _audioInstance.PendingBufferCount < DynamicSoundEffectInstance.BUFFERCOUNT )
			{
				_gameInstance.RenderAudio( _audioBuffer );
				_audioInstance.SubmitBuffer( _audioBuffer );
			}
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
			base.Draw( gameTime );

            _framesAccum++;
            var elapsed = gameTime.TotalGameTime.TotalSeconds - _lastFPSUpdateSeconds;

            if( elapsed >= 1.0 )
            {
                _lastFPS = _framesAccum.ToString();
                _lastFPSUpdateSeconds = gameTime.TotalGameTime.TotalSeconds - (elapsed-1.0);
                _framesAccum = 0;
            }

			_gameInstance.RenderVideo( _drawBuffer, _viewport.Width, _viewport.Height );

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
