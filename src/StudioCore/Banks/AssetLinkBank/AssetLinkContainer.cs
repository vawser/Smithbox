using StudioCore.Banks.FormatBank;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.ChrLinkBank
{
    public class AssetLinkContainer
    {
        public AssetLinkResource Data;

        public AssetLinkContainer(bool load)
        {
            if(load)
            {
                Data = LoadMappingJSON();
            }
            else
            {
                Data = null;
            }
        }

        private AssetLinkResource LoadMappingJSON()
        {
            var baseResource = new AssetLinkResource();

            var baseResourcePath = "";

            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Mappings\\{Project.GetGameIDForDir()}\\AssetLinks.json";

            if (File.Exists(baseResourcePath))
            {
                using (var stream = File.OpenRead(baseResourcePath))
                {
                    baseResource = JsonSerializer.Deserialize(stream, AssetLinkSerializationContext.Default.AssetLinkResource);
                }
            }
            else
            {
                TaskLogs.AddLog($"{baseResourcePath} does not exist!");
            }

            return baseResource;
        }
    }
}
