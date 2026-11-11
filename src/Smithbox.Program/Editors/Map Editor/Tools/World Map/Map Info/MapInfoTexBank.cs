using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The mapinfotexbnd tiles of the project, their edits and the undo history.
///
/// Painting works in "global" pixels: gx = XX * 256 + px across an area's tiles, gy = -YY * 256 + py going south,
/// so a stroke or fill can cross tile edges.
/// </summary>
public class MapInfoTexBank
{
    private readonly ProjectEntry Project;

    /// <summary>The tiles by name; replaced whole when loading ends, so it can be read from the draw loop</summary>
    public Dictionary<string, MapInfoTexTile> Tiles { get; private set; } = new();

    public bool Loaded { get; private set; }
    public bool Loading => _loadTask != null && !_loadTask.IsCompleted;
    public int LoadErrors { get; private set; }

    private Task _loadTask;

    private const int MaxUndo = 100;
    private readonly List<Stroke> UndoStack = new();
    private readonly List<Stroke> RedoStack = new();
    private Stroke CurrentStroke;

    public MapInfoTexBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool AnyModified => Tiles.Values.Any(t => t.Modified);

    public bool CanUndo => UndoStack.Count > 0;
    public bool CanRedo => RedoStack.Count > 0;

    #region Load / Save

    public void Load()
    {
        if (Loading)
            return;

        var entries = Project.Locator.FileDictionary.Entries
            .Where(e => string.Equals(e.Extension, "mapinfotexbnd", StringComparison.OrdinalIgnoreCase))
            .Select(e => (e.Filename, e.Path))
            .ToList();

        var fs = Project.VFS.FS;

        // Painting is blocked while loading, so the history can be cleared here
        UndoStack.Clear();
        RedoStack.Clear();
        CurrentStroke = null;
        Loaded = false;

        _loadTask = Task.Run(() =>
        {
            var tiles = new Dictionary<string, MapInfoTexTile>(StringComparer.OrdinalIgnoreCase);
            var errors = 0;

            foreach (var (name, path) in entries)
            {
                try
                {
                    if (!fs.FileExists(path))
                        continue;

                    var tile = MapInfoTexTile.Read(name, path, fs.ReadFile(path).Value);

                    if (tile != null)
                        tiles[name] = tile;
                }
                catch (Exception e)
                {
                    errors++;
                    Smithbox.LogError(this, LOC.Get("MAP_MapInfo_Failed_Read", path), e);
                }
            }

            Tiles = tiles;
            LoadErrors = errors;
            Loaded = true;
        });
    }

