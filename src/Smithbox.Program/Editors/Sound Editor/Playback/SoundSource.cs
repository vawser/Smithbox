using System;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// One playable sound, as the decoder needs it: the bytes of a file vgmstream can read, the extension it should see
/// (vgmstream picks its parser by extension), and the subsong inside it.
/// <para>FMOD: the whole .fsb, Subsong = sample index + 1. Wwise: a single .wem (streamed file, or the slice of a
/// bank's DATA chunk that DIDX points at), Subsong = 0.</para>
/// </summary>
public sealed class SoundSource
{
    /// <summary>Shown in logs and errors.</summary>
    public string Name { get; init; }

    /// <summary>Extension without the dot: "fsb", "wem".</summary>
    public string Extension { get; init; }

    /// <summary>1-based subsong for multi-sound files, 0 for single-sound files.</summary>
    public int Subsong { get; init; }

    /// <summary>
    /// Identifies the bytes, so the temp file for a container is written once and reused for each of its subsongs
    /// (e.g. "DS3|/sound/fdp_c1070.fsb"). Sources with the same key must have the same bytes.
    /// </summary>
    public string CacheKey { get; init; }

    /// <summary>Reads the bytes. Only called when the temp file for <see cref="CacheKey"/> doesn't exist yet.</summary>
    public Func<byte[]> ReadBytes { get; init; }
}
