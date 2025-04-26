using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudioCore.Core.ProjectNS;

public static class EditorSettings
{
    private static bool Display = false;
    private static bool Open = false;
    public static bool Commit = false;

    public static void Setup()
    {

    }

    public static void Show()
    {
        Display = true;
    }

    public static void Draw()
    {
        if (Display)
        {
            Open = true;
            ImGui.OpenPopup("Editor Settings");
            Display = false;
        }

        var inputWidth = 400.0f;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        ImGui.SetNextWindowSize(new Vector2(1200, 800), ImGuiCond.Always);

        if (ImGui.BeginPopupModal("Editor Settings", ref Open, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                // Ignore Read Asserts
                ImGui.Checkbox("Ignore Read Asserts##ignoreReadAsserts", ref CFG.Current.System_IgnoreAsserts);
                UIHelper.Tooltip("If true, file reads will ignore failed asserts. Useful for loading files that have been 'corrupted' intentionally.");
            }

            if (ImGui.CollapsingHeader("Mod Engine", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputText("Executable Path##modEnginePath", ref CFG.Current.ModEngineInstall, 255);
                UIHelper.Tooltip("Select the modengine2_launcher.exe within your ModEngine2 install folder.");

                ImGui.SameLine();

                if (ImGui.Button("Select##modEnginePathSelect"))
                {
                    using (var dialog = new OpenFileDialog())
                    {
                        DialogResult result = dialog.ShowDialog();

                        if (result == DialogResult.OK)
                        {
                            if (dialog.FileName.Contains("modengine2_launcher.exe"))
                            {
                                CFG.Current.ModEngineInstall = dialog.FileName;
                            }
                            else
                            {
                                MessageBox.Show("The file you selected was not modengine2_launcher.exe");
                            }
                        }
                    }
                }

                ImGui.InputText("DLL Entries##modEngineDllEntries", ref CFG.Current.ModEngineDlls, 255);
                UIHelper.Tooltip("The relative paths of the DLLs to include in the 'Launch Mod' action. Separate them by a space if using multiple.");
            }

            ImGui.EndPopup();
        }
    }
}

