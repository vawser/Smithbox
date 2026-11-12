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
/// The NVM mode's Models tab: the map's navmeshes (MSB navmesh parts and the NVMs they load), and making new ones. A new
/// navmesh is a new MSB navmesh part at the end of the map's (model n####B{BB}), its NVM in the binder (named as the
/// game names them) and an MCP room after the map's own; Generate builds its NVM from the selected collision parts,
/// Duplicate copies a navmesh's NVM onto a new part moved by an offset. Remove takes a part out with its room, its edges
/// and (if no other part loads it) its NVM. Mark Gates flags GATE where a navmesh meets the others, so the Graph tab can
/// join it to the MCG. Each is one undoable action: the MSB part (EntAddAction / EntDeleteAction) with the files
/// (NavigationNvmEditAction). See NvmModelEdit.
/// </summary>
public partial class NavigationTool
{
    private int NvmModelRoom = -1;
    private bool NvmShowModels = true;
    private Vector3 NvmDuplicateOffset = new(0, 0, 5);
    private bool NvmGenerateReplace;
    private bool NvmGenerateMarkGates = true;
    private NavGenParameters NvmGenerateParameters = NvmModelEdit.DefaultParameters;
    private float NvmGateTolerance = 0.05f;

    private bool NvmGenerating;
    private volatile int NvmGenerateRun;
    private readonly ConcurrentQueue<Action> NvmGenerateResults = new();

    private NvmRoom NvmModelSelected(NvmMap data)
        => data != null && NvmModelRoom >= 0 && NvmModelRoom < data.Rooms.Count ? data.Rooms[NvmModelRoom] : null;

    #region Tab

    private void DisplayNvmModelsTab(MapContainer map)
    {
        var data = Nvm.GetMap(map);
        if (data == null)
            return;

        var selected = NvmModelSelected(data);

        // Navmeshes: the counts stay outside the list toggle
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("MAP_Navigation_Nvm_Models_Header"), LOC.Get("MAP_Navigation_Nvm_Models_Header_TT"), ref NvmShowModels);
        GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Models_Counts", data.Rooms.Count, data.Files.Count, data.Files.Keys.Count(f => data.Rooms.All(r => r.FileName != f))));

        if (data.Rooms.Count > 0 && PickedNvmRoom(NvmData) is NvmRoom picked)
        {
            if (ImGui.Button($"{LOC.Get("MAP_Navigation_Nvm_Models_Select_Picked")}##navigationNvmModelsSelectPicked"))
                NvmModelRoom = picked.Index;
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Models_Select_Picked_TT"));
        }

