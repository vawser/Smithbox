#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.FxEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Plays the effects of the map elements that place one in the view's viewports, drawn by the shared FX display
/// (<see cref="FxDisplay"/>, as the FX, Time Act and Cutscene editors draw theirs):
/// <list type="bullet">
/// <item>SFX and WindSFX regions, and the sign effect of Message regions, on the region.</item>
/// <item>Assets (objects in DS3 and Sekiro) through their model's SFX param row: AssetModelSfxParam (ModelSfxParam in
/// DS3 and Sekiro), row = the model's number * 1000 (* 10 in DS3 and Sekiro) + the part's relative id. Each of the row's eight slots places an
/// effect on a dummy poly of the model (its origin when the dummy isn't found). A relative id of -1 places none.</item>
/// </list>
/// <list type="bullet">
/// <item>Each effect follows its element live, so moving the element moves the effect. Region shape scale is left
/// out.</item>
/// <item>Map effects are ambient: one that ends is started again, so it keeps playing as it would in the game.</item>
/// <item>Only the nearest effects to the camera run (within the distance and up to the count set in Viewport
/// Filters), as a map can place hundreds. An effect that comes into range starts a little way in, so it doesn't
/// appear from nothing.</item>
/// <item>The effects come from the project's FX bank (with any unsaved FX Editor edits), so the FX Editor has to be
/// enabled for the project. The bank is indexed in the background the first time there is something to show.</item>
/// </list>
/// </summary>
public sealed class MapEffectPlayback : IDisposable
{
    private sealed class Source
    {
        public Entity Entity = null!;

        /// <summary>Which effect of the element: 0 for a region, the param slot for an asset</summary>
        public int Slot;

        public int EffectId;
        public float DistanceSq;
        public Func<Matrix4x4> World = null!;
    }

    private sealed class Running
    {
        public int EffectId;
        public FxEffectDef? Def;
        public FxEffectInstance? Instance;
        public FxRuntimeContext? Context;
        public uint Seed;
        public int Restarts;
    }

    /// <summary>One slot of a model SFX param row</summary>
    public readonly record struct ModelSfx(int Slot, int EffectId, int DummyId);

    private readonly MapEditorView _view;
    private readonly ProjectEntry _project;

    private readonly FxDefCache _defs;
    private FxDisplay? _display;

    private readonly Dictionary<(Entity, int), Running> _running = new();
    private readonly List<FxEffectInstance> _instances = new();

    // Every effect element of the view's loaded maps, found again a few times a second (maps load and unload, and
    // edits change effect ids)
    private readonly List<Source> _sources = new();
    private float _scanCountdown;
    private const float ScanInterval = 0.25f;

    // The model SFX param rows by row id, read again now and then, as the params can be edited
    private readonly Dictionary<int, ModelSfx[]> _modelSfx = new();
    private float _paramCountdown;
    private const float ParamInterval = 2.0f;

    // The dummy polys of asset models, read in the background the first time one is in range
    private readonly Dictionary<string, Task<ModelRig?>> _rigs = new(StringComparer.OrdinalIgnoreCase);

    private const float Step = 1f / 30f;

    /// <summary>How far in an effect that comes into range starts, so it shows established rather than just started</summary>
    private const float PreWarm = 1.5f;

    /// <summary>An effect that ends this soon after it starts, this many times in a row, isn't started again</summary>
    private const float ShortLife = 0.1f;
    private const int MaxShortRestarts = 8;

    private const int ModelSfxSlots = 8;

    public MapEffectPlayback(MapEditorView view, ProjectEntry project)
    {
        _view = view;
        _project = project;
        _defs = new FxDefCache(() => project.Handler.FxData);
        _preload = new FxPreload(_defs);
    }

    // An element's effect starts once it is ready (built, textures and models loaded), so it doesn't stall as it shows
    private readonly FxPreload _preload;

    /// <summary>The effects in range, and how many of them are ready to play</summary>
    public int LoadingCount => _preload.Count;
    public int LoadedCount => _preload.ReadyCount;
    public bool IsLoading => !_preload.IsDone;

    /// <summary>Whether time moves on. Paused effects stay on show where they are.</summary>
    public bool IsPlaying { get; private set; } = true;

    /// <summary>Whether playback was stopped: nothing runs or is drawn until it is started again</summary>
    public bool IsStopped { get; private set; }

    /// <summary>The playback speed, 1 being the game's</summary>
    public float Speed = 1.0f;

    /// <summary>Whether the game is one the FX runtime reads</summary>
    public bool IsSupportedGame => EditorConditions.SupportsFxPlayback(_project.Descriptor.ProjectType);

