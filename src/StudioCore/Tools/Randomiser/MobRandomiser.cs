using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools.Randomiser;

public class MobRandomiser
{

    public void Display()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (SupportsRandomiser())
        {
            ImguiUtils.WrappedText("This randomiser allows you to randomise the type of enemies throughout the world.");
            ImguiUtils.WrappedText("");

            DisplayMobConfiguration();

            if (ImGui.Button("Randomise", buttonSize))
            {
                RandomiseMobs();
            }
        }
        else
        {
            ImguiUtils.WrappedText($"This randomiser is not available.");
        }
    }
    public bool SupportsRandomiser()
    {
        return false;
    }

    private void DisplayMobConfiguration()
    {

    }

    private void RandomiseMobs()
    {

    }
}
