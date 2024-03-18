using StudioCore.Banks.FormatBank;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.MappingBank
{
    public class TexturePathCorrectionContainer
    {
        public TexturePathCorrectionResource Data;

        public TexturePathCorrectionContainer()
        {
            Data = LoadMappingJSON();
        }

        private TexturePathCorrectionResource LoadMappingJSON()
        {
            var baseResource = new TexturePathCorrectionResource();

            var baseResourcePath = "";

            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Mappings\\{Project.GetGameIDForDir()}\\TexturePathCorrections.json";

            if (File.Exists(baseResourcePath))
            {
                using (var stream = File.OpenRead(baseResourcePath))
                {
                    baseResource = JsonSerializer.Deserialize(stream, TexturePathCorrectionSerializationContext.Default.TexturePathCorrectionResource);
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
