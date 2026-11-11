using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The ER world map built from the game's own map tiles (menu/71_maptile.tpfbhd/.tpfbdt), as the game's map menu
/// does. See Claude/Documentation/World Map Tiles.md.
///
/// Tiles are MENU_MapTile_Mmm_Ll_xx_yy_ffffffff, 256x256: map Mmm (M00 Lands Between, M10 Shadow of the Erdtree),
/// level Ll, cell (xx, yy) with yy counting from the south, and a set of map fragment bits. The game shows, per
/// cell, the variant whose bits are the fragments the player has; with every fragment that is the cell's mask from
/// the map's .mtmsk (menu/71_maptile.mtmskbnd.dcx). That is the variant used here.
///
/// Image space is L0 map pixels, one per metre: the game's CS::WorldMapAreaConverter puts the centre of block
/// mAA_28_64_00 at (128, 128), with x east and y south, for both m60 and m61. A level with scale s draws an L0
/// pixel as s of its pixels, and its rows run down from row ceil(10496 * s / 256) - 1.
/// </summary>
public class WorldMapTileSet : IDisposable
{
    /// <summary>The L0 image is 41 x 41 cells</summary>
    public const float ImageSize = 41 * 256;

    /// <summary>The levels with revealed art (L3 and L4 only have the unrevealed outline)</summary>
    private static readonly float[] LevelScales = { 1.0f, 0.75f, 0.2f };

    private static readonly Regex TileName = new(@"MENU_MapTile_(M\d\d)_L(\d)_(\d\d)_(\d\d)_([0-9a-f]{8})", RegexOptions.IgnoreCase);

    private readonly ProjectEntry Project;

    private IMemoryOwner<byte> MappedData;
    private BXF4Reader Reader;

    /// <summary>The binder file shown for each (map, level, x, y)</summary>
    private Dictionary<(string Map, int Level, int X, int Y), BinderFileHeader> Cells = new();

    private Task LoadTask;
    public bool Loaded { get; private set; }
    public string LoadError { get; private set; }

    public WorldMapTileSet(ProjectEntry project)
    {
        Project = project;
    }

    public static bool IsSupported(ProjectEntry project) => project.Descriptor.ProjectType is ProjectType.ER;

    #region Load

    /// <summary>Starts reading the tile index in the background, once</summary>
    public void EnsureLoading()
    {
        if (LoadTask != null)
            return;

        var fs = Project.VFS.FS;

        LoadTask = Task.Run(() =>
        {
            try
            {
                Load(fs);
                Loaded = true;
            }
            catch (Exception e)
            {
                LoadError = e.Message;
                Smithbox.LogError(this, LOC.Get("MAP_WorldMapTiles_Failed_Load"), e);
            }
        });
    }

    private void Load(VirtualFileSystem fs)
    {
        const string bhdPath = "/menu/71_maptile.tpfbhd";
        const string bdtPath = "/menu/71_maptile.tpfbdt";
        const string maskPath = "/menu/71_maptile.mtmskbnd.dcx";

        var bhd = fs.ReadFileOrThrow(bhdPath);

        // The data is about 1.2 GB, so it is mapped rather than read; tiles are read from it as they are shown
        if (fs.TryMemoryMapFile(bdtPath, out var mapped))
        {
            MappedData = mapped;
            Reader = new BXF4Reader(bhd, mapped.Memory);
        }
        else
        {
            Reader = new BXF4Reader(bhd, fs.ReadFileOrThrow(bdtPath));
        }

        var masks = ReadMasks(fs.ReadFileOrThrow(maskPath));

        // All variants of each cell
        var variants = new Dictionary<(string, int, int, int), List<(uint Flags, BinderFileHeader File)>>();

        foreach (var file in Reader.Files)
        {
            var match = TileName.Match(file.Name ?? "");

            if (!match.Success)
                continue;

            var level = int.Parse(match.Groups[2].Value);

            if (level >= LevelScales.Length)
                continue;

            var key = (match.Groups[1].Value.ToUpperInvariant(), level, int.Parse(match.Groups[3].Value), int.Parse(match.Groups[4].Value));

            if (!variants.TryGetValue(key, out var list))
                variants[key] = list = new();

            list.Add((Convert.ToUInt32(match.Groups[5].Value, 16), file));
        }

        // The variant with every fragment of the cell: its mask exactly, else the largest variant inside the mask
        var cells = new Dictionary<(string, int, int, int), BinderFileHeader>();

        foreach (var (key, list) in variants)
        {
            var (map, level, x, y) = key;
            var mask = masks.GetValueOrDefault((map, level * 10000 + x * 100 + y));

            var chosen = list.FirstOrDefault(v => v.Flags == mask);

            if (chosen.File == null)
            {
                chosen = list.Where(v => (v.Flags & ~mask) == 0)
                    .OrderByDescending(v => BitOperations.PopCount(v.Flags))
                    .FirstOrDefault();
            }

            if (chosen.File != null)
                cells[key] = chosen.File;
        }

        Cells = cells;
    }