    public void Save(MapInfoTexTile tile)
    {
        try
        {
            Project.VFS.ProjectFS.WriteFile(tile.Path, tile.Write());
            tile.Modified = false;

            Smithbox.Log(this, LOC.Get("MAP_MapInfo_Saved", tile.Path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_MapInfo_Failed_Write", tile.Path), e);
        }
    }

    /// <summary>Saves the tiles with unsaved changes, for the map editor's save</summary>
    public void SaveModified()
    {
        foreach (var tile in Tiles.Values.Where(t => t.Modified).ToList())
            Save(tile);
    }

    #endregion

    #region Global pixels

    public MapInfoTexTile GetTile(string area, int x, int y)
    {
        return Tiles.GetValueOrDefault($"{area}_{x:00}_{y:00}_02");
    }

    public static (int X, int Y, int Px, int Py) Split(int gx, int gy)
    {
        var x = (int)Math.Floor(gx / (double)MapInfoTexTile.Size);
        var ty = (int)Math.Floor(gy / (double)MapInfoTexTile.Size);
        return (x, -ty, gx - x * MapInfoTexTile.Size, gy - ty * MapInfoTexTile.Size);
    }

    public bool TryGet(string area, MapInfoLayer layer, int gx, int gy, out byte value, out MapInfoTexTile tile)
    {
        var (x, y, px, py) = Split(gx, gy);
        tile = GetTile(area, x, y);
        value = MapInfoLayers.None;

        if (tile == null || !tile.HasLayer(layer))
            return false;

        value = tile.Get(layer, px, py);
        return true;
    }

    #endregion

    #region Editing

    /// <summary>
    /// One undo step: a copy of each touched slot's pixels from before the stroke, and from after it.
    /// A slot is 192 KB, so even a fill over a whole area stays small.
    /// </summary>
    private class Stroke
    {
        public readonly Dictionary<(MapInfoTexTile Tile, int Slot), byte[]> Before = new();
        public readonly Dictionary<(MapInfoTexTile Tile, int Slot), byte[]> After = new();
    }

    public void BeginStroke()
    {
        CurrentStroke ??= new Stroke();
    }

    public void EndStroke()
    {
        if (CurrentStroke == null)
            return;

        foreach (var (key, before) in CurrentStroke.Before)
        {
            var now = key.Tile.Pixels[key.Slot];

            if (!now.AsSpan().SequenceEqual(before))
                CurrentStroke.After[key] = (byte[])now.Clone();
        }

        if (CurrentStroke.After.Count > 0)
        {
            foreach (var key in CurrentStroke.Before.Keys.Where(k => !CurrentStroke.After.ContainsKey(k)).ToList())
                CurrentStroke.Before.Remove(key);

            UndoStack.Add(CurrentStroke);

            if (UndoStack.Count > MaxUndo)
                UndoStack.RemoveAt(0);

            RedoStack.Clear();
        }

        CurrentStroke = null;
    }

    private bool SetPixel(MapInfoTexTile tile, MapInfoLayer layer, int px, int py, byte value)
    {
        var slot = MapInfoLayers.Slot(layer);
        var pixels = tile.Pixels[slot];

        if (pixels == null)
            return false;

        var index = MapInfoTexTile.Index(layer, px, py);
        var old = pixels[index];

        if (old == value)
            return false;

        if (CurrentStroke != null && !CurrentStroke.Before.ContainsKey((tile, slot)))
            CurrentStroke.Before[(tile, slot)] = (byte[])pixels.Clone();

        pixels[index] = value;
        tile.Modified = true;
        tile.Version++;
        return true;
    }

    /// <summary>Paints a disc of global pixels; returns whether anything changed</summary>
    public bool PaintDisc(string area, MapInfoLayer layer, float cx, float cy, float radius, byte value)
    {
        var changed = false;
        var r2 = radius * radius;

        for (var gy = (int)Math.Floor(cy - radius); gy <= (int)Math.Ceiling(cy + radius); gy++)
        {
            for (var gx = (int)Math.Floor(cx - radius); gx <= (int)Math.Ceiling(cx + radius); gx++)
            {
                var dx = gx + 0.5f - cx;
                var dy = gy + 0.5f - cy;

                if (dx * dx + dy * dy > r2)
                    continue;

                var (x, y, px, py) = Split(gx, gy);
                var tile = GetTile(area, x, y);

                if (tile != null && SetPixel(tile, layer, px, py, value))
                    changed = true;
            }
        }

        return changed;
    }

    /// <summary>Paints along a segment, so a fast stroke leaves no gaps</summary>
    public bool PaintLine(string area, MapInfoLayer layer, float x0, float y0, float x1, float y1, float radius, byte value)
    {
        var length = MathF.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
        var steps = Math.Max(1, (int)MathF.Ceiling(length / MathF.Max(0.5f, radius * 0.5f)));
        var changed = false;

        for (var i = 0; i <= steps; i++)
        {
            var t = i / (float)steps;
            changed |= PaintDisc(area, layer, x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, radius, value);
        }

        return changed;
    }

    /// <summary>
    /// Fills the region of equal value around a global pixel, across tile edges, within the area's loaded tiles.
    /// Returns the number of pixels changed.
    /// </summary>
    public int Fill(string area, MapInfoLayer layer, int gx, int gy, byte value)
    {
        if (!TryGet(area, layer, gx, gy, out var target, out _) || target == value)
            return 0;

        // Visited pixels per tile (a whole area is a few million pixels)
        var visited = new Dictionary<MapInfoTexTile, bool[]>();
        var queue = new Queue<(int, int)>();
        var count = 0;

        bool Visit(int x, int y)
        {
            var (tx, ty, px, py) = Split(x, y);
            var tile = GetTile(area, tx, ty);

            if (tile == null || !tile.HasLayer(layer))
                return false;

            if (!visited.TryGetValue(tile, out var seen))
                visited[tile] = seen = new bool[MapInfoTexTile.Size * MapInfoTexTile.Size];

            var i = py * MapInfoTexTile.Size + px;

            if (seen[i] || tile.Get(layer, px, py) != target)
                return false;

            seen[i] = true;
            queue.Enqueue((x, y));
            return true;
        }

        Visit(gx, gy);

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();
            var (tx, ty, px, py) = Split(x, y);

            if (SetPixel(GetTile(area, tx, ty), layer, px, py, value))
                count++;

            Visit(x + 1, y);
            Visit(x - 1, y);
            Visit(x, y + 1);
            Visit(x, y - 1);
        }

        return count;
    }

    public void Undo()
    {
        if (UndoStack.Count == 0)
            return;

        var stroke = UndoStack[^1];
        UndoStack.RemoveAt(UndoStack.Count - 1);
        Apply(stroke, true);
        RedoStack.Add(stroke);
    }

    public void Redo()
    {
        if (RedoStack.Count == 0)
            return;

        var stroke = RedoStack[^1];
        RedoStack.RemoveAt(RedoStack.Count - 1);
        Apply(stroke, false);
        UndoStack.Add(stroke);
    }

    private static void Apply(Stroke stroke, bool undo)
    {
        foreach (var ((tile, slot), pixels) in undo ? stroke.Before : stroke.After)
        {
            pixels.CopyTo(tile.Pixels[slot], 0);
            tile.Modified = true;
            tile.Version++;
        }
    }

    /// <summary>Drops a tile's edits by reading it again, and clears the history, which may point at it</summary>
    public void Revert(MapInfoTexTile tile)
    {
        try
        {
            var fresh = MapInfoTexTile.Read(tile.Name, tile.Path, Project.VFS.FS.ReadFile(tile.Path).Value);

            if (fresh == null)
                return;

            fresh.Version = tile.Version + 1;
            Tiles = new Dictionary<string, MapInfoTexTile>(Tiles, StringComparer.OrdinalIgnoreCase) { [tile.Name] = fresh };
            UndoStack.Clear();
            RedoStack.Clear();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_MapInfo_Failed_Read", tile.Path), e);
        }
    }

    #endregion
}
