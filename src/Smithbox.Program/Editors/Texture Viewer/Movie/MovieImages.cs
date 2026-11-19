using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// An external image of a movie, ready to draw: the sheet's GPU texture and the cell's UVs.
/// </summary>
public class MovieImage
{
    public TextureResource Resource;
    public Vector2 Uv0;
    public Vector2 Uv1;

    /// <summary>The cell or texture size in pixels.</summary>
    public Vector2 Size;

    /// <summary>Where it came from, for the tooltip and the Images list.</summary>
    public string Source;

    public bool IsReady => Resource?.GPUTexture != null && Resource.GPUTexture.Width > 0;

    /// <summary>Shown in place of a placeholder (Scaleform Configuration).</summary>
    public bool IsSubstitute;

    public ImTextureID TextureId => new ImTextureID(Resource.GPUTexture.TexHandle);
}

/// <summary>
/// Resolves a movie's external image names (DefineExternalImage2 FileName, a .tga) the way the game does:
///  - a cell of a shoebox layout (sblytbnd) with the same stem (MENU_YOU_DIED.tga ↔ MENU_YOU_DIED.png), on the sheet
///    the atlas names (imagePath SB_Brightness.png ↔ texture SB_Brightness) in the TPF beside the layout binder
///    (/menu/hi/01_common.sblytbnd ↔ /menu/hi/01_common.tpf); a low binder only where no hi one has the cell;
///  - else a texture of that name in the menu TPFs (DS3 and BB have no layout binders).
/// TPFs are read in the background; the Texture Viewer's loaded copy is used when it has the file, so unsaved
/// texture edits show. One GPU texture per sheet.
/// </summary>
public class MovieImages
{
    private readonly ProjectEntry Project;

    private class CellSource
    {
        public FileDictionaryEntry Tpf;
        public string Sheet;
        public SubTexture Cell;
        public string Layout;
    }

    private Dictionary<string, CellSource> Cells;
    private object CellsBuiltFrom;
    private int CellsBuiltCount;

    /// <summary>Texture name (lower case) → the menu TPF holding it, once all candidate TPFs are read.</summary>
    private Dictionary<string, FileDictionaryEntry> Direct;
    private List<FileDictionaryEntry> DirectCandidates;

    private readonly Dictionary<string, TPF> Tpfs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Task<TPF>> TpfLoads = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (TextureResource Texture, int Width, int Height)> Sheets = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Resolved images by file name; null is a miss.</summary>
    private readonly Dictionary<string, MovieImage> Resolved = new(StringComparer.OrdinalIgnoreCase);

    public MovieImages(ProjectEntry project)
    {
        Project = project;
    }

    private static readonly string[] ImageExtensions = [".tga", ".tif", ".tiff", ".png", ".dds", ".psd", ".bmp", ".jpg"];

    /// <summary>
    /// A name without its folder and image extensions, lower case. Every trailing image extension goes: the exporter
    /// appended .tga to the artist's file name, so MENU_Clock.tif.tga is the texture or cell MENU_Clock.
    /// </summary>
    public static string Stem(string name)
    {
        var file = name.Replace('\\', '/');
        file = file[(file.LastIndexOf('/') + 1)..];

        while (true)
        {
            var ext = ImageExtensions.FirstOrDefault(e => file.EndsWith(e, StringComparison.OrdinalIgnoreCase));
            if (ext == null || file.Length == ext.Length)
                break;
            file = file[..^ext.Length];
        }

        return file.ToLowerInvariant();
    }

    /// <summary>The project's placeholder substitutes (Scaleform Configuration); null for none.</summary>
    public MovieConfiguration Configuration;
    private int ConfigurationVersion = -1;
    private readonly Dictionary<string, MovieImage> Substituted = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The image to preview for a file name: the Scaleform Configuration's substitute when one is set for it (and can
    /// be found), else the image itself. Null when it can't be found (or is still loading: see <paramref name="pending"/>).
    /// Call from the main thread (uploads the sheet on first use).
    /// </summary>
    public MovieImage Get(string fileName, out bool pending)
    {
        pending = false;

        if (string.IsNullOrEmpty(fileName))
            return null;

        if (Configuration != null && Configuration.Version != ConfigurationVersion)
        {
            Substituted.Clear();
            ConfigurationVersion = Configuration.Version;
        }

        var substitute = Configuration?.FindSubstitute(fileName);
        if (!string.IsNullOrWhiteSpace(substitute))
        {
            if (Substituted.TryGetValue(fileName, out var known))
                return known;

            var image = Lookup(substitute, out pending);
            if (image != null)
            {
                var shown = new MovieImage
                {
                    Resource = image.Resource,
                    Uv0 = image.Uv0,
                    Uv1 = image.Uv1,
                    Size = image.Size,
                    IsSubstitute = true,
                    Source = LOC.Get("TEXVIEW_ScaleformConfig_Substitute_Source", substitute, image.Source),
                };
                Substituted[fileName] = shown;
                return shown;
            }

            if (pending)
                return null;
        }

        return Lookup(fileName, out pending);
    }

