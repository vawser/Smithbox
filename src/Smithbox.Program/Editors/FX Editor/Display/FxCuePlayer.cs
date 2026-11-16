using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// One effect a timeline starts: which effect, when it starts and stops (in the timeline's seconds), and where it is
/// at any time of the timeline. The place is asked for at past times too (a seek replays the effect from its start),
/// so it must not depend on anything but the time.
/// </summary>
public sealed class FxCue
{
    public int EffectId;

    /// <summary>When the effect starts, in seconds of the timeline</summary>
    public float Start;

    /// <summary>When it is told to stop (its nodes then follow their Termination action); null runs it to its end</summary>
    public float? End;

    /// <summary>Where the effect is at a time of the timeline (seconds)</summary>
    public Func<float, Matrix4x4> WorldAt = _ => Matrix4x4.Identity;

    /// <summary>What it stands for in the host (an MQB or TAE event), for the host's own use</summary>
    public object Tag;

    /// <summary>Seeds the effect's randomness, so a replay shows the same particles. 0 takes the effect id.</summary>
    public uint Seed;
}

/// <summary>
/// The effect definitions of a project's effects by id, built once and kept until the effect changes (an FX Editor
/// edit, undo or reload), with the effects their Proxy nodes start. Shared by every player of one host.
/// </summary>
public sealed class FxDefCache
{
    private readonly Func<FxData> GetData;
    private readonly Dictionary<int, (object Fxr, int Version, FxEffectDef Def)> Defs = new();
    private readonly HashSet<int> Logged = new();

    public FxDefCache(Func<FxData> data)
    {
        GetData = data;
    }

    /// <summary>The effect's definition, null if the bank has no such effect (or isn't indexed yet) or it can't be read.</summary>
    public FxEffectDef Get(int id)
    {
        var data = GetData();
        var bank = data?.Bank;
        if (bank == null || data.Schema == null || !bank.Entries.TryGetValue(id, out var entry))
            return null;

        if (!entry.IsLoaded && !bank.Load(entry))
            return null;

        // The FX Editor replaces the entry's file object (reload, revert) or edits it in place (EditVersion)
        if (Defs.TryGetValue(id, out var known) && ReferenceEquals(known.Fxr, entry.Fxr) && known.Version == data.EditVersion)
            return known.Def;

        FxEffectDef def = null;
        try
        {
            def = FxEffectDef.Build(data.Schema, data.Game, entry.Fxr);
        }
        catch (Exception e)
        {
            if (Logged.Add(id))
                Smithbox.LogError(this, $"[FX Display] Couldn't read effect {id}: {e.Message}", e);
        }

        Defs[id] = (entry.Fxr, data.EditVersion, def);
        return def;
    }

    /// <summary>
    /// The effect's definition if <see cref="Get"/> would hand it back without building it (it's built and current,
    /// or the bank has no such effect); false when getting it would read and build the effect.
    /// </summary>
    public bool TryGetBuilt(int id, out FxEffectDef def)
    {
        def = null;
        var data = GetData();
        var bank = data?.Bank;
        if (bank == null || data.Schema == null)
            return false;

        if (!bank.Entries.TryGetValue(id, out var entry))
            return bank.IsIndexed;

        if (!entry.IsLoaded || !Defs.TryGetValue(id, out var known) || !ReferenceEquals(known.Fxr, entry.Fxr)
            || known.Version != data.EditVersion)
            return false;

        def = known.Def;
        return true;
    }

    /// <summary>Whether the bank holds the effect, once it's indexed (null until then).</summary>
    public bool? Has(int id)
    {
        var bank = GetData()?.Bank;
        return bank is { IsIndexed: true } ? bank.Entries.ContainsKey(id) : null;
    }

    public void Clear() => Defs.Clear();
}

/// <summary>
/// Runs the effects a timeline schedules (<see cref="FxCue"/>) at a time of that timeline, for an
/// <see cref="FxDisplay"/> to draw. The time can run on, jump forward or go back:
/// <list type="bullet">
/// <item>An effect starts at its cue's start and is told to stop at its end; it's dropped once it has nothing left
/// to draw.</item>
/// <item>A time before an effect's last step replays it from its start with the same seed, so a time always shows the
/// same particles; a jump forward steps it on in bigger steps.</item>
/// </list>
/// Setting other cues (<see cref="SetCues"/>) starts again from nothing.
/// </summary>
public sealed class FxCuePlayer
{
    private sealed class Running
    {
        public FxCue Cue;
        public FxEffectDef Def;
        public FxEffectInstance Instance;
        public FxRuntimeContext Context;
    }

    private readonly FxDefCache Defs;
    private readonly List<Running> Cues = new();
    private readonly List<FxEffectInstance> Active = new();

