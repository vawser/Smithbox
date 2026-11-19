using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The NGP mode's Models tab (Phase G4): the map's navmeshes (MSB navmesh parts and the NGP meshes of the same position),
/// and making new ones. A new navmesh is a new MSB2 navmesh part after the map's others (model nAA_XXYY, its NavmeshGroup
/// bit that of the collisions it was made from) with its mesh at the end of the NGP. Generate builds the mesh from the
/// selected collision parts (DS2S: hk2014 collision); Duplicate copies a navmesh moved by an offset; Remove takes a part
/// out with its mesh. Each is one undoable action: the MSB part (EntAddAction / EntDeleteAction) with the NGP
/// (NavigationNgpEditAction), whose gates, links and IDs NgpRebuild works out again. See NgpModelEdit.
/// </summary>
public partial class NavigationTool
{
    private int NgpModelMesh = -1;
    private bool NgpShowModels = true;
    private Vector3 NgpDuplicateOffset = new(0, 0, 5);
    private bool NgpGenerateReplace;
    private NavGenParameters NgpGenerateParameters = NgpModelEdit.DefaultParameters;

    private bool NgpGenerating;
    private volatile int NgpGenerateRun;
    private readonly ConcurrentQueue<Action> NgpGenerateResults = new();

    private NgpMesh NgpModelSelected(NgpMap data)
        => data != null && NgpModelMesh >= 0 && NgpModelMesh < data.Meshes.Count ? data.Meshes[NgpModelMesh] : null;

    /// <summary>Whether mesh i is part i for every mesh, which edits that add or remove parts rely on.</summary>
    private static bool NgpPartsMatch(NgpMap data) => data.PartCount == data.Meshes.Count;

    #region Tab