    /// <summary>The image of exactly this name (no substitute): a shoebox cell, else a menu texture. Cached.</summary>
    public MovieImage Lookup(string fileName, out bool pending)
    {
        pending = false;

        if (string.IsNullOrEmpty(fileName))
            return null;

        if (Resolved.TryGetValue(fileName, out var done))
            return done;

        var cells = GetCells();
        if (cells == null)
        {
            pending = true;
            return null;
        }

        var stem = Stem(fileName);

        if (cells.TryGetValue(stem, out var cell))
        {
            var tpf = GetTpf(cell.Tpf, out var loading);
            if (tpf == null)
            {
                if (loading)
                {
                    pending = true;
                    return null;
                }
            }
            else
            {
                var image = FromSheet(cell.Tpf, tpf, cell.Sheet, cell.Cell.XValue, cell.Cell.YValue, cell.Cell.WidthValue, cell.Cell.HeightValue,
                    $"{cell.Cell.Name} in {cell.Sheet} ({cell.Layout}, {cell.Tpf.Path})");
                if (image != null)
                {
                    Resolved[fileName] = image;
                    return image;
                }
            }
        }

        // A texture of its own in a menu TPF
        var direct = GetDirect(out var directLoading);
        if (direct == null)
        {
            if (directLoading)
            {
                pending = true;
                return null;
            }
        }
        else if (direct.TryGetValue(stem, out var tpfEntry))
        {
            var tpf = GetTpf(tpfEntry, out _);
            var index = tpf?.Textures.FindIndex(t => t.Name.Equals(stem, StringComparison.OrdinalIgnoreCase)) ?? -1;
            if (index >= 0)
            {
                var (w, h) = IconIndex.TextureSize(tpf.Textures[index]);
                var image = FromSheet(tpfEntry, tpf, tpf.Textures[index].Name, 0, 0, w, h, $"{tpf.Textures[index].Name} ({tpfEntry.Path})");
                Resolved[fileName] = image;
                return image;
            }
        }

        Resolved[fileName] = null;
        return null;
    }

