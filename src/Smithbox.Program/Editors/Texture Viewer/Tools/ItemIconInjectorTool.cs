using Andre.IO.VFS;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The Item Icon Injector (Texture Editor Tools window, GFX category): adds an item icon by id. The image goes into a
/// MENU_ItemIcon_NNNNN cell of the hi icon sheets and their low twins (through Texture Import's cell jobs), and every
/// movie with the icon's page shows it on the page's frame (ItemIconPages), all in one undoable action. Shown when a
/// movie of the project has item icon pages (SDT). See Claude/Documentation/Item Icon Injector Plan.md.
/// </summary>
public class ItemIconInjectorTool
{
    private readonly TexEditorView View;
    private readonly ProjectEntry Project;

    private int IconId;
    private string ImagePath = "";
    private int SheetChoice;
    private string Status = "";
    private bool StatusIsError;
    private bool Waiting;

    /// <summary>A movie with item icon pages.</summary>
    private sealed record MovieScan(MovieContents Contents, List<ItemIconPage> Pages);

    private List<MovieScan> Scans;
    private Task<List<(FileDictionaryEntry Entry, GFX Movie)>> ScanTask;

    private string IdentifiedPath;
    private (int W, int H) IdentifiedSize;

    public ItemIconInjectorTool(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private TextureBank Bank => Project.Handler?.TextureData?.PrimaryBank;
    private MovieBank Movies => Bank?.Movies;

    /// <summary>The movies have been scanned and some have item icon pages, so the tool is shown.</summary>
    public bool IsAvailable
    {
        get
        {
            UpdateScan();
            return Scans != null && Scans.Count > 0;
        }
    }

    public void OnToolWindow()
    {
        if (!IsAvailable)
            return;

        if (!ToolSections.Header($"{LOC.Get("TEXVIEW_IconInjector_Header")}##itemIconInjectorHeader"))
            return;

        ImGui.BeginChild("ItemIconInjectorToolSection", ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

        Display();

        ImGui.EndChild();
    }

    #region Scan
    /// <summary>
    /// Reads the movies that mention item icons on a task (once), then adds them to the bank on the main thread,
    /// keeping a movie the bank already read (it may have unsaved edits).
    /// </summary>
    private void UpdateScan()
    {
        var movies = Movies;
        if (Scans != null || movies?.Entries == null || movies.Entries.Count == 0)
            return;

        if (ScanTask == null)
        {
            var fs = movies.TargetFS;
            var entries = movies.Entries.Keys.ToList();

            ScanTask = Task.Run(() =>
            {
                var found = new List<(FileDictionaryEntry, GFX)>();
                foreach (var entry in entries)
                {
                    try
                    {
                        var data = fs.ReadFile(entry.Path);
                        if (data == null || !ItemIconPages.MentionsIcons(data.Value.Span))
                            continue;

                        var movie = GFX.Read(data.Value);
                        if (ItemIconPages.FindPages(movie).Count > 0)
                            found.Add((entry, movie));
                    }
                    catch
                    {
                        // A movie that can't be read has no pages for the tool; opening it logs why
                    }
                }
                return found;
            });
            return;
        }

        if (!ScanTask.IsCompleted)
            return;

        var scans = new List<MovieScan>();
        if (ScanTask.IsCompletedSuccessfully)
        {
            foreach (var (entry, movie) in ScanTask.Result)
            {
                if (!movies.Entries.TryGetValue(entry, out var contents) || contents == null)
                {
                    contents = new MovieContents(entry, movie);
                    movies.Entries[entry] = contents;
                }

                var pages = ItemIconPages.FindPages(contents.Movie);
                if (pages.Count > 0)
                    scans.Add(new MovieScan(contents, pages));
            }
        }

        Scans = scans.OrderBy(s => s.Contents.Entry.Path, StringComparer.OrdinalIgnoreCase).ToList();
        ScanTask = null;
    }
    #endregion

    #region Sheets
    /// <summary>A hi sheet whose atlas has item icon cells.</summary>
    private sealed record IconSheet(ShoeboxLayoutContainer Container, TextureAtlas Atlas, string Sheet, int IconCells, int Cells)
    {
        /// <summary>Most of the sheet's cells are item icons (an icon sheet, not a tutorial or effect sheet).</summary>
        public bool IsIconSheet => IconCells * 2 >= Cells;
    }

    private List<IconSheet> GetSheets()
    {
        var sheets = new List<IconSheet>();
        var containers = Bank?.ShoeboxEntries;
        if (containers == null)
            return sheets;

        foreach (var (entry, container) in containers.ToList())
        {
            if (container == null || container.IsBundled || container.Layouts == null || IconIndex.IsLow(entry))
                continue;

            foreach (var layout in container.Layouts.Values)
            {
                foreach (var atlas in layout.TextureAtlases)
                {
                    var icons = atlas.SubTextures.Count(c => ItemIconPages.TryParse(c.Name, out _, out _));
                    if (icons == 0)
                        continue;

                    var sheet = Path.GetFileNameWithoutExtension(atlas.ImagePath.Replace('\\', '/'));
                    sheets.Add(new IconSheet(container, atlas, sheet, icons, atlas.SubTextures.Count));
                }
            }
        }

        return sheets.OrderBy(s => s.IsIconSheet ? 0 : 1).ThenBy(s => s.Sheet, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static SubTexture FindCell(TextureAtlas atlas, int id)
    {
        return atlas?.SubTextures.FirstOrDefault(c => ItemIconPages.TryParse(c.Name, out var n, out var placeholder) && n == id && !placeholder);
    }

    /// <summary>The cell new icons are formatted like: the last icon cell of the atlas' most common icon size.</summary>
    private static SubTexture IconTemplate(TextureAtlas atlas)
    {
        return atlas.SubTextures
            .Where(c => ItemIconPages.TryParse(c.Name, out _, out var placeholder) && !placeholder && c.WidthValue > 0 && c.HeightValue > 0)
            .GroupBy(c => (c.WidthValue, c.HeightValue))
            .OrderByDescending(g => g.Count())
            .Select(g => g.Last())
            .FirstOrDefault();
    }
    #endregion

    #region Display
    private void Display()
    {
        GUI.WrappedText(LOC.Get("TEXVIEW_IconInjector_Hint"));
        GUI.Spacer();

        var busy = Waiting && View.TextureImport.IsRunning;

        // Icon ID (applied by the Add button only)
        GUI.WrappedText(LOC.Get("TEXVIEW_IconInjector_Id"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##itemIconId", ref IconId);
        IconId = Math.Clamp(IconId, 0, 99999);
        GUI.Tooltip(LOC.Get("TEXVIEW_IconInjector_Id_TT"));

        var id = IconId;
        ImGui.TextDisabled(ItemIconPages.Stem(id));

        // Movies
        var states = DescribeMovies(id);
        var withPage = states.Count(s => s.HasPage);
        var shown = states.Count(s => s.Shown);

        if (withPage == 0)
        {
            var pages = Scans.SelectMany(s => s.Pages.Select(p => p.Page)).Distinct().OrderBy(p => p);
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TEXVIEW_IconInjector_No_Page", id / ItemIconPages.PageSize,
                string.Join(", ", pages.Select(p => $"{p * ItemIconPages.PageSize}–{p * ItemIconPages.PageSize + ItemIconPages.PageSize - 1}"))));
        }
        else
        {
            GUI.WrappedText(LOC.Get("TEXVIEW_IconInjector_Movies", withPage, Scans.Count, shown));
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            foreach (var s in states)
            {
                ImGui.Text($"{s.Name}: {(s.Shown ? LOC.Get("TEXVIEW_IconInjector_Movie_Shown") : s.HasPage ? LOC.Get("TEXVIEW_IconInjector_Movie_Will_Add") : LOC.Get("TEXVIEW_IconInjector_Movie_No_Page"))}");
            }
            ImGui.EndTooltip();
        }

        // Atlas
        var sheets = GetSheets();
        var existing = sheets.Where(s => FindCell(s.Atlas, id) != null).ToList();

        if (existing.Count > 0)
            GUI.WrappedText(LOC.Get("TEXVIEW_IconInjector_Cell_Found", string.Join(", ", existing.Select(s => s.Sheet))));
        else
            GUI.WrappedText(LOC.Get("TEXVIEW_IconInjector_Cell_None"));

        // Image
        GUI.Spacer();
        GUI.WrappedText(LOC.Get("TEXVIEW_IconInjector_Image"));
        ImGui.SetNextItemWidth(-ImGui.CalcTextSize(LOC.Get("TEXVIEW_IconInjector_Browse")).X - ImGui.GetStyle().FramePadding.X * 2 - ImGui.GetStyle().ItemSpacing.X);
        ImGui.InputText("##itemIconImage", ref ImagePath, 1024);
        GUI.Tooltip(LOC.Get("TEXVIEW_IconInjector_Image_TT"));
        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("TEXVIEW_IconInjector_Browse")}##itemIconBrowse"))
        {
            if (PlatformUtils.Instance.OpenFileDialog(LOC.Get("TEXVIEW_IconInjector_Dialog"), TextureEncoder.FileDialogFilters, CFG.Current.TextureViewerImport_LastFolder, out var path)
                && !string.IsNullOrEmpty(path))
            {
                ImagePath = path;
                CFG.Current.TextureViewerImport_LastFolder = Path.GetDirectoryName(path);
            }
        }

        var path0 = ImagePath.Trim().Trim('"');
        var hasImage = path0.Length > 0;
        var imageOk = hasImage && File.Exists(path0) && TextureEncoder.IsImageFile(path0);

        if (hasImage && !imageOk)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TEXVIEW_IconInjector_Image_Missing"));
        else if (imageOk)
        {
            var (w, h) = Identify(path0);
            ImGui.TextDisabled(w > 0 ? LOC.Get("TEXVIEW_IconInjector_Image_Size", w, h) : LOC.Get("TEXVIEW_IconInjector_Image_Unknown_Size"));
        }

        // Sheet, for a new cell
        if (existing.Count == 0 && sheets.Count > 0)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("TEXVIEW_IconInjector_Sheet"));

            SheetChoice = Math.Clamp(SheetChoice, 0, sheets.Count);
            var preview = SheetChoice == 0 ? LOC.Get("TEXVIEW_IconInjector_Sheet_Automatic") : SheetLabel(sheets[SheetChoice - 1]);

            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("##itemIconSheet", preview))
            {
                if (ImGui.Selectable($"{LOC.Get("TEXVIEW_IconInjector_Sheet_Automatic")}##sheetAuto", SheetChoice == 0))
                    SheetChoice = 0;

                for (int i = 0; i < sheets.Count; i++)
                {
                    if (ImGui.Selectable($"{SheetLabel(sheets[i])}##sheet{i}", SheetChoice == i + 1))
                        SheetChoice = i + 1;
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("TEXVIEW_IconInjector_Sheet_TT"));

            // Shared with Texture Import's option, so both tools grow sheets the same way
            ImGui.Checkbox($"{LOC.Get("TEXVIEW_TexImport_Grow_Sheet")}##itemIconGrowSheet", ref CFG.Current.TextureViewerImport_GrowSheet);
            GUI.Tooltip(LOC.Get("TEXVIEW_IconInjector_Grow_Sheet_TT"));
        }

        // Add
        GUI.Spacer();
        var canAdd = !busy && !View.TextureImport.IsRunning && withPage > 0
            && (imageOk || (!hasImage && existing.Count > 0))
            && (imageOk || shown < withPage);

        ImGui.BeginDisabled(!canAdd);
        if (ImGui.Button($"{LOC.Get(imageOk ? "TEXVIEW_IconInjector_Add" : "TEXVIEW_IconInjector_Add_Movies_Only")}##itemIconAdd"))
        {
            Add(id, imageOk ? path0 : null, sheets, existing);
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get(imageOk ? "TEXVIEW_IconInjector_Add_TT" : "TEXVIEW_IconInjector_Add_Movies_Only_TT"));

        if (busy)
            ImGui.TextDisabled(LOC.Get("TEXVIEW_IconInjector_Status_Encoding"));
        else if (Status != "")
        {
            if (StatusIsError)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, Status);
            else
                GUI.WrappedText(Status);
        }
    }

