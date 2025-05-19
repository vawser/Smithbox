using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class SimpleTreasureMaker
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public string SelectedMap = "";
    public (string, MapContainer) TargetMap = ("None", null);

    public SimpleTreasureMaker(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    // Support ER only for now
    public bool IsSupported()
    {
        bool isSupported = false;

        if(Project.ProjectType is ProjectType.ER)
        {
            isSupported = true;
        }

        return isSupported;
    }

    public void Display()
    {
        UIHelper.WrappedText("Use this tool to quickly create treasures.");
        UIHelper.WrappedText("Enable the Placement Orb, as this is where the asset element of the treasure will spawn.");
        UIHelper.WrappedText("");

        UIHelper.WrappedText("Target Map:");
        foreach (var obj in Editor.Universe.LoadedObjectContainers)
        {
            var mapID = obj.Key;
            var map = obj.Value;

            if (map != null)
            {
                if (ImGui.Selectable(mapID, SelectedMap == mapID))
                {
                    SelectedMap = mapID;
                    TargetMap = (mapID, (MapContainer)map);
                }

                var mapName = AliasUtils.GetMapNameAlias(Editor.Project, mapID);
                UIHelper.DisplayAlias(mapName);
            }
        }
        UIHelper.WrappedText("");

    }

    public void GenerateCorpseTreasure(MapContainer map)
    {
        // ER Corpses:
        // AEG099_610: Commoner

        // -- Treasure
        // EventID: unique
        // PartName: blank
        // RegionName: blank
        // EntityID: 0
        // Unk14: 0
        // MapID: -1
        // UnkE0C: 255
        // UnkS04: 0
        // UnkS08: 0
        // UnkS0C: -1
        // TreasurePartName: set to newly created asset
        // ItemLotID: set to newly created lot / or blank
        // ActionButtonID: -1
        // PickupAnimID: -1
        // InChest: false
        // StartDisabled: false
    }

    public void GenerateChestTreasure(MapContainer map)
    {

    }
}
