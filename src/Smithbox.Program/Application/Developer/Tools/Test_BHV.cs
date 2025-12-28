using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Utilities;
using System.IO;

namespace StudioCore.Application;

public static class Test_BHV
{
    public static string _filePath = "";

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
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

            DPI.ApplyInputWidth();
            ImGui.InputText("##testPath", ref _filePath, 255);

            ImGui.TableSetColumnIndex(2);

            if (ImGui.Button("Select##testPathSelect", DPI.StandardButtonSize))
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
            if (ImGui.Button("Run Test", DPI.StandardButtonSize))
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