    private sealed record MovieState(string Name, bool HasPage, bool Shown);

    private (int Id, int Versions) DescribedFor = (-1, 0);
    private List<MovieState> Described = new();

    /// <summary>Whether each scanned movie has the icon's page and shows it, cached until the id or a movie changes.</summary>
    private List<MovieState> DescribeMovies(int id)
    {
        var versions = Scans.Aggregate(17, (h, s) => unchecked(h * 31 + s.Contents.Version));
        if (DescribedFor == (id, versions))
            return Described;

        Described = Scans.Select(s =>
        {
            var (hasPage, shown) = ItemIconPages.Describe(s.Contents.Movie, s.Pages, id);
            return new MovieState(s.Contents.Entry.Filename, hasPage, shown);
        }).ToList();
        DescribedFor = (id, versions);
        return Described;
    }

    private static string SheetLabel(IconSheet s)
    {
        return LOC.Get("TEXVIEW_IconInjector_Sheet_Label", s.Sheet, s.IconCells);
    }

    private (int W, int H) Identify(string path)
    {
        if (path == IdentifiedPath)
            return IdentifiedSize;

        IdentifiedPath = path;
        IdentifiedSize = (0, 0);

        try
        {
            if (Path.GetExtension(path).Equals(".dds", StringComparison.OrdinalIgnoreCase))
            {
                var dds = new DDS(File.ReadAllBytes(path));
                IdentifiedSize = (dds.dwWidth, dds.dwHeight);
            }
            else if (SixLabors.ImageSharp.Image.Identify(path) is { } info)
            {
                IdentifiedSize = (info.Width, info.Height);
            }
        }
        catch
        {
            // Unknown size: the encoder says what is wrong with the file when it is used
        }

        return IdentifiedSize;
    }
    #endregion

