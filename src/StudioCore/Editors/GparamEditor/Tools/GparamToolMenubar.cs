using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.GparamEditor.Utils;
using StudioCore.Editors.ParamEditor;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Tools;

public class GparamToolMenubar
{
    private GparamEditorScreen Screen;
    public GparamActionHandler Handler;

    public GparamToolMenubar(GparamEditorScreen screen)
    {
        Screen = screen;
        Handler = new GparamActionHandler(screen);
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            // Color Picker
            if (ImGui.Button("Color Picker", UI.MenuButtonSize))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            // Gparam Reloader
            /*
            
            ImGui.Separator();

            if (GparamMemoryTools.IsGparamReloaderSupported())
            {
                UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
                if (ImGui.BeginMenu("Gparam Reloader"))
                {
                    if (ImGui.Button("Current Gparam", UI.MenuButtonSize))
                    {
                        GparamMemoryTools.ReloadCurrentGparam(Screen.Selection._selectedGparamInfo);
                    }
                    UIHelper.ShowHoverTooltip($"{KeyBindings.Current.GPARAM_ReloadParam.HintText}");

                    ImGui.EndMenu();
                }
            }
            */

            ImGui.EndMenu();
        }
    }
}

