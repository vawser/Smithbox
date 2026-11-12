using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Edit tab's Remove Face tool, in both modes: the face under the cursor is outlined, and a click removes it as one
/// undoable action (NavFaceRemove for the Havok games, NvmFaceRemove for the NVM ones); the map's save writes it.
/// </summary>
public partial class NavigationTool
{
    // The face under the cursor: in a Havok working copy or an NVM one
    private NavVertexFile FaceRemoveFile;
    private NvmVertexCopy FaceRemoveNvm;
    private int FaceRemoveIndex = -1;

    /// <summary>The hovered face's corners in the viewport, drawn as its outline.</summary>
    private readonly List<Vector3> FaceRemoveOutline = new();

    private void ClearFaceRemoveHover()
    {
        FaceRemoveFile = null;
        FaceRemoveNvm = null;
        FaceRemoveIndex = -1;
        FaceRemoveOutline.Clear();
    }

    #region Tab

    /// <summary>The groups of the Add Face or Remove Face tool.</summary>
    private void DisplayFaceToolOptions(bool active)
    {
        if (IsFaceTool)
            DisplayFaceAddOptions(active);
        else if (IsRemoveTool)
            DisplayFaceRemoveOptions(active);
    }

    private void DisplayFaceRemoveOptions(bool active)
    {
        // Face
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Face_Remove_Header"), LOC.Get("MAP_Navigation_Face_Remove_Header_TT"));

        if (!active)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Edit_Cursor_Inactive"));
        else if (FaceRemoveIndex >= 0 && (FaceRemoveFile?.File.Name ?? FaceRemoveNvm?.Room.FileName) is string name)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Remove_Hover", FaceRemoveIndex, name, FaceRemoveOutline.Count));
        else
            GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Remove_Hover_None"));

        if (FaceMessage != null)
        {
            if (FaceMessageIsError)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, FaceMessage);
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, FaceMessage);
        }

        // Controls
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Edit_Controls_Header"), LOC.Get("MAP_Navigation_Edit_Controls_Header_TT"),
            ref ShowVertexControls);

        if (ShowVertexControls)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Face_Remove_Control_Click", InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action)));
    }

    #endregion

    #region Havok

    private void HandleFaceRemove(Ray ray, bool canInteract)
    {
        VertexHover = null;
        ClearFaceRemoveHover();

        var canEdit = OverlayNavigation != null && OverlayContainer != null && (VertexMap == null || VertexMap == OverlayContainer);
        var flying = InputManager.IsMouseDown(Veldrid.MouseButton.Right) || InputManager.IsMouseDown(Veldrid.MouseButton.Middle);

        if (!canInteract || !canEdit || flying || ImGui.GetIO().WantTextInput)
            return;

        if (VertexRayCast(ray) is not { } hit)
            return;

        FaceRemoveFile = hit.File;
        FaceRemoveIndex = hit.Face;
        FaceRemoveOutline.AddRange(hit.File.FaceCorners(hit.Face).Select(hit.File.WorldVertex));

        if (InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            CommitHavokFaceRemove(hit.File, hit.Face);
    }

    /// <summary>
    /// Removes a face from its file: the NVA entries of the file get the new face count (and the running face data
    /// indices follow), the connections and node bank faces of the navmesh follow the renumbering (those naming the face
    /// or its edges go), and the file's document is swapped, all as one undoable action.
    /// </summary>
    private void CommitHavokFaceRemove(NavVertexFile file, int face)
    {
        var navigation = OverlayNavigation;
        var map = OverlayContainer;

        if (navigation == null || map == null)
            return;

        // Removing a face renumbers the file, which would drop its brush edits waiting
        if (file.IsEdited)
        {
            SetFaceMessage(LOC.Get("MAP_Navigation_Face_Error_Pending", file.File.Name), true);
            return;
        }

        var entry = file.Entry;
        var before = new NavModelState(navigation);

        NavFaceEditResult result;
        try
        {
            result = NavFaceRemove.Build(file.File.Document, face);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Face_Remove_Failed", file.File.Name), e);
            result = new NavFaceEditResult { Error = "MAP_Navigation_Face_Error_File" };
        }

        if (!result.Ok)
        {
            SetFaceMessage(LOC.Get(result.Error), true);
            return;
        }

        int faces = result.Document.ObjectsOf<Havok.Shared.IHavokNavMeshEdit>().First().m_faces.Count;

        foreach (var e in navigation.Nva.Navmeshes)
        {
            if (string.Equals(navigation.FileNameOf(e), file.File.Name, StringComparison.OrdinalIgnoreCase))
                e.FaceCount = faces;
        }

        var changes = NavigationRemap.RemapNavmesh(navigation, entry.NameID, result.FaceMap, result.EdgeMap);
        changes.NvaChanged = true;

        NavModelEdit.RecomputeFaceDataIndex(navigation.Nva, navigation.Profile);

        var after = new NavModelState(navigation);
        ModelEditMap = map;
        View.ViewportActionManager.ExecuteAction(new NavigationModelEditAction(map, Bank, before, after,
            new[] { new NavFileChange(file.File.Name, file.File.Document, result.Document) }, changes, OnModelEdited));

        SetFaceMessage(LOC.Get(result.GraphRebuilt ? "MAP_Navigation_Face_Removed_Graph" : "MAP_Navigation_Face_Removed",
            file.File.Name, face, result.LinksRemoved, result.Opened, result.VerticesRemoved, changes.Dropped));
        ClearFaceRemoveHover();
    }

    #endregion

    #region NVM

    private void HandleNvmFaceRemove(Ray ray, bool canInteract)
    {
        NvmVertexHover = null;
        ClearFaceRemoveHover();

        var canEdit = NvmData != null && NvmContainer != null && (NvmVertexMap == null || NvmVertexMap == NvmContainer || !HasNvmVertexEdits);
        var flying = InputManager.IsMouseDown(Veldrid.MouseButton.Right) || InputManager.IsMouseDown(Veldrid.MouseButton.Middle);

        if (!canInteract || !canEdit || flying || ImGui.GetIO().WantTextInput)
            return;

        if (NvmVertexRayCast(ray) is not { } hit)
            return;

        FaceRemoveNvm = hit.Copy;
        FaceRemoveIndex = hit.Triangle;
        FaceRemoveOutline.AddRange(NvmGeometry.VertexIndices(hit.Copy.Nvm.Triangles[hit.Triangle]).Select(hit.Copy.WorldVertex));

        if (InputManager.IsMousePressed(MousebindID.Viewport_Picking_Action))
            CommitNvmFaceRemove(hit.Copy, hit.Triangle);
    }

    /// <summary>
    /// Removes a triangle from its NVM: every room that loads the file follows, their MCP room boxes are set from the new
    /// root box, the MCG edges' triangle lists follow the renumbering, and it is all swapped in as one undoable action.
    /// </summary>
    private void CommitNvmFaceRemove(NvmVertexCopy copy, int triangle)
    {
        var map = NvmVertexMap ?? NvmContainer;
        var data = map != null ? Nvm.GetMap(map) : null;

        if (data == null || !data.Files.TryGetValue(copy.Room.FileName, out var before) || !ReferenceEquals(before, copy.Nvm))
            return;

        if (copy.IsEdited)
        {
            SetFaceMessage(LOC.Get("MAP_Navigation_Face_Error_Pending", copy.Room.FileName), true);
            return;
        }

        NavFaceEditResult result;
        try
        {
            result = NvmFaceRemove.Build(data, copy.Room, triangle);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Face_Remove_Failed", copy.Room.FileName), e);
            result = new NavFaceEditResult { Error = "MAP_Navigation_Face_Error_File" };
        }

        if (!result.Ok)
        {
            SetFaceMessage(LOC.Get(result.Error), true);
            return;
        }

        var boxes = data.Rooms.Where(r => r.FileName != null && r.FileName.Equals(copy.Room.FileName, StringComparison.OrdinalIgnoreCase))
            .Select(r => (r, before, result.Nvm)).ToList();
        var mcp = NvmEdit.WithRoomBoxes(data, data.Mcp, boxes);

        var files = new List<NavigationNvmEditAction.FileChange> { new(copy.Room.FileName, before, result.Nvm) };
        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(map, Nvm, files, data.Mcp, mcp, data.Mcg, result.Mcg, OnNvmEdited));

        NvmVertexCopies.Clear();

        SetFaceMessage(LOC.Get("MAP_Navigation_Face_Removed", copy.Room.FileName, triangle, 0, result.Opened, result.VerticesRemoved, 0));
        ClearFaceRemoveHover();
    }

    #endregion
}
