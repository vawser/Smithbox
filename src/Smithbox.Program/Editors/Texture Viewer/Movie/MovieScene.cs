using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The placements of one (timeline, depth) slot from the placement that creates an instance to its removal or
/// replacement. SWF has no tweens: an animated element has a placement (keyframe) per frame, all in its track, so
/// moving the element means moving every keyframe of the track.
/// </summary>
public class MovieTrack
{
    public List<GFX.Tag> Timeline;
    public ushort Depth;
    public List<GFX.PlaceObject> Placements = new();

    public IEnumerable<GFX.PlaceObject> WithMatrix => Placements.Where(p => p.Matrix != null);
    public int KeyframeCount => Placements.Count(p => p.Matrix != null);
    public bool IsAnimated => KeyframeCount > 1;
}

/// <summary>
/// An instance on the display list at the shown frame.
/// </summary>
public class MovieNode
{
    public MovieNode Parent;
    public List<MovieNode> Children = new();

    /// <summary>The placement that created the instance (its identity: selection paths are made of these).</summary>
    public GFX.PlaceObject Origin;

    /// <summary>The latest placement that set the matrix (the keyframe shown), or null for the identity.</summary>
    public GFX.PlaceObject MatrixSource;

    /// <summary>The latest placement that set the colour transform, or null.</summary>
    public GFX.PlaceObject ColorSource;

    public MovieTrack Track;

    /// <summary>The sprite whose timeline holds the instance (0: the root timeline).</summary>
    public int TimelineSprite;

    public ushort Depth;
    public int CharacterId;
    public GFX.Tag Character;
    public string Name;
    public ushort? ClipDepth;
    public bool Hidden;

    public Matrix3x2 Local = Matrix3x2.Identity;
    public Matrix3x2 World = Matrix3x2.Identity;

    /// <summary>Composite colour transform: colour * Mult + Add (0–1 per channel).</summary>
    public Vector4 Mult = Vector4.One;
    public Vector4 Add = Vector4.Zero;

    /// <summary>Local bounds of the character in pixels (empty for sprites: see WorldMin/WorldMax).</summary>
    public Vector2 BoundsMin;
    public Vector2 BoundsMax;
    public bool HasBounds;

    /// <summary>Screen-independent world bounds (pixels), of the instance and its descendants.</summary>
    public Vector2 WorldMin = new(float.MaxValue);
    public Vector2 WorldMax = new(float.MinValue);

    public bool IsMask => ClipDepth != null;
    public bool IsSprite => Character is GFX.DefineSprite;

    /// <summary>The identity of the instance on this display list (its placements from the root), as a string.</summary>
    public string PathKey = "";

    /// <summary>Hidden in the preview by its eye toggle, or inside an instance that is (preview only).</summary>
    public bool PreviewHidden;

    /// <summary>The instance draws something: an image, shape or text field, itself or below it.</summary>
    public bool DisplaysSomething;

    /// <summary>Hidden, a mask, transparent, or inside one of those.</summary>
    public bool IsDrawn;

    public string Label
    {
        get
        {
            var kind = Character switch
            {
                GFX.DefineSprite => "Sprite",
                GFX.DefineExternalImage2 => "Image",
                GFX.DefineEditText => "Text",
                GFX.DefineShape => "Shape",
                null => "Missing",
                _ => Character.ToString(),
            };
            return string.IsNullOrEmpty(Name) ? $"[{Depth}] {kind} {CharacterId}" : $"[{Depth}] {Name}  ({kind} {CharacterId})";
        }
    }

    /// <summary>The placements from the root to this instance: its identity across evaluations.</summary>
    public List<GFX.PlaceObject> Path
    {
        get
        {
            var path = new List<GFX.PlaceObject>();
            for (var n = this; n != null; n = n.Parent)
                path.Insert(0, n.Origin);
            return path;
        }
    }

    /// <summary>The instance-name path the game uses (PlayerHUD/Gauge/AP_Gauge), unnamed levels as [depth].</summary>
    public string NamePath
    {
        get
        {
            var parts = new List<string>();
            for (var n = this; n != null; n = n.Parent)
                parts.Insert(0, string.IsNullOrEmpty(n.Name) ? $"[{n.Depth}]" : n.Name);
            return string.Join("/", parts);
        }
    }

