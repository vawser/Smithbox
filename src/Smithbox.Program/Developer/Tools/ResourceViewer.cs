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
        UIHelper.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Actions"),
            LOC.Get("DEV_Tool_Header_Actions_TT"));

        UIHelper.MultiButtonInput("resourceViewerActions",
            "clearConsumption", 
            LOC.Get("DEV_Tool_Action_Clear_Tracked_Consumption"),
            LOC.Get("DEV_Tool_Action_Clear_Tracked_Consumption_TT"),
            ClearTrackedConsumption);

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Load_Consumption"),
            LOC.Get("DEV_Tool_Header_Load_Consumption_TT"));

        ImGui.Text(LOC.Get("DEV_Tool_Mesh_Consumption_B", MeshConsumptionSize));
        ImGui.Text(LOC.Get("DEV_Tool_Mesh_Consumption_MB", MeshConsumptionSize / 1024));
        ImGui.Text(LOC.Get("DEV_Tool_Mesh_Consumption_GB", MeshConsumptionSize / 1024 / 1024));

        UIHelper.Spacer();

        ImGui.Text(LOC.Get("DEV_Tool_Texture_Consumption_B", TexConsumptionSize));
        ImGui.Text(LOC.Get("DEV_Tool_Texture_Consumption_MB", TexConsumptionSize / 1024));
        ImGui.Text(LOC.Get("DEV_Tool_Texture_Consumption_GB", TexConsumptionSize / 1024 / 1024));

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Mesh_Load_Instances"),
            LOC.Get("DEV_Tool_Header_Mesh_Load_Instances", ProcessedMeshes.Count));

        ImGui.BeginChild("meshLoadSection", new Vector2(0, 200), ImGuiChildFlags.Borders);
        for(int i = 0; i < ProcessedMeshes.Count; i++)
        {
            var meshName = ProcessedMeshes[i];
            ImGui.Text(meshName);
        }
        ImGui.EndChild();

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Texture_Load_Instances"),
            LOC.Get("DEV_Tool_Header_Texture_Load_Instances_TT", ProcessedTextures.Count)); 

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
