using StudioCore.Banks.AliasBank;
using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Banks.TextureAdditionBank

{
    /// <summary>
    /// For textures that are pointing to other file locations, and actually reside there.
    /// This forces the Model Loading processes to load both the original and variant 
    /// </summary>
    public class TextureAdditionBank
    {
        public TextureAdditionResource TextureAdditions { get; set; }

        private string AliasDirectory = "";

        private string AliasFileName = "";

        public TextureAdditionBank()
        {
            AliasDirectory = "Mappings";
            AliasFileName = "AdditionalTextures";
        }

        public void LoadBank()
        {
            try
            {
                TextureAdditions = BankUtils.LoadTextureAdditionJSON(AliasDirectory, AliasFileName);
            }
            catch (Exception e)
            {
#if DEBUG
                TaskLogs.AddLog($"Failed to load Alias Bank {AliasFileName}: {e.Message}");
#endif
            }

            TaskLogs.AddLog($"Texture Addition Bank: Loaded Additions");
        }

        public bool HasAdditionalTextures(string modelid)
        {
            if (IsBankValid())
            {
                if (TextureAdditions.list.Any(x => x.BaseID == modelid))
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
                if (TextureAdditions.list.Any(x => x.BaseID == modelid))
                {
                    var additionalTexture = TextureAdditions.list.Find(x => x.BaseID == modelid);

                    return additionalTexture.AdditionalIDs;
                }
            }

            return new List<string>();
        }

        public bool IsBankValid()
        {
            if (TextureAdditions.list == null)
                return false;

            if (TextureAdditions == null)
                return false;

            return true;
        }
    }
}
