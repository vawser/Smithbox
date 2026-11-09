namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The viewport's playback clock and the inputs an effect is previewed with (FX Editor Plan §10.2): the preview
/// state and the external values the game would set (0 = Terminate, 1 = precipitation, 2 = time of day, ...).
/// The effect runtime (phase V2) reads its time and inputs from here; until then only the clock runs.
/// </summary>
public class FxPlayback
{
    /// <summary>Seconds since the effect started.</summary>
    public float Time;

    public bool Playing = true;

    /// <summary>Raised when the effect is started again (Restart, a loop, or a new effect).</summary>
    public event Action Restarted;

    /// <summary>The state the debug shapes show; the runtime picks its own from the state conditions.</summary>
    public int PreviewState;

    /// <summary>External values by id, as the game sets them. Missing ids read as 0.</summary>
    public SortedDictionary<int, float> ExternalValues = new() { { 0, 0 } };

    /// <summary>Bumped when anything the preview depends on changes, so the viewport knows to rebuild.</summary>
    public int Version { get; private set; }

    public float Speed
    {
        get => CFG.Current.FxEditor_Playback_Speed;
        set => CFG.Current.FxEditor_Playback_Speed = Math.Clamp(value, 0.01f, 10f);
    }

    public bool Loop
    {
        get => CFG.Current.FxEditor_Playback_Loop;
        set => CFG.Current.FxEditor_Playback_Loop = value;
    }

    /// <summary>
    /// How long the clock runs before it loops or stops. Effects have no stored length (they end by their
    /// state conditions), so this is set by hand.
    /// </summary>
    public float Length
    {
        get => CFG.Current.FxEditor_Playback_Length;
        set => CFG.Current.FxEditor_Playback_Length = Math.Clamp(value, 0.1f, 600f);
    }

    public void Update(float dt)
    {
        if (!Playing)
            return;

        Time += dt * Speed;

        if (Time < Length)
            return;

        if (Loop)
        {
            Restart();
        }
        else
        {
            Time = Length;
            Playing = false;
        }
    }

    public void Restart()
    {
        Time = 0;
        Restarted?.Invoke();
    }

    /// <summary>Moves the clock to a time (the scrubber). The runtime catches up by stepping from the start.</summary>
    public void Seek(float time)
    {
        Time = Math.Clamp(time, 0, Length);
    }

    public float GetExternalValue(int id) => ExternalValues.TryGetValue(id, out var v) ? v : 0;

    public void SetExternalValue(int id, float value)
    {
        ExternalValues[id] = value;
        Version++;
    }

    public void RemoveExternalValue(int id)
    {
        if (ExternalValues.Remove(id))
            Version++;
    }

    public void SetPreviewState(int state)
    {
        if (state == PreviewState)
            return;
        PreviewState = Math.Max(0, state);
        Version++;
    }
}