    /// <summary>(map, id) -> fragment bits, id = level * 10000 + x * 100 + y</summary>
    private static Dictionary<(string, int), uint> ReadMasks(Memory<byte> data)
    {
        var result = new Dictionary<(string, int), uint>();
        var binder = BND4.Read(data);

        foreach (var file in binder.Files)
        {
            var match = Regex.Match(file.Name ?? "", @"MENU_MapTile_(M\d\d)\.mtmsk", RegexOptions.IgnoreCase);

            if (!match.Success)
                continue;

            var map = match.Groups[1].Value.ToUpperInvariant();
            var xml = XDocument.Parse(System.Text.Encoding.UTF8.GetString(file.Bytes.Span));

            foreach (var element in xml.Descendants("MapTileMask"))
            {
                if (int.TryParse(element.Attribute("id")?.Value, out var id)
                    && uint.TryParse(element.Attribute("mask")?.Value, out var mask))
                {
                    result[(map, id)] = mask;
                }
            }
        }

        return result;
    }

    #endregion

    #region Draw

    /// <summary>The level to draw at a zoom (screen pixels per L0 pixel): the smallest with enough detail</summary>
    public static int LevelFor(float zoom)
    {
        for (var level = LevelScales.Length - 1; level >= 0; level--)
        {
            if (LevelScales[level] >= zoom)
                return level;
        }

        return 0;
    }

    private static int TopRow(int level) => (int)MathF.Ceiling(ImageSize * LevelScales[level] / 256f) - 1;

    /// <summary>The L0 rectangle of a level's cell</summary>
    private static (Vector2 Min, Vector2 Max) CellRect(int level, int x, int y)
    {
        var size = 256f / LevelScales[level];
        var min = new Vector2(x * size, (TopRow(level) - y) * size);
        return (min, min + new Vector2(size));
    }

    /// <summary>
    /// Draws a map's tiles. imageMin is the screen position of L0 (0, 0), zoom the screen pixels per L0 pixel and
    /// visibleMin/Max the shown part of the image in L0 pixels. The coarsest level is drawn under the chosen one,
    /// so cells still loading show something.
    /// </summary>
    public void Draw(Hexa.NET.ImGui.ImDrawListPtr drawList, string map, Vector2 imageMin, float zoom, Vector2 visibleMin, Vector2 visibleMax)
    {
        if (!Loaded)
            return;

        Frame++;

        var coarsest = LevelScales.Length - 1;
        var level = LevelFor(zoom);

        // Back to front: the coarsest level always (asked for), the levels between it and the chosen one, and the
        // next finer level, only where already loaded, then the chosen level on top (asked for). A cell still
        // loading shows the best tile already there, so crossing a level while zooming in or out doesn't flash
        // coarse tiles.
        for (var l = coarsest; l > level; l--)
            DrawLevel(drawList, map, l, imageMin, zoom, visibleMin, visibleMax, l == coarsest);

        if (level > 0)
            DrawLevel(drawList, map, level - 1, imageMin, zoom, visibleMin, visibleMax, false);

        DrawLevel(drawList, map, level, imageMin, zoom, visibleMin, visibleMax, true);

        Evict();
    }

