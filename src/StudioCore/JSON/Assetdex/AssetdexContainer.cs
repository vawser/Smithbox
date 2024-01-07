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

namespace StudioCore.JSON.Assetdex
{
    public class AssetdexContainer
    {
        private AssetdexResource chrEntries = new AssetdexResource();
        private AssetdexResource objEntries = new AssetdexResource();
        private AssetdexResource partEntries = new AssetdexResource();
        private AssetdexResource mapPieceEntries = new AssetdexResource();

        public AssetdexContainer(string gametype)
        {
            chrEntries = LoadJSON(gametype, "Chr");
            objEntries = LoadJSON(gametype, "Obj");
            partEntries = LoadJSON(gametype, "Part");
            mapPieceEntries = LoadJSON(gametype, "MapPiece");
        }

        private AssetdexResource LoadJSON(string gametype, string type)
        {
            var resource = new AssetdexResource();

            var json_filepath = AppContext.BaseDirectory + $"\\Assets\\Assetdex\\{gametype}\\{type}.json";

            if (File.Exists(json_filepath))
            {
                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    TypeInfoResolver = new DefaultJsonTypeInfoResolver()
                };
                resource = JsonSerializer.Deserialize<AssetdexResource>(File.OpenRead(json_filepath), options);
            }

            return resource;
        }

        public List<AssetdexReference> GetChrEntries()
        {
            return chrEntries.list;
        }
        public List<AssetdexReference> GetObjEntries()
        {
            return objEntries.list;
        }
        public List<AssetdexReference> GetPartEntries()
        {
            return partEntries.list;
        }
        public List<AssetdexReference> GetMapPieceEntries()
        {
            return mapPieceEntries.list;
        }
    }
}
