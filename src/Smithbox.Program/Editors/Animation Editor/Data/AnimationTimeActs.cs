#nullable enable
using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>A time act event as the Animation Editor shows it: its type, its window in seconds, its first field</summary>
public readonly record struct AnimationTimeActEvent(int Type, float Start, float End, bool OpenEnded, int FirstField);

/// <summary>
/// The events of every time act in a character's anibnds, by animation ID, read with the character (off the main
/// thread). The Timeline shows them read-only under the keys (Animation Editor Analysis.md §8, W2), and the gravity
/// check (<see cref="AnimationGravityCheck" />) takes its Disable Gravity windows from them.
/// <para>
/// An entry that imports another animation's events (<c>ImportOtherAnim</c>) takes its source's, and remembers where
/// they came from. The first binder's time act for an ID is the one kept, as the clip list keeps the first file. Time
/// act edits not yet saved in the Time Act Editor aren't seen.
/// </para>
/// </summary>
public sealed class AnimationTimeActs
{
    private readonly Dictionary<long, List<AnimationTimeActEvent>> Events = new();
    private readonly Dictionary<long, long> Imports = new();

    // Animations whose HKX another entry plays (a Standard header with ImportsHKX): used though they may have no entry
    private readonly HashSet<long> HkxImports = new();

    /// <summary>Event names by type, from the game's time act templates (empty when the game has none)</summary>
    public IReadOnlyDictionary<int, string> Names { get; private set; } = new Dictionary<int, string>();

    private AnimationTimeActs() { }

    /// <summary>Reads the events of every time act in the binders</summary>
    public static AnimationTimeActs Read(ProjectType game, VirtualFileSystem fs, IEnumerable<string> binders)
    {
        var acts = new AnimationTimeActs { Names = EventNames(game) };
        var imports = new Dictionary<long, long>();

        foreach (var path in binders)
        {
            if (fs.ReadFile(path) is not { } data)
                continue;

            try
            {
                using var binder = AnimationPackage.OpenBinder(game, data);

                // Binders with a version marker moved their time acts
                var firstTimeAct = binder.Files.Any(f => f.ID == 9999999) ? 5000000 : 3000000;

                foreach (var file in binder.Files)
                {
                    if (!file.Name.EndsWith(".tae", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var bytes = binder.ReadFile(file).ToArray();
                    var tae = TAE.Read(DCX.Is(bytes) ? DCX.Decompress(bytes, out _) : bytes);
                    var category = file.ID % firstTimeAct;

                    foreach (var animation in tae.Animations)
                    {
                        var id = animation.ID >= 1000000 ? animation.ID : category * 1000000L + animation.ID;

                        // An entry that takes every event of another animation: resolved once all are read
                        if (animation.MiniHeader is TAE.Animation.AnimMiniHeader.ImportOtherAnim import)
                        {
                            var from = import.ImportFromAnimID;
                            imports.TryAdd(id, from >= 1000000 ? from : category * 1000000L + from);
                            continue;
                        }

                        if (animation.MiniHeader is TAE.Animation.AnimMiniHeader.Standard { ImportsHKX: true } standard)
                        {
                            var hkx = standard.ImportHKXSourceAnimID;
                            acts.HkxImports.Add(hkx >= 1000000 ? hkx : category * 1000000L + hkx);
                        }

                        var events = animation.Events
                            .Select(e => new AnimationTimeActEvent(e.Type, e.StartTime, e.EndTime, e.IsInfiniteEnd, FirstField(e)))
                            .ToList();

                        acts.Events.TryAdd(id, events);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(AnimationTimeActs), LOC.Get("ANIM_Gravity_Read_Failed", path), e);
            }
        }

        // Imports take their source's events (following a chain a few steps)
        foreach (var (id, from) in imports)
        {
            var source = from;
            for (var step = 0; step < 8 && imports.TryGetValue(source, out var next); step++)
                source = next;

            if (acts.Events.TryGetValue(source, out var events) && acts.Events.TryAdd(id, events))
                acts.Imports[id] = source;
        }

        return acts;
    }

    /// <summary>Whether the time acts use an animation: it has an entry, or another entry plays its HKX</summary>
    public bool Uses(long animationId) => Events.ContainsKey(animationId) || HkxImports.Contains(animationId);

    /// <summary>Every animation ID with a time act entry (imports included)</summary>
    public IEnumerable<long> AnimationIds => Events.Keys;

    /// <summary>The animation's events, null when its ID has no time act entry</summary>
    public IReadOnlyList<AnimationTimeActEvent>? For(long animationId)
    {
        return Events.TryGetValue(animationId, out var events) ? events : null;
    }

    /// <summary>The animation whose events an entry imports, null for an entry with its own</summary>
    public long? ImportedFrom(long animationId)
    {
        return Imports.TryGetValue(animationId, out var source) ? source : null;
    }

    /// <summary>"[type] name" as the Time Act Editor's graph labels events</summary>
    public string NameOf(int type)
    {
        return Names.TryGetValue(type, out var name) ? $"[{type}] {name}" : $"[{type}]";
    }

    private static int FirstField(TAE.Event e)
    {
        var p = e.GetParameterBytes(false);
        return p.Length >= 4 ? BitConverter.ToInt32(p, 0) : int.MinValue;
    }

    // ---- the game's event names

    private static readonly Dictionary<ProjectType, Dictionary<int, string>> NameCache = new();

    /// <summary>The action names of the game's Character.xml (Assets/TAE/&lt;game&gt;/Defs), by action ID</summary>
    private static Dictionary<int, string> EventNames(ProjectType game)
    {
        lock (NameCache)
        {
            if (NameCache.TryGetValue(game, out var known))
                return known;

            var names = new Dictionary<int, string>();
            try
            {
                var defs = Path.Join(AppContext.BaseDirectory, "Assets", "TAE", ProjectUtils.GetGameDirectory(game), "Defs", "Character.xml");
                if (File.Exists(defs))
                {
                    foreach (var action in XDocument.Load(defs).Root?.Elements("action") ?? Enumerable.Empty<XElement>())
                    {
                        if (int.TryParse(action.Attribute("id")?.Value, out var id) && action.Attribute("name")?.Value is { Length: > 0 } name)
                            names.TryAdd(id, name);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(AnimationTimeActs), LOC.Get("ANIM_Gravity_Template_Failed"), e);
            }

            NameCache[game] = names;
            return names;
        }
    }
}