    #region Add
    private void Add(int id, string imagePath, List<IconSheet> sheets, List<IconSheet> existing)
    {
        Status = "";
        StatusIsError = false;

        var failures = new List<string>();
        var requests = new List<TextureImport.SheetCellRequest>();

        if (imagePath != null)
        {
            var targets = existing.Count > 0 ? existing : PickSheet(id, sheets, failures);

            foreach (var sheet in targets)
            {
                if (!AddRequests(id, imagePath, sheet, requests, failures))
                    break;
            }
        }

        if (failures.Count > 0)
        {
            Report(false, string.Join("\n", failures));
            return;
        }

        var report = new List<string>();
        Func<List<EditorAction>> movieActions = () => MovieActions(id, report);

        // No image: the movies only, at once
        if (requests.Count == 0)
        {
            var actions = movieActions();
            if (actions.Count > 0)
                View.ActionManager.ExecuteAction(actions[0]);
            Report(actions.Count > 0, string.Join("\n", report));
            return;
        }

        var cellNames = string.Join(", ", requests.Select(r => $"{r.Texture.Name}{(r.Existing != null ? "" : "+")}").Distinct());

        if (!View.TextureImport.StartSheetCells(requests, movieActions, (ok, status) =>
            {
                Waiting = false;
                Report(ok, ok ? string.Join("\n", report.Prepend(LOC.Get("TEXVIEW_IconInjector_Status_Cells", cellNames))) : status);
            }, failures))
        {
            Report(false, string.Join("\n", failures));
            return;
        }

        Waiting = true;
        Status = LOC.Get("TEXVIEW_IconInjector_Status_Encoding");
    }

