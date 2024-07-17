using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public static class FMGUtils
{
    /// <summary>
    ///     Get category for grouped entries (Goods, Weapons, etc)
    /// </summary>
    public static FmgEntryCategory GetFmgCategory(int id)
    {
        switch ((FmgIDType)id)
        {
            case FmgIDType.TitleTest:
            case FmgIDType.TitleTest2:
            case FmgIDType.TitleTest3:
            case FmgIDType.ERUnk45:
            case FmgIDType.ERUnk45_SOTE:
            case FmgIDType.ERUnk45_SOTE_DLC2:
                return FmgEntryCategory.None;

            case FmgIDType.TitleGoods:
            case FmgIDType.TitleGoods_Patch:
            case FmgIDType.TitleGoods_DLC2:
            case FmgIDType.TitleGoods_SOTE:
            case FmgIDType.TitleGoods_SOTE_DLC2:
            case FmgIDType.DescriptionGoods:
            case FmgIDType.DescriptionGoods_Patch:
            case FmgIDType.DescriptionGoods_DLC1:
            case FmgIDType.DescriptionGoods_DLC2:
            case FmgIDType.DescriptionGoods_SOTE:
            case FmgIDType.DescriptionGoods_SOTE_DLC2:
            case FmgIDType.SummaryGoods:
            case FmgIDType.SummaryGoods_Patch:
            case FmgIDType.SummaryGoods_DLC1:
            case FmgIDType.SummaryGoods_DLC2:
            case FmgIDType.SummaryGoods_SOTE:
            case FmgIDType.SummaryGoods_SOTE_DLC2:
            case FmgIDType.GoodsInfo2:
                return FmgEntryCategory.Goods;

            case FmgIDType.TitleWeapons:
            case FmgIDType.TitleWeapons_Patch:
            case FmgIDType.TitleWeapons_DLC1:
            case FmgIDType.TitleWeapons_DLC2:
            case FmgIDType.TitleWeapons_SOTE:
            case FmgIDType.TitleWeapons_SOTE_DLC2:
            case FmgIDType.DescriptionWeapons:
            case FmgIDType.DescriptionWeapons_Patch:
            case FmgIDType.DescriptionWeapons_DLC1:
            case FmgIDType.DescriptionWeapons_DLC2:
            case FmgIDType.DescriptionWeapons_SOTE:
            case FmgIDType.DescriptionWeapons_SOTE_DLC2:
            case FmgIDType.SummaryWeapons:
            case FmgIDType.SummaryWeapons_Patch:
            case FmgIDType.SummaryWeapons_SOTE:
            case FmgIDType.SummaryWeapons_SOTE_DLC2:
            case FmgIDType.WeaponEffect:
            case FmgIDType.WeaponEffect_SOTE:
            case FmgIDType.WeaponEffect_SOTE_DLC2:
                return FmgEntryCategory.Weapons;

            case FmgIDType.TitleArmor:
            case FmgIDType.TitleArmor_Patch:
            case FmgIDType.TitleArmor_DLC1:
            case FmgIDType.TitleArmor_DLC2:
            case FmgIDType.TitleArmor_SOTE:
            case FmgIDType.TitleArmor_SOTE_DLC2:
            case FmgIDType.DescriptionArmor:
            case FmgIDType.DescriptionArmor_Patch:
            case FmgIDType.DescriptionArmor_DLC1:
            case FmgIDType.DescriptionArmor_DLC2:
            case FmgIDType.DescriptionArmor_SOTE:
            case FmgIDType.DescriptionArmor_SOTE_DLC2:
            case FmgIDType.SummaryArmor:
            case FmgIDType.SummaryArmor_Patch:
            case FmgIDType.SummaryArmor_SOTE:
            case FmgIDType.SummaryArmor_SOTE_DLC2:
                return FmgEntryCategory.Armor;

            case FmgIDType.TitleRings:
            case FmgIDType.TitleRings_Patch:
            case FmgIDType.TitleRings_DLC1:
            case FmgIDType.TitleRings_DLC2:
            case FmgIDType.TitleRings_SOTE:
            case FmgIDType.TitleRings_SOTE_DLC2:
            case FmgIDType.DescriptionRings:
            case FmgIDType.DescriptionRings_Patch:
            case FmgIDType.DescriptionRings_DLC1:
            case FmgIDType.DescriptionRings_DLC2:
            case FmgIDType.DescriptionRings_SOTE:
            case FmgIDType.DescriptionRings_SOTE_DLC2:
            case FmgIDType.SummaryRings:
            case FmgIDType.SummaryRings_DLC1:
            case FmgIDType.SummaryRings_DLC2:
            case FmgIDType.SummaryRings_Patch:
            case FmgIDType.SummaryRings_SOTE:
            case FmgIDType.SummaryRings_SOTE_DLC2:
                return FmgEntryCategory.Rings;

            case FmgIDType.TitleSpells:
            case FmgIDType.TitleSpells_Patch:
            case FmgIDType.TitleSpells_DLC1:
            case FmgIDType.TitleSpells_DLC2:
            case FmgIDType.TitleSpells_SOTE:
            case FmgIDType.TitleSpells_SOTE_DLC2:
            case FmgIDType.DescriptionSpells:
            case FmgIDType.DescriptionSpells_Patch:
            case FmgIDType.DescriptionSpells_DLC1:
            case FmgIDType.DescriptionSpells_DLC2:
            case FmgIDType.DescriptionSpells_SOTE:
            case FmgIDType.DescriptionSpells_SOTE_DLC2:
            case FmgIDType.SummarySpells:
            case FmgIDType.SummarySpells_DLC1:
            case FmgIDType.SummarySpells_DLC2:
            case FmgIDType.SummarySpells_SOTE:
            case FmgIDType.SummarySpells_SOTE_DLC2:
                return FmgEntryCategory.Spells;

            case FmgIDType.TitleCharacters:
            case FmgIDType.TitleCharacters_Patch:
            case FmgIDType.TitleCharacters_DLC1:
            case FmgIDType.TitleCharacters_DLC2:
            case FmgIDType.TitleCharacters_SOTE:
            case FmgIDType.TitleCharacters_SOTE_DLC2:
                return FmgEntryCategory.Characters;

            case FmgIDType.TitleLocations:
            case FmgIDType.TitleLocations_Patch:
            case FmgIDType.TitleLocations_DLC1:
            case FmgIDType.TitleLocations_DLC2:
            case FmgIDType.TitleLocations_SOTE:
            case FmgIDType.TitleLocations_SOTE_DLC2:
                return FmgEntryCategory.Locations;

            case FmgIDType.TitleGem_SOTE:
            case FmgIDType.TitleGem_SOTE_DLC2:
            case FmgIDType.DescriptionGem:
            case FmgIDType.DescriptionGem_SOTE:
            case FmgIDType.DescriptionGem_SOTE_DLC2:
            case FmgIDType.SummaryGem_SOTE:
            case FmgIDType.SummaryGem_SOTE_DLC2:
                return FmgEntryCategory.Gem;

            case FmgIDType.TitleSwordArts_SOTE:
            case FmgIDType.TitleSwordArts_SOTE_DLC2:
            case FmgIDType.SummarySwordArts:
            case FmgIDType.SummarySwordArts_SOTE:
            case FmgIDType.SummarySwordArts_SOTE_DLC2:
                return FmgEntryCategory.SwordArts;

            case FmgIDType.TutorialTitle:
            case FmgIDType.TutorialTitle2023:
            case FmgIDType.TutorialTitle_SOTE:
            case FmgIDType.TutorialTitle_SOTE_DLC2:
            case FmgIDType.TutorialBody:
            case FmgIDType.TutorialBody2023:
            case FmgIDType.TutorialBody_SOTE:
            case FmgIDType.TutorialBody_SOTE_DLC2:
                return FmgEntryCategory.Tutorial;

            //return FmgEntryCategory.ItemFmgDummy;

            case FmgIDType.TitleMission:
            case FmgIDType.SummaryMission:
            case FmgIDType.DescriptionMission:
            case FmgIDType.MissionLocation:
                return FmgEntryCategory.Mission;

            case FmgIDType.TitleBooster:
            case FmgIDType.DescriptionBooster:
                return FmgEntryCategory.Booster;

            case FmgIDType.TitleArchive:
            case FmgIDType.DescriptionArchive:
                return FmgEntryCategory.Archive;

            default:
                return FmgEntryCategory.None;
        }
    }


    /// <summary>
    ///     Get entry text type (such as weapon Title, Summary, Description)
    /// </summary>
    public static FmgEntryTextType GetFmgTextType(int id)
    {
        switch ((FmgIDType)id)
        {
            case FmgIDType.DescriptionGoods:
            case FmgIDType.DescriptionGoods_DLC1:
            case FmgIDType.DescriptionGoods_DLC2:
            case FmgIDType.DescriptionWeapons:
            case FmgIDType.DescriptionWeapons_DLC1:
            case FmgIDType.DescriptionWeapons_DLC2:
            case FmgIDType.DescriptionArmor:
            case FmgIDType.DescriptionArmor_DLC1:
            case FmgIDType.DescriptionArmor_DLC2:
            case FmgIDType.DescriptionRings:
            case FmgIDType.DescriptionRings_DLC1:
            case FmgIDType.DescriptionRings_DLC2:
            case FmgIDType.DescriptionSpells:
            case FmgIDType.DescriptionSpells_DLC1:
            case FmgIDType.DescriptionSpells_DLC2:
            case FmgIDType.DescriptionArmor_Patch:
            case FmgIDType.DescriptionGoods_Patch:
            case FmgIDType.DescriptionRings_Patch:
            case FmgIDType.DescriptionSpells_Patch:
            case FmgIDType.DescriptionWeapons_Patch:
            case FmgIDType.DescriptionGem:
            case FmgIDType.SummarySwordArts: // Include as Description (for text box size)
            case FmgIDType.DescriptionBooster:
            case FmgIDType.DescriptionMission:
            case FmgIDType.DescriptionArchive:
            case FmgIDType.DescriptionWeapons_SOTE:
            case FmgIDType.DescriptionArmor_SOTE:
            case FmgIDType.DescriptionGoods_SOTE:
            case FmgIDType.DescriptionRings_SOTE:
            case FmgIDType.DescriptionGem_SOTE:
            case FmgIDType.DescriptionSpells_SOTE:
            case FmgIDType.SummarySwordArts_SOTE:
                return FmgEntryTextType.Description;

            case FmgIDType.SummaryGoods:
            case FmgIDType.SummaryGoods_DLC1:
            case FmgIDType.SummaryGoods_DLC2:
            case FmgIDType.SummaryWeapons:
            case FmgIDType.SummaryArmor:
            case FmgIDType.SummaryRings:
            case FmgIDType.SummaryRings_DLC1:
            case FmgIDType.SummaryRings_DLC2:
            case FmgIDType.SummarySpells:
            case FmgIDType.SummarySpells_DLC1:
            case FmgIDType.SummarySpells_DLC2:
            case FmgIDType.SummaryArmor_Patch:
            case FmgIDType.SummaryGoods_Patch:
            case FmgIDType.SummaryRings_Patch:
            case FmgIDType.SummaryWeapons_Patch:
            case FmgIDType.SummaryMission:
            case FmgIDType.TutorialTitle: // Include as summary (not all TutorialBody's have a title)
            case FmgIDType.TutorialTitle2023:
            case FmgIDType.SummaryWeapons_SOTE:
            case FmgIDType.SummaryArmor_SOTE:
            case FmgIDType.SummaryGoods_SOTE:
            case FmgIDType.SummaryRings_SOTE:
            case FmgIDType.SummaryGem_SOTE:
            case FmgIDType.SummarySpells_SOTE:
            case FmgIDType.TutorialTitle_SOTE:
                return FmgEntryTextType.Summary;

            case FmgIDType.TitleGoods:
            case FmgIDType.TitleGoods_DLC2:
            case FmgIDType.TitleWeapons:
            case FmgIDType.TitleWeapons_DLC1:
            case FmgIDType.TitleWeapons_DLC2:
            case FmgIDType.TitleArmor:
            case FmgIDType.TitleArmor_DLC1:
            case FmgIDType.TitleArmor_DLC2:
            case FmgIDType.TitleRings:
            case FmgIDType.TitleRings_DLC1:
            case FmgIDType.TitleRings_DLC2:
            case FmgIDType.TitleSpells:
            case FmgIDType.TitleSpells_DLC1:
            case FmgIDType.TitleSpells_DLC2:
            case FmgIDType.TitleCharacters:
            case FmgIDType.TitleCharacters_DLC1:
            case FmgIDType.TitleCharacters_DLC2:
            case FmgIDType.TitleLocations:
            case FmgIDType.TitleLocations_DLC1:
            case FmgIDType.TitleLocations_DLC2:
            case FmgIDType.TitleTest:
            case FmgIDType.TitleTest2:
            case FmgIDType.TitleTest3:
            case FmgIDType.TitleArmor_Patch:
            case FmgIDType.TitleCharacters_Patch:
            case FmgIDType.TitleGoods_Patch:
            case FmgIDType.TitleLocations_Patch:
            case FmgIDType.TitleRings_Patch:
            case FmgIDType.TitleSpells_Patch:
            case FmgIDType.TitleWeapons_Patch:
            case FmgIDType.TitleBooster:
            case FmgIDType.TitleMission:
            case FmgIDType.TitleArchive:
            case FmgIDType.TitleWeapons_SOTE:
            case FmgIDType.TitleArmor_SOTE:
            case FmgIDType.TitleGoods_SOTE:
            case FmgIDType.TitleRings_SOTE:
            case FmgIDType.TitleGem_SOTE:
            case FmgIDType.TitleSpells_SOTE:
            case FmgIDType.TitleCharacters_SOTE:
            case FmgIDType.TitleLocations_SOTE:
            case FmgIDType.TitleSwordArts_SOTE:
                return FmgEntryTextType.Title;

            case FmgIDType.GoodsInfo2:
            case FmgIDType.GoodsInfo2_SOTE:
            case FmgIDType.MissionLocation:
                return FmgEntryTextType.ExtraText;

            case FmgIDType.WeaponEffect:
            case FmgIDType.WeaponEffect_SOTE:
            case FmgIDType.TutorialBody: // Include as TextBody to make it display foremost.
            case FmgIDType.TutorialBody2023:
            case FmgIDType.TutorialBody_SOTE:
                return FmgEntryTextType.TextBody;

            default:
                return FmgEntryTextType.TextBody;
        }
    }

    public static FmgFileCategory GetFMGUICategory(FmgEntryCategory entryCategory)
    {
        switch (entryCategory)
        {
            case FmgEntryCategory.Goods:
            case FmgEntryCategory.Weapons:
            case FmgEntryCategory.Armor:
            case FmgEntryCategory.Rings:
            case FmgEntryCategory.Gem:
            case FmgEntryCategory.SwordArts:
            case FmgEntryCategory.Generator:
            case FmgEntryCategory.Booster:
            case FmgEntryCategory.FCS:
            case FmgEntryCategory.Archive:
                return FmgFileCategory.Item;
            default:
                return FmgFileCategory.Menu;
        }
    }
}
