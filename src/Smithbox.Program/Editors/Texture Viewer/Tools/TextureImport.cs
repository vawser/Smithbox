using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// Texture Import (Texture Editor plan, T2): replaces a texture's image, adds a texture from a file, or replaces every
/// texture of the selected TPF that has a file of the same name in a folder. Files are encoded on a background task
/// (TextureEncoder); the results come back through a queue and are applied as undoable actions on the main thread.
/// </summary>
public class TextureImport
{
    public TexEditorView View;
    public ProjectEntry Project;

    private enum JobKind { Replace, Add, ReplaceCells }

    /// <summary>An image file for one layout cell rectangle.</summary>
    /// <summary>
    /// The image for one cell rectangle: a file, or a rectangle of another sheet (hi → low sync).
    /// </summary>
    private sealed record CellSource(CellRect Rect, string Name, string Path, byte[] FromSheet = null, CellRect FromRect = default);

    /// <summary>
    /// A cell to add to an atlas once the sheet is patched: formatted like the template, plus attribute overrides
    /// (half / originalWidth / originalHeight for low twins).
    /// </summary>
    private sealed record NewCell(TextureAtlas Atlas, SubTexture Template, string Name, CellRect Rect, Dictionary<string, string> Attributes = null);

    /// <summary>
    /// One file to encode (or, for ReplaceCells, one sheet with images for some of its cells), with everything read
    /// from the editor up front (the task never touches it).
    /// </summary>
    private sealed record Job(JobKind Kind, string Path, TextureTarget Target, TPF.Texture Texture, int InsertIndex,
        TextureEncodeRequest Request, byte FormatByte, byte[] SheetBytes = null, List<CellSource> Cells = null,
        List<NewCell> NewCells = null, int GrowWidth = 0, int GrowHeight = 0, TextureAtlas GrowAtlas = null);

    private sealed record JobResult(Job Job, TextureEncodeResult Result);

    private readonly ConcurrentQueue<List<JobResult>> Results = new();
    private CancellationTokenSource Cancel;
    private Task Running;

    private volatile int ProgressDone;
    private volatile int ProgressTotal;
    private volatile string ProgressFile = "";
    private string Status = "";

    public TextureImport(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool IsRunning => Running != null && !Running.IsCompleted;

    #region Tool Window
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("TEXVIEW_TexImport_Header")}##textureImportHeader"))
            return;

        ImGui.BeginChild("TextureImportToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("TEXVIEW_TexImport_Hint"));

        var sel = View.Selection;

        if (sel.SelectedTpf == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("TEXVIEW_TexImport_No_TPF"));
            return;
        }

