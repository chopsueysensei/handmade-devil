using System;
using Onirika.Utils;



namespace HandmadeDevil.Core
{
	public class HandmadeGame : GameModule
	{
		private GameState _gs { get { return gameState as GameState; } }


		public HandmadeGame()
		{
			gameInput = new GameInput();
			gameState = new GameState()
			{
				xOffset = 0,
				yOffset = 0,
				time = 0.0,
			};
		}

		public override void Update()
		{
			_gs.xOffset++;
            _gs.yOffset++;
		}

		public override void RenderVideo( UInt32[] videoBuffer, int width, int height )
		{
			int i = 0;
			for( int y = 0; y < height; ++y )
				for( int x = 0; x < width; ++x )
				{
					videoBuffer[i++] = (UInt32)(
						(0xFF<<24)
						| (((byte) (x + _gs.xOffset))<<16)
						| (((byte) (y + _gs.yOffset))<<8)
					);			
				}
		}

		public override void RenderAudio( byte[] audioBuffer )
		{
			const float Freq = 440f;
			const float Amp = 15000;

			for( int i = 0; i < platformConfig.AudioBufferLenBytes; i += platformConfig.BytesPerSample )
			{
				double sample = Amp * Math.Sin( 2 * Math.PI * Freq * _gs.time );
				// Left channel
				Int16 lSample = Sample( sample );
				ToByteArray( lSample, audioBuffer, i );

				// Right channel
				Int16 rSample = Sample( sample );
				ToByteArray( rSample, audioBuffer, i+2 );

				_gs.time += 1.0 / platformConfig.SampleRate;
			}
		}

		#region Aux functions

		private static Int16 Sample( double sample )
		{
			Int16 result = 0;

			sample = (sample > 1.0) ? 1.0 : (sample < -1.0 ? -1.0 : sample);
			if( sample > .0 )
				result = (Int16)( sample * Int16.MaxValue );
			else
				result = (Int16)( -sample * Int16.MinValue );

			return result;
		}

		private static void ToByteArray( Int16 sample, byte[] buffer, int index )
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

		#endregion
	}
}

