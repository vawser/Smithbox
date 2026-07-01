using Microsoft.Extensions.Logging;
using Octokit;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

public static class TextUtils
{
    public static string GetPrimaryLanguage()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        switch(curProject.Descriptor.ProjectType)
        {
            case ProjectType.DES:  return CFG.Current.TextEditor_Primary_Language_DES;
            case ProjectType.DS1:  return CFG.Current.TextEditor_Primary_Language_DS1;
            case ProjectType.DS1R: return CFG.Current.TextEditor_Primary_Language_DS1R;
            case ProjectType.DS2:  return CFG.Current.TextEditor_Primary_Language_DS2;
            case ProjectType.DS2S: return CFG.Current.TextEditor_Primary_Language_DS2S;
            case ProjectType.BB:   return CFG.Current.TextEditor_Primary_Language_BB;
            case ProjectType.DS3:  return CFG.Current.TextEditor_Primary_Language_DS3;
            case ProjectType.SDT:  return CFG.Current.TextEditor_Primary_Language_SDT;
            case ProjectType.ER:   return CFG.Current.TextEditor_Primary_Language_ER;
            case ProjectType.AC6:  return CFG.Current.TextEditor_Primary_Language_AC6;
            case ProjectType.NR:   return CFG.Current.TextEditor_Primary_Language_NR;
        }

