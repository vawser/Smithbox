using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class TimeActEditorTab
{
    public Smithbox BaseEditor;

    public TimeActEditorTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public enum TimeactCompressionType
    {
        [Display(Name = "Default")] Default,
        [Display(Name = "DFLT")] DFLT,
        [Display(Name = "KRAK")] KRAK,
        [Display(Name = "KRAK_MAX")] KRAK_MAX,

    }

    public void Display()
    {
        // Files
        if (ImGui.CollapsingHeader("Data", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var objTitle = TimeActUtils.GetObjectTitle().ToLower();

            ImGui.Text("Each set of Time Acts will increase the setup time of the Time Act Editor, so only enable what you need.");

            ImGui.Checkbox("Load character time acts", ref CFG.Current.TimeActEditor_Load_CharacterTimeActs);
            UIHelper.ShowHoverTooltip("Load the character time act files when setting up the Time Act Editor.");

            ImGui.Checkbox("Load character time acts for vanilla comparison", ref CFG.Current.TimeActEditor_Load_VanillaCharacterTimeActs);
            UIHelper.ShowHoverTooltip("Load the vanilla character time act files when setting up the Time Act Editor.");

            ImGui.Checkbox($"Load {objTitle} time acts", ref CFG.Current.TimeActEditor_Load_ObjectTimeActs);
            UIHelper.ShowHoverTooltip($"Load the {objTitle} time act files when setting up the Time Act Editor.");

            ImGui.Checkbox($"Load {objTitle} time acts for vanilla comparison", ref CFG.Current.TimeActEditor_Load_VanillaObjectTimeActs);
            UIHelper.ShowHoverTooltip($"Load the vanilla {objTitle} time act files when setting up the Time Act Editor.");


            var curProjectType = ProjectType.Undefined;

            if (BaseEditor.ProjectManager.SelectedProject != null)
            {
                curProjectType = BaseEditor.ProjectManager.SelectedProject.ProjectType;
            }

            if (curProjectType is ProjectType.ER or ProjectType.AC6)
            {
                if (ImGui.BeginCombo("Compression Type", CFG.Current.CurrentTimeActCompressionType.ToString()))
                {
                    foreach (var entry in Enum.GetValues(typeof(TimeactCompressionType)))
                    {
                        var type = (TimeactCompressionType)entry;

                        if (ImGui.Selectable(type.GetDisplayName()))
                        {
                            CFG.Current.CurrentTimeActCompressionType = (TimeactCompressionType)entry;
                        }
                    }
                    ImGui.EndCombo();
                }
                UIHelper.ShowHoverTooltip("Change the compression type used when saving the Time Act container (if applicable).");
            }
        }

        // Time Acts
        if (ImGui.CollapsingHeader("Time Acts", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display time act aliases", ref CFG.Current.TimeActEditor_DisplayTimeActRow_AliasInfo);
            UIHelper.ShowHoverTooltip("Display aliases for each of the Time Act rows");
        }

        // Animations
        if (ImGui.CollapsingHeader("Animations", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display animation file name", ref CFG.Current.TimeActEditor_DisplayAnimFileName);
            UIHelper.ShowHoverTooltip("Display the stored filename for each animation.");

            ImGui.Checkbox("Display animation aliases", ref CFG.Current.TimeActEditor_DisplayAnimRow_GeneratorInfo);
            UIHelper.ShowHoverTooltip("Display the generator info aliases for each of the Animation rows");

            ImGui.Checkbox("Display every valid generator in animation alias", ref CFG.Current.TimeActEditor_DisplayAllGenerators);
            UIHelper.ShowHoverTooltip("By default only the first generator is the list is displayed, this will display them all.");
        }

        // Events
        if (ImGui.CollapsingHeader("Events", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display event bank", ref CFG.Current.TimeActEditor_DisplayEventBank);
            UIHelper.ShowHoverTooltip("Display the event bank ID.");

            ImGui.Checkbox("Display event id", ref CFG.Current.TimeActEditor_DisplayEventID);
            UIHelper.ShowHoverTooltip("Display the internal event ID for each event.");

            ImGui.Checkbox("Display additional in-line info: Enums", ref CFG.Current.TimeActEditor_DisplayEventRow_EnumInfo);
            UIHelper.ShowHoverTooltip("Display additional info about the Enum properties in the Event row name.");

            ImGui.Checkbox("Display additional in-line info: Param References", ref CFG.Current.TimeActEditor_DisplayEventRow_ParamRefInfo);
            UIHelper.ShowHoverTooltip("Display additional info about the Param Reference properties in the Event row name.");

            ImGui.Checkbox("Display additional in-line info: Data Aliases", ref CFG.Current.TimeActEditor_DisplayEventRow_DataAliasInfo);
            UIHelper.ShowHoverTooltip("Display additional info about the Data Alias properties in the Event row name.");

            ImGui.Checkbox("Include Data Alias name within additional in-line info", ref CFG.Current.TimeActEditor_DisplayEventRow_DataAliasInfo_IncludeAliasName);
            UIHelper.ShowHoverTooltip("Include the alias name in the Data Alias properties in the Event row name.");

            ImGui.Checkbox("Display additional in-line info: Project Enums", ref CFG.Current.TimeActEditor_DisplayEventRow_ProjectEnumInfo);
            UIHelper.ShowHoverTooltip("Display additional info about the Project Enum properties in the Event row name.");
        }

        // Properties List
        if (ImGui.CollapsingHeader("Properties", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display property type column", ref CFG.Current.TimeActEditor_DisplayPropertyType);
            UIHelper.ShowHoverTooltip("Display the property type as an additional column in the Properties view.");
        }

        // Text Colors
        if (ImGui.CollapsingHeader("Event List - Additional Info Coloring", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.ColorEdit4("Enum Text", ref UI.Current.ImGui_TimeAct_InfoText_1_Color);
            ImGui.ColorEdit4("Param Reference Text", ref UI.Current.ImGui_TimeAct_InfoText_2_Color);
            ImGui.ColorEdit4("Alias Text", ref UI.Current.ImGui_TimeAct_InfoText_3_Color);
            ImGui.ColorEdit4("Project Enum Text", ref UI.Current.ImGui_TimeAct_InfoText_4_Color);

            if (ImGui.Button("Reset"))
            {
                UI.Current.ImGui_TimeAct_InfoText_1_Color = UI.Default.ImGui_TimeAct_InfoText_1_Color;
                UI.Current.ImGui_TimeAct_InfoText_2_Color = UI.Default.ImGui_TimeAct_InfoText_2_Color;
                UI.Current.ImGui_TimeAct_InfoText_3_Color = UI.Default.ImGui_TimeAct_InfoText_3_Color;
                UI.Current.ImGui_TimeAct_InfoText_4_Color = UI.Default.ImGui_TimeAct_InfoText_4_Color;
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
    }
}