    /// <summary>
    /// Whether asset effects are read for the game. AC6 is left out: the field read as the SFX relative id there
    /// holds the asset's pose animation.
    /// </summary>
    public bool SupportsAssetEffects => SupportsAssetEffectsFor(_project);

    public static bool SupportsAssetEffectsFor(ProjectEntry project) =>
        project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.NR;

    /// <summary>The param asset effects are read from: ModelSfxParam in DS3 and Sekiro, AssetModelSfxParam after</summary>
    public static string ModelSfxParamName(ProjectEntry project) =>
        project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT ? "ModelSfxParam" : "AssetModelSfxParam";

    /// <summary>The fields that hold an asset's SFX relative id</summary>
    public static bool IsRelativeSfxField(string name) =>
        name is "AssetSfxParamRelativeID" or "ModelSfxParamRelativeID" or "ModelSfxParamRelativeIDs";

    /// <summary>The model SFX param row of a model and relative id, null when the relative id places none (-1)</summary>
    public static int? GetModelSfxRowId(ProjectEntry project, string modelName, int relative) =>
        relative >= 0 && GetModelNumber(modelName) is { } number ? number * ModelSfxRowStride(project) + relative : null;

    /// <summary>
    /// What the model's number is multiplied by for its row: 10 in DS3 and Sekiro (o103000 is ModelSfxParam 1030000),
    /// 1000 from ER on (AEG023_862 with relative id 3 is AssetModelSfxParam 23862003)
    /// </summary>
    public static int ModelSfxRowStride(ProjectEntry project) =>
        project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT ? 10 : 1000;

    /// <summary>Whether the project has effects to show (the FX Editor is enabled for it)</summary>
    public bool IsAvailable => _project.Handler.FxData is { Bank: not null, Schema: not null };

    public FxBank? Bank => _project.Handler.FxData?.Bank;

    /// <summary>Effect elements in the loaded maps, effects running now, particles, and running elements whose effect the bank hasn't got</summary>
    public int SourceCount => _sources.Count;
    public int RunningCount => _instances.Count;
    public int ParticleCount { get; private set; }
    public int MissingCount { get; private set; }

    public void Play()
    {
        IsPlaying = true;
        IsStopped = false;
    }

    public void Pause() => IsPlaying = false;

    /// <summary>Drops every running effect: nothing is drawn until playback starts again</summary>
    public void Stop()
    {
        IsPlaying = false;
        IsStopped = true;
        ClearRunning();
    }

    /// <summary>Starts every effect in range again from its start</summary>
    public void Restart()
    {
        ClearRunning();
        Play();
    }

    /// <summary>Finds the effect elements again at the next update, for when one was added, removed or edited</summary>
    public void Invalidate()
    {
        _scanCountdown = 0;
        _paramCountdown = 0;
    }

    public void Update()
    {
        if (!CFG.Current.MapEditor_Display_Effects || !IsSupportedGame || !IsAvailable || !Smithbox.Instance.HasRenderer)
        {
            if (_running.Count > 0 || _instances.Count > 0)
                Clear();

            return;
        }

        if (IsStopped)
            return;

        var dt = ImGui.GetIO().DeltaTime;

        _paramCountdown -= dt;
        if (_paramCountdown <= 0)
        {
            _paramCountdown = ParamInterval;
            _modelSfx.Clear();
        }

        _scanCountdown -= dt;
        if (_scanCountdown <= 0)
        {
            _scanCountdown = ScanInterval;
            Scan();
        }

        // The bank is read only once a map has something to show from it (indexing reads every sfx binder)
        if (_sources.Count > 0 && Bank is { } bank)
        {
            bank.StartIndexing();
            bank.Update();
        }

        var (eye, forward) = GetCamera();

        // Paused effects are only drawn, a stopped one has been dropped
        var advance = IsPlaying ? Math.Clamp(dt, 0, 0.1f) * Speed : 0;

        // Before stepping: the effects' textures and models are loaded through it
        _display ??= CreateDisplay();

        StepEffects(eye, advance);

        _display?.Update(_instances, eye, forward);
    }

