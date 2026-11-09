using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Runs the selected effect for the viewport (FX Editor Plan, phase V2). It follows the playback clock: forward
/// steps simulate on, and a clock that goes back (restart, loop, scrubbing) replays from time 0 with the same seed,
/// so the same time always shows the same particles. An edit, undo or redo rebuilds the effect and replays it to
/// the current time.
/// </summary>
public class FxPreviewRuntime
{
    private readonly FxEditorScreen Editor;

    public FxEffectInstance Instance { get; private set; }
    public FxRuntimeContext Context { get; private set; }

    /// <summary>Why the effect couldn't be built, null if it could.</summary>
    public string Error { get; private set; }

    private FxEntry Entry;
    private object Fxr;
    private int Edits = -1;
    private FxEffectDef Def;
    private readonly Dictionary<int, FxEffectDef> ProxyDefs = new();

    /// <summary>The most a catch-up may step in one frame; a longer seek takes bigger steps.</summary>
    private const int MaxStepsPerFrame = 900;
    private const float Step = 1f / 30f;

    public FxPreviewRuntime(FxEditorScreen editor)
    {
        Editor = editor;
    }

    /// <summary>
    /// Live changes (a value mid-edit, phase V6) rebuild at most this often: each rebuild replays the effect from 0 to
    /// the current time, so a drag rebuilding every frame would stall a long effect.
    /// </summary>
    private const double LiveInterval = 1 / 15.0;

    private int LiveEdits = -1;
    private DateTime LastBuild;

    public void Update(int editVersion, int liveVersion, Vector3 cameraPosition)
    {
        var entry = Editor.Selection.Entry;
        if (entry?.Fxr == null)
        {
            Clear();
            return;
        }

        // A committed edit, undo or a new effect rebuilds now; a live change once the interval has passed (a later
        // frame picks it up if nothing else changes)
        bool committed = entry != Entry || entry.Fxr != Fxr || editVersion != Edits;
        bool live = liveVersion != LiveEdits && (DateTime.Now - LastBuild).TotalSeconds >= LiveInterval;
        if (committed || live)
        {
            Entry = entry;
            Fxr = entry.Fxr;
            Edits = editVersion;
            LiveEdits = liveVersion;
            LastBuild = DateTime.Now;
            ProxyDefs.Clear();
            Instance = null;
            Build(entry);
        }

        if (Def == null)
            return;

        var playback = Editor.Playback;
        float target = playback.Time;

        // A different motion is a different path from the start, and different forces push particles differently
        if (!playback.MotionKey.Equals(MotionKey) || !playback.ForceKey.Equals(ForceKey))
        {
            MotionKey = playback.MotionKey;
            ForceKey = playback.ForceKey;
            Instance = null;
        }

        if (Instance == null || target < Instance.Time - 1e-4f)
            Restart();

        Context.External = playback.GetExternalValue;
        Context.CameraPosition = cameraPosition;

        float behind = target - Instance.Time;
        if (behind <= 0)
            return;

        // Normal playback is one frame's worth; a seek forward is stepped at most MaxStepsPerFrame times
        float step = Math.Max(Step, behind / MaxStepsPerFrame);
        try
        {
            Instance.SimulateTo(target, step);
        }
        catch (Exception e)
        {
            Error = e.Message;
            Smithbox.LogError(this, $"[FX Editor] The preview of {entry.Stem} stopped: {e.Message}", e);
            Def = null;
            Instance = null;
        }
    }

    private void Build(FxEntry entry)
    {
        Error = null;
        try
        {
            Def = FxEffectDef.Build(Editor.Data.Schema, Editor.Data.Game, entry.Fxr);
        }
        catch (Exception e)
        {
            Def = null;
            Error = e.Message;
            Smithbox.LogError(this, $"[FX Editor] The preview couldn't read {entry.Stem}: {e.Message}", e);
        }
    }

    private (int, float, float, float) MotionKey;
    private (bool, float, float, bool) ForceKey;

    /// <summary>Where the effect is now, as the motion puts it (identity when it stays still).</summary>
    public System.Numerics.Matrix4x4 EffectWorld => Instance?.World ?? System.Numerics.Matrix4x4.Identity;

    private void Restart()
    {
        Context = new FxRuntimeContext
        {
            ResolveEffect = ResolveProxy,
            Motion = Editor.Playback.MotionFunction(),
            IsHidden = Editor.Playback.IsNodeHidden,
            Forces = new FxForceField
            {
                Wind = Editor.Playback.WindVector(),
                UseVolumes = CFG.Current.FxEditor_Preview_Effect_Forces,
            },
        };
        // Seeded by the effect id, so a replay matches what was shown before
        Instance = new FxEffectInstance(Def, Context, (uint)Entry.Id | 1);
    }

    /// <summary>Another effect for a Proxy node, from the bank (with its unsaved edits, if it has any).</summary>
    private FxEffectDef ResolveProxy(int id)
    {
        if (ProxyDefs.TryGetValue(id, out var def))
            return def;

        def = null;
        var bank = Editor.Data.Bank;
        if (bank.Entries.TryGetValue(id, out var entry) && (entry.IsLoaded || bank.Load(entry)))
        {
            try { def = FxEffectDef.Build(Editor.Data.Schema, Editor.Data.Game, entry.Fxr); }
            catch { def = null; }
        }

        ProxyDefs[id] = def;
        return def;
    }

    public void Clear()
    {
        Entry = null;
        Fxr = null;
        Def = null;
        Instance = null;
        Context = null;
        Error = null;
        ProxyDefs.Clear();
    }
}
