using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// <summary>
///     FMG sections in UI
/// </summary>
public enum FmgFileCategory
{
    Loose = 0,
    Item = 1,
    Menu = 2
}

/// <summary>
///     FMG sections in UI
/// </summary>
public enum FmgUICategory
{
    Text = 0,
    Item = 1,
    Menu = 2
}

/// <summary>
///     Entry type for Title, Summary, Description, or other.
/// </summary>
public enum FmgEntryTextType
{
    TextBody = 0,
    Title = 1,
    Summary = 2,
    Description = 3,
    ExtraText = 4
}

/// <summary>
///     Text categories used for grouping multiple FMGs or broad identification
/// </summary>
public enum FmgEntryCategory
{
    None = -1,
    Goods,
    Weapons,
    Armor,
    Rings,
    Spells,
    Characters,
    Locations,
    Gem,
    Message,
    SwordArts,
    Effect,
    ActionButtonText,
    Tutorial,
    LoadingScreen,
    Generator,
    Booster,
    FCS,
    Mission,
    Archive,

    ItemFmgDummy = 200 // Anything with this will be sorted into the item section of the editor.
}


/// <summary>
///     BND IDs for FMG files used for identification
/// </summary>
public enum FmgIDType
{
    // Note: Matching names with _DLC and _PATCH are used as identifiers for patch FMGs. This is a little dumb and patch fmg handling should probably be redone.
    None = -1,

    TitleGoods = 10,
    TitleWeapons = 11,
    TitleArmor = 12,
    TitleRings = 13,
    TitleSpells = 14,
    TitleTest = 15,
    TitleTest2 = 16,
    TitleTest3 = 17,
    TitleCharacters = 18,
    TitleLocations = 19,
    SummaryGoods = 20,
    SummaryWeapons = 21,
    SummaryArmor = 22,
    SummaryRings = 23,
    DescriptionGoods = 24,
    DescriptionWeapons = 25,
    DescriptionArmor = 26,
    DescriptionRings = 27,
    SummarySpells = 28,
    DescriptionSpells = 29,

    //
    TalkMsg = 1,
    BloodMsg = 2,
    MovieSubtitle = 3,
    Event = 30,
    MenuInGame = 70,
    MenuCommon = 76,
    MenuOther = 77,
    MenuDialog = 78,
    MenuKeyGuide = 79,
    MenuLineHelp = 80,
    MenuContext = 81,
    MenuTags = 90,
    Win32Tags = 91,
    Win32Messages = 92,
    Event_Patch = 101,
    MenuDialog_Patch = 102,
    Win32Messages_Patch = 103,
    TalkMsg_Patch = 104,
    BloodMsg_Patch = 107,
    MenuLineHelp_Patch = 121,
    MenuKeyGuide_Patch = 122,
    MenuOther_Patch = 123,
    MenuCommon_Patch = 124,

    // DS1 _DLC
    DescriptionGoods_Patch = 100,
    DescriptionSpells_Patch = 105,
    DescriptionWeapons_Patch = 106,
    DescriptionArmor_Patch = 108,
    DescriptionRings_Patch = 109,
    SummaryGoods_Patch = 110,
    TitleGoods_Patch = 111,
    SummaryRings_Patch = 112,
    TitleRings_Patch = 113,
    SummaryWeapons_Patch = 114,
    TitleWeapons_Patch = 115,
    SummaryArmor_Patch = 116,
    TitleArmor_Patch = 117,
    TitleSpells_Patch = 118,
    TitleCharacters_Patch = 119,
    TitleLocations_Patch = 120,

    // DS3 _DLC1
    TitleWeapons_DLC1 = 211,
    TitleArmor_DLC1 = 212,
    TitleRings_DLC1 = 213,
    TitleSpells_DLC1 = 214,
    TitleCharacters_DLC1 = 215,
    TitleLocations_DLC1 = 216,
    SummaryGoods_DLC1 = 217,
    SummaryRings_DLC1 = 220,
    DescriptionGoods_DLC1 = 221,
    DescriptionWeapons_DLC1 = 222,
    DescriptionArmor_DLC1 = 223,
    DescriptionRings_DLC1 = 224,
    SummarySpells_DLC1 = 225,
    DescriptionSpells_DLC1 = 226,

