using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Tools;

public class ModelActionMenubar
{
    private ModelEditorScreen Screen;
    private ModelActionHandler ActionHandler;

    public ModelActionMenubar(ModelEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            if (ImGui.Button("Create", UI.MenuButtonSize))
            {
                ActionHandler.CreateHandler();
            }
            UIHelper.ShowHoverTooltip($"Adds new entry based on current selection in Model Hierarchy.\n{KeyBindings.Current.CORE_CreateNewEntry.HintText}");

            if (ImGui.Button("Duplicate", UI.MenuButtonSize))
            {
                ActionHandler.DuplicateHandler();
            }
            UIHelper.ShowHoverTooltip($"Duplicates current selection in Model Hierarchy.\n{KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}");

            if (ImGui.Button("Delete", UI.MenuButtonSize))
            {
                ActionHandler.DeleteHandler();
            }
            UIHelper.ShowHoverTooltip($"Deletes current selection in Model Hierarchy.\n{KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}");

            ImGui.EndMenu();
        }
    }

}
