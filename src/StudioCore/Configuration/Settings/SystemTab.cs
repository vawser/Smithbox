using ImGuiNET;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class SystemTab
{
    public SystemTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Check for new versions of Smithbox during startup",
                ref CFG.Current.System_Check_Program_Update);
            UIHelper.ShowHoverTooltip("When enabled Smithbox will automatically check for new versions upon program start.");

            ImGui.Checkbox("Enable Soapstone Server",
                ref CFG.Current.Enable_Soapstone_Server);
            UIHelper.ShowHoverTooltip("Enables the Soapstone Server, allow for cross-data integration with DarkScript.");

            ImGui.Checkbox("Enable debugging tools", ref CFG.Current.DisplayDebugTools);
            UIHelper.ShowHoverTooltip("If enabled, various debugging tools will be available.");

            ImGui.Separator();

            UIHelper.WrappedText("By default, files are read by Smithbox in a strict manner. Data that is present in locations that it should not be will throw an exception.");

            UIHelper.WrappedText("This option will remove that strictness, and will cause Smithbox to ignore the invalid data when reading a file.");

            ImGui.Checkbox("Ignore asserts", ref CFG.Current.System_IgnoreAsserts);
            UIHelper.ShowHoverTooltip("If enabled, when attempting to read files, asserts will be ignored.");

            Smithbox.UpdateSoulsFormatsToggles();
        }

        if (ImGui.CollapsingHeader("Loggers"))
        {
            ImGui.Checkbox("Show Action Logger", ref UI.Current.System_ShowActionLogger);
            UIHelper.ShowHoverTooltip("If enabled, the action logger will be visible in the menu bar.");

            ImGui.InputInt("Action Log Visibility Duration", ref CFG.Current.System_ActionLogger_FadeTime);
            UIHelper.ShowHoverTooltip("The number of frames for which the action logger message stays visible in the menu bar.\n-1 means the message never disappears.");

            ImGui.Separator();

            ImGui.Checkbox("Show Warning Logger", ref UI.Current.System_ShowWarningLogger);
            UIHelper.ShowHoverTooltip("If enabled, the warning logger will be visible in the menu bar.");

            ImGui.InputInt("Warning Log Visibility Duration", ref CFG.Current.System_WarningLogger_FadeTime);
            UIHelper.ShowHoverTooltip("The number of frames for which the warning logger message stays visible in the menu bar.\n-1 means the message never disappears.");

        }

        if (ImGui.CollapsingHeader("Editors"))
        {
            UIHelper.WrappedText("Determine which editors are enabled." +
                "\nIf an editor was disabled at start, it will only appear once Smithbox is restarted if enabled.");
            ImGui.Checkbox("Enable Map Editor", ref CFG.Current.EnableEditor_MSB);
            UIHelper.ShowHoverTooltip("Enables the Map Editor in Smithbox.");

            ImGui.Checkbox("Enable Model Editor", ref CFG.Current.EnableEditor_FLVER);
            UIHelper.ShowHoverTooltip("Enables the Model Editor in Smithbox.");

            ImGui.Checkbox("Enable Param Editor", ref CFG.Current.EnableEditor_PARAM);
            UIHelper.ShowHoverTooltip("Enables the Param Editor in Smithbox.");

            ImGui.Checkbox("Enable Text Editor", ref CFG.Current.EnableEditor_FMG);
            UIHelper.ShowHoverTooltip("Enables the Text Editor in Smithbox.");

            ImGui.Checkbox("Enable Gparam Editor", ref CFG.Current.EnableEditor_GPARAM);
            UIHelper.ShowHoverTooltip("Enables the Gparam Editor in Smithbox.");

            ImGui.Checkbox("Enable Texture Viewer", ref CFG.Current.EnableViewer_TEXTURE);
            UIHelper.ShowHoverTooltip("Enables the Texture Viewer in Smithbox.");

            ImGui.Checkbox("Enable Time Act Editor", ref CFG.Current.EnableEditor_TAE);
            UIHelper.ShowHoverTooltip("Enables the Time Act Editor in Smithbox.");

            ImGui.Checkbox("Enable EMEVD Editor", ref CFG.Current.EnableEditor_EMEVD);
            UIHelper.ShowHoverTooltip("Enables the EMEVD Editor in Smithbox." +
                "\nWARNING: this editor is a work-in-progress, so is only suited for read-only uses currently.");

            ImGui.Checkbox("Enable ESD Editor", ref CFG.Current.EnableEditor_ESD);
            UIHelper.ShowHoverTooltip("Enables the ESD Editor in Smithbox." +
                "\nWARNING: this editor is a work-in-progress, so is only suited for read-only uses currently.");

            ImGui.Checkbox("Enable Cutscene Editor", ref CFG.Current.EnableEditor_MQB_wip);
            UIHelper.ShowHoverTooltip("Enables the Cutscene Editor in Smithbox." +
                "\nWARNING: this editor is a work-in-progress, so is only suited for read-only uses currently.");

            // WIP
            /*
            ImGui.Checkbox("Enable Havok Editor", ref CFG.Current.EnableHavokEditor);
            ImGui.Checkbox("Enable Material Editor", ref CFG.Current.EnableMaterialEditor);
            ImGui.Checkbox("Enable Particle Editor", ref CFG.Current.EnableParticleEditor);
            */
        }

        if (ImGui.CollapsingHeader("Meta Tools"))
        {
            ImGui.Checkbox("Change Base Aliases", ref CFG.Current.AliasBank_EditorMode);
            UIHelper.ShowHoverTooltip("If enabled, editing the name and tags for alias banks will commit the changes to the Smithbox base version instead of the mod-specific version.");

            ImGui.Checkbox("Enable DokuWiki Tools",
                ref CFG.Current.EnableWikiTools);
            UIHelper.ShowHoverTooltip("Enables various functionality changes for DokuWiki outputs.");
        }

        if (ImGui.CollapsingHeader("Configuration"))
        {
            if (ImGui.Button("Reset Configuration"))
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                $"Do you want to delete your Smithbox configuration files?",
                $"Warning", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    var configFolder = CFG.GetConfigFilePath();
                    var keybindsFolder = CFG.GetBindingsFilePath();
                    var interfaceFolder = UI.GetConfigFilePath();

                    if (File.Exists(configFolder))
                    {
                        File.Delete(configFolder);
                    }
                    if (File.Exists(keybindsFolder))
                    {
                        File.Delete(keybindsFolder);
                    }
                    if (File.Exists(interfaceFolder))
                    {
                        File.Delete(interfaceFolder);
                    }

                    CFG.Save();
                    UI.Save();
                }
            }
            UIHelper.ShowHoverTooltip("This will delete your Smithbox folder and the configuration files within.");
        }
    }
}
