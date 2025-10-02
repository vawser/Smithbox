using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class DuplicateAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public DuplicateAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry) && Editor.ViewportSelection.IsSelection())
        {
            ApplyDuplicate();
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
        UIHelper.Tooltip($"Duplicate the currently selected map objects.\n\nShortcut: {KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}");

    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Duplicate", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
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

        if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
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

        if (Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Toolbar_Duplicate_Increment_InstanceID);
            UIHelper.Tooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
        }

        if (Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Toolbar_Duplicate_Increment_PartNames);
            UIHelper.Tooltip("When enabled, the duplicated Asset entities PartNames property will be updated.");
        }

        if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
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
        if (Editor.ViewportSelection.IsSelection())
        {
            CloneMapObjectsAction action = new(Editor, Editor.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList(), true);
            Editor.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}