    //
    Modern_MenuText = 200,
    Modern_LineHelp = 201,
    Modern_KeyGuide = 202,
    Modern_SystemMessage_win64 = 203,
    Modern_Dialogues = 204,
    TalkMsg_DLC1 = 230,
    Event_DLC1 = 231,
    Modern_MenuText_DLC1 = 232,
    Modern_LineHelp_DLC1 = 233,
    Modern_SystemMessage_win64_DLC1 = 235,
    Modern_Dialogues_DLC1 = 236,
    SystemMessage_PS4_DLC1 = 237,
    SystemMessage_XboxOne_DLC1 = 238,
    BloodMsg_DLC1 = 239,

    // DS3 _DLC2
    TitleGoods_DLC2 = 250,
    TitleWeapons_DLC2 = 251,
    TitleArmor_DLC2 = 252,
    TitleRings_DLC2 = 253,
    TitleSpells_DLC2 = 254,
    TitleCharacters_DLC2 = 255,
    TitleLocations_DLC2 = 256,
    SummaryGoods_DLC2 = 257,
    SummaryRings_DLC2 = 260,
    DescriptionGoods_DLC2 = 261,
    DescriptionWeapons_DLC2 = 262,
    DescriptionArmor_DLC2 = 263,
    DescriptionRings_DLC2 = 264,
    SummarySpells_DLC2 = 265,
    DescriptionSpells_DLC2 = 266,

    //
    TalkMsg_DLC2 = 270,
    Event_DLC2 = 271,
    Modern_MenuText_DLC2 = 272,
    Modern_LineHelp_DLC2 = 273,
    Modern_SystemMessage_win64_DLC2 = 275,
    Modern_Dialogues_DLC2 = 276,
    SystemMessage_PS4_DLC2 = 277,
    SystemMessage_XboxOne_DLC2 = 278,
    BloodMsg_DLC2 = 279,

    // SDT
    Skills = 40,

    // ER - Item
    DescriptionGem = 37,
    SummarySwordArts = 43,
    WeaponEffect = 44,
    ERUnk45 = 45,
    GoodsInfo2 = 46,

    // ER - Menu
    TalkMsg_FemalePC_Alt = 4,
    NetworkMessage = 31,
    EventTextForTalk = 33,
    EventTextForMap = 34,
    TutorialTitle = 207,
    TutorialBody = 208,
    TextEmbedImageName_win64 = 209,

    // ER - Item: SOTE - DLC1
    TitleWeapons_SOTE = 310,
    SummaryWeapons_SOTE = 311,
    DescriptionWeapons_SOTE = 312,
    TitleArmor_SOTE = 313,
    SummaryArmor_SOTE = 314,
    DescriptionArmor_SOTE = 315,
    TitleGoods_SOTE = 319,
    SummaryGoods_SOTE = 320,
    DescriptionGoods_SOTE = 321,
    TitleRings_SOTE = 316,
    SummaryRings_SOTE = 317,
    DescriptionRings_SOTE = 318,
    TitleGem_SOTE = 322,
    SummaryGem_SOTE = 323,
    DescriptionGem_SOTE = 324,
    TitleSpells_SOTE = 325,
    SummarySpells_SOTE = 326,
    DescriptionSpells_SOTE = 327,
    TitleCharacters_SOTE = 328,
    TitleLocations_SOTE = 329,

    GoodsDialog = 330,
    TitleSwordArts_SOTE = 331,
    SummarySwordArts_SOTE = 332,
    WeaponEffect_SOTE = 333,
    ERUnk45_SOTE = 334,
    GoodsInfo2_SOTE = 335,

    // ER - Menu: SOTE - DLC1
    TalkMsg_SOTE = 360,
    BloodMsg_SOTE = 361,
    MovieSubtitle_SOTE = 362,
    NetworkMessage_SOTE = 364,
    ActionButtonText_SOTE = 365,
    EventTextForTalk_SOTE = 366,
    EventTextForMap_SOTE = 367,
    Modern_MenuText_SOTE = 368,
    Modern_LineHelp_SOTE = 369,
    Modern_KeyGuide_SOTE = 370,
    Modern_SystemMessage_win64_SOTE = 371,
    Modern_Dialogues_SOTE = 372,
    LoadingTitle_SOTE = 373,
    LoadingText_SOTE = 374,
    TutorialTitle_SOTE = 375,
    TutorialBody_SOTE = 376,