    private void DisplayNgpModelsTab(MapContainer map)
    {
        var data = Ngp.GetMap(map);
        if (data == null)
            return;

        var selected = NgpModelSelected(data);

        // Navmeshes: the counts stay outside the list toggle
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Ngp_Models_Header"), LOC.Get("MAP_Navigation_Ngp_Models_Header_TT"), ref NgpShowModels);
        GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Models_Counts", data.Meshes.Count, data.PartCount, NgpModelEdit.MaxMeshes));

        if (!NgpPartsMatch(data))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Navigation_Ngp_Models_Mismatch", data.PartCount, data.Meshes.Count));

        if (data.Meshes.Count > 0 && NgpData != null && PickedNgpMesh(NgpData) is NgpMesh picked)
        {
            if (ImGui.Button($"{LOC.Get("MAP_Navigation_Ngp_Models_Select_Picked")}##navigationNgpModelsSelectPicked"))
                NgpModelMesh = picked.Index;
            GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Models_Select_Picked_TT"));
        }

        if (NgpShowModels && data.Meshes.Count > 0 && ImGui.BeginTable("##navigationNgpModels", 5,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp,
            new Vector2(0, 220 * DPI.UIScale())))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Ngp_Models_Column_Mesh"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Ngp_Models_Column_Part"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Ngp_Models_Column_Model"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Ngp_Models_Column_Faces"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Ngp_Models_Column_Gates"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            foreach (var mesh in data.Meshes)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{mesh.Index}##navigationNgpModel_{mesh.Index}", mesh.Index == NgpModelMesh, ImGuiSelectableFlags.SpanAllColumns))
                    NgpModelMesh = mesh.Index == NgpModelMesh ? -1 : mesh.Index;
                ImGui.TableNextColumn();
                ImGui.Text(mesh.PartName ?? LOC.Get("MAP_Navigation_Ngp_Models_No_Part"));
                ImGui.TableNextColumn();
                ImGui.Text(mesh.ModelName ?? "-");
                ImGui.TableNextColumn();
                ImGui.Text(mesh.Mesh.Faces.Count.ToString());
                ImGui.TableNextColumn();
                ImGui.Text(mesh.Mesh.GateCount.ToString());
            }

            ImGui.EndTable();
        }

        if (NgpGenerating)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Ngp_Generate_Running"));

        DisplayNgpGenerate(map, data, selected);
        DisplayNgpDuplicate(map, data, selected);
        DisplayNgpRemove(map, data, selected);
        DisplayNgpStrips(map, data);
    }

    private void DisplayNgpGenerate(MapContainer map, NgpMap data, NgpMesh selected)
    {
        // Generate
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Generate_Header"), LOC.Get("MAP_Navigation_Ngp_Generate_Header_TT"));

        var sources = SelectedSources().Where(s => !s.IsAsset).ToList();
        GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Generate_Sources", sources.Count));

        foreach (var source in sources.Take(8))
            GUI.WrappedText($"  {source.Label}");
        if (sources.Count > 8)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Generate_More", sources.Count - 8));

        if (selected != null)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Ngp_Generate_Replace", selected.Label)}##navigationNgpGenerateReplace", ref NgpGenerateReplace);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Generate_Replace_TT"));
        }
        else
        {
            NgpGenerateReplace = false;
        }

        NgpGenerateParameters = DisplayGenerateParameters("navigationNgpGenerate", LOC.Get("MAP_Navigation_Ngp_Generate_Settings"),
            NgpGenerateParameters, NgpModelEdit.DefaultParameters) with { Links = false };

        bool canAdd = NgpGenerateReplace || (NgpPartsMatch(data) && data.Meshes.Count < NgpModelEdit.MaxMeshes);

        GUI.ConditionalMultiButtonInput("navigationNgpGenerateActions",
            "navigationNgpGenerateButton", LOC.Get("MAP_Navigation_Ngp_Generate"), LOC.Get("MAP_Navigation_Ngp_Generate_TT"),
            () => StartNgpGenerate(map, sources, NgpGenerateReplace ? selected : null), sources.Count > 0 && !NgpGenerating && canAdd,

            "navigationNgpGenerateCancel", LOC.Get("MAP_Navigation_Generate_Cancel"), LOC.Get("MAP_Navigation_Ngp_Generate_Cancel_TT"),
            () => { NgpGenerateRun++; NgpGenerating = false; }, NgpGenerating);

        if (sources.Count == 0)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Generate_Select"));
    }

    private void DisplayNgpDuplicate(MapContainer map, NgpMap data, NgpMesh selected)
    {
        // Duplicate
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Duplicate_Header"), LOC.Get("MAP_Navigation_Ngp_Duplicate_Header_TT"));

        if (selected == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Models_Select_First"));
            return;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##navigationNgpDuplicateOffset", ref NgpDuplicateOffset);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Ngp_Duplicate_Offset_TT"));

        GUI.ConditionalMultiButtonInput("navigationNgpDuplicateActions",
            "navigationNgpDuplicate", LOC.Get("MAP_Navigation_Ngp_Duplicate", selected.Label), LOC.Get("MAP_Navigation_Ngp_Duplicate_TT"),
            () => DuplicateNgpMesh(map, data, selected, NgpDuplicateOffset), NgpPartsMatch(data) && data.Meshes.Count < NgpModelEdit.MaxMeshes);
    }

    private void DisplayNgpRemove(MapContainer map, NgpMap data, NgpMesh selected)
    {
        // Remove
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Ngp_Remove_Header"), LOC.Get("MAP_Navigation_Ngp_Remove_Header_TT"));

        if (selected == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Ngp_Models_Select_First"));
            return;
        }

        GUI.ConditionalMultiButtonInput("navigationNgpRemoveActions",
            "navigationNgpRemove", LOC.Get("MAP_Navigation_Ngp_Remove", selected.Label), LOC.Get("MAP_Navigation_Ngp_Remove_TT"),
            () => RemoveNgpMesh(map, data, selected), NgpPartsMatch(data) && selected.Part is MsbEntity);
    }

    #endregion

    #region Edits

    private static uint[] NavmeshGroups(MSB2.Part.Navmesh p)
        => new[] { (uint)p.NavmeshGroup0, (uint)p.NavmeshGroup1, (uint)p.NavmeshGroup2, (uint)p.NavmeshGroup3 };

    private static uint[] CollisionGroups(MSB2.Part.Collision c)
        => new[] { c.NvmGroup0, c.NvmGroup1, c.NvmGroup2, c.NvmGroup3 };

    /// <summary>
    /// Adds a navmesh: a new MSB2 navmesh part after the map's others (a copy of the template part, else the map's first
    /// navmesh part, named nAA_XXYY_0000 with the given group bit) and its mesh at the end of the NGP, as one action.
    /// </summary>
    private void AddNgpMesh(MapContainer map, NgpMap data, NGP.Mesh mesh, object template, int groupBit, string done)
    {
        if (!NgpPartsMatch(data) || data.Meshes.Count >= NgpModelEdit.MaxMeshes)
        {
            SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Models_Cannot_Add", data.PartCount, data.Meshes.Count, NgpModelEdit.MaxMeshes), true);
            return;
        }

        var taken = map.Objects.Select(o => (o.WrappedObject as IMsbEntry)?.Name).Where(n => n != null).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var (model, name) = NgpModelEdit.NewNames(data, taken);
        if (model == null)
        {
            SetNgpMessage(LOC.Get("MAP_Navigation_Nvm_Models_No_Name"), true);
            return;
        }

        var source = (template as MsbEntity)?.WrappedObject as MSB2.Part.Navmesh
            ?? data.Meshes.Select(m => (m.Part as MsbEntity)?.WrappedObject as MSB2.Part.Navmesh).FirstOrDefault(p => p != null);

        var part = source?.DeepCopy() as MSB2.Part.Navmesh ?? new MSB2.Part.Navmesh { Position = data.World.Translation };
        part.Name = name;
        part.ModelName = model;
        part.Rotation = Vector3.Zero;

        var groups = NgpModelEdit.GroupOf(groupBit);
        part.NavmeshGroup0 = (int)groups[0];
        part.NavmeshGroup1 = (int)groups[1];
        part.NavmeshGroup2 = (int)groups[2];
        part.NavmeshGroup3 = (int)groups[3];

        NGP after;
        try
        {
            after = NgpModelEdit.WithMeshAdded(data.Ngp, mesh);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Edit_Commit_Failed", data.FileName), e);
            SetNgpMessage(LOC.Get("MAP_Navigation_Edit_Commit_Failed", data.FileName), true);
            return;
        }

        var entity = new MsbEntity(View.Universe, map, part, MsbEntityType.Part);
        var actions = new List<ViewportAction>
        {
            new EntAddAction(View, map, new List<MsbEntity> { entity }, map.RootObject),
            new NavigationNgpEditAction(map, Ngp, data.Ngp, after, OnNgpEdited),
        };

        View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));

        NgpModelMesh = data.Meshes.Count;
        int newGates = after.Meshes[^1].GateCount;
        SetNgpMessage(LOC.Get(done, part.Name, mesh.Faces.Count, newGates, groupBit));
        Smithbox.Log(this, NgpMessage);
    }

    private void DuplicateNgpMesh(MapContainer map, NgpMap data, NgpMesh source, Vector3 offset)
    {
        int bit = source.Part is MsbEntity e && e.WrappedObject is MSB2.Part.Navmesh p ? NgpModelEdit.GroupBit(NavmeshGroups(p)) : -1;
        AddNgpMesh(map, data, NgpModelEdit.Moved(source.Mesh, offset), source.Part, bit, "MAP_Navigation_Ngp_Duplicated");
    }

    private void RemoveNgpMesh(MapContainer map, NgpMap data, NgpMesh mesh)
    {
        if (mesh.Part is not MsbEntity entity || !NgpPartsMatch(data))
        {
            SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Remove_No_Part", mesh.Label), true);
            return;
        }

        var after = NgpModelEdit.WithMeshRemoved(data.Ngp, mesh.Index);

        var actions = new List<ViewportAction>
        {
            new NavigationNgpEditAction(map, Ngp, data.Ngp, after, OnNgpEdited),
            new EntDeleteAction(View, new List<MsbEntity> { entity }, true),
        };

        View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));

        NgpModelMesh = -1;
        SetNgpPicked(-1, -1, default);
        SelectNgpGate(-1);
        SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Removed", mesh.Label, data.Ngp.Gates.Count - after.Gates.Count));
    }

    #endregion

    #region Generate

    private void StartNgpGenerate(MapContainer map, List<GenerateSource> sources, NgpMesh replace)
    {
        var data = Ngp.GetMap(map);
        if (data == null)
            return;

        var input = new List<Vector3>();
        int missing = 0;

        // The collision parts' triangles in map space, wound to face up (the NVM mode's reader serves both)
        foreach (var source in sources)
        {
            if (NvmSourceTriangles(source) is List<Vector3> triangles)
                input.AddRange(triangles);
            else
                missing++;
        }

        if (input.Count < 3)
        {
            SetNgpMessage(LOC.Get("MAP_Navigation_Nvm_Generate_No_Collision", missing), true);
            return;
        }

        // The mesh is map-local: the map offset comes off
        if (!Matrix4x4.Invert(data.World, out var toLocal))
            toLocal = Matrix4x4.Identity;

        var sourceGroups = sources.Select(s => s.Part.WrappedObject as MSB2.Part.Collision).Where(c => c != null).Select(CollisionGroups).ToList();
        var navGroups = data.Meshes.Select(m => (m.Part as MsbEntity)?.WrappedObject as MSB2.Part.Navmesh).Where(p => p != null).Select(NavmeshGroups).ToList();
        var colGroups = map.Objects.Select(o => o.WrappedObject as MSB2.Part.Collision).Where(c => c != null).Select(CollisionGroups).ToList();
        int groupBit = NgpModelEdit.NewGroupBit(sourceGroups, navGroups, colGroups, out bool fromSources);

        var parameters = NgpGenerateParameters with { Links = false };
        int runtimeSlot = data.Ngp.Meshes.FirstOrDefault()?.RuntimeSlot ?? 0;
        int replaceIndex = replace?.Index ?? -1;
        int run = ++NgpGenerateRun;
        NgpGenerating = true;
        SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Generate_Started", input.Count / 3, missing));

        Task.Run(() =>
        {
            NavGenPolygons generated;
            NGP.Mesh mesh = null;

            try
            {
                generated = NavigationGenerator.GeneratePolygons(input, parameters);
                if (generated.Ok)
                {
                    var local = generated.Polygons.Select(p => (IReadOnlyList<Vector3>)p.Select(v => Vector3.Transform(v, toLocal)).ToList()).ToList();
                    mesh = NgpModelEdit.FromPolygons(local, runtimeSlot);
                }
            }
            catch (Exception e)
            {
                generated = new NavGenPolygons { Error = "MAP_Navigation_Generate_Error_Recast" };
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Ngp_Generate_Failed"), e);
            }

            NgpGenerateResults.Enqueue(() =>
            {
                if (run != NgpGenerateRun)
                    return;

                NgpGenerating = false;

                if (!generated.Ok || mesh == null)
                {
                    SetNgpMessage(LOC.Get(generated.Error ?? "MAP_Navigation_Ngp_Generate_Too_Big"), true);
                    return;
                }

                var current = Ngp.GetMap(map);
                if (current == null)
                    return;

                if (replaceIndex < 0)
                {
                    AddNgpMesh(map, current, mesh, null, groupBit, fromSources ? "MAP_Navigation_Ngp_Generated" : "MAP_Navigation_Ngp_Generated_New_Group");
                    return;
                }

                if (replaceIndex >= current.Meshes.Count)
                    return;

                var after = NgpModelEdit.WithMeshReplaced(current.Ngp, replaceIndex, mesh);
                View.ViewportActionManager.ExecuteAction(new NavigationNgpEditAction(map, Ngp, current.Ngp, after, OnNgpEdited));
                SetNgpMessage(LOC.Get("MAP_Navigation_Ngp_Regenerated", current.Meshes[replaceIndex].Label, mesh.Faces.Count, after.Meshes[replaceIndex].GateCount));
                Smithbox.Log(this, NgpMessage);
            });
        });
    }

    /// <summary>Applies finished generations; called each frame.</summary>
    private void DrainNgpGenerateResults()
    {
        while (NgpGenerateResults.TryDequeue(out var apply))
            apply();
    }

    #endregion
}
