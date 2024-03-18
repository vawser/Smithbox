using StudioCore.Banks.FormatBank;
using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Banks.ChrLinkBank
{
    /// <summary>
    /// For textures that are pointing to other file locations, and actually reside there.
    /// This forces the Model Loading processes to load both the original and variant 
    /// </summary>
    public class AssetLinkBank
    {
        public AssetLinkContainer _MappingBank { get; set; }

        public bool IsMappingBankLoading { get; set; }
        public bool CanReloadMappingBank { get; set; }

        public AssetLinkBank()
        {
            CanReloadMappingBank = false;
        }

        public AssetLinkResource Entries
        {
            get
            {
                if (IsMappingBankLoading)
                {
                    return new AssetLinkResource();
                }

                return _MappingBank.Data;
            }
        }

        public void ReloadBank()
        {
            TaskManager.Run(new TaskManager.LiveTask($"Character ID Links - Load Mappings", TaskManager.RequeueType.None, false,
            () =>
            {
                _MappingBank = new AssetLinkContainer(false);
                IsMappingBankLoading = true;

                if (Project.Type != ProjectType.Undefined)
                {
                    try
                    {
                        _MappingBank = new AssetLinkContainer(true);
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
