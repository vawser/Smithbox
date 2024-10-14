using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Tools;

public class GparamActionMenubar
{
    private GparamEditorScreen Screen;
    public GparamActionHandler Handler;

    public GparamActionMenubar(GparamEditorScreen screen)
    {
        Screen = screen;
        Handler = new GparamActionHandler(screen);
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Actions"))
        {
            if (ImGui.Button("Duplicate Value Row", UI.MenuButtonSize))
            {
                Screen.ActionHandler.DuplicateValueRow();
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}");

            if (ImGui.Button("Delete Value Row", UI.MenuButtonSize))
            {
                Screen.ActionHandler.DeleteValueRow();
            }
            UIHelper.ShowHoverTooltip($"{KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}");

            ImGui.EndMenu();
        }
    }
}
