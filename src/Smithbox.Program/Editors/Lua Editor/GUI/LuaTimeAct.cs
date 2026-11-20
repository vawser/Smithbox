#nullable enable
using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.FxEditor;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ProjectileEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Renderer;
using System.Drawing;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>What a fired TAE event did, for the AI Simulation's log</summary>
public enum LuaTimeActLogKind
{
    Event,
    Effect,
    Bullet,
    SpEffect,
    Problem,
}

/// <summary>A SpEffect the character's TAE has put on it: which, from what event, and until when (scene clock)</summary>
public sealed class LuaActiveSpEffect
{
    public int Id;
    public string? Name;
    public string Event = "";

    /// <summary>The SpEffect's effectEndurance: how long it lasts after its event (below 0: until the simulation stops)</summary>
    public float Endurance;

    public float Until;
}

/// <summary>One line of the log: when it fired (scene clock and the clip's own time), in which clip, and what it did</summary>
public sealed record LuaTimeActLogEntry(float SceneTime, long Animation, float ClipTime, LuaTimeActLogKind Kind,
    string Event, string Detail);

/// <summary>
/// The TimeAct of the clip the Lua viewer's character is playing: its TAE events step with the clip, and those that
/// spawn effects and bullets show them in the viewport (see Claude/Documentation/Lua Editor TAE Simulation.md).
/// <list type="bullet">
/// <item>The clip's TAE animation comes from the Time Act Editor's bank (so unsaved TAE edits show), found in the
/// character's animation sets as its clips are; an animation that imports another's events plays those.</item>
/// <item>The viewer's clock only goes forward, and clips change every step, so effects and bullets run on one scene
/// clock: a clip's events are placed on it from when the clip started (its play), and what they spawn plays on into the
/// next step. A lasting effect stops at its event's end, or when its play ends.</item>
/// <item>Effects: the event's FFX on its dummy poly (following it, unless the event says otherwise), drawn by the shared
/// FX display, from the FX Editor's bank.</item>
/// <item>Bullets: the event's BehaviorParam row (by the character's NpcParam variations, or as it is for a common
/// judge) gives the Bullet row, flown by the Projectile Editor's simulation from the dummy poly, the way the character
/// faces, at the scene's target, with its flight and hit effects.</item>
/// </list>
/// </summary>
public sealed class LuaTimeAct : IDisposable
{
    private const int LogLimit = 300;
    private const float MinFlightLength = 1f / 30f;
    private const float HitEffectLength = 5.0f;
    private const int BehaviorRefBullet = 1;

    // An NPC's BehaviorParam rows: 200000000 + its behaviorVariationId × 1000 + the event's judge ID (as the Time Act
    // Editor's resolver numbers them)
    private const int NpcBehaviorBase = 200000000;
    private const int BehaviorVariationScale = 1000;
    private static readonly TimeSpan BuildInterval = TimeSpan.FromMilliseconds(33);

    private readonly LuaViewer Viewer;
    private ProjectEntry Project => Viewer.Universe.Project;

    private readonly FxDefCache Defs;
    private readonly FxCuePlayer Effects;
    private FxDisplay? Display;

    private readonly TimeActEventResolver Resolver;
    private readonly ProjectileDebugLines Lines = new() { Overlay = true };
    private bool LinesShown;
    private DateTime LastBuild;

    /// <summary>Seconds since the viewer started: the clock effects and bullets run on</summary>
    public float SceneTime { get; private set; }

    /// <summary>The clip being played, with its TAE animation and the events it has fired</summary>
    public LuaTimeActPlay? Current { get; private set; }

    /// <summary>What the events did, newest last</summary>
    public List<LuaTimeActLogEntry> Log { get; } = new();

    /// <summary>Bumped whenever the log gains a line, for the window to follow</summary>
    public int LogVersion { get; private set; }

    private readonly List<Bullet> Bullets = new();
    private readonly List<LiveAnchor> Anchors = new();

    // --- SpEffects the TAE puts on the character ---

    // Held a little past the frame the event was last seen active in, so one that lasts no time still counts
    private const float SpEffectHold = 0.05f;
    private const string SpEffectParamName = "SpEffectParam";

    private readonly Dictionary<int, LuaActiveSpEffect> SpEffects = new();

    /// <summary>The SpEffects on the character now from its TAE's AddSpEffect events</summary>
    public IReadOnlyCollection<LuaActiveSpEffect> ActiveSpEffects => SpEffects.Values;

    /// <summary>Bumped whenever a SpEffect goes on or comes off, for the situation to evaluate again</summary>
    public int SpEffectVersion { get; private set; }

    /// <summary>A SpEffect went on (it wasn't on before): for the goal's Interrupt, if it observes it</summary>
    public event Action<int>? SpEffectActivated;

    public bool HasActiveSpEffect(int id) => SpEffects.TryGetValue(id, out var s) && s.Until >= SceneTime;

