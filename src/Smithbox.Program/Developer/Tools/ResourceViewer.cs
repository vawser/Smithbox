using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Developer;

public class ResourceViewer
{
    public static long MeshConsumptionSize = -1;
    public static long TexConsumptionSize = -1;

    public static List<string> ProcessedMeshes = new List<string>();
    public static List<string> ProcessedTextures = new List<string>();

    public ResourceViewer() { }

    public void Display()
    {
        UIHelper.SimpleHeader("Actions", "");
        UIHelper.MultiButtonInput("resourceViewerActions",
            "clearConsumption", "Clear Tracked Consumption", "", ClearTrackedConsumption);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Load Consumption", "");

        ImGui.Text($@"Mesh Consumption (B): {MeshConsumptionSize}");
        ImGui.Text($@"Mesh Consumption (MB): {MeshConsumptionSize / 1024}");
        ImGui.Text($@"Mesh Consumption (GB): {MeshConsumptionSize / 1024 / 1024}");

        UIHelper.Spacer();

        ImGui.Text($@"Texture Consumption (B): {TexConsumptionSize}");
        ImGui.Text($@"Texture Consumption (MB): {TexConsumptionSize / 1024}");
        ImGui.Text($@"Texture Consumption (GB): {TexConsumptionSize / 1024 / 1024}");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Mesh Load Instances", $"{ProcessedMeshes.Count} entries.");

        ImGui.BeginChild("meshLoadSection", new Vector2(0, 200), ImGuiChildFlags.Borders);
        for(int i = 0; i < ProcessedMeshes.Count; i++)
        {
            var meshName = ProcessedMeshes[i];
            ImGui.Text(meshName);
        }
        ImGui.EndChild();

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Texture Load Instances", $"{ProcessedTextures.Count} entries.");

        ImGui.BeginChild("textureLoadSection", new Vector2(0, 200), ImGuiChildFlags.Borders);
        for (int i = 0; i < ProcessedTextures.Count; i++)
        {
            var meshName = ProcessedTextures[i];
            ImGui.Text(meshName);
        }
        ImGui.EndChild();
    }

    public void ClearTrackedConsumption()
    {
        MeshConsumptionSize = -1;
        TexConsumptionSize = -1;
        ProcessedMeshes.Clear();
        ProcessedTextures.Clear();
    }
}
