using Hexa.NET.ImGui;
using SoulsFormats;
using SoulsFormats.KF4;
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
                // Opened by OnGui, so the popup is opened and drawn under the same ImGui ID
                DisplayPopup = true;
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
        GUI.Tooltip($"Duplicate the selected map objects into another map.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Duplicate_To_Map)}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.BeginMenu("Duplicate to Map"))
        {
            DisplayMenu();

            ImGui.EndMenu();
        }
        GUI.Tooltip($"Duplicate the selected map objects into another map.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Duplicate_To_Map)}");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        DisplayMenu();
    }


    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        GUI.WrappedText("Use this to duplicate an existing map object into a different loaded map.");

        GUI.Spacer();
        GUI.SimpleHeader("Target Map", "The target map to duplicate the current selection to.");

        MapTargetPicker.Display(View, "targetMapSelect", ref TargetMap);

        var targetMap = View.Selection.ResolveTargetMap(TargetMap);

        if (targetMap == null)
        {
            GUI.WrappedText("No loaded map has been targeted.");
        }
        else
        {
            var sel = View.ViewportSelection.GetPlacedEntities();

            if (sel.Any(e => e.WrappedObject is BTL.Light))
            {
                GUI.SimpleHeader("Target Lights", "The target BTL to duplicate the current selection to.");

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
                    GUI.WrappedText("No BTL has been targeted.");
                }
            }

            GUI.Spacer();
            GUI.SimpleHeader("Actions", "");

            GUI.MultiButtonInput("extDuplicateActions",
                "duplicateMap", "Duplicate to Map", "", DuplicateToMapActions);
        }
    }

    public void DuplicateToMapActions()
    {
        var targetMap = View.Selection.ResolveTargetMap(TargetMap);

        // Without a target the action would quietly duplicate in place
        if (targetMap == null)
        {
            Smithbox.LogError<DuplicateToMapAction>("No loaded map has been targeted.");
            return;
        }

        var sel = View.ViewportSelection.GetPlacedEntities();

        // A BTL picked for an earlier target map belongs to that map
        var targetBtl = TargetBTL.Item2 != null && targetMap.BTLParents.Contains(TargetBTL.Item2)
            ? TargetBTL.Item2
            : null;

        DuplicateToMap(sel, targetMap, targetBtl);
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void DuplicateToMap(List<MsbEntity> mapObjects, MapContainer mapContainer, Entity btlParent)
    {
        var action = new EntDuplicateAction(
                view: View,
                objects: mapObjects,
                targetMap: mapContainer,
                targetBTL: btlParent,
                duplicateToMap: true,
                createMapGroupEntry: false,
                incrementEntityID: CFG.Current.Toolbar_Duplicate_Increment_Entity_ID,
                incrementInstanceID: CFG.Current.Toolbar_Duplicate_Increment_InstanceID,
                incrementPartNames: CFG.Current.Toolbar_Duplicate_Increment_PartNames,
                clearEntityID: CFG.Current.Toolbar_Duplicate_Clear_Entity_ID,
                clearEntityGroupID: CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs,
                randomiseRotation: CFG.Current.Toolbar_Duplicate_Randomise_Rotation,
                randomRotationDir: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Dir,
                randomRotationRange: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Range);

        View.ViewportActionManager.ExecuteAction(action);

        View.DelayPicking();
    }

    private List<MsbEntity> StoredSelection = new();

    public bool HasCopiedObjects => StoredSelection.Count > 0;

    public void OnCopy(MapEditorView activeView)
    {
        StoredSelection = new();

        var sel = activeView.ViewportSelection.GetPlacedEntities();
        foreach(var entry in sel)
        {
            var clone = entry.Clone();
            StoredSelection.Add((MsbEntity)clone);
        }
    }

    public void OnPaste(MapEditorView activeView)
    {
        var mapContainer = activeView.Selection.SelectedMapContainer;

        if (mapContainer == null)
            return;

        var btlParent = mapContainer.BTLParents.FirstOrDefault();

        var action = new EntDuplicateAction(
                view: View,
                objects: StoredSelection,
                targetMap: mapContainer,
                targetBTL: btlParent,
                duplicateToMap: true,
                createMapGroupEntry: false,
                incrementEntityID: CFG.Current.Toolbar_Duplicate_Increment_Entity_ID,
                incrementInstanceID: CFG.Current.Toolbar_Duplicate_Increment_InstanceID,
                incrementPartNames: CFG.Current.Toolbar_Duplicate_Increment_PartNames,
                clearEntityID: CFG.Current.Toolbar_Duplicate_Clear_Entity_ID,
                clearEntityGroupID: CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs,
                randomiseRotation: CFG.Current.Toolbar_Duplicate_Randomise_Rotation,
                randomRotationDir: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Dir,
                randomRotationRange: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Range);

        ViewportAction paste = action;

        // Moved in the same undo step, once the clones exist; the orb's position is taken now, at the paste
        if (CFG.Current.MapEditor_Paste_At_Placement_Orb && View.ViewportHandler.ActiveViewport?.Viewport != null)
        {
            var target = View.ViewportWindow.GetPlacementPosition();

            paste = new ViewportCompoundAction(new List<ViewportAction>
            {
                action,
                new DeferredAction(() => PullToCameraAction.BuildMoveToPointAction(action.CreatedClones, target))
            });
        }

        View.ViewportActionManager.ExecuteAction(paste);
        View.DelayPicking();
    }
}