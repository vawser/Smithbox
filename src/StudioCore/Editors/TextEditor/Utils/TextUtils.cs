using HKLib.hk2018.hkAsyncThreadPool;
using Microsoft.Extensions.Logging;
using Silk.NET.OpenGL;
using StudioCore.Core;
using StudioCore.Editors.TextEditor.Enums;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public static class TextUtils
{
    /// <summary>
    /// Whether the current project type supports the Text Editor
    /// </summary>
    public static bool IsSupportedProjectType()
    {
        return true;
    }

    /// <summary>
    /// Whether the current project supports the passed category
    /// </summary>
    public static bool IsSupportedLanguage(ProjectEntry project, TextContainerCategory category)
    {
        switch (project.ProjectType)
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
            case ProjectType.AC6:
                return CategoryGroupings.AC6_Languages.Contains(category);
            case ProjectType.AC4:
                return CategoryGroupings.AC4_Languages.Contains(category);
            case ProjectType.ACFA:
                return CategoryGroupings.ACFA_Languages.Contains(category);
            case ProjectType.ACV:
                return CategoryGroupings.ACV_Languages.Contains(category);
            case ProjectType.ACVD:
                return CategoryGroupings.ACVD_Languages.Contains(category);
            default: break;
        }

        return false;
    }

    /// <summary>
    /// Get the display name for a FMG based on the BND ID
    /// </summary>
    public static string GetFmgDisplayName(ProjectEntry project, TextContainerWrapper info, int id, string fmgName)
    {
        var name = $"Unknown";

        switch(project.ProjectType)
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
                        TaskLogs.AddLog($"Item_MsgBndID_DES: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DES: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Sample_MsgBndID_DES: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_BB: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_BB: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"SellRegion_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"SellRegion_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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

        switch (project.ProjectType)
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
                        TaskLogs.AddLog($"Item_MsgBndID_DES: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DES: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
                    }
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                if (IsTalkFolderFmg(info))
                {
                    foreach (var entry in Enum.GetValues(typeof(TalkFmgName_DS2)))
                    {
                        var enumVal = (TalkFmgName_DS2)entry;
                        name = enumVal.ToString();
                    }
                }
                else if (IsBloodMessageFolderFmg(info))
                {
                    foreach (var entry in Enum.GetValues(typeof(BloodMessageFmgName_DS2)))
                    {
                        var enumVal = (BloodMessageFmgName_DS2)entry;
                        name = enumVal.ToString();
                    }
                }
                else
                {
                    foreach (var entry in Enum.GetValues(typeof(CommonFmgName_DS2)))
                    {
                        var enumVal = (CommonFmgName_DS2)entry;
                        name = enumVal.ToString();
                    }
                }
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
                        TaskLogs.AddLog($"Item_MsgBndID_BB: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_BB: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"SellRegion_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"SellRegion_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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

        switch (project.ProjectType)
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
                        TaskLogs.AddLog($"Item_MsgBndID_DES: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Sample_MsgBndID_DES: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_BB: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_BB: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"SellRegion_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"SellRegion_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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

        switch (project.ProjectType)
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
                        TaskLogs.AddLog($"Item_MsgBndID_DES: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Sample_MsgBndID_DES: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS1: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_BB: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_BB: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_DS3: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"SellRegion_MsgBndID_SDT: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"SellRegion_MsgBndID_ER: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Item_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"Menu_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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
                        TaskLogs.AddLog($"NgWord_MsgBndID_AC6: {id} not defined", LogLevel.Warning);
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
        if(project.ProjectType is ProjectType.DES)
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
}
