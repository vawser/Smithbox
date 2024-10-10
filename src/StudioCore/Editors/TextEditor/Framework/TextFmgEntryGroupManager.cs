using HKLib.hk2018.hkAsyncThreadPool;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Manager for the FMG Entry Groups and the setup of the BND ID groupings
/// </summary>
public class TextFmgEntryGroupManager
{
    public TextEditorScreen Screen;
    public TextSelectionManager Selection;
    public static List<EntryGroupAssociation> Groupings;

    public TextFmgEntryGroupManager(TextEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;

        Groupings = new();

        switch (Smithbox.ProjectType)
        {
            case ProjectType.ER:
                // Vanilla
                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Goods,
                    Item_MsgBndID_ER.Summary_Goods,
                    Item_MsgBndID_ER.Description_Goods,
                    Item_MsgBndID_ER.Effect_Goods));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Weapons,
                    Item_MsgBndID_ER.Summary_Weapons,
                    Item_MsgBndID_ER.Description_Weapons,
                    Item_MsgBndID_ER.Effect_Weapons));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Armor,
                    Item_MsgBndID_ER.Summary_Armor,
                    Item_MsgBndID_ER.Description_Armor,
                    null));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Accessories,
                    Item_MsgBndID_ER.Summary_Accessories,
                    Item_MsgBndID_ER.Description_Accessories,
                    null));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Magic,
                    Item_MsgBndID_ER.Summary_Magic,
                    Item_MsgBndID_ER.Description_Magic,
                    null));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Ash_of_War,
                    Item_MsgBndID_ER.Summary_Ash_of_War,
                    Item_MsgBndID_ER.Description_Ash_of_War,
                    Item_MsgBndID_ER.Effect_Ash_of_War));

                // DLC 1
                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Goods_DLC1,
                    Item_MsgBndID_ER.Summary_Goods_DLC1,
                    Item_MsgBndID_ER.Description_Goods_DLC1,
                    Item_MsgBndID_ER.Effect_Goods_DLC1));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Weapons_DLC1,
                    Item_MsgBndID_ER.Summary_Weapons_DLC1,
                    Item_MsgBndID_ER.Description_Weapons_DLC1,
                    Item_MsgBndID_ER.Effect_Weapons_DLC1));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Armor_DLC1,
                    Item_MsgBndID_ER.Summary_Armor_DLC1,
                    Item_MsgBndID_ER.Description_Armor_DLC1,
                    null));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Accessories_DLC1,
                    Item_MsgBndID_ER.Summary_Accessories_DLC1,
                    Item_MsgBndID_ER.Description_Accessories_DLC1,
                    null));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Magic_DLC1,
                    Item_MsgBndID_ER.Summary_Magic_DLC1,
                    Item_MsgBndID_ER.Description_Magic_DLC1,
                    null));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Ash_of_War_DLC1,
                    Item_MsgBndID_ER.Summary_Ash_of_War_DLC1,
                    Item_MsgBndID_ER.Description_Ash_of_War_DLC1,
                    Item_MsgBndID_ER.Effect_Ash_of_War_DLC1));

                // DLC 2
                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Goods_DLC2,
                    Item_MsgBndID_ER.Summary_Goods_DLC2,
                    Item_MsgBndID_ER.Description_Goods_DLC2,
                    Item_MsgBndID_ER.Effect_Goods_DLC2));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Weapons_DLC2,
                    Item_MsgBndID_ER.Summary_Weapons_DLC2,
                    Item_MsgBndID_ER.Description_Weapons_DLC2,
                    Item_MsgBndID_ER.Effect_Weapons_DLC2));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Armor_DLC2,
                    Item_MsgBndID_ER.Summary_Armor_DLC2,
                    Item_MsgBndID_ER.Description_Armor_DLC2,
                    null));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Accessories_DLC2,
                    Item_MsgBndID_ER.Summary_Accessories_DLC2,
                    Item_MsgBndID_ER.Description_Accessories_DLC2,
                    null));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Magic_DLC2,
                    Item_MsgBndID_ER.Summary_Magic_DLC2,
                    Item_MsgBndID_ER.Description_Magic_DLC2,
                    null));

                Groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Ash_of_War_DLC2,
                    Item_MsgBndID_ER.Summary_Ash_of_War_DLC2,
                    Item_MsgBndID_ER.Description_Ash_of_War_DLC2,
                    Item_MsgBndID_ER.Effect_Ash_of_War_DLC2));
                break;
        }
    }

    public FmgEntryGroup GetEntryGroup(FMG.Entry baseEntry)
    {
        return FmgEntryGroupCache.GetFmgEntryGroup(Selection, Selection._selectedFmgEntry);
    }
}

