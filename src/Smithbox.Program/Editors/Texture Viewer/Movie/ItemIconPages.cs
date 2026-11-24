using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// An item icon page of a movie: a 1000-frame sprite whose frame f shows icon Page * 1000 + f (SDT's
/// …MENU_ItemIcon_01000_22, placed as State_1). See Claude/Documentation/Param Editor.md (Item icon injector).
/// </summary>
public sealed class ItemIconPage
{
    public GFX.DefineSprite Sprite;
    public int Page;
}

/// <summary>
/// The changes one injection makes to a movie: tag lists before and after (the movie's own list and page sprites'),
/// so applying and undoing swap the contents and keep every list object (and the scene's references) the same.
/// </summary>
public sealed class ItemIconEdit
{
    public GFX Movie;
    public List<(List<GFX.Tag> Owner, List<GFX.Tag> Before, List<GFX.Tag> After)> Lists = new();

    /// <summary>The image character added for the icon, or null when the movie had one.</summary>
    public GFX.DefineExternalImage2 AddedImage;

    /// <summary>The pages whose frame now shows the icon.</summary>
    public List<int> Pages = new();

    public bool IsEmpty => Lists.Count == 0;

    public void Apply() => Set(true);

    public void Revert() => Set(false);

    private void Set(bool after)
    {
        foreach (var (owner, before, afterList) in Lists)
        {
            var source = after ? afterList : before;
            owner.Clear();
            owner.AddRange(source);
        }
    }
}

/// <summary>
/// Finds the item icon pages of a movie and injects an icon into them: a DefineExternalImage2 for
/// MENU_ItemIcon_NNNNN.tga, shown on frame NNNNN % 1000 of the page at the depth that holds that frame's icon
/// (pages put a background below the icon on some frames). The frame after it shows what it showed before.
/// Uses only SoulsFormats, so Claude/Tools/GfxIconSurvey compiles it to check injections on the game's movies.
/// </summary>
public static class ItemIconPages
{
    public const string Prefix = "MENU_ItemIcon_";
    public const int PageSize = 1000;

