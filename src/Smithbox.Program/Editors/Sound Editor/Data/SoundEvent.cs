namespace StudioCore.Editors.SoundEditor;

public enum SoundEventNameSource
{
    /// <summary>No name known: shown by ID.</summary>
    None,
    /// <summary>From the shipped Wwise name table.</summary>
    Table,
    /// <summary>Built from a sound ID of the bank's own media, hash confirmed.</summary>
    Rule,
    /// <summary>Found by trying the bank's ID range: the hash matches, but it may be a different name with the same hash.</summary>
    Guess,
}

/// <summary>
/// One Wwise event of a bank: its actions and the media its Play actions can reach (through the HIRC graph).
/// </summary>
public class SoundEvent
{
    public SoundContainer Container;
    public int Index;
    public uint Id;

    public string Name = "";
    public SoundEventNameSource NameSource;

    public string SoundType;
    public string SoundId;

    public int PlayActions, StopActions, OtherActions;

    /// <summary>Play targets that aren't in this bank (another bank holds the sound).</summary>
    public int ExternalTargets;

    /// <summary>The bank's sounds this event can play (any of them, depending on random / switch containers).</summary>
    public List<SoundEntry> Entries = new();

    public string DisplayName => !string.IsNullOrEmpty(Name) ? Name : Id.ToString();

    /// <summary>The project's Sounds alias for <see cref="SoundId"/>, or null.</summary>
    public string Alias => SoundId == null ? null : Container?.Data?.Names?.SoundAlias(SoundId)?.Name;

    public string ActionSummary
    {
        get
        {
            var parts = new List<string>();
            if (PlayActions > 0) parts.Add(PlayActions == 1 ? "Play" : $"Play x{PlayActions}");
            if (StopActions > 0) parts.Add(StopActions == 1 ? "Stop" : $"Stop x{StopActions}");
            if (OtherActions > 0) parts.Add($"+{OtherActions}");
            return string.Join(", ", parts);
        }
    }
}