    // ER - Item: SOTE - DLC2
    TitleWeapons_SOTE_DLC2 = 410,
    SummaryWeapons_SOTE_DLC2 = 411,
    DescriptionWeapons_SOTE_DLC2 = 412,
    TitleArmor_SOTE_DLC2 = 413,
    SummaryArmor_SOTE_DLC2 = 414,
    DescriptionArmor_SOTE_DLC2 = 415,
    TitleGoods_SOTE_DLC2 = 419,
    SummaryGoods_SOTE_DLC2 = 420,
    DescriptionGoods_SOTE_DLC2 = 421,
    TitleRings_SOTE_DLC2 = 416,
    SummaryRings_SOTE_DLC2 = 417,
    DescriptionRings_SOTE_DLC2 = 418,
    TitleGem_SOTE_DLC2 = 422,
    SummaryGem_SOTE_DLC2 = 423,
    DescriptionGem_SOTE_DLC2 = 424,
    TitleSpells_SOTE_DLC2 = 425,
    SummarySpells_SOTE_DLC2 = 426,
    DescriptionSpells_SOTE_DLC2 = 427,
    TitleCharacters_SOTE_DLC2 = 428,
    TitleLocations_SOTE_DLC2 = 429,

    GoodsDialog_SOTE_DLC2 = 430,
    TitleSwordArts_SOTE_DLC2 = 431,
    SummarySwordArts_SOTE_DLC2 = 432,
    WeaponEffect_SOTE_DLC2 = 433,
    ERUnk45_SOTE_DLC2 = 434,
    GoodsInfo2_SOTE_DLC2 = 435,

    // ER - Menu: SOTE - DLC2
    TalkMsg_SOTE_DLC2 = 460,
    BloodMsg_SOTE_DLC2 = 461,
    MovieSubtitle_SOTE_DLC2 = 462,
    NetworkMessage_SOTE_DLC2 = 464,
    ActionButtonText_SOTE_DLC2 = 465,
    EventTextForTalk_SOTE_DLC2 = 466,
    EventTextForMap_SOTE_DLC2 = 467,
    Modern_MenuText_SOTE_DLC2 = 468,
    Modern_LineHelp_SOTE_DLC2 = 469,
    Modern_KeyGuide_SOTE_DLC2 = 470,
    Modern_SystemMessage_win64_SOTE_DLC2 = 471,
    Modern_Dialogues_SOTE_DLC2 = 472,
    LoadingTitle_SOTE_DLC2 = 473,
    LoadingText_SOTE_DLC2 = 474,
    TutorialTitle_SOTE_DLC2 = 475,
    TutorialBody_SOTE_DLC2 = 476,

    // AC6 - Item
    TitleBooster = 38,
    DescriptionBooster = 39,

    // AC6 - Menu
    RankerProfile = 50,
    TitleMission = 60,
    SummaryMission = 61,
    DescriptionMission = 62,
    MissionLocation = 63,
    TitleArchive = 65,
    DescriptionArchive = 66,
    TutorialTitle2023 = 73,
    TutorialBody2023 = 74,

    // Multiple use cases. Differences are applied in ApplyGameDifferences();
    // ER: ActionButtonText
    ReusedFMG_32 = 32,

    // FMG 32
    // BB:  GemExtraInfo
    // DS3: ActionButtonText
    // SDT: ActionButtonText
    // ER:  ActionButtonText
    ReusedFMG_35 = 35,

    // FMG 35
    // Most: TitleGem
    // AC6:  TitleGenerator
    ReusedFMG_36 = 36,

    // FMG 36
    // Most: SummaryGem
    // AC6:  DescriptionGenerator
    ReusedFMG_41 = 41,

    // FMG 41
    // Most: TitleMessage
    // AC6:  TitleFCS
    ReusedFMG_42 = 42,

    // FMG 42
    // Most: TitleSwordArts
    // AC6:  DescriptionFCS
    ReusedFMG_210 = 210,

    // FMG 210
    // DS3: TitleGoods_DLC1
    // SDT: ?
    // ER:  ToS_win64
    // AC6: TextEmbeddedImageNames
    ReusedFMG_205 = 205,

    // FMG 205
    // DS3: SystemMessage_PS4
    // SDT: TutorialText
    // ER:  LoadingTitle
    // AC6: MenuContext
    ReusedFMG_206 = 206
    // FMG 206
    // DS3: SystemMessage_XboxOne 
    // SDT: TutorialTitle
    // ER:  LoadingText
}