    private MovieImage FromSheet(FileDictionaryEntry tpfEntry, TPF tpf, string sheet, int x, int y, int w, int h, string source)
    {
        var index = tpf.Textures.FindIndex(t => t.Name.Equals(sheet, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return null;

        var key = $"{tpfEntry.Path}|{sheet}";
        if (!Sheets.TryGetValue(key, out var handle))
        {
            // TextureResource uploads PC, PS3, PS4 (BB) and Switch 2 textures
            var texture = new TextureResource(tpf, index);
            texture._LoadTexture(AccessLevel.AccessFull);
            var (sw, sh) = IconIndex.TextureSize(tpf.Textures[index]);
            handle = (texture, sw, sh);
            Sheets[key] = handle;
        }

        if (handle.Texture == null || handle.Width <= 0 || handle.Height <= 0)
            return null;

        return new MovieImage
        {
            Resource = handle.Texture,
            Uv0 = new Vector2((float)x / handle.Width, (float)y / handle.Height),
            Uv1 = new Vector2((float)(x + w) / handle.Width, (float)(y + h) / handle.Height),
            Size = new Vector2(w, h),
            Source = source,
        };
    }

    /// <summary>The cell index, rebuilt when the bank's layout containers change; null while they are set up.</summary>
    private Dictionary<string, CellSource> GetCells()
    {
        var containers = Project.Handler?.TextureData?.PrimaryBank?.ShoeboxEntries;
        if (containers == null)
            return null;

        if (Cells != null && ReferenceEquals(CellsBuiltFrom, containers) && CellsBuiltCount == containers.Count)
            return Cells;

        var cells = new Dictionary<string, CellSource>();
        List<KeyValuePair<FileDictionaryEntry, ShoeboxLayoutContainer>> all;
        try
        {
            all = containers.ToList();
        }
        catch (InvalidOperationException)
        {
            return null; // still being filled
        }

        var textures = Project.Locator.TextureFiles.Entries;

        // Hi binders first, so a low binder only adds cells no hi binder has
        foreach (var pass in new[] { false, true })
        {
            foreach (var (entry, container) in all)
            {
                if (string.IsNullOrEmpty(entry.Path) || IconIndex.IsLow(entry) != pass || container?.Layouts == null)
                    continue;

                var tpf = textures?.FirstOrDefault(t => t.Extension == "tpf"
                    && t.Folder.Equals(entry.Folder, StringComparison.OrdinalIgnoreCase)
                    && t.Filename.Equals(entry.Filename, StringComparison.OrdinalIgnoreCase));
                if (tpf == null)
                    continue;

                foreach (var (layoutName, layout) in container.Layouts)
                {
                    foreach (var atlas in layout.TextureAtlases)
                    {
                        var sheet = Path.GetFileNameWithoutExtension(atlas.ImagePath.Replace('\\', '/'));
                        foreach (var sub in atlas.SubTextures)
                        {
                            cells.TryAdd(Stem(sub.Name), new CellSource { Tpf = tpf, Sheet = sheet, Cell = sub, Layout = Path.GetFileName(layoutName) });
                        }
                    }
                }
            }
        }

        Cells = cells;
        CellsBuiltFrom = containers;
        CellsBuiltCount = all.Count;

        // Misses may be found now
        foreach (var key in Resolved.Where(r => r.Value == null).Select(r => r.Key).ToList())
            Resolved.Remove(key);

        return Cells;
    }

    /// <summary>
    /// The menu TPFs' own textures (the menu folder, its hi and language folders; not the item, map or low ones),
    /// indexed once they are all read. Null while reading.
    /// </summary>
    private Dictionary<string, FileDictionaryEntry> GetDirect(out bool loading)
    {
        loading = false;
        if (Direct != null)
            return Direct;

        if (DirectCandidates == null)
        {
            var excluded = new[] { "/knowledge", "/mapimage", "/low", "/tex", "/icon" };
            DirectCandidates = (Project.Locator.TextureFiles.Entries ?? new())
                .Where(t => t.Extension == "tpf" && t.Folder.StartsWith("/menu", StringComparison.OrdinalIgnoreCase)
                    && t.Folder.Count(c => c == '/') <= 3
                    && !excluded.Any(x => t.Folder.Contains(x, StringComparison.OrdinalIgnoreCase))
                    && !IconIndex.IsLow(t))
                .OrderBy(t => t.Folder.Equals("/menu", StringComparison.OrdinalIgnoreCase) ? 0 : t.Folder.EndsWith("/hi", StringComparison.OrdinalIgnoreCase) ? 1 : t.Folder.Contains("engus", StringComparison.OrdinalIgnoreCase) ? 2 : 3)
                .ThenBy(t => t.Path)
                .ToList();
        }

        var direct = new Dictionary<string, FileDictionaryEntry>();
        foreach (var entry in DirectCandidates)
        {
            var tpf = GetTpf(entry, out var isLoading);
            if (isLoading)
            {
                loading = true;
                continue;
            }
            if (tpf == null)
                continue;

            foreach (var t in tpf.Textures)
                direct.TryAdd(t.Name.ToLowerInvariant(), entry);
        }

        if (loading)
            return null;

        Direct = direct;
        return Direct;
    }

    /// <summary>A TPF by path: the bank's loaded copy, else read in the background (null and loading until then).</summary>
    private TPF GetTpf(FileDictionaryEntry entry, out bool loading)
    {
        loading = false;

        if (Tpfs.TryGetValue(entry.Path, out var tpf))
            return tpf;

        var bank = Project.Handler?.TextureData?.PrimaryBank;
        if (bank?.Entries != null && bank.Entries.TryGetValue(entry, out var contents) && contents?.Files != null)
        {
            var loaded = contents.Files.Values.FirstOrDefault();
            if (loaded != null)
            {
                Tpfs[entry.Path] = loaded;
                return loaded;
            }
        }

        if (!TpfLoads.TryGetValue(entry.Path, out var load))
        {
            var fs = Project.VFS.FS;
            var path = entry.Path;
            TpfLoads[entry.Path] = Task.Run(() =>
            {
                var data = fs.ReadFile(path);
                return data == null ? null : TPF.Read(data.Value);
            });
            loading = true;
            return null;
        }

        if (!load.IsCompleted)
        {
            loading = true;
            return null;
        }

        TpfLoads.Remove(entry.Path);
        if (load.IsFaulted)
            Smithbox.LogError(this, LOC.Get("TEXVIEW_Movie_Log_Tpf_Failed", entry.Path), load.Exception);

        Tpfs[entry.Path] = load.IsFaulted ? null : load.Result;
        return Tpfs[entry.Path];
    }

    /// <summary>
    /// The image names the preview can show (shoebox cells, and menu textures once they are read), for picking a
    /// substitute. Starts reading the menu TPFs when they are not read yet.
    /// </summary>
    public List<string> KnownNames()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var cells = GetCells();
        if (cells != null)
        {
            foreach (var c in cells.Values)
                names.Add(StripExtensions(c.Cell.Name));
        }
        var direct = GetDirect(out _);
        if (direct != null)
        {
            foreach (var key in direct.Keys)
                names.Add(key);
        }
        return names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string StripExtensions(string name)
    {
        var stem = Stem(name);
        var file = name.Replace('\\', '/');
        file = file[(file.LastIndexOf('/') + 1)..];
        return file.Length >= stem.Length ? file[..stem.Length] : stem;
    }

    /// <summary>Forgets every lookup and GPU texture (texture or layout edits, or Reload Images).</summary>
    public void Reset()
    {
        foreach (var sheet in Sheets.Values)
            sheet.Texture?.Dispose();

        Sheets.Clear();
        Resolved.Clear();
        Substituted.Clear();
        Tpfs.Clear();
        TpfLoads.Clear();
        Cells = null;
        Direct = null;
        DirectCandidates = null;
    }

    public void Dispose()
    {
        Reset();
    }
}
