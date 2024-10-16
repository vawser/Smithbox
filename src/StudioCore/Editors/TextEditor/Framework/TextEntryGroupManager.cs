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
public class TextEntryGroupManager
{
    public TextEditorScreen Screen;
    public TextSelectionManager Selection;

    public TextEntryGroupManager(TextEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
    }

    public FmgEntryGroup GetEntryGroup(FMG.Entry entry)
    {
        return new FmgEntryGroup(this, Selection.SelectedContainer, Selection.SelectedFmgInfo, entry);
    }

    /// <summary>
    /// Build association groupings.
    /// Note: the row ID needs to match between each FMG for this to work correctly.
    /// </summary>
    /// <returns></returns>
    public List<EntryGroupAssociation> GetGroupings()
    {
        List<EntryGroupAssociation> groupings = new();

        switch (Smithbox.ProjectType)
        {

            case ProjectType.DES:
                // Vanilla
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DES.Title_Goods,
                    Item_MsgBndID_DES.Summary_Goods,
                    Item_MsgBndID_DES.Description_Goods,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DES.Title_Weapons,
                    Item_MsgBndID_DES.Summary_Weapons,
                    Item_MsgBndID_DES.Description_Weapons,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DES.Title_Armor,
                    Item_MsgBndID_DES.Summary_Armor,
                    Item_MsgBndID_DES.Description_Armor,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DES.Title_Accessories,
                    Item_MsgBndID_DES.Summary_Accessories,
                    Item_MsgBndID_DES.Description_Accessories,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DES.Title_Magic,
                    Item_MsgBndID_DES.Summary_Magic,
                    Item_MsgBndID_DES.Description_Magic,
                    null));
                break;

