using HKLib.hk2018.hkAsyncThreadPool;
using StudioCore.Core.Project;
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
    /// <returns></returns>
    public static bool IsSupportedProjectType()
    {
        if(Smithbox.ProjectType is ProjectType.DES)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get the display name for a FMG based on the BND ID
    /// </summary>
    public static string GetFmgDisplayName(TextContainerInfo info, int id)
    {
        var name = $"Unknown";

        switch(Smithbox.ProjectType)
        {
            case ProjectType.DES:
                break;
            case ProjectType.DS1: 
            case ProjectType.DS1R: 
                if(IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_DS1)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_DS1)id;
                    name = enumObj.GetDisplayName();
                }
                break;
            case ProjectType.DS2: 
            case ProjectType.DS2S:
                if (IsTalkFmg(info))
                {
                    var enumObj = (TalkFmgName_DS2)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsBloodMessageFmg(info))
                {
                    var enumObj = (BloodMessageFmgName_DS2)id;
                    name = enumObj.GetDisplayName();
                }
                else
                {
                    var enumObj = (CommonFmgName_DS2)id;
                    name = enumObj.GetDisplayName();
                }
                break;
            case ProjectType.BB:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_BB)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_BB)id;
                    name = enumObj.GetDisplayName();
                }
                break;
            case ProjectType.DS3:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_DS3)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_DS3)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsNgWordContainer(info))
                {
                    var enumObj = (NgWord_MsgBndID_DS3)id;
                    name = enumObj.GetDisplayName();
                }
                break;
            case ProjectType.SDT:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_SDT)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_SDT)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsSellRegionContainer(info))
                {
                    var enumObj = (SellRegion_MsgBndID_SDT)id;
                    name = enumObj.GetDisplayName();
                }
                break;
            case ProjectType.ER:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_ER)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_ER)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsNgWordContainer(info))
                {
                    var enumObj = (NgWord_MsgBndID_ER)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsSellRegionContainer(info))
                {
                    var enumObj = (SellRegion_MsgBndID_ER)id;
                    name = enumObj.GetDisplayName();
                }
                break;
            case ProjectType.AC6:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_AC6)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_AC6)id;
                    name = enumObj.GetDisplayName();
                }
                else if (IsNgWordContainer(info))
                {
                    var enumObj = (NgWord_MsgBndID_AC6)id;
                    name = enumObj.GetDisplayName();
                }
                break;

            default: break;
        }

        return name;
    }

    /// <summary>
    /// Get the internal name for a FMG based on the BND ID
    /// </summary>
    public static string GetFmgInternalName(TextContainerInfo info, int id)
    {
        var name = $"";

        switch (Smithbox.ProjectType)
        {
            case ProjectType.DES:
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_DS1)id;
                    name = $"{enumObj}";
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_DS1)id;
                    name = $"{enumObj}";
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                if (IsTalkFmg(info))
                {
                    var enumObj = (TalkFmgName_DS2)id;
                    name = $"{enumObj}";
                }
                else if (IsBloodMessageFmg(info))
                {
                    var enumObj = (BloodMessageFmgName_DS2)id;
                    name = $"{enumObj}";
                }
                else
                {
                    var enumObj = (CommonFmgName_DS2)id;
                    name = $"{enumObj}";
                }
                break;
            case ProjectType.BB:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_BB)id;
                    name = $"{enumObj}";
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_BB)id;
                    name = $"{enumObj}";
                }
                break;
            case ProjectType.DS3:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_DS3)id;
                    name = $"{enumObj}";
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_DS3)id;
                    name = $"{enumObj}";
                }
                else if (IsNgWordContainer(info))
                {
                    var enumObj = (NgWord_MsgBndID_DS3)id;
                    name = $"{enumObj}";
                }
                break;
            case ProjectType.SDT:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_SDT)id;
                    name = $"{enumObj}";
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_SDT)id;
                    name = $"{enumObj}";
                }
                else if (IsSellRegionContainer(info))
                {
                    var enumObj = (SellRegion_MsgBndID_SDT)id;
                    name = $"{enumObj}";
                }
                break;
            case ProjectType.ER:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_ER)id;
                    name = $"{enumObj}";
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_ER)id;
                    name = $"{enumObj}";
                }
                else if (IsNgWordContainer(info))
                {
                    var enumObj = (NgWord_MsgBndID_ER)id;
                    name = $"{enumObj}";
                }
                else if (IsSellRegionContainer(info))
                {
                    var enumObj = (SellRegion_MsgBndID_ER)id;
                    name = $"{enumObj}";
                }
                break;
            case ProjectType.AC6:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_AC6)id;
                    name = $"{enumObj}";
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_AC6)id;
                    name = $"{enumObj}";
                }
                else if (IsNgWordContainer(info))
                {
                    var enumObj = (NgWord_MsgBndID_AC6)id;
                    name = $"{enumObj}";
                }
                break;

            default: break;
        }

        return name;
    }

    /// <summary>
    /// Get the internal name for a FMG based on the BND ID
    /// </summary>
    public static int GetFmgInternalId(TextContainerInfo info, int id)
    {
        int retId = -1;

        switch (Smithbox.ProjectType)
        {
            case ProjectType.DES:
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_DS1)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_DS1)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                if (IsTalkFmg(info))
                {
                    var enumObj = (TalkFmgName_DS2)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsBloodMessageFmg(info))
                {
                    var enumObj = (BloodMessageFmgName_DS2)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else
                {
                    var enumObj = (CommonFmgName_DS2)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                break;
            case ProjectType.BB:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_BB)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_BB)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                break;
            case ProjectType.DS3:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_DS3)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_DS3)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsNgWordContainer(info))
                {
                    var enumObj = (NgWord_MsgBndID_DS3)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                break;
            case ProjectType.SDT:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_SDT)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_SDT)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsSellRegionContainer(info))
                {
                    var enumObj = (SellRegion_MsgBndID_SDT)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                break;
            case ProjectType.ER:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_ER)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_ER)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsNgWordContainer(info))
                {
                    var enumObj = (NgWord_MsgBndID_ER)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsSellRegionContainer(info))
                {
                    var enumObj = (SellRegion_MsgBndID_ER)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                break;
            case ProjectType.AC6:
                if (IsItemContainer(info))
                {
                    var enumObj = (Item_MsgBndID_AC6)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsMenuContainer(info))
                {
                    var enumObj = (Menu_MsgBndID_AC6)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                else if (IsNgWordContainer(info))
                {
                    var enumObj = (NgWord_MsgBndID_AC6)id;
                    retId = Convert.ToInt32($"{enumObj}");
                }
                break;

            default: break;
        }

        return retId;
    }

    /// <summary>
    /// Check if the current project type uses FMG patching
    /// </summary>
    public static bool UsesFmgPatching()
    {
        if(Smithbox.ProjectType 
            is ProjectType.DS1 
            or ProjectType.DS1R 
            or ProjectType.DS3 
            or ProjectType.ER)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if FMG container is an item FMG container
    /// </summary>
    public static bool IsItemContainer(TextContainerInfo info)
    {
        if (info.AbsolutePath.Contains("item"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if FMG container is an menu FMG container
    /// </summary>
    public static bool IsMenuContainer(TextContainerInfo info)
    {
        if (info.AbsolutePath.Contains("menu"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Check if FMG container is an ngword FMG container
    /// </summary>
    public static bool IsNgWordContainer(TextContainerInfo info)
    {
        if (info.AbsolutePath.Contains("ngword"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// BB onwards: check if FMG container is a sellregion 
    /// </summary>
    public static bool IsSellRegionContainer(TextContainerInfo info)
    {
        if (info.AbsolutePath.Contains("sellregion"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// DS2 only: check if FMG is part of the bloodmes folder
    /// </summary>
    public static bool IsBloodMessageFmg(TextContainerInfo info)
    {
        if (info.AbsolutePath.Contains("bloodmes"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// DS2 only: check if FMG is part of the ta;l folder
    /// </summary>
    public static bool IsTalkFmg(TextContainerInfo info)
    {
        if (info.AbsolutePath.Contains("talk"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determine the container category based on the container filepath.
    /// Used to determine the 'language' grouping each container belongs to.
    /// </summary>
    public static TextContainerCategory GetLanguageCategory(string path)
    {
        var group = TextContainerCategory.None;

        // Common
        if (path.Contains("common"))
        {
            group = TextContainerCategory.Common;
        }

        // English (US)
        if (path.Contains("ENGLISH") ||
            path.Contains("english") ||
            path.Contains("engus"))
        {
            group = TextContainerCategory.English;
        }

        // English (UK)
        if (path.Contains("enggb"))
        {
            group = TextContainerCategory.EnglishUK;
        }

        // French
        if (path.Contains("FRENCH") ||
            path.Contains("french") ||
            path.Contains("frafr"))
        {
            group = TextContainerCategory.French;
        }

        // German
        if (path.Contains("GERMAN") ||
            path.Contains("germany") ||
            path.Contains("deude"))
        {
            group = TextContainerCategory.German;
        }

        // Italian
        if (path.Contains("ITALIAN") ||
            path.Contains("italian") ||
            path.Contains("itait"))
        {
            group = TextContainerCategory.Italian;
        }

        // Japanese
        if (path.Contains("JAPANESE") ||
            path.Contains("japanese") ||
            path.Contains("jpnjp"))
        {
            group = TextContainerCategory.Japanese;
        }

        // Korean
        if (path.Contains("KOREAN") ||
            path.Contains("korean") ||
            path.Contains("korkr"))
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
            path.Contains("spaes"))
        {
            group = TextContainerCategory.Spanish;
        }

        // Spanish (Neutral)
        if (path.Contains("neutralspanish"))
        {
            group = TextContainerCategory.SpanishNeutral;
        }

        // Spanish (Latin)
        if (path.Contains("spaar"))
        {
            group = TextContainerCategory.SpanishLatin;
        }

        // Traditional Chinese
        if (path.Contains("TCHINESE") ||
            path.Contains("chinese") ||
            path.Contains("zhotw"))
        {
            group = TextContainerCategory.TraditionalChinese;
        }

        // Simplified Chinese
        if (path.Contains("zhocn"))
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
        if (path.Contains("portuguese") ||
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
    /// Get a pretty name for the container row name
    /// </summary>
    public static string GetPrettyContainerName(string name)
    {
        var prettyName = name;

        if (name.Contains("item"))
            prettyName = "Item";

        if (name.Contains("menu"))
            prettyName = "Menu";

        if (name.Contains("sellregion"))
            prettyName = "Sell Region";

        if (name.Contains("ngword"))
            prettyName = "Blocked Words";

        if (name.Contains("dlc01") || name.Contains("dlc1"))
            prettyName = $"{prettyName} - DLC 1";

        if (name.Contains("dlc02") || name.Contains("dlc2"))
            prettyName = $"{prettyName} - DLC 2";

        return prettyName;
    }

    /// <summary>
    /// These helper functions return the associated partner FMGs 
    /// (based on Title as the source FMG) so we can display 
    /// Title / Description / Summary / Extra Text together within the editing panel
    /// </summary>

    /// <summary>
    /// Get the associated summary text
    /// </summary>
    public static int GetAssociatedSummaryID(FmgInfo info)
    {
        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            // Goods
            if (info.ID == 10)
            {
                return 20;
            }
            // Weapons
            if (info.ID == 11)
            {
                return 21;
            }
            // Armor
            if (info.ID == 12)
            {
                return 22;
            }
            // Accessories
            if (info.ID == 13)
            {
                return 23;
            }
            // Magic
            if (info.ID == 14)
            {
                return 28;
            }
        }

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {

        }

        if (Smithbox.ProjectType is ProjectType.DS3)
        {

        }

        if (Smithbox.ProjectType is ProjectType.BB)
        {

        }

        if (Smithbox.ProjectType is ProjectType.SDT)
        {

        }

        if (Smithbox.ProjectType is ProjectType.ER)
        {

        }

        if (Smithbox.ProjectType is ProjectType.AC6)
        {

        }

        return -1;
    }

    /// <summary>
    /// Get the associated description text
    /// </summary>
    public static int GetAssociatedDescriptionID(FmgInfo info)
    {
        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            // Goods
            if (info.ID == 10)
            {
                return 24;
            }
            // Weapons
            if (info.ID == 11)
            {
                return 25;
            }
            // Armor
            if (info.ID == 12)
            {
                return 26;
            }
            // Accessories
            if (info.ID == 13)
            {
                return 27;
            }
            // Magic
            if (info.ID == 14)
            {
                return 29;
            }
        }

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {

        }

        if (Smithbox.ProjectType is ProjectType.DS3)
        {

        }

        if (Smithbox.ProjectType is ProjectType.BB)
        {

        }

        if (Smithbox.ProjectType is ProjectType.SDT)
        {

        }

        if (Smithbox.ProjectType is ProjectType.ER)
        {

        }

        if (Smithbox.ProjectType is ProjectType.AC6)
        {

        }

        return -1;
    }

    /// <summary>
    /// Get the associated effect text
    /// </summary>
    public static int GetAssociatedEffectTextID(FmgInfo info)
    {
        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {

        }

        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {

        }

        if (Smithbox.ProjectType is ProjectType.DS3)
        {

        }

        if (Smithbox.ProjectType is ProjectType.BB)
        {

        }

        if (Smithbox.ProjectType is ProjectType.SDT)
        {

        }

        if (Smithbox.ProjectType is ProjectType.ER)
        {

        }

        if (Smithbox.ProjectType is ProjectType.AC6)
        {

        }

        return -1;
    }
}
