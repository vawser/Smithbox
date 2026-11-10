using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Renderer;
public static class SubTextureHelper
{
    public static SubTexture GetPreviewSubTexture(ProjectEntry project, string textureKey, Param.Row context, IconConfig fieldIcon, IconConfigurationEntry iconEntry, object fieldValue, string fieldName)
    {
        // Guard clauses checking the validity of the TextureRef
        if (context[fieldName] == null)
        {
            return null;
        }

        var imageIdx = $"{fieldValue}";

        SubTexture subTex = null;

        // Special-case handling for some icons in AC6 that are awkward to fit into the Icon Configuration system
        if (iconEntry.SubTexturePrefix == "AC6-Weapon" || iconEntry.SubTexturePrefix == "AC6-Armor")
        {
            if (iconEntry.SubTexturePrefix == "AC6-Weapon")
            {
                subTex = GetMatchingSubTexture(project, textureKey, imageIdx, "WP_A_");

                // If failed, check for WP_R_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(project, textureKey, imageIdx, "WP_R_");
                }

                // If failed, check for WP_L_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(project, textureKey, imageIdx, "WP_L_");
                }
            }

            if (iconEntry.SubTexturePrefix == "AC6-Armor")
            {
                var prefix = "";

                var headEquip = context["headEquip"].Value.Value.ToString();
                var bodyEquip = context["bodyEquip"].Value.Value.ToString();
                var armEquip = context["armEquip"].Value.Value.ToString();
                var legEquip = context["legEquip"].Value.Value.ToString();

                if (headEquip == "1")
                {
                    prefix = "HD_M_";
                }
                if (bodyEquip == "1")
                {
                    prefix = "BD_M_";
                }
                if (armEquip == "1")
                {
                    prefix = "AM_M_";
                }
                if (legEquip == "1")
                {
                    prefix = "LG_M_";
                }

                subTex = GetMatchingSubTexture(project, textureKey, imageIdx, prefix);
            }
        }
        else
        {
            subTex = GetMatchingSubTexture(project, textureKey, imageIdx, iconEntry.SubTexturePrefix);
        }

        return subTex;
    }

    public static SubTexture GetMatchingSubTexture(ProjectEntry project, string currentTextureName, string imageIndex, string namePrepend)
    {
        if (project.Locator.ShoeboxFiles == null)
            return null;

        if (project.Locator.ShoeboxFiles.Entries == null)
            return null;

        var shoeboxEntry = project.Handler.TextureData.PrimaryBank.ShoeboxEntries.FirstOrDefault();

        if (shoeboxEntry.Value == null)
            return null;

        if (shoeboxEntry.Value.Textures.ContainsKey(currentTextureName))
        {
            var subTexs = shoeboxEntry.Value.Textures[currentTextureName];

            int matchId;
            var successMatch = int.TryParse(imageIndex, out matchId);

            foreach (var entry in subTexs)
            {
                var SubTexName = entry.Name.Replace(".png", "");

                Match contents = Regex.Match(SubTexName, $@"{namePrepend}([0-9]+)");
                if (contents.Success)
                {
                    var id = contents.Groups[1].Value;

                    int numId;
                    var successNum = int.TryParse(id, out numId);

                    if (successMatch && successNum && matchId == numId)
                    {
                        return entry;
                    }
                }
            }
        }

        return null;
    }
}