using Silk.NET.SDL;
using System;
using Veldrid.Sdl2;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// One SDL2 audio device in push mode (SDL_QueueAudio), opened for the format of the sound being played. SDL converts
/// to whatever the hardware wants. The audio subsystem is started on first use, never at program start: Smithbox only
/// asks SDL for video.
/// </summary>
public sealed unsafe class SoundOutput : IDisposable
{
    private const ushort AudioS16Lsb = 0x8010;

    private uint _device;

    public int SampleRate { get; private set; }
    public int Channels { get; private set; }
    public int BytesPerFrame => Channels * 2;
    public bool IsOpen => _device != 0;

    /// <summary>Opens (or reopens, if the format changed) the device for 16-bit PCM. Returns false with an error.</summary>
    public bool Open(int sampleRate, int channels, out string error)
    {
        error = null;
        if (_device != 0 && sampleRate == SampleRate && channels == Channels)
        {
            Clear();
            return true;
        }
        Close();

        var sdl = SdlProvider.SDL.Value;
        if (sdl.WasInit(Sdl.InitAudio) == 0 && sdl.InitSubSystem(Sdl.InitAudio) != 0)
        {
            error = $"SDL audio init failed: {sdl.GetErrorS()}";
            return false;
        }

        var want = new AudioSpec
        {
            Freq = sampleRate,
            Format = AudioS16Lsb,
            Channels = (byte)channels,
            Samples = 2048,
        };
        AudioSpec have;
        _device = sdl.OpenAudioDevice((byte*)null, 0, &want, &have, 0);
        if (_device == 0)
        {
            error = $"SDL could not open an audio device ({sampleRate} Hz, {channels} ch): {sdl.GetErrorS()}";
            return false;
        }

        SampleRate = sampleRate;
        Channels = channels;
        sdl.PauseAudioDevice(_device, 0);
        return true;
    }

    public void Queue(ReadOnlySpan<byte> pcm)
    {
        if (_device == 0 || pcm.IsEmpty) return;
        fixed (byte* p = pcm)
            SdlProvider.SDL.Value.QueueAudio(_device, p, (uint)pcm.Length);
    }

    /// <summary>Bytes queued and not yet played.</summary>
    public uint Queued => _device == 0 ? 0 : SdlProvider.SDL.Value.GetQueuedAudioSize(_device);

    public void Clear()
    {
        if (_device != 0) SdlProvider.SDL.Value.ClearQueuedAudio(_device);
    }

    public void SetPaused(bool paused)
    {
        if (_device != 0) SdlProvider.SDL.Value.PauseAudioDevice(_device, paused ? 1 : 0);
    }

    public void Close()
    {
        if (_device == 0) return;
        var sdl = SdlProvider.SDL.Value;
        sdl.ClearQueuedAudio(_device);
        sdl.CloseAudioDevice(_device);
        _device = 0;
    }

    public void Dispose() => Close();
}
