using HandmadeDevil.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenTK.Audio;
using System;
using System.IO;

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
        AudioContext _audioContext;
        DynamicSoundEffectInstance _audioInstance;


        ///
        /// OTHER STATE
        ///
        uint _framesAccum;
        double _lastFPSUpdateSeconds;
        string _lastFPS;
        // ???
        Viewport _viewport;
        UInt32[] _drawBuffer;
        byte[] _audioBuffer;



        public HandmadeGame( byte[] gameStateData = null ) : base( gameStateData )
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

            // Init audio
            _audioContext = new AudioContext();
            _audioBuffer = new byte[_cfg.AudioBufferLenBytes];
            _audioInstance = new DynamicSoundEffectInstance( _cfg.SampleRate, Microsoft.Xna.Framework.Audio.AudioChannels.Stereo );
            _audioInstance = new DynamicSoundEffectInstance( 48000, Microsoft.Xna.Framework.Audio.AudioChannels.Stereo );
            _audioInstance.Play();
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
            this.Content.Unload();

            _audioInstance.Dispose();
            _audioContext.Dispose();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update( GameTime gameTime )
        {
            if( isPaused )
                return;

            base.Update( gameTime );

            if( Keyboard.GetState().IsKeyDown( Keys.Escape ) )
                Exit();
            if( Keyboard.GetState().IsKeyDown( Keys.RightControl ) )
            {
                 if( Keyboard.GetState().IsKeyDown( Keys.D1) )
                     SaveGameStateToSlot( 1 );
            }
            if( Keyboard.GetState().IsKeyDown( Keys.RightShift ) )
            {
                if( Keyboard.GetState().IsKeyDown( Keys.D1 ) )
                    ReadGameStateFromSlot( 1 );
            }

            HandmadeCore.Update( gameState, gameTime );

            while( _audioInstance.PendingBufferCount < DynamicSoundEffectInstance.BufferCount )
            {
                HandmadeCore.RenderAudio( gameState, _cfg, _audioBuffer );
                _audioInstance.SubmitBuffer( _audioBuffer );
            }
        }

        private void SaveGameStateToSlot( int slotIndex )
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            var gameStateStr = SerializeGameStateXML();
            File.WriteAllText( Path.Combine( dir, "game_state_" + slotIndex + ".xml" ), gameStateStr );
        }

        private void ReadGameStateFromSlot( int slotIndex )
        {
            string gameStateStr;
            var dir = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                gameStateStr = File.ReadAllText( Path.Combine( dir, "game_state_" + slotIndex + ".xml" ) );
                gameState = DeserializeGameState( gameStateStr );
            }
            catch( FileNotFoundException )
            {}
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

            HandmadeCore.RenderVideo( gameState, _drawBuffer, _viewport.Width, _viewport.Height );

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