    /// <summary>
    /// Puts an event's SpEffect on (or keeps it on): while the event is active, then for the SpEffect's effectEndurance
    /// (a negative one stays until the simulation is reset). That AddSpEffect applies over its event's span and the
    /// endurance runs from its end is an assumption.
    /// </summary>
    private void ApplySpEffect(LuaTimeActPlay play, TAE.Event ev, int id)
    {
        if (!SpEffects.TryGetValue(id, out var active))
        {
            var row = CharacterComposition.GetRow(Project, SpEffectParamName, id);
            active = new LuaActiveSpEffect
            {
                Id = id,
                Name = row?.Name,
                Event = EventName(ev),
                Endurance = row != null && CharacterComposition.HasField(row, "effectEndurance")
                    ? CharacterComposition.GetFloat(row, "effectEndurance")
                    : 0,
            };
            SpEffects[id] = active;
            SpEffectVersion++;
            SpEffectActivated?.Invoke(id);

            var life = active.Endurance < 0 ? LOC.Get("LUA_TAE_SpEffect_Permanent")
                : LOC.Get("LUA_TAE_SpEffect_Endurance", $"{active.Endurance:0.##}");
            AddLog(play, ev, LuaTimeActLogKind.SpEffect, active.Event,
                LOC.Get("LUA_TAE_SpEffect", string.IsNullOrEmpty(active.Name) ? $"{id}" : $"{id} {active.Name}", life));
        }

        var until = active.Endurance < 0 ? float.PositiveInfinity : SceneTime + Math.Max(active.Endurance, SpEffectHold);
        active.Until = Math.Max(active.Until, until);
    }

    /// <summary>Keeps the playing clip's active AddSpEffect events' SpEffects on, and takes off those that have run out</summary>
    private void UpdateSpEffects()
    {
        if (Current is { } play)
        {
            foreach (var (ev, id) in play.SpEffectEvents)
            {
                if (play.LastTime >= ev.StartTime && play.LastTime < ev.MemeEndTime)
                    ApplySpEffect(play, ev, id);
            }
        }

        var expired = SpEffects.Values.Where(s => s.Until < SceneTime).Select(s => s.Id).ToList();
        foreach (var id in expired)
            SpEffects.Remove(id);
        if (expired.Count > 0)
            SpEffectVersion++;
    }

    // The TAE sources by animation set name (c4300), as found
    private readonly Dictionary<string, TimeActContainerWrapper?> Sources = new(StringComparer.OrdinalIgnoreCase);

    public LuaTimeAct(LuaViewer viewer)
    {
        Viewer = viewer;
        Defs = new FxDefCache(() => viewer.Universe.Project.Handler.FxData);
        Effects = new FxCuePlayer(Defs);
        Resolver = new TimeActEventResolver(viewer.Universe.Project);
    }

    /// <summary>Whether the project can show effects (the FX Editor is enabled for it)</summary>
    public bool EffectsAvailable => Project.Handler.FxData is { Bank: not null, Schema: not null };

    /// <summary>Whether the project can read TAE events (the Time Act Editor is enabled for it)</summary>
    public bool TimeActsAvailable => Project.Handler.TimeActData != null;

    public int RunningEffects => Effects.Instances.Count;
    public int MissingEffects => Effects.MissingCount;
    public int LoadingEffects => Effects.IsLoading ? Effects.Preload.Count - Effects.Preload.ReadyCount : 0;
    public int FlyingBullets => Bullets.Count(b => SceneTime - b.FiredAt <= b.Result.Duration);
    public bool BankIndexing => Project.Handler.FxData?.Bank is { } bank && !bank.IsIndexed;

    // --- the clock ---

    /// <summary>
    /// Steps the current clip's events, after the character's clip has been posed for this frame (the panel's
    /// update) and the act player has started any new clip
    /// </summary>
    public void Update(float dt)
    {
        SceneTime += dt;
        Lines.Update();

        var self = Viewer.Self;
        var animator = self?.Animator;
        if (self == null || animator?.CurrentAnimation == null || self.IsLoading)
        {
            EndPlay();
        }
        else
        {
            var id = (long)animator.CurrentAnimation.Id % 1_000_000_000;
            var time = animator.Time;

            // Another clip, the same clip from its start again (a loop, a step played again), or another character
            if (Current == null || Current.Character != self || Current.AnimationId != id || time + 1e-3f < Current.LastTime)
            {
                EndPlay();
                Current = StartPlay(self, id, time, animator.Duration);
            }

            Step(Current, time);
        }

        UpdateSpEffects();
        UpdateAnchors();
        UpdateEffects();
        DrawBullets();
    }

    /// <summary>Forgets everything shown (the character changed, or the viewer closed)</summary>
    public void Reset()
    {
        EndPlay();
        Effects.Clear();
        Display?.Clear();
        Anchors.Clear();
        Bullets.Clear();
        if (SpEffects.Count > 0)
        {
            SpEffects.Clear();
            SpEffectVersion++;
        }
        HideLines();
    }

    public void ClearLog()
    {
        Log.Clear();
        LogVersion++;
    }

