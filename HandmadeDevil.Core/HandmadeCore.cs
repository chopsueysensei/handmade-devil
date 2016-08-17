using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;



namespace HandmadeDevil.Core
{
	public static class HandmadeCore
	{
		public static void Update( GameState gs, GameTime gt )
		{
			gs.xOffset++;
            gs.yOffset++;
		}

		public static void RenderVideo( GameState gs, UInt32[] videoBuffer, int width, int height )
		{
			int i = 0;
			for( int y = 0; y < height; ++y )
				for( int x = 0; x < width; ++x )
				{
					videoBuffer[i++] = (UInt32)(
						(0xFF<<24)
						| (((byte) (x + gs.xOffset))<<16)
						| (((byte) (y + gs.yOffset))<<8)
					);			
				}
		}

		public static void RenderAudio( GameState gs, GameConfig gc, byte[] audioBuffer )
		{
			const float Freq = 220f;
			const float Amp = 0.5f;               // Range 0.0/1.0

			for( int i = 0; i < gc.AudioBufferLenBytes; i += gc.BytesPerSample )
			{
				double sample = Amp * Math.Sin( 2 * Math.PI * Freq * gs.time );
				// Left channel
				Int16 lSample = Sample( sample );
				ToByteArray( lSample, audioBuffer, i );

				// Right channel
				Int16 rSample = Sample( sample );
				ToByteArray( rSample, audioBuffer, i+2 );

				gs.time += 1.0 / gc.SampleRate;
			}
		}

		#region Aux functions

        /// <summary>
        /// Samples a continuous sound function to a discrete 16-bit PCM sample
        /// </summary>
        /// <param name="sample">Audio curve in the range -1.0/+1.0</param>
        /// <returns></returns>
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