    public IEnumerable<MovieNode> SelfAndDescendants()
    {
        yield return this;
        foreach (var c in Children)
            foreach (var d in c.SelfAndDescendants())
                yield return d;
    }
}

/// <summary>
/// Evaluates a movie's display list: the root timeline at a frame, every sprite instance at frame 0 or its chosen
/// frame (per sprite id, so every use of a symbol shows the same state).
/// </summary>
public class MovieScene
{
    public MovieContents Contents;

    public int RootFrame;
    public Dictionary<int, int> SpriteFrames = new();

    public List<MovieNode> Roots = new();

    /// <summary>Instances hidden in the preview by their eye toggle, by PathKey. Preview only, not saved.</summary>
    public HashSet<string> HiddenPaths = new();

    /// <summary>The drawn leaves in draw order (back to front), for drawing and picking.</summary>
    public List<MovieNode> DrawOrder = new();

    private int BuiltVersion = -1;
    private int BuiltFrame = -1;
    private string BuiltFrames = "";

    private readonly Dictionary<List<GFX.Tag>, Dictionary<GFX.PlaceObject, MovieTrack>> Tracks = new(ReferenceEqualityComparer.Instance);

    private const int MaxNesting = 32;

    public MovieScene(MovieContents contents)
    {
        Contents = contents;
    }

    /// <summary>Evaluates again if the movie, the frame or a sprite frame changed since the last time.</summary>
    public void Update()
    {
        var frames = string.Join(",", SpriteFrames.OrderBy(k => k.Key).Select(k => $"{k.Key}:{k.Value}"));
        if (BuiltVersion == Contents.Version && BuiltFrame == RootFrame && BuiltFrames == frames)
            return;

        BuiltVersion = Contents.Version;
        BuiltFrame = RootFrame;
        BuiltFrames = frames;
        Build();
    }

    public void Invalidate() => BuiltVersion = -1;

    private void Build()
    {
        Roots = new();
        DrawOrder = new();
        Roots = BuildTimeline(Contents.Movie.Tags, 0, RootFrame, null, new HashSet<int>());
    }

    private List<MovieNode> BuildTimeline(List<GFX.Tag> timeline, int spriteId, int frame, MovieNode parent, HashSet<int> ancestors)
    {
        var nodes = new List<MovieNode>();
        var tracks = GetTracks(timeline);

        foreach (var (depth, state) in Evaluate(timeline, frame))
        {
            var node = new MovieNode
            {
                Parent = parent,
                Origin = state.Origin,
                MatrixSource = state.MatrixSource,
                ColorSource = state.ColorSource,
                PathKey = $"{parent?.PathKey}/{System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(state.Origin)}",
                Track = tracks.GetValueOrDefault(state.Origin),
                TimelineSprite = spriteId,
                Depth = depth,
                CharacterId = state.CharacterId,
                Character = Contents.Characters.GetValueOrDefault(state.CharacterId),
                Name = state.Name,
                ClipDepth = state.ClipDepth,
                Hidden = state.Hidden,
            };

            node.Local = state.MatrixSource?.Matrix?.ToMatrix3x2() ?? Matrix3x2.Identity;
            node.World = parent == null ? node.Local : node.Local * parent.World;

            var cx = state.ColorSource?.ColorTransform;
            var mult = cx == null || !cx.HasMult ? Vector4.One : new Vector4(cx.RedMult, cx.GreenMult, cx.BlueMult, cx.AlphaMult) / 256f;
            var add = cx == null || !cx.HasAdd ? Vector4.Zero : new Vector4(cx.RedAdd, cx.GreenAdd, cx.BlueAdd, cx.AlphaAdd) / 255f;
            // The child's transform first, then the parent's
            node.Mult = parent == null ? mult : mult * parent.Mult;
            node.Add = parent == null ? add : add * parent.Mult + parent.Add;

            node.PreviewHidden = (parent?.PreviewHidden ?? false) || HiddenPaths.Contains(node.PathKey);
            node.IsDrawn = (parent?.IsDrawn ?? true) && !node.PreviewHidden && !node.Hidden && !node.IsMask && node.Mult.W + node.Add.W > 0.001f;

            SetBounds(node);

            if (node.Character is GFX.DefineSprite sprite && ancestors.Count < MaxNesting && !ancestors.Contains(sprite.Id))
            {
                ancestors.Add(sprite.Id);
                node.Children = BuildTimeline(sprite.Tags, sprite.Id, SpriteFrames.GetValueOrDefault(sprite.Id), node, ancestors);
                ancestors.Remove(sprite.Id);
            }
            else if (node.IsDrawn && node.HasBounds)
            {
                DrawOrder.Add(node);
            }

            node.DisplaysSomething = node.IsSprite ? node.Children.Any(c => c.DisplaysSomething) : node.HasBounds && !node.IsMask;
            Grow(node);
            nodes.Add(node);
        }

        return nodes;
    }

