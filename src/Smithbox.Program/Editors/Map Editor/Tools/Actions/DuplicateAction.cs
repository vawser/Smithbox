using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Linq;

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
        UIHelper.Tooltip($"Duplicate the currently selected map objects.\n\nShortcut: {InputManager.GetHint(KeybindID.Duplicate)}");

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
        UIHelper.Tooltip($"Duplicate the currently selected map objects.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Duplicate"))
        {
            DisplayMenu();
        }
    }

    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        var windowWidth = ImGui.GetWindowWidth();

        UIHelper.SimpleHeader("Options", "Options", "", UI.Current.ImGui_Default_Text_Color);

        if (View.Project.Descriptor.ProjectType != ProjectType.DS2S && View.Project.Descriptor.ProjectType != ProjectType.DS2)
        {
            if (ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Toolbar_Duplicate_Increment_Entity_ID))
            {
                if (CFG.Current.Toolbar_Duplicate_Increment_Entity_ID)
                {
                    CFG.Current.Toolbar_Duplicate_Clear_Entity_ID = false;
                }
            }
            UIHelper.Tooltip("When enabled, the duplicated entities will be given a new valid Entity ID.");
        }

        if (View.Project.Descriptor.ProjectType == ProjectType.ER || View.Project.Descriptor.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Toolbar_Duplicate_Increment_InstanceID);
            UIHelper.Tooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
        }

        if (View.Project.Descriptor.ProjectType == ProjectType.ER || View.Project.Descriptor.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_PartNames);
            UIHelper.Tooltip("When enabled, the duplicated Asset entities PartNames property will be updated.");
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
            UIHelper.Tooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

            ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs);
            UIHelper.Tooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
        }

        ImGui.Checkbox("Place at List End", ref CFG.Current.Toolbar_Duplicate_Place_at_List_End);
        UIHelper.Tooltip("When enabled, a duplicated map object is placed at the end of its category list, rather than after its source map object.");

        UIHelper.WrappedText("");

        if (ImGui.Button("Duplicate Selection", DPI.WholeWidthButton(windowWidth, 24)))
        {
            ApplyDuplicate();
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyDuplicate()
    {
        if (View.ViewportSelection.IsSelection())
        {
            CloneMapObjectsAction action = new(View, View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList(), true);
            View.ViewportActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }

        View.DelayPicking();
    }
}