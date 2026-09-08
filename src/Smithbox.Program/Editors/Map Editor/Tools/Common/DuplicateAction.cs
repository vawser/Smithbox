using Hexa.NET.ImGui;
using SoulsFormats.KF4;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class DuplicateAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public DuplicateAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                ApplyDuplicate();
            }
        }
    }


    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.Selectable("Duplicate"))
        {
            ApplyDuplicate();
        }
        GUI.Tooltip($"Duplicate the currently selected map objects.\n\nShortcut: {InputManager.GetHint(KeybindID.Duplicate)}");

    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Duplicate", InputManager.GetHint(KeybindID.Duplicate)))
        {
            ApplyDuplicate();
        }
        GUI.Tooltip($"Duplicate the currently selected map objects.");
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
        GUI.WrappedText("Configure how the duplicate action works.");

        GUI.Spacer();
        GUI.SimpleHeader("Options", "");

        if (View.Project.Descriptor.ProjectType != ProjectType.DS2S && View.Project.Descriptor.ProjectType != ProjectType.DS2)
        {
            if (ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Toolbar_Duplicate_Increment_Entity_ID))
            {
                if (CFG.Current.Toolbar_Duplicate_Increment_Entity_ID)
                {
                    CFG.Current.Toolbar_Duplicate_Clear_Entity_ID = false;
                }
            }
            GUI.Tooltip("When enabled, the duplicated entities will be given a new valid Entity ID.");
        }

        if (View.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Toolbar_Duplicate_Increment_InstanceID);
            GUI.Tooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");

            ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_PartNames);
            GUI.Tooltip("When enabled, the duplicated Asset entities PartNames property will be updated.");
        }

        if (View.Project.Descriptor.ProjectType != ProjectType.DS2S && View.Project.Descriptor.ProjectType != ProjectType.DS2)
        {
            if (ImGui.Checkbox("Clear Entity ID", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_ID))
            {
                if (CFG.Current.Toolbar_Duplicate_Clear_Entity_ID)
                {
                    CFG.Current.Toolbar_Duplicate_Increment_Entity_ID = false;
                }
            }
            GUI.Tooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

            ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs);
            GUI.Tooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
        }

        ImGui.Checkbox("Place at List End", ref CFG.Current.Toolbar_Duplicate_Place_at_List_End);
        GUI.Tooltip("When enabled, a duplicated map object is placed at the end of its category list, rather than after its source map object.");

        ImGui.Checkbox("Randomise Rotation", ref CFG.Current.Toolbar_Duplicate_Randomise_Rotation);
        GUI.Tooltip("When enabled, a duplicated map object's rotation is randomised.");

        if(CFG.Current.Toolbar_Duplicate_Randomise_Rotation)
        {
            var preview = $"{CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Dir.ToString()}";

            GUI.Spacer();
            GUI.SimpleHeader("Rotation Parameters", "");

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("Axis##randomiseDirAxis", preview))
            {
                foreach(var val in Enum.GetValues(typeof(RandomiseRotationDirection)))
                {
                    var curType = (RandomiseRotationDirection)val;
                    var selected = CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Dir == curType;

                    if (ImGui.Selectable($"{curType.ToString()}", selected))
                    {
                        CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Dir = curType;
                    }
                }

                ImGui.EndCombo();
            }

            GUI.SetInputWidth();
            ImGui.DragFloat("Rotation Range##dupeRandomizeRange", ref CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Range);
            if(CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Range < 0)
            {
                CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Range = 0;
            }
        }


        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("duplicateActions",
            "applyDuplicate", "Duplicate Selection", "", ApplyDuplicate);
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyDuplicate()
    {
        if (View.ViewportSelection.IsSelection())
        {
            var mapObjects = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();
            var mapContainer = View.Selection.SelectedMapContainer;
            var btlParent = mapContainer.BTLParents.FirstOrDefault();

            EntDuplicateAction action = new(
                view: View, 
                objects: mapObjects, 
                targetMap: mapContainer, 
                targetBTL: btlParent,
                duplicateToMap: false,
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
        }
        else
        {
            Smithbox.LogError<DuplicateAction>("No object selected.");
        }

        View.DelayPicking();
    }

    public enum RandomiseRotationDirection
    {
        X,
        Y,
        Z
    }
}