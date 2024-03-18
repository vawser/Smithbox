using StudioCore.Banks.FormatBank;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Banks.CorrectedTextureBank
{
    /// <summary>
    /// For textures that are pointing to other file locations, but actually reside within the existing texture file
    /// This 'corrects' the path so Smithbox can load it
    /// </summary>
    public class CorrectedTextureBank
    {
        public CorrectedTextureContainer _MappingBank { get; set; }

        public bool IsMappingBankLoading { get; set; }
        public bool CanReloadMappingBank { get; set; }

        public CorrectedTextureBank()
        {
            CanReloadMappingBank = false;
        }

        public CorrectedTextureResource Entries
        {
            get
            {
                if (IsMappingBankLoading)
                {
                    return new CorrectedTextureResource();
                }

                return _MappingBank.Data;
            }
        }


        /// <summary>
        /// Corrects a passed virtual texture path
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public string CorrectTexturePath(string virtualPath)
        {
            var newPath = virtualPath;

            if (IsBankValid())
            {
                var texturePathCorrection = Entries.list.Find(x => x.VirtualPath == virtualPath);

                if (texturePathCorrection != null)
                {
                    var overridePath = texturePathCorrection.CorrectedPath;
                    newPath = overridePath;
                }
            }

            return newPath;
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
            TaskManager.Run(new TaskManager.LiveTask($"Corrected Textures - Load Mappings", TaskManager.RequeueType.None, false,
            () =>
            {
                _MappingBank = new CorrectedTextureContainer();
                IsMappingBankLoading = true;

                if (Project.Type != ProjectType.Undefined)
                {
                    try
                    {
                        _MappingBank = new CorrectedTextureContainer();
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