    private void Report(bool ok, string status)
    {
        Status = status;
        StatusIsError = !ok;
        if (ok)
            Smithbox.Log(this, status);
        else if (!string.IsNullOrEmpty(status))
            Smithbox.LogError(this, status);
    }

    /// <summary>The sheet a new icon cell goes in: the one picked, else the first icon sheet with room for it.</summary>
    private List<IconSheet> PickSheet(int id, List<IconSheet> sheets, List<string> failures)
    {
        if (sheets.Count == 0)
        {
            failures.Add(LOC.Get("TEXVIEW_IconInjector_No_Sheets"));
            return new();
        }

        if (SheetChoice > 0 && SheetChoice <= sheets.Count)
            return [sheets[SheetChoice - 1]];

        foreach (var sheet in sheets.Where(s => s.IsIconSheet))
        {
            var template = IconTemplate(sheet.Atlas);
            var tpfEntry = sheet.Container.FindPairedTpf();
            if (template == null || tpfEntry == null)
                continue;

            var target = LayoutEditor.LoadSheet(Project, tpfEntry, sheet.Sheet, out var tex);
            if (target == null)
                continue;

            var info = TextureFormats.GetInfo(target.Tpf, tex);
            var spot = LayoutGeometry.FindFreeSpot(sheet.Atlas.SubTextures.Select(CellRect.Of), template.WidthValue, template.HeightValue, info.Width, info.Height);
            if (spot != null)
                return [sheet];
        }

        // None has room: the first icon sheet (it grows when Grow Sheet is on)
        return [sheets.FirstOrDefault(s => s.IsIconSheet) ?? sheets[0]];
    }

