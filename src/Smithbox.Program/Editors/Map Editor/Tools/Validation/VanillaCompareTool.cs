using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Vanilla Compare: what the active map changes from its vanilla MSB. Lists its modified and added objects (click to
/// select) and the vanilla objects it no longer has, each with Restore. Compared on request, as a whole map takes a
/// moment; the list says when edits have made it out of date. Plan: Documentation/Map Editor Vanilla Compare.md.
/// </summary>
public class VanillaCompareTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private MapContainer ComparedMap;
    private int ComparedVersion;
    private bool HasVanilla;

    private List<Entity> Modified = new();
    private List<Entity> Added = new();
    private List<IMsbEntry> Removed = new();

    public VanillaCompareTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MapVanillaCompare Compare => View.Editor.VanillaCompare;

    public void OnToolWindow()
    {
        GUI.WrappedText(LOC.Get("MAP_Vanilla_Tool_Hint"));

        var map = View.Selection.SelectedMapContainer;

        if (map == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Vanilla_Tool_No_Map"));
            return;
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Vanilla_Tool_Actions_Header"), LOC.Get("MAP_Vanilla_Tool_Actions_Header_TT"));

        GUI.MultiButtonInput("vanillaCompareActions",
            "vanillaCompareRun", LOC.Get("MAP_Vanilla_Tool_Compare"), LOC.Get("MAP_Vanilla_Tool_Compare_TT", map.Name),
            () => Run(map));

        if (ComparedMap == null)
            return;

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Vanilla_Tool_Results_Header", ComparedMap.Name), LOC.Get("MAP_Vanilla_Tool_Results_Header_TT"));

        if (ComparedMap.LoadState is not MapContentLoadState.Loaded)
        {
            GUI.WrappedText(LOC.Get("MAP_Vanilla_Tool_Unloaded"));
            return;
        }

        if (!HasVanilla)
        {
            GUI.WrappedText(LOC.Get("MAP_Vanilla_Compare_No_Vanilla", ComparedMap.Name));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_Vanilla_Tool_Summary", Modified.Count, Added.Count, Removed.Count));

        if ((ComparedMap.OwnerView?.ViewportActionManager.Version ?? 0) != ComparedVersion)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_Vanilla_Tool_Out_Of_Date"));
        }

        ImGui.BeginChild("vanillaCompareLists", ImGuiChildFlags.Borders);

        if (ImGui.BeginTabBar("##vanillaCompareTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Vanilla_Tool_Tab_Modified", Modified.Count)}##vanillaTabModified"))
            {
                DisplayEntities(Modified, "modified", UI.Current.ImGui_MapEditor_VanillaModified_Text);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Vanilla_Tool_Tab_Added", Added.Count)}##vanillaTabAdded"))
            {
                DisplayEntities(Added, "added", UI.Current.ImGui_MapEditor_VanillaAdded_Text);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_Vanilla_Tool_Tab_Removed", Removed.Count)}##vanillaTabRemoved"))
            {
                DisplayRemoved();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.EndChild();
    }

    private void Run(MapContainer map)
    {
        ComparedMap = map;
        ComparedVersion = map.OwnerView?.ViewportActionManager.Version ?? 0;
        HasVanilla = Compare.HasVanilla(map);

        Modified = new();
        Added = new();
        Removed = new();

        if (!HasVanilla)
            return;

        foreach (var ent in map.Objects)
        {
            switch (Compare.GetStatus(ent))
            {
                case VanillaStatus.Modified:
                    Modified.Add(ent);
                    break;
                case VanillaStatus.Added:
                    Added.Add(ent);
                    break;
            }
        }

        Removed = Compare.GetRemoved(map).ToList();
    }

    private void DisplayEntities(List<Entity> entities, string id, System.Numerics.Vector4 colour)
    {
        if (entities.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Vanilla_Tool_None"));
            return;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(entities.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var ent = entities[i];
                var current = ComparedMap.Objects.Contains(ent);

                ImGui.PushStyleColor(ImGuiCol.Text, colour);
                var clicked = ImGui.Selectable($"{ent.WrappedObject?.GetType().Name}: {ent.PrettyName}##vanilla_{id}_{i}", false,
                    current ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled);
                ImGui.PopStyleColor();

                if (clicked)
                {
                    View.ViewportSelection.ClearSelection();
                    View.ViewportSelection.AddSelection(ent);
                    View.ViewportSelection.GotoTreeTarget = ent;
                    View.FrameAction.ApplyViewportFrame();
                }

                GUI.Tooltip(current ? LOC.Get("MAP_Vanilla_Tool_Select_TT") : LOC.Get("MAP_Vanilla_Tool_Stale_TT"));
            }
        }

        clipper.End();
    }

    private void DisplayRemoved()
    {
        if (Removed.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Vanilla_Tool_None"));
            return;
        }

        IMsbEntry restore = null;

        var clipper = new ImGuiListClipper();
        clipper.Begin(Removed.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var entry = Removed[i];

                if (ImGui.Button($"{LOC.Get("MAP_Vanilla_Tool_Restore")}##vanillaRestore{i}"))
                {
                    restore = entry;
                }
                GUI.Tooltip(LOC.Get("MAP_Vanilla_Tool_Restore_TT"));

                ImGui.SameLine();
                ImGui.TextUnformatted($"{entry.GetType().Name}: {entry.Name}");
            }
        }

        clipper.End();

        // After the list, so it isn't changed while it is drawn
        if (restore != null && ComparedMap != null)
        {
            // A name taken since (an object added under it) would make the restored one ambiguous
            var clash = ComparedMap.Objects.FirstOrDefault(o => o.Name == restore.Name &&
                MsbDiff.ListOf(o.WrappedObject) == MsbDiff.ListOf(restore));

            if (clash != null)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Vanilla_Tool_Restore_Clash", restore.Name, ComparedMap.Name));
                return;
            }

            var action = MapVanillaCompare.BuildRestoreAction(View, ComparedMap, restore);

            if (action != null)
            {
                (ComparedMap.OwnerView ?? View).ViewportActionManager.ExecuteAction(action);

                // The vanilla entry now belongs to the map, so its vanilla copy is read again
                Compare.Invalidate(ComparedMap);
                Run(ComparedMap);
            }
        }
    }
}
