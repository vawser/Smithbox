using Hexa.NET.ImGui;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The Layout window (T5): which layout describes the selected sheet, the view options, the selected cell's fields,
/// add / delete / export, the grid generator, the checks and the cell list.
/// </summary>
public partial class LayoutEditor
{
    // Field buffers: refreshed from the cell unless the field was being edited last frame
    private string NameBuffer = "";
    private int XBuffer, YBuffer, WBuffer, HBuffer, OWBuffer, OHBuffer;
    private readonly bool[] FieldActive = new bool[7];
    private SubTexture BufferCell;

    private string CellFilter = "";
    private bool CellFilterExact;
    private bool ShowCellList = true;
    private bool ShowChecks = true;
    private bool ShowGrid = false;

    private readonly GridOptions Grid = new();

    private List<LayoutIssue> Issues = new();
    private int IssuesFrame = -1000;
    private TextureAtlas IssuesFor;

    private string Status = "";

    public void DisplayWindow()
    {
        Resolve();

        // Nothing to draw while the window has no area (mid re-dock, or a dock tab that isn't shown)
        var avail = ImGui.GetContentRegionAvail();
        if (avail.X <= 0 || avail.Y <= 0)
            return;

        if (!ImGui.BeginChild("TextureLayoutWindow", new Vector2(0, 0), ImGuiChildFlags.Borders))
        {
            ImGui.EndChild();
            return;
        }

        GUI.TitleHeader(LOC.Get("TEXVIEW_Layout_Header"), LOC.Get("TEXVIEW_Layout_Header_TT"));

        if (View.Selection.SelectedTexture == null)
        {
            GUI.WrappedText(LOC.Get("TEXVIEW_Layout_No_Texture"));
            ImGui.EndChild();
            return;
        }

        DisplaySource();

        if (Atlas != null)
        {
            DisplayOptions();
            DisplaySelection();

            if (IsEditable)
                DisplayGrid();

            DisplayChecks();
            DisplayCellList();
        }

        ImGui.EndChild();
    }

    #region Source
    private void DisplaySource()
    {
        // Layout
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_Layout_Source_Header"), LOC.Get("TEXVIEW_Layout_Source_Header_TT"));

        if (Atlas == null)
        {
            GUI.WrappedText(LOC.Get("TEXVIEW_Layout_None", View.Selection.SelectedTexture.Name));

            if (Container != null && Container.Layouts.Values.Any(l => !l.IsReadOnly))
            {
                GUI.MultiButtonInput("layoutCreate",
                    "create", LOC.Get("TEXVIEW_Layout_Create"), LOC.Get("TEXVIEW_Layout_Create_TT"),
                    () => CreateLayout());
            }
            else
            {
                GUI.WrappedText(LOC.Get("TEXVIEW_Layout_None_No_Binder"));
            }

            return;
        }

        var where = Container.IsBundled
            ? LOC.Get(Layout.SourcePath != null && Layout.SourcePath.StartsWith(ShoeboxLayoutContainer.GetProjectLayoutFolder(Project), StringComparison.OrdinalIgnoreCase)
                ? "TEXVIEW_Layout_Source_Project_Folder" : "TEXVIEW_Layout_Source_Bundled", Path.GetFileName(Layout.SourcePath ?? Layout.FileName))
            : LOC.Get("TEXVIEW_Layout_Source_Binder", Container.FileEntry.Path, Path.GetFileName(Layout.FileName.Replace('\\', '/')));

        GUI.WrappedText(where);
        GUI.WrappedText(LOC.Get("TEXVIEW_Layout_Source_Counts", Atlas.SubTextures.Count, SheetWidth, SheetHeight));

        if (Layout.IsModified)
            GUI.WrappedTextColored(UI.Current.ImGui_TextureEditor_ModifiedFile_Text, LOC.Get("TEXVIEW_Layout_Modified"));

        if (!IsEditable)
        {
            GUI.WrappedText(Layout.ParseError != null
                ? LOC.Get("TEXVIEW_Layout_Read_Only_Parse", Layout.ParseError)
                : LOC.Get("TEXVIEW_Layout_Read_Only_Generated"));
        }
    }
    #endregion

    #region Options
    private void DisplayOptions()
    {
        // Display
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_Layout_Display_Header"), LOC.Get("TEXVIEW_Layout_Display_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXVIEW_Layout_Show_Cells")}##layoutShowCells", ref CFG.Current.TextureViewerLayout_ShowCells);
        GUI.Tooltip(LOC.Get("TEXVIEW_Layout_Show_Cells_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXVIEW_Layout_Show_Names")}##layoutShowNames", ref CFG.Current.TextureViewerLayout_ShowNames);
        GUI.Tooltip(LOC.Get("TEXVIEW_Layout_Show_Names_TT"));

