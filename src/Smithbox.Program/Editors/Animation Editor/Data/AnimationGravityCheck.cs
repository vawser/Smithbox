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
using System.Numerics;
using System.Text.Json;
using System.Xml.Linq;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// Whether a clip's rising root motion has gravity off in its time act. In game the character is held to the ground
/// unless the time act disables gravity, so vertical root motion only lifts it then: vanilla ER clips whose root rises
/// have ChrActionFlag FlagType 27 Disable Gravity over the risen frames (94.6% of them, against 2.5% of flat clips;
/// Animation Editor Analysis.md §5.12).
/// <para>
/// The event and flag are read from the game's time act templates (Assets/TAE/&lt;game&gt;): the action whose first
/// field is the FlagType enum, and that enum's "Disable Gravity" option. A game without them is not checked. The
/// windows are read from the character's anibnds when it is opened (and again with <see cref="Reload" />), so time act
/// edits not yet saved in the Time Act Editor aren't seen.
/// </para>
/// </summary>
public sealed class AnimationGravityCheck
{
    /// <summary>Metres the root has to rise above its first frame before it counts as lifting the character</summary>
    public const float RiseThreshold = 0.15f;

    /// <summary>Below this share of the climb with gravity off, a partly covered climb is warned about</summary>
    public const float CoveredShare = 0.75f;

    /// <summary>For each animation ID with a time act: the windows (in seconds) gravity is off, maybe none</summary>
    private readonly Dictionary<long, List<(float Start, float End)>> Windows = new();

    /// <summary>False when the game's templates have no Disable Gravity flag, so nothing is checked</summary>
    public bool Supported { get; private set; }

    private AnimationGravityCheck() { }

    /// <summary>Reads the gravity windows of every time act in the binders (off the main thread)</summary>
    public static AnimationGravityCheck Read(ProjectType game, VirtualFileSystem fs, IEnumerable<string> binders)
    {
        var check = new AnimationGravityCheck();
        if (GravityFlag(game) is not { } flag)
            return check;

        check.Supported = true;
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

                        var windows = animation.Events
                            .Where(e => e.Type == flag.Action && FlagOf(e) == flag.Value)
                            .Select(e => (e.StartTime, e.EndTime))
                            .ToList();

                        // The first binder's time act for an ID is the one kept, as the clip list keeps the first file
                        check.Windows.TryAdd(id, windows);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(AnimationGravityCheck), LOC.Get("ANIM_Gravity_Read_Failed", path), e);
            }
        }

        // Imports take their source's windows (following a chain a few steps)
        foreach (var (id, from) in imports)
        {
            var source = from;
            for (var step = 0; step < 8 && imports.TryGetValue(source, out var next); step++)
                source = next;

            if (check.Windows.TryGetValue(source, out var windows))
                check.Windows.TryAdd(id, windows);
        }

        return check;
    }

    private static int FlagOf(TAE.Event e)
    {
        var p = e.GetParameterBytes(false);
        return p.Length >= 4 ? BitConverter.ToInt32(p, 0) : int.MinValue;
    }

    /// <summary>What the check found for a clip</summary>
    public enum Result
    {
        /// <summary>Not checked: the game has no such flag, or the root doesn't rise</summary>
        Fine,

        /// <summary>The clip's ID has no time act</summary>
        NoTimeAct,

        /// <summary>Gravity is never off while the root is up</summary>
        NotDisabled,

        /// <summary>Gravity is off over part of the rise only (from the first risen frame to the highest)</summary>
        PartlyDisabled
    }

    /// <summary>
    /// Checks a clip's root motion as it plays: from the first frame the root is above its start to the highest frame
    /// (the climb), every frame should have gravity off. Vanilla clips turn gravity back on shortly before landing, so
    /// the way down isn't required.
    /// </summary>
    public Result Check(long animationId, EditableClip clip, out (int First, int Peak, float Height) rise)
    {
        rise = default;
        if (!Supported || clip.EffectiveRootMotion is not { Length: > 1 } motion)
            return Result.Fine;

        var up = clip.UpAxis;
        var start = Vector3.Dot(new Vector3(motion[0].X, motion[0].Y, motion[0].Z), up);
        var first = -1;
        var peak = 0;
        var height = 0f;
        for (var f = 0; f < motion.Length; f++)
        {
            var h = Vector3.Dot(new Vector3(motion[f].X, motion[f].Y, motion[f].Z), up) - start;
            if (h > RiseThreshold && first < 0)
                first = f;
            if (h > height)
            {
                height = h;
                peak = f;
            }
        }

        if (first < 0)
            return Result.Fine;

        rise = (first, peak, height);

        if (!Windows.TryGetValue(animationId, out var windows))
            return Result.NoTimeAct;

        var covered = 0;
        for (var f = first; f <= peak; f++)
        {
            var time = f * clip.FrameDuration;
            if (windows.Any(w => time >= w.Start - 1e-3f && time <= w.End + 1e-3f))
                covered++;
        }

        // Vanilla climbs can lose gravity-off a frame or two early, so only a clearly short window is warned about
        return covered == 0 ? Result.NotDisabled
            : covered < CoveredShare * (peak - first + 1) ? Result.PartlyDisabled
            : Result.Fine;
    }

    /// <summary>The gravity windows of an animation, in frames, for the warning</summary>
    public string WindowsText(long animationId, float frameDuration)
    {
        if (!Windows.TryGetValue(animationId, out var windows) || windows.Count == 0 || frameDuration <= 0)
            return "";

        return string.Join(", ", windows.Select(w => $"{MathF.Round(w.Start / frameDuration)}-{MathF.Round(w.End / frameDuration)}"));
    }

    // ---- the game's flag

    private static readonly Dictionary<ProjectType, (int Action, int Value)?> Flags = new();

    /// <summary>
    /// The action that sets a character flag (its first field is the FlagType enum, in the game's Character.xml) and
    /// the FlagType value named Disable Gravity; null when the game's templates have no such flag
    /// </summary>
    private static (int Action, int Value)? GravityFlag(ProjectType game)
    {
        lock (Flags)
        {
            if (Flags.TryGetValue(game, out var known))
                return known;

            (int, int)? found = null;
            try
            {
                var folder = Path.Join(AppContext.BaseDirectory, "Assets", "TAE", ProjectUtils.GetGameDirectory(game));
                var defs = Path.Join(folder, "Defs", "Character.xml");
                var enums = Path.Join(folder, "Enums", "FlagType.json");

                if (File.Exists(defs) && File.Exists(enums))
                {
                    var action = XDocument.Load(defs).Root?.Elements("action")
                        .FirstOrDefault(a => a.Elements().FirstOrDefault()?.Attribute("enum")?.Value == "FlagType");

                    var entry = JsonSerializer.Deserialize(File.ReadAllText(enums),
                        StudioCore.Editors.ParamEditor.ParamEditorJsonSerializerContext.Default.ParamEnumEntry);
                    var option = entry?.Options.FirstOrDefault(o => o.Names.Any(n =>
                        n.Language == "English" && n.Text?.Contains("Disable Gravity", StringComparison.OrdinalIgnoreCase) == true));

                    if (action != null && int.TryParse(action.Attribute("id")?.Value, out var actionId) &&
                        option != null && int.TryParse(option.Key, out var value))
                        found = (actionId, value);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(AnimationGravityCheck), LOC.Get("ANIM_Gravity_Template_Failed"), e);
            }

            Flags[game] = found;
            return found;
        }
    }
}
