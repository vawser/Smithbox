using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Banks.TextureCorrectionBank
{
    /// <summary>
    /// For textures that are pointing to other file locations, but actually reside within the existing texture file
    /// This 'corrects' the path so Smithbox can load it
    /// </summary>
    public class TextureCorrectionBank
    {
        public TextureCorrectionResource TextureCorrections { get; set; }

        private string AliasDirectory = "";

        private string AliasFileName = "";

        public TextureCorrectionBank()
        {
            AliasDirectory = "Mappings";
            AliasFileName = "CorrectedTextures";
        }

        public void LoadBank()
        {
            try
            {
                TextureCorrections = BankUtils.LoadTextureCorrectionJSON(AliasDirectory, AliasFileName);
            }
            catch (Exception e)
            {
#if DEBUG
                TaskLogs.AddLog($"Failed to load Alias Bank {AliasFileName}: {e.Message}");
#endif
            }

            TaskLogs.AddLog($"Texture Correction Bank: Loaded Corrections");
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
                var texturePathCorrection = TextureCorrections.list.Find(x => x.VirtualPath == virtualPath);

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
            if (TextureCorrections.list == null)
                return false;

            if (TextureCorrections == null)
                return false;

            return true;
        }
    }
}
