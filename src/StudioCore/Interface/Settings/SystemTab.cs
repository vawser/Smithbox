using ImGuiNET;
using SoapstoneLib;
using SoulsFormats;
using StudioCore.Platform;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class SystemTab
{
    public SystemTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("系统 System"))
        {
            if (ImGui.CollapsingHeader("总览 General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("启动时检查更新 Check for new versions of Smithbox during startup",
                    ref CFG.Current.System_Check_Program_Update);
                ImguiUtils.ShowHoverTooltip("When enabled Smithbox will automatically check for new versions upon program start.");

                ImGui.SliderFloat("帧率 Frame Rate", ref CFG.Current.System_Frame_Rate, 20.0f, 240.0f);
                ImguiUtils.ShowHoverTooltip("Adjusts the frame rate of the viewport.");

                // Round FPS to the nearest whole number
                CFG.Current.System_Frame_Rate = (float)Math.Round(CFG.Current.System_Frame_Rate);

                if (ImGui.Button("重置 Reset"))
                {
                    CFG.Current.System_Frame_Rate = CFG.Default.System_Frame_Rate;
                    CFG.Current.System_UI_Scale = CFG.Default.System_UI_Scale;
                    Smithbox.FontRebuildRequest = true;
                }
                ImGui.SameLine();
                if (ImGui.Button("重置应用数据 Reset AppData"))
                {
                    DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"你要删除你的应用数据吗 Do you want to delete your AppData files?",
                    $"警告 Warning", MessageBoxButtons.YesNo);

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
                ImguiUtils.ShowHoverTooltip("会重置你的SmithBox到初始状态 This will delete your Smithbox folder in %appdata%/Local/, allowing it to be re-generated.");
            }

            if (ImGui.CollapsingHeader("格式 Formats"))
            {
                ImguiUtils.WrappedText("By default, files are read by Smithbox in a strict manner. Data that is present in locations that it should not be will throw an exception.");

                ImguiUtils.WrappedText("This option will remove that strictness, and will cause Smithbox to ignore the invalid data when reading a file.");

                ImGui.Checkbox("忽略异常 Ignore asserts", ref CFG.Current.System_IgnoreAsserts);
                ImguiUtils.ShowHoverTooltip("If enabled, when attempting to read files, asserts will be ignored.");

                Smithbox.UpdateFormatAssertState();
            }

            if (ImGui.CollapsingHeader("云服务器 Soapstone Server"))
            {
                var running = SoapstoneServer.GetRunningPort() is int port
                    ? $"运行端口 running on port {port}"
                    : "未运行 not running";
                ImGui.Text(
                    $"The server is {running}.\nIt is not accessible over the network, only to other programs on this computer.\nPlease restart the program for changes to take effect.");
                ImGui.Checkbox("启用交叉特征 Enable cross-editor features", ref CFG.Current.System_Enable_Soapstone_Server);
            }

            if (ImGui.CollapsingHeader("资源 Resources"))
            {
                ImGui.Checkbox("Alias Banks - Editor Mode", ref CFG.Current.AliasBank_EditorMode);
                ImguiUtils.ShowHoverTooltip("If enabled, editing the name and tags for alias banks will commit the changes to the Smithbox base version instead of the mod-specific version.");

                if (FeatureFlags.EnableEditor_TimeAct)
                {
                    ImGui.Checkbox("Time Act Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_TimeAct);
                    ImguiUtils.ShowHoverTooltip("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }

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

            ImGui.EndTabItem();
        }
    }
}
