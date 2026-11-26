using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Properties panel information for connect collisions. ER: what a ConnectCollision does in plain words (target map,
/// linked collision, target groups, load level, world-state gate, the map's limits, connections back) and, on a
/// Collision, its connections and Create Connect. Other games: the target map name. Rules in
/// <see cref="ConnectCollisions" />; plan Claude/Documentation/Connect Collision Tooling.md (C2, C3).
/// </summary>
public static class PropInfo_Part_ConnectCollision
{
    // Create Connect's target, kept for the collision it was typed for
    private static string CreateTarget = "";
    private static object CreateTargetFor;

    public static void Display(MapEditorView view, Entity firstEnt)
    {
        switch (firstEnt.WrappedObject)
        {
            case MSBE.Part.ConnectCollision connect when firstEnt.Container is MapContainer map:
                DisplayConnect(view, map, firstEnt, connect);
                break;
            case MSBE.Part.Collision collision when firstEnt.Container is MapContainer map:
                DisplayCollision(view, map, firstEnt, collision);
                break;
            default:
                if (EntityHelper.IsPartConnectCollision(firstEnt))
                    DisplayTargetName(view, firstEnt);
                break;
        }
    }

    private static void DisplayConnect(MapEditorView view, MapContainer map, Entity ent, MSBE.Part.ConnectCollision connect)
    {
        var warning = UI.Current.ImGui_Warning_Text_Color;
        var target = ConnectCollisions.TargetName(connect.MapID);

        ImGui.Separator();
        ImGui.Text(LOC.Get("MAP_ConnectCollision_Header"));
        GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Header_TT"));
        ImGui.Separator();

        // Target map
        if (target == null)
        {
            GUI.WrappedTextColored(warning, LOC.Get("MAP_ConnectCollision_No_Target"));
        }
        else
        {
            ImGui.Text(LOC.Get("MAP_ConnectCollision_Target", target));
            GUI.DisplayAlias(AliasHelper.GetMapNameAlias(view.Project, target));

            var targetMap = view.Selection.GetMapContainerFromMapID(target);

            if (targetMap is { LoadState: MapContentLoadState.Loaded })
            {
                if (GUI.ExplainedButton(LOC.Get("MAP_ConnectCollision_View_From_Here"), LOC.Get("MAP_ConnectCollision_View_From_Here_TT", target), true))
                    view.DisplayGroupTool.ViewFromConnect(ent);
            }
            else if (targetMap == null)
            {
                if (MapExists(view, target))
                {
                    if (GUI.ExplainedButton(LOC.Get("MAP_ConnectCollision_Load_Target", target), LOC.Get("MAP_ConnectCollision_Load_Target_TT"), true))
                        view.Universe.LoadMap(target, false, false, true);
                }
                else
                {
                    GUI.WrappedTextColored(warning, LOC.Get("MAP_ConnectCollision_Target_Missing", target));
                }
            }
        }

        // Linked collision
        Entity collisionEnt = null;

        if (string.IsNullOrEmpty(connect.CollisionName))
        {
            GUI.WrappedTextColored(warning, LOC.Get("MAP_ConnectCollision_No_Collision"));
        }
        else
        {
            collisionEnt = map.Objects.FirstOrDefault(e => e.WrappedObject is MSBE.Part.Collision && e.Name == connect.CollisionName);

            if (collisionEnt == null)
            {
                GUI.WrappedTextColored(warning, LOC.Get("MAP_ConnectCollision_Collision_Missing", connect.CollisionName));
            }
            else
            {
                if (GUI.ExplainedButton(LOC.Get("MAP_ConnectCollision_Select_Collision", connect.CollisionName), LOC.Get("MAP_ConnectCollision_Select_Collision_TT"), true))
                    Select(view, collisionEnt);

                var collision = (MSBE.Part.Collision)collisionEnt.WrappedObject;

                if (!ConnectCollisions.MatchesCollision(connect, collision))
                {
                    GUI.WrappedTextColored(warning, LOC.Get("MAP_ConnectCollision_Mismatch"));

                    if (GUI.ExplainedButton(LOC.Get("MAP_ConnectCollision_Match"), LOC.Get("MAP_ConnectCollision_Match_TT"), true))
                    {
                        var actions = ConnectCollisions.MatchCollision(ent, collision);

                        if (actions.Count > 0)
                            view.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));
                    }
                }
            }
        }

        // What it does
        GUI.Spacer();

        var standOn = string.IsNullOrEmpty(connect.CollisionName) ? LOC.Get("MAP_ConnectCollision_Its_Collision") : connect.CollisionName;
        var groups = ConnectCollisions.GroupCount(connect);

        if (target != null)
        {
            GUI.WrappedText(groups == 0
                ? LOC.Get("MAP_ConnectCollision_Groups_None", target, standOn)
                : LOC.Get("MAP_ConnectCollision_Groups", groups, target, standOn));

            if (ConnectCollisions.IsOpenWorldArea(connect.MapID[0]))
            {
                if (ConnectCollisions.LoadLevels.Contains(connect.UnkT08))
                    GUI.WrappedText(LOC.Get("MAP_ConnectCollision_Load_Level", ConnectCollisions.EffectiveLoadLevel(connect.UnkT08)));
                else
                    GUI.WrappedTextColored(warning, LOC.Get("MAP_ConnectCollision_Load_Level_Bad", connect.UnkT08));
            }
        }

        GUI.WrappedText(DescribeGate(connect));

        // The map's limits
        var (targets, areas) = ConnectCollisions.Usage(map);
        var over = targets.Count > ConnectCollisions.MaxTargets || areas.Count > ConnectCollisions.MaxForeignAreas;
        var usage = LOC.Get("MAP_ConnectCollision_Usage", targets.Count, ConnectCollisions.MaxTargets, areas.Count, ConnectCollisions.MaxForeignAreas);

        if (over)
            GUI.WrappedTextColored(warning, usage);
        else
            GUI.WrappedText(usage);

        GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Usage_TT"));

        // Connections back, when the target map is loaded
        if (target != null && view.Selection.GetMapContainerFromMapID(target) is { LoadState: MapContentLoadState.Loaded } loadedTarget)
        {
            var back = ConnectCollisions.ConnectsIn(loadedTarget)
                .Where(e => string.Equals(ConnectCollisions.TargetName(((MSBE.Part.ConnectCollision)e.WrappedObject).MapID), map.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            GUI.Spacer();

            if (back.Count == 0)
            {
                GUI.WrappedText(LOC.Get("MAP_ConnectCollision_Back_None", target));
            }
            else
            {
                ImGui.Text(LOC.Get("MAP_ConnectCollision_Back", target, back.Count));
                GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Back_TT"));

                foreach (var b in back)
                {
                    if (ImGui.Selectable($"{b.Name}##connectBack_{b.Name}"))
                        Select(view, b);
                }
            }

            if (collisionEnt != null)
                DisplayReciprocal(view, map, collisionEnt, connect, loadedTarget);
        }

        ImGui.Text("");
    }

    // Create Reciprocal: the candidates for the selected connect, refreshed once a second, and the picked one
    private static object ReciprocalFor;
    private static double ReciprocalTime;
    private static List<ConnectCollisions.ReciprocalCandidate> ReciprocalList;
    private static Entity ReciprocalPick;

    /// <summary>
    /// C5: a collision of the target map near the linked collision, picked from the nearest few, gets a connect back
    /// to this map.
    /// </summary>
    private static void DisplayReciprocal(MapEditorView view, MapContainer map, Entity collisionEnt,
        MSBE.Part.ConnectCollision connect, MapContainer target)
    {
        var now = ImGui.GetTime();

        if (ReciprocalFor != connect || now - ReciprocalTime > 1.0)
        {
            if (ReciprocalFor != connect)
                ReciprocalPick = null;

            ReciprocalFor = connect;
            ReciprocalTime = now;
            ReciprocalList = ConnectCollisions.ReciprocalCandidates(map, collisionEnt, target);

            // The best one, unless it already links back or the user picked another still in the list
            if (ReciprocalList != null && (ReciprocalPick == null || ReciprocalList.All(c => c.Collision != ReciprocalPick)))
                ReciprocalPick = ReciprocalList.FirstOrDefault(c => !c.AlreadyLinked)?.Collision;
        }

        GUI.Spacer();
        ImGui.Text(LOC.Get("MAP_ConnectCollision_Reciprocal_Header", target.Name));
        GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Reciprocal_Header_TT", ConnectCollisions.ReciprocalMaxGap));

        if (ReciprocalList == null)
        {
            GUI.WrappedText(LOC.Get("MAP_ConnectCollision_Reciprocal_No_Mesh", collisionEnt.Name));
            return;
        }

        if (ReciprocalList.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_ConnectCollision_Reciprocal_None", target.Name, ConnectCollisions.ReciprocalMaxGap));
            return;
        }

        foreach (var c in ReciprocalList)
        {
            var where = c.Overlap > 0
                ? LOC.Get("MAP_ConnectCollision_Reciprocal_Overlaps", (int)MathF.Round(c.Overlap * 100))
                : LOC.Get("MAP_ConnectCollision_Reciprocal_Gap", c.Gap.ToString("0.0"));

            var label = c.AlreadyLinked
                ? $"{c.Collision.Name}  {where}  {LOC.Get("MAP_ConnectCollision_Reciprocal_Linked")}"
                : $"{c.Collision.Name}  {where}";

            if (ImGui.Selectable($"{label}##reciprocal_{c.Collision.Name}", c.Collision == ReciprocalPick))
                ReciprocalPick = c.Collision;

            GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Reciprocal_Pick_TT"));
        }

        var reason = ReciprocalPick == null
            ? LOC.Get("MAP_ConnectCollision_Reciprocal_Need_Pick")
            : ConnectCollisions.CannotConnect(target, map.Name);

        var label2 = ReciprocalPick == null
            ? LOC.Get("MAP_ConnectCollision_Reciprocal_Create_None")
            : LOC.Get("MAP_ConnectCollision_Reciprocal_Create", ReciprocalPick.Name);

        if (GUI.ExplainedButton(label2, reason ?? LOC.Get("MAP_ConnectCollision_Reciprocal_Create_TT", target.Name, map.Name), reason == null))
        {
            var created = ConnectCollisions.Prepare(view, target, ReciprocalPick, map.Name);
            var parent = ReciprocalPick.Parent ?? target.RootObject;

            view.ViewportActionManager.ExecuteAction(new EntAddAction(view, target, new List<MsbEntity> { created }, parent));
            ReciprocalFor = null;

            // Ready to pick the groups of this map it shows (the Display Groups tool's Connect Target Groups)
            view.DisplayGroupTool.ViewFromConnect(created);
        }

        // A start for this connect's own target groups: what the picked collision shows of its map
        if (ReciprocalPick != null)
        {
            var pickGroups = ReciprocalPick.DisplayGroups;
            var own = connect.DisplayGroupStruct?.DispGroups;
            var usable = pickGroups != null && own != null && pickGroups.Any(w => w != 0);
            var copyReason = usable ? null : LOC.Get("MAP_ConnectCollision_Copy_Groups_None", ReciprocalPick.Name);

            if (GUI.ExplainedButton(LOC.Get("MAP_ConnectCollision_Copy_Groups", ReciprocalPick.Name),
                copyReason ?? LOC.Get("MAP_ConnectCollision_Copy_Groups_TT", ReciprocalPick.Name, target.Name), usable))
            {
                var next = new uint[own.Length];
                for (var i = 0; i < next.Length && i < pickGroups.Length; i++)
                    next[i] = pickGroups[i];

                if (!own.SequenceEqual(next))
                {
                    var change = new DisplayGroupEditAction.Change
                    {
                        Target = own,
                        OldValues = (uint[])own.Clone(),
                        NewValues = next,
                        Owner = ConnectEntityOf(map, connect)
                    };

                    view.ViewportActionManager.ExecuteAction(new DisplayGroupEditAction(new List<DisplayGroupEditAction.Change> { change }));
                }
            }
        }
    }

    private static Entity ConnectEntityOf(MapContainer map, MSBE.Part.ConnectCollision connect)
    {
        return map.Objects.FirstOrDefault(e => e.WrappedObject == connect);
    }

    private static string DescribeGate(MSBE.Part.ConnectCollision connect)
    {
        return connect.UnkT0A switch
        {
            0 or 10 or 30 => LOC.Get(connect.UnkT0B ? "MAP_ConnectCollision_Gate_Off" : "MAP_ConnectCollision_Gate_On", 300 + connect.UnkT0A),
            11 or 12 => LOC.Get(connect.UnkT0B ? "MAP_ConnectCollision_Gate_Always" : "MAP_ConnectCollision_Gate_Never"),
            _ => LOC.Get("MAP_ConnectCollision_Gate_Always")
        };
    }

    private static void DisplayCollision(MapEditorView view, MapContainer map, Entity ent, MSBE.Part.Collision collision)
    {
        if (CreateTargetFor != collision)
        {
            CreateTargetFor = collision;
            CreateTarget = "";
        }

        var connects = ConnectCollisions.ConnectsOf(map, collision.Name);

        ImGui.Separator();
        ImGui.Text(LOC.Get("MAP_ConnectCollision_Collision_Header", connects.Count));
        GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Collision_Header_TT"));
        ImGui.Separator();

        foreach (var c in connects)
        {
            var target = ConnectCollisions.TargetName(((MSBE.Part.ConnectCollision)c.WrappedObject).MapID) ?? "-";

            if (ImGui.Selectable($"{c.Name}  →  {target}##connectOf_{c.Name}"))
                Select(view, c);

            GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Select_Connect_TT"));
        }

        // Connects that no longer match the collision (it was moved, or its model changed)
        var mismatched = connects.Where(c => !ConnectCollisions.MatchesCollision((MSBE.Part.ConnectCollision)c.WrappedObject, collision)).ToList();

        if (mismatched.Count > 0)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_ConnectCollision_Mismatch_Count", mismatched.Count));

            if (GUI.ExplainedButton(LOC.Get("MAP_ConnectCollision_Match_All"), LOC.Get("MAP_ConnectCollision_Match_All_TT"), true))
            {
                var actions = mismatched.SelectMany(c => ConnectCollisions.MatchCollision(c, collision)).ToList();

                if (actions.Count > 0)
                    view.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));
            }
        }

        // Create Connect: the target is applied with the button only
        GUI.Spacer();
        ImGui.Text(LOC.Get("MAP_ConnectCollision_Create_Target"));
        GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Create_Target_TT"));

        GUI.SetInputWidth();
        ImGui.InputText("##connectCreateTarget", ref CreateTarget, 32);

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##connectCreateSuggest", LOC.Get("MAP_ConnectCollision_Create_Suggestions")))
        {
            foreach (var suggestion in Suggestions(view, map))
            {
                var label = suggestion;
                var alias = AliasHelper.GetMapNameAlias(view.Project, suggestion);

                if (!string.IsNullOrEmpty(alias))
                    label = $"{suggestion}  {alias}";

                if (ImGui.Selectable($"{label}##connectSuggest_{suggestion}", suggestion == CreateTarget))
                    CreateTarget = suggestion;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Create_Suggestions_TT"));

        var reason = string.IsNullOrWhiteSpace(CreateTarget) ? LOC.Get("MAP_ConnectCollision_Create_Need_Target") : ConnectCollisions.CannotConnect(map, CreateTarget);
        var targetName = reason == null ? ConnectCollisions.TargetName(ConnectCollisions.MapIdFor(CreateTarget)) : null;

        if (reason == null && !MapExists(view, targetName))
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_ConnectCollision_Target_Missing", targetName));

        if (GUI.ExplainedButton(LOC.Get("MAP_ConnectCollision_Create"), reason ?? LOC.Get("MAP_ConnectCollision_Create_TT", targetName), reason == null))
        {
            var created = ConnectCollisions.Prepare(view, map, ent, targetName);
            var parent = ent.Parent ?? map.RootObject;

            view.ViewportActionManager.ExecuteAction(new EntAddAction(view, map, new List<MsbEntity> { created }, parent));
            CreateTarget = "";
        }

        ImGui.Text("");
    }

    /// <summary>Loaded maps, then the map's tile relations, without the map itself</summary>
    private static List<string> Suggestions(MapEditorView view, MapContainer map)
    {
        var list = new List<string>();

        foreach (var entry in view.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null && entry.Key.Filename != map.Name)
                list.Add(entry.Key.Filename);
        }

        foreach (var related in MapConnections_ER.GetRelatedMaps(view, map.Name).Keys)
        {
            if (related != map.Name && !list.Contains(related))
                list.Add(related);
        }

        return list;
    }

    private static bool MapExists(MapEditorView view, string mapName)
    {
        var entries = view.Project.Locator?.MapFiles?.Entries;

        return entries == null || entries.Any(e => string.Equals(e.Filename, mapName, StringComparison.OrdinalIgnoreCase));
    }

    private static void Select(MapEditorView view, Entity ent)
    {
        view.ViewportSelection.ClearSelection();
        view.ViewportSelection.AddSelection(ent);
        view.FrameAction.ApplyViewportFrame();
    }

    /// <summary>Other games: the target map's name, with 255 shown as 00</summary>
    private static void DisplayTargetName(MapEditorView view, Entity firstEnt)
    {
        var mapIds = PropFinderUtil.FindPropertyValue("MapID", firstEnt.WrappedObject) as byte[];

        if (mapIds == null)
            return;

        var mapString = "m" + string.Join("_", mapIds.Select(n => (n == 255 ? 0 : n).ToString("D2")));

        ImGui.Separator();
        ImGui.Text(LOC.Get("MAP_ConnectCollision_Target_Header"));
        ImGui.Separator();

        ImGui.Text(mapString);
        GUI.DisplayAlias(AliasHelper.GetMapNameAlias(view.Project, mapString));
        ImGui.Text("");
    }
}
