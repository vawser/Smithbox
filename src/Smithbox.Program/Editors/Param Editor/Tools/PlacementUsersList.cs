using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The "Used by" list of the map parts a param row places or changes, from <see cref="NrPlacementIndex"/>, with its
/// Scan Maps / Rescan button. A click shows the part in the Map Editor.
/// </summary>
public static class PlacementUsersList
{
    /// <param name="query">Picks the parts from the scanned index</param>
    /// <param name="id">A unique ID suffix within the window</param>
    /// <param name="tooltip">What the list holds</param>
    public static void Display(ProjectEntry project, Func<NrPlacementIndex, List<PlacementRecord>> query, string id, string tooltip)
    {
        ImGui.TextUnformatted(LOC.Get("PARAM_RandomAppear_Used_By"));
        GUI.Tooltip(tooltip);

        var index = NrPlacementIndex.For(project);

        ImGui.SameLine();

        if (index.IsScanning)
        {
            ImGui.TextDisabled(LOC.Get("PARAM_RandomAppear_Scanning", index.Progress, index.Total));
            return;
        }

        var scanLabel = index.IsScanned ? LOC.Get("PARAM_RandomAppear_Rescan") : LOC.Get("PARAM_RandomAppear_Scan");

        if (ImGui.SmallButton($"{scanLabel}##placementUsersScan_{id}"))
            index.Scan();
        GUI.Tooltip(LOC.Get("PARAM_RandomAppear_Scan_TT"));

        if (!index.IsScanned)
            return;

        ImGui.SameLine();
        ImGui.TextDisabled(LOC.Get("PARAM_RandomAppear_Scanned_At", index.ScannedAt.ToString("HH:mm")));

        var users = query(index);

        if (users.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("PARAM_RandomAppear_Unused"));
            return;
        }

        for (var i = 0; i < users.Count; i++)
        {
            var user = users[i];
            var label = user.EntityID > 0
                ? $"{user.MapID}: {user.Name} ({user.EntityID})"
                : $"{user.MapID}: {user.Name}";

            if (ImGui.Selectable($"{label}##placementUser_{id}_{i}"))
            {
                EditorCommandQueue.AddCommand($"map/load/{user.MapID}");
                EditorCommandQueue.AddCommand($"map/select/{user.MapID}/{user.Name}/{nameof(MsbEntityType.Part)}");
            }
            GUI.Tooltip(LOC.Get("PARAM_RandomAppear_User_TT", user.PartType));
        }
    }
}