    /// <summary>The hi cell request for one sheet, and its low twin's (replace where the cell is, else add).</summary>
    private bool AddRequests(int id, string imagePath, IconSheet sheet, List<TextureImport.SheetCellRequest> requests, List<string> failures)
    {
        var tpfEntry = sheet.Container.FindPairedTpf();
        TPF.Texture tex = null;
        var target = tpfEntry != null ? LayoutEditor.LoadSheet(Project, tpfEntry, sheet.Sheet, out tex) : null;

        if (target == null)
        {
            failures.Add(LOC.Get("TEXVIEW_IconInjector_No_Texture", sheet.Sheet));
            return false;
        }

        if (sheet.Atlas.Layout == null || sheet.Atlas.Layout.IsReadOnly)
        {
            failures.Add(LOC.Get("TEXVIEW_IconInjector_Read_Only", sheet.Sheet));
            return false;
        }

        var info = TextureFormats.GetInfo(target.Tpf, tex);
        var cell = FindCell(sheet.Atlas, id);
        var template = IconTemplate(sheet.Atlas);
        var hiName = cell?.Name ?? $"{ItemIconPages.Stem(id)}{Path.GetExtension(template?.Name ?? ".png")}";
        var hiW = cell?.WidthValue ?? template?.WidthValue ?? 0;
        var hiH = cell?.HeightValue ?? template?.HeightValue ?? 0;

        if (hiW <= 0 || hiH <= 0)
        {
            failures.Add(LOC.Get("TEXVIEW_IconInjector_No_Template", sheet.Sheet));
            return false;
        }

        requests.Add(new TextureImport.SheetCellRequest(target, tex, sheet.Atlas, info.Width, info.Height, cell, template, hiName, hiW, hiH, imagePath));

        // The low twin (none: the sheet has no low copy)
        var twin = LayoutEditor.FindLowTwin(Project, sheet.Container, sheet.Atlas, sheet.Sheet);
        if (twin == null)
            return true;

        if (twin.Problem != null)
        {
            failures.Add(twin.Problem);
            return false;
        }

        var lowCell = FindCell(twin.Atlas, id);
        if (lowCell != null)
        {
            requests.Add(new TextureImport.SheetCellRequest(twin.Target, twin.Texture, twin.Atlas, twin.SheetWidth, twin.SheetHeight,
                lowCell, null, lowCell.Name, lowCell.WidthValue, lowCell.HeightValue, imagePath));
            return true;
        }

        var scale = LayoutGeometry.TwinScaleFor(sheet.Atlas, twin.Atlas, hiName);
        var lowW = Math.Max(1, (int)Math.Round(hiW * scale));
        var lowH = Math.Max(1, (int)Math.Round(hiH * scale));
        var half = lowW < hiW;
        var lowTemplate = IconTemplate(twin.Atlas) ?? twin.Atlas.SubTextures.LastOrDefault();
        var lowName = $"{ItemIconPages.Stem(id)}{Path.GetExtension(lowTemplate?.Name ?? hiName)}";

        requests.Add(new TextureImport.SheetCellRequest(twin.Target, twin.Texture, twin.Atlas, twin.SheetWidth, twin.SheetHeight,
            null, lowTemplate, lowName, lowW, lowH, imagePath,
            new Dictionary<string, string>
            {
                ["half"] = half ? "1" : "0",
                ["originalWidth"] = $"{(half ? hiW : lowW)}",
                ["originalHeight"] = $"{(half ? hiH : lowH)}",
            }));

        return true;
    }

    /// <summary>
    /// The movie edits for the icon, built now (main thread, after the cells are encoded): one action for every
    /// movie with the icon's page. The report says what each movie got.
    /// </summary>
    private List<EditorAction> MovieActions(int id, List<string> report)
    {
        var edits = new List<(MovieContents, ItemIconEdit)>();
        int already = 0;
        var skipped = new List<string>();

        foreach (var scan in Scans)
        {
            var edit = ItemIconPages.Inject(scan.Contents.Movie, id, out var problem);
            if (edit == null)
            {
                if (scan.Pages.Any(p => p.Page == id / ItemIconPages.PageSize))
                    skipped.Add($"{scan.Contents.Entry.Filename} ({problem})");
                continue;
            }

            if (edit.IsEmpty)
            {
                already++;
                continue;
            }

            edits.Add((scan.Contents, edit));
        }

        report.Add(LOC.Get("TEXVIEW_IconInjector_Status_Movies", edits.Count, already));
        if (skipped.Count > 0)
            report.Add(LOC.Get("TEXVIEW_IconInjector_Status_Skipped", string.Join(", ", skipped)));
        report.Add(LOC.Get("TEXVIEW_IconInjector_Status_Save"));

        return edits.Count > 0 ? [new MovieIconInjectAction(edits, Movies?.Images)] : new();
    }
    #endregion
}
