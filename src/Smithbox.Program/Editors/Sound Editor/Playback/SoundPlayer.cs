using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace StudioCore.Editors.SoundEditor;

public enum SoundPlayerState
{
    Stopped,
    Loading,
    Playing,
    Paused,
    Failed,
}

/// <summary>
/// Plays one sound at a time. Play starts a vgmstream decode on a worker thread and feeds SDL as the PCM arrives, so
/// long streams start at once. The decoded PCM is kept, so Loop, Seek (within what has been decoded) and replaying
/// the same sound cost nothing. Volume is applied when samples are queued.
/// <para>The UI reads <see cref="State"/>, <see cref="Position"/>, <see cref="Duration"/> each frame; everything
/// else runs on the worker. One player per editor; <see cref="Dispose"/> when the editor closes.</para>
/// </summary>
public sealed class SoundPlayer : IDisposable
{
    // ~0.25 s of audio kept queued in SDL; small enough that Stop/Seek/Volume react quickly
    private const double QueueSeconds = 0.25;

    private readonly object _lock = new();
    private Thread _worker;
    private CancellationTokenSource _cancel;
    private readonly SoundOutput _output = new();
    private volatile Process _proc;

    // decoded PCM16 of the current sound, grown by the worker
    private byte[] _pcm = Array.Empty<byte>();
    private int _decoded;
    private bool _decodeDone;
    private int _rate, _channels;

    // playback cursor (bytes into _pcm of the next byte to queue); -1 = no seek request
    private int _feed;
    private volatile int _seekRequest = -1;
    private volatile bool _pauseRequest;

    public SoundSource Current { get; private set; }
    public SoundPlayerState State { get; private set; }
    public string Error { get; private set; }

    public bool Loop { get; set; }

    /// <summary>0..1, applied as samples are queued.</summary>
    public float Volume { get; set; } = 1f;

    /// <summary>Length in seconds, from the decoded length once done, else from vgmstream's info (0 if unknown).</summary>
    public double Duration { get; private set; }

    /// <summary>Seconds into the sound that are being heard now.</summary>
    public double Position
    {
        get
        {
            lock (_lock)
            {
                if (_rate == 0 || _channels == 0) return 0;
                long heard = Math.Max(0, _feed - (long)_output.Queued);
                return (double)heard / (_rate * _channels * 2);
            }
        }
    }

    /// <summary>Seconds decoded so far (seeking past this waits for the decode).</summary>
    public double Decoded
    {
        get { lock (_lock) return _rate == 0 ? 0 : (double)_decoded / (_rate * _channels * 2); }
    }

    public bool IsActive => State is SoundPlayerState.Loading or SoundPlayerState.Playing or SoundPlayerState.Paused;

    /// <summary>Plays <paramref name="source"/> from the start, stopping whatever was playing.</summary>
    public void Play(SoundSource source)
    {
        // same sound fully decoded: replay from memory
        if (Current != null && source.CacheKey == Current.CacheKey && source.Subsong == Current.Subsong && _decodeDone && State != SoundPlayerState.Failed)
        {
            StopWorker();
            StartWorker(source, decode: false);
            return;
        }

        StopWorker();
        lock (_lock)
        {
            _pcm = Array.Empty<byte>();
            _decoded = 0;
            _decodeDone = false;
            _rate = _channels = 0;
            Duration = 0;
        }
        StartWorker(source, decode: true);
    }

    public void Stop()
    {
        StopWorker();
        if (State != SoundPlayerState.Failed) State = SoundPlayerState.Stopped;
    }

    public void TogglePause()
    {
        if (State == SoundPlayerState.Playing) { _pauseRequest = true; State = SoundPlayerState.Paused; _output.SetPaused(true); }
        else if (State == SoundPlayerState.Paused) { _pauseRequest = false; State = SoundPlayerState.Playing; _output.SetPaused(false); }
        else if (Current != null) Play(Current);
    }

    /// <summary>Moves playback to <paramref name="seconds"/> (clamped to what has been decoded).</summary>
    public void Seek(double seconds)
    {
        lock (_lock)
        {
            if (_rate == 0) return;
            int frame = _channels * 2;
            long b = (long)(seconds * _rate) * frame;
            _seekRequest = (int)Math.Clamp(b, 0, Math.Max(0, _decoded - frame));
        }
        if (State == SoundPlayerState.Stopped && Current != null && _decodeDone)
            StartWorker(Current, decode: false, keepSeek: true);
    }

    private void StartWorker(SoundSource source, bool decode, bool keepSeek = false)
    {
        Current = source;
        Error = null;
        State = decode ? SoundPlayerState.Loading : SoundPlayerState.Playing;
        _pauseRequest = false;
        if (!keepSeek) _seekRequest = -1;
        lock (_lock) _feed = 0;

        _cancel = new CancellationTokenSource();
        var token = _cancel.Token;
        _worker = new Thread(() => Run(source, decode, token)) { IsBackground = true, Name = "SoundPlayer" };
        _worker.Start();
    }

    private void StopWorker()
    {
        _cancel?.Cancel();
        // the worker may be blocked reading the decoder's stdout; killing the process ends that read
        var proc = _proc;
        if (proc != null) VgmstreamCli.Kill(proc);
        _worker?.Join(2000);
        _worker = null;
        _cancel = null;
        _output.Clear();
    }