    private LuaTimeActPlay StartPlay(LuaCharacter self, long id, float time, float duration)
    {
        var play = new LuaTimeActPlay(self, id, SceneTime - time, duration);

        if (Project.Handler.TimeActData is not { } data)
        {
            play.Why = LOC.Get("LUA_TAE_Why_No_Editor");
            return play;
        }

        if (!FindAnimation(self.AnimationName, id, out var source, out var entry, out var animation))
        {
            play.Why = LOC.Get("LUA_TAE_Why_No_Animation", FormatAnimation(id));
            return play;
        }

        // An animation that imports another plays that one's events
        for (var hops = 0; hops < 4 && animation!.Events.Count == 0 &&
                           animation.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim import; hops++)
        {
            if (!source!.TryFindAnimation(import.ImportFromAnimID, out var importedEntry, out var imported))
                break;
            play.ImportedFrom = import.ImportFromAnimID;
            entry = importedEntry;
            animation = imported;
        }

        entry!.ApplyTemplate(animation!, data.GetTemplate(source!));

        play.Source = source;
        play.Entry = entry;
        play.Animation = animation;
        play.Events = animation!.Events.OrderBy(e => e.StartTime).ToList();

        // The turn speed events (SetTurnSpeed): the act player turns the character during them. The mask events
        // (DrawMask, HideMask, ShowMask): the viewer shows and hides the model's mask groups by them.
        foreach (var ev in play.Events)
        {
            if (ev.Parameters == null)
                continue;
            var role = data.GetSimulationRole(ev);
            if (role == TimeActSimulationRole.TurnSpeed && TimeActSimulation.GetFloat(ev, "TurnSpeed") is { } speed)
                play.TurnEvents.Add((ev, speed));
            // How long the clip blends in from the last (its first Blend event's length, as the Time Act Editor reads it)
            else if (role == TimeActSimulationRole.Blend && play.BlendDuration == null &&
                     ev.EndTime < TAE.Event.EldenRingInfiniteLengthEventPlaceholder)
                play.BlendDuration = Math.Max(ev.EndTime - ev.StartTime, 0);
            else if (role is TimeActSimulationRole.DrawMask or TimeActSimulationRole.HideMask or TimeActSimulationRole.ShowMask)
                play.MaskEvents.Add((ev, role));
            // The SpEffect is the field the meta refers to SpEffectParam (SpEffectID)
            else if (role == TimeActSimulationRole.AddSpEffect &&
                     ev.Parameters.Values.Keys.FirstOrDefault(k => data.GetFieldMeta(ev, k)?.ParamRef == SpEffectParamName) is { } field &&
                     GetInt(ev, field) is > 0 and var spEffect)
                play.SpEffectEvents.Add((ev, spEffect));
        }

        // The play's effects are cued now, so they are loading before they fire. Effect events are known (for the
        // log and the event list) even when the effects aren't shown.
        var cues = new List<(FxCue Cue, LiveAnchor Anchor)>();
        for (var i = 0; i < play.Events.Count; i++)
        {
            if (BuildEffectCue(play, play.Events[i], i) is { } cue)
                cues.Add(cue);
        }
        if (CFG.Current.LuaEditor_TimeAct_Effects && CFG.Current.LuaEditor_Display_Tae_Effects && EffectsAvailable && cues.Count > 0)
        {
            Anchors.AddRange(cues.Select(c => c.Anchor));
            Effects.AddCues(cues.Select(c => c.Cue));
        }

        return play;
    }

    /// <summary>Fires the events the clip's time has passed</summary>
    private void Step(LuaTimeActPlay play, float time)
    {
        play.LastTime = time;
        foreach (var ev in play.Events)
        {
            if (ev.StartTime > time + 1e-4f)
                break;
            if (!play.Fired.Add(ev))
                continue;
            Fire(play, ev);
        }
    }

    /// <summary>The play has ended: its lasting effects are told to stop now, if their event hadn't ended already</summary>
    private void EndPlay()
    {
        if (Current == null)
            return;

        foreach (var cue in Current.LastingCues)
        {
            if (cue.End is not { } end || end > SceneTime)
                cue.End = Math.Max(SceneTime, cue.Start);
        }

        // Cued effects whose event never came are moved into the past, stopped, so the player drops them unstarted
        var gone = SceneTime - Effects.MaxLinger - 1;
        foreach (var cue in Current.PendingCues)
        {
            if (cue.Start > SceneTime)
            {
                cue.Start = gone;
                cue.End = gone;
            }
        }

        Current = null;
    }

    private void Fire(LuaTimeActPlay play, TAE.Event ev)
    {
        var name = EventName(ev);
        var data = Project.Handler.TimeActData;

        if (ev.Parameters == null)
        {
            if (CFG.Current.LuaEditor_TimeAct_Log_All_Events)
                AddLog(play, ev, LuaTimeActLogKind.Event, name, LOC.Get("LUA_TAE_No_Template"));
            return;
        }

        // Applied as it fires too, for an event the clock passes over in one frame
        if (play.SpEffectEvents.FirstOrDefault(s => s.Event == ev) is { Event: not null } added)
        {
            ApplySpEffect(play, ev, added.Id);
            return;
        }

        if (data?.GetSimulationRole(ev) == TimeActSimulationRole.Bullet)
        {
            FireBullet(play, ev, name);
            return;
        }

        if (play.EffectEvents.TryGetValue(ev, out var effect))
        {
            var dummy = effect.Dummy;
            var found = dummy < 0 || play.Character.DummyWorld(dummy) != null;
            var where = dummy >= 0 ? LOC.Get("LUA_TAE_On_Dummy", dummy) : LOC.Get("LUA_TAE_On_Body");
            if (!found)
                where = LOC.Get("LUA_TAE_Dummy_Missing", dummy);
            var detail = LOC.Get("LUA_TAE_Effect", effect.EffectId, where, LifeText(ev, effect.OneShot));
            if (!EffectsAvailable)
                detail += "  " + LOC.Get("LUA_TAE_Effect_No_Bank");
            else if (Defs.Has(effect.EffectId) == false)
                detail += "  " + LOC.Get("LUA_TAE_Effect_Missing");
            AddLog(play, ev, found ? LuaTimeActLogKind.Effect : LuaTimeActLogKind.Problem, name, detail);
            return;
        }

        if (CFG.Current.LuaEditor_TimeAct_Log_All_Events)
            AddLog(play, ev, LuaTimeActLogKind.Event, name, Summary(ev));
    }

