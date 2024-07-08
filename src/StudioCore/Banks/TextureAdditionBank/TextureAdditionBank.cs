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
            TaskManager.Run(new TaskManager.LiveTask($"Load Texture Additions", TaskManager.RequeueType.WaitThenRequeue, false, () =>
            {
                try
                {
                    TextureAdditions = BankUtils.LoadTextureAdditionJSON(AliasDirectory, AliasFileName);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to load {AliasFileName}: {e.Message}");
                }
            }));
        }

        public List<TextureAdditionReference> GetList()
        {
            if (TextureAdditions == null)
                return new List<TextureAdditionReference>();

            if (TextureAdditions.list == null)
                return new List<TextureAdditionReference>();

            return TextureAdditions.list;
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