    private void DrawLevel(Hexa.NET.ImGui.ImDrawListPtr drawList, string map, int level, Vector2 imageMin, float zoom,
        Vector2 visibleMin, Vector2 visibleMax, bool request)
    {
        var size = 256f / LevelScales[level];
        var top = TopRow(level);

        var x0 = (int)MathF.Floor(visibleMin.X / size);
        var x1 = (int)MathF.Floor(visibleMax.X / size);
        var y0 = top - (int)MathF.Floor(visibleMax.Y / size);
        var y1 = top - (int)MathF.Floor(visibleMin.Y / size);

        for (var y = y0; y <= y1; y++)
        {
            for (var x = x0; x <= x1; x++)
            {
                var key = (map, level, x, y);

                if (!Cells.TryGetValue(key, out var file))
                    continue;

                TexturePool.TextureHandle handle;

                if (request)
                {
                    handle = GetTexture(key, file);
                }
                else
                {
                    // Already loaded only; using it keeps it from being freed
                    if (!Textures.TryGetValue(key, out var loaded))
                        continue;

                    loaded.LastUsed = Frame;
                    handle = loaded.Handle;
                }

                if (handle is not { Resident: true })
                    continue;

                var (min, max) = CellRect(level, x, y);
                drawList.AddImage(new Hexa.NET.ImGui.ImTextureID(handle.TexHandle), imageMin + min * zoom, imageMin + max * zoom);
            }
        }
    }

    #endregion

    #region Textures

    private class TileTexture
    {
        public TexturePool.TextureHandle Handle;
        public long LastUsed;
        public volatile bool Requested;
    }

    private readonly Dictionary<(string, int, int, int), TileTexture> Textures = new();
    private readonly ConcurrentQueue<((string, int, int, int) Key, BinderFileHeader File, TileTexture Entry)> Requests = new();
    private int Workers;
    private long Frame;

    /// <summary>Tiles kept on the GPU; a BC7 tile is 64 KB</summary>
    private const int MaxTextures = 1500;

    private const int MaxWorkers = 2;

    private TexturePool.TextureHandle GetTexture((string, int, int, int) key, BinderFileHeader file)
    {
        if (!Textures.TryGetValue(key, out var entry))
        {
            var handle = SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();

            if (handle == null)
                return null;

            entry = new TileTexture { Handle = handle };
            Textures[key] = entry;
        }

        entry.LastUsed = Frame;

        if (!entry.Requested)
        {
            entry.Requested = true;
            Requests.Enqueue((key, file, entry));
            StartWorkers();
        }

        return entry.Handle;
    }

    /// <summary>
    /// Reads requested tiles off the draw thread and queues their upload. A tile no longer drawn by the time its
    /// turn comes is skipped, and asked for again if it comes back into view.
    /// </summary>
    private void StartWorkers()
    {
        while (Workers < MaxWorkers && !Requests.IsEmpty)
        {
            Interlocked.Increment(ref Workers);

            Task.Run(() =>
            {
                try
                {
                    while (Requests.TryDequeue(out var request))
                    {
                        if (Frame - request.Entry.LastUsed > 2)
                        {
                            request.Entry.Requested = false;
                            continue;
                        }

                        try
                        {
                            var data = Reader.ReadFile(request.File);
                            var tpf = TPF.Read(data);

                            if (tpf.Textures.Count == 0)
                                continue;

                            var texture = tpf.Textures[0];
                            var platform = tpf.Platform;
                            var handle = request.Entry.Handle;

                            SceneRenderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
                            {
                                handle.FillWithTPF(d, cl, platform, texture, texture.Name);
                            });
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this, LOC.Get("MAP_WorldMapTiles_Failed_Tile", request.File.Name), e);
                        }
                    }
                }
                finally
                {
                    Interlocked.Decrement(ref Workers);
                }
            });
        }
    }

    /// <summary>Frees the tiles drawn longest ago once there are too many</summary>
    private void Evict()
    {
        if (Textures.Count <= MaxTextures)
            return;

        foreach (var (key, entry) in Textures.Where(t => Frame - t.Value.LastUsed > 2)
                     .OrderBy(t => t.Value.LastUsed).Take(Textures.Count - MaxTextures).ToList())
        {
            // A tile being read or uploaded keeps its handle until it lands
            if (entry.Requested && !entry.Handle.Resident)
                continue;

            entry.Handle.Dispose();
            Textures.Remove(key);
        }
    }

    #endregion

    public void Dispose()
    {
        foreach (var entry in Textures.Values)
            entry.Handle.Dispose();

        Textures.Clear();
        MappedData?.Dispose();
        MappedData = null;
    }
}
