﻿using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

//#if MONOMAC
//using MonoMac.OpenAL;
//#else
using OpenTK.Audio.OpenAL;
//#endif


/*
 * Custom DynamicSoundEffectInstance wrapping OpenAL written by pjrader1
 * Taken from http://stackoverflow.com/questions/15959903/xna-monogame-dynamicsoundeffectinstance-buffer-already-full-exception
 */
public sealed class DynamicSoundEffectInstance : IDisposable
{
	public const int BufferCount = 2;

	private SoundState soundState = SoundState.Stopped;
	private AudioChannels channels;
	private int sampleRate;
	private ALFormat format;
	private bool looped = false;
	private float volume = 1.0f;
	private float pan = 0;
	private float pitch = 0f;
	private int sourceId;
	private int[] bufferIds;
	private int[] bufferIdsToFill;
	private int currentBufferToFill;
	private bool isDisposed = false;
	private bool hasSourceId = false;
	private Thread bufferFillerThread = null;

	// Events
	public event EventHandler<EventArgs> BufferNeeded;

	internal void OnBufferNeeded(EventArgs args)
	{
		if (BufferNeeded != null)
		{
			BufferNeeded(this, args);
		}
	}

	public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels)
	{
		this.sampleRate = sampleRate;
		this.channels = channels;
		switch (channels)
		{
		case AudioChannels.Mono:
			this.format = ALFormat.Mono16;
			break;
		case AudioChannels.Stereo:
			this.format = ALFormat.Stereo16;
			break;
		default:
			break;
		}                       
	}

	public bool IsDisposed
	{
		get
		{
			return isDisposed;
		}
	}

	public float Pan
	{
		get
		{
			return pan;
		}

		set
		{
			pan = value;
			if (hasSourceId)
			{
				// Listener
				// Pan
				AL.Source(sourceId, ALSource3f.Position, pan, 0.0f, 0.1f);
			}
		}
	}

	public float Pitch
	{
		get
		{
			return pitch;
		}
		set
		{
			pitch = value;
			if (hasSourceId)
			{
				// Pitch
				AL.Source(sourceId, ALSourcef.Pitch, XnaPitchToAlPitch(pitch));
			}
		}
	}

	public float Volume
	{
		get
		{
			return volume;
		}

		set
		{
			volume = value;
			if (hasSourceId)
			{
				// Volume
				AL.Source(sourceId, ALSourcef.Gain, volume * SoundEffect.MasterVolume);
			}

		}
	}   

	public SoundState State
	{
		get
		{
			return soundState;
		}
	}

	private float XnaPitchToAlPitch(float pitch)
	{
		// pitch is different in XNA and OpenAL. XNA has a pitch between -1 and 1 for one octave down/up.
		// openAL uses 0.5 to 2 for one octave down/up, while 1 is the default. The default value of 0 would make it completely silent.
		return (float)Math.Exp(0.69314718 * pitch);
	}

	public void Play()
	{
		if (!hasSourceId)
		{
			bufferIds = AL.GenBuffers(BufferCount);
			sourceId = AL.GenSource();

            ALError error = AL.GetError();
            if( error != ALError.NoError )
                throw new Exception( AL.GetErrorString( error ) );

			hasSourceId = true;
		}
		soundState = SoundState.Playing;

		if (bufferFillerThread == null)
		{
			bufferIdsToFill = bufferIds;
			currentBufferToFill = 0;
			OnBufferNeeded(EventArgs.Empty);
			bufferFillerThread = new Thread(new ThreadStart(BufferFiller));
			bufferFillerThread.Start();
		}

		AL.SourcePlay(sourceId);
	}

	public void Apply3D(AudioListener listener, AudioEmitter emitter)
	{
		Apply3D(new AudioListener[] { listener }, emitter);
	}

	public void Pause()
	{
		if (hasSourceId)
		{
			AL.SourcePause(sourceId);
			soundState = SoundState.Paused;
		}
	}


	public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
	{
		// get AL's listener position
		float x, y, z;
		AL.GetListener(ALListener3f.Position, out x, out y, out z);

		for (int i = 0; i < listeners.Length; i++)
		{
			AudioListener listener = listeners[i];

			// get the emitter offset from origin
			Vector3 posOffset = emitter.Position - listener.Position;
			// set up orientation matrix
			Matrix orientation = Matrix.CreateWorld(Vector3.Zero, listener.Forward, listener.Up);
			// set up our final position and velocity according to orientation of listener
			Vector3 finalPos = new Vector3(x + posOffset.X, y + posOffset.Y, z + posOffset.Z);
			finalPos = Vector3.Transform(finalPos, orientation);
			Vector3 finalVel = emitter.Velocity;
			finalVel = Vector3.Transform(finalVel, orientation);

			// set the position based on relative positon
			AL.Source(sourceId, ALSource3f.Position, finalPos.X, finalPos.Y, finalPos.Z);
			AL.Source(sourceId, ALSource3f.Velocity, finalVel.X, finalVel.Y, finalVel.Z);
		}
	}


	public void Dispose()
	{
		if (!isDisposed)
		{
			Stop(true);
			AL.DeleteBuffers(bufferIds);
			AL.DeleteSource(sourceId);
			bufferIdsToFill = null;
			hasSourceId = false;
			isDisposed = true;
		}
	}

	public void Stop()
	{
		if (hasSourceId)
		{
			AL.SourceStop(sourceId);
			int pendingBuffers = PendingBufferCount;
			if(pendingBuffers > 0)
				AL.SourceUnqueueBuffers(sourceId, PendingBufferCount);
			if (bufferFillerThread != null)
				bufferFillerThread.Abort();
			bufferFillerThread = null;
		}
		soundState = SoundState.Stopped;
	}

	public void Stop(bool immediate)
	{
		Stop();
	}

	public TimeSpan GetSampleDuration(int sizeInBytes)
	{
		throw new NotImplementedException();
	}

	public int GetSampleSizeInBytes(TimeSpan duration)
	{
		int size = (int)(duration.TotalMilliseconds * ((float)sampleRate / 1000.0f));
		return (size + (size & 1)) * 16;
	}

	public void SubmitBuffer(byte[] buffer)
	{
		this.SubmitBuffer(buffer, 0, buffer.Length);
	}

	public void SubmitBuffer(byte[] buffer, int offset, int count)
	{
		if (bufferIdsToFill != null) {
			AL.BufferData (bufferIdsToFill [currentBufferToFill], format, buffer, count, sampleRate);
			AL.SourceQueueBuffer (sourceId, bufferIdsToFill [currentBufferToFill]);
			currentBufferToFill++;
			if (currentBufferToFill >= bufferIdsToFill.Length)
				bufferIdsToFill = null;
			else
				OnBufferNeeded (EventArgs.Empty);
		} else {
			throw new  Exception ("Buffer already full.");
		}
	}

	private void BufferFiller()
	{
		bool done = false;

		while (!done)
		{
			var state = AL.GetSourceState(sourceId);
			if (state == ALSourceState.Stopped || state == ALSourceState.Initial)
				AL.SourcePlay(sourceId);

			if (bufferIdsToFill != null)
				continue;

			int buffersProcessed;
			AL.GetSource(sourceId, ALGetSourcei.BuffersProcessed, out buffersProcessed);
            ALError error = AL.GetError();
            if( error != ALError.NoError )
                throw new Exception( AL.GetErrorString( error ) );

			if (buffersProcessed == 0)
				continue;

			bufferIdsToFill = AL.SourceUnqueueBuffers(sourceId, buffersProcessed);
			currentBufferToFill = 0;
			OnBufferNeeded(EventArgs.Empty);
		}
	}

	public bool IsLooped
	{
		get
		{
			return looped;
		}

		set
		{
			looped = value;                
		}
	}

	public int PendingBufferCount 
	{
		get
		{
			if (hasSourceId)
			{
				int buffersQueued;
				AL.GetSource(sourceId, ALGetSourcei.BuffersQueued, out buffersQueued);
				return buffersQueued;
			}
			return 0;
		}
	}

	public bool NeedsBuffer
	{
		get
		{
			return bufferIdsToFill != null && currentBufferToFill < bufferIdsToFill.Length;
		}
	}
}   