/// <summary>
/// Represents a BND ID grouping, e.g. which BND IDs should be a Title, Summary, etc
/// </summary>
public class EntryGroupAssociation
{
    public Enum Title { get; set; }
    public Enum Summary { get; set; }
    public Enum Description { get; set; }
    public Enum Effect { get; set; }

    public EntryGroupAssociation(Enum titleEnum, Enum summaryEnum, Enum descriptionEnum, Enum effectEnum)
    {
        Title = titleEnum;
        Summary = summaryEnum;
        Description = descriptionEnum;
        Effect = effectEnum;
    }

    public string GetTitleEnumName()
    {
        return $"{Title}";
    }
    public int GetTitleEnumID()
    {
        return Convert.ToInt32(Title);
    }

    public string GetSummaryEnumName()
    {
        return $"{Summary}";
    }
    public int GetSummaryEnumID()
    {
        return Convert.ToInt32(Summary);
    }

    public string GetDescriptionEnumName()
    {
        return $"{Description}";
    }
    public int GetDescriptionEnumID()
    {
        return Convert.ToInt32(Description);
    }

    public string GetEffectEnumName()
    {
        return $"{Effect}";
    }
    public int GetEffectEnumID()
    {
        return Convert.ToInt32(Effect);
    }
}

/// <summary>
/// Represents a FMG Entry Group, which is an arbitary grouping of FMG Entries 
/// based on a Entry Group Association that the currently selected FMG Entry belongs to.
/// </summary>
public class FmgEntryGroup
{
    public FMG.Entry Title { get; set; }
    public FMG.Entry Summary { get; set; }
    public FMG.Entry Description { get; set; }
    public FMG.Entry Effect { get; set; }

    /// <summary>
    /// If this is false, fallback to Simple Editor in Group Editor mode
    /// </summary>
    public bool SupportsGrouping = false;

    private TextContainerInfo CurrentContainer;

    public FmgEntryGroup(TextContainerInfo containerInfo, FMG.Entry baseEntry)
    {
        CurrentContainer = containerInfo;

        var id = baseEntry.ID;

        foreach(var entry in TextFmgEntryGroupManager.Groupings)
        {
            var titleId = entry.GetTitleEnumID();
            if (titleId == id)
            {

            }
        }
    }
}

/// <summary>
/// A cache for already created FMG Entry Groups.
/// Invalidated when the selected File Container changes.
/// </summary>
public static class FmgEntryGroupCache
{
    public static Dictionary<string, FmgEntryGroup> Cache = new();

    public static void ClearCache()
    {
        Cache.Clear();
    }

    public static FmgEntryGroup GetFmgEntryGroup(TextSelectionManager selection, FMG.Entry entry)
    {
        var fmgKey = selection.SelectedFmgKey;
        var fmgEntryKey = selection._selectedFmgEntryIndex;

        var cacheKey = $"fmg{fmgKey}_fmgEntry{fmgEntryKey}";

        if (Cache.ContainsKey(cacheKey))
        {
            return Cache[cacheKey];
        }
        else
        {
            var groupedEntries = new FmgEntryGroup(selection.SelectedContainer, selection._selectedFmgEntry);

            Cache.Add(cacheKey, groupedEntries);

            return groupedEntries;
        }
    }
}