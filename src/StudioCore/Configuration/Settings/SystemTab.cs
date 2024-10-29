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

            ImGui.Checkbox("Enable Debug Tools", ref CFG.Current.DisplayDebugTools);
            UIHelper.ShowHoverTooltip("If enabled, the debug tools will be available via the icon bar.");

            ImGui.Separator();

            UIHelper.WrappedText("By default, files are read by Smithbox in a strict manner. Data that is present in locations that it should not be will throw an exception.");

            UIHelper.WrappedText("This option will remove that strictness, and will cause Smithbox to ignore the invalid data when reading a file.");

            ImGui.Checkbox("Ignore asserts", ref CFG.Current.System_IgnoreAsserts);
            UIHelper.ShowHoverTooltip("If enabled, when attempting to read files, asserts will be ignored.");

            Smithbox.UpdateSoulsFormatsToggles();
        }

        if (ImGui.CollapsingHeader("Editors"))
        {
            UIHelper.WrappedText("Determine which editors are enabled. Changes will only take affect after Smithbox is restarted.");
            ImGui.Checkbox("Enable Map Editor", ref CFG.Current.EnableMapEditor);
            ImGui.Checkbox("Enable Model Editor", ref CFG.Current.EnableModelEditor);
            ImGui.Checkbox("Enable Param Editor", ref CFG.Current.EnableParamEditor);
            ImGui.Checkbox("Enable Text Editor", ref CFG.Current.EnableTextEditor);
            ImGui.Checkbox("Enable Time Act Editor", ref CFG.Current.EnableTimeActEditor);
            ImGui.Checkbox("Enable Gparam Editor", ref CFG.Current.EnableGparamEditor);
            ImGui.Checkbox("Enable Texture Viewer", ref CFG.Current.EnableTextureViewer);
            ImGui.Checkbox("Enable EMEVD Editor", ref CFG.Current.EnableEmevdEditor);
            ImGui.Checkbox("Enable ESD Editor", ref CFG.Current.EnableEsdEditor);

            // WIP
            /*
            ImGui.Checkbox("Enable Cutscene Editor", ref CFG.Current.EnableCutsceneEditor);
            ImGui.Checkbox("Enable Havok Editor", ref CFG.Current.EnableHavokEditor);
            ImGui.Checkbox("Enable Material Editor", ref CFG.Current.EnableMaterialEditor);
            ImGui.Checkbox("Enable Particle Editor", ref CFG.Current.EnableParticleEditor);
            */
        }

        if (ImGui.CollapsingHeader("Aliases"))
        {
            ImGui.Checkbox("Change Base Aliases", ref CFG.Current.AliasBank_EditorMode);
            UIHelper.ShowHoverTooltip("If enabled, editing the name and tags for alias banks will commit the changes to the Smithbox base version instead of the mod-specific version.");
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