    private static readonly Regex IconName = new(@"^MENU_ItemIcon_(\d+)(d?)\.\w+$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string Stem(int id) => $"{Prefix}{id:D5}";
    public static string FileName(int id) => $"{Stem(id)}.tga";

    /// <summary>The icon number and whether it is a page placeholder (…d), from an image or cell name.</summary>
    public static bool TryParse(string name, out int number, out bool placeholder)
    {
        var m = IconName.Match(name ?? "");
        number = m.Success && int.TryParse(m.Groups[1].Value, out var n) ? n : -1;
        placeholder = m.Success && m.Groups[2].Value.Length > 0;
        return number >= 0;
    }

    /// <summary>
    /// The movie's icon pages: sprites of 1000 frames, without Move placements, whose icons all say the same page
    /// (icon N on frame N % 1000, or the page placeholder).
    /// </summary>
    public static List<ItemIconPage> FindPages(GFX movie)
    {
        var chars = movie.GetCharacters();
        var pages = new List<ItemIconPage>();

        foreach (var sprite in movie.Tags.OfType<GFX.DefineSprite>())
        {
            if (sprite.FrameCount != PageSize)
                continue;

            int? page = null;
            var consistent = true;
            var frame = 0;

            foreach (var tag in sprite.Tags)
            {
                if (tag.Code == GFX.TagCode.ShowFrame)
                {
                    frame++;
                    continue;
                }

                if (tag.Code == GFX.TagCode.RemoveObject || (tag is GFX.PlaceObject { Move: true }))
                {
                    consistent = false;
                    break;
                }

                if (tag is not GFX.PlaceObject { CharacterId: ushort c } || !TryIcon(chars, c, out var number, out var placeholder))
                    continue;

                // A plain icon on another frame says nothing about the page (not seen in the game's movies)
                if (!placeholder && number % PageSize != frame)
                    continue;
                if (placeholder && number % PageSize != 0)
                    continue;

                var p = number / PageSize;
                if (page == null)
                    page = p;
                else if (page != p)
                    consistent = false;
            }

            if (consistent && page != null && frame >= PageSize)
                pages.Add(new ItemIconPage { Sprite = sprite, Page = page.Value });
        }

        return pages;
    }

    /// <summary>Whether the movie shows the icon on its page frame (every page for it), and whether it has that page at all.</summary>
    public static (bool HasPage, bool Shown) Describe(GFX movie, List<ItemIconPage> pages, int id)
    {
        var mine = pages.Where(p => p.Page == id / PageSize).ToList();
        if (mine.Count == 0)
            return (false, false);

        var chars = movie.GetCharacters();
        var shown = mine.All(p =>
        {
            var states = States(p.Sprite, out _);
            var frame = id % PageSize;
            if (frame >= states.Count)
                return false;
            return states[frame].Values.Any(po => po.CharacterId is ushort c && chars.TryGetValue(c, out var def)
                && def is GFX.DefineExternalImage2 img && img.FileName.Equals(FileName(id), StringComparison.OrdinalIgnoreCase));
        });

        return (true, shown);
    }

    /// <summary>
    /// Builds (does not apply) the edit that shows the icon in every page of the movie for its id. Null with a
    /// problem when the movie can't take it; an empty edit when the movie already shows it.
    /// </summary>
    public static ItemIconEdit Inject(GFX movie, int id, out string problem)
    {
        problem = null;
        var page = id / PageSize;
        var frame = id % PageSize;
        var pages = FindPages(movie).Where(p => p.Page == page).ToList();

        if (pages.Count == 0)
        {
            problem = $"no page {page}";
            return null;
        }

        var chars = movie.GetCharacters();
        var edit = new ItemIconEdit { Movie = movie };
        var top = movie.Tags.ToList();

        var image = chars.Values.OfType<GFX.DefineExternalImage2>()
            .FirstOrDefault(i => i.FileName.Equals(FileName(id), StringComparison.OrdinalIgnoreCase));

        foreach (var p in pages)
        {
            var states = States(p.Sprite, out var frames);
            if (frame >= states.Count)
            {
                problem = $"page {page} has no frame {frame}";
                return null;
            }

            // The depth that holds the frame's icon, and the placement there (cloned for the new icon)
            var (depth, current) = IconAt(states[frame], chars);
            if (current == null)
            {
                // No icon on this frame: the depth and placement the page's icons use
                current = p.Sprite.Tags.OfType<GFX.PlaceObject>().FirstOrDefault(po => po.CharacterId is ushort c && TryIcon(chars, c, out _, out _));
                if (current == null)
                {
                    problem = $"page {page} has no icon placement";
                    return null;
                }
                depth = current.Depth;
            }

            if (image != null && current.CharacterId == image.Id)
                continue;

            if (image == null)
            {
                var template = chars[current.CharacterId.Value] as GFX.DefineExternalImage2;
                var newId = NextCharacterId(movie);
                if (template == null || newId > ushort.MaxValue)
                {
                    problem = template == null ? "no image to copy" : "no free character id";
                    return null;
                }

                image = new GFX.DefineExternalImage2
                {
                    Code = GFX.TagCode.GfxDefineExternalImage2,
                    LongHeader = template.LongHeader,
                    Id = (uint)newId,
                    BitmapFormat = template.BitmapFormat,
                    TargetWidth = template.TargetWidth,
                    TargetHeight = template.TargetHeight,
                    ExportName = Stem(id),
                    FileName = FileName(id),
                    Extra = Array.Empty<byte>(),
                };
                edit.AddedImage = image;
            }

            var tags = p.Sprite.Tags.ToList();
            var removeSample = tags.OfType<GFX.RemoveObject2>().FirstOrDefault();

            var placed = Clone(current);
            placed.CharacterId = (ushort)image.Id;

            // The next frame first (it is later in the list, so frame's indices stay valid): back to what it showed
            if (frame + 1 < states.Count)
                Rewrite(tags, frames[frame + 1], depth, placed, states[frame + 1].GetValueOrDefault(depth), removeSample);

            var previous = frame > 0 ? states[frame - 1].GetValueOrDefault(depth) : null;
            Rewrite(tags, frames[frame], depth, previous, placed, removeSample);

            edit.Lists.Add((p.Sprite.Tags, p.Sprite.Tags.ToList(), tags));
            edit.Pages.Add(p.Page);
        }

        if (edit.AddedImage != null)
        {
            // Defined before the first page sprite that places it
            var index = pages.Select(p => top.IndexOf(p.Sprite)).Where(i => i >= 0).DefaultIfEmpty(top.Count).Min();
            top.Insert(index, edit.AddedImage);
            edit.Lists.Add((movie.Tags, movie.Tags.ToList(), top));
        }

        return edit;
    }

    /// <summary>
    /// Replaces a frame's ops at one depth so the depth goes from what the previous frame showed (from) to 'to'
    /// (null: empty). The ops go where the frame's old ops at that depth were, else just before its ShowFrame.
    /// </summary>
    private static void Rewrite(List<GFX.Tag> tags, (int Start, int End) range, int depth, GFX.PlaceObject from, GFX.PlaceObject to, GFX.RemoveObject2 removeSample)
    {
        var insertAt = range.End;
        var removed = new List<GFX.Tag>();

        for (int i = range.End - 1; i >= range.Start; i--)
        {
            var t = tags[i];
            var tagDepth = t is GFX.PlaceObject po ? po.Depth : t is GFX.RemoveObject2 ro ? ro.Depth : -1;
            if (tagDepth != depth)
                continue;

            removed.Add(t);
            tags.RemoveAt(i);
            insertAt = i;
        }

        // The instance stays when the character is the same placement
        var same = from != null && to != null && from.CharacterId == to.CharacterId && from.GetBytes().AsSpan().SequenceEqual(to.GetBytes());

        var ops = new List<GFX.Tag>();
        if (!same)
        {
            if (from != null)
                ops.Add(new GFX.RemoveObject2 { Code = GFX.TagCode.RemoveObject2, LongHeader = removeSample?.LongHeader ?? false, Depth = (ushort)depth });

            // A placement from an earlier frame is copied: the list keeps one object per tag
            if (to != null)
                ops.Add(removed.Contains(to) ? to : Clone(to));
        }

        tags.InsertRange(insertAt, ops);
    }

    /// <summary>
    /// The placement at each depth after each frame, and each frame's tag range (its control tags, up to its
    /// ShowFrame, which is at End).
    /// </summary>
    private static List<Dictionary<int, GFX.PlaceObject>> States(GFX.DefineSprite sprite, out List<(int Start, int End)> frames)
    {
        var states = new List<Dictionary<int, GFX.PlaceObject>>();
        frames = new List<(int, int)>();
        var current = new Dictionary<int, GFX.PlaceObject>();
        var start = 0;

        for (int i = 0; i < sprite.Tags.Count; i++)
        {
            var tag = sprite.Tags[i];

            if (tag is GFX.RemoveObject2 r)
                current.Remove(r.Depth);
            else if (tag is GFX.PlaceObject { Move: false } p)
                current[p.Depth] = p;
            else if (tag.Code == GFX.TagCode.ShowFrame)
            {
                states.Add(new Dictionary<int, GFX.PlaceObject>(current));
                frames.Add((start, i));
                start = i + 1;
            }
        }

        return states;
    }

    /// <summary>The lowest depth of a frame's state that shows an item icon, and its placement.</summary>
    private static (int Depth, GFX.PlaceObject Placement) IconAt(Dictionary<int, GFX.PlaceObject> state, Dictionary<int, GFX.Tag> chars)
    {
        foreach (var (depth, p) in state.OrderBy(kv => kv.Key))
        {
            if (p.CharacterId is ushort c && TryIcon(chars, c, out _, out _))
                return (depth, p);
        }
        return (-1, null);
    }

    private static bool TryIcon(Dictionary<int, GFX.Tag> chars, int id, out int number, out bool placeholder)
    {
        number = -1;
        placeholder = false;
        return chars.TryGetValue(id, out var def) && def is GFX.DefineExternalImage2 img && TryParse(img.FileName, out number, out placeholder);
    }

    private static GFX.PlaceObject Clone(GFX.PlaceObject p)
    {
        var c = new GFX.PlaceObject { Code = p.Code, LongHeader = p.LongHeader };
        c.SetBytes(p.GetBytes());
        return c;
    }

    /// <summary>One past every character id the movie uses: defined, placed, imported or named in SymbolClass.</summary>
    public static int NextCharacterId(GFX movie)
    {
        var max = 0;

        foreach (var tag in movie.AllTags())
        {
            if (tag.DefinedCharacterId is int d)
                max = Math.Max(max, d);
            if (tag is GFX.PlaceObject { CharacterId: ushort c })
                max = Math.Max(max, c);

            try
            {
                if (tag.Code == GFX.TagCode.SymbolClass && tag.Data != null)
                {
                    foreach (var id in SymbolClassIds(tag.Data))
                        max = Math.Max(max, id);
                }
                else if (tag.Code == GFX.TagCode.ImportAssets2 && tag.Data != null)
                {
                    foreach (var id in ImportIds(tag.Data))
                        max = Math.Max(max, id);
                }
            }
            catch
            {
                // A malformed table adds nothing; the ids from the other tags still count
            }
        }

        return max + 1;
    }

    // SymbolClass: count, then (id, name\0)
    private static IEnumerable<int> SymbolClassIds(byte[] d)
    {
        int n = BitConverter.ToUInt16(d, 0), o = 2;
        for (int i = 0; i < n && o + 2 <= d.Length; i++)
        {
            yield return BitConverter.ToUInt16(d, o);
            o += 2;
            o = Array.IndexOf(d, (byte)0, o) + 1;
            if (o <= 0)
                yield break;
        }
    }

    // ImportAssets2: url\0, 2 reserved bytes, count, then (id, name\0)
    private static IEnumerable<int> ImportIds(byte[] d)
    {
        int o = Array.IndexOf(d, (byte)0) + 1 + 2;
        if (o < 3 || o + 2 > d.Length)
            yield break;
        int n = BitConverter.ToUInt16(d, o);
        o += 2;
        for (int i = 0; i < n && o + 2 <= d.Length; i++)
        {
            yield return BitConverter.ToUInt16(d, o);
            o += 2;
            o = Array.IndexOf(d, (byte)0, o) + 1;
            if (o <= 0)
                yield break;
        }
    }

    /// <summary>Whether raw movie bytes mention item icons at all (to skip parsing the movies that don't).</summary>
    public static bool MentionsIcons(ReadOnlySpan<byte> data) => data.IndexOf(Encoding.ASCII.GetBytes(Prefix)) >= 0;
}