    private void AddLog(LuaTimeActPlay play, TAE.Event ev, LuaTimeActLogKind kind, string name, string detail)
    {
        Log.Add(new LuaTimeActLogEntry(SceneTime, play.AnimationId, ev.StartTime, kind, name, detail));
        if (Log.Count > LogLimit)
            Log.RemoveRange(0, Log.Count - LogLimit);
        LogVersion++;
    }

    // --- the TAE ---

    /// <summary>The TAE animation of a clip: in the character's own anibnd, else in the sets it shares</summary>
    private bool FindAnimation(string chr, long id, out TimeActContainerWrapper? source, out TimeActEntry? entry,
        out TAE.Animation? animation)
    {
        foreach (var set in AnimationPackage.GetAnimationSetNames(AnimatedModelKind.Character, chr))
        {
            source = GetSource(set);
            if (source != null && source.TryFindAnimation(id, out entry, out animation))
                return true;
        }
        source = null;
        entry = null;
        animation = null;
        return false;
    }

    /// <summary>
    /// The TAE source of an animation set: the Time Act Editor's (with its edits), else one read from the project's
    /// files while the editor's bank is still scanning
    /// </summary>
    private TimeActContainerWrapper? GetSource(string set)
    {
        var bank = Project.Handler.TimeActData?.PrimaryBank;
        var fromBank = bank?.GetCharacters().FirstOrDefault(w => string.Equals(w.Name, set, StringComparison.OrdinalIgnoreCase));
        if (fromBank != null)
        {
            Sources[set] = fromBank;
            // Read once: populating again would read the binder again (the editor does the same when it opens it)
            if (fromBank.TimeActs.Count == 0)
                fromBank.PopulateTimeActList();
            return fromBank;
        }

        if (Sources.TryGetValue(set, out var known))
            return known;

        TimeActContainerWrapper? made = null;
        var sourceEntry = Project.Locator.TimeActFiles.Entries.FirstOrDefault(e =>
            e.Folder.StartsWith("/chr", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(e.Filename, set, StringComparison.OrdinalIgnoreCase));
        if (sourceEntry != null)
        {
            try
            {
                var modelEntry = Project.Locator.ChrFiles.Entries.FirstOrDefault(e =>
                    string.Equals(e.Filename, set, StringComparison.OrdinalIgnoreCase));
                made = new TimeActContainerWrapper(Project, sourceEntry, modelEntry, Project.VFS.FS);
                made.PopulateTimeActList();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("LUA_TAE_Read_Failed", sourceEntry.Path), e);
                made = null;
            }
        }

        // Not remembered while the bank is still scanning: it may yet list the set
        if (made != null || bank is not { IsScanning: true })
            Sources[set] = made;
        return made;
    }

    public static string FormatAnimation(long id) => $"a{id / 1_000_000:D3}_{id % 1_000_000:D6}";

    private static string EventName(TAE.Event ev) => ev.TypeName ?? LOC.Get("LUA_TAE_Event_Type", ev.Type);

    /// <summary>The event's parameters as a short line (the first few)</summary>
    private static string Summary(TAE.Event ev)
    {
        if (ev.Parameters?.Values is not { Count: > 0 } values)
            return "";
        var text = string.Join(", ", values.Take(4).Select(kv => $"{kv.Key} {kv.Value}"));
        return values.Count > 4 ? text + ", …" : text;
    }

    private static string LifeText(TAE.Event ev, bool oneShot)
    {
        if (oneShot)
            return LOC.Get("LUA_TAE_Life_OneShot");
        return ev.EndTime >= TAE.Event.EldenRingInfiniteLengthEventPlaceholder
            ? LOC.Get("LUA_TAE_Life_Clip")
            : LOC.Get("LUA_TAE_Life_Seconds", $"{Math.Max(ev.EndTime - ev.StartTime, 0):0.00}");
    }

    private static int? GetInt(TAE.Event ev, string name) =>
        ev.Parameters?.Values is { } values && values.TryGetValue(name, out var value) && value is IConvertible number
            ? number.ToInt32(null)
            : null;

    private static bool? GetBool(TAE.Event ev, string name)
    {
        if (ev.Parameters?.Values is not { } values || !values.TryGetValue(name, out var value))
            return null;
        return value switch
        {
            bool b => b,
            IConvertible c => c.ToInt32(null) != 0,
            _ => null
        };
    }

    // --- effects ---

    /// <summary>An effect cue's place: on a dummy poly of a character, followed every frame, or held where it spawned</summary>
    private sealed class LiveAnchor
    {
        public required LuaCharacter Character;
        public required FxCue Cue;
        public int Dummy;
        public bool Follows;
        public bool IgnoreAngle;
        public bool Spawned;
        public Matrix4x4 World = Matrix4x4.Identity;
    }