            case ProjectType.DS1:
            case ProjectType.DS1R:
                // Vanilla
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS1.Title_Goods,
                    Item_MsgBndID_DS1.Summary_Goods,
                    Item_MsgBndID_DS1.Description_Goods,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS1.Title_Weapons,
                    Item_MsgBndID_DS1.Summary_Weapons,
                    Item_MsgBndID_DS1.Description_Weapons,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS1.Title_Armor,
                    Item_MsgBndID_DS1.Summary_Armor,
                    Item_MsgBndID_DS1.Description_Armor,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS1.Title_Accessories,
                    Item_MsgBndID_DS1.Summary_Accessories,
                    Item_MsgBndID_DS1.Description_Accessories,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS1.Title_Magic,
                    Item_MsgBndID_DS1.Summary_Magic,
                    Item_MsgBndID_DS1.Description_Magic,
                    null));
                break;

            case ProjectType.DS2:
            case ProjectType.DS2S:
                groupings.Add(new EntryGroupAssociation(
                    CommonFmgName_DS2.itemname,
                    CommonFmgName_DS2.simpleexplanation,
                    CommonFmgName_DS2.detailedexplanation,
                    null));
                break;

            case ProjectType.BB:
                // Vanilla
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_BB.Title_Goods,
                    Item_MsgBndID_BB.Summary_Goods,
                    Item_MsgBndID_BB.Description_Goods,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_BB.Title_Weapons,
                    Item_MsgBndID_BB.Summary_Weapons,
                    Item_MsgBndID_BB.Description_Weapons,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_BB.Title_Armor,
                    Item_MsgBndID_BB.Summary_Armor,
                    Item_MsgBndID_BB.Description_Armor,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_BB.Title_Accessories,
                    Item_MsgBndID_BB.Summary_Accessories,
                    Item_MsgBndID_BB.Description_Accessories,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_BB.Title_Magic,
                    Item_MsgBndID_BB.Summary_Magic,
                    Item_MsgBndID_BB.Description_Magic,
                    null));
                break;

            case ProjectType.DS3:
                // Vanilla
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Goods,
                    Item_MsgBndID_DS3.Summary_Goods,
                    Item_MsgBndID_DS3.Description_Goods,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Weapons,
                    Item_MsgBndID_DS3.Summary_Weapons,
                    Item_MsgBndID_DS3.Description_Weapons,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Armor,
                    Item_MsgBndID_DS3.Summary_Armor,
                    Item_MsgBndID_DS3.Description_Armor,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Accessories,
                    Item_MsgBndID_DS3.Summary_Accessories,
                    Item_MsgBndID_DS3.Description_Accessories,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Magic,
                    Item_MsgBndID_DS3.Summary_Magic,
                    Item_MsgBndID_DS3.Description_Magic,
                    null));

                // DLC1
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Goods_DLC1,
                    Item_MsgBndID_DS3.Summary_Goods_DLC1,
                    Item_MsgBndID_DS3.Description_Goods_DLC1,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Weapons_DLC1,
                    null,
                    Item_MsgBndID_DS3.Description_Weapons_DLC1,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Armor_DLC1,
                    null,
                    Item_MsgBndID_DS3.Description_Armor_DLC1,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Accessories_DLC1,
                    Item_MsgBndID_DS3.Summary_Accessories_DLC1,
                    Item_MsgBndID_DS3.Description_Accessories_DLC1,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Magic_DLC1,
                    Item_MsgBndID_DS3.Summary_Magic_DLC1,
                    Item_MsgBndID_DS3.Description_Magic_DLC1,
                    null));

                // DLC2
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Goods_DLC2,
                    Item_MsgBndID_DS3.Summary_Goods_DLC2,
                    Item_MsgBndID_DS3.Description_Goods_DLC2,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Weapons_DLC2,
                    null,
                    Item_MsgBndID_DS3.Description_Weapons_DLC2,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Armor_DLC2,
                    null,
                    Item_MsgBndID_DS3.Description_Armor_DLC2,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Accessories_DLC2,
                    Item_MsgBndID_DS3.Summary_Accessories_DLC2,
                    Item_MsgBndID_DS3.Description_Accessories_DLC2,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_DS3.Title_Magic_DLC2,
                    Item_MsgBndID_DS3.Summary_Magic_DLC2,
                    Item_MsgBndID_DS3.Description_Magic_DLC2,
                    null));
                break;

            case ProjectType.SDT:
                // Vanilla
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_SDT.Title_Goods,
                    Item_MsgBndID_SDT.Summary_Goods,
                    Item_MsgBndID_SDT.Description_Goods,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_SDT.Title_Weapons,
                    Item_MsgBndID_SDT.Summary_Weapons,
                    Item_MsgBndID_SDT.Description_Weapons,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_SDT.Title_Armor,
                    Item_MsgBndID_SDT.Summary_Armor,
                    Item_MsgBndID_SDT.Description_Armor,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_SDT.Title_Accessories,
                    Item_MsgBndID_SDT.Summary_Accessories,
                    Item_MsgBndID_SDT.Description_Accessories,
                    null));
                break;

            case ProjectType.ER:
                // Vanilla
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Goods,
                    Item_MsgBndID_ER.Summary_Goods,
                    Item_MsgBndID_ER.Description_Goods,
                    Item_MsgBndID_ER.Effect_Goods));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Weapons,
                    Item_MsgBndID_ER.Summary_Weapons,
                    Item_MsgBndID_ER.Description_Weapons,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Armor,
                    Item_MsgBndID_ER.Summary_Armor,
                    Item_MsgBndID_ER.Description_Armor,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Accessories,
                    Item_MsgBndID_ER.Summary_Accessories,
                    Item_MsgBndID_ER.Description_Accessories,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Magic,
                    Item_MsgBndID_ER.Summary_Magic,
                    Item_MsgBndID_ER.Description_Magic,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Ash_of_War,
                    Item_MsgBndID_ER.Summary_Ash_of_War,
                    Item_MsgBndID_ER.Description_Ash_of_War,
                    Item_MsgBndID_ER.Effect_Ash_of_War));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Skill,
                    null,
                    Item_MsgBndID_ER.Description_Skill,
                    null));

                // DLC 1
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Goods_DLC1,
                    Item_MsgBndID_ER.Summary_Goods_DLC1,
                    Item_MsgBndID_ER.Description_Goods_DLC1,
                    Item_MsgBndID_ER.Effect_Goods_DLC1));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Weapons_DLC1,
                    Item_MsgBndID_ER.Summary_Weapons_DLC1,
                    Item_MsgBndID_ER.Description_Weapons_DLC1,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Armor_DLC1,
                    Item_MsgBndID_ER.Summary_Armor_DLC1,
                    Item_MsgBndID_ER.Description_Armor_DLC1,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Accessories_DLC1,
                    Item_MsgBndID_ER.Summary_Accessories_DLC1,
                    Item_MsgBndID_ER.Description_Accessories_DLC1,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Magic_DLC1,
                    Item_MsgBndID_ER.Summary_Magic_DLC1,
                    Item_MsgBndID_ER.Description_Magic_DLC1,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Ash_of_War_DLC1,
                    Item_MsgBndID_ER.Summary_Ash_of_War_DLC1,
                    Item_MsgBndID_ER.Description_Ash_of_War_DLC1,
                    Item_MsgBndID_ER.Effect_Ash_of_War_DLC1));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Skill_DLC1,
                    null,
                    Item_MsgBndID_ER.Description_Skill_DLC1,
                    null));

                // DLC 2
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Goods_DLC2,
                    Item_MsgBndID_ER.Summary_Goods_DLC2,
                    Item_MsgBndID_ER.Description_Goods_DLC2,
                    Item_MsgBndID_ER.Effect_Goods_DLC2));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Weapons_DLC2,
                    Item_MsgBndID_ER.Summary_Weapons_DLC2,
                    Item_MsgBndID_ER.Description_Weapons_DLC2,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Armor_DLC2,
                    Item_MsgBndID_ER.Summary_Armor_DLC2,
                    Item_MsgBndID_ER.Description_Armor_DLC2,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Accessories_DLC2,
                    Item_MsgBndID_ER.Summary_Accessories_DLC2,
                    Item_MsgBndID_ER.Description_Accessories_DLC2,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Magic_DLC2,
                    Item_MsgBndID_ER.Summary_Magic_DLC2,
                    Item_MsgBndID_ER.Description_Magic_DLC2,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Ash_of_War_DLC2,
                    Item_MsgBndID_ER.Summary_Ash_of_War_DLC2,
                    Item_MsgBndID_ER.Description_Ash_of_War_DLC2,
                    Item_MsgBndID_ER.Effect_Ash_of_War_DLC2));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_ER.Title_Skill_DLC2,
                    null,
                    Item_MsgBndID_ER.Description_Skill_DLC2,
                    null));

                break;

            case ProjectType.AC6:
                // Vanilla
                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_AC6.Title_Weapons,
                    null,
                    Item_MsgBndID_AC6.Description_Weapons,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_AC6.Title_Armor,
                    null,
                    Item_MsgBndID_AC6.Description_Armor,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_AC6.Title_Generator,
                    null,
                    Item_MsgBndID_AC6.Description_Generator,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_AC6.Title_Booster,
                    null,
                    Item_MsgBndID_AC6.Description_Booster,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Item_MsgBndID_AC6.Title_FCS,
                    null,
                    Item_MsgBndID_AC6.Description_FCS,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Menu_MsgBndID_AC6.Archive_Name,
                    null,
                    Menu_MsgBndID_AC6.Archive_Content,
                    null));

                groupings.Add(new EntryGroupAssociation(
                    Menu_MsgBndID_AC6.Mission_Name,
                    null,
                    Menu_MsgBndID_AC6.Mission_Overview,
                    null));
                break;
        }

        return groupings;
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

    public FmgEntryGroup(TextEntryGroupManager entryManager, 
        TextContainerInfo containerInfo,
        FmgInfo selectedFmgInfo,
        FMG.Entry baseEntry)
    {
        var targetBinderID = selectedFmgInfo.ID;

        var associationGroup = entryManager.GetGroupings()
            .Where(e => 
            e.GetTitleEnumID() == targetBinderID || 
            e.GetSummaryEnumID() == targetBinderID ||
            e.GetDescriptionEnumID() == targetBinderID ||
            e.GetEffectEnumID() == targetBinderID)
            .FirstOrDefault();

        if(associationGroup != null)
        {
            SupportsGrouping = true;

            Title = SetGroupEntry(containerInfo, 
                associationGroup.GetTitleEnumID(), 
                baseEntry);

            Summary = SetGroupEntry(containerInfo, 
                associationGroup.GetSummaryEnumID(), 
                baseEntry);

            Description = SetGroupEntry(containerInfo, 
                associationGroup.GetDescriptionEnumID(), 
                baseEntry);

            Effect = SetGroupEntry(containerInfo, 
                associationGroup.GetEffectEnumID(), 
                baseEntry);
        }
    }

    public FMG.Entry SetGroupEntry(TextContainerInfo containerInfo, int targetBndId, FMG.Entry baseEntry)
    {
        foreach (var fmg in containerInfo.FmgInfos)
        {
            if (fmg.ID == targetBndId)
            {
                foreach (var entry in fmg.File.Entries)
                {
                    if (entry.ID == baseEntry.ID)
                    {
                        return entry;
                    }
                }
            }
        }

        return null;
    }
}
