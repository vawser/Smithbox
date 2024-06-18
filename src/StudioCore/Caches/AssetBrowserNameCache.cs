using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Caches;

public class AssetBrowserNameCache
{
    public List<string> CharacterList = new List<string>();
    public List<string> AssetList = new List<string>();
    public List<string> PartList = new List<string>();
    public Dictionary<string, List<string>> MapPieceDict = new Dictionary<string, List<string>>();

    public Dictionary<string, AliasReference> Characters = new Dictionary<string, AliasReference>();
    public Dictionary<string, AliasReference> Assets = new Dictionary<string, AliasReference>();
    public Dictionary<string, AliasReference> Parts = new Dictionary<string, AliasReference>();
    public Dictionary<string, AliasReference> MapPieces = new Dictionary<string, AliasReference>();

    public bool UpdateCacheComplete = false;

    public AssetBrowserNameCache()
    {
    }

    public void BuildCache()
    {
        UpdateCacheComplete = false;
        if (Smithbox.EditorHandler != null)
        {
            Smithbox.EditorHandler.TextureViewer.InvalidateCachedName = true;
        }

        Characters = new Dictionary<string, AliasReference>();
        Assets = new Dictionary<string, AliasReference>();
        Parts = new Dictionary<string, AliasReference>();
        MapPieces = new Dictionary<string, AliasReference>();

        CharacterList = AssetListLocator.GetChrModels();
        AssetList = AssetListLocator.GetObjModels();
        PartList = AssetListLocator.GetPartsModels();
        MapPieceDict = new Dictionary<string, List<string>>();

        if (Smithbox.BankHandler.CharacterAliases.Aliases != null)
        {
            Characters = Smithbox.BankHandler.CharacterAliases.GetEntries();
        }
        if (Smithbox.BankHandler.AssetAliases.Aliases != null)
        {
            Assets = Smithbox.BankHandler.AssetAliases.GetEntries();
        }
        if (Smithbox.BankHandler.PartAliases.Aliases != null)
        {
            Parts = Smithbox.BankHandler.PartAliases.GetEntries();
        }
        if (Smithbox.BankHandler.MapPieceAliases.Aliases != null)
        {
            MapPieces = Smithbox.BankHandler.MapPieceAliases.GetEntries();
        }

        List<string> mapList = ResourceMapLocator.GetFullMapList();

        foreach (var mapId in mapList)
        {
            var assetMapId = ResourceMapLocator.GetAssetMapID(mapId);

            List<ResourceDescriptor> modelList = new List<ResourceDescriptor>();

            if (Smithbox.ProjectType == ProjectType.DS2S || Smithbox.ProjectType == ProjectType.DS2)
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

        TaskLogs.AddLog($"Name Cache: Loaded Asset Browser Names");
    }
}