        if (NvmShowModels && data.Rooms.Count > 0 && ImGui.BeginTable("##navigationNvmModels", 5,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp,
            new Vector2(0, 220 * DPI.UIScale())))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Nvm_Models_Column_Room"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Nvm_Models_Column_Part"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Nvm_Models_Column_File"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Nvm_Models_Column_Triangles"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_Navigation_Nvm_Models_Column_Entities"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            foreach (var room in data.Rooms)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{room.Index}##navigationNvmModel_{room.Index}", room.Index == NvmModelRoom, ImGuiSelectableFlags.SpanAllColumns))
                    NvmModelRoom = room.Index == NvmModelRoom ? -1 : room.Index;
                ImGui.TableNextColumn();
                ImGui.Text(room.PartName);
                ImGui.TableNextColumn();
                ImGui.Text(room.FileName ?? LOC.Get("MAP_Navigation_Nvm_Models_No_File"));
                ImGui.TableNextColumn();
                ImGui.Text(room.Nvm?.Triangles.Count.ToString() ?? "-");
                ImGui.TableNextColumn();
                ImGui.Text(room.Nvm?.Entities.Count.ToString() ?? "-");
            }

            ImGui.EndTable();
        }

        if (NvmGenerating)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_Navigation_Nvm_Generate_Running"));

        DisplayNvmGenerate(map, data, selected);
        DisplayNvmDuplicate(map, data, selected);
        DisplayNvmMarkGates(map, data, selected);
        DisplayNvmRemove(map, data, selected);
    }

    private void DisplayNvmGenerate(MapContainer map, NvmMap data, NvmRoom selected)
    {
        // Generate
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Generate_Header"), LOC.Get("MAP_Navigation_Nvm_Generate_Header_TT"));

        var sources = SelectedSources().Where(s => !s.IsAsset).ToList();
        GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Generate_Sources", sources.Count));

        foreach (var source in sources.Take(8))
            GUI.WrappedText($"  {source.Label}");
        if (sources.Count > 8)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Generate_More", sources.Count - 8));

        if (selected?.Nvm != null)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Nvm_Generate_Replace", selected.PartName)}##navigationNvmGenerateReplace", ref NvmGenerateReplace);
            GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Generate_Replace_TT"));
        }
        else
        {
            NvmGenerateReplace = false;
        }

        ImGui.Checkbox($"{LOC.Get("MAP_Navigation_Nvm_Generate_Mark_Gates")}##navigationNvmGenerateGates", ref NvmGenerateMarkGates);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Generate_Mark_Gates_TT"));

        NvmGenerateParameters = DisplayGenerateParameters("navigationNvmGenerate", LOC.Get("MAP_Navigation_Nvm_Generate_Settings"),
            NvmGenerateParameters, NvmModelEdit.DefaultParameters) with { Links = false };

        GUI.ConditionalMultiButtonInput("navigationNvmGenerateActions",
            "navigationNvmGenerateButton", LOC.Get("MAP_Navigation_Nvm_Generate"), LOC.Get("MAP_Navigation_Nvm_Generate_TT"),
            () => StartNvmGenerate(map, sources, NvmGenerateReplace ? selected : null), sources.Count > 0 && !NvmGenerating,

            "navigationNvmGenerateCancel", LOC.Get("MAP_Navigation_Generate_Cancel"), LOC.Get("MAP_Navigation_Nvm_Generate_Cancel_TT"),
            () => { NvmGenerateRun++; NvmGenerating = false; }, NvmGenerating);

        if (sources.Count == 0)
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Generate_Select"));
    }

    private void DisplayNvmDuplicate(MapContainer map, NvmMap data, NvmRoom selected)
    {
        // Duplicate
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Duplicate_Header"), LOC.Get("MAP_Navigation_Nvm_Duplicate_Header_TT"));

        if (selected?.Nvm == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Models_Select_First"));
            return;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##navigationNvmDuplicateOffset", ref NvmDuplicateOffset);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Duplicate_Offset_TT"));

        GUI.MultiButtonInput("navigationNvmDuplicateActions",
            "navigationNvmDuplicate", LOC.Get("MAP_Navigation_Nvm_Duplicate", selected.PartName), LOC.Get("MAP_Navigation_Nvm_Duplicate_TT"),
            () => DuplicateNvmRoom(map, data, selected, NvmDuplicateOffset));
    }

    private void DisplayNvmMarkGates(MapContainer map, NvmMap data, NvmRoom selected)
    {
        // Gates
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Mark_Gates_Header"), LOC.Get("MAP_Navigation_Nvm_Mark_Gates_Header_TT"));

        if (selected?.Nvm == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Models_Select_First"));
            return;
        }

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##navigationNvmGateTolerance", ref NvmGateTolerance))
            NvmGateTolerance = Math.Clamp(NvmGateTolerance, 0.001f, 2f);
        GUI.Tooltip(LOC.Get("MAP_Navigation_Nvm_Mark_Gates_Tolerance_TT"));

        GUI.MultiButtonInput("navigationNvmMarkGatesActions",
            "navigationNvmMarkGates", LOC.Get("MAP_Navigation_Nvm_Mark_Gates", selected.PartName), LOC.Get("MAP_Navigation_Nvm_Mark_Gates_TT"),
            () => MarkNvmGates(map, data, selected));
    }

    private void DisplayNvmRemove(MapContainer map, NvmMap data, NvmRoom selected)
    {
        // Remove
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Remove_Header"), LOC.Get("MAP_Navigation_Nvm_Remove_Header_TT"));

        if (selected == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Navigation_Nvm_Models_Select_First"));
            return;
        }

        GUI.MultiButtonInput("navigationNvmRemoveActions",
            "navigationNvmRemove", LOC.Get("MAP_Navigation_Nvm_Remove", selected.PartName), LOC.Get("MAP_Navigation_Nvm_Remove_TT"),
            () => RemoveNvmRoom(map, data, selected));
    }

    #endregion

    #region Edits

    /// <summary>The entity of a room's MSB part in the map.</summary>
    private static MsbEntity PartEntity(MapContainer map, NvmRoom room)
        => map.Objects.OfType<MsbEntity>().FirstOrDefault(e => ReferenceEquals(e.WrappedObject, room.Part));

    /// <summary>
    /// A new navmesh part for a model: a copy of a template part (the source's, else the map's first navmesh part, else
    /// a default one), named after the model (made unique among the map's parts), at a placement.
    /// </summary>
    private static IMsbPart NewNvmPart(MapContainer map, NvmMap data, object template, string model, Vector3 position, Vector3 rotation, uint[] nvmGroups)
    {
        template ??= data.Rooms.FirstOrDefault()?.Part;

        IMsbPart part = template switch
        {
            MSB1.Part.Navmesh p => p.DeepCopy(),
            MSBD.Part.Navmesh p => (IMsbPart)p.DeepCopy(),
            _ => data.Profile.BigEndian ? new MSBD.Part.Navmesh() : new MSB1.Part.Navmesh(),
        };

        var names = map.Objects.Select(o => (o.WrappedObject as IMsbEntry)?.Name).Where(n => n != null).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var name = model;
        for (int i = 0; names.Contains(name); i++)
            name = $"{model}_{i:D4}";

        part.Name = name;
        part.ModelName = model;
        part.Position = position;
        part.Rotation = rotation;

        // The navmesh group of the collision it was made from (the game's parts set one bit, as their collision does)
        var groups = part switch { MSB1.Part.Navmesh p => p.NvmGroups, MSBD.Part.Navmesh p => p.NvmGroups, _ => null };
        if (groups != null && nvmGroups != null)
            Array.Copy(nvmGroups, groups, Math.Min(groups.Length, nvmGroups.Length));

        return part;
    }

    /// <summary>The map as it will be once a new room (part + NVM) is added, for marking its gates.</summary>
    private static NvmMap WithNewRoom(NvmMap data, string file, NVM nvm, IMsbPart part, Matrix4x4 world, MCP mcp, MCG mcg)
    {
        var files = data.Files.Select(f => (f.Key, f.Value)).Append((file, nvm)).ToList();
        var parts = data.Rooms.Select(r => (new NvmReader.PartInfo(r.PartName, r.ModelName, default, default, default, r.Part), r.World))
            .Append((new NvmReader.PartInfo(part.Name, part.ModelName, default, default, default, part), world)).ToList();
        return NvmReader.Build(data.MapId, data.Profile, files, parts, mcp, mcg);
    }

    /// <summary>
    /// Adds a navmesh: the MSB part, its NVM (gates marked where it meets the others, if asked) and its MCP room, as one
    /// undoable action.
    /// </summary>
    private void AddNvmRoom(MapContainer map, NvmMap data, NVM nvm, object template, Vector3 position, Vector3 rotation, uint[] nvmGroups, bool markGates, string done)
    {
        var (model, file) = NvmModelEdit.NewNames(data);
        if (model == null)
        {
            SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Models_No_Name"), true);
            return;
        }

        var part = NewNvmPart(map, data, template, model, position, rotation, nvmGroups);
        var world = NvmPlacement.PartWorld(position, rotation, part.Scale);
        var (mcp, mcg) = NvmModelEdit.WithRoomAdded(data, nvm, world);

        var files = new List<NavigationNvmEditAction.FileChange>();
        int gates = 0;

        if (markGates)
        {
            var after = WithNewRoom(data, file, nvm, part, world, mcp, mcg);
            foreach (var (name, flagged) in NvmModelEdit.MarkGates(after, after.Rooms[^1], NvmGateTolerance, out gates))
            {
                if (name.Equals(file, StringComparison.OrdinalIgnoreCase))
                    nvm = flagged;
                else
                    files.Add(new(name, data.Files[name], flagged));
            }
        }

        files.Insert(0, new(file, null, nvm));

        var entity = new MsbEntity(View.Universe, map, part, MsbEntityType.Part);
        var actions = new List<ViewportAction>
        {
            new EntAddAction(View, map, new List<MsbEntity> { entity }, map.RootObject),
            new NavigationNvmEditAction(map, Nvm, files, data.Mcp, mcp ?? data.Mcp, data.Mcg, mcg ?? data.Mcg, OnNvmEdited),
        };

        View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));

        NvmModelRoom = data.Rooms.Count;
        SetNvmMessage(LOC.Get(done, part.Name, file, nvm.Triangles.Count, gates));
        Smithbox.Log(this, NvmMessage);
    }

    private void DuplicateNvmRoom(MapContainer map, NvmMap data, NvmRoom source, Vector3 offset)
    {
        var (position, rotation) = source.Part switch
        {
            MSB1.Part.Navmesh p => (p.Position, p.Rotation),
            MSBD.Part.Navmesh p => (p.Position, p.Rotation),
            _ => (Vector3.Zero, Vector3.Zero),
        };

        // The NVM is in its part's space: the copy is the same file on a part moved by the offset. Its gate flags are
        // those of the source's place, so they are cleared before they are marked again where it now is
        var nvm = NvmEdit.Copy(source.Nvm);
        foreach (var t in nvm.Triangles)
            t.Flags &= ~NVM.TriangleFlags.GATE;
        nvm.Entities.Clear();

        AddNvmRoom(map, data, nvm, source.Part, position + offset, rotation, null, true, "MAP_Navigation_Nvm_Duplicated");
    }

    private void MarkNvmGates(MapContainer map, NvmMap data, NvmRoom room)
    {
        var changed = NvmModelEdit.MarkGates(data, room, NvmGateTolerance, out int marked);

        if (changed.Count == 0)
        {
            SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Mark_Gates_None", room.PartName));
            return;
        }

        var files = changed.Select(c => new NavigationNvmEditAction.FileChange(c.Key, data.Files[c.Key], c.Value)).ToList();
        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(map, Nvm, files, data.Mcp, data.Mcp, data.Mcg, data.Mcg, OnNvmEdited));
        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Mark_Gates_Done", marked, files.Count));
    }

    private void RemoveNvmRoom(MapContainer map, NvmMap data, NvmRoom room)
    {
        var entity = PartEntity(map, room);
        if (entity == null)
        {
            SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Remove_No_Part", room.PartName), true);
            return;
        }

        var (mcp, mcg) = NvmModelEdit.WithRoomRemoved(data, room.Index);

        // The NVM goes too, unless another part loads it
        var files = new List<NavigationNvmEditAction.FileChange>();
        if (room.FileName != null && data.Rooms.All(r => r.Index == room.Index || !string.Equals(r.FileName, room.FileName, StringComparison.OrdinalIgnoreCase)))
            files.Add(new(room.FileName, room.Nvm, null));

        var actions = new List<ViewportAction>
        {
            new NavigationNvmEditAction(map, Nvm, files, data.Mcp, mcp ?? data.Mcp, data.Mcg, mcg ?? data.Mcg, OnNvmEdited),
            new EntDeleteAction(View, new List<MsbEntity> { entity }, true),
        };

        View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));

        int edges = (data.Mcg?.Edges.Count ?? 0) - (mcg?.Edges.Count ?? 0);
        NvmModelRoom = -1;
        SetNvmPicked(-1, -1, default);
        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Removed", room.PartName, files.Count, edges));
    }

    #endregion

    #region Generate

    /// <summary>
    /// The selected collision parts' triangles in map space, each part wound to face up (NvmModelEdit.FacingUp). Null
    /// for a part whose collision file isn't loaded.
    /// </summary>
    private List<Vector3> NvmSourceTriangles(GenerateSource source)
    {
        var names = View.HavokCollisionBank.GetMapCollisionNames(source.MapId)
            .Where(n => n.StartsWith("h", StringComparison.OrdinalIgnoreCase)).ToList();
        var stem = NvmMap.FileFor(source.Model, names.Select(n => n.Split('.')[0]));
        var file = stem == null ? null : names.First(n => n.Split('.')[0].Equals(stem, StringComparison.OrdinalIgnoreCase));
        var document = file != null ? View.HavokCollisionBank.GetMapDocument(file) : null;

        if (document == null)
            return null;

        var world = source.Part.GetWorldMatrix();
        var triangles = Havok.Shared.CollisionGeometry.Submeshes(document.Objects).SelectMany(t => t).Select(v => Vector3.Transform(v, world)).ToList();
        return NvmModelEdit.FacingUp(triangles);
    }

    private void StartNvmGenerate(MapContainer map, List<GenerateSource> sources, NvmRoom replace)
    {
        var input = new List<Vector3>();
        int missing = 0;

        foreach (var source in sources)
        {
            if (NvmSourceTriangles(source) is List<Vector3> triangles)
                input.AddRange(triangles);
            else
                missing++;
        }

        if (input.Count < 3)
        {
            SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Generate_No_Collision", missing), true);
            return;
        }

        // A replaced navmesh keeps its part, so the faces go into the part's space
        var toLocal = Matrix4x4.Identity;
        if (replace != null && !Matrix4x4.Invert(replace.World, out toLocal))
            toLocal = Matrix4x4.Identity;

        var parameters = NvmGenerateParameters with { Links = false };
        bool bigEndian = Nvm.Profile.BigEndian;
        var groups = sources.Select(s => s.Part.WrappedObject switch { MSB1.Part.Collision c => c.NvmGroups, MSBD.Part.Collision c => c.NvmGroups, _ => null }).FirstOrDefault(g => g != null);
        var replaceName = replace?.FileName;
        bool markGates = NvmGenerateMarkGates;
        int run = ++NvmGenerateRun;
        NvmGenerating = true;
        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Generate_Started", input.Count / 3, missing));

        Task.Run(() =>
        {
            NavGenPolygons generated;
            NVM nvm = null;

            try
            {
                generated = NavigationGenerator.GeneratePolygons(input, parameters);
                if (generated.Ok)
                {
                    var local = generated.Polygons.Select(p => (IReadOnlyList<Vector3>)p.Select(v => Vector3.Transform(v, toLocal)).ToList()).ToList();
                    nvm = NvmModelEdit.FromPolygons(local, bigEndian);
                }
            }
            catch (Exception e)
            {
                generated = new NavGenPolygons { Error = "MAP_Navigation_Generate_Error_Recast" };
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Nvm_Generate_Failed"), e);
            }

            NvmGenerateResults.Enqueue(() =>
            {
                if (run != NvmGenerateRun)
                    return;

                NvmGenerating = false;

                if (!generated.Ok || nvm == null || nvm.Triangles.Count == 0)
                {
                    SetNvmMessage(LOC.Get(generated.Error ?? "MAP_Navigation_Generate_Error_Empty"), true);
                    return;
                }

                var data = Nvm.GetMap(map);
                if (data == null)
                    return;

                if (replaceName == null)
                {
                    AddNvmRoom(map, data, nvm, null, Vector3.Zero, Vector3.Zero, groups, markGates, "MAP_Navigation_Nvm_Generated");
                    return;
                }

                ReplaceNvm(map, data, replaceName, nvm, markGates);
            });
        });
    }

    /// <summary>
    /// Puts a generated NVM in place of a room's: its MCP box set from it, and its edges' gate triangles found again
    /// (the triangle numbers changed), as one action.
    /// </summary>
    private void ReplaceNvm(MapContainer map, NvmMap data, string fileName, NVM nvm, bool markGates)
    {
        var room = data.Rooms.FirstOrDefault(r => string.Equals(r.FileName, fileName, StringComparison.OrdinalIgnoreCase));
        if (room == null)
            return;

        var before = room.Nvm;
        var files = new List<NavigationNvmEditAction.FileChange>();
        int gates = 0;

        // The map with the new NVM, for its gates, room box and edges
        NvmMap With(NVM replaced) => NvmReader.Build(data.MapId, data.Profile,
            data.Files.Select(f => (f.Key, f.Key.Equals(fileName, StringComparison.OrdinalIgnoreCase) ? replaced : f.Value)).ToList(),
            data.Rooms.Select(r => (new NvmReader.PartInfo(r.PartName, r.ModelName, default, default, default, r.Part), r.World)).ToList(), data.Mcp, data.Mcg);

        var after = With(nvm);

        if (markGates)
        {
            foreach (var (name, flagged) in NvmModelEdit.MarkGates(after, after.Rooms[room.Index], NvmGateTolerance, out gates))
            {
                if (name.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    nvm = flagged;
                else
                    files.Add(new(name, data.Files[name], flagged));
            }

            after = With(nvm);
        }

        files.Insert(0, new(fileName, before, nvm));

        var mcp = NvmEdit.WithRoomBoxes(data, data.Mcp, data.Rooms.Where(r => r.FileName == fileName).Select(r => (r, before, nvm)));
        var mcg = NvmModelEdit.WithEdgeGatesFound(after, data.Mcg, after.Rooms[room.Index]);

        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(map, Nvm, files, data.Mcp, mcp, data.Mcg, mcg, OnNvmEdited));
        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Regenerated", room.PartName, nvm.Triangles.Count, gates));
        Smithbox.Log(this, NvmMessage);
    }

    /// <summary>Applies finished generations; called each frame.</summary>
    private void DrainNvmGenerateResults()
    {
        while (NvmGenerateResults.TryDequeue(out var apply))
            apply();
    }

    #endregion

    #region Graph: nodes

    /// <summary>The Graph tab's node actions: add a node at the picked point, remove the chosen one, link the picked room.</summary>
    private void DisplayNvmNodeActions(NvmMap data)
    {
        // Nodes
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Navigation_Nvm_Nodes_Header"), LOC.Get("MAP_Navigation_Nvm_Nodes_Header_TT"));

        var room = PickedNvmRoom(data);
        bool hasNode = data.Mcg != null && NvmSelectedNode >= 0 && NvmSelectedNode < data.Mcg.Nodes.Count;
        int node = NvmSelectedNode;

        GUI.ConditionalMultiButtonInput("navigationNvmNodeEdits",
            "navigationNvmNodeAdd", LOC.Get("MAP_Navigation_Nvm_Node_Add"), LOC.Get("MAP_Navigation_Nvm_Node_Add_TT"), () =>
            {
                var after = NvmModelEdit.WithNode(data.Mcg, NvmPickedPoint);
                ExecuteNvmGraph(after);
                SelectNvmNode(after.Nodes.Count - 1);
            }, data.Mcg != null && room != null,

            "navigationNvmNodeRemove", LOC.Get("MAP_Navigation_Nvm_Node_Remove"), LOC.Get("MAP_Navigation_Nvm_Node_Remove_TT"), () =>
            {
                ExecuteNvmGraph(NvmModelEdit.WithoutNode(data.Mcg, node));
                SelectNvmNode(-1);
            }, hasNode,

            "navigationNvmRoomLink", LOC.Get("MAP_Navigation_Nvm_Room_Link"), LOC.Get("MAP_Navigation_Nvm_Room_Link_TT"),
            () => LinkNvmRoom(data, room), data.Mcg != null && room != null);
    }

    private void LinkNvmRoom(NvmMap data, NvmRoom room)
    {
        var (mcp, mcg, added) = NvmModelEdit.WithRoomLinked(data, room, 2f);

        if (added == 0 && ReferenceEquals(mcp, data.Mcp))
        {
            SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Room_Link_None", room.PartName));
            return;
        }

        View.ViewportActionManager.ExecuteAction(new NavigationNvmEditAction(NvmContainer, Nvm, null, data.Mcp, mcp, data.Mcg, mcg, OnNvmEdited));
        SetNvmMessage(LOC.Get("MAP_Navigation_Nvm_Room_Linked", room.PartName, added));
    }

    #endregion
}
