using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Creates and edits the boxes of a map's .mfr ("Multi Forbidden Region") file (ER): where the game won't save the
/// player's position (Save Limit) or limits multiplayer (Multiplay Limit). The boxes are in map space and drawn by
/// <see cref="MapForbiddenRegionTool"/>. Every change swaps the map's whole document through an
/// <see cref="MfrEditAction"/>, recomputing the boxes' quadtree cells; the map's save writes the file.
///
/// The viewport side (picking boxes, the gizmo, placing a box on a surface) is in MfrEditorTool.Viewport.
/// See Claude/Documentation/MFR Format.md.
/// </summary>
public partial class MfrEditorTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    // The map edited, and the selected box's index in its document
    private MapContainer CurrentMap;
    private int SelectedIndex = -1;

    // The selected box's fields as typed, written back as one action when an input is left after a change. Taken
    // from the document whenever no input is being edited.
    private MFR.Entry Buffer;
    private Vector3 BufferEuler;
    private int BufferVersion = -1;

    // Box list
    private string ListFilter = "";
    private int TypeFilter; // 0 all, 1 Save Limit, 2 Multiplay Limit
    private int ScrollTarget = -1;

    // The type new boxes get
    private MFR.EntryType NewType = MFR.EntryType.SaveLimit;

    public MfrEditorTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        GizmoTarget = new MfrGizmoTarget(this);
    }

    public bool CanUse() => View.MfrBank.CanUse();

    public void OnToolWindow()
    {
        ImGui.BeginChild("forbiddenRegionEditorSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        LastDrawnFrame = ImGui.GetFrameCount();

        if (!CanUse())
        {
            GUI.WrappedText(LOC.Get("MAP_MfrEditor_Unsupported_Game"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_MfrEditor_Hint"));

        // Display
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MfrEditor_Display_Header"), LOC.Get("MAP_MfrEditor_Display_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_MfrEditor_Show_Boxes")}##mfrShowBoxes", ref CFG.Current.MapEditor_Display_Forbidden_Regions);
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_Show_Boxes_TT"));

        MapForbiddenRegionTool.DisplayOptions();

        DisplayViewportEditing();

        // Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MfrEditor_Map_Header"), LOC.Get("MAP_MfrEditor_Map_Header_TT"));

        var maps = GetDrawnMaps().OrderBy(m => m.Name).ToList();

        // The map of the selected object, when it changes
        UpdateMapFromSelection();

        if (CurrentMap != null && !maps.Contains(CurrentMap))
            SetCurrentMap(null);

        if (CurrentMap == null && maps.Count > 0)
            SetCurrentMap(maps[0]);

        if (maps.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_MfrEditor_No_Maps"));
            return;
        }

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##mfrMap", CurrentMap?.Name ?? ""))
        {
            foreach (var map in maps)
            {
                var alias = AliasHelper.GetMapNameAlias(Project, map.Name);
                var label = string.IsNullOrEmpty(alias) ? map.Name : $"{map.Name}: {alias}";

                if (ImGui.Selectable($"{label}##mfrMap_{map.Name}", map == CurrentMap))
                    SetCurrentMap(map);
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_Map_TT"));

        var document = View.MfrBank.Get(CurrentMap);

        if (document == null)
        {
            GUI.WrappedText(LOC.Get("MAP_MfrEditor_No_File", CurrentMap.Name));

            GUI.MultiButtonInput("mfrCreateFile",
                "mfrCreateFileButton", LOC.Get("MAP_MfrEditor_Create_File"), LOC.Get("MAP_MfrEditor_Create_File_TT"),
                () => CreateFile(CurrentMap));
            return;
        }

        var saves = document.Entries.Count(e => e.Type == MFR.EntryType.SaveLimit);
        var multis = document.Entries.Count(e => e.Type == MFR.EntryType.MultiplayLimit);

        GUI.WrappedText(LOC.Get("MAP_MfrEditor_File_Info", document.Entries.Count, saves, multis));
        GUI.WrappedText(LOC.Get("MAP_MfrEditor_Grid_Info", document.GridDepth, document.GridSize,
            document.Origin.X, document.Origin.Z));

        if (View.MfrBank.IsModified(CurrentMap))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("MAP_MfrEditor_Modified"));
        }

        var outside = document.Entries.Count(e => !IsInsideGrid(document, e));
        if (outside > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_MfrEditor_Outside_Grid", outside));
        }

        if (SelectedIndex >= document.Entries.Count)
            Select(document.Entries.Count - 1);

        DisplayPicked();
        DisplayList(document);
        DisplayFields(document);
        DisplayActions(document);
    }

    #region Map

    /// <summary>
    /// The loaded maps drawn in this view, which the overlay draws boxes for.
    /// </summary>
    public IEnumerable<MapContainer> GetDrawnMaps()
    {
        foreach (var wrapper in Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            var map = wrapper.MapContainer;

            if (map == null || map.LoadState is not MapContentLoadState.Loaded || map.RootObject == null)
                continue;

            // A map is drawn in the scene of the view that loaded it
            if (map.OwnerView != null && map.OwnerView != View)
                continue;

            yield return map;
        }
    }

    private MapContainer LastSelectionMap;

    private void UpdateMapFromSelection()
    {
        var selected = View.ViewportSelection.GetSelection()
            .OfType<Entity>()
            .Select(e => e.Container as MapContainer)
            .FirstOrDefault(m => m != null);

        if (selected != LastSelectionMap)
        {
            LastSelectionMap = selected;

            if (selected != null && selected != CurrentMap)
                SetCurrentMap(selected);
        }
    }

    private void SetCurrentMap(MapContainer map)
    {
        if (map == CurrentMap)
            return;

        CurrentMap = map;
        SelectedIndex = -1;
        Buffer = null;
    }

    private void Select(int index)
    {
        SelectedIndex = index;
        Buffer = null;
    }

    /// <summary>The editor's selected and hovered box of a map, or -1, for the overlay.</summary>
    public void GetHighlight(MapContainer map, out int selected, out int hovered)
    {
        var shown = ImGui.GetFrameCount() - LastDrawnFrame <= 2;

        selected = shown && map == CurrentMap ? SelectedIndex : -1;
        hovered = IsEditing && map == HoveredMap ? HoveredIndex : -1;
    }

    private void CreateFile(MapContainer map)
    {
        // Overworld tiles ship with this grid when empty: 256 m centred on the tile. Other maps get it fitted to
        // their boxes later (Fit Grid to Boxes).
        var document = new MFR();

        Commit(map, null, document, LOC.Get("MAP_MfrEditor_Action_Create_File", map.Name));
    }

    #endregion

    #region List

    private void DisplayList(MFR document)
    {
        // Boxes
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MfrEditor_List_Header"), LOC.Get("MAP_MfrEditor_List_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        var types = new[] { LOC.Get("MAP_MfrEditor_Type_All"), LOC.Get("MAP_Forbidden_Region_Save_Limit"), LOC.Get("MAP_Forbidden_Region_Multiplay_Limit") };
        ImGui.Combo("##mfrTypeFilter", ref TypeFilter, types, types.Length);
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_Type_Filter_TT"));

        var exact = false;
        EditorFilters.DisplayFramedListFilter("mfrListFilter", ref ListFilter, ref exact);

        var rows = new List<int>();
        for (var i = 0; i < document.Entries.Count; i++)
        {
            var e = document.Entries[i];

            if (TypeFilter == 1 && e.Type != MFR.EntryType.SaveLimit) continue;
            if (TypeFilter == 2 && e.Type != MFR.EntryType.MultiplayLimit) continue;
            if (!string.IsNullOrEmpty(ListFilter) && !EditorFilters.IsMatch(ListFilter, RowText(i, e), exact)) continue;

            rows.Add(i);
        }

        if (ScrollTarget >= 0 && !rows.Contains(ScrollTarget))
            ScrollTarget = -1;

        ImGui.BeginChild("mfrBoxList", new Vector2(0, 220 * DPI.UIScale()), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(rows.Count);

        if (ScrollTarget >= 0)
            clipper.IncludeItemByIndex(rows.IndexOf(ScrollTarget));

        while (clipper.Step())
        {
            for (var r = clipper.DisplayStart; r < clipper.DisplayEnd; r++)
            {
                var i = rows[r];
                var e = document.Entries[i];

                if (ImGui.Selectable($"{RowText(i, e)}##mfrRow{i}", i == SelectedIndex, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    Select(i);

                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        Frame(CurrentMap, e);
                }

                if (i == ScrollTarget)
                {
                    ImGui.SetScrollHereY();
                    ScrollTarget = -1;
                }
            }
        }

        clipper.End();
        ImGui.EndChild();
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_List_TT"));
    }

    private static string RowText(int index, MFR.Entry e)
    {
        var type = e.Type == MFR.EntryType.MultiplayLimit ? LOC.Get("MAP_Forbidden_Region_Multiplay_Limit") : LOC.Get("MAP_Forbidden_Region_Save_Limit");
        var c = e.Center;

        return $"{index}: {type}  ({c.X:0.#}, {c.Y:0.#}, {c.Z:0.#})";
    }

    #endregion

    #region Fields

    private void DisplayFields(MFR document)
    {
        if (SelectedIndex < 0 || SelectedIndex >= document.Entries.Count)
            return;

        var entry = document.Entries[SelectedIndex];
        var version = View.MfrBank.GetVersion(CurrentMap);

        // Retake the fields from the document unless one is being typed in
        if (Buffer == null || (!ImGui.IsAnyItemActive() && version != BufferVersion))
        {
            Buffer = MfrBoxMath.Copy(entry);
            BufferEuler = GetEuler(entry);
            BufferVersion = version;
        }

        // Selected Box
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MfrEditor_Fields_Header", SelectedIndex), LOC.Get("MAP_MfrEditor_Fields_Header_TT"));

        var changed = false;

        // Type
        var type = Buffer.Type == MFR.EntryType.MultiplayLimit ? 1 : 0;
        var typeNames = new[] { LOC.Get("MAP_Forbidden_Region_Save_Limit"), LOC.Get("MAP_Forbidden_Region_Multiplay_Limit") };
        ImGui.SetNextItemWidth(-1);
        if (ImGui.Combo("##mfrType", ref type, typeNames, typeNames.Length))
        {
            Buffer.Type = type == 1 ? MFR.EntryType.MultiplayLimit : MFR.EntryType.SaveLimit;
            changed = true;
        }
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_Type_TT"));

        // Centre
        ImGui.TextUnformatted(LOC.Get("MAP_MfrEditor_Center"));
        var center = Buffer.Center;
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##mfrCenter", ref center);
        Buffer.Center = center;
        changed |= ImGui.IsItemDeactivatedAfterEdit();
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_Center_TT"));

        // Size (full, not half: like a box region's width, height and depth)
        ImGui.TextUnformatted(LOC.Get("MAP_MfrEditor_Size"));
        var size = Buffer.HalfExtents * 2;
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##mfrSize", ref size);
        Buffer.HalfExtents = Vector3.Max(size / 2, new Vector3(0.005f));
        changed |= ImGui.IsItemDeactivatedAfterEdit();
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_Size_TT"));

        // Rotation
        ImGui.TextUnformatted(LOC.Get("MAP_MfrEditor_Rotation"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##mfrRotation", ref BufferEuler);
        var rotationChanged = ImGui.IsItemDeactivatedAfterEdit();
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_Rotation_TT"));

        if (rotationChanged)
        {
            SetEuler(Buffer, BufferEuler);
            changed = true;
        }

        if (MfrBoxMath.IsMirrored(entry))
        {
            GUI.WrappedText(LOC.Get("MAP_MfrEditor_Mirrored"));
        }

        GUI.WrappedText(LOC.Get("MAP_MfrEditor_Cell", entry.Cell));

        if (changed)
        {
            var index = SelectedIndex;
            var edited = MfrBoxMath.Copy(Buffer);

            Edit(LOC.Get("MAP_MfrEditor_Action_Edit", index, CurrentMap.Name), d => d.Entries[index] = edited);
        }
    }

    // The same rotation convention as MSB entries (degrees, Smithbox's XZY order), so an asset's rotation can be typed in
    private static Vector3 GetEuler(MFR.Entry entry)
    {
        var t = new Transform(Vector3.Zero, MfrBoxMath.GetRotation(entry), Vector3.One);
        return t.EulerRotationXZY * (180f / MathF.PI);
    }

    private static void SetEuler(MFR.Entry entry, Vector3 degrees)
    {
        var mirrored = MfrBoxMath.IsMirrored(entry);
        var t = new Transform(Vector3.Zero, Quaternion.Identity, Vector3.One) { EulerRotationXZY = degrees * (MathF.PI / 180f) };
        MfrBoxMath.SetRotation(entry, t.Rotation, mirrored);
    }

    #endregion

    #region Actions

    private void DisplayActions(MFR document)
    {
        var hasSelection = SelectedIndex >= 0 && SelectedIndex < document.Entries.Count;

        // New Boxes
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MfrEditor_Create_Header"), LOC.Get("MAP_MfrEditor_Create_Header_TT"));

        var type = NewType == MFR.EntryType.MultiplayLimit ? 1 : 0;
        var typeNames = new[] { LOC.Get("MAP_Forbidden_Region_Save_Limit"), LOC.Get("MAP_Forbidden_Region_Multiplay_Limit") };
        ImGui.SetNextItemWidth(-1);
        if (ImGui.Combo("##mfrNewType", ref type, typeNames, typeNames.Length))
            NewType = type == 1 ? MFR.EntryType.MultiplayLimit : MFR.EntryType.SaveLimit;
        GUI.Tooltip(LOC.Get("MAP_MfrEditor_New_Type_TT"));

        var hasObjects = GetSelectionBounds(CurrentMap, out _, out _);

        GUI.ConditionalMultiButtonInput("mfrCreateActions",
            "mfrPlace", LOC.Get(Placing ? "MAP_MfrEditor_Place_Cancel" : "MAP_MfrEditor_Place"), LOC.Get("MAP_MfrEditor_Place_TT"),
            TogglePlacing, true,

            "mfrAddFront", LOC.Get("MAP_MfrEditor_Add_In_Front"), LOC.Get("MAP_MfrEditor_Add_In_Front_TT"),
            AddInFront, true,

            "mfrFitSelection", LOC.Get("MAP_MfrEditor_Fit_Selection"), LOC.Get("MAP_MfrEditor_Fit_Selection_TT"),
            AddAroundSelection, hasObjects);

        // Selected Box
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MfrEditor_Edit_Header"), LOC.Get("MAP_MfrEditor_Edit_Header_TT"));

        GUI.ConditionalMultiButtonInput("mfrEditActions",
            "mfrFrame", LOC.Get("MAP_MfrEditor_Frame"), LOC.Get("MAP_MfrEditor_Frame_TT"),
            () => Frame(CurrentMap, document.Entries[SelectedIndex]), hasSelection,

            "mfrDuplicate", LOC.Get("MAP_MfrEditor_Duplicate"), LOC.Get("MAP_MfrEditor_Duplicate_TT"),
            DuplicateSelected, hasSelection,

            "mfrDelete", LOC.Get("MAP_MfrEditor_Delete"), LOC.Get("MAP_MfrEditor_Delete_TT"),
            DeleteSelected, hasSelection,

            "mfrResetRotation", LOC.Get("MAP_MfrEditor_Reset_Rotation"), LOC.Get("MAP_MfrEditor_Reset_Rotation_TT"),
            ResetRotation, hasSelection);

        // Grid
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MfrEditor_Grid_Header"), LOC.Get("MAP_MfrEditor_Grid_Header_TT"));

        GUI.ConditionalMultiButtonInput("mfrGridActions",
            "mfrFitGrid", LOC.Get("MAP_MfrEditor_Fit_Grid"), LOC.Get("MAP_MfrEditor_Fit_Grid_TT"),
            FitGrid, document.Entries.Count > 0);
    }

    /// <summary>
    /// Records a change to the current map's document as one action: the change is made on a copy, the cells are
    /// recomputed, and the action swaps it in.
    /// </summary>
    private void Edit(string description, Action<MFR> change)
    {
        var map = CurrentMap;
        var before = View.MfrBank.Get(map);

        if (map == null || before == null)
            return;

        var after = MfrBank.Copy(before);
        change(after);

        Commit(map, before, after, description);
    }

    private void Commit(MapContainer map, MFR before, MFR after, string description)
    {
        after.UpdateCells();

        View.ViewportActionManager.ExecuteAction(new MfrEditAction(View.MfrBank, map, before, after, description));
        Buffer = null;
    }

    /// <summary>Adds a box to the current map's document and selects it.</summary>
    private void AddBox(MFR.Entry entry, string description)
    {
        var map = CurrentMap;
        var document = View.MfrBank.Get(map);

        if (map == null)
            return;

        if (document == null)
        {
            // A map without a file gets one with its first box
            var created = new MFR();
            created.Entries.Add(entry);
            Commit(map, null, created, description);
        }
        else
        {
            Edit(description, d => d.Entries.Add(entry));
        }

        var count = View.MfrBank.Get(map)?.Entries.Count ?? 0;
        Select(count - 1);
        ScrollTarget = count - 1;
    }

    private void AddInFront()
    {
        if (View.ViewportHandler.ActiveViewport?.Viewport is not VulkanViewport viewport || CurrentMap == null)
            return;

        var ray = viewport.GetRay(viewport.Width / 2f, viewport.Height / 2f);
        var world = ray.Origin + Vector3.Normalize(ray.Direction) * 10f;

        var entry = new MFR.Entry { Type = NewType, HalfExtents = new Vector3(2), Center = ToMap(CurrentMap, world) };
        AddBox(entry, LOC.Get("MAP_MfrEditor_Action_Add", CurrentMap.Name));
    }

    private void AddAroundSelection()
    {
        if (!GetSelectionBounds(CurrentMap, out var min, out var max))
            return;

        var entry = MfrBoxMath.FromBounds(min, max, NewType);
        AddBox(entry, LOC.Get("MAP_MfrEditor_Action_Add_Around", CurrentMap.Name));
    }

    /// <summary>
    /// The map-space bounds of the selected objects of the map that have something drawn.
    /// </summary>
    private bool GetSelectionBounds(MapContainer map, out Vector3 min, out Vector3 max)
    {
        min = new Vector3(float.MaxValue);
        max = new Vector3(float.MinValue);

        if (map?.RootObject == null)
            return false;

        Matrix4x4.Invert(map.RootObject.GetLocalTransform().WorldMatrix, out var toMap);
        var any = false;

        foreach (var ent in View.ViewportSelection.GetSelection().OfType<Entity>())
        {
            if (ent.Container != map || ent.RenderSceneMesh == null)
                continue;

            var box = ent.RenderSceneMesh.GetBounds();

            if (!float.IsFinite(box.Min.X) || !float.IsFinite(box.Max.X))
                continue;

            for (var c = 0; c < 8; c++)
            {
                var corner = new Vector3((c & 1) == 0 ? box.Min.X : box.Max.X, (c & 2) == 0 ? box.Min.Y : box.Max.Y, (c & 4) == 0 ? box.Min.Z : box.Max.Z);
                var p = Vector3.Transform(corner, toMap);
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }

            any = true;
        }

        return any;
    }

    private void DuplicateSelected()
    {
        var index = SelectedIndex;
        var source = View.MfrBank.Get(CurrentMap)?.Entries.ElementAtOrDefault(index);

        if (source == null)
            return;

        // Beside the original, so the copy can be told apart
        var copy = MfrBoxMath.Copy(source);
        copy.Center += source.AxisX * (source.HalfExtents.X * 2 + 0.5f);

        AddBox(copy, LOC.Get("MAP_MfrEditor_Action_Duplicate", index, CurrentMap.Name));
    }

    private void DeleteSelected()
    {
        var index = SelectedIndex;

        Edit(LOC.Get("MAP_MfrEditor_Action_Delete", index, CurrentMap.Name), d => d.Entries.RemoveAt(index));
        Select(Math.Min(index, (View.MfrBank.Get(CurrentMap)?.Entries.Count ?? 0) - 1));
    }

    private void ResetRotation()
    {
        var index = SelectedIndex;

        Edit(LOC.Get("MAP_MfrEditor_Action_Edit", index, CurrentMap.Name), d =>
        {
            var e = d.Entries[index];
            e.AxisX = Vector3.UnitX;
            e.AxisY = Vector3.UnitY;
            e.AxisZ = Vector3.UnitZ;
        });
    }

    /// <summary>
    /// Centres the quadtree on the boxes' XZ footprint and sizes it to hold them all, then recomputes the cells.
    /// </summary>
    private void FitGrid()
    {
        Edit(LOC.Get("MAP_MfrEditor_Action_Fit_Grid", CurrentMap.Name), d =>
        {
            var min = new Vector3(float.MaxValue);
            var max = new Vector3(float.MinValue);

            foreach (var e in d.Entries)
            {
                var (lo, hi) = GetFootprint(e);
                min = Vector3.Min(min, lo);
                max = Vector3.Max(max, hi);
            }

            var centre = (min + max) / 2;
            var half = MathF.Max(max.X - min.X, max.Z - min.Z) / 2;

            d.Origin = new Vector3(centre.X, centre.Y, centre.Z);
            d.GridSize = MathF.Ceiling(MathF.Max(half * 1.01f, 1f));
        });
    }

    /// <summary>The box's map-space bounds.</summary>
    private static (Vector3 Min, Vector3 Max) GetFootprint(MFR.Entry e)
    {
        var h = e.HalfExtents;
        var extent = new Vector3(
            MathF.Abs(e.AxisX.X) * h.X + MathF.Abs(e.AxisY.X) * h.Y + MathF.Abs(e.AxisZ.X) * h.Z,
            MathF.Abs(e.AxisX.Y) * h.X + MathF.Abs(e.AxisY.Y) * h.Y + MathF.Abs(e.AxisZ.Y) * h.Z,
            MathF.Abs(e.AxisX.Z) * h.X + MathF.Abs(e.AxisY.Z) * h.Y + MathF.Abs(e.AxisZ.Z) * h.Z);

        return (e.Center - extent, e.Center + extent);
    }

    /// <summary>
    /// False if the box's footprint reaches past the quadtree's square. The game clamps it to an edge cell, so it is
    /// still filed, but in a cell that covers less than the box.
    /// </summary>
    private static bool IsInsideGrid(MFR document, MFR.Entry e)
    {
        var (min, max) = GetFootprint(e);
        var o = document.Origin;
        var s = document.GridSize;

        return min.X >= o.X - s && max.X <= o.X + s && min.Z >= o.Z - s && max.Z <= o.Z + s;
    }

    private void Frame(MapContainer map, MFR.Entry entry)
    {
        if (View.ViewportHandler.ActiveViewport?.Viewport is not { } viewport || map?.RootObject == null)
            return;

        var root = map.RootObject.GetLocalTransform().WorldMatrix;
        var (min, max) = GetFootprint(entry);

        var a = Vector3.Transform(min, root);
        var b = Vector3.Transform(max, root);

        viewport.FrameBox(new BoundingBox(Vector3.Min(a, b), Vector3.Max(a, b)), CFG.Current.Viewport_Frame_Offset, CFG.Current.Viewport_Frame_Distance);
    }

    private static Vector3 ToMap(MapContainer map, Vector3 world)
    {
        Matrix4x4.Invert(map.RootObject.GetLocalTransform().WorldMatrix, out var toMap);
        return Vector3.Transform(world, toMap);
    }

    #endregion
}
