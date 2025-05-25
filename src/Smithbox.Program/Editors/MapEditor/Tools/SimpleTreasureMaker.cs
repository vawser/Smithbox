using CsvHelper.Configuration.Attributes;
using Hexa.NET.ImGui;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Defaults;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.MapEditor.Tools;

public class SimpleTreasureMaker
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public string SelectedMap = "";
    public (string, ObjectContainer) TargetMap = ("None", null);

    public SimpleTreasureMaker(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    // Disable for now
    public bool IsSupported()
    {
        bool isSupported = false;

        if(Project.ProjectType is ProjectType.ER)
        {
            //isSupported = true;
        }

        return isSupported;
    }

    private TreasureCreationType CreationType;
    private string CurCreationType = "Corpse";

    public void Display()
    {
        UIHelper.WrappedText("Use this tool to quickly create treasures.");
        UIHelper.WrappedText("Enable the Placement Orb, as this is where the asset element of the treasure will spawn.");
        UIHelper.WrappedText("");

        UIHelper.WrappedText("Target Map:");

        if(!Editor.IsAnyMapLoaded())
        {
            UIHelper.WrappedText("No maps loaded yet.");
        }

        foreach (var entry in Project.MapData.PrimaryBank.Maps)
        {
            var mapID = entry.Key.Filename;
            var map = entry.Value.MapContainer;

            if (map != null)
            {
                if (ImGui.Selectable(mapID, SelectedMap == mapID))
                {
                    SelectedMap = mapID;
                    TargetMap = (mapID, map);
                }

                var mapName = AliasUtils.GetMapNameAlias(Editor.Project, mapID);
                UIHelper.DisplayAlias(mapName);
            }
        }
        UIHelper.WrappedText("");

        UIHelper.WrappedText("Creation Type:");
        ImGui.SetNextItemWidth(400f);
        if (ImGui.BeginCombo("##TreasureCreationType", CurCreationType))
        {
            // Treasure Type
            foreach (var entry in Enum.GetValues<TreasureCreationType>())
            {
                if (ImGui.Selectable($"{entry.GetDisplayName()}", CreationType == entry))
                {
                    CurCreationType = $"{entry}";
                    CreationType = entry;
                }
            }

            ImGui.EndCombo();
        }

        UIHelper.WrappedText("");

        // Corpse
        if (CreationType is TreasureCreationType.Corpse)
        {
            ImGui.SetNextItemWidth(400f);
            if (ImGui.Button("Create##createCorpseEntry"))
            {
                var objContainer = TargetMap.Item2;

                if (objContainer != null)
                {
                    var mapContainer = (MapContainer)objContainer;

                    var newTreasure = MapObjectDefaults.CreateBlankTreasure_ER();
                    var newAsset = MapObjectDefaults.CreateBlankAsset_ER();

                    var newTreasureEntity = new MsbEntity(Editor, objContainer, newTreasure);
                    var newAssetEntity = new MsbEntity(Editor, objContainer, newAsset);

                    var newEntities = new List<MsbEntity> 
                    {
                        newTreasureEntity,
                        newAssetEntity
                    };

                    Entity parent = objContainer.RootObject;

                    AddMapObjectsAction act = new(Editor, mapContainer, newEntities, true, parent);

                    Editor.EditorActionManager.ExecuteAction(act);
                }
            }
        }

        // Chest
        if (CreationType is TreasureCreationType.Chest)
        {
            ImGui.SetNextItemWidth(400f);
            if (ImGui.Button("Create##createChestEntry"))
            {

            }
        }
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

public enum TreasureCreationType
{
    [Display(Name = "Corpse")]
    Corpse,
    [Display(Name = "Chest")]
    Chest
}