        return "English";
    }

    public static LanguageDescriptor GetPrimaryLanguageDescriptor()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject.Handler.TextData != null)
        {
            var primaryLang = curProject.Handler.TextData.FmgDescriptors.Languages.FirstOrDefault(e => e.Language == GetPrimaryLanguage());

            return primaryLang;
        }

        return null;
    }

    public static LanguageDescriptor GetContainerLanguageDescriptor(string path)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject.Handler.TextData != null)
        {
            var containerLang = curProject.Handler.TextData.FmgDescriptors.Languages.FirstOrDefault(e => path.Contains(e.Abbreviation));

            return containerLang;
        }

        return null;
    }
    public static FmgContainerDescriptor GetContainerDescriptor(FileDictionaryEntry entry)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject.Handler.TextData != null)
        {
            var desc = curProject.Handler.TextData.FmgDescriptors.Containers.FirstOrDefault(e => e.SimpleName == entry.Filename);

            return desc;
        }

        return null;
    }


    public static void ValidateLanguage()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        // Validate that the current CFG language exists, other reset to English
        if (curProject.Handler.TextData != null)
        {
            var languages = curProject.Handler.TextData.FmgDescriptors.Languages;

            switch (curProject.Descriptor.ProjectType)
            {
                case ProjectType.DES:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_DES))
                    {
                        CFG.Current.TextEditor_Primary_Language_DES = "English";
                    }
                    break;
                case ProjectType.DS1:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_DS1))
                    {
                        CFG.Current.TextEditor_Primary_Language_DS1 = "English";
                    }
                    break;
                case ProjectType.DS1R:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_DS1R))
                    {
                        CFG.Current.TextEditor_Primary_Language_DS1R = "English";
                    }
                    break;
                case ProjectType.DS2:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_DS2))
                    {
                        CFG.Current.TextEditor_Primary_Language_DS2 = "English";
                    }
                    break;
                case ProjectType.DS2S:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_DS2S))
                    {
                        CFG.Current.TextEditor_Primary_Language_DS2S = "English";
                    }
                    break;
                case ProjectType.DS3:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_DS3))
                    {
                        CFG.Current.TextEditor_Primary_Language_DS3 = "English";
                    }
                    break;
                case ProjectType.BB:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_BB))
                    {
                        CFG.Current.TextEditor_Primary_Language_BB = "English";
                    }
                    break;
                case ProjectType.SDT:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_SDT))
                    {
                        CFG.Current.TextEditor_Primary_Language_SDT = "English";
                    }
                    break;
                case ProjectType.ER:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_ER))
                    {
                        CFG.Current.TextEditor_Primary_Language_ER = "English";
                    }
                    break;
                case ProjectType.AC6:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_AC6))
                    {
                        CFG.Current.TextEditor_Primary_Language_AC6 = "English";
                    }
                    break;
                case ProjectType.NR:
                    if (languages.Any(e => e.Language != CFG.Current.TextEditor_Primary_Language_NR))
                    {
                        CFG.Current.TextEditor_Primary_Language_NR = "English";
                    }
                    break;
            }

        }
    }



    // TODO: remove the defunct stuff below once FMG descriptor change is done

    /// <summary>
    /// Whether the current project supports the passed category
    /// </summary>
    public static bool IsSupportedLanguage(ProjectEntry project, TextContainerCategory category)
    {
        switch (project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                return CategoryGroupings.DES_Languages.Contains(category);
            case ProjectType.DS1:
                return CategoryGroupings.DS1_Languages.Contains(category);
            case ProjectType.DS1R:
                return CategoryGroupings.DS1R_Languages.Contains(category);
            case ProjectType.DS2:
            case ProjectType.DS2S:
                return CategoryGroupings.DS2_Languages.Contains(category);
            case ProjectType.BB:
                return CategoryGroupings.BB_Languages.Contains(category);
            case ProjectType.DS3:
                return CategoryGroupings.DS3_Languages.Contains(category);
            case ProjectType.SDT:
                return CategoryGroupings.SDT_Languages.Contains(category);
            case ProjectType.ER:
                return CategoryGroupings.ER_Languages.Contains(category);
            case ProjectType.NR:
                return CategoryGroupings.NR_Languages.Contains(category);
            case ProjectType.AC6:
                return CategoryGroupings.AC6_Languages.Contains(category);
            default: break;
        }

        return false;
    }

    public static List<TextContainerCategory> GetSupportedLanguages(ProjectEntry project)
    {
        switch (project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                return CategoryGroupings.DES_Languages;
            case ProjectType.DS1:
                return CategoryGroupings.DS1_Languages;
            case ProjectType.DS1R:
                return CategoryGroupings.DS1R_Languages;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                return CategoryGroupings.DS2_Languages;
            case ProjectType.BB:
                return CategoryGroupings.BB_Languages;
            case ProjectType.DS3:
                return CategoryGroupings.DS3_Languages;
            case ProjectType.SDT:
                return CategoryGroupings.SDT_Languages;
            case ProjectType.ER:
                return CategoryGroupings.ER_Languages;
            case ProjectType.NR:
                return CategoryGroupings.NR_Languages;
            case ProjectType.AC6:
                return CategoryGroupings.AC6_Languages;
            default: break;
        }

        return new List<TextContainerCategory>();
    }

    /// <summary>
    /// Get the display name for a FMG based on the BND ID
    /// </summary>
    public static string GetFmgDisplayName(ProjectEntry project, TextContainerWrapper info, int id, string fmgName)
    {
        var name = $"Unknown";

        switch(project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DES), id))
                    {
                        var enumObj = (Item_MsgBndID_DES)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName", 
                            "DES", id, "Item", nameof(Item_MsgBndID_DES), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DES), id))
                    {
                        var enumObj = (Menu_MsgBndID_DES)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "DES", id, "Menu", nameof(Menu_MsgBndID_DES), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsSampleContainer(info))
                {
                    if (Enum.IsDefined(typeof(Sample_MsgBndID_DES), id))
                    {
                        var enumObj = (Sample_MsgBndID_DES)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "DES", id, "Sample", nameof(Sample_MsgBndID_DES), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.DS1: 
            case ProjectType.DS1R: 
                if(IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DS1), id))
                    {
                        var enumObj = (Item_MsgBndID_DS1)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "DS1", id, "Item", nameof(Item_MsgBndID_DS1), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DS1), id))
                    {
                        var enumObj = (Menu_MsgBndID_DS1)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "DS1", id, "Menu", nameof(Menu_MsgBndID_DS1), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.DS2: 
            case ProjectType.DS2S:
                if (IsTalkFolderFmg(info))
                {
                    foreach(var entry in Enum.GetValues(typeof(TalkFmgName_DS2)))
                    {
                        var enumVal = (TalkFmgName_DS2)Enum.Parse(typeof(TalkFmgName_DS2), entry.ToString());
                        var internalName = enumVal.ToString();

                        if(internalName == Path.GetFileNameWithoutExtension(fmgName))
                        {
                            name = enumVal.GetDisplayName();
                        }
                    }
                }
                else if (IsBloodMessageFolderFmg(info))
                {
                    foreach (var entry in Enum.GetValues(typeof(BloodMessageFmgName_DS2)))
                    {
                        var enumVal = (BloodMessageFmgName_DS2)Enum.Parse(typeof(BloodMessageFmgName_DS2), entry.ToString());
                        var internalName = enumVal.ToString();

                        if (internalName == Path.GetFileNameWithoutExtension(fmgName))
                        {
                            name = enumVal.GetDisplayName();
                        }
                    }
                }
                else
                {
                    foreach (var entry in Enum.GetValues(typeof(CommonFmgName_DS2)))
                    {
                        var enumVal = (CommonFmgName_DS2)Enum.Parse(typeof(CommonFmgName_DS2), entry.ToString());
                        var internalName = enumVal.ToString();

                        if (internalName == Path.GetFileNameWithoutExtension(fmgName))
                        {
                            name = enumVal.GetDisplayName();
                        }
                    }
                }
                break;
            case ProjectType.BB:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_BB), id))
                    {
                        var enumObj = (Item_MsgBndID_BB)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "BB", id, "Item", nameof(Item_MsgBndID_BB), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_BB), id))
                    {
                        var enumObj = (Menu_MsgBndID_BB)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "BB", id, "Menu", nameof(Menu_MsgBndID_BB), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.DS3:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DS3), id))
                    {
                        var enumObj = (Item_MsgBndID_DS3)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "DS3", id, "Item", nameof(Item_MsgBndID_DS3), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DS3), id))
                    {
                        var enumObj = (Menu_MsgBndID_DS3)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "DS3", id, "Menu", nameof(Menu_MsgBndID_DS3), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_DS3), id))
                    {
                        var enumObj = (NgWord_MsgBndID_DS3)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "DS3", id, "NgWord", nameof(NgWord_MsgBndID_DS3), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.SDT:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_SDT), id))
                    {
                        var enumObj = (Item_MsgBndID_SDT)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "SDT", id, "Item", nameof(Item_MsgBndID_SDT), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_SDT), id))
                    {
                        var enumObj = (Menu_MsgBndID_SDT)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "SDT", id, "Menu", nameof(Menu_MsgBndID_SDT), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsSellRegionContainer(info))
                {
                    if (Enum.IsDefined(typeof(SellRegion_MsgBndID_SDT), id))
                    {
                        var enumObj = (SellRegion_MsgBndID_SDT)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "SDT", id, "SellRegion", nameof(SellRegion_MsgBndID_SDT), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.ER:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_ER), id))
                    {
                        var enumObj = (Item_MsgBndID_ER)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "ER", id, "Item", nameof(Item_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_ER), id))
                    {
                        var enumObj = (Menu_MsgBndID_ER)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "ER", id, "Menu", nameof(Menu_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_ER), id))
                    {
                        var enumObj = (NgWord_MsgBndID_ER)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "ER", id, "NgWord", nameof(NgWord_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsSellRegionContainer(info))
                {
                    if (Enum.IsDefined(typeof(SellRegion_MsgBndID_ER), id))
                    {
                        var enumObj = (SellRegion_MsgBndID_ER)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "ER", id, "SellRegion", nameof(SellRegion_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.AC6:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_AC6), id))
                    {
                        var enumObj = (Item_MsgBndID_AC6)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "AC6", id, "Item", nameof(Item_MsgBndID_AC6), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_AC6), id))
                    {
                        var enumObj = (Menu_MsgBndID_AC6)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "AC6", id, "Menu", nameof(Menu_MsgBndID_AC6), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_AC6), id))
                    {
                        var enumObj = (NgWord_MsgBndID_AC6)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "AC6", id, "NgWord", nameof(NgWord_MsgBndID_AC6), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.NR:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_NR), id))
                    {
                        var enumObj = (Item_MsgBndID_NR)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "NR", id, "Item", nameof(Item_MsgBndID_NR), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_NR), id))
                    {
                        var enumObj = (Menu_MsgBndID_NR)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "NR", id, "Menu", nameof(Menu_MsgBndID_NR), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_NR), id))
                    {
                        var enumObj = (NgWord_MsgBndID_NR)id;
                        name = enumObj.GetDisplayName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DisplayName",
                            "NR", id, "NgWord", nameof(NgWord_MsgBndID_NR), fmgName, info.FileEntry.Path));
                    }
                }
                break;

            default: break;
        }

        return name;
    }

    /// <summary>
    /// Get the internal name for a FMG based on the BND ID
    /// </summary>
    public static string GetFmgInternalName(ProjectEntry project, TextContainerWrapper info, int id, string fmgName)
    {
        var name = $"";

        switch (project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DES), id))
                    {
                        var enumObj = (Item_MsgBndID_DES)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "DES", id, "Item", nameof(Item_MsgBndID_DES), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DES), id))
                    {
                        var enumObj = (Menu_MsgBndID_DES)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "DES", id, "Menu", nameof(Menu_MsgBndID_DES), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DS1), id))
                    {
                        var enumObj = (Item_MsgBndID_DS1)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "DS1", id, "Item", nameof(Item_MsgBndID_DS1), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DS1), id))
                    {
                        var enumObj = (Menu_MsgBndID_DS1)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "DS1", id, "Menu", nameof(Menu_MsgBndID_DS1), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                name = fmgName;
                break;
            case ProjectType.BB:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_BB), id))
                    {
                        var enumObj = (Item_MsgBndID_BB)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "BB", id, "Item", nameof(Item_MsgBndID_BB), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_BB), id))
                    {
                        var enumObj = (Menu_MsgBndID_BB)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "BB", id, "Menu", nameof(Menu_MsgBndID_BB), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.DS3:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DS3), id))
                    {
                        var enumObj = (Item_MsgBndID_DS3)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "DS3", id, "Item", nameof(Item_MsgBndID_DS3), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DS3), id))
                    {
                        var enumObj = (Menu_MsgBndID_DS3)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "DS3", id, "Menu", nameof(Menu_MsgBndID_DS3), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_DS3), id))
                    {
                        var enumObj = (NgWord_MsgBndID_DS3)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "DS3", id, "NgWord", nameof(NgWord_MsgBndID_DS3), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.SDT:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_SDT), id))
                    {
                        var enumObj = (Item_MsgBndID_SDT)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "SDT", id, "Item", nameof(Item_MsgBndID_SDT), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_SDT), id))
                    {
                        var enumObj = (Menu_MsgBndID_SDT)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "SDT", id, "Menu", nameof(Menu_MsgBndID_SDT), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsSellRegionContainer(info))
                {
                    if (Enum.IsDefined(typeof(SellRegion_MsgBndID_SDT), id))
                    {
                        var enumObj = (SellRegion_MsgBndID_SDT)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "SDT", id, "SellRegion", nameof(SellRegion_MsgBndID_SDT), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.ER:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_ER), id))
                    {
                        var enumObj = (Item_MsgBndID_ER)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "ER", id, "Item", nameof(Item_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_ER), id))
                    {
                        var enumObj = (Menu_MsgBndID_ER)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "ER", id, "Menu", nameof(Menu_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_ER), id))
                    {
                        var enumObj = (NgWord_MsgBndID_ER)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "ER", id, "NgWord", nameof(NgWord_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsSellRegionContainer(info))
                {
                    if (Enum.IsDefined(typeof(SellRegion_MsgBndID_ER), id))
                    {
                        var enumObj = (SellRegion_MsgBndID_ER)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "ER", id, "SellRegion", nameof(SellRegion_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.AC6:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_AC6), id))
                    {
                        var enumObj = (Item_MsgBndID_AC6)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "AC6", id, "Item", nameof(Item_MsgBndID_AC6), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_AC6), id))
                    {
                        var enumObj = (Menu_MsgBndID_AC6)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "AC6", id, "Menu", nameof(Menu_MsgBndID_AC6), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_AC6), id))
                    {
                        var enumObj = (NgWord_MsgBndID_AC6)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "AC6", id, "NgWord", nameof(NgWord_MsgBndID_AC6), fmgName, info.FileEntry.Path));
                    }
                }
                break;

            case ProjectType.NR:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_NR), id))
                    {
                        var enumObj = (Item_MsgBndID_NR)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "NR", id, "Item", nameof(Item_MsgBndID_NR), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_NR), id))
                    {
                        var enumObj = (Menu_MsgBndID_NR)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "NR", id, "Menu", nameof(Menu_MsgBndID_NR), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_NR), id))
                    {
                        var enumObj = (NgWord_MsgBndID_NR)id;
                        name = enumObj.ToString();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_InternalName",
                            "NR", id, "NgWord", nameof(NgWord_MsgBndID_NR), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            default: break;
        }

        return name;
    }

    /// <summary>
    /// Get the grouping string for a FMG based on the BND ID
    /// </summary>
    public static string GetFmgGrouping(ProjectEntry project, TextContainerWrapper info, int id, string fmgName)
    {
        var name = $"Unknown";

        switch (project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DES), id))
                    {
                        var enumObj = (Item_MsgBndID_DES)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "DES", id, "Item", nameof(Item_MsgBndID_DES), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DES), id))
                    {
                        var enumObj = (Menu_MsgBndID_DES)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "DES", id, "Menu", nameof(Menu_MsgBndID_DES), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsSampleContainer(info))
                {
                    if (Enum.IsDefined(typeof(Sample_MsgBndID_DES), id))
                    {
                        var enumObj = (Sample_MsgBndID_DES)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "DES", id, "Sample", nameof(Sample_MsgBndID_DES), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DS1), id))
                    {
                        var enumObj = (Item_MsgBndID_DS1)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "DS1", id, "Item", nameof(Item_MsgBndID_DS1), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DS1), id))
                    {
                        var enumObj = (Menu_MsgBndID_DS1)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "DS1", id, "Menu", nameof(Menu_MsgBndID_DS1), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                if (IsTalkFolderFmg(info))
                {
                    foreach (var entry in Enum.GetValues(typeof(TalkFmgName_DS2)))
                    {
                        var enumVal = (TalkFmgName_DS2)Enum.Parse(typeof(TalkFmgName_DS2), entry.ToString());
                        var internalName = enumVal.ToString();

                        if (internalName == Path.GetFileNameWithoutExtension(fmgName))
                        {
                            name = enumVal.GetShortName();
                        }
                    }
                }
                else if (IsBloodMessageFolderFmg(info))
                {
                    foreach (var entry in Enum.GetValues(typeof(BloodMessageFmgName_DS2)))
                    {
                        var enumVal = (BloodMessageFmgName_DS2)Enum.Parse(typeof(BloodMessageFmgName_DS2), entry.ToString());
                        var internalName = enumVal.ToString();

                        if (internalName == Path.GetFileNameWithoutExtension(fmgName))
                        {
                            name = enumVal.GetShortName();
                        }
                    }
                }
                else
                {
                    foreach (var entry in Enum.GetValues(typeof(CommonFmgName_DS2)))
                    {
                        var enumVal = (CommonFmgName_DS2)Enum.Parse(typeof(CommonFmgName_DS2), entry.ToString());
                        var internalName = enumVal.ToString();

                        if (internalName == Path.GetFileNameWithoutExtension(fmgName))
                        {
                            name = enumVal.GetShortName();
                        }
                    }
                }
                break;
            case ProjectType.BB:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_BB), id))
                    {
                        var enumObj = (Item_MsgBndID_BB)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "BB", id, "Item", nameof(Item_MsgBndID_BB), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_BB), id))
                    {
                        var enumObj = (Menu_MsgBndID_BB)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "BB", id, "Menu", nameof(Menu_MsgBndID_BB), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.DS3:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DS3), id))
                    {
                        var enumObj = (Item_MsgBndID_DS3)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "DS3", id, "Item", nameof(Item_MsgBndID_DS3), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DS3), id))
                    {
                        var enumObj = (Menu_MsgBndID_DS3)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "DS3", id, "Menu", nameof(Menu_MsgBndID_DS3), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_DS3), id))
                    {
                        var enumObj = (NgWord_MsgBndID_DS3)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "DS3", id, "NgWord", nameof(NgWord_MsgBndID_DS3), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.SDT:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_SDT), id))
                    {
                        var enumObj = (Item_MsgBndID_SDT)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "SDT", id, "Item", nameof(Item_MsgBndID_SDT), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_SDT), id))
                    {
                        var enumObj = (Menu_MsgBndID_SDT)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "SDT", id, "Menu", nameof(Menu_MsgBndID_SDT), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsSellRegionContainer(info))
                {
                    if (Enum.IsDefined(typeof(SellRegion_MsgBndID_SDT), id))
                    {
                        var enumObj = (SellRegion_MsgBndID_SDT)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "SDT", id, "SellRegion", nameof(SellRegion_MsgBndID_SDT), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.ER:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_ER), id))
                    {
                        var enumObj = (Item_MsgBndID_ER)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "ER", id, "Item", nameof(Item_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_ER), id))
                    {
                        var enumObj = (Menu_MsgBndID_ER)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "ER", id, "Menu", nameof(Menu_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_ER), id))
                    {
                        var enumObj = (NgWord_MsgBndID_ER)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "ER", id, "NgWord", nameof(NgWord_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsSellRegionContainer(info))
                {
                    if (Enum.IsDefined(typeof(SellRegion_MsgBndID_ER), id))
                    {
                        var enumObj = (SellRegion_MsgBndID_ER)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "ER", id, "SellRegion", nameof(SellRegion_MsgBndID_ER), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.AC6:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_AC6), id))
                    {
                        var enumObj = (Item_MsgBndID_AC6)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "AC6", id, "Item", nameof(Item_MsgBndID_AC6), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_AC6), id))
                    {
                        var enumObj = (Menu_MsgBndID_AC6)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "AC6", id, "Menu", nameof(Menu_MsgBndID_AC6), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_AC6), id))
                    {
                        var enumObj = (NgWord_MsgBndID_AC6)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "AC6", id, "NgWord", nameof(NgWord_MsgBndID_AC6), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            case ProjectType.NR:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_NR), id))
                    {
                        var enumObj = (Item_MsgBndID_NR)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "NR", id, "Item", nameof(Item_MsgBndID_NR), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_NR), id))
                    {
                        var enumObj = (Menu_MsgBndID_NR)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "NR", id, "Menu", nameof(Menu_MsgBndID_NR), fmgName, info.FileEntry.Path));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_NR), id))
                    {
                        var enumObj = (NgWord_MsgBndID_NR)id;
                        name = enumObj.GetShortName();
                    }
                    else
                    {
                        Smithbox.LogError(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_FmgGrouping",
                            "NR", id, "NgWord", nameof(NgWord_MsgBndID_NR), fmgName, info.FileEntry.Path));
                    }
                }
                break;
            default: break;
        }

        return name;
    }

    /// <summary>
    /// Get the grouping string for a FMG based on the BND ID
    /// </summary>
    public static string GetFmgDlcGrouping(ProjectEntry project, TextContainerWrapper info, int id, string fmgName)
    {
        var name = $"Unknown";

        switch (project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DES), id))
                    {
                        var enumObj = (Item_MsgBndID_DES)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Item_MsgBndID_DES", id));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DES), id))
                    {
                        var enumObj = (Menu_MsgBndID_DES)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Menu_MsgBndID_DES", id));
                    }
                }
                else if (IsSampleContainer(info))
                {
                    if (Enum.IsDefined(typeof(Sample_MsgBndID_DES), id))
                    {
                        var enumObj = (Sample_MsgBndID_DES)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Sample_MsgBndID_DES", id));
                    }
                }
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DS1), id))
                    {
                        var enumObj = (Item_MsgBndID_DS1)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Item_MsgBndID_DS1", id));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DS1), id))
                    {
                        var enumObj = (Menu_MsgBndID_DS1)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Menu_MsgBndID_DS1", id));
                    }
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                if (IsTalkFolderFmg(info))
                {
                    foreach (var entry in Enum.GetValues(typeof(TalkFmgName_DS2)))
                    {
                        var enumVal = (TalkFmgName_DS2)Enum.Parse(typeof(TalkFmgName_DS2), entry.ToString());
                        var internalName = enumVal.ToString();

                        if (internalName == Path.GetFileNameWithoutExtension(fmgName))
                        {
                            name = enumVal.GetDescription();
                        }
                    }
                }
                else if (IsBloodMessageFolderFmg(info))
                {
                    foreach (var entry in Enum.GetValues(typeof(BloodMessageFmgName_DS2)))
                    {
                        var enumVal = (BloodMessageFmgName_DS2)Enum.Parse(typeof(BloodMessageFmgName_DS2), entry.ToString());
                        var internalName = enumVal.ToString();

                        if (internalName == Path.GetFileNameWithoutExtension(fmgName))
                        {
                            name = enumVal.GetDescription();
                        }
                    }
                }
                else
                {
                    foreach (var entry in Enum.GetValues(typeof(CommonFmgName_DS2)))
                    {
                        var enumVal = (CommonFmgName_DS2)Enum.Parse(typeof(CommonFmgName_DS2), entry.ToString());
                        var internalName = enumVal.ToString();

                        if (internalName == Path.GetFileNameWithoutExtension(fmgName))
                        {
                            name = enumVal.GetDescription();
                        }
                    }
                }
                break;
            case ProjectType.BB:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_BB), id))
                    {
                        var enumObj = (Item_MsgBndID_BB)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Item_MsgBndID_BB", id));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_BB), id))
                    {
                        var enumObj = (Menu_MsgBndID_BB)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Menu_MsgBndID_BB", id));
                    }
                }
                break;
            case ProjectType.DS3:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_DS3), id))
                    {
                        var enumObj = (Item_MsgBndID_DS3)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Item_MsgBndID_DS3", id));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_DS3), id))
                    {
                        var enumObj = (Menu_MsgBndID_DS3)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Menu_MsgBndID_DS3", id));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_DS3), id))
                    {
                        var enumObj = (NgWord_MsgBndID_DS3)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "NgWord_MsgBndID_DS3", id));
                    }
                }
                break;
            case ProjectType.SDT:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_SDT), id))
                    {
                        var enumObj = (Item_MsgBndID_SDT)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Item_MsgBndID_SDT", id));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_SDT), id))
                    {
                        var enumObj = (Menu_MsgBndID_SDT)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Menu_MsgBndID_SDT", id));
                    }
                }
                else if (IsSellRegionContainer(info))
                {
                    if (Enum.IsDefined(typeof(SellRegion_MsgBndID_SDT), id))
                    {
                        var enumObj = (SellRegion_MsgBndID_SDT)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "SellRegion_MsgBndID_SDT", id));
                    }
                }
                break;
            case ProjectType.ER:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_ER), id))
                    {
                        var enumObj = (Item_MsgBndID_ER)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Item_MsgBndID_ER", id));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_ER), id))
                    {
                        var enumObj = (Menu_MsgBndID_ER)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Menu_MsgBndID_ER", id));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_ER), id))
                    {
                        var enumObj = (NgWord_MsgBndID_ER)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "NgWord_MsgBndID_ER", id));
                    }
                }
                else if (IsSellRegionContainer(info))
                {
                    if (Enum.IsDefined(typeof(SellRegion_MsgBndID_ER), id))
                    {
                        var enumObj = (SellRegion_MsgBndID_ER)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "SellRegion_MsgBndID_ER", id));
                    }
                }
                break;
            case ProjectType.AC6:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_AC6), id))
                    {
                        var enumObj = (Item_MsgBndID_AC6)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Item_MsgBndID_AC6", id));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_AC6), id))
                    {
                        var enumObj = (Menu_MsgBndID_AC6)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Menu_MsgBndID_AC6", id));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_AC6), id))
                    {
                        var enumObj = (NgWord_MsgBndID_AC6)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "NgWord_MsgBndID_AC6", id));
                    }
                }
                break;
            case ProjectType.NR:
                if (IsItemContainer(info))
                {
                    if (Enum.IsDefined(typeof(Item_MsgBndID_NR), id))
                    {
                        var enumObj = (Item_MsgBndID_NR)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Item_MsgBndID_NR", id));
                    }
                }
                else if (IsMenuContainer(info))
                {
                    if (Enum.IsDefined(typeof(Menu_MsgBndID_NR), id))
                    {
                        var enumObj = (Menu_MsgBndID_NR)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "Menu_MsgBndID_NR", id));
                    }
                }
                else if (IsNgWordContainer(info))
                {
                    if (Enum.IsDefined(typeof(NgWord_MsgBndID_NR), id))
                    {
                        var enumObj = (NgWord_MsgBndID_NR)id;
                        name = enumObj.GetDescription();
                    }
                    else
                    {
                        Smithbox.Log(typeof(TextUtils),
                            LOC.Get("TEXT_Util_Log_Missing_Enum_DlcGrouping",
                            "NgWord_MsgBndID_NR", id));
                    }
                }
                break;
            default: break;
        }

        return name;
    }

    /// <summary>
    /// Returns true if there are any FMG entries for the target group string
    /// </summary>
    public static bool HasGroupEntries(ProjectEntry project, TextContainerWrapper info, string target)
    {
        foreach (var fmgInfo in info.FmgWrappers)
        {
            var id = fmgInfo.ID;
            var fmgName = fmgInfo.Name;
            var displayGroup = TextUtils.GetFmgGrouping(project, info, id, fmgName);

            if (displayGroup == target)
            {
                return true;
            }
        }

        return false;
    }

    /// Returns true if there are any DLC FMG entries for the target DLC string
    /// </summary>
    public static bool HasDLCEntries(ProjectEntry project, TextContainerWrapper info, string target)
    {
        foreach (var fmgInfo in info.FmgWrappers)
        {
            var id = fmgInfo.ID;
            var fmgName = fmgInfo.Name;
            var dlcGroup = TextUtils.GetFmgDlcGrouping(project, info, id, fmgName);

            if (dlcGroup == target)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The FMGs to display for Simple View
    /// </summary>
    public static bool IsSimpleFmg(string displayGroup)
    {
        if (displayGroup == "Common" ||
            displayGroup == "Title" ||
            displayGroup == "Menu" ||
            displayGroup == "Unknown")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if FMG container is an item FMG container
    /// </summary>
    public static bool IsItemContainer(TextContainerWrapper info)
    {
        if (info.FileEntry.Path.Contains("item"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if FMG container is an menu FMG container
    /// </summary>
    public static bool IsMenuContainer(TextContainerWrapper info)
    {
        if (info.FileEntry.Path.Contains("menu"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if FMG container is an menu FMG container
    /// </summary>
    public static bool IsSampleContainer(TextContainerWrapper info)
    {
        if (info.FileEntry.Path.Contains("sample"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if FMG container is an ngword FMG container
    /// </summary>
    public static bool IsNgWordContainer(TextContainerWrapper info)
    {
        if (info.FileEntry.Path.Contains("ngword"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// BB onwards: check if FMG container is a sellregion 
    /// </summary>
    public static bool IsSellRegionContainer(TextContainerWrapper info)
    {
        if (info.FileEntry.Path.Contains("sellregion"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// DS2 only: check if FMG is part of the bloodmes folder
    /// </summary>
    public static bool IsBloodMessageFolderFmg(TextContainerWrapper info)
    {
        // Second part is so we ignore the bloodmessage fmgs
        if (info.FileEntry.Path.Contains("bloodmes") && !info.FileEntry.Path.Contains("bloodmessage"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// DS2 only: check if FMG is part of the talk folder
    /// </summary>
    public static bool IsTalkFolderFmg(TextContainerWrapper info)
    {
        if (info.FileEntry.Path.Contains("talk"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determine the container category based on the container filepath.
    /// Used to determine the 'language' grouping each container belongs to.
    /// </summary>
    public static TextContainerCategory GetLanguageCategory(ProjectEntry project, string path)
    {
        var group = TextContainerCategory.None;

        // Normalize the directory separators for folder checking
        path = path.Replace('\\', '/').Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');

        // Get a lowercase version for easier checking in some games
        string pathLower = path.ToLowerInvariant();

        // Special-case: DES the msg folder has Japanese, so default to Japanese.
        if(project.Descriptor.ProjectType is ProjectType.DES)
        {
            group = TextContainerCategory.Japanese;
        }

        // Common
        if (path.Contains("common"))
        {
            group = TextContainerCategory.Common;
        }

        // English (US)
        if (path.Contains("ENGLISH") ||
            path.Contains("english") ||
            path.Contains("engus") ||
            pathLower.Contains("lang/en") ||
            pathLower.Contains("lang/us"))
        {
            group = TextContainerCategory.English;
        }

        // English (UK)
        if (path.Contains("enggb") ||
            pathLower.Contains("lang/uk"))
        {
            group = TextContainerCategory.EnglishUK;
        }

        // French
        if (path.Contains("FRENCH") ||
            path.Contains("french") ||
            path.Contains("frafr") ||
            pathLower.Contains("lang/fr"))
        {
            group = TextContainerCategory.French;
        }

        // German
        if (path.Contains("GERMAN") ||
            path.Contains("germany") ||
            path.Contains("deude") ||
            pathLower.Contains("lang/ge"))
        {
            group = TextContainerCategory.German;
        }

        // Italian
        if (path.Contains("ITALIAN") ||
            path.Contains("italian") ||
            path.Contains("itait") ||
            pathLower.Contains("lang/it"))
        {
            group = TextContainerCategory.Italian;
        }

        // Japanese
        if (path.Contains("JAPANESE") ||
            path.Contains("japanese") ||
            path.Contains("jpnjp") ||
            pathLower.Contains("lang/jp"))
        {
            group = TextContainerCategory.Japanese;
        }

        // Korean
        if (path.Contains("KOREAN") ||
            path.Contains("korean") ||
            path.Contains("korkr") ||
            pathLower.Contains("lang/kr"))
        {
            group = TextContainerCategory.Korean;
        }

        // Polish
        if (path.Contains("POLISH") ||
            path.Contains("polish") ||
            path.Contains("polpl"))
        {
            group = TextContainerCategory.Polish;
        }

        // Russian
        if (path.Contains("RUSSIAN") ||
            path.Contains("russian") ||
            path.Contains("rusru"))
        {
            group = TextContainerCategory.Russian;
        }

        // Spanish
        if (path.Contains("SPANISH") ||
            path.Contains("spanish") ||
            path.Contains("spaes") ||
            pathLower.Contains("lang/es") ||
            pathLower.Contains("lang/sp"))
        {
            group = TextContainerCategory.Spanish;
        }

        // Spanish (Neutral)
        if (path.Contains("neutralspanish"))
        {
            group = TextContainerCategory.SpanishNeutral;
        }

        // Spanish (Latin)
        if (path.Contains("NSPANISH") ||
            path.Contains("spaar"))
        {
            group = TextContainerCategory.SpanishLatin;
        }

        // Traditional Chinese
        if (path.Contains("TCHINESE") ||
            path.Contains("chinese") ||
            path.Contains("zhotw") ||
            pathLower.Contains("lang/cn"))
        {
            group = TextContainerCategory.TraditionalChinese;
        }

        // Simplified Chinese
        if (path.Contains("SCHINESE") || 
            path.Contains("zhocn"))
        {
            group = TextContainerCategory.SimplifiedChinese;
        }

        // Danish
        if (path.Contains("dandk"))
        {
            group = TextContainerCategory.Danish;
        }

        // Finnish
        if (path.Contains("finfi"))
        {
            group = TextContainerCategory.Finnish;
        }

        // Dutch
        if (path.Contains("nldnl"))
        {
            group = TextContainerCategory.Dutch;
        }

        // Norwegian
        if (path.Contains("norno"))
        {
            group = TextContainerCategory.Norwegian;
        }

        // Swedish
        if (path.Contains("swese"))
        {
            group = TextContainerCategory.Swedish;
        }

        // Turkish
        if (path.Contains("turtr"))
        {
            group = TextContainerCategory.Turkish;
        }

        // Portuguese (Latin)
        if (path.Contains("PORTUGUESE") || 
            path.Contains("portuguese") ||
            path.Contains("porbr"))
        {
            group = TextContainerCategory.PortugueseLatin;
        }

        // Portuguese 
        if (path.Contains("porpt"))
        {
            group = TextContainerCategory.Portuguese;
        }

        // Arabic 
        if (path.Contains("araae"))
        {
            group = TextContainerCategory.Arabic;
        }

        // Thai 
        if (path.Contains("thath"))
        {
            group = TextContainerCategory.Thai;
        }

        // Sell Region
        if (path.Contains("sellregion"))
        {
            group = TextContainerCategory.SellRegion;
        }

        return group;
    }

    /// <summary>
    /// Get the sub category for DS2 fgms
    /// </summary>
    public static ContainerSubCategory GetSubCategory(string path)
    {
        if(path.Contains("bloodmes"))
        {
            return ContainerSubCategory.bloodmes;
        }

        if (path.Contains("talk"))
        {
            return ContainerSubCategory.talk;
        }

        return ContainerSubCategory.common;
    }

    public static bool IsObsoleteContainer(ProjectEntry project, FileDictionaryEntry entry)
    {
        if (!CFG.Current.TextEditor_Container_List_Display_Obsolete_Containers)
        {
            switch (project.Descriptor.ProjectType)
            {
                // NR DLC update means these containers are no longer present
                case ProjectType.NR:
                    if (entry.Filename == "item" || entry.Filename == "menu")
                        return true;
                break;
            }
        }

        return false;
    }

    public static string GetStoredTextDirectory(ProjectEntry project)
    {
        return Path.Join(project.Descriptor.ProjectPath, ".smithbox", "Workflow", "Exported Text");
    }

    public static List<string> GetStoredContainerWrappers(ProjectEntry project)
    {
        List<string> results = new();

        var wrapperDir = GetStoredTextDirectory(project);

        if (Directory.Exists(wrapperDir))
        {
            foreach (var entry in Directory.GetFiles(wrapperDir))
            {
                results.Add(entry);
            }
        }

        return results;
    }
}