    /// <summary>
    /// Every effect element of the maps this view has loaded, by distance from the camera
    /// </summary>
    private void Scan()
    {
        _sources.Clear();

        var cfg = CFG.Current;
        var regions = cfg.MapEditor_Effects_Sfx || cfg.MapEditor_Effects_WindSfx || cfg.MapEditor_Effects_MessageSfx;
        var assets = cfg.MapEditor_Effects_Assets && SupportsAssetEffects;

        if (!regions && !assets)
            return;

        var (eye, _) = GetCamera();

        HashSet<Entity>? selected = null;
        if (cfg.MapEditor_Effects_Selected_Only)
            selected = _view.ViewportSelection.GetSelection().OfType<Entity>().ToHashSet();

        // Assets out of range aren't looked up, as a map has thousands of them
        var maxDistSq = cfg.MapEditor_Effects_Max_Distance * cfg.MapEditor_Effects_Max_Distance;

        foreach (var wrapper in _project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            var map = wrapper.MapContainer;
            if (map == null || map.OwnerView != _view)
                continue;

            if (regions)
            {
                foreach (var region in Snapshot(map.Regions))
                {
                    if (!IsShown(region, selected) || GetRegionEffectId(region) is not { } effectId)
                        continue;

                    _sources.Add(new Source
                    {
                        Entity = region,
                        EffectId = effectId,
                        DistanceSq = Vector3.DistanceSquared(region.GetWorldMatrix().Translation, eye),
                        World = () => WithoutScale(region.GetWorldMatrix())
                    });
                }
            }

            if (assets)
            {
                foreach (var part in Snapshot(map.Parts))
                {
                    if (!IsShown(part, selected) || !EntityHelper.IsPartAsset(part))
                        continue;

                    var distanceSq = Vector3.DistanceSquared(part.GetWorldMatrix().Translation, eye);
                    if (selected == null && distanceSq > maxDistSq)
                        continue;

                    AddAssetSources(part, distanceSq);
                }
            }
        }

        // The slots of one asset share a distance: ties are broken the same way every scan, so the effects at the
        // Max Effects cut-off don't swap in and out (each swap restarting them)
        _sources.Sort((a, b) =>
        {
            var order = a.DistanceSq.CompareTo(b.DistanceSq);
            if (order == 0)
                order = string.CompareOrdinal(a.Entity.Name, b.Entity.Name);
            return order != 0 ? order : a.Slot.CompareTo(b.Slot);
        });
    }

    private static Entity[] Snapshot(List<Entity>? entities)
    {
        if (entities == null)
            return Array.Empty<Entity>();

        try
        {
            // A map can still be loading on another thread
            return entities.ToArray();
        }
        catch (Exception)
        {
            return Array.Empty<Entity>();
        }
    }

    private static bool IsShown(Entity? entity, HashSet<Entity>? selected)
    {
        return entity is { EditorVisible: true, WrappedObject: not null } && (selected == null || selected.Contains(entity));
    }

    /// <summary>
    /// The effect a region places, if it is one of the types shown and has an effect id above 0
    /// </summary>
    private static int? GetRegionEffectId(Entity region)
    {
        var cfg = CFG.Current;
        var obj = region.WrappedObject;

        string? property = obj.GetType().Name switch
        {
            "SFX" when cfg.MapEditor_Effects_Sfx => "EffectID",
            "WindSFX" when cfg.MapEditor_Effects_WindSfx => "EffectID",
            "Message" when cfg.MapEditor_Effects_MessageSfx => "MessageSfxID",
            _ => null
        };

        if (property == null)
            return null;

        return ToInt(obj.GetType().GetProperty(property)?.GetValue(obj)) is { } id && id > 0 ? id : null;
    }

    // ---- Assets ----

    private void AddAssetSources(Entity part, float distanceSq)
    {
        var obj = part.WrappedObject;

        if (obj.GetType().GetProperty("ModelName")?.GetValue(obj) is not string modelName ||
            GetRelativeSfxId(obj) is not { } relative ||
            GetModelSfxRowId(_project, modelName, relative) is not { } rowId)
        {
            return;
        }

        var effects = GetModelSfx(rowId);
        if (effects.Length == 0)
            return;

        // Placed once the model's dummies are read
        if (GetRig(modelName) is not { } rig)
            return;

        foreach (var sfx in effects)
        {
            var dummy = sfx.DummyId >= 0 ? rig.FindDummy(sfx.DummyId) : null;
            var dummyModel = DummyModelMatrix(rig, dummy);


            _sources.Add(new Source
            {
                Entity = part,
                Slot = sfx.Slot,
                EffectId = sfx.EffectId,
                DistanceSq = distanceSq,
                World = () => WithoutScale(dummyModel * part.GetWorldMatrix())
            });
        }
    }

    /// <summary>The part's SFX relative id: AssetSfxParamRelativeID (ER, NR), ModelSfxParamRelativeID (Sekiro), the first of ModelSfxParamRelativeIDs (DS3)</summary>
    private static int? GetRelativeSfxId(object part)
    {
        var type = part.GetType();

        foreach (var name in new[] { "AssetSfxParamRelativeID", "ModelSfxParamRelativeID" })
        {
            if (type.GetProperty(name) is { } property)
                return ToInt(property.GetValue(part));
        }

        if (type.GetProperty("ModelSfxParamRelativeIDs")?.GetValue(part) is short[] { Length: > 0 } ids)
            return ids[0];

        return null;
    }