        if (!TextureFormats.IsEditable(sel.SelectedTpf))
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("TEXVIEW_TexImport_Read_Only_Platform", sel.SelectedTpf.Platform));
            return;
        }

        // Selected Texture
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_TexImport_Selected_Header"), LOC.Get("TEXVIEW_TexImport_Selected_Header_TT"));

        if (sel.SelectedTexture != null)
        {
            var info = TextureFormats.GetInfo(sel.SelectedTpf, sel.SelectedTexture);
            GUI.WrappedText(LOC.Get("TEXVIEW_TexImport_Selected_Info", sel.SelectedTexture.Name,
                info.Width, info.Height, TextureFormats.GetLabel(info.Dxgi), info.MipLevels, sel.SelectedTexture.Format));
        }
        else
        {
            GUI.WrappedText(LOC.Get("TEXVIEW_TexImport_Selected_None"));
        }

        // Format
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_TexImport_Format_Header"), LOC.Get("TEXVIEW_TexImport_Format_Header_TT"));

        var preview = CFG.Current.TextureViewerImport_Format == 0
            ? LOC.Get("TEXVIEW_TexImport_Format_Keep")
            : TextureFormats.GetLabel(CFG.Current.TextureViewerImport_Format);

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##textureImportFormat", preview))
        {
            if (ImGui.Selectable($"{LOC.Get("TEXVIEW_TexImport_Format_Keep")}##formatKeep", CFG.Current.TextureViewerImport_Format == 0))
                CFG.Current.TextureViewerImport_Format = 0;

            foreach (var choice in TextureFormats.Choices)
            {
                if (ImGui.Selectable($"{choice.Label}##format{choice.Dxgi}", CFG.Current.TextureViewerImport_Format == choice.Dxgi))
                    CFG.Current.TextureViewerImport_Format = choice.Dxgi;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TEXVIEW_TexImport_Format_TT"));

        // Mip Maps
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_TexImport_Mips_Header"), LOC.Get("TEXVIEW_TexImport_Mips_Header_TT"));

        string[] mipLabels = [LOC.Get("TEXVIEW_TexImport_Mips_Match"), LOC.Get("TEXVIEW_TexImport_Mips_Full"), LOC.Get("TEXVIEW_TexImport_Mips_None")];

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##textureImportMips", mipLabels[Math.Clamp(CFG.Current.TextureViewerImport_MipMode, 0, 2)]))
        {
            for (int i = 0; i < mipLabels.Length; i++)
            {
                if (ImGui.Selectable($"{mipLabels[i]}##mips{i}", CFG.Current.TextureViewerImport_MipMode == i))
                    CFG.Current.TextureViewerImport_MipMode = i;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TEXVIEW_TexImport_Mips_TT"));

        // Size
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_TexImport_Size_Header"), LOC.Get("TEXVIEW_TexImport_Size_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXVIEW_TexImport_Match_Size")}##textureImportMatchSize", ref CFG.Current.TextureViewerImport_MatchSize);
        GUI.Tooltip(LOC.Get("TEXVIEW_TexImport_Match_Size_TT"));

        // Encoder
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_TexImport_Encoder_Header"), LOC.Get("TEXVIEW_TexImport_Encoder_Header_TT"));

        GUI.WrappedText(TextureEncoder.GpuAdapterName != null
            ? LOC.Get("TEXVIEW_TexImport_Encoder_GPU", TextureEncoder.GpuAdapterName)
            : LOC.Get("TEXVIEW_TexImport_Encoder_Pending"));

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_TexImport_Actions_Header"), LOC.Get("TEXVIEW_TexImport_Actions_Header_TT"));

        bool idle = !IsRunning;

        GUI.ConditionalMultiButtonInput("textureImportActions",
            "replaceSelected", LOC.Get("TEXVIEW_TexImport_Replace_Selected"), LOC.Get("TEXVIEW_TexImport_Replace_Selected_TT"),
            () => ReplaceFromDialog(sel.SelectedTexture), idle && sel.SelectedTexture != null,

            "addTexture", LOC.Get("TEXVIEW_TexImport_Add"), LOC.Get("TEXVIEW_TexImport_Add_TT"),
            AddFromDialog, idle,

            "replaceFolder", LOC.Get("TEXVIEW_TexImport_Replace_Folder"), LOC.Get("TEXVIEW_TexImport_Replace_Folder_TT"),
            ReplaceMatchingFromFolderDialog, idle);

        // Layout Cells: on a sheet with a layout, replace cells instead of the whole image
        var layout = View.LayoutEditor;
        if (layout.Atlas != null && sel.SelectedTexture != null)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("TEXVIEW_TexImport_Cells_Header"), LOC.Get("TEXVIEW_TexImport_Cells_Header_TT"));

            GUI.WrappedText(layout.Selected.Count > 0
                ? LOC.Get("TEXVIEW_TexImport_Cells_Selected", layout.Selected.Count, string.Join(", ", layout.Selected.Take(3).Select(c => c.Name)))
                : LOC.Get("TEXVIEW_TexImport_Cells_None"));

            GUI.ConditionalMultiButtonInput("textureImportCellActions",
                "replaceCell", LOC.Get("TEXVIEW_TexImport_Replace_Cell"), LOC.Get("TEXVIEW_TexImport_Replace_Cell_TT"),
                ReplaceSelectedCellsFromDialog, idle && layout.Selected.Count > 0,

                "replaceCellsFolder", LOC.Get("TEXVIEW_TexImport_Replace_Cells_Folder"), LOC.Get("TEXVIEW_TexImport_Replace_Cells_Folder_TT"),
                ReplaceCellsFromFolderDialog, idle);

            // New Cells
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("TEXVIEW_TexImport_New_Cells_Header"), LOC.Get("TEXVIEW_TexImport_New_Cells_Header_TT"));

            string[] sizeLabels = [LOC.Get("TEXVIEW_TexImport_Cell_Size_Image"), LOC.Get("TEXVIEW_TexImport_Cell_Size_Selected")];
            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("##textureImportCellSize", sizeLabels[Math.Clamp(CFG.Current.TextureViewerImport_CellSizeMode, 0, 1)]))
            {
                for (int i = 0; i < sizeLabels.Length; i++)
                {
                    if (ImGui.Selectable($"{sizeLabels[i]}##cellSize{i}", CFG.Current.TextureViewerImport_CellSizeMode == i))
                        CFG.Current.TextureViewerImport_CellSizeMode = i;
                }
                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("TEXVIEW_TexImport_Cell_Size_TT"));

            ImGui.Checkbox($"{LOC.Get("TEXVIEW_TexImport_Grow_Sheet")}##textureImportGrow", ref CFG.Current.TextureViewerImport_GrowSheet);
            GUI.Tooltip(LOC.Get("TEXVIEW_TexImport_Grow_Sheet_TT"));

            GUI.ConditionalMultiButtonInput("textureImportNewCellActions",
                "insertCells", LOC.Get("TEXVIEW_TexImport_Insert_Cells"), LOC.Get("TEXVIEW_TexImport_Insert_Cells_TT"),
                InsertImagesAsCellsFromDialog, idle && layout.IsEditable);
        }

        // Progress
        if (IsRunning || Status != "")
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("TEXVIEW_TexImport_Progress_Header"), LOC.Get("TEXVIEW_TexImport_Progress_Header_TT"));

            if (IsRunning)
            {
                var total = Math.Max(1, ProgressTotal);
                ImGui.ProgressBar(ProgressDone / (float)total, new Vector2(-1, 0), $"{ProgressDone} / {ProgressTotal}");
                GUI.WrappedText(ProgressFile);

                GUI.MultiButtonInput("textureImportCancel",
                    "cancel", LOC.Get("TEXVIEW_TexImport_Cancel"), LOC.Get("TEXVIEW_TexImport_Cancel_TT"),
                    () => Cancel?.Cancel());
            }
            else
            {
                GUI.WrappedText(Status);
            }
        }
    }
    #endregion

    #region Commands
    /// <summary>
    /// Asks for an image and replaces the given texture of the selected TPF with it.
    /// </summary>
    public void ReplaceFromDialog(TPF.Texture tex)
    {
        var target = View.GetEditTarget();
        if (target == null || tex == null || IsRunning)
            return;

        if (!PickFile(LOC.Get("TEXVIEW_TexImport_Dialog_Replace"), out var path))
            return;

        Start([MakeReplaceJob(target, tex, path)]);
    }

    /// <summary>
    /// Asks for one or more images and adds each as a new texture after the selected one.
    /// </summary>
    public void AddFromDialog()
    {
        var target = View.GetEditTarget();
        if (target == null || IsRunning)
            return;

        if (!PlatformUtils.Instance.OpenMultiFileDialog(LOC.Get("TEXVIEW_TexImport_Dialog_Add"), TextureEncoder.FileDialogFilters, out var paths) || paths == null || paths.Count == 0)
            return;

        CFG.Current.TextureViewerImport_LastFolder = Path.GetDirectoryName(paths[0]);

        var reference = View.Selection.SelectedTexture ?? target.Tpf.Textures.FirstOrDefault();
        var index = reference != null ? target.Tpf.Textures.IndexOf(reference) + 1 : target.Tpf.Textures.Count;

        var jobs = new List<Job>();
        foreach (var path in paths)
        {
            jobs.Add(MakeAddJob(target, reference, path, index++));
        }

        Start(jobs);
    }

    /// <summary>
    /// Asks for a folder and replaces every texture of the selected TPF that has an image there with its name.
    /// </summary>
    public void ReplaceMatchingFromFolderDialog()
    {
        var target = View.GetEditTarget();
        if (target == null || IsRunning)
            return;

        if (!PlatformUtils.Instance.OpenFolderDialog(LOC.Get("TEXVIEW_TexImport_Dialog_Folder"), out var folder, CFG.Current.TextureViewerImport_LastFolder) || string.IsNullOrEmpty(folder))
            return;

        CFG.Current.TextureViewerImport_LastFolder = folder;

        var files = Directory.EnumerateFiles(folder)
            .Where(TextureEncoder.IsImageFile)
            .GroupBy(f => Path.GetFileNameWithoutExtension(f), StringComparer.OrdinalIgnoreCase)
            // A .dds wins over another format with the same name, as it is used as it is when it already fits
            .ToDictionary(g => g.Key, g => g.OrderBy(f => Path.GetExtension(f).Equals(".dds", StringComparison.OrdinalIgnoreCase) ? 0 : 1).First(), StringComparer.OrdinalIgnoreCase);

        var jobs = target.Tpf.Textures
            .Where(t => files.ContainsKey(t.Name))
            .Select(t => MakeReplaceJob(target, t, files[t.Name]))
            .ToList();

        if (jobs.Count == 0)
        {
            Status = LOC.Get("TEXVIEW_TexImport_Status_No_Matches", target.Tpf.Textures.Count, folder);
            return;
        }

        Start(jobs);
    }

    /// <summary>
    /// Asks for an image and puts it into each selected layout cell of the selected sheet (one rectangle per alias
    /// group), leaving the rest of the sheet as it is.
    /// </summary>
    public void ReplaceSelectedCellsFromDialog()
    {
        var target = View.GetEditTarget();
        var layout = View.LayoutEditor;
        var tex = View.Selection.SelectedTexture;

        if (target == null || tex == null || layout.Atlas == null || layout.Selected.Count == 0 || IsRunning)
            return;

        if (!PickFile(LOC.Get("TEXVIEW_TexImport_Dialog_Replace_Cell"), out var path))
            return;

        var cells = layout.Selected
            .GroupBy(CellRect.Of)
            .Select(g => new CellSource(g.Key, g.First().Name, path))
            .ToList();

        Start([new Job(JobKind.ReplaceCells, path, target, tex, -1, null, tex.Format, tex.Bytes, cells)]);
    }

    /// <summary>
    /// Asks for a folder and puts each image into the layout cell with its name (MENU_ItemIcon_00123.png → that cell,
    /// or any of its aliases), all in one change of the selected sheet.
    /// </summary>
    public void ReplaceCellsFromFolderDialog()
    {
        var target = View.GetEditTarget();
        var layout = View.LayoutEditor;
        var tex = View.Selection.SelectedTexture;

        if (target == null || tex == null || layout.Atlas == null || IsRunning)
            return;

        if (!PlatformUtils.Instance.OpenFolderDialog(LOC.Get("TEXVIEW_TexImport_Dialog_Cells_Folder"), out var folder, CFG.Current.TextureViewerImport_LastFolder) || string.IsNullOrEmpty(folder))
            return;

        CFG.Current.TextureViewerImport_LastFolder = folder;

        var files = Directory.EnumerateFiles(folder)
            .Where(TextureEncoder.IsImageFile)
            .GroupBy(f => Path.GetFileNameWithoutExtension(f), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // One image per rectangle: the first cell name (of the alias group) that has a file
        var cells = new List<CellSource>();
        foreach (var group in layout.Atlas.SubTextures.GroupBy(CellRect.Of))
        {
            foreach (var cell in group)
            {
                var stem = Path.GetFileNameWithoutExtension(cell.Name);
                if (files.TryGetValue(stem, out var file))
                {
                    cells.Add(new CellSource(group.Key, cell.Name, file));
                    break;
                }
            }
        }

        if (cells.Count == 0)
        {
            Status = LOC.Get("TEXVIEW_TexImport_Status_No_Cell_Matches", layout.Atlas.SubTextures.Count, folder);
            return;
        }

        Start([new Job(JobKind.ReplaceCells, folder, target, tex, -1, null, tex.Format, tex.Bytes, cells)]);
    }

    /// <summary>
    /// Asks for images and adds each as a new cell of the selected sheet: placed in free space (down, then across,
    /// on whole blocks), named after the file, formatted like the selected (or last) cell, its pixels pasted in.
    /// When the sheet is full and growing is on, the sheet doubles (height first) without re-encoding what's there.
    /// </summary>
    public void InsertImagesAsCellsFromDialog()
    {
        var target = View.GetEditTarget();
        var layout = View.LayoutEditor;
        var tex = View.Selection.SelectedTexture;

        if (target == null || tex == null || !layout.IsEditable || IsRunning)
            return;

        if (!PlatformUtils.Instance.OpenMultiFileDialog(LOC.Get("TEXVIEW_TexImport_Dialog_Insert_Cells"), TextureEncoder.FileDialogFilters, out var paths) || paths == null || paths.Count == 0)
            return;

        CFG.Current.TextureViewerImport_LastFolder = Path.GetDirectoryName(paths[0]);

        var atlas = layout.Atlas;
        var template = layout.Primary ?? atlas.SubTextures.LastOrDefault();
        var templateRect = template != null ? CellRect.Of(template) : new CellRect(0, 0, 0, 0);
        var extension = template != null ? Path.GetExtension(template.Name) : ".png";

        var items = new List<(string Name, int W, int H, string Path)>();
        var failures = new List<string>();

        foreach (var path in paths)
        {
            int w, h;
            if (CFG.Current.TextureViewerImport_CellSizeMode == 1 && templateRect.W > 0)
            {
                (w, h) = (templateRect.W, templateRect.H);
            }
            else
            {
                var info = Path.GetExtension(path).Equals(".dds", StringComparison.OrdinalIgnoreCase)
                    ? DdsSize(path)
                    : SixLabors.ImageSharp.Image.Identify(path) is { } id ? (id.Width, id.Height) : (0, 0);
                (w, h) = info;
            }

            var stem = Path.GetFileNameWithoutExtension(path);
            items.Add(($"{stem}{extension}", w, h, path));
        }

        var plan = PlanNewCells(atlas, template, items, layout.SheetWidth, layout.SheetHeight, CFG.Current.TextureViewerImport_GrowSheet, failures);

        if (plan.Cells.Count == 0)
        {
            Status = string.Join("\n", failures.Prepend(LOC.Get("TEXVIEW_TexImport_Status_Nothing_Inserted")));
            return;
        }

        Status = string.Join("\n", failures);

        Start([new Job(JobKind.ReplaceCells, paths[0], target, tex, -1, null, tex.Format, tex.Bytes, plan.Cells,
            plan.NewCells, plan.GrowWidth, plan.GrowHeight, atlas)]);
    }

    /// <summary>
    /// Copies hi cells into the sheet's low twin (T6 hi/low sync): a cell the low layout has (by name) gets the hi
    /// pixels scaled to its own size; a cell it lacks is added, at the twin's scale (half cells get half="1" and their
    /// original size), in the low sheet's free space. One undoable change of the low TPF and layout.
    /// </summary>
    public void SyncCellsToLowTwin(IEnumerable<SubTexture> hiCells)
    {
        var layout = View.LayoutEditor;
        var tex = View.Selection.SelectedTexture;
        var twin = layout.GetLowTwin();

        if (twin == null || twin.Problem != null || tex == null || IsRunning)
            return;

        var lowByName = twin.Atlas.SubTextures.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var replace = new List<CellSource>();
        var add = new List<(string Name, int W, int H, string Path)>();
        var sources = new Dictionary<string, CellRect>(StringComparer.OrdinalIgnoreCase);

        foreach (var hi in hiCells.Distinct())
        {
            var hiRect = CellRect.Of(hi);
            if (hiRect.W <= 0 || hiRect.H <= 0)
                continue;

            if (lowByName.TryGetValue(hi.Name, out var low))
            {
                replace.Add(new CellSource(CellRect.Of(low), low.Name, null, tex.Bytes, hiRect));
            }
            else
            {
                // The low size: the name family's choice (or the sheet's), unless the user picked one
                var scale = CFG.Current.TextureViewerTwin_ScaleMode switch
                {
                    1 => 1.0,
                    2 => 0.5,
                    _ => LayoutGeometry.TwinScaleFor(layout.Atlas, twin.Atlas, hi.Name),
                };
                int w = Math.Max(1, (int)Math.Round(hiRect.W * scale));
                int h = Math.Max(1, (int)Math.Round(hiRect.H * scale));
                add.Add((hi.Name, w, h, null));
                sources[hi.Name] = hiRect;
            }
        }

        var failures = new List<string>();
        var template = twin.Atlas.SubTextures.LastOrDefault();
        var plan = PlanNewCells(twin.Atlas, template, add, twin.SheetWidth, twin.SheetHeight, CFG.Current.TextureViewerImport_GrowSheet, failures);

        // New low cells cut their pixels from the hi sheet; half cells say so
        var cells = replace.Concat(plan.Cells.Select(c => c with { FromSheet = tex.Bytes, FromRect = sources[c.Name] })).ToList();
        var newCells = plan.NewCells.Select(n =>
        {
            var hiRect = sources[n.Name];
            var half = n.Rect.W < hiRect.W;
            return n with
            {
                Attributes = new Dictionary<string, string>
                {
                    ["half"] = half ? "1" : "0",
                    ["originalWidth"] = $"{(half ? hiRect.W : n.Rect.W)}",
                    ["originalHeight"] = $"{(half ? hiRect.H : n.Rect.H)}",
                },
            };
        }).ToList();

        if (cells.Count == 0)
        {
            Status = string.Join("\n", failures.Prepend(LOC.Get("TEXVIEW_TexImport_Status_Nothing_Synced")));
            return;
        }

        Status = string.Join("\n", failures);

        Start([new Job(JobKind.ReplaceCells, twin.Container.FileEntry.Path, twin.Target, twin.Texture, -1, null, twin.Texture.Format,
            twin.Texture.Bytes, cells, newCells, plan.GrowWidth, plan.GrowHeight, twin.Atlas)]);
    }

    /// <summary>
    /// Places new cells in an atlas' free space (taken names are refused), growing the sheet (height, then width,
    /// doubling, up to 8192) when allowed and needed.
    /// </summary>
    private static (List<CellSource> Cells, List<NewCell> NewCells, int GrowWidth, int GrowHeight) PlanNewCells(
        TextureAtlas atlas, SubTexture template, List<(string Name, int W, int H, string Path)> items,
        int sheetWidth, int sheetHeight, bool allowGrow, List<string> failures)
    {
        var cells = new List<CellSource>();
        var newCells = new List<NewCell>();
        var occupied = atlas.SubTextures.Select(CellRect.Of).ToList();
        var taken = new HashSet<string>(atlas.SubTextures.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
        int w = sheetWidth, h = sheetHeight;

        foreach (var item in items)
        {
            if (item.W <= 0 || item.H <= 0)
            {
                failures.Add(LOC.Get("TEXVIEW_TexImport_Cell_No_Size", item.Name));
                continue;
            }

            if (!taken.Add(item.Name))
            {
                failures.Add(LOC.Get("TEXVIEW_TexImport_Cell_Exists", item.Name));
                continue;
            }

            var spot = LayoutGeometry.FindFreeSpot(occupied, item.W, item.H, w, h);

            while (spot == null && allowGrow && (w < 8192 || h < 8192))
            {
                if (h <= w && h < 8192) h *= 2; else w *= 2;
                spot = LayoutGeometry.FindFreeSpot(occupied, item.W, item.H, w, h);
            }

            if (spot == null)
            {
                failures.Add(LOC.Get("TEXVIEW_TexImport_Cell_No_Space", item.Name, item.W, item.H));
                continue;
            }

            occupied.Add(spot.Value);
            cells.Add(new CellSource(spot.Value, item.Name, item.Path));
            newCells.Add(new NewCell(atlas, template, item.Name, spot.Value));
        }

        bool grown = w != sheetWidth || h != sheetHeight;
        return (cells, newCells, grown ? w : 0, grown ? h : 0);
    }

    private static (int, int) DdsSize(string path)
    {
        try
        {
            var dds = new SoulsFormats.DDS(File.ReadAllBytes(path));
            return (dds.dwWidth, dds.dwHeight);
        }
        catch
        {
            return (0, 0);
        }
    }

    private bool PickFile(string title, out string path)
    {
        var ok = PlatformUtils.Instance.OpenFileDialog(title, TextureEncoder.FileDialogFilters, CFG.Current.TextureViewerImport_LastFolder, out path);

        if (ok && !string.IsNullOrEmpty(path))
        {
            CFG.Current.TextureViewerImport_LastFolder = Path.GetDirectoryName(path);
            return true;
        }

        return false;
    }
    #endregion

    #region Jobs
    private Job MakeReplaceJob(TextureTarget target, TPF.Texture tex, string path)
    {
        var info = TextureFormats.GetInfo(target.Tpf, tex);
        var request = MakeRequest(info);

        var formatByte = request.TargetFormat == info.Dxgi
            ? tex.Format
            : TextureFormats.SuggestFormatByte(Project.Descriptor.ProjectType, target.Tpf, request.TargetFormat, tex);

        return new Job(JobKind.Replace, path, target, tex, -1, request, formatByte);
    }

    private Job MakeAddJob(TextureTarget target, TPF.Texture reference, string path, int index)
    {
        // A new texture takes the chosen format, else the selected texture's, else BC7
        var info = reference != null ? TextureFormats.GetInfo(target.Tpf, reference) : null;
        if (info == null || !info.Readable)
            info = new TextureInfo(0, 0, 0, TextureEncoder.BC7_UNORM, true, TPF.TexType.Texture, true);

        var request = MakeRequest(info);

        // A new texture keeps its own size, and gets a full chain unless top level only was asked for
        request.ResizeWidth = 0;
        request.ResizeHeight = 0;
        if (CFG.Current.TextureViewerImport_MipMode == 0)
            request.MipLevels = 0;

        var formatByte = TextureFormats.SuggestFormatByte(Project.Descriptor.ProjectType, target.Tpf, request.TargetFormat);

        return new Job(JobKind.Add, path, target, null, index, request, formatByte);
    }

    /// <summary>The encode settings for replacing a texture described by info, from the tool's options.</summary>
    private static TextureEncodeRequest MakeRequest(TextureInfo info)
    {
        var target = CFG.Current.TextureViewerImport_Format == 0 ? info.Dxgi : CFG.Current.TextureViewerImport_Format;

        int mips = CFG.Current.TextureViewerImport_MipMode switch
        {
            1 => 0,
            2 => 1,
            // Match: a full chain stays a full chain at the new size, a partial one keeps its count
            _ => info.MipLevels == TextureEncoder.FullChain(info.Width, info.Height) ? 0 : Math.Max(1, info.MipLevels),
        };

        return new TextureEncodeRequest
        {
            TargetFormat = target,
            // The texture's header style is kept (old DXT1/DXT5 headers stay legacy)
            ForceDx10Header = info.Dx10Header,
            // Only when the format is kept: a legacy FourCC names the old format
            LegacyFourCC = target == info.Dxgi ? info.FourCC : null,
            MipLevels = mips,
            ResizeWidth = CFG.Current.TextureViewerImport_MatchSize ? info.Width : 0,
            ResizeHeight = CFG.Current.TextureViewerImport_MatchSize ? info.Height : 0,
        };
    }

    private void Start(List<Job> jobs)
    {
        Cancel?.Cancel();
        Cancel = new CancellationTokenSource();
        var token = Cancel.Token;

        ProgressDone = 0;
        ProgressTotal = jobs.Count;
        ProgressFile = "";
        Status = "";

        Running = Task.Run(() =>
        {
            var results = new List<JobResult>();

            foreach (var job in jobs)
            {
                if (token.IsCancellationRequested)
                    break;

                ProgressFile = Path.GetFileName(job.Path);

                TextureEncodeResult result;
                try
                {
                    if (job.Kind == JobKind.ReplaceCells)
                    {
                        result = RunCellJob(job);
                    }
                    else
                    {
                        var bytes = File.ReadAllBytes(job.Path);
                        result = TextureEncoder.Encode(bytes, Path.GetExtension(job.Path), job.Request);
                    }
                }
                catch (Exception e)
                {
                    result = new TextureEncodeResult { Error = e.Message };
                }

                results.Add(new JobResult(job, result));
                ProgressDone++;
            }

            // A cancelled run applies nothing, so a batch is all or nothing
            if (!token.IsCancellationRequested)
                Results.Enqueue(results);
        }, token);
    }

    /// <summary>
    /// A cell job on the task: grow the sheet if asked, then paste each image (a file, or a rectangle cut from
    /// another sheet) into its cell. New cells are added to the layout when the result is applied.
    /// </summary>
    private static TextureEncodeResult RunCellJob(Job job)
    {
        var sheet = job.SheetBytes;
        long ms = 0;

        if (job.GrowWidth > 0)
        {
            var grown = TextureCellPatcher.Grow(sheet, job.GrowWidth, job.GrowHeight);
            if (grown.Dds == null)
                return grown;
            sheet = grown.Dds;
            ms += grown.Milliseconds;
        }

        // Sheets to cut from, decoded once each
        var decoded = new Dictionary<byte[], SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>>(ReferenceEqualityComparer.Instance);

        try
        {
            var patches = new List<CellPatch>();

            foreach (var c in job.Cells)
            {
                if (c.FromSheet == null)
                {
                    patches.Add(new CellPatch(c.Name, c.Rect.X, c.Rect.Y, c.Rect.W, c.Rect.H, File.ReadAllBytes(c.Path), Path.GetExtension(c.Path)));
                    continue;
                }

                if (!decoded.TryGetValue(c.FromSheet, out var source))
                {
                    source = DdsDecoder.DecodeToImage(c.FromSheet, out var error);
                    if (source == null)
                        return new TextureEncodeResult { Error = error };
                    decoded[c.FromSheet] = source;
                }

                var from = c.FromRect;
                var rect = new SixLabors.ImageSharp.Rectangle(from.X, from.Y, from.W, from.H);
                rect.Intersect(new SixLabors.ImageSharp.Rectangle(0, 0, source.Width, source.Height));
                if (rect.Width <= 0 || rect.Height <= 0)
                    continue;

                using var crop = source.Clone(ctx => ctx.Crop(rect));
                using var png = new MemoryStream();
                crop.SaveAsPng(png);
                patches.Add(new CellPatch(c.Name, c.Rect.X, c.Rect.Y, c.Rect.W, c.Rect.H, png.ToArray(), ".png"));
            }

            var result = TextureCellPatcher.Patch(sheet, patches);
            result.Milliseconds += ms;
            return result;
        }
        finally
        {
            foreach (var img in decoded.Values)
                img.Dispose();
        }
    }

    /// <summary>
    /// Applies finished imports as one undoable action. Called every frame from the view.
    /// </summary>
    public void Update()
    {
        while (Results.TryDequeue(out var results))
        {
            Apply(results);
        }
    }

    private void Apply(List<JobResult> results)
    {
        var actions = new List<EditorAction>();
        var failed = new List<string>();
        int gpu = 0, kept = 0;
        long ms = 0;

        foreach (var (job, result) in results)
        {
            ms += result.Milliseconds;

            if (result.Dds == null)
            {
                failed.Add($"{Path.GetFileName(job.Path)}: {result.Error}");
                Smithbox.LogError(this, LOC.Get("TEXVIEW_TexImport_Log_Failed", job.Path, result.Error));
                continue;
            }

            if (result.UsedGpu) gpu++;
            if (result.PassedThrough) kept++;

            if (job.Kind == JobKind.ReplaceCells)
            {
                // The texture could have been removed or edited while the task ran
                if (!job.Target.Tpf.Textures.Contains(job.Texture) || job.Texture.Bytes != job.SheetBytes)
                {
                    failed.Add(LOC.Get("TEXVIEW_TexImport_Cells_Changed", job.Texture.Name));
                    continue;
                }

                // Same format, size and mips: only the cells' blocks changed
                var before = TextureState.Capture(job.Texture);
                var after = before.Clone();
                after.Bytes = result.Dds;

                actions.Add(new TextureChangeAction(View, job.Target, job.Texture, before, after));

                // A grown sheet: the atlas' own size attributes follow
                if (job.GrowWidth > 0 && job.GrowAtlas != null)
                {
                    if (job.GrowAtlas.Element.Get("width") != null)
                        actions.Add(new AtlasAttributeAction(job.GrowAtlas, "width", $"{job.GrowWidth}"));
                    if (job.GrowAtlas.Element.Get("height") != null)
                        actions.Add(new AtlasAttributeAction(job.GrowAtlas, "height", $"{job.GrowHeight}"));
                }

                // New cells, formatted like their template, after the existing ones
                foreach (var n in job.NewCells ?? new())
                {
                    var cell = n.Atlas.CreateLike(n.Template, n.Name, n.Rect.X, n.Rect.Y, n.Rect.W, n.Rect.H);
                    foreach (var (attr, value) in n.Attributes ?? new())
                    {
                        if (cell.Element.Get(attr) != null)
                            cell.Element.Set(attr, value);
                    }

                    var index = n.Atlas.SubTextures.Count + actions.OfType<LayoutCellInsertRemoveAction>().Count(a => a.Atlas == n.Atlas);
                    actions.Add(new LayoutCellInsertRemoveAction(View.LayoutEditor, n.Atlas, cell, index, true));
                }
            }
            else if (job.Kind == JobKind.Replace)
            {
                // The texture could have been removed while the task ran
                if (!job.Target.Tpf.Textures.Contains(job.Texture))
                    continue;

                var before = TextureState.Capture(job.Texture);
                var after = before.Clone();
                after.Bytes = result.Dds;
                after.Format = job.FormatByte;
                after.Mipmaps = (byte)Math.Max(1, result.MipLevels);

                actions.Add(new TextureChangeAction(View, job.Target, job.Texture, before, after));
            }
            else
            {
                var name = UniqueName(job.Target.Tpf, Path.GetFileNameWithoutExtension(job.Path), actions);
                var state = new TextureState
                {
                    Name = name,
                    Format = job.FormatByte,
                    Type = TPF.TexType.Texture,
                    Mipmaps = (byte)Math.Max(1, result.MipLevels),
                    Flags1 = 0,
                    Bytes = result.Dds,
                };

                actions.Add(new TextureInsertRemoveAction(View, job.Target, state.CreateTexture(job.Target.Tpf.Platform), job.InsertIndex, true));
            }
        }

        if (actions.Count == 1)
            View.ActionManager.ExecuteAction(actions[0]);
        else if (actions.Count > 1)
            View.ActionManager.ExecuteAction(new CompoundAction(actions));

        Status = LOC.Get("TEXVIEW_TexImport_Status_Done", actions.Count, results.Count, gpu, kept, ms);

        if (failed.Count > 0)
            Status += "\n" + string.Join("\n", failed);

        Smithbox.Log(this, LOC.Get("TEXVIEW_TexImport_Log_Done", actions.Count, results.Count, ms));
    }

    /// <summary>A texture name not yet used in the TPF (or by an add in the same batch).</summary>
    private static string UniqueName(TPF tpf, string name, List<EditorAction> pending)
    {
        var used = new HashSet<string>(tpf.Textures.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);
        foreach (var a in pending.OfType<TextureInsertRemoveAction>())
            used.Add(a.TextureName);

        if (!used.Contains(name))
            return name;

        for (int i = 1; ; i++)
        {
            var candidate = $"{name}_{i}";
            if (!used.Contains(candidate))
                return candidate;
        }
    }
    #endregion
}
