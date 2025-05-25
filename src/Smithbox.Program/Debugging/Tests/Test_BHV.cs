using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.DebugNS;

public static class Test_BHV
{
    public static string _filePath = "";

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        var inputWidth = 400.0f;

        if (ImGui.BeginTable($"generatorTable", 3, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Input", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch);

            // File Path
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("File Path");
            UIHelper.Tooltip("The file path of the file.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(inputWidth);
            ImGui.InputText("##testPath", ref _filePath, 255);

            ImGui.TableSetColumnIndex(2);

            if (ImGui.Button("Select##testPathSelect"))
            {
                var newFilePath = "";
                var result = PlatformUtils.Instance.OpenFileDialog("Select File", [""], out newFilePath);

                if (result)
                {
                    _filePath = newFilePath;
                }
            }

            ImGui.EndTable();
        }

        if (File.Exists(_filePath))
        {
            if (ImGui.Button("Run Test"))
            {
                Run();
            }
        }
    }

    public static void Run()
    {
        var filename = Path.GetFileName(_filePath);

        var bhvFile = BHV.Read(_filePath, filename);
    }
}