        if (!IsEditable)
            return;

        // Editing
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_Layout_Editing_Header"), LOC.Get("TEXVIEW_Layout_Editing_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TEXVIEW_Layout_Draw_Mode")}##layoutDrawMode", ref DrawMode);
        GUI.Tooltip(LOC.Get("TEXVIEW_Layout_Draw_Mode_TT"));

        GUI.WrappedText(LOC.Get("TEXVIEW_Layout_Snap"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##layoutSnap", ref CFG.Current.TextureViewerLayout_Snap);
        CFG.Current.TextureViewerLayout_Snap = Math.Clamp(CFG.Current.TextureViewerLayout_Snap, 1, 512);
        GUI.Tooltip(LOC.Get("TEXVIEW_Layout_Snap_TT"));
    }
    #endregion

    #region Selection
    private void DisplaySelection()
    {
        // Selected Cell
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_Layout_Selected_Header"), LOC.Get("TEXVIEW_Layout_Selected_Header_TT"));

        var cell = Primary;

        if (cell == null)
        {
            GUI.WrappedText(LOC.Get(IsEditable ? "TEXVIEW_Layout_Selected_None_Editable" : "TEXVIEW_Layout_Selected_None"));
        }
        else
        {
            if (Selected.Count > 1)
                GUI.WrappedText(LOC.Get("TEXVIEW_Layout_Selected_Count", Selected.Count));

            DisplayFields(cell);

            var aliases = LayoutGeometry.AliasGroup(Atlas, cell).Where(a => a != cell).ToList();
            if (aliases.Count > 0)
                GUI.WrappedText(LOC.Get("TEXVIEW_Layout_Aliases", string.Join(", ", aliases.Select(a => a.Name))));
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("TEXVIEW_Layout_Actions_Header"), LOC.Get("TEXVIEW_Layout_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("layoutActions",
            "addNext", LOC.Get("TEXVIEW_Layout_Add_Next"), LOC.Get("TEXVIEW_Layout_Add_Next_TT"),
            () => Status = AddNextSlot() ? "" : LOC.Get("TEXVIEW_Layout_Status_No_Slot"), IsEditable,

            "delete", LOC.Get("TEXVIEW_Layout_Delete"), LOC.Get("TEXVIEW_Layout_Delete_TT"),
            DeleteSelected, IsEditable && Selected.Count > 0,

            "export", LOC.Get("TEXVIEW_Layout_Export"), LOC.Get("TEXVIEW_Layout_Export_TT", CFG.Current.TextureViewerToolbar_ExportTextureLocation),
            () => Status = ExportSelected(), Selected.Count > 0);

        // The sheet's pixels under the cells (PC TPFs; the layout itself can be read-only)
        GUI.ConditionalMultiButtonInput("layoutPixelActions",
            "replaceCellImage", LOC.Get("TEXVIEW_Layout_Replace_Cell_Image"), LOC.Get("TEXVIEW_TexImport_Replace_Cell_TT"),
            View.TextureImport.ReplaceSelectedCellsFromDialog, Selected.Count > 0 && View.GetEditTarget() != null && !View.TextureImport.IsRunning,

            "replaceCellImages", LOC.Get("TEXVIEW_TexImport_Replace_Cells_Folder"), LOC.Get("TEXVIEW_TexImport_Replace_Cells_Folder_TT"),
            View.TextureImport.ReplaceCellsFromFolderDialog, View.GetEditTarget() != null && !View.TextureImport.IsRunning);

        if (Status != "")
            GUI.WrappedText(Status);
    }

    private void DisplayFields(SubTexture cell)
    {
        if (BufferCell != cell)
        {
            BufferCell = cell;
            Array.Clear(FieldActive);
        }

        if (!FieldActive[0]) NameBuffer = cell.Name;
        if (!FieldActive[1]) XBuffer = cell.XValue;
        if (!FieldActive[2]) YBuffer = cell.YValue;
        if (!FieldActive[3]) WBuffer = cell.WidthValue;
        if (!FieldActive[4]) HBuffer = cell.HeightValue;
        if (!FieldActive[5]) OWBuffer = int.TryParse(cell.OriginalWidth, out var ow) ? ow : 0;
        if (!FieldActive[6]) OHBuffer = int.TryParse(cell.OriginalHeight, out var oh) ? oh : 0;

        var flags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;
        if (!ImGui.BeginTable("layoutCellFields", 2, flags))
            return;

        ImGui.TableSetupColumn("Property", ImGuiTableColumnFlags.WidthStretch, 0.4f);
        ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

        var editable = IsEditable;

        // Name: the primary cell only (aliases keep their names)
        Row(LOC.Get("TEXVIEW_Layout_Field_Name"), LOC.Get("TEXVIEW_Layout_Field_Name_TT"));
        if (editable)
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##layoutName", ref NameBuffer, 255);
            FieldActive[0] = ImGui.IsItemActive();
            if (ImGui.IsItemDeactivatedAfterEdit() && NameBuffer.Trim() != "" && NameBuffer != cell.Name)
                Change([cell], c => c.Name = NameBuffer.Trim());
        }
        else ImGui.Text(cell.Name);

        // The rectangle: applied to every selected cell
        IntField(1, LOC.Get("TEXVIEW_Layout_Field_X"), ref XBuffer, editable, v => Change(Selected, c => c.X = $"{v}"));
        IntField(2, LOC.Get("TEXVIEW_Layout_Field_Y"), ref YBuffer, editable, v => Change(Selected, c => c.Y = $"{v}"));
        IntField(3, LOC.Get("TEXVIEW_Layout_Field_Width"), ref WBuffer, editable, v => Change(Selected, c => { c.Width = $"{Math.Max(1, v)}"; SyncOriginal(c); }));
        IntField(4, LOC.Get("TEXVIEW_Layout_Field_Height"), ref HBuffer, editable, v => Change(Selected, c => { c.Height = $"{Math.Max(1, v)}"; SyncOriginal(c); }));

        // Half: only on layouts that have the attribute
        if (cell.Has("half"))
        {
            Row(LOC.Get("TEXVIEW_Layout_Field_Half"), LOC.Get("TEXVIEW_Layout_Field_Half_TT"));
            var half = cell.IsHalf;
            if (editable)
            {
                if (ImGui.Checkbox("##layoutHalf", ref half))
                    Change(Selected, c => c.Half = half ? "1" : "0");
            }
            else ImGui.Text(half ? "1" : "0");
        }

        if (cell.Has("originalWidth"))
        {
            IntField(5, LOC.Get("TEXVIEW_Layout_Field_Original_Width"), ref OWBuffer, editable, v => Change(Selected, c => c.OriginalWidth = $"{v}"));
            IntField(6, LOC.Get("TEXVIEW_Layout_Field_Original_Height"), ref OHBuffer, editable, v => Change(Selected, c => c.OriginalHeight = $"{v}"));
        }

        ImGui.EndTable();
    }

    /// <summary>originalWidth/Height follow the size, except on half cells (where they are twice it).</summary>
    private static void SyncOriginal(SubTexture c)
    {
        if (c.Has("originalWidth") && !c.IsHalf)
        {
            c.OriginalWidth = c.Width;
            c.OriginalHeight = c.Height;
        }
    }

    private void IntField(int slot, string label, ref int value, bool editable, Action<int> commit)
    {
        Row(label, LOC.Get("TEXVIEW_Layout_Field_Rect_TT"));

        if (!editable)
        {
            ImGui.Text($"{value}");
            return;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt($"##layoutField{slot}", ref value);
        FieldActive[slot] = ImGui.IsItemActive();

        if (ImGui.IsItemDeactivatedAfterEdit())
            commit(value);
    }

    private static void Row(string label, string tooltip)
    {
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text(label);
        GUI.Tooltip(tooltip);
        ImGui.TableSetColumnIndex(1);
    }
    #endregion

    #region Grid
    private void DisplayGrid()
    {
        // Grid Generator
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("TEXVIEW_Layout_Grid_Header"), LOC.Get("TEXVIEW_Layout_Grid_Header_TT"), ref ShowGrid);

        if (!ShowGrid)
            return;

        if (ImGui.BeginTable("layoutGridFields", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("Property", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthStretch);

            GridInt("TEXVIEW_Layout_Grid_Origin_X", "TEXVIEW_Layout_Grid_Origin_TT", "gridOx", ref Grid.OriginX);
            GridInt("TEXVIEW_Layout_Grid_Origin_Y", "TEXVIEW_Layout_Grid_Origin_TT", "gridOy", ref Grid.OriginY);
            GridInt("TEXVIEW_Layout_Grid_Columns", "TEXVIEW_Layout_Grid_Count_TT", "gridCols", ref Grid.Columns);
            GridInt("TEXVIEW_Layout_Grid_Rows", "TEXVIEW_Layout_Grid_Count_TT", "gridRows", ref Grid.Rows);
            GridInt("TEXVIEW_Layout_Grid_Cell_Width", "TEXVIEW_Layout_Grid_Cell_TT", "gridCw", ref Grid.CellWidth);
            GridInt("TEXVIEW_Layout_Grid_Cell_Height", "TEXVIEW_Layout_Grid_Cell_TT", "gridCh", ref Grid.CellHeight);
            GridInt("TEXVIEW_Layout_Grid_Spacing", "TEXVIEW_Layout_Grid_Spacing_TT", "gridSp", ref Grid.Spacing);

            Row(LOC.Get("TEXVIEW_Layout_Grid_Prefix"), LOC.Get("TEXVIEW_Layout_Grid_Name_TT"));
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##gridPrefix", ref Grid.Prefix, 128);
            GUI.Tooltip(LOC.Get("TEXVIEW_Layout_Grid_Name_TT"));

            Row(LOC.Get("TEXVIEW_Layout_Grid_Suffix"), LOC.Get("TEXVIEW_Layout_Grid_Name_TT"));
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##gridSuffix", ref Grid.Suffix, 32);
            GUI.Tooltip(LOC.Get("TEXVIEW_Layout_Grid_Name_TT"));

            GridInt("TEXVIEW_Layout_Grid_Digits", "TEXVIEW_Layout_Grid_Name_TT", "gridDigits", ref Grid.Digits);

            var start = (int)Grid.StartId;
            GridInt("TEXVIEW_Layout_Grid_Start_Id", "TEXVIEW_Layout_Grid_Name_TT", "gridStart", ref start);
            Grid.StartId = Math.Max(0, start);

            ImGui.EndTable();
        }

        ImGui.Checkbox($"{LOC.Get("TEXVIEW_Layout_Grid_Skip_Occupied")}##gridSkip", ref Grid.SkipOccupied);
        GUI.Tooltip(LOC.Get("TEXVIEW_Layout_Grid_Skip_Occupied_TT"));

        Grid.Columns = Math.Clamp(Grid.Columns, 1, 256);
        Grid.Rows = Math.Clamp(Grid.Rows, 1, 256);
        Grid.CellWidth = Math.Max(1, Grid.CellWidth);
        Grid.CellHeight = Math.Max(1, Grid.CellHeight);
        Grid.Digits = Math.Clamp(Grid.Digits, 1, 12);

        GUI.MultiButtonInput("layoutGridActions",
            "fromSelected", LOC.Get("TEXVIEW_Layout_Grid_From_Selected"), LOC.Get("TEXVIEW_Layout_Grid_From_Selected_TT"),
            GridFromSelected,

            "generate", LOC.Get("TEXVIEW_Layout_Grid_Generate"), LOC.Get("TEXVIEW_Layout_Grid_Generate_TT"),
            () => Status = LOC.Get("TEXVIEW_Layout_Status_Generated", Generate(Grid)));
    }

    private static void GridInt(string labelKey, string tooltipKey, string id, ref int value)
    {
        Row(LOC.Get(labelKey), LOC.Get(tooltipKey));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt($"##{id}", ref value);
        GUI.Tooltip(LOC.Get(tooltipKey));
    }

    /// <summary>Fills the grid inputs from the selected (or last) cell: its size, position, and name pattern.</summary>
    private void GridFromSelected()
    {
        var cell = Primary ?? Atlas.SubTextures.LastOrDefault();
        if (cell == null)
            return;

        var r = CellRect.Of(cell);
        Grid.OriginX = r.X; Grid.OriginY = r.Y;
        Grid.CellWidth = r.W; Grid.CellHeight = r.H;

        var m = System.Text.RegularExpressions.Regex.Match(cell.Name, @"^(.*?)(\d+)(\D*)$");
        if (m.Success)
        {
            Grid.Prefix = m.Groups[1].Value;
            Grid.Digits = m.Groups[2].Value.Length;
            Grid.Suffix = m.Groups[3].Value;

            var next = LayoutGeometry.NextName(Atlas, cell);
            var nm = System.Text.RegularExpressions.Regex.Match(next, @"^(.*?)(\d+)(\D*)$");
            if (nm.Success && long.TryParse(nm.Groups[2].Value, out var id))
                Grid.StartId = id;
        }
    }
    #endregion

    #region Checks
    private void DisplayChecks()
    {
        // Recomputed every half second or when the atlas changes (partial overlaps are pairwise)
        if (IssuesFor != Atlas || ImGui.GetFrameCount() - IssuesFrame > 30)
        {
            Issues = LayoutGeometry.Check(Atlas, SheetWidth, SheetHeight);
            IssuesFor = Atlas;
            IssuesFrame = ImGui.GetFrameCount();
        }

        // Checks
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("TEXVIEW_Layout_Checks_Header", Issues.Count), LOC.Get("TEXVIEW_Layout_Checks_Header_TT"), ref ShowChecks);

        if (Issues.Count == 0)
        {
            GUI.WrappedText(LOC.Get("TEXVIEW_Layout_Checks_None"));
            return;
        }

        if (!ShowChecks)
            return;

        if (!ImGui.BeginChild("layoutChecks", new Vector2(0, Math.Min(Issues.Count, 8) * ImGui.GetTextLineHeightWithSpacing() + 8), ImGuiChildFlags.Borders))
        {
            ImGui.EndChild();
            return;
        }

        for (int i = 0; i < Issues.Count; i++)
        {
            var issue = Issues[i];
            if (ImGui.Selectable($"{LOC.Get($"TEXVIEW_Layout_Check_{issue.Kind}")}: {issue.Message}##issue{i}"))
            {
                if (issue.Cells.Count > 0)
                {
                    Select(issue.Cells);
                    ScrollTo = issue.Cells[0];
                }
            }
        }

        ImGui.EndChild();
    }
    #endregion

    #region Cell List
    private void DisplayCellList()
    {
        // Cells
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("TEXVIEW_Layout_Cells_Header"), LOC.Get("TEXVIEW_Layout_Cells_Header_TT"), ref ShowCellList);

        if (!ShowCellList)
            return;

        EditorFilters.DisplayFramedListFilter("layoutCellFilter", ref CellFilter, ref CellFilterExact);

        var cells = Atlas.SubTextures.Where(c => EditorFilters.IsMatch(CellFilter, c.Name, CellFilterExact)).ToList();

        // Only list when the child is drawn: while the window is re-docked or a hidden dock tab, the clipper's
        // first row doesn't advance the cursor and ImGui asserts "Unable to calculate item height"
        if (!ImGui.BeginChild("layoutCellList", new Vector2(0, 0), ImGuiChildFlags.Borders) || cells.Count == 0)
        {
            ImGui.EndChild();
            return;
        }

        // A known row height, so the clipper never has to measure the first row
        var clipper = new ImGuiListClipper();
        clipper.Begin(cells.Count, ImGui.GetTextLineHeightWithSpacing());

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var c = cells[i];
                var r = CellRect.Of(c);

                if (ImGui.Selectable($"{c.Name}  ({r.X}, {r.Y}  {r.W}x{r.H}){(c.IsHalf ? "  ½" : "")}##cell{i}", Selected.Contains(c)))
                {
                    if (ImGui.GetIO().KeyShift)
                    {
                        if (!Selected.Remove(c)) Selected.Add(c);
                    }
                    else
                    {
                        Select([c]);
                    }

                    ScrollTo = c;
                }
            }
        }

        clipper.End();
        ImGui.EndChild();
    }
    #endregion

    #region Export
    /// <summary>Crops each selected cell from the sheet into a PNG in the export folder (the TODO's "export icon").</summary>
    private string ExportSelected()
    {
        var folder = CFG.Current.TextureViewerToolbar_ExportTextureLocation;
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
            return LOC.Get("TEXVIEW_TexExport_Log_Export_Dir_Not_Set");

        var tex = View.Selection.SelectedTexture;
        using var sheet = DdsDecoder.DecodeToImage(TexUtils.GetDdsBytes(View.Selection.SelectedTpf, tex), out var error);
        if (sheet == null)
            return error;

        int count = 0;
        foreach (var c in Selected)
        {
            var r = CellRect.Of(c);
            var rect = new Rectangle(r.X, r.Y, r.W, r.H);
            rect.Intersect(new Rectangle(0, 0, sheet.Width, sheet.Height));
            if (rect.Width <= 0 || rect.Height <= 0)
                continue;

            using var icon = sheet.Clone(ctx => ctx.Crop(rect));
            var name = Path.GetFileNameWithoutExtension(c.Name);
            icon.SaveAsPng(Path.Combine(folder, $"{name}.png"));
            count++;
        }

        return LOC.Get("TEXVIEW_Layout_Status_Exported", count, folder);
    }
    #endregion
}
