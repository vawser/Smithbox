#nullable enable
using Havok;
using Havok.Files;
using SoulsFormats;
using StudioCore.Editors.HavokEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The clips the character's behavior graph plays alongside the selected animation (the always-on layers, such as the
/// Nightreign heroes' idle offsets, eye blinks and bone-masked arm and face poses), worked out by
/// <see cref="HavokBehaviorPoseGraph"/> from the character's behbnd and laid on the clip by <see cref="TimeActPlayback"/>.
/// The behbnd is read once per character, in the background. Variable values can be overridden for the session.
/// </summary>
public class TimeActBehaviorLayers
{
    private readonly ProjectEntry _project;

    private string? _model;
    private Task<List<HavokBehaviorPoseGraph>>? _loading;
    private List<HavokBehaviorPoseGraph>? _graphs;

    private string? _planAnimation;
    private int _planVersion = -1;

    // Bumped by each override change, so the plan is worked out again
    private int _version;

    private readonly Dictionary<string, float> _overrides = new(StringComparer.OrdinalIgnoreCase);

    // The package's clips by animation name, for the package they came from
    private AnimationPackage? _namePackage;
    private Dictionary<string, AnimationEntry>? _entriesByName;

    public TimeActBehaviorLayers(ProjectEntry project)
    {
        _project = project;
    }

    /// <summary>The character's behavior files are still being read</summary>
    public bool IsLoading => _loading != null;

    /// <summary>The behavior file the plan comes from, or null</summary>
    public string? SourceFile { get; private set; }

    /// <summary>The layers for the selected animation, or null if its behavior graph doesn't play it</summary>
    public HavokBehaviorLayerPlan? Plan { get; private set; }

    /// <summary>The variable values set in place of the file's initial values</summary>
    public IReadOnlyDictionary<string, float> Overrides => _overrides;

    public void SetOverride(string name, float value)
    {
        _overrides[name] = value;
        _version++;
    }

    public void ClearOverrides()
    {
        _overrides.Clear();
        _version++;
    }

    /// <summary>
    /// Follows the model and animation on show: starts reading the model's behbnd when the model changes, and works out
    /// the plan again when the animation or an override changes.
    /// </summary>
    /// <param name="model">The model's name, e.g. c0000.</param>
    /// <param name="animationName">The playing clip's animation, e.g. a000_000000, or null.</param>
    public void Update(string? model, string? animationName)
    {
        if (!string.Equals(model, _model, StringComparison.OrdinalIgnoreCase))
        {
            _model = model;
            _graphs = null;
            _planAnimation = null;
            Plan = null;
            SourceFile = null;

            var path = model != null ? $"/chr/{model.ToLowerInvariant()}.behbnd.dcx" : null;
            _loading = path != null ? Task.Run(() => ReadGraphs(path, model!)) : null;
        }

        if (_loading is { IsCompleted: true } done)
        {
            _graphs = done.IsCompletedSuccessfully ? done.Result : new();
            _loading = null;
            _planVersion = -1;

            if (done.IsFaulted)
                Smithbox.LogError(this, LOC.Get("TAE_BehaviorLayers_Read_Failed", _model ?? ""), done.Exception);
        }

        if (_graphs == null)
            return;

        if (animationName == _planAnimation && _planVersion == _version)
            return;

        _planAnimation = animationName;
        _planVersion = _version;
        Plan = null;
        SourceFile = null;

        if (animationName == null)
            return;

        foreach (var graph in _graphs)
        {
            if (graph.Build(animationName, _overrides) is { } plan)
            {
                Plan = plan;
                SourceFile = graph.FileName;
                break;
            }
        }
    }

    /// <summary>
    /// The plan's layers as the animator takes them, at a time of the playback: looping clips loop on it, the others play
    /// once and hold. Clips the package doesn't have are left out.
    /// </summary>
    public void AddLayers(AnimationPackage package, float time,
        List<(HavokClip Clip, float Time, float Weight, bool Additive, float[]? BoneWeights, bool Underlay)> into)
    {
        if (Plan == null)
            return;

        foreach (var layer in Plan.Layers)
        {
            if (GetClip(package, layer.AnimationName) is not { Duration: > 0 } clip)
                continue;

            var additive = clip.BlendMode != HavokBlendMode.Normal;
            var weight = layer.EffectiveWeight(additive);
            if (weight <= 0)
                continue;

            var at = layer.Looping ? time % clip.Duration : Math.Min(time, clip.Duration);
            into.Add((clip, at, weight, additive, layer.BoneWeights, layer.Underlay));
        }
    }

    /// <summary>Whether the package has a clip of this animation (the layer will play)</summary>
    public bool HasClip(AnimationPackage? package, string animationName) =>
        package != null && FindEntry(package, animationName) != null;

    private HavokClip? GetClip(AnimationPackage package, string animationName)
    {
        return FindEntry(package, animationName) is { } entry ? package.GetClip(entry) : null;
    }

    private AnimationEntry? FindEntry(AnimationPackage package, string animationName)
    {
        if (package != _namePackage || _entriesByName == null)
        {
            _namePackage = package;
            _entriesByName = new(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in package.Animations)
                _entriesByName.TryAdd(StemOf(entry.Name), entry);
        }

        return _entriesByName.GetValueOrDefault(animationName);
    }

    /// <summary>An animation's name without its folder and extensions: a000_000000 of …/a000_000000.hkx</summary>
    public static string StemOf(string name)
    {
        return Path.GetFileName(name.Replace('\\', '/')).Split('.')[0];
    }

    // The behavior files of the behbnd, the model's own first (enemies keep most clips in the shared c9997.hkx)
    private List<HavokBehaviorPoseGraph> ReadGraphs(string path, string model)
    {
        var result = new List<HavokBehaviorPoseGraph>();
        var profile = HavokTypeUtils.GetProfile(_project);

        if (profile == null || _project.VFS.FS.ReadFile(path) is not { } raw)
            return result;

        var data = DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw.ToArray();
        if (!BND4.Is(data))
            return result;

        using var binder = BND4.Read(data);

        HavokCompendium? compendium = null;
        var compendiumFile = binder.Files.FirstOrDefault(f => f.Name.Contains(".compendium", StringComparison.OrdinalIgnoreCase));
        if (compendiumFile != null)
        {
            var bytes = DCX.Is(compendiumFile.Bytes) ? DCX.Decompress(compendiumFile.Bytes).ToArray() : compendiumFile.Bytes.ToArray();
            compendium = HavokFile.ReadCompendium(bytes, profile);
        }

        var files = binder.Files
            .Where(f => HavokBehaviorScan.IsBehaviorGraphPath(f.Name))
            .OrderBy(f => StemOf(f.Name).Equals(model, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ToList();

        foreach (var file in files)
        {
            try
            {
                var bytes = DCX.Is(file.Bytes) ? DCX.Decompress(file.Bytes).ToArray() : file.Bytes.ToArray();
                var document = HavokFile.Read(bytes, profile, HavokFile.Sniff(bytes).UsesCompendium ? compendium : null);
                result.Add(new HavokBehaviorPoseGraph(document, Path.GetFileName(file.Name.Replace('\\', '/'))));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_BehaviorLayers_File_Read_Failed", file.Name, model), e);
            }
        }

        return result;
    }
}
