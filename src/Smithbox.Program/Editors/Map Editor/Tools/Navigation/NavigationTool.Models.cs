using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Models tab (Navigation Editor Plan, Phase 3): the map's navmesh models, removing one (it becomes a placeholder)
/// and duplicating one into a free placeholder pair, moved by an offset. See NavModelEdit.
/// </summary>
public partial class NavigationTool
{
    private bool ShowModelList = true;
    private int SelectedModelID = -1;
    private int SlotModelID = -1;
    private Vector3 DuplicateOffset = Vector3.Zero;

    /// <summary>The map the models are listed for, read again after a navigation edit.</summary>
    private NavigationMap ModelsNavigation;
    private string ModelsMap;

    /// <summary>What the last model edit did, or why it was refused.</summary>
    private string ModelMessage;
    private bool ModelMessageIsError;

    private void DisplayModelsTab(MapContainer map)
    {
        GUI.WrappedText(LOC.Get("MAP_Navigation_Models_Description"));

        if (ModelsNavigation == null || ModelsMap != map.Name)
        {
            ModelsNavigation = Bank.GetMap(map);
            ModelsMap = map.Name;
        }

        var navigation = ModelsNavigation;
        if (navigation == null)
            return;

        var models = navigation.Models;
        var removable = NavModelEdit.Removable(navigation).ToHashSet();
        var slots = NavModelEdit.FreeSlots(navigation).ToList();

        // Models: the counts stay outside the list toggle
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Models_Header"), LOC.Get("MAP_Navigation_Models_Header_TT"), ref ShowModelList);
        GUI.WrappedText(LOC.Get("MAP_Navigation_Models_Counts", models.Count, models.Count(m => !m.IsPlaceholder), models.Count(m => m.IsPlaceholder), slots.Count));

        if (Picked != null && ImGui.Button($"{LOC.Get("MAP_Navigation_Models_Select_Picked")}##navigationModelsSelectPicked"))
            SelectedModelID = Picked.Entry.ModelID;
        if (Picked != null)
            GUI.Tooltip(LOC.Get("MAP_Navigation_Models_Select_Picked_TT"));

        if (ShowModelList && ImGui.BeginTable("##navigationModels", 4,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new Vector2(0, 220 * DPI.UIScale())))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Models_Column_Model"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Models_Column_N"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Models_Column_O"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Models_Column_State"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(models.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var model = models[i];
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    if (ImGui.Selectable($"{model.ModelID:D6}##navigationModel_{i}", model.ModelID == SelectedModelID, ImGuiSelectableFlags.SpanAllColumns))
                        SelectedModelID = model.ModelID;

                    ImGui.TableNextColumn();
                    ImGui.Text($"{model.NEntry?.FaceCount ?? 0}");

                    ImGui.TableNextColumn();
                    ImGui.Text($"{model.OEntry?.FaceCount ?? 0}");

                    ImGui.TableNextColumn();
                    ImGui.Text(model.Quarters.Count > 0 ? LOC.Get("MAP_Navigation_Models_State_Quarters")
                        : model.IsPlaceholder ? LOC.Get(slots.Contains(model) ? "MAP_Navigation_Models_State_Free" : "MAP_Navigation_Models_State_Placeholder")
                        : LOC.Get("MAP_Navigation_Models_State_Mesh"));
                }
            }

