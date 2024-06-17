using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Locators;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AssetBrowser;

public static class AssetBrowserCache
{
    public static List<string> CharacterList = new List<string>();
    public static List<string> AssetList = new List<string>();
    public static List<string> PartList = new List<string>();
    public static Dictionary<string, List<string>> MapPieceDict = new Dictionary<string, List<string>>();

    public static Dictionary<string, AliasReference> Characters = new Dictionary<string, AliasReference>();
    public static Dictionary<string, AliasReference> Assets = new Dictionary<string, AliasReference>();
    public static Dictionary<string, AliasReference> Parts = new Dictionary<string, AliasReference>();
    public static Dictionary<string, AliasReference> MapPieces = new Dictionary<string, AliasReference>();

    public static bool UpdateCacheComplete = false;

    public static void UpdateCache()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Asset Browser - Load Name Cache", TaskManager.RequeueType.None, false,
        () =>
        {
            UpdateCacheComplete = false;
            EditorContainer.TextureViewer.InvalidateCachedName = true;

            Characters = new Dictionary<string, AliasReference>();
            Assets = new Dictionary<string, AliasReference>();
            Parts = new Dictionary<string, AliasReference>();
            MapPieces = new Dictionary<string, AliasReference>();

            CharacterList = AssetListLocator.GetChrModels();
            AssetList = AssetListLocator.GetObjModels();
            PartList = AssetListLocator.GetPartsModels();
            MapPieceDict = new Dictionary<string, List<string>>();

            foreach (AliasReference v in ModelAliasBank.Bank.AliasNames.GetEntries("Characters"))
            {
                if (!Characters.ContainsKey(v.id))
                {
                    Characters.Add(v.id, v);
                }
            }
            foreach (AliasReference v in ModelAliasBank.Bank.AliasNames.GetEntries("Objects"))
            {
                if (!Assets.ContainsKey(v.id))
                {
                    Assets.Add(v.id, v);
                }
            }
            foreach (AliasReference v in ModelAliasBank.Bank.AliasNames.GetEntries("Parts"))
            {
                if (!Parts.ContainsKey(v.id))
                {
                    Parts.Add(v.id, v);
                }
            }
            foreach (AliasReference v in ModelAliasBank.Bank.AliasNames.GetEntries("MapPieces"))
            {
                if (!MapPieces.ContainsKey(v.id))
                {
                    MapPieces.Add(v.id, v);
                }
            }

            List<string> mapList = ResourceMapLocator.GetFullMapList();

            foreach (var mapId in mapList)
            {
                var assetMapId = ResourceMapLocator.GetAssetMapID(mapId);

                List<ResourceDescriptor> modelList = new List<ResourceDescriptor>();

                if (Project.Type == ProjectType.DS2S || Project.Type == ProjectType.DS2)
                {
                    modelList = AssetListLocator.GetMapModelsFromBXF(mapId);
                }
                else
                {
                    modelList = AssetListLocator.GetMapModels(mapId);
                }

                var cache = new List<string>();
                foreach (var model in modelList)
                {
                    cache.Add(model.AssetName);
                }

                if (!MapPieceDict.ContainsKey(assetMapId))
                {
                    MapPieceDict.Add(assetMapId, cache);
                }
            }

            UpdateCacheComplete = true;
        }));
    }
}
