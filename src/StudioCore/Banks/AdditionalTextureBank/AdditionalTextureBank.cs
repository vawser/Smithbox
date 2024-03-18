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

namespace StudioCore.Banks.ChrLinkBank
{
    /// <summary>
    /// For textures that are pointing to other file locations, and actually reside there.
    /// This forces the Model Loading processes to load both the original and variant 
    /// </summary>
    public class AdditionalTextureBank
    {
        public AdditionalTextureContainer _MappingBank { get; set; }

        public bool IsMappingBankLoading { get; set; }
        public bool CanReloadMappingBank { get; set; }

        public AdditionalTextureBank()
        {
            CanReloadMappingBank = false;
        }

        public AdditionalTextureResource Entries
        {
            get
            {
                if (IsMappingBankLoading)
                {
                    return new AdditionalTextureResource();
                }

                return _MappingBank.Data;
            }
        }

        public bool HasAdditionalTextures(string modelid)
        {
            if (IsBankValid())
            {
                if (Entries.list.Any(x => x.BaseID == modelid))
                {
                    return true;
                }
            }

            return false;
        }

        public List<string> GetAdditionalTextures(string modelid)
        {
            if (IsBankValid())
            {
                if (Entries.list.Any(x => x.BaseID == modelid))
                {
                    var additionalTexture = AdditionalTextures.Bank.Entries.list.Find(x => x.BaseID == modelid);

                    return additionalTexture.AdditionalIDs;
                }
            }

            return new List<string>();
        }

        public bool IsBankValid()
        {
            if (Entries.list == null)
                return false;

            if (Entries == null)
                return false;

            return true;
        }

        public void ReloadBank()
        {
            TaskManager.Run(new TaskManager.LiveTask($"Additional Textures - Load Mappings", TaskManager.RequeueType.None, false,
            () =>
            {
                _MappingBank = new AdditionalTextureContainer(false);
                IsMappingBankLoading = true;

                if (Project.Type != ProjectType.Undefined)
                {
                    try
                    {
                        _MappingBank = new AdditionalTextureContainer(true);
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
