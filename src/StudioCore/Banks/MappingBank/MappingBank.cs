using StudioCore.Banks.FormatBank;
using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Banks.MappingBank
{
    public class MappingBank
    {
        public MappingContainer _MappingBank { get; set; }

        public bool IsMappingBankLoading { get; set; }
        public bool CanReloadMappingBank { get; set; }

        private string MappingInfoName = "";

        private MappingBankType MappingBankType;

        public MappingBank(MappingBankType mappingBankType)
        {
            CanReloadMappingBank = false;

            MappingBankType = mappingBankType;

            if (MappingBankType is MappingBankType.Texture)
            {
                MappingInfoName = "Texture";
            }
        }

        public MappingResource Entries
        {
            get
            {
                if (IsMappingBankLoading)
                {
                    return new MappingResource();
                }

                return _MappingBank.Data;
            }
        }

        public void ReloadBank()
        {
            TaskManager.Run(new TaskManager.LiveTask($"Mapping Info - Load {MappingInfoName} Mappings", TaskManager.RequeueType.None, false,
            () =>
            {
                _MappingBank = new MappingContainer();
                IsMappingBankLoading = true;

                if (Project.Type != ProjectType.Undefined)
                {
                    try
                    {
                        _MappingBank = new MappingContainer(MappingBankType);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"FAILED LOAD: {e.Message}");
                    }

                    IsMappingBankLoading = false;
                }
                else
                    IsMappingBankLoading = false;
            }));
        }
    }
}
