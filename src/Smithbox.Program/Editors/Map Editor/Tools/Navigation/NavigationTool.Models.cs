using Hexa.NET.ImGui;
using System;
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
    private ModelTarget DuplicateTarget = ModelTarget.New;
    private int DuplicateNewModelId = -1;

    /// <summary>The preferred ModelID each target picker last set its new ModelID from, so a new choice resets it.</summary>
    private readonly Dictionary<string, int> NewModelIdPreferred = new();

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

        // A game with the n set only (AC6) has no o column
        bool hasO = navigation.Profile.HasOSet;

        if (ShowModelList && ImGui.BeginTable("##navigationModels", hasO ? 4 : 3,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new Vector2(0, 220 * DPI.UIScale())))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Models_Column_Model"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Models_Column_N"), ImGuiTableColumnFlags.WidthFixed);
            if (hasO)
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
                    {
                        SelectedModelID = model.ModelID;
                        if (OverlayOnlySelectedModel)
                            OverlayDirty = true;
                    }

                    ImGui.TableNextColumn();
                    ImGui.Text($"{model.NEntry?.FaceCount ?? 0}");

                    if (hasO)
                    {
                        ImGui.TableNextColumn();
                        ImGui.Text($"{model.OEntry?.FaceCount ?? 0}");
                    }

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
            GUI.WrappedText(hasO
                ? LOC.Get("MAP_Navigation_Models_Selected", selected.ModelID, (uint)selected.NEntry.NameID, (uint)selected.OEntry.NameID)
                : LOC.Get("MAP_Navigation_Models_Selected_N", selected.ModelID, (uint)selected.NEntry.NameID));

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

        // A copy can't replace its own source
        if (DuplicateTarget == ModelTarget.Replace)
            DuplicateTarget = ModelTarget.New;

        if (!DisplayModelTarget("navigationModels", map, navigation, null, removable, slots, ref DuplicateTarget, ref DuplicateNewModelId, selected.ModelID + 1))
            return;

        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##navigationModelsOffset", ref DuplicateOffset);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Models_Offset_TT"));

        if (ImGui.Button($"{LOC.Get("MAP_Navigation_Models_Duplicate")}##navigationModelsDuplicate"))
        {
            if (DuplicateTarget == ModelTarget.New)
            {
                int id = DuplicateNewModelId;
                var offset = DuplicateOffset;
                EditModel(map, navigation, () => NavModelEdit.Add(navigation, id, NavModelEdit.TranslatedCopy(selected.NFile.Document, offset),
                    hasO ? NavModelEdit.TranslatedCopy(selected.OFile.Document, offset) : null),
                    LOC.Get("MAP_Navigation_Models_Duplicated", selected.ModelID, id));
            }
            else
            {
                var slot = slots.First(s => s.ModelID == SlotModelID);
                EditModel(map, navigation, () => NavModelEdit.Duplicate(navigation, selected, slot, DuplicateOffset),
                    LOC.Get("MAP_Navigation_Models_Duplicated", selected.ModelID, slot.ModelID));
            }
        }
        GUI.Tooltip(LOC.Get("MAP_Navigation_Models_Duplicate_TT"));
    }

    /// <summary>
    /// The choice of where a model edit puts its new files: in place of the selected model (when given), into a free
    /// placeholder, or as a new model with its own ModelID. Only the targets the map allows are offered. Returns false
    /// when none can be used, or the new ModelID is taken.
    /// </summary>
    private bool DisplayModelTarget(string id, MapContainer map, NavigationMap navigation, NavmeshModel replaceable, HashSet<NavmeshModel> removable,
        List<NavmeshModel> slots, ref ModelTarget target, ref int newModelId, int preferredId)
    {
        bool canReplace = replaceable != null && removable.Contains(replaceable);
        bool canSlot = slots.Count > 0;
        string cannotAdd = NavModelEdit.CanAdd(navigation);
        bool canNew = cannotAdd == null;

        if (!canReplace && !canSlot && !canNew)
        {
            GUI.WrappedText(LOC.Get(cannotAdd ?? "MAP_Navigation_Generate_No_Target"));
            return false;
        }

        // A target the map doesn't allow falls back to one it does
        if ((target == ModelTarget.Replace && !canReplace) || (target == ModelTarget.Slot && !canSlot) || (target == ModelTarget.New && !canNew))
            target = canNew ? ModelTarget.New : canReplace ? ModelTarget.Replace : ModelTarget.Slot;

        if (canNew && ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Model_Target_New")}##{id}TargetNew", target == ModelTarget.New))
            target = ModelTarget.New;
        if (canNew)
            GUI.Tooltip(LOC.Get("MAP_Navigation_Model_Target_New_TT"));

        if (canReplace && ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Generate_Replace", replaceable.ModelID)}##{id}TargetReplace", target == ModelTarget.Replace))
            target = ModelTarget.Replace;
        if (canReplace)
            GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Replace_TT"));

        if (canSlot && ImGui.RadioButton($"{LOC.Get("MAP_Navigation_Generate_Into_Slot")}##{id}TargetSlot", target == ModelTarget.Slot))
            target = ModelTarget.Slot;
        if (canSlot)
            GUI.Tooltip(LOC.Get("MAP_Navigation_Generate_Into_Slot_TT"));

        if (!canNew)
            GUI.WrappedText(LOC.Get(cannotAdd));

        if (target == ModelTarget.Slot)
        {
            if (slots.All(s => s.ModelID != SlotModelID))
                SlotModelID = slots[0].ModelID;

            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo($"##{id}Slot", LOC.Get("MAP_Navigation_Models_Slot", SlotModelID)))
            {
                foreach (var slot in slots)
                    if (ImGui.Selectable($"{LOC.Get("MAP_Navigation_Models_Slot", slot.ModelID)}##{id}Slot_{slot.ModelID}", slot.ModelID == SlotModelID))
                        SlotModelID = slot.ModelID;
                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("MAP_Navigation_Models_Slot_TT"));
        }

        if (target == ModelTarget.New)
        {
            // Starts from the preferred ID (the collision model's, or the source's next), or the next free one after it
            if (newModelId < 0 || NewModelIdPreferred.GetValueOrDefault(id, int.MinValue) != preferredId)
            {
                NewModelIdPreferred[id] = preferredId;
                newModelId = NavModelEdit.NextFreeModelId(navigation, preferredId >= 0 ? preferredId : 900000);
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputInt($"##{id}NewModelId", ref newModelId);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Model_Target_New_Id_TT"));
            newModelId = Math.Clamp(newModelId, 0, 999999);

            var taken = newModelId;
            if (navigation.Nva.Navmeshes.Any(e => e.ModelID == taken))
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Navigation_Model_Target_New_Id_Taken", newModelId));
                return false;
            }

            GUI.WrappedText(navigation.Profile.HasOSet
                ? LOC.Get("MAP_Navigation_Model_Target_New_Files",
                    NavigationMap.MeshName(navigation.NameBase, NavmeshSet.N, newModelId), NavigationMap.MeshName(navigation.NameBase, NavmeshSet.O, newModelId))
                : LOC.Get("MAP_Navigation_Model_Target_New_File", NavigationMap.MeshName(navigation.NameBase, NavmeshSet.N, newModelId)));
        }

        return true;
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
        RefreshDrawnNavmeshes = true;
    }

    /// <summary>Set by a model edit: the Map Editor's own navmesh drawing of the map is brought up to date next frame.</summary>
    private bool RefreshDrawnNavmeshes;

    /// <summary>The map of the last model edit, whose drawing is refreshed (undo and redo edit the same map).</summary>
    private MapContainer ModelEditMap;

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
            if (entity.WrappedObject is not SoulsFormats.NVA.Navmesh entry || entity.RenderSceneMesh is not MeshRenderableProxy proxy)
                continue;

            var resource = (proxy.ResourceHandle as ResourceHandle<HavokNavmeshResource>)?.Get();
            var document = navigation.Files.GetValueOrDefault(NavigationMap.FileNameFromEntry(map.Name, entry, navigation.Profile, navigation.Files.Keys))?.Document;

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
}
