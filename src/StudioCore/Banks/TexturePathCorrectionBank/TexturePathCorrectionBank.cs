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
    /// <summary>
    /// For textures that are pointing to other file locations, but actually reside within the existing texture file
    /// This 'corrects' the path so Smithbox can load it
    /// </summary>
    public class TexturePathCorrectionBank
    {
        public TexturePathCorrectionContainer _MappingBank { get; set; }

        public bool IsMappingBankLoading { get; set; }
        public bool CanReloadMappingBank { get; set; }

        public TexturePathCorrectionBank()
        {
            CanReloadMappingBank = false;
        }

        public TexturePathCorrectionResource Entries
        {
            get
            {
                if (IsMappingBankLoading)
                {
                    return new TexturePathCorrectionResource();
                }

                return _MappingBank.Data;
            }
        }

        public void ReloadBank()
        {
            TaskManager.Run(new TaskManager.LiveTask($"Texture Links - Load Mappings", TaskManager.RequeueType.None, false,
            () =>
            {
                _MappingBank = new TexturePathCorrectionContainer();
                IsMappingBankLoading = true;

                if (Project.Type != ProjectType.Undefined)
                {
                    try
                    {
                        _MappingBank = new TexturePathCorrectionContainer();
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