            clipper.End();
            ImGui.EndTable();
        }

        if (ModelMessage != null)
        {
            if (ModelMessageIsError)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, ModelMessage);
            else
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, ModelMessage);
        }

        var selected = models.FirstOrDefault(m => m.ModelID == SelectedModelID);

        // Remove
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Models_Remove_Header"), LOC.Get("MAP_Navigation_Models_Remove_Header_TT"));

        if (selected == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Models_None_Selected"));
        }
        else if (!removable.Contains(selected))
        {
            GUI.WrappedText(LOC.Get(selected.Quarters.Count > 0 ? "MAP_Navigation_Model_Error_Quarters" : "MAP_Navigation_Model_Error_Placeholder"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Models_Selected", selected.ModelID, (uint)selected.NEntry.NameID, (uint)selected.OEntry.NameID));

            if (ImGui.Button($"{LOC.Get("MAP_Navigation_Models_Remove")}##navigationModelsRemove"))
                EditModel(map, navigation, () => NavModelEdit.Remove(navigation, selected), LOC.Get("MAP_Navigation_Models_Removed", selected.ModelID));
            GUI.Tooltip(LOC.Get("MAP_Navigation_Models_Remove_TT"));
        }

        DisplayGenerate(map, navigation, selected, removable, slots);

        // Duplicate
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Models_Duplicate_Header"), LOC.Get("MAP_Navigation_Models_Duplicate_Header_TT"));

        if (selected == null || !removable.Contains(selected))
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Models_Duplicate_Needs_Source"));
            return;
        }

        if (slots.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Models_No_Free_Slot"));
            return;
        }

        if (slots.All(s => s.ModelID != SlotModelID))
            SlotModelID = slots[0].ModelID;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##navigationModelsSlot", LOC.Get("MAP_Navigation_Models_Slot", SlotModelID)))
        {
            foreach (var slot in slots)
                if (ImGui.Selectable($"{LOC.Get("MAP_Navigation_Models_Slot", slot.ModelID)}##navigationModelsSlot_{slot.ModelID}", slot.ModelID == SlotModelID))
                    SlotModelID = slot.ModelID;
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Models_Slot_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##navigationModelsOffset", ref DuplicateOffset);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Models_Offset_TT"));

        if (ImGui.Button($"{LOC.Get("MAP_Navigation_Models_Duplicate")}##navigationModelsDuplicate"))
        {
            var slot = slots.First(s => s.ModelID == SlotModelID);
            EditModel(map, navigation, () => NavModelEdit.Duplicate(navigation, selected, slot, DuplicateOffset),
                LOC.Get("MAP_Navigation_Models_Duplicated", selected.ModelID, slot.ModelID));
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Models_Duplicate_TT"));
    }

    /// <summary>
    /// Makes a model edit and records it as one undoable action: the NVA and NVC values are copied before and after it,
    /// and the action puts the new file documents in the bank. A refused edit changes nothing.
    /// </summary>
    private void EditModel(MapContainer map, NavigationMap navigation, System.Func<NavModelEditResult> edit, string done)
    {
        var before = new NavModelState(navigation);
        var result = edit();

        if (!result.Ok)
        {
            before.Restore();
            ModelMessage = LOC.Get(result.Error);
            ModelMessageIsError = true;
            return;
        }

        ModelEditMap = map;
        var after = new NavModelState(navigation);
        View.ViewportActionManager.ExecuteAction(new NavigationModelEditAction(map, Bank, before, after, result.Files, result.Changes, OnModelEdited));

        ModelMessage = done;
        ModelMessageIsError = false;
    }

    private void OnModelEdited()
    {
        ModelsNavigation = null;
        SetPicked(null);
        OnNavigationEdited();
        DiagnoseNextRebuild = true;
        RefreshDrawnNavmeshes = true;
    }

    /// <summary>Set by a model edit: the Map Editor's own navmesh drawing of the map is brought up to date next frame.</summary>
    private bool RefreshDrawnNavmeshes;

    /// <summary>The map of the last model edit, whose drawing is refreshed (undo and redo edit the same map).</summary>
    private MapContainer ModelEditMap;

    /// <summary>
    /// TEMPORARY diagnostics (overlay not showing a generated model): the next overlay rebuild after a model edit logs
    /// what it drew. Remove once the overlay is confirmed in the app.
    /// </summary>
    private bool DiagnoseNextRebuild;

    /// <summary>The document each drawn navmesh resource was last rebuilt from.</summary>
    private readonly Dictionary<HavokNavmeshResource, Havok.HavokDocument> DrawnFrom = new();

    /// <summary>Geometry buffers replaced by a refresh, disposed once the GPU no longer uses them.</summary>
    private readonly List<(List<VertexIndexBufferAllocator.VertexIndexBufferHandle> Buffers, int Frame)> PendingBufferRelease = new();

    private int DrawFrame;
    private const int ReleaseDelayFrames = 10;

    /// <summary>
    /// Rebuilds the Map Editor's own drawing of each navmesh entry of a map whose file's document in the navmesh bank
    /// isn't the one it was drawn from, as the drawing reads the files from disk when the map loads. An entry whose
    /// file had no loaded resource (a placeholder that was given a model, or a renamed one) is only drawn after the map
    /// is saved and loaded again; the overlay shows it meanwhile.
    /// </summary>
    private void RefreshMapNavmeshDrawing(MapContainer map)
    {
        var navigation = Bank.GetMap(map);
        if (navigation == null)
            return;

        foreach (var entity in map.Objects)
        {
            if (entity.WrappedObject is not SoulsFormats.NVA_ER.Navmesh entry || entity.RenderSceneMesh is not MeshRenderableProxy proxy)
                continue;

            var resource = (proxy.ResourceHandle as ResourceHandle<HavokNavmeshResource>)?.Get();
            var document = navigation.Files.GetValueOrDefault(NavigationMap.FileNameFromEntry(map.Name, entry))?.Document;

            if (resource == null || document == null || (DrawnFrom.TryGetValue(resource, out var drawn) && ReferenceEquals(drawn, document)))
                continue;

            PendingBufferRelease.Add((resource.Refresh(document), DrawFrame));
            DrawnFrom[resource] = document;

            proxy.ScheduleRenderableConstruction();
            foreach (var submesh in proxy.Submeshes)
                submesh.ScheduleRenderableConstruction();
        }
    }

    /// <summary>Called each frame: refreshes the drawing after a model edit, and disposes replaced buffers.</summary>
    private void UpdateNavmeshDrawing()
    {
        DrawFrame++;

        var map = ModelEditMap ?? OverlayContainer;
        if (RefreshDrawnNavmeshes && map != null && map.LoadState == MapContentLoadState.Loaded)
        {
            RefreshDrawnNavmeshes = false;
            RefreshMapNavmeshDrawing(map);
        }

        for (int i = PendingBufferRelease.Count - 1; i >= 0; i--)
        {
            if (DrawFrame - PendingBufferRelease[i].Frame < ReleaseDelayFrames)
                continue;

            foreach (var buffer in PendingBufferRelease[i].Buffers)
                buffer?.Dispose();

            PendingBufferRelease.RemoveAt(i);
        }
    }

    /// <summary>TEMPORARY diagnostics: what an overlay rebuild drew, by navmesh entry.</summary>
    private void LogOverlayDiagnostics(NavigationMap navigation)
    {
        if (!DiagnoseNextRebuild)
            return;

        DiagnoseNextRebuild = false;
        var lines = new List<string> { $"[NavDiag] overlay rebuilt for {navigation.MapId}, set {OverlaySet}, {navigation.Files.Count} files, faces drawn {OverlayData?.Batches.Values.Where(b => b.Layer == NavOverlayLayer.Faces).Sum(b => b.FaceCount) ?? -1}" };

        foreach (var entry in NavigationOverlayBuilder.EntriesOf(navigation, OverlaySet))
        {
            var name = navigation.FileNameOf(entry);
            var file = navigation.FileOf(entry);
            lines.Add($"[NavDiag]   entry {(uint)entry.NameID} model {entry.ModelID:D6} faces {entry.FaceCount} file {name} {(file == null ? "MISSING" : $"{file.FaceCount} faces, doc {file.Document?.GetHashCode():X8}")}");
        }

        Smithbox.Log(typeof(NavigationTool), string.Join("\n", lines));
    }
}
