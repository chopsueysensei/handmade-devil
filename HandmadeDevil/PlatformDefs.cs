using System;
using Microsoft.Xna.Framework;
using Onirika.Utils;


namespace HandmadeDevil
{
	public struct PlatformConfig : GameModule.IPlatformConfig
	{
		public Vector2		DebugPanelPos			{ get; }
		public int			SampleRate				{ get; }
		public int			LatencySamples			{ get; }
		public int			BytesPerSample			{ get; }
		public int			AudioBufferLenBytes		{ get; }


		public PlatformConfig( string configFilePath )
		{
			// TODO read this from a file
			DebugPanelPos			= new Vector2( 10f, 10f );
			SampleRate				= 48000;
			LatencySamples			= 4096;
			BytesPerSample			= 2 * 2;		// 16 bit stereo
			AudioBufferLenBytes		= LatencySamples * BytesPerSample;
		}
	}
}

