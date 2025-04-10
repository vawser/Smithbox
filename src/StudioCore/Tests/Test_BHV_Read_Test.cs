using Hexa.NET.ImGui;
using SoulsFormats;
using System.IO;
using System.Numerics;

namespace StudioCore.Tests;

public static class Test_BHV_Read_Test
{
    private static string bhvFilePath = "";

    public static void Display()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        ImGui.Text("BHV File Path");
        ImGui.InputText("##bhvFilePath", ref bhvFilePath, 255);

        if (ImGui.Button("Read Test", buttonSize))
        {
            Run();
        }
    }

    public static void Run()
    {
        var filename = Path.GetFileName(bhvFilePath);

        var bhvFile = BHV.Read(bhvFilePath, filename);
    }
}