    /// <summary>The most an effect is stepped in one frame; a longer jump takes bigger steps.</summary>
    private const int MaxStepsPerFrame = 900;
    private const float Step = 1f / 30f;

    /// <summary>How long after its end an effect may still be drawing (its particles dying out) before it's dropped.</summary>
    public float MaxLinger = 20;

    /// <summary>The effects running at the last <see cref="Update"/>, for <see cref="FxDisplay.Update"/>.</summary>
    public IReadOnlyList<FxEffectInstance> Instances => Active;

    /// <summary>How many cues are set, and how many of their effects aren't in the bank (once it's indexed).</summary>
    public int CueCount => Cues.Count;
    public int MissingCount { get; private set; }

    /// <summary>Particles across the running effects, last update.</summary>
    public int ParticleCount { get; private set; }

    /// <summary>
    /// Gets the cues' effects ready before they play (<see cref="Prepare"/>): an effect isn't started until it is,
    /// and the host holds its clock while <see cref="IsLoading"/>.
    /// </summary>
    public readonly FxPreload Preload;

    /// <summary>Whether the cues' effects are still being made ready: the host's playback should wait.</summary>
    public bool IsLoading => Cues.Count > 0 && !Preload.IsDone;

    public FxCuePlayer(FxDefCache defs)
    {
        Defs = defs;
        Preload = new FxPreload(defs);
    }

    public void SetCues(IEnumerable<FxCue> cues)
    {
        Cues.Clear();
        Active.Clear();
        foreach (var cue in cues)
            Cues.Add(new Running { Cue = cue });

        // The same effects again (an edit elsewhere in the timeline) stay ready
        Preload.Set(Cues.Select(r => r.Cue.EffectId));
    }

    /// <summary>
    /// Makes the cues' effects ready a little more: definitions, textures and models. Call every frame before
    /// <see cref="Update"/>, after the bank's own update. Returns whether they're all ready.
    /// </summary>
    /// <param name="display">The display that draws them, for their textures and models</param>
    public bool Prepare(FxDisplay display) => Preload.Update(display);

    public void Clear() => SetCues(Array.Empty<FxCue>());

    /// <summary>Starts every effect running at <paramref name="time"/> again from its start.</summary>
    public void Restart()
    {
        foreach (var r in Cues)
            r.Instance = null;
    }

    /// <param name="time">The timeline's time, in seconds</param>
    /// <param name="eye">The camera position (for effects that read it)</param>
    public void Update(float time, Vector3 eye)
    {
        Active.Clear();
        MissingCount = 0;
        ParticleCount = 0;

        foreach (var r in Cues)
        {
            var cue = r.Cue;
            float local = time - cue.Start;

            // Not started yet, or long since stopped
            if (local < 0 || cue.End is { } end && time > end + MaxLinger)
            {
                r.Instance = null;
                continue;
            }

            // Not started until it's ready: building it here would stall the frame it first shows in
            if (r.Instance == null && !Preload.IsReady(cue.EffectId))
                continue;

            // The definition (again, if the FX Editor changed the effect)
            var def = Defs.Get(cue.EffectId);
            if (def == null)
            {
                if (Defs.Has(cue.EffectId) == false)
                    MissingCount++;
                r.Instance = null;
                continue;
            }

            if (def != r.Def)
            {
                r.Def = def;
                r.Instance = null;
            }

            if (r.Instance == null || local < r.Instance.Time - 1e-4f)
                Start(r);

            if (r.Instance == null)
                continue;

            r.Context.CameraPosition = eye;

            try
            {
                // Stepped to its end, told to stop there, then stepped on
                float? stopAt = cue.End is { } e ? Math.Max(0, e - cue.Start) : null;
                if (stopAt is { } s && !r.Instance.Terminated && local >= s)
                {
                    SimulateTo(r.Instance, s);
                    r.Instance.Terminate();
                }
                SimulateTo(r.Instance, local);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[FX Display] Effect {cue.EffectId} stopped: {e.Message}", e);
                r.Instance = null;
                continue;
            }

            if (r.Instance.Finished)
                continue;

            Active.Add(r.Instance);
            ParticleCount += r.Context.ParticleCount;
        }
    }

    private static void SimulateTo(FxEffectInstance instance, float target)
    {
        float behind = target - instance.Time;
        if (behind <= 0)
            return;
        instance.SimulateTo(target, Math.Max(Step, behind / MaxStepsPerFrame));
    }

    private void Start(Running r)
    {
        var cue = r.Cue;
        r.Context = new FxRuntimeContext
        {
            ResolveEffect = Defs.Get,
            // The effect's own time is the time since its cue started
            Motion = t => cue.WorldAt(cue.Start + t),
        };
        uint seed = cue.Seed != 0 ? cue.Seed : (uint)cue.EffectId | 1;
        r.Instance = new FxEffectInstance(r.Def, r.Context, seed);
    }
}
