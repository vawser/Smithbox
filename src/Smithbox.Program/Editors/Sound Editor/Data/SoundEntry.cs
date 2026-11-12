namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// One playable sound in a container: an FSB sample, or a Wwise media (wem) the bank holds or streams.
/// Facts come from our own header parse; <see cref="Info"/> is vgmstream's, fetched when the row is selected.
/// </summary>
public class SoundEntry
{
    public SoundContainer Container;

    /// <summary>Position in the container's list.</summary>
    public int Index;

    /// <summary>Wwise media ID (0 for FMOD).</summary>
    public uint MediaId;

    /// <summary>FSB sample name; empty for Wwise until S3 names media.</summary>
    public string Name = "";

    /// <summary>For DS2 binders: the .fsb inside the binder this sample comes from.</summary>
    public string InnerFile;

    public string Codec = "";
    public int SampleRate;
    public int Channels;
    public double Seconds;

    /// <summary>Seconds is data size / byte rate (wem header), not an exact sample count.</summary>
    public bool SecondsApprox;

    public long LoopStart = -1;
    public long LoopEnd = -1;

    /// <summary>Wwise: played from a separate .wem file instead of the bank's DATA.</summary>
    public bool IsStreamed;
    public string StreamPath;

    /// <summary>Wwise: the HIRC Sound objects that play this media.</summary>
    public int SoundObjectCount;

    /// <summary>No audio data (e.g. DS2's frpg2_c7310 placeholder).</summary>
    public bool IsEmpty;

    /// <summary>Wwise plug-in media in DIDX (effect / impulse-response data), not a wem: listed, not playable.</summary>
    public bool IsNotAudio;

    public bool CanPlay => Source != null && !IsEmpty && !IsNotAudio;

    public SoundSource Source;

    public SoundInfo Info;
    public string InfoError;
    public bool InfoRequested;

    /// <summary>The label used in lists: the sample name, or the media ID.</summary>
    public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : MediaId.ToString();

    /// <summary>Best known length: vgmstream's once fetched, else our own.</summary>
    public double BestSeconds => Info != null && Info.Samples > 0 ? Info.Seconds : Seconds;
}