    /// <summary>
    /// Whether an event spawns an effect, as the Time Act Editor tells: a parameter the meta tags with the Particles
    /// alias, or an event type named FFX
    /// </summary>
    private bool IsEffectEvent(TAE.Event ev)
    {
        if (ev.Parameters == null)
            return false;
        var data = Project.Handler.TimeActData;
        if (data?.GetSimulationRole(ev) is TimeActSimulationRole.Bullet or TimeActSimulationRole.Attack
            or TimeActSimulationRole.Throw or TimeActSimulationRole.CastMagic)
            return false;
        return data?.HasAliasField(ev, ProjectAliasType.Particles) == true ||
               (ev.TypeName ?? "").Contains("FFX", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The effect an event spawns: the parameter tagged with the Particles alias, else FFXID, else the first FFXID one.
    /// A floor-determined effect is an offset into the floor material's effects. Null if it has none above 0.
    /// </summary>
    private int? GetEffectId(TAE.Event ev)
    {
        var values = ev.Parameters!.Values;
        var data = Project.Handler.TimeActData;
        var name = data?.GetAliasParameter(ev, ProjectAliasType.Particles);
        if (name == null && values.ContainsKey("FFXID"))
            name = "FFXID";
        name ??= values.Keys.FirstOrDefault(k => k.StartsWith("FFXID", StringComparison.OrdinalIgnoreCase) &&
                                                !k.Contains("Deprecated", StringComparison.OrdinalIgnoreCase));
        if (name == null || !values.TryGetValue(name, out var value) || value is not IConvertible number)
            return null;

        try
        {
            var id = number.ToInt32(null);
            if (data?.GetSimulationRole(ev) == TimeActSimulationRole.FloorEffect)
                return TimeActFloorEffects.Resolve(Project, id, CFG.Current.TimeActEditor_Floor_Material);
            return id > 0 ? id : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>The effect cue of an event of the play, placed on the scene clock, or null if it spawns none</summary>
    private (FxCue Cue, LiveAnchor Anchor)? BuildEffectCue(LuaTimeActPlay play, TAE.Event ev, int index)
    {
        if (!IsEffectEvent(ev) || GetEffectId(ev) is not { } effectId)
            return null;

        var oneShot = (ev.TypeName ?? "").Contains("OneShot", StringComparison.OrdinalIgnoreCase);
        var dummy = GetInt(ev, "DummyPolyID") ?? GetInt(ev, "DummyPolyBladeBaseID") ?? -1;
        play.EffectEvents[ev] = (effectId, dummy, oneShot);

        // A lasting effect is told to stop at its event's end; one that lasts until the clip ends, when the play does
        var start = play.Base + ev.StartTime;
        float? end = oneShot || ev.EndTime >= TAE.Event.EldenRingInfiniteLengthEventPlaceholder
            ? null
            : play.Base + Math.Max(ev.EndTime, ev.StartTime + 1f / 30f);

        var anchor = new LiveAnchor
        {
            Character = play.Character,
            Cue = null!,
            Dummy = dummy,
            // Only one-shots say whether they follow their dummy; the rest are held on it while they last
            Follows = GetBool(ev, "IsFollowDummyPoly") ?? true,
            IgnoreAngle = GetBool(ev, "IsIgnoreDummyPolyAngle") == true,
        };
        var cue = new FxCue
        {
            EffectId = effectId,
            Start = start,
            End = end,
            WorldAt = _ => anchor.World,
            Tag = ev,
            Seed = (uint)HashCode.Combine(effectId, index, ev.StartTime) | 1,
        };
        anchor.Cue = cue;

        play.PendingCues.Add(cue);
        if (!oneShot)
            play.LastingCues.Add(cue);
        return (cue, anchor);
    }

    /// <summary>Moves each effect onto its dummy poly in this frame's pose (or keeps it where it spawned)</summary>
    private void UpdateAnchors()
    {
        Anchors.RemoveAll(a => a.Cue.End is { } end && SceneTime > end + Effects.MaxLinger ||
                               a.Cue.End == null && SceneTime - a.Cue.Start > Effects.MaxLinger * 3);

        foreach (var a in Anchors)
        {
            if (SceneTime < a.Cue.Start || a.Spawned && !a.Follows)
                continue;
            // The character has gone: the effect stays where it was
            if (Viewer.Self != a.Character)
            {
                a.Spawned = true;
                continue;
            }
            var world = a.Character.DummyWorld(a.Dummy) ?? Matrix4x4.CreateTranslation(a.Character.BodyCenter());
            a.World = a.IgnoreAngle ? Matrix4x4.CreateTranslation(world.Translation) : world;
            a.Spawned = true;
        }
    }

    private void UpdateEffects()
    {
        if (!CFG.Current.LuaEditor_TimeAct_Effects || !EffectsAvailable)
        {
            if (Effects.CueCount > 0)
            {
                Effects.Clear();
                Display?.Clear();
            }
            return;
        }

        Effects.Prune(SceneTime);

        // A kind the Viewport Filters turned off goes at once (what fires later isn't cued while it is off)
        if (!CFG.Current.LuaEditor_Display_Tae_Effects)
            Effects.RemoveCues(c => c.Tag is TAE.Event);
        if (!CFG.Current.LuaEditor_Display_Bullet_Effects)
            Effects.RemoveCues(c => c.Tag is BulletEffectTag { Hit: false });
        if (!CFG.Current.LuaEditor_Display_Hit_Effects)
            Effects.RemoveCues(c => c.Tag is BulletEffectTag { Hit: true });

        // The bank is read only once something has an effect to show from it (indexing reads every sfx binder)
        if (Effects.CueCount > 0 && Project.Handler.FxData?.Bank is { } bank)
        {
            bank.StartIndexing();
            bank.Update();
        }

        Display ??= new FxDisplay(new FxDisplayHost(Project, () => Viewer.Universe.RenderScene,
            () => FxDisplayHost.PipelineOf(Viewer.Viewport)));

        var (eye, forward) = Camera();
        Effects.Prepare(Display);
        Effects.Update(SceneTime, eye);
        Display.Update(Effects.Instances, eye, forward);
    }

    private (Vector3 Eye, Vector3 Forward) Camera()
    {
        if (Viewer.Viewport is not StudioCore.Editors.Viewport.VulkanViewport vk)
            return (Vector3.Zero, Vector3.UnitZ);
        var transform = vk.ViewportCamera.CameraTransform;
        return (transform.Position, Vector3.Transform(Vector3.UnitZ, transform.RotationMatrix));
    }

    // --- bullets ---

    /// <summary>A bullet event that fired: from where, what it flew, and when</summary>
    private sealed class Bullet
    {
        public float FiredAt;
        /// <summary>From the simulation's space (bullets fly along +Z from above the origin) to the world</summary>
        public Matrix4x4 Anchor;
        public required SimulationSettings Settings;
        public required SimulationResult Result;
    }

    private void FireBullet(LuaTimeActPlay play, TAE.Event ev, string name)
    {
        if (!CFG.Current.LuaEditor_TimeAct_Bullets)
            return;

        if (!ResolveBullet(play, ev, out var behavior, out var bulletRow, out var failure))
        {
            AddLog(play, ev, LuaTimeActLogKind.Problem, name, failure);
            return;
        }

        var self = play.Character;
        var dummy = GetInt(ev, "DummyPolyID") ?? -1;
        var dummyWorld = self.DummyWorld(dummy);
        var from = dummyWorld?.Translation ?? self.BodyCenter();
        var floor = self.Position.Y;

        // The simulation flies along +Z: turned the way the character faces, at the dummy poly over the floor
        var forward = self.WorldForward;
        var yaw = MathF.Atan2(forward.X, forward.Z);
        var anchor = Matrix4x4.CreateRotationY(yaw) * Matrix4x4.CreateTranslation(from.X, floor, from.Z);
        Matrix4x4.Invert(anchor, out var toSimulation);

        var target = TargetCenter();
        var end = Math.Min(ev.MemeEndTime, Math.Max(play.Duration, ev.StartTime));
        var settings = ProjectileSimulator.CurrentSettings with
        {
            Orientation = CastOrientation.Forward,
            LaunchHeight = Math.Max(from.Y - floor, 0),
            SimulateWall = false,
            SimulateTarget = true,
            TargetPosition = Vector3.Transform(target, toSimulation),
            Motion = TargetMotion.Still,
            EmitterDuration = Math.Max(end - ev.StartTime, 0),
        };

        var bulletParam = ProjectileParams.GetSpellBulletParam(Project)!;
        SimulationResult result;
        try
        {
            result = ProjectileSimulation.Run(new[] { bulletRow!.ID }, id =>
                bulletParam[id] is { } row ? BulletDefinition.FromRow(row, FindAttack) : null!, settings);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("LUA_TAE_Bullet_Failed", bulletRow!.ID), e);
            AddLog(play, ev, LuaTimeActLogKind.Problem, name, LOC.Get("LUA_TAE_Bullet_Failed", bulletRow.ID));
            return;
        }

        var bullet = new Bullet { FiredAt = SceneTime, Anchor = anchor, Settings = settings, Result = result };
        Bullets.Add(bullet);
        if (CFG.Current.LuaEditor_TimeAct_Effects && EffectsAvailable)
            Effects.AddCues(BulletCues(bullet, bulletParam));

        var where = dummy >= 0 && dummyWorld == null ? LOC.Get("LUA_TAE_Dummy_Missing", dummy)
            : dummy >= 0 ? LOC.Get("LUA_TAE_On_Dummy", dummy) : LOC.Get("LUA_TAE_On_Body");
        var label = string.IsNullOrEmpty(bulletRow.Name) ? $"{bulletRow.ID}" : $"{bulletRow.ID} {bulletRow.Name}";
        AddLog(play, ev, LuaTimeActLogKind.Bullet, name,
            LOC.Get("LUA_TAE_Bullet", behavior!.ID, label, where, result.Tracks.Count));
    }

    /// <summary>
    /// The Bullet row a bullet event fires: the event's BehaviorParam row (the character's NpcParam variations + the
    /// judge ID, or the judge ID as it is for a common one), whose refType is a bullet, and its refId
    /// </summary>
    private bool ResolveBullet(LuaTimeActPlay play, TAE.Event ev, out Param.Row? behavior, out Param.Row? bullet,
        out string failure)
    {
        behavior = null;
        bullet = null;
        failure = "";

        if (ProjectileParams.GetParams(Project) == null)
        {
            failure = LOC.Get("LUA_TAE_Bullet_No_Params");
            return false;
        }
        if (GetInt(ev, "BehaviorJudgeID") is not { } judge)
        {
            failure = LOC.Get("LUA_TAE_Bullet_No_Judge");
            return false;
        }

        var common = Project.Handler.TimeActData?.GetFieldMeta(ev, "BehaviorJudgeID")?.BehaviorRef == "Common";
        var rows = Resolver.GetBehaviorRows(ev, play.Character.Name, null, judge, common);

        // Running as an NpcParam row: its own variation's row comes first (the others stay as fallbacks)
        if (!common && Viewer.SelfNpc is { BehaviorVariation: { } variation } npc && npc.Character == play.Character.Name &&
            CharacterComposition.GetRow(Project, "BehaviorParam", NpcBehaviorBase + variation * BehaviorVariationScale + judge) is { } own)
        {
            rows.Remove(own);
            rows.Insert(0, own);
        }
        if (rows.Count == 0)
        {
            failure = LOC.Get("LUA_TAE_Bullet_No_Behavior", judge, play.Character.Name);
            return false;
        }

        behavior = rows.FirstOrDefault(r => CharacterComposition.GetValue(r, "refType") == BehaviorRefBullet);
        if (behavior == null)
        {
            failure = LOC.Get("LUA_TAE_Bullet_Not_Bullet", rows[0].ID);
            return false;
        }

        var bulletId = CharacterComposition.GetValue(behavior, "refId");
        if (ProjectileParams.GetSpellBulletParam(Project)?[bulletId] is not { } row)
        {
            failure = LOC.Get("LUA_TAE_Bullet_No_Row", behavior.ID, bulletId);
            return false;
        }

        bullet = row;
        return true;
    }

    /// <summary>An enemy's bullets hit with its own attacks first, then the player's</summary>
    private AttackDefinition FindAttack(int id)
    {
        var row = CharacterComposition.GetRow(Project, ProjectileParams.EnemyAttackParam, id) ??
                  ProjectileParams.FindAttackRow(Project, id, out _);
        return row != null ? AttackDefinition.FromRow(row) : null!;
    }

    /// <summary>What the bullets aim at: the target character's middle, else the unseen point a metre up</summary>
    private Vector3 TargetCenter()
    {
        if (Viewer.Target is { } target)
            return target.BodyCenter();
        return Viewer.TargetPoint() + Vector3.UnitY;
    }

    /// <summary>What a bullet's effect cue is, for the Viewport Filters: a bullet's flight effect, or its hit effect</summary>
    private sealed record BulletEffectTag(ProjectileTrack Track, bool Hit);

    /// <summary>Each bullet's flight effect along its flight, and its hit effect where it landed (as the filters allow)</summary>
    private static List<FxCue> BulletCues(Bullet bullet, Param bulletParam)
    {
        var showFlight = CFG.Current.LuaEditor_Display_Bullet_Effects;
        var showHit = CFG.Current.LuaEditor_Display_Hit_Effects;
        var cues = new List<FxCue>();
        var start = bullet.FiredAt;
        var anchor = bullet.Anchor;

        for (var i = 0; i < bullet.Result.Tracks.Count; i++)
        {
            var track = bullet.Result.Tracks[i];
            if (bulletParam[track.RowId] is not { } row)
                continue;

            var flight = ProjectileParams.GetFlightEffect(row);
            if (flight > 0 && showFlight)
            {
                cues.Add(new FxCue
                {
                    EffectId = flight,
                    Start = start + track.SpawnTime,
                    End = start + Math.Max(track.EndTime, track.SpawnTime + MinFlightLength),
                    WorldAt = t => ProjectileEffects.FlightWorld(track, t - start) * anchor,
                    Tag = new BulletEffectTag(track, false),
                    Seed = (uint)HashCode.Combine(flight, i, start) | 1,
                });
            }

            var hit = ProjectileParams.GetHitEffect(row);
            if (hit > 0 && showHit && track.End is ProjectileEnd.HitGround or ProjectileEnd.HitWall or ProjectileEnd.HitTarget)
            {
                var landed = ProjectileEffects.FlightWorld(track, track.EndTime) * anchor;
                cues.Add(new FxCue
                {
                    EffectId = hit,
                    Start = start + track.EndTime,
                    End = start + track.EndTime + HitEffectLength,
                    WorldAt = _ => landed,
                    Tag = new BulletEffectTag(track, true),
                    Seed = (uint)HashCode.Combine(hit, i, start, 1) | 1,
                });
            }
        }
        return cues;
    }

    /// <summary>The bullets in flight now, as the Projectile Editor draws them, moved to where each fired</summary>
    private void DrawBullets()
    {
        // A bullet is kept a little after its flight, for its impact marker
        Bullets.RemoveAll(b => SceneTime - b.FiredAt > b.Result.Duration + 1.0f);

        if (!CFG.Current.LuaEditor_TimeAct_Bullets || Bullets.Count == 0)
        {
            HideLines();
            return;
        }

        if (Lines.IsUpdating || DateTime.UtcNow - LastBuild < BuildInterval)
            return;
        LastBuild = DateTime.UtcNow;

        var world = new Dictionary<Color, List<(Vector3, Vector3)>>();
        foreach (var bullet in Bullets)
        {
            var local = new Dictionary<Color, List<(Vector3, Vector3)>>();
            var flightTime = SceneTime - bullet.FiredAt;

            if (CFG.Current.LuaEditor_Display_Trails)
                ProjectileViewportWindow.AddTrails(local, bullet.Result, flightTime);
            ProjectileViewportWindow.AddProjectiles(local, bullet.Result, flightTime,
                CFG.Current.LuaEditor_Display_Bullets, CFG.Current.LuaEditor_Display_Reach);
            if (CFG.Current.LuaEditor_Display_Impacts)
                ProjectileViewportWindow.AddImpacts(local, bullet.Result, flightTime);

            foreach (var (color, list) in local)
            {
                var into = ProjectileViewportWindow.Lines(world, color);
                foreach (var (a, b) in list)
                    into.Add((Vector3.Transform(a, bullet.Anchor), Vector3.Transform(b, bullet.Anchor)));
            }
        }

        if (world.Count == 0 && !LinesShown)
            return;
        Lines.Show(Viewer.Universe.RenderScene, world);
        LinesShown = world.Count > 0;
    }

    private void HideLines()
    {
        if (!LinesShown)
            return;
        Lines.Clear();
        LinesShown = false;
    }

    public void Dispose()
    {
        Reset();
        Display?.Dispose();
        Display = null;
    }
}

/// <summary>One play of a clip: the TAE animation it has, when it started on the scene clock, and what has fired</summary>
public sealed class LuaTimeActPlay
{
    public LuaCharacter Character { get; }
    public long AnimationId { get; }

    /// <summary>The scene time of the clip's time 0</summary>
    public float Base { get; }

    /// <summary>How long the clip is</summary>
    public float Duration { get; }

    public float LastTime;

    public TimeActContainerWrapper? Source;
    public TimeActEntry? Entry;
    public TAE.Animation? Animation;

    /// <summary>The animation whose events this one imports, if it does</summary>
    public long? ImportedFrom;

    /// <summary>Why there are no events, when there are none to play</summary>
    public string? Why;

    public List<TAE.Event> Events = new();
    public HashSet<TAE.Event> Fired { get; } = new();

    /// <summary>How long the clip blends in from the one before (its TAE Blend event), or null when it has none</summary>
    public float? BlendDuration;

    /// <summary>The clip's turn speed events (SetTurnSpeed), with their TurnSpeed in degrees a second</summary>
    public List<(TAE.Event Event, float Speed)> TurnEvents { get; } = new();

    /// <summary>The clip's AddSpEffect events, with the SpEffect each puts on</summary>
    public List<(TAE.Event Event, int Id)> SpEffectEvents { get; } = new();

    /// <summary>The clip's mask events (DrawMask, HideMask, ShowMask)</summary>
    public List<(TAE.Event Event, TimeActSimulationRole Role)> MaskEvents { get; } = new();

    private const int MaskCount = 32;

    /// <summary>
    /// What the mask events say at a time of the clip, by mask group: shown, hidden, or null where they say nothing.
    /// A DrawMask stays from its start (Mask{n} 0 hides, 1 shows, others keep); a HideMask or ShowMask holds while
    /// active (Mask{n} set). Null when the clip has none.
    /// </summary>
    public bool?[]? MaskOverridesAt(float time)
    {
        if (MaskEvents.Count == 0)
            return null;

        var overrides = new bool?[MaskCount];
        foreach (var (ev, role) in MaskEvents)
        {
            if (time < ev.StartTime)
                continue;
            var active = time < ev.MemeEndTime;
            if (role != TimeActSimulationRole.DrawMask && !active)
                continue;

            for (var i = 0; i < MaskCount; i++)
            {
                if (TimeActSimulation.GetFloat(ev, $"Mask{i}") is not { } value)
                    break;
                var v = (int)value;
                bool? shown = role switch
                {
                    TimeActSimulationRole.DrawMask => v switch { 0 => false, 1 => true, _ => null },
                    TimeActSimulationRole.HideMask => v != 0 ? false : null,
                    _ => v != 0 ? true : null,
                };
                if (shown is { } s)
                    overrides[i] = s;
            }
        }
        return overrides;
    }

    /// <summary>The turn speed at a time of the clip: the last of the turn events active then, or null if none is</summary>
    public float? TurnSpeedAt(float time)
    {
        float? speed = null;
        foreach (var (ev, s) in TurnEvents)
        {
            if (time >= ev.StartTime && time < ev.MemeEndTime)
                speed = s;
        }
        return speed;
    }

    /// <summary>The effect each effect event spawns</summary>
    public Dictionary<TAE.Event, (int EffectId, int Dummy, bool OneShot)> EffectEvents { get; } = new();

    /// <summary>The play's effect cues; those that aren't one-shots stop when the play ends</summary>
    public List<FxCue> PendingCues { get; } = new();
    public List<FxCue> LastingCues { get; } = new();

    public LuaTimeActPlay(LuaCharacter character, long animationId, float playBase, float duration)
    {
        Character = character;
        AnimationId = animationId;
        Base = playBase;
        Duration = duration;
    }
}
