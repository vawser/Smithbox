using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class DuplicateToMapAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public bool DisplayPopup = false;

    public (string, ObjectContainer) TargetMap = ("None", null);
    public (string, Entity) TargetBTL = ("None", null);

    public DuplicateToMapAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Update Loop
    /// </summary>
    public void OnGui()
    {
        if (DisplayPopup)
        {
            ImGui.OpenPopup("##DupeToTargetMapPopup");
            DisplayPopup = false;
        }

        if (ImGui.BeginPopup("##DupeToTargetMapPopup"))
        {
            DisplayMenu();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Duplicate_To_Map))
            {
                ImGui.OpenPopup("##DupeToTargetMapPopup");
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.Selectable("Duplicate to Map"))
        {
            DisplayPopup = true;
        }
        UIHelper.Tooltip($"Duplicate the selected map objects into another map.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Duplicate_To_Map)}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.BeginMenu("Duplicate Selected to Map"))
        {
            DisplayMenu();

            ImGui.EndMenu();
        }
        UIHelper.Tooltip($"Duplicate the selected map objects into another map.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        ImGui.BeginChild("DuplicateToMapToolSection", ImGuiChildFlags.Borders);
        DisplayMenu();
        ImGui.EndChild();
    }


    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        UIHelper.WrappedText("Use this to duplicate an existing map object into a different loaded map.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Target Map", "The target map to duplicate the current selection to.");

        UIHelper.SetInputWidth();
        if (ImGui.BeginCombo("##targetMapSelect", TargetMap.Item1))
        {
            foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
            {
                var map = entry.Value.MapContainer;

                if (map == null)
                    continue;

                var mapID = entry.Key.Filename;
                var mapName = AliasHelper.GetMapNameAlias(View.Project, mapID);
                var displayName = $"{mapID}: {mapName}";

                if (ImGui.Selectable(displayName, TargetMap.Item1 == mapID))
                {
                    TargetMap = (mapID, map);
                }
            }

            ImGui.EndCombo();
        }

        if (TargetMap != (null, null))
        {
            MapContainer targetMap = (MapContainer)TargetMap.Item2;
            var sel = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();

            if (sel.Any(e => e.WrappedObject is BTL.Light))
            {
                UIHelper.SimpleHeader("Target Lights", "The target BTL to duplicate the current selection to.");

                if (ImGui.BeginCombo("##TargetBTL", TargetBTL.Item1))
                {
                    foreach (Entity btl in targetMap.BTLParents)
                    {
                        var adName = (string)btl.WrappedObject;
                        if (ImGui.Selectable(adName))
                        {
                            TargetBTL = (adName, btl);
                            break;
                        }
                    }
                    ImGui.EndCombo();
                }

                if (TargetBTL.Item2 == null)
                {
                    UIHelper.WrappedText("No BTL has been targeted.");
                }
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            UIHelper.MultiButtonInput("extDuplicateActions",
                "duplicateMap", "Duplicate to Map", "", DuplicateToMapActions);
        }
    }

    public void DuplicateToMapActions()
    {
        MapContainer targetMap = (MapContainer)TargetMap.Item2;
        var sel = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();

        DuplicateToMap(sel, targetMap, TargetBTL.Item2);
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void DuplicateToMap(List<MsbEntity> selection, MapContainer targetMap, Entity targetBtl)
    {
        var action = new CloneMapObjectsAction(View, selection, true, targetMap, targetBtl);

        View.ViewportActionManager.ExecuteAction(action);

        View.DelayPicking();
    }
}