    private static void SetBounds(MovieNode node)
    {
        switch (node.Character)
        {
            case GFX.DefineExternalImage2 image:
                node.BoundsMin = Vector2.Zero;
                node.BoundsMax = new Vector2(image.TargetWidth, image.TargetHeight);
                node.HasBounds = true;
                break;
            case GFX.DefineShape shape:
                node.BoundsMin = new Vector2(shape.Bounds.XMin, shape.Bounds.YMin) / GFX.TwipsPerPixel;
                node.BoundsMax = new Vector2(shape.Bounds.XMax, shape.Bounds.YMax) / GFX.TwipsPerPixel;
                node.HasBounds = true;
                break;
            case GFX.DefineEditText text:
                node.BoundsMin = new Vector2(text.Bounds.XMin, text.Bounds.YMin) / GFX.TwipsPerPixel;
                node.BoundsMax = new Vector2(text.Bounds.XMax, text.Bounds.YMax) / GFX.TwipsPerPixel;
                node.HasBounds = true;
                break;
        }
    }

    /// <summary>The four corners of a node's local bounds in world pixels (top left, top right, bottom right, bottom left).</summary>
    public static Vector2[] WorldCorners(MovieNode node)
    {
        var a = node.BoundsMin;
        var b = node.BoundsMax;
        return
        [
            Vector2.Transform(a, node.World),
            Vector2.Transform(new Vector2(b.X, a.Y), node.World),
            Vector2.Transform(b, node.World),
            Vector2.Transform(new Vector2(a.X, b.Y), node.World),
        ];
    }

    private static void Grow(MovieNode node)
    {
        if (node.HasBounds && !node.IsMask)
        {
            foreach (var p in WorldCorners(node))
            {
                node.WorldMin = Vector2.Min(node.WorldMin, p);
                node.WorldMax = Vector2.Max(node.WorldMax, p);
            }
        }

        foreach (var c in node.Children)
        {
            if (c.WorldMin.X <= c.WorldMax.X)
            {
                node.WorldMin = Vector2.Min(node.WorldMin, c.WorldMin);
                node.WorldMax = Vector2.Max(node.WorldMax, c.WorldMax);
            }
        }
    }

    private class SlotState
    {
        public GFX.PlaceObject Origin;
        public GFX.PlaceObject MatrixSource;
        public GFX.PlaceObject ColorSource;
        public int CharacterId;
        public string Name;
        public ushort? ClipDepth;
        public bool Hidden;
    }

    /// <summary>The display list of a timeline after the control tags of frames 0..frame (by depth).</summary>
    private static SortedDictionary<ushort, SlotState> Evaluate(List<GFX.Tag> timeline, int frame)
    {
        var slots = new SortedDictionary<ushort, SlotState>();
        int shown = 0;

        foreach (var tag in timeline)
        {
            switch (tag)
            {
                case GFX.PlaceObject place:
                    {
                        slots.TryGetValue(place.Depth, out var slot);
                        if (!place.Move || slot == null)
                        {
                            slot = new SlotState { Origin = place };
                            slots[place.Depth] = slot;
                        }
                        else if (place.CharacterId != null && place.CharacterId != slot.CharacterId)
                        {
                            // Move with a new character: the same slot shows another symbol, a new instance
                            slot.Origin = place;
                        }

                        if (place.CharacterId is ushort id) slot.CharacterId = id;
                        if (place.Matrix != null) slot.MatrixSource = place;
                        if (place.ColorTransform != null) slot.ColorSource = place;
                        if (place.Name != null) slot.Name = place.Name;
                        if (place.ClipDepth != null) slot.ClipDepth = place.ClipDepth;
                        if (place.Visible != null) slot.Hidden = place.Visible == 0;
                        break;
                    }
                case GFX.RemoveObject2 remove:
                    slots.Remove(remove.Depth);
                    break;
                default:
                    if (tag.Code == GFX.TagCode.ShowFrame)
                    {
                        if (shown == frame)
                            return slots;
                        shown++;
                    }
                    break;
            }
        }

        return slots;
    }

