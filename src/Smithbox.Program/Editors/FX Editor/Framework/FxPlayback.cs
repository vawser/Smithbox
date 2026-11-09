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

    // ---------------------------------------------------------------- motion

    /// <summary>How the preview moves the effect: 0 still, 1 forward (a projectile), 2 swing (a weapon).</summary>
    public int Motion
    {
        get => CFG.Current.FxEditor_Preview_Motion;
        set => CFG.Current.FxEditor_Preview_Motion = Math.Clamp(value, 0, 2);
    }

    /// <summary>The settings the motion depends on, so the runtime knows to replay when they change.</summary>
    public (int, float, float, float) MotionKey => (Motion, CFG.Current.FxEditor_Preview_Motion_Speed,
        CFG.Current.FxEditor_Preview_Swing_Radius, CFG.Current.FxEditor_Preview_Swing_Speed);

    /// <summary>
    /// The effect's world transform at a time, null when it stays still.
    /// - Forward: along +Z at the speed (m/s), facing +Z.
    /// - Swing: round a circle of the radius about the Y axis at the angular speed (degrees/s), facing along the
    ///   circle, as a weapon's tip moves in a horizontal slash.
    /// </summary>
    public Func<float, System.Numerics.Matrix4x4> MotionFunction()
    {
        float speed = CFG.Current.FxEditor_Preview_Motion_Speed;
        float radius = CFG.Current.FxEditor_Preview_Swing_Radius;
        float degrees = CFG.Current.FxEditor_Preview_Swing_Speed;

        return Motion switch
        {
            1 => t => System.Numerics.Matrix4x4.CreateTranslation(0, 0, speed * t),
            2 => t =>
            {
                float a = degrees * (MathF.PI / 180f) * t;
                var position = new System.Numerics.Vector3(MathF.Sin(a) * radius, 0, MathF.Cos(a) * radius);
                return System.Numerics.Matrix4x4.CreateRotationY(a + MathF.PI / 2)
                    * System.Numerics.Matrix4x4.CreateTranslation(position);
            },
            _ => null,
        };
    }

    // ---------------------------------------------------------------- visibility

    /// <summary>
    /// Nodes hidden in playback, by object. Editor-only (nothing is written to the file); weak, so a reloaded
    /// effect's old nodes don't linger, and an undone delete brings its node back as it was.
    /// </summary>
    private readonly System.Runtime.CompilerServices.ConditionalWeakTable<SoulsFormats.FxrNode, object> HiddenNodes = new();

    private static readonly object HiddenMark = new();

    public bool IsNodeHidden(SoulsFormats.FxrNode node) => node != null && HiddenNodes.TryGetValue(node, out _);

    public void SetNodeHidden(SoulsFormats.FxrNode node, bool hidden)
    {
        if (node == null)
            return;
        if (hidden)
            HiddenNodes.AddOrUpdate(node, HiddenMark);
        else
            HiddenNodes.Remove(node);
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
