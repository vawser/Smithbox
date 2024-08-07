using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class TimeActEditorTab
{
    public TimeActEditorTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("Time Act Editor"))
        {
            // Setup
            if (ImGui.CollapsingHeader("Setup"))
            {
                ImGui.Checkbox("Load project character time acts", ref CFG.Current.TimeActEditor_Load_CharacterTimeActs);
                ImguiUtils.ShowHoverTooltip("Load the character time act files when setting up the Time Act Editor.");

                ImGui.Checkbox("Load vanilla character time acts", ref CFG.Current.TimeActEditor_Load_VanillaCharacterTimeActs);
                ImguiUtils.ShowHoverTooltip("Load the vanillacharacter time act files when setting up the Time Act Editor.");

                ImGui.Checkbox("Load project object time acts", ref CFG.Current.TimeActEditor_Load_ObjectTimeActs);
                ImguiUtils.ShowHoverTooltip("Load the object/asset time act files when setting up the Time Act Editor.");

                ImGui.Checkbox("Load vanilla object time acts", ref CFG.Current.TimeActEditor_Load_VanillaObjectTimeActs);
                ImguiUtils.ShowHoverTooltip("Load the vanilla object/asset time act files when setting up the Time Act Editor.");
            }

            // Time Acts
            if (ImGui.CollapsingHeader("Time Acts"))
            {
                ImGui.Checkbox("Display time act aliases", ref CFG.Current.TimeActEditor_DisplayTimeActRow_AliasInfo);
                ImguiUtils.ShowHoverTooltip("Display aliases for each of the Time Act rows");
            }

            // Animations
            if (ImGui.CollapsingHeader("Animations"))
            {
                ImGui.Checkbox("Display animation aliases", ref CFG.Current.TimeActEditor_DisplayAnimRow_GeneratorInfo);
                ImguiUtils.ShowHoverTooltip("Display the generator info aliases for each of the Animation rows");

                ImGui.Checkbox("Display every valid generator in animation alias", ref CFG.Current.TimeActEditor_DisplayAllGenerators);
                ImguiUtils.ShowHoverTooltip("By default only the first generator is the list is displayed, this will display them all.");
            }

            // Events
            if (ImGui.CollapsingHeader("Events"))
            {
                ImGui.Checkbox("Display additional in-line info: Enums", ref CFG.Current.TimeActEditor_DisplayEventRow_EnumInfo);
                ImguiUtils.ShowHoverTooltip("Display additional info about the Enum properties in the Event row name.");

                ImGui.Checkbox("Display additional in-line info: Param References", ref CFG.Current.TimeActEditor_DisplayEventRow_ParamRefInfo);
                ImguiUtils.ShowHoverTooltip("Display additional info about the Param Reference properties in the Event row name.");

                ImGui.Checkbox("Display additional in-line info: Data Aliases", ref CFG.Current.TimeActEditor_DisplayEventRow_DataAliasInfo);
                ImguiUtils.ShowHoverTooltip("Display additional info about the Data Alias properties in the Event row name.");

                ImGui.Checkbox("Include Data Alias name within additional in-line info", ref CFG.Current.TimeActEditor_DisplayEventRow_DataAliasInfo_IncludeAliasName);
                ImguiUtils.ShowHoverTooltip("Include the alias name in the Data Alias properties in the Event row name.");

                ImGui.Checkbox("Display additional in-line info: Project Enums", ref CFG.Current.TimeActEditor_DisplayEventRow_ProjectEnumInfo);
                ImguiUtils.ShowHoverTooltip("Display additional info about the Project Enum properties in the Event row name.");
            }

            // Properties List
            if (ImGui.CollapsingHeader("Properties", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display property type column", ref CFG.Current.TimeActEditor_DisplayPropertyType);
                ImguiUtils.ShowHoverTooltip("Display the property type as an additional column in the Properties view.");
            }

            // Text Colors
            if (ImGui.CollapsingHeader("Event List - Additional Info Coloring", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Enum Text", ref CFG.Current.ImGui_TimeAct_InfoText_1_Color);
                ImGui.ColorEdit4("Param Reference Text", ref CFG.Current.ImGui_TimeAct_InfoText_2_Color);
                ImGui.ColorEdit4("Alias Text", ref CFG.Current.ImGui_TimeAct_InfoText_3_Color);
                ImGui.ColorEdit4("Project Enum Text", ref CFG.Current.ImGui_TimeAct_InfoText_4_Color);

                if(ImGui.Button("Reset"))
                {
                    CFG.Current.ImGui_TimeAct_InfoText_1_Color = CFG.Default.ImGui_TimeAct_InfoText_1_Color;
                    CFG.Current.ImGui_TimeAct_InfoText_2_Color = CFG.Default.ImGui_TimeAct_InfoText_2_Color;
                    CFG.Current.ImGui_TimeAct_InfoText_3_Color = CFG.Default.ImGui_TimeAct_InfoText_3_Color;
                    CFG.Current.ImGui_TimeAct_InfoText_4_Color = CFG.Default.ImGui_TimeAct_InfoText_4_Color;
                }
            }

            // Grid
            /*
            if (ImGui.CollapsingHeader("Viewport Grid"))
            {
                ImGui.SliderInt("Grid size", ref CFG.Current.TimeActEditor_Viewport_Grid_Size, 100, 1000);
                ImguiUtils.ShowHoverTooltip("The overall maximum size of the grid.\nThe grid will only update upon restarting Smithbox after changing this value.");

                ImGui.SliderInt("Grid increment", ref CFG.Current.TimeActEditor_Viewport_Grid_Square_Size, 1, 100);
                ImguiUtils.ShowHoverTooltip("The increment size of the grid.");

                var height = CFG.Current.TimeActEditor_Viewport_Grid_Height;

                ImGui.InputFloat("Grid height", ref height);
                ImguiUtils.ShowHoverTooltip("The height at which the horizontal grid sits.");

                if (height < -10000)
                    height = -10000;

                if (height > 10000)
                    height = 10000;

                CFG.Current.TimeActEditor_Viewport_Grid_Height = height;

                ImGui.ColorEdit3("Grid color", ref CFG.Current.TimeActEditor_Viewport_Grid_Color);

                if (ImGui.Button("Regenerate"))
                {
                    CFG.Current.TimeActEditor_Viewport_RegenerateMapGrid = true;
                }
                ImGui.SameLine();
                if (ImGui.Button("Reset"))
                {
                    CFG.Current.TimeActEditor_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                    CFG.Current.TimeActEditor_Viewport_Grid_Size = 1000;
                    CFG.Current.TimeActEditor_Viewport_Grid_Square_Size = 10;
                    CFG.Current.TimeActEditor_Viewport_Grid_Height = 0;
                }
                ImguiUtils.ShowHoverTooltip("Resets all of the values within this section to their default values.");
            }
            */

            ImGui.EndTabItem();
        }
    }
}
