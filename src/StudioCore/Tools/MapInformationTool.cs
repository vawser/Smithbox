using ImGuiNET;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Core;
using StudioCore.Editors.MapEditor;
using StudioCore.Formats;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StudioCore.Tools
{
    public static class MapInformationTool
    {
        //public static string exportPath = $"{AppContext.BaseDirectory}";

        public static string exportPath = $"C:\\Users\\benja\\Modding\\FROM Software\\MSB\\";
        // C:\Users\benja\Modding\FROM Software\MSB

        public static bool TargetProject = false;
        public static bool OneFile = false;

        public static List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();

        public static List<MSBE> ER_Maps = new List<MSBE>();

        public static string Output = "";

        public static void SelectExportDirectory()
        {
            if (PlatformUtils.Instance.OpenFolderDialog("Select report directory...", out var selectedPath))
            {
                exportPath = selectedPath;
            }
        }

        public static void GenerateTargetReport()
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

            // ER
            if (Smithbox.ProjectType == ProjectType.ER)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSBE.Read(res.AssetPath);

                    if (msb.Regions.WeatherOverrides.Count > 0)
                    {
                        TaskLogs.AddLog($"{res.AssetName}");

                        foreach (var part in msb.Regions.WeatherOverrides)
                        {
                            TaskLogs.AddLog($"{part.UnkT08}");
                            TaskLogs.AddLog($"{part.UnkT09}");
                            TaskLogs.AddLog($"{part.UnkT0A}");
                            TaskLogs.AddLog($"{part.UnkT0B}");
                        }
                    }
                }
            }
        }

        public static void GenerateReport()
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

            // ER
            if(Smithbox.ProjectType == ProjectType.ER)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSBE.Read(res.AssetPath);
                    ProcessMap_ER(res.AssetName, msb);
                }
            }
        }

        private static void ProcessMap_ER(string mapid, MSBE map)
        {
            ProcessParts_ER(mapid, map);
            ProcessRegions_ER(mapid, map);
            ProcessEvents_ER(mapid, map);
        }

        private static void ProcessEvents_ER(string mapid, MSBE map)
        {
            var mapAlias = Smithbox.NameCacheHandler.MapNameCache.GetMapName(mapid);

            // Assets
            var assetHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8,AssetSfxParamRelativeID,UnkModelMaskAndAnimID\n";

            Output = Output + assetHeader;

            foreach (var part in map.Parts.Assets)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                if (modelId != null)
                {
                    if (Smithbox.NameCacheHandler.AssetBrowserNameCache.Assets.ContainsKey(modelId))
                        aliasName = Smithbox.NameCacheHandler.AssetBrowserNameCache.Assets[modelId].name;
                }

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                var relativesfxid = part.AssetSfxParamRelativeID;
                var modelmaskanimid = part.UnkModelMaskAndAnimID;

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8},{relativesfxid},{modelmaskanimid}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_Assets.txt", Output);
            Output = "";
        }

        private static void ProcessRegions_ER(string mapid, MSBE map)
        {
            var mapAlias = Smithbox.NameCacheHandler.MapNameCache.GetMapName(mapid);

            // Assets
            var assetHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8,AssetSfxParamRelativeID,UnkModelMaskAndAnimID\n";

            Output = Output + assetHeader;

            foreach (var part in map.Parts.Assets)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                if (modelId != null)
                {
                    if (Smithbox.NameCacheHandler.AssetBrowserNameCache.Assets.ContainsKey(modelId))
                        aliasName = Smithbox.NameCacheHandler.AssetBrowserNameCache.Assets[modelId].name;
                }

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                var relativesfxid = part.AssetSfxParamRelativeID;
                var modelmaskanimid = part.UnkModelMaskAndAnimID;

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8},{relativesfxid},{modelmaskanimid}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_Assets.txt", Output);
            Output = "";
        }

        private static void ProcessParts_ER(string mapid, MSBE map)
        {
            var mapAlias = Smithbox.NameCacheHandler.MapNameCache.GetMapName(mapid);

            // Assets
            var assetHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8,AssetSfxParamRelativeID,UnkModelMaskAndAnimID\n";

            Output = Output + assetHeader;

            foreach (var part in map.Parts.Assets)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                if (modelId != null)
                {
                    if (Smithbox.NameCacheHandler.AssetBrowserNameCache.Assets.ContainsKey(modelId))
                        aliasName = Smithbox.NameCacheHandler.AssetBrowserNameCache.Assets[modelId].name;
                }

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                var relativesfxid = part.AssetSfxParamRelativeID;
                var modelmaskanimid = part.UnkModelMaskAndAnimID;

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8},{relativesfxid},{modelmaskanimid}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_Assets.txt", Output);
            Output = "";

            // DummyAssets
            var dummyAssetHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8\n";

            Output = Output + dummyAssetHeader;

            foreach (var part in map.Parts.DummyAssets)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                if (modelId != null)
                {
                    if (Smithbox.NameCacheHandler.AssetBrowserNameCache.Assets.ContainsKey(modelId))
                        aliasName = Smithbox.NameCacheHandler.AssetBrowserNameCache.Assets[modelId].name;
                }

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_DummyAssets.txt", Output);
            Output = "";

            // Players
            var playerHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8\n";

            Output = Output + playerHeader;

            foreach (var part in map.Parts.Players)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_Players.txt", Output);
            Output = "";

            // MapPieces
            var mapPieceHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8\n";

            Output = Output + mapPieceHeader;

            foreach (var part in map.Parts.MapPieces)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                if (modelId != null)
                {
                    if (Smithbox.NameCacheHandler.AssetBrowserNameCache.MapPieces.ContainsKey(modelId))
                        aliasName = Smithbox.NameCacheHandler.AssetBrowserNameCache.MapPieces[modelId].name;
                }

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_MapPieces.txt", Output);
            Output = "";


            // Collisions
            var collisionHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8,HitFilterID,HitFilterName,LocationTextID,PlayRegionID\n";

            Output = Output + collisionHeader;

            foreach (var part in map.Parts.Collisions)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                int hitFilterTypeID = (int)part.HitFilterID;
                var hitFilterType = part.HitFilterID;
                var locationTextId = part.LocationTextID;
                var playRegionId = part.PlayRegionID;

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8},{hitFilterTypeID},{hitFilterType},{locationTextId},{playRegionId}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_Collisions.txt", Output);
            Output = "";

            // ConnectCollisions
            var connectCollisionHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8,CollisionName,MapID,MapAlias\n";

            Output = Output + connectCollisionHeader;

            foreach (var part in map.Parts.ConnectCollisions)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                var collisionName = part.CollisionName;
                var mapId_0 = part.MapID[0];
                var mapId_1 = part.MapID[1];
                var mapId_2 = part.MapID[2];
                var mapId_3 = part.MapID[3];

                if (mapId_3 == 255)
                    mapId_3 = 0;

                var collisionMapId = $"m{mapId_0}_{mapId_1}_{mapId_2}_{mapId_3}";
                var collisionMapIdAlias = Smithbox.NameCacheHandler.MapNameCache.GetMapName(collisionMapId);

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8},{collisionName},{collisionMapId},{collisionMapIdAlias}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_ConnectCollisions.txt", Output);
            Output = "";

            // Enemies
            var enemyHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8,NpcParamID,NpcThinkParamID,TalkID,PlatoonID,CharaInitID,CollisionPartName,WalkRouteName,ChrActivateCondParamID,BackupEventAnimID,spEffectSetID_1,spEffectSetID_2,spEffectSetID_3,spEffectSetID_4\n";

            Output = Output + enemyHeader;

            foreach (var part in map.Parts.Enemies)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                if(modelId != null)
                {
                    if (Smithbox.NameCacheHandler.AssetBrowserNameCache.Characters.ContainsKey(modelId))
                        aliasName = Smithbox.NameCacheHandler.AssetBrowserNameCache.Characters[modelId].name;
                }

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                var npcParam = part.NPCParamID;
                var thinkParam = part.ThinkParamID;
                var talkID = part.TalkID;
                var platoonID = part.PlatoonID;
                var chrInitId = part.CharaInitID;
                var colName = part.CollisionPartName;
                var walkName = part.WalkRouteName;
                var chrActivateCondition = part.ChrActivateCondParamID;
                var backupAnimId = part.BackupEventAnimID;
                var spEffectSetID = part.SpEffectSetParamID;
                var spEffectSetID_1 = spEffectSetID[0];
                var spEffectSetID_2 = spEffectSetID[0];
                var spEffectSetID_3 = spEffectSetID[0];
                var spEffectSetID_4 = spEffectSetID[0];

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8},{npcParam},{thinkParam},{talkID},{platoonID},{chrInitId},{colName},{walkName},{chrActivateCondition},{backupAnimId},{spEffectSetID_1},{spEffectSetID_2},{spEffectSetID_3},{spEffectSetID_4}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_Enemies.txt", Output);
            Output = "";

            // Dummy Enemies
            var dummyEnemyHeader = $"EntityID,Name,NameAlias,ModelID,entityGroupID_1,entityGroupID_2,entityGroupID_3,entityGroupID_4,entityGroupID_5,entityGroupID_6,entityGroupID_7,entityGroupID_8,NpcParamID,NpcThinkParamID,TalkID,PlatoonID,CharaInitID,CollisionPartName,WalkRouteName,ChrActivateCondParamID,BackupEventAnimID,spEffectSetID_1,spEffectSetID_2,spEffectSetID_3,spEffectSetID_4\n";

            Output = Output + dummyEnemyHeader;

            foreach (var part in map.Parts.DummyEnemies)
            {
                var partLine = "";

                var name = part.Name;
                var modelId = part.ModelName;
                var aliasName = "";

                if (modelId != null)
                {
                    if (Smithbox.NameCacheHandler.AssetBrowserNameCache.Characters.ContainsKey(modelId))
                        aliasName = Smithbox.NameCacheHandler.AssetBrowserNameCache.Characters[modelId].name;
                }

                var entityID = part.EntityID;
                var entityGroupID = part.EntityGroupIDs;
                var entityGroupID_1 = entityGroupID[0];
                var entityGroupID_2 = entityGroupID[1];
                var entityGroupID_3 = entityGroupID[2];
                var entityGroupID_4 = entityGroupID[3];
                var entityGroupID_5 = entityGroupID[4];
                var entityGroupID_6 = entityGroupID[5];
                var entityGroupID_7 = entityGroupID[6];
                var entityGroupID_8 = entityGroupID[7];

                var npcParam = part.NPCParamID;
                var thinkParam = part.ThinkParamID;
                var talkID = part.TalkID;
                var platoonID = part.PlatoonID;
                var chrInitId = part.CharaInitID;
                var colName = part.CollisionPartName;
                var walkName = part.WalkRouteName;
                var chrActivateCondition = part.ChrActivateCondParamID;
                var backupAnimId = part.BackupEventAnimID;
                var spEffectSetID = part.SpEffectSetParamID;
                var spEffectSetID_1 = spEffectSetID[0];
                var spEffectSetID_2 = spEffectSetID[0];
                var spEffectSetID_3 = spEffectSetID[0];
                var spEffectSetID_4 = spEffectSetID[0];

                partLine = $"{entityID},{name},{aliasName},{modelId},{entityGroupID_1},{entityGroupID_2},{entityGroupID_3},{entityGroupID_4},{entityGroupID_5},{entityGroupID_6},{entityGroupID_7},{entityGroupID_8},{npcParam},{thinkParam},{talkID},{platoonID},{chrInitId},{colName},{walkName},{chrActivateCondition},{backupAnimId},{spEffectSetID_1},{spEffectSetID_2},{spEffectSetID_3},{spEffectSetID_4}";

                Output = Output + partLine + "\n";
            }

            File.WriteAllText($"{exportPath}\\{mapid}_Part_DummyEnemies.txt", Output);
            Output = "";

            TaskLogs.AddLog($"{mapid} - {mapAlias} complete.");
        }
    }
}
