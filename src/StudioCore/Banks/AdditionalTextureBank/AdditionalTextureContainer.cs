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
    public class AdditionalTextureContainer
    {
        public AdditionalTextureResource Data;

        public AdditionalTextureContainer(bool load)
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

        private AdditionalTextureResource LoadMappingJSON()
        {
            var baseResource = new AdditionalTextureResource();

            var baseResourcePath = "";

            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Mappings\\{Project.GetGameIDForDir()}\\AdditionalTextures.json";

            if (File.Exists(baseResourcePath))
            {
                using (var stream = File.OpenRead(baseResourcePath))
                {
                    baseResource = JsonSerializer.Deserialize(stream, AdditionalTextureSerializationContext.Default.AdditionalTextureResource);
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
