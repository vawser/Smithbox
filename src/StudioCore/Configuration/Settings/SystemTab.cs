using ImGuiNET;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Platform;
using StudioCore.Settings;
using StudioCore.Utilities;
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
            ImguiUtils.ShowHoverTooltip("When enabled Smithbox will automatically check for new versions upon program start.");

            ImGui.SliderFloat("Frame Rate", ref CFG.Current.System_Frame_Rate, 20.0f, 240.0f);
            ImguiUtils.ShowHoverTooltip("Adjusts the frame rate of the viewport.");

            // Round FPS to the nearest whole number
            CFG.Current.System_Frame_Rate = (float)Math.Round(CFG.Current.System_Frame_Rate);

            if (ImGui.Button("Reset"))
            {
                CFG.Current.System_Frame_Rate = CFG.Default.System_Frame_Rate;
                CFG.Current.System_UI_Scale = CFG.Default.System_UI_Scale;
                Smithbox.FontRebuildRequest = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Reset AppData"))
            {
                DialogResult result = PlatformUtils.Instance.MessageBox(
                $"Do you want to delete your AppData files?",
                $"Warning", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    var configFolder = CFG.GetConfigFilePath();
                    var keybindsFolder = CFG.GetBindingsFilePath();

                    if (File.Exists(configFolder))
                    {
                        File.Delete(configFolder);
                    }
                    if (File.Exists(keybindsFolder))
                    {
                        File.Delete(keybindsFolder);
                    }

                    CFG.Save();
                }
            }
            ImguiUtils.ShowHoverTooltip("This will delete your Smithbox folder in %appdata%/Local/, allowing it to be re-generated.");
        }

        if (ImGui.CollapsingHeader("Formats"))
        {
            ImguiUtils.WrappedText("By default, files are read by Smithbox in a strict manner. Data that is present in locations that it should not be will throw an exception.");

            ImguiUtils.WrappedText("This option will remove that strictness, and will cause Smithbox to ignore the invalid data when reading a file.");

            ImGui.Checkbox("Ignore asserts", ref CFG.Current.System_IgnoreAsserts);
            ImguiUtils.ShowHoverTooltip("If enabled, when attempting to read files, asserts will be ignored.");

            Smithbox.UpdateSoulsFormatsToggles();
        }

        if (ImGui.CollapsingHeader("Loggers"))
        {
            ImGui.Checkbox("Display Information Logger", ref CFG.Current.Interface_DisplayInfoLogger);
            ImguiUtils.ShowHoverTooltip("If enabled, the information logger will be visible in the menubar.");

            Smithbox.UpdateSoulsFormatsToggles();
        }

        if (ImGui.CollapsingHeader("Soapstone Server"))
        {
            var running = SoapstoneServer.GetRunningPort() is int port
                ? $"running on port {port}"
                : "not running";
            ImGui.Text(
                $"The server is {running}.\nIt is not accessible over the network, only to other programs on this computer.\nPlease restart the program for changes to take effect.");
            ImGui.Checkbox("Enable cross-editor features", ref CFG.Current.System_Enable_Soapstone_Server);
        }

        if (ImGui.CollapsingHeader("Resources"))
        {
            ImGui.Checkbox("Alias Banks - Commit to Base", ref CFG.Current.AliasBank_EditorMode);
            ImguiUtils.ShowHoverTooltip("If enabled, editing the name and tags for alias banks will commit the changes to the Smithbox base version instead of the mod-specific version.");

            if (FeatureFlags.EnableEditor_Cutscene)
            {
                ImGui.Checkbox("Cutscene Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Cutscene);
                ImguiUtils.ShowHoverTooltip("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
            }

            if (FeatureFlags.EnableEditor_Material)
            {
                ImGui.Checkbox("Material Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Material);
                ImguiUtils.ShowHoverTooltip("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
            }

            if (FeatureFlags.EnableEditor_Particle)
            {
                ImGui.Checkbox("Particle Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Particle);
                ImguiUtils.ShowHoverTooltip("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
            }
        }

        if (ImGui.CollapsingHeader("Secret Tools"))
        {
            ImGui.Checkbox("Display Randomiser Toosl", ref CFG.Current.DisplayRandomiserTools);
            ImguiUtils.ShowHoverTooltip("If enabled, the randomiser tools will be available via the icon bar.");

            ImGui.Checkbox("Display Debug Toosl", ref CFG.Current.DisplayDebugTools);
            ImguiUtils.ShowHoverTooltip("If enabled, the debug tools will be available via the icon bar.");
        }
    }
}
