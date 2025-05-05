using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Interface;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public class ToolSubMenu
{
    private ParamEditorScreen Editor;
    public ActionHandler Handler;

    public ToolSubMenu(ParamEditorScreen screen)
    {
        Editor = screen;
        Handler = new ActionHandler(screen);
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateParamPinGroup))
        {
            Editor.ToolWindow.PinGroupHandler.SetAutoGroupName("Param");
            Editor.ToolWindow.PinGroupHandler.CreateParamGroup();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateRowPinGroup))
        {
            Editor.ToolWindow.PinGroupHandler.SetAutoGroupName("Row");
            Editor.ToolWindow.PinGroupHandler.CreateRowGroup();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CreateFieldPinGroup))
        {
            Editor.ToolWindow.PinGroupHandler.SetAutoGroupName("Field");
            Editor.ToolWindow.PinGroupHandler.CreateFieldGroup();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedParams))
        {
            Editor.Project.PinnedParams = new();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedRows))
        {
            Editor.Project.PinnedRows = new();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ClearCurrentPinnedFields))
        {
            Editor.Project.PinnedFields = new();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_OnlyShowPinnedParams))
        {
            CFG.Current.Param_PinGroups_ShowOnlyPinnedParams = !CFG.Current.Param_PinGroups_ShowOnlyPinnedParams;
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_OnlyShowPinnedRows))
        {
            CFG.Current.Param_PinGroups_ShowOnlyPinnedRows = !CFG.Current.Param_PinGroups_ShowOnlyPinnedRows;
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_OnlyShowPinnedFields))
        {
            CFG.Current.Param_PinGroups_ShowOnlyPinnedFields = !CFG.Current.Param_PinGroups_ShowOnlyPinnedFields;
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ApplyRowNamer))
        {
            Editor.RowNamer.ApplyRowNamer();
        }
    }

    public void OnProjectChanged()
    {

    }
    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            if (ImGui.MenuItem("Trim Row Names"))
            {
                if (Editor._activeView._selection.ActiveParamExists())
                {
                    Handler.RowNameTrimHandler();
                }
            }
            UIHelper.Tooltip("This will trim the whitespace from the front and end of row names.");

            if (ImGui.MenuItem("Sort Rows"))
            {
                if (Editor._activeView._selection.ActiveParamExists())
                {
                    Handler.SortRowsHandler();
                }
            }
            UIHelper.Tooltip("This will sort the rows by ID. WARNING: this is not recommended as row index can be important.");


            ImGui.EndMenu();
        }
    }
}
