using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Interface;
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
            UIHelper.WrappedText("This randomiser allows you to randomise the placement of treasures throughout the world.");
            UIHelper.WrappedText("");

            switch (Smithbox.ProjectType)
            {
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    DisplayConfiguration_DS1();
                    break;
                case ProjectType.DS2:
                case ProjectType.DS2S:
                    DisplayConfiguration_DS2();
                    break;
                case ProjectType.DS3:
                    DisplayConfiguration_DS3();
                    break;
                case ProjectType.BB:
                    DisplayConfiguration_BB();
                    break;
                case ProjectType.SDT:
                    DisplayConfiguration_SDT();
                    break;
                case ProjectType.ER:
                    DisplayConfiguration_ER();
                    break;
                case ProjectType.AC6:
                    DisplayConfiguration_AC6();
                    break;
            }

            if(ImGui.Button("Randomise", buttonSize))
            {
                switch(Smithbox.ProjectType)
                {
                    case ProjectType.DS1:
                    case ProjectType.DS1R:
                        Randomise_DS1();
                        break;
                    case ProjectType.DS2:
                    case ProjectType.DS2S:
                        Randomise_DS2();
                        break;
                    case ProjectType.DS3:
                        Randomise_DS3();
                        break;
                    case ProjectType.BB:
                        Randomise_BB();
                        break;
                    case ProjectType.SDT:
                        Randomise_SDT();
                        break;
                    case ProjectType.ER:
                        Randomise_ER();
                        break;
                    case ProjectType.AC6:
                        Randomise_AC6();
                        break;
                }
            }
        }
        else
        {
            UIHelper.WrappedText($"This randomiser is not available.");
        }
    }

    private bool SupportsRandomiser()
    {
        if(Smithbox.ProjectType is 
            ProjectType.DS1 or ProjectType.DS1R or 
            ProjectType.DS2 or ProjectType.DS2S or 
            ProjectType.DS3 or 
            ProjectType.BB or 
            ProjectType.SDT or 
            ProjectType.ER or
            ProjectType.AC6)
        {
            return true;
        }

        return false;
    }

    // DS1
    #region DS1 - Randomiser
    private void DisplayConfiguration_DS1()
    {

    }

    private void Randomise_DS1()
    {
    }
    #endregion

    // DS2
    #region DS2 - Randomiser
    private void DisplayConfiguration_DS2()
    {

    }

    private void Randomise_DS2()
    {
    }
    #endregion

    // BB
    #region BB - Randomiser
    private void DisplayConfiguration_BB()
    {

    }

    private void Randomise_BB()
    {
    }
    #endregion

    // DS3
    #region DS3 - Randomiser
    private void DisplayConfiguration_DS3()
    {

    }

    private void Randomise_DS3()
    {
    }
    #endregion

    // SDT
    #region SDT - Randomiser
    private void DisplayConfiguration_SDT()
    {

    }

    private void Randomise_SDT()
    {
    }
    #endregion

    // ER
    #region ER - Randomiser
    private void DisplayConfiguration_ER()
    {
        // World treasures
        if(ImGui.CollapsingHeader("World"))
        {
            ImGui.Checkbox("Include World Treasures", ref RandomiserCFG.Current.ER_IncludeWorldTreasures);
            UIHelper.ShowHoverTooltip("Include treasures considered 'world' treasures, for example those found in the open-world, in legacy dungeons and in catacombs.");

            ImGui.InputText("World Treasures", ref RandomiserCFG.Current.ER_WorldTreasures, 255);
            UIHelper.ShowHoverTooltip("Refers to ItemLotParam row IDs.\n\nRanges are defined within [ ] braces: first value is start of range, second value is end of range. Single values in [ ] braces are treated as a single ID.\n\nEach range is split by the : character.");

            ImGui.Separator();

            ImGui.Checkbox("Include World Treasures (DLC)", ref RandomiserCFG.Current.ER_IncludeWorldTreasures_DLC);
            UIHelper.ShowHoverTooltip("Include treasures considered 'world' treasures in the DLC, for example those found in the open-world, in legacy dungeons and in catacombs.");

            ImGui.InputText("World Treasures (DLC)", ref RandomiserCFG.Current.ER_WorldTreasures_DLC, 255);
            UIHelper.ShowHoverTooltip("Refers to ItemLotParam row IDs.\n\nRanges are defined within [ ] braces: first value is start of range, second value is end of range. Single values in [ ] braces are treated as a single ID.\n\nEach range is split by the : character.");
        }

        // Items
        if (ImGui.CollapsingHeader("Items"))
        {
            ImGui.Checkbox("Ignore Key Items", ref RandomiserCFG.Current.ER_IgnoreKeyItems);
            UIHelper.ShowHoverTooltip("Ignore key items (such as Keys) when randomising treasures.");

            ImGui.InputText("Key Items", ref RandomiserCFG.Current.ER_KeyItems, 255);
            UIHelper.ShowHoverTooltip("Refers to EquipParamGoods row IDs.\n\nEach entry is split by the , character.");

            ImGui.Checkbox("Ignore Progression Items", ref RandomiserCFG.Current.ER_IgnoreProgressionItems);
            UIHelper.ShowHoverTooltip("Ignore progression items (suuch as Bell Bearings, Golden Seeds, etc) when randomising treasures.");

            ImGui.InputText("Progression Items", ref RandomiserCFG.Current.ER_ProgressionItems, 255);
            UIHelper.ShowHoverTooltip("Refers to EquipParamGoods row IDs.\n\nEach entry is split by the , character.");
        }
    }

    private void Randomise_ER()
    {
        var rowList = RandomiserUtils.GetRowList(RandomiserCFG.Current.ER_WorldTreasures);
        foreach(var entry in rowList)
        {
            TaskLogs.AddLog($"{entry}");
        }
    }
    #endregion

    // AC6
    #region AC6 - Randomiser
    private void DisplayConfiguration_AC6()
    {

    }

    private void Randomise_AC6()
    {
    }
    #endregion
}
