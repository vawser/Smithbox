using Hexa.NET.ImGui;
using StudioCore.GraphicsEditor;
using StudioCore.Tools;

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
            if (ImGui.MenuItem("Color Picker"))
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
                    if (ImGui.MenuItem("Current Gparam", KeyBindings.Current.GPARAM_ReloadParam.HintText))
                    {
                        GparamMemoryTools.ReloadCurrentGparam(Screen.Selection._selectedGparamInfo);
                    }

                    ImGui.EndMenu();
                }
            }
            */

            ImGui.EndMenu();
        }
    }
}

