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
    public class MappingContainer
    {
        public MappingResource Data;

        private MappingBankType MappingBankType;

        public MappingContainer()
        {
            Data = new MappingResource();

            MappingBankType = MappingBankType.None;
        }

        public MappingContainer(MappingBankType mappingBankType)
        {
            MappingBankType = mappingBankType;

            Data = LoadMappingJSON();
        }

        private MappingResource LoadMappingJSON()
        {
            var baseResource = new MappingResource();

            if (MappingBankType is MappingBankType.None)
                return null;

            var baseResourcePath = "";

            if (MappingBankType is MappingBankType.Texture)
            {
                baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Mappings\\{Project.GetGameIDForDir()}\\{GetTypeFileName()}.json";
            }

            if (File.Exists(baseResourcePath))
            {
                using (var stream = File.OpenRead(baseResourcePath))
                {
                    baseResource = JsonSerializer.Deserialize(stream, MappingResourceSerializationContext.Default.MappingResource);
                }
            }
            else
            {
                TaskLogs.AddLog($"{baseResourcePath} does not exist!");
            }

            return baseResource;
        }

        private string GetTypeFileName()
        {
            var typDir = "";

            if (MappingBankType is MappingBankType.Texture)
                typDir = "Texture";

            return typDir;
        }
    }
}