    private void Run(SoundSource source, bool decode, CancellationToken token)
    {
        Process proc = null;
        var errors = new StringBuilder();
        try
        {
            Stream pcmIn = null;
            if (decode)
            {
                try { Duration = VgmstreamCli.GetInfo(source).Seconds; } catch { /* the decode reports the real error */ }

                proc = VgmstreamCli.StartDecode(source, errors);
                _proc = proc;
                if (token.IsCancellationRequested) return;
                pcmIn = proc.StandardOutput.BaseStream;
                var wav = WavHeader.Read(pcmIn);
                if (wav == null || wav.BitsPerSample != 16)
                {
                    proc.WaitForExit(2000);
                    lock (errors) Fail($"vgmstream could not decode {source.Name}: {FirstLine(errors.ToString(), wav == null ? "no WAV output" : $"{wav.BitsPerSample}-bit output")}");
                    return;
                }
                lock (_lock)
                {
                    _rate = wav.SampleRate;
                    _channels = wav.Channels;
                    // the header's length is exact for vgmstream; reserve it, but grow if it lies
                    _pcm = new byte[Math.Max(64 * 1024, (int)Math.Min(wav.DataLength, 512u * 1024 * 1024))];
                }
            }

            if (!_output.Open(_rate, _channels, out var err)) { Fail(err); return; }
            _output.SetPaused(_pauseRequest);

            var readBuf = new byte[32 * 1024];
            byte[] scaled = new byte[readBuf.Length];
            int frame = _channels * 2;
            int target = (int)(QueueSeconds * _rate) * frame;

            while (!token.IsCancellationRequested)
            {
                // 1. pull more PCM from the decoder
                if (decode && !_decodeDone)
                {
                    int n = pcmIn.Read(readBuf, 0, readBuf.Length);
                    if (n <= 0)
                    {
                        lock (_lock)
                        {
                            _decodeDone = true;
                            _decoded -= _decoded % frame;
                            Duration = (double)_decoded / (_rate * frame);
                        }
                    }
                    else
                    {
                        lock (_lock)
                        {
                            if (_decoded + n > _pcm.Length)
                                Array.Resize(ref _pcm, Math.Max(_pcm.Length * 2, _decoded + n));
                            Buffer.BlockCopy(readBuf, 0, _pcm, _decoded, n);
                            _decoded += n;
                        }
                    }
                }

                // 2. seek
                int seek = _seekRequest;
                if (seek >= 0)
                {
                    _seekRequest = -1;
                    _output.Clear();
                    lock (_lock) _feed = seek - seek % frame;
                }

                // 3. feed SDL up to the target
                if (State == SoundPlayerState.Loading && (_decoded >= target || _decodeDone)) State = _pauseRequest ? SoundPlayerState.Paused : SoundPlayerState.Playing;

                if (!_pauseRequest)
                {
                    while (_output.Queued < target)
                    {
                        int avail;
                        lock (_lock) avail = _decoded - _feed;
                        if (avail <= 0) break;
                        int chunk = Math.Min(avail, scaled.Length);
                        chunk -= chunk % frame;
                        if (chunk == 0) break;
                        lock (_lock) ApplyVolume(_pcm.AsSpan(_feed, chunk), scaled.AsSpan(0, chunk), Volume);
                        _output.Queue(scaled.AsSpan(0, chunk));
                        lock (_lock) _feed += chunk;
                    }
                }

                // 4. end of sound
                bool allFed;
                lock (_lock) allFed = _decodeDone && _feed >= _decoded;
                if (allFed && _output.Queued == 0)
                {
                    if (Loop && _decoded > 0) { lock (_lock) _feed = 0; continue; }
                    State = SoundPlayerState.Stopped;
                    return;
                }

                // while decoding the read blocks for us; afterwards, wait for SDL to drain a little
                if (!decode || _decodeDone || _pauseRequest)
                    Thread.Sleep(10);
            }
        }
        catch (Exception e)
        {
            Fail($"Playback of {source.Name} failed: {e.Message}");
        }
        finally
        {
            if (proc != null)
            {
                _proc = null;
                VgmstreamCli.Kill(proc);
                proc.Dispose();
            }
        }
    }

    private static void ApplyVolume(ReadOnlySpan<byte> src, Span<byte> dst, float volume)
    {
        if (volume >= 0.999f) { src.CopyTo(dst); return; }
        var s = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, short>(src);
        var d = System.Runtime.InteropServices.MemoryMarshal.Cast<byte, short>(dst);
        float v = Math.Clamp(volume, 0f, 1f);
        for (int i = 0; i < s.Length; i++) d[i] = (short)(s[i] * v);
    }

    private void Fail(string message)
    {
        Error = message;
        State = SoundPlayerState.Failed;
        Smithbox.Log(typeof(SoundPlayer), message, LogLevel.Warning);
    }

    private static string FirstLine(string err, string fallback)
    {
        var t = (err ?? "").Trim();
        if (t.Length == 0) return fallback;
        int nl = t.IndexOf('\n');
        return nl < 0 ? t : t[..nl].Trim();
    }

    public void Dispose()
    {
        StopWorker();
        _output.Dispose();
    }
}
