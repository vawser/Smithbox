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

public class TreasureRandomiser
{
    public void Display()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (SupportsRandomiser())
        {
            ImguiUtils.WrappedText("This randomiser allows you to randomise the placement of treasures throughout the world.");
            ImguiUtils.WrappedText("");

            DisplayTreasureConfiguration();

            if(ImGui.Button("Randomise", buttonSize))
            {
                RandomiseTreasure();
            }
        }
        else
        {
            ImguiUtils.WrappedText($"This randomiser is not available.");
        }
    }

    private bool SupportsRandomiser()
    {
        if(Smithbox.ProjectType is ProjectType.ER)
        {
            return true;
        }

        return false;
    }

    private void DisplayTreasureConfiguration()
    {

    }

    private void RandomiseTreasure()
    {
    }
}