    /// <summary>The number of a model name: AEG099_001 is 99001, o103000 is 103000</summary>
    public static int? GetModelNumber(string modelName)
    {
        var digits = new string(modelName.Where(char.IsAsciiDigit).ToArray());
        return digits.Length is > 0 and < 10 && int.TryParse(digits, out var number) ? number : null;
    }

    /// <summary>The effects of a model SFX param row, empty if there is no such row</summary>
    private ModelSfx[] GetModelSfx(int rowId)
    {
        if (_modelSfx.TryGetValue(rowId, out var cached))
            return cached;

        var effects = ReadModelSfx(_project, rowId) ?? Array.Empty<ModelSfx>();
        _modelSfx[rowId] = effects;
        return effects;
    }

    /// <summary>The effects of a model SFX param row: null if there is no such row, empty if it places none</summary>
    public static ModelSfx[]? ReadModelSfx(ProjectEntry project, int rowId)
    {
        if (CharacterComposition.GetRow(project, ModelSfxParamName(project), rowId) is not { } row)
            return null;

        var result = new List<ModelSfx>();
        for (var i = 0; i < ModelSfxSlots; i++)
        {
            var sfx = GetCell(row, $"sfxId_{i}");
            if (sfx is > 0)
                result.Add(new ModelSfx(i, sfx.Value, GetCell(row, $"dmypolyId_{i}") ?? -1));
        }

        return result.ToArray();
    }

    private static int? GetCell(Param.Row row, string field)
    {
        foreach (var cell in row.Cells)
        {
            if (cell.Def.InternalName.Equals(field, StringComparison.OrdinalIgnoreCase))
                return ToInt(cell.Value);
        }

        return null;
    }

    /// <summary>The model's rig (for its dummy polys), or null until it has been read or if it can't be</summary>
    private ModelRig? GetRig(string modelName)
    {
        if (!_rigs.TryGetValue(modelName, out var load))
        {
            load = Task.Run(() => ReadRig(modelName));
            _rigs[modelName] = load;
        }

        return load.IsCompletedSuccessfully ? load.Result : null;
    }

    private ModelRig? ReadRig(string modelName)
    {
        try
        {
            var asset = ModelLocator.GetObjModel(_project, modelName, modelName);
            if (asset.AssetVirtualPath == null)
                return null;

            var path = PathBuilder.GetRelativePath(_project, asset.AssetVirtualPath);
            var data = _project.VFS.FS.ReadFile(path);

            return data is { } container ? ModelRig.Read(_project.Descriptor.ProjectType, container, modelName) : null;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[Map Editor] Couldn't read the dummy polys of {modelName} for its effects: {e.Message}", e);
            return null;
        }
    }

    /// <summary>Where a dummy is in its model, in the reference pose: the origin when there is no dummy</summary>
    private static Matrix4x4 DummyModelMatrix(ModelRig rig, RigDummy? dummy)
    {
        if (dummy == null)
            return Matrix4x4.Identity;

        return dummy.Node >= 0 && dummy.Node < rig.NodeReferenceWorld.Length
            ? dummy.Offset * rig.NodeReferenceWorld[dummy.Node]
            : dummy.Offset;
    }

    // ---- Playback ----

