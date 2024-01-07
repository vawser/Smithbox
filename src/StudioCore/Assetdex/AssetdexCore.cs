using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Assetdex
{
    /// <summary>
    /// Class <c>AssetdexCore</c> contains the <c>AssetReference</c> dictionaries that host the documentation for each asset.
    /// </summary>
    public class AssetdexCore
    {
        private Dictionary<GameType, AssetContainer> assetContainers = new Dictionary<GameType, AssetContainer>();

        public AssetdexCore()
        {
            assetContainers.Add(GameType.Undefined, BuildAssetContainer("DS1")); // Fallback for Undefined project
            assetContainers.Add(GameType.DemonsSouls, BuildAssetContainer("DES"));
            assetContainers.Add(GameType.DarkSoulsPTDE, BuildAssetContainer("DS1"));
            assetContainers.Add(GameType.DarkSoulsRemastered, BuildAssetContainer("DS1R"));
            assetContainers.Add(GameType.DarkSoulsIISOTFS, BuildAssetContainer("DS2S"));
            assetContainers.Add(GameType.Bloodborne, BuildAssetContainer("BB"));
            assetContainers.Add(GameType.DarkSoulsIII, BuildAssetContainer("DS3"));
            assetContainers.Add(GameType.Sekiro, BuildAssetContainer("SDT"));
            assetContainers.Add(GameType.EldenRing, BuildAssetContainer("ER"));
            assetContainers.Add(GameType.ArmoredCoreVI, BuildAssetContainer("AC6"));
        }

        private AssetContainer BuildAssetContainer(string gametype)
        {
            AssetContainer container = new AssetContainer(gametype);

            return container;
        }

        public Dictionary<string, AssetReference> GetChrEntriesForGametype(GameType gametype)
        {
            Dictionary<string, AssetReference> dict = new Dictionary<string, AssetReference>();

            foreach(AssetReference entry in assetContainers[gametype].GetChrEntries())
            {
                if(!dict.ContainsKey(entry.id.ToLower()))
                    dict.Add(entry.id.ToLower(), entry);
            }

            return dict;
        }

        public Dictionary<string, AssetReference> GetObjEntriesForGametype(GameType gametype)
        {
            Dictionary<string, AssetReference> dict = new Dictionary<string, AssetReference>();

            foreach (AssetReference entry in assetContainers[gametype].GetObjEntries())
            {
                if (!dict.ContainsKey(entry.id.ToLower()))
                    dict.Add(entry.id.ToLower(), entry);
            }

            return dict;
        }

        public Dictionary<string, AssetReference> GetPartEntriesForGametype(GameType gametype)
        {
            Dictionary<string, AssetReference> dict = new Dictionary<string, AssetReference>();

            foreach (AssetReference entry in assetContainers[gametype].GetPartEntries())
            {
                if (!dict.ContainsKey(entry.id.ToLower()))
                    dict.Add(entry.id.ToLower(), entry);
            }

            return dict;
        }

        public Dictionary<string, AssetReference> GetMapPieceEntriesForGametype(GameType gametype)
        {
            Dictionary<string, AssetReference> dict = new Dictionary<string, AssetReference>();

            foreach (AssetReference entry in assetContainers[gametype].GetMapPieceEntries())
            {
                if (!dict.ContainsKey(entry.id.ToLower()))
                    dict.Add(entry.id.ToLower(), entry);
            }

            return dict;
        }
    }
}
