using Org.BouncyCastle.Pqc.Crypto.Lms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using static SoulsFormats.HKXPWV;

namespace StudioCore.Assetdex
{
    public class AssetContainer
    {
        private AssetResource chrEntries = new AssetResource();
        private AssetResource objEntries = new AssetResource();
        private AssetResource partEntries = new AssetResource();
        private AssetResource mapPieceEntries = new AssetResource();

        public AssetContainer(string gametype) 
        {
            chrEntries = LoadJSON(gametype, "Chr");
            objEntries = LoadJSON(gametype, "Obj");
            partEntries = LoadJSON(gametype, "Part");
            mapPieceEntries = LoadJSON(gametype, "MapPiece");
        }

        private AssetResource LoadJSON(string gametype, string type)
        {
            AssetResource resource = new AssetResource();

            string json_filepath = AppContext.BaseDirectory + $"\\Assets\\Assetdex\\{gametype}\\{type}.json";

            if (File.Exists(json_filepath))
            {
                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                };
                resource = JsonSerializer.Deserialize<AssetResource>(File.OpenRead(json_filepath), options);
            }

            return resource;
        }

        public List<AssetReference> GetChrEntries()
        {
            return chrEntries.list;
        }
        public List<AssetReference> GetObjEntries()
        {
            return objEntries.list;
        }
        public List<AssetReference> GetPartEntries()
        {
            return partEntries.list;
        }
        public List<AssetReference> GetMapPieceEntries()
        {
            return mapPieceEntries.list;
        }
    }
}
