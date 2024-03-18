using StudioCore.Banks.FormatBank;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MSB_AC6;

namespace StudioCore.Banks.BlockedTextureBank
{
    public class BlockedTextureBank
    {
        public BlockedTextureContainer _MappingBank { get; set; }

        public bool IsMappingBankLoading { get; set; }
        public bool CanReloadMappingBank { get; set; }

        public BlockedTextureBank()
        {
            CanReloadMappingBank = false;
        }

        public BlockedTextureResource Entries
        {
            get
            {
                if (IsMappingBankLoading)
                {
                    return new BlockedTextureResource();
                }

                return _MappingBank.Data;
            }
        }

        public bool IsBlockedTexture(string virtualPath)
        {
            if (IsBankValid())
            {
                foreach (var entry in Entries.list)
                {
                    if (virtualPath.Contains(entry))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsBankValid()
        {
            if (Entries.list == null)
            {
                return false;
            }

            if (Entries == null)
            {
                return false;
            }

            return true;
        }

        public void ReloadBank()
        {
            TaskManager.Run(new TaskManager.LiveTask($"Blocked Textures - Load Mappings", TaskManager.RequeueType.None, false,
            () =>
            {
                _MappingBank = new BlockedTextureContainer(false);
                IsMappingBankLoading = true;

                if (Project.Type != ProjectType.Undefined)
                {
                    try
                    {
                        _MappingBank = new BlockedTextureContainer(true);
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