    private void StepEffects(Vector3 eye, float advance)
    {
        var cfg = CFG.Current;
        var maxDistSq = cfg.MapEditor_Effects_Max_Distance * cfg.MapEditor_Effects_Max_Distance;
        var maxCount = Math.Max(0, cfg.MapEditor_Effects_Max_Count);

        // The nearest elements in range run; the rest are dropped
        var wanted = new Dictionary<(Entity, int), Source>();
        foreach (var source in _sources)
        {
            if (wanted.Count >= maxCount)
                break;

            if (source.DistanceSq > maxDistSq && !cfg.MapEditor_Effects_Selected_Only)
                break;

            wanted.TryAdd((source.Entity, source.Slot), source);
        }

        foreach (var gone in _running.Keys.Where(k => !wanted.ContainsKey(k)).ToList())
            _running.Remove(gone);

        _preload.Set(wanted.Values.Select(s => s.EffectId));
        _preload.Update(_display);

        _instances.Clear();
        ParticleCount = 0;
        MissingCount = 0;

        foreach (var (key, source) in wanted)
        {
            // Not started until it's ready: building it here would stall the frame it first shows in
            if ((!_running.TryGetValue(key, out var started) || started.EffectId != source.EffectId)
                && !_preload.IsReady(source.EffectId))
                continue;

            // The definition (again, if the FX Editor changed the effect)
            var def = _defs.Get(source.EffectId);
            if (def == null)
            {
                if (_defs.Has(source.EffectId) == false)
                    MissingCount++;

                _running.Remove(key);
                continue;
            }

            if (!_running.TryGetValue(key, out var running) || running.EffectId != source.EffectId)
            {
                running = new Running
                {
                    EffectId = source.EffectId,
                    // Two elements of one effect don't show the same particles
                    Seed = (uint)HashCode.Combine(source.EffectId, source.Entity.Name, source.Slot) | 1,
                };
                _running[key] = running;
            }

            if (running.Def != def)
            {
                running.Def = def;
                running.Instance = null;
            }

            // Before stepping: LevelsOfDetail nodes pick their child by the camera's distance as they update
            if (running.Context != null)
                running.Context.CameraPosition = eye;

            try
            {
                if (running.Instance == null)
                {
                    // Pre-warmed only while playing: a paused view shows nothing new moving
                    Start(source, running, IsPlaying ? PreWarm : 0, eye);
                }
                else if (advance > 0)
                {
                    running.Instance.SimulateTo(running.Instance.Time + advance, Step);
                }

                // Ambient effects keep playing: one that has ended is started again
                if (running.Instance is { Finished: true } finished && IsPlaying)
                {
                    running.Restarts = finished.Time < ShortLife ? running.Restarts + 1 : 0;
                    running.Instance = null;

                    if (running.Restarts <= MaxShortRestarts)
                        Start(source, running, 0, eye);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Editor] Effect {source.EffectId} on {source.Entity.Name} stopped: {e.Message}", e);
                running.Instance = null;
                running.Restarts = MaxShortRestarts + 1;
                continue;
            }

            if (running.Instance == null || running.Instance.Finished || running.Context == null)
                continue;

            running.Context.CameraPosition = eye;
            _instances.Add(running.Instance);
            ParticleCount += running.Context.ParticleCount;
        }
    }

    private void Start(Source source, Running running, float preWarm, Vector3 eye)
    {
        if (running.Def == null || running.Restarts > MaxShortRestarts)
            return;

        // Read live, so the effect follows its element as it is moved
        var world = source.World;

        running.Context = new FxRuntimeContext
        {
            ResolveEffect = _defs.Get,
            Motion = _ => world(),
            // Set before the pre-warm, or its detail levels would be picked as seen from the world origin
            CameraPosition = eye,
        };

        running.Instance = new FxEffectInstance(running.Def, running.Context, running.Seed);

        if (preWarm > 0)
            running.Instance.SimulateTo(preWarm, 1f / 15f);

    }

    /// <summary>
    /// A place and facing without the scale of a region's shape or an asset's size
    /// </summary>
    private static Matrix4x4 WithoutScale(Matrix4x4 world)
    {
        if (Matrix4x4.Decompose(world, out _, out var rotation, out var translation))
            return Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(translation);

        return Matrix4x4.CreateTranslation(world.Translation);
    }

    private static int? ToInt(object? value)
    {
        if (value is not IConvertible number)
            return null;

        try
        {
            return number.ToInt32(null);
        }
        catch
        {
            return null;
        }
    }

    private FxDisplay? CreateDisplay()
    {
        var scene = _view.ViewportHandler.RenderScene;
        if (scene == null)
            return null;

        return new FxDisplay(new FxDisplayHost(_project, () => _view.ViewportHandler.RenderScene,
            () => FxDisplayHost.PipelineOf(_view.ViewportHandler.ActiveViewport?.Viewport)));
    }

    private (Vector3 Eye, Vector3 Forward) GetCamera()
    {
        if (_view.ViewportHandler.ActiveViewport?.Viewport is not VulkanViewport vk)
            return (Vector3.Zero, Vector3.UnitZ);

        var transform = vk.ViewportCamera.CameraTransform;
        return (transform.Position, Vector3.Transform(Vector3.UnitZ, transform.RotationMatrix));
    }

    private void ClearRunning()
    {
        _running.Clear();
        _instances.Clear();
        ParticleCount = 0;
        _display?.Clear();
    }

    /// <summary>Drops what is drawn (effects turned off, or the view closing)</summary>
    public void Clear()
    {
        ClearRunning();
        _sources.Clear();
        _modelSfx.Clear();
        _scanCountdown = 0;
    }

    public void Dispose()
    {
        Clear();
        _rigs.Clear();
        _display?.Dispose();
        _display = null;
    }
}