    /// <summary>The tracks of a timeline, by every placement in them (cached; the tag lists don't change).</summary>
    public Dictionary<GFX.PlaceObject, MovieTrack> GetTracks(List<GFX.Tag> timeline)
    {
        if (Tracks.TryGetValue(timeline, out var known))
            return known;

        var byPlacement = new Dictionary<GFX.PlaceObject, MovieTrack>(ReferenceEqualityComparer.Instance);
        var open = new Dictionary<ushort, (MovieTrack Track, int CharacterId)>();

        foreach (var tag in timeline)
        {
            if (tag is GFX.PlaceObject place)
            {
                var newInstance = !place.Move || !open.ContainsKey(place.Depth)
                    || (place.CharacterId != null && place.CharacterId != open[place.Depth].CharacterId);

                if (newInstance)
                {
                    var track = new MovieTrack { Timeline = timeline, Depth = place.Depth };
                    open[place.Depth] = (track, place.CharacterId ?? (open.TryGetValue(place.Depth, out var o) ? o.CharacterId : 0));
                }

                var current = open[place.Depth];
                current.Track.Placements.Add(place);
                byPlacement[place] = current.Track;
            }
            else if (tag is GFX.RemoveObject2 remove)
            {
                open.Remove(remove.Depth);
            }
        }

        Tracks[timeline] = byPlacement;
        return byPlacement;
    }

    /// <summary>The number of frames of a timeline (its ShowFrame tags, at least 1).</summary>
    public static int FrameCount(List<GFX.Tag> timeline)
    {
        return Math.Max(1, timeline.Count(t => t.Code == GFX.TagCode.ShowFrame));
    }

    /// <summary>The frame labels of a timeline with the frame each names.</summary>
    public static List<(int Frame, GFX.FrameLabel Label)> Labels(List<GFX.Tag> timeline)
    {
        var labels = new List<(int, GFX.FrameLabel)>();
        int frame = 0;
        foreach (var tag in timeline)
        {
            if (tag is GFX.FrameLabel label)
                labels.Add((frame, label));
            else if (tag.Code == GFX.TagCode.ShowFrame)
                frame++;
        }
        return labels;
    }

    /// <summary>The node with this identity path, or null when it is not on the display list at this frame.</summary>
    public MovieNode Find(List<GFX.PlaceObject> path)
    {
        if (path == null || path.Count == 0)
            return null;

        var level = Roots;
        MovieNode found = null;
        foreach (var origin in path)
        {
            found = level.FirstOrDefault(n => ReferenceEquals(n.Origin, origin));
            if (found == null)
                return null;
            level = found.Children;
        }
        return found;
    }

    /// <summary>The topmost drawn leaf under a world point.</summary>
    public MovieNode Pick(Vector2 world)
    {
        for (int i = DrawOrder.Count - 1; i >= 0; i--)
        {
            var node = DrawOrder[i];
            if (!Matrix3x2.Invert(node.World, out var inverse))
                continue;

            var local = Vector2.Transform(world, inverse);
            if (local.X >= Math.Min(node.BoundsMin.X, node.BoundsMax.X) && local.X <= Math.Max(node.BoundsMin.X, node.BoundsMax.X)
                && local.Y >= Math.Min(node.BoundsMin.Y, node.BoundsMax.Y) && local.Y <= Math.Max(node.BoundsMin.Y, node.BoundsMax.Y))
                return node;
        }
        return null;
    }
}
