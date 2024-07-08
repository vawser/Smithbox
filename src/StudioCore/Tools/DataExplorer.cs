using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Locators;

namespace StudioCore.Tools;

public static class DataExplorer
{

    public static bool TargetProject = false;

    public static List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();

    public static Dictionary<string, MSBE> maps = null;

    private static string _textInput = "";

    private static List<DataMatch> _matches = new List<DataMatch>();

    private struct DataMatch
    {
        public string MapID;
        public string Name;
    }


    public static void Display()
    {
        ImGui.Columns(2);

        ImGui.BeginChild("actionWindow");
        ImGui.Checkbox("Check project files", ref TargetProject);
        ImguiUtils.ShowHoverTooltip("The check will use the game root files by default, if you want to use your project's specific files, tick this.");

        ImGui.Text("Input String");
        ImGui.InputText("##textInput", ref _textInput, 255);

        if(ImGui.Button("Search"))
        {
            if (maps == null)
            {
                maps = new Dictionary<string, MSBE>();
                LoadMaps();
            }

            GetMatches_NpcParam();
        }
        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild("matchWindow");
        ImGui.Text("Matches:");
        foreach (var entry in _matches)
        {
            ImGui.Text($"{entry.MapID} - {AliasUtils.GetAliasFromCache(entry.MapID, Smithbox.BankHandler.MapAliases.Aliases.list)}");
            ImGui.Text($"{entry.Name} - {AliasUtils.GetAliasFromCache(entry.MapID, Smithbox.BankHandler.CharacterAliases.Aliases.list)}");
        }
        ImGui.EndChild();


        ImGui.Columns(1);
    }

    private static void GetMatches_NpcParam()
    {
        _matches = new List<DataMatch>();

        foreach (var map in maps)
        {
            foreach(var enemy in map.Value.Parts.Enemies)
            {
                if(enemy.NPCParamID.ToString() == _textInput)
                {
                    DataMatch match = new DataMatch();
                    match.MapID = map.Key;
                    match.Name = enemy.Name;
                    _matches.Add(match);
                }
            }
        }
    }

    private static void LoadMaps()
    {
        var mapDir = $"{Smithbox.GameRoot}/map/mapstudio/";

        if (TargetProject)
        {
            mapDir = $"{Smithbox.ProjectRoot}/map/mapstudio/";
        }

        foreach (var entry in Directory.EnumerateFiles(mapDir))
        {
            if (entry.Contains(".msb.dcx"))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                ResourceDescriptor ad = ResourceMapLocator.GetMapMSB(name);
                if (ad.AssetPath != null)
                {
                    resMaps.Add(ad);
                }
            }
        }

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            foreach (var res in resMaps)
            {
                var msb = MSBE.Read(res.AssetPath);

                var id = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(res.AssetPath));
                maps.Add(id, msb);
            }
        }
    }
}
