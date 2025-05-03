using SoulsFormats;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Utils;

public static class TextParamUtils
{
    public static List<FMG.Entry> GetFmgEntriesByAssociatedParam(ParamEditorScreen editor, string paramName)
    {
        if (editor.Project.TextEditor == null)
            return new List<FMG.Entry>();

        List<FMG.Entry> entries = new List<FMG.Entry>();

        var searchStr = GetAssociatedEnumString(paramName);
        if(searchStr != "")
        {
            foreach(var (path, entry) in editor.Project.TextData.PrimaryBank.Entries)
            {
                if (entry.ContainerDisplayCategory == CFG.Current.TextEditor_PrimaryCategory)
                {
                    foreach (var fmgInfo in entry.FmgWrappers)
                    {
                        var enumName = TextUtils.GetFmgInternalName(editor.Project, entry, fmgInfo.ID, fmgInfo.Name);

                        if (enumName.Contains(searchStr))
                        {
                            foreach(var fmgEntry in fmgInfo.File.Entries)
                            {
                                entries.Add(fmgEntry);
                            }
                        }
                    }
                }
            }
        }

        return entries;
    }

    /// <summary>
    /// Aligned with Text Categorizations, if they change this needs to change
    /// </summary>
    public static string GetAssociatedEnumString(string paramName)
    {
        if(paramName == "EquipParamAccessory")
        {
            return "Title_Accessories";
        }
        if (paramName == "EquipParamGoods")
        {
            return "Title_Goods";
        }
        if (paramName == "EquipParamWeapon")
        {
            return "Title_Weapons";
        }
        if (paramName == "Magic")
        {
            return "Title_Magic";
        }
        if (paramName == "EquipParamProtector")
        {
            return "Title_Armor";
        }
        if (paramName == "EquipParamGem")
        {
            return "Title_Ash_of_War";
        }
        if (paramName == "SwordArtsParam")
        {
            return "Title_Skills";
        }
        if (paramName == "EquipParamGenerator")
        {
            return "Title_Generator";
        }
        if (paramName == "EquipParamFcs")
        {
            return "Title_FCS";
        }
        if (paramName == "EquipParamBooster")
        {
            return "Title_Booster";
        }
        if (paramName == "MissionParam")
        {
            return "Mission_Name";
        }
        if (paramName == "ArchiveParam")
        {
            return "Archive_Name";
        }

        return "";
    }
}
