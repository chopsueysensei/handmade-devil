using Microsoft.Xna.Framework;
using System;


namespace HandmadeDevil.Core
{
    public struct GameConfig
    {
        public Vector2 DebugPanelPos        { get; private set; }
        public Vector2 DebugConsolePos      { get; private set; }
        public int SampleRate               { get; private set; }
        public int LatencySamples           { get; private set; }
        public int BytesPerSample           { get; private set; }
        public int AudioBufferLenBytes      { get; private set; }


        public GameConfig( string configFilePath )
            : this()
        {
            // TODO read this from a file
            DebugPanelPos			= new Vector2( 10f, 10f );
            DebugConsolePos         = new Vector2( 10f, -25f );
            SampleRate				= 48000;
            LatencySamples			= 4096;
            BytesPerSample			= 2 * 2;		// 16 bit stereo
            AudioBufferLenBytes		= LatencySamples * BytesPerSample;
        }
    }
}
