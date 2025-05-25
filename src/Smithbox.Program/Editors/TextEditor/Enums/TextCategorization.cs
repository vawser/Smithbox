using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

// TODO: really these should now be objects but I don't want to re-work everything again so they'll remain enums for now
// TODO: ACFA, ACV, ACVD

/// ----------------------------------
/// Keep the enum names consistent between each game as the ParamMETA 
/// uses them to link the FmgRef to the actual FMG.
/// ----------------------------------

/// ShortName: holds the display grouping
/// - "Common"
/// - "Title"
/// - "Summary"
/// - "Description"
/// - "Effect"
/// - "Menu"
/// 
/// Description: holds the DLC grouping
/// - ""
/// - "DLC1"
/// - "DLC2"

/// <summary>
/// FMG IDs for DES
/// </summary>
public enum Item_MsgBndID_DES
{
    [Display(Name = "Goods", ShortName="Title", Description ="")] 
    Title_Goods = 10,

    [Display(Name = "Weapons", ShortName = "Title", Description = "")] 
    Title_Weapons = 11,

    [Display(Name = "Armor", ShortName = "Title", Description = "")] 
    Title_Armor = 12,

    [Display(Name = "Accessories", ShortName = "Title", Description = "")] 
    Title_Accessories = 13,

    [Display(Name = "Magic", ShortName = "Title", Description = "")] 
    Title_Magic = 14,

    [Display(Name = "Effect", ShortName = "Title", Description = "")] 
    Title_Effect = 15,

    [Display(Name = "Effect", ShortName = "Summary", Description = "")] 
    Summary_Effect = 16,

    [Display(Name = "Effect", ShortName = "Description", Description = "")] 
    Description_Effect = 17,

    [Display(Name = "Characters", ShortName = "Common", Description = "")] 
    Title_Characters = 18,

    [Display(Name = "Locations", ShortName = "Common", Description = "")] 
    Title_Locations = 19,

    [Display(Name = "Goods", ShortName = "Summary", Description = "")] 
    Summary_Goods = 20,

    [Display(Name = "Weapons", ShortName = "Summary", Description = "")] 
    Summary_Weapons = 21,

    [Display(Name = "Armor", ShortName = "Summary", Description = "")] 
    Summary_Armor = 22,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "")] 
    Summary_Accessories = 23,

    [Display(Name = "Magic", ShortName = "Summary", Description = "")] 
    Summary_Magic = 28,

    [Display(Name = "Goods", ShortName = "Description", Description = "")] 
    Description_Goods = 24,

    [Display(Name = "Weapons", ShortName = "Description", Description = "")] 
    Description_Weapons = 25,

    [Display(Name = "Armor", ShortName = "Description", Description = "")] 
    Description_Armor = 26,

    [Display(Name = "Accessories", ShortName = "Description", Description = "")] 
    Description_Accessories = 27,

    [Display(Name = "Magic", ShortName = "Description", Description = "")] 
    Description_Magic = 29
}

public enum Menu_MsgBndID_DES
{
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "")]
    Talk_Messages = 1,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "")]
    Blood_Messages = 2,

    [Display(Name = "Movie Subtitles", ShortName = "Common", Description = "")]
    Movie_Subtitles = 3,

    [Display(Name = "Event Text", ShortName = "Common", Description = "")]
    Event_Text = 30,

    [Display(Name = "In-game Text", ShortName = "Common", Description = "")]
    Menu_Ingame = 70,

    [Display(Name = "Common Text", ShortName = "Common", Description = "")]
    Menu_Common = 76,

    [Display(Name = "Other Text", ShortName = "Common", Description = "")]
    Menu_Other = 77,

    [Display(Name = "Dialog Text", ShortName = "Common", Description = "")]
    Menu_Dialog = 78,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "")]
    Menu_Key_Guide = 79,

    [Display(Name = "Line Help", ShortName = "Common", Description = "")]
    Menu_Line_Help = 80,

    [Display(Name = "Context", ShortName = "Common", Description = "")]
    Menu_Context = 81,

    [Display(Name = "Tags", ShortName = "Common", Description = "")]
    Menu_Tags = 90
}

public enum Sample_MsgBndID_DES
{
    [Display(Name = "Sample", ShortName = "Common", Description = "")]
    Sample = 0,

    [Display(Name = "Key Guide Test", ShortName = "Common", Description = "")]
    Key_Guide_Test = 500
}

/// <summary>
/// FMG IDs for DS1
/// </summary>
public enum Item_MsgBndID_DS1
{
    [Display(Name = "Goods", ShortName = "Title", Description = "")]
    Title_Goods = 10,

    [Display(Name = "Weapons", ShortName = "Title", Description = "")]
    Title_Weapons = 11,

    [Display(Name = "Armor", ShortName = "Title", Description = "")]
    Title_Armor = 12,

    [Display(Name = "Accessories", ShortName = "Title", Description = "")]
    Title_Accessories = 13,

    [Display(Name = "Magic", ShortName = "Title", Description = "")]
    Title_Magic = 14,

    [Display(Name = "Test [1]", ShortName = "Common", Description = "")]
    Title_Test = 15,

    [Display(Name = "Test [2]", ShortName = "Common", Description = "")]
    Title_Test_2 = 16,

    [Display(Name = "Test [3]", ShortName = "Common", Description = "")]
    Title_Test_3 = 17,

    [Display(Name = "Characters", ShortName = "Common", Description = "")]
    Title_Characters = 18,

    [Display(Name = "Locations", ShortName = "Common", Description = "")]
    Title_Locations = 19,

    [Display(Name = "Goods", ShortName = "Summary", Description = "")]
    Summary_Goods = 20,

    [Display(Name = "Weapons", ShortName = "Summary", Description = "")]
    Summary_Weapons = 21,

    [Display(Name = "Armor", ShortName = "Summary", Description = "")]
    Summary_Armor = 22,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "")]
    Summary_Accessories = 23,

    [Display(Name = "Magic", ShortName = "Summary", Description = "")]
    Summary_Magic = 28,

    [Display(Name = "Goods", ShortName = "Description", Description = "")]
    Description_Goods = 24,

    [Display(Name = "Weapons", ShortName = "Description", Description = "")]
    Description_Weapons = 25,

    [Display(Name = "Armor", ShortName = "Description", Description = "")]
    Description_Armor = 26,

    [Display(Name = "Accessories", ShortName = "Description", Description = "")]
    Description_Accessories = 27,

    [Display(Name = "Magic", ShortName = "Description", Description = "")]
    Description_Magic = 29,

    // DLC
    [Display(Name = "Goods", ShortName = "Title", Description = "DLC 1")]
    Title_Goods_DLC = 111,

    [Display(Name = "Weapons", ShortName = "Title", Description = "DLC 1")]
    Title_Weapons_DLC = 115,

    [Display(Name = "Armor", ShortName = "Title", Description = "DLC 1")]
    Title_Armor_DLC = 117,

    [Display(Name = "Accessories", ShortName = "Title", Description = "DLC 1")]
    Title_Accessories_DLC = 113,

    [Display(Name = "Magic", ShortName = "Title", Description = "DLC 1")]
    Title_Magic_DLC = 118,

    [Display(Name = "Characters", ShortName = "Common", Description = "DLC 1")]
    Title_Characters_DLC = 119,

    [Display(Name = "Locations", ShortName = "Common", Description = "DLC 1")]
    Title_Locations_DLC = 120,

    [Display(Name = "Goods", ShortName = "Summary", Description = "DLC 1")]
    Summary_Goods_DLC = 110,

    [Display(Name = "Weapons", ShortName = "Summary", Description = "DLC 1")]
    Summary_Weapons_DLC = 114,

    [Display(Name = "Armor", ShortName = "Summary", Description = "DLC 1")]
    Summary_Armor_DLC = 116,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "DLC 1")]
    Summary_Accessories_DLC = 112,

    [Display(Name = "Goods", ShortName = "Description", Description = "DLC 1")]
    Description_Goods_DLC = 100,

    [Display(Name = "Weapons", ShortName = "Description", Description = "DLC 1")]
    Description_Weapons_DLC = 106,

    [Display(Name = "Armor", ShortName = "Description", Description = "DLC 1")]
    Description_Armor_DLC = 108,

    [Display(Name = "Accessories", ShortName = "Description", Description = "DLC 1")]
    Description_Accessories_DLC = 109,

    [Display(Name = "Magic", ShortName = "Description", Description = "DLC 1")]
    Description_Magic_DLC = 105
}

public enum Menu_MsgBndID_DS1
{
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "")]
    Talk_Messages = 1,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "")]
    Blood_Messages = 2,

    [Display(Name = "Movie Subtitles", ShortName = "Common", Description = "")]
    Movie_Subtitles = 3,

    [Display(Name = "Event Text", ShortName = "Common", Description = "")]
    Event_Text = 30,

    [Display(Name = "In-game Text", ShortName = "Common", Description = "")]
    Menu_Ingame = 70,

    [Display(Name = "Common Text", ShortName = "Common", Description = "")]
    Menu_Common = 76,

    [Display(Name = "Other Text", ShortName = "Common", Description = "")]
    Menu_Other = 77,

    [Display(Name = "Dialog Text", ShortName = "Common", Description = "")]
    Menu_Dialog = 78,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "")]
    Menu_Key_Guide = 79,

    [Display(Name = "Line Help", ShortName = "Common", Description = "")]
    Menu_Line_Help = 80,

    [Display(Name = "Context", ShortName = "Common", Description = "")]
    Menu_Context = 81,

    [Display(Name = "Tags", ShortName = "Common", Description = "")]
    Menu_Tags = 90,

    [Display(Name = "Tags: PC", ShortName = "Common", Description = "")]
    Win32_Tags = 91,

    [Display(Name = "Messages: PC", ShortName = "Common", Description = "")]
    Win32_Messages = 92,

    // DLC
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "DLC 1")]
    Talk_Messages_DLC = 104,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "DLC 1")]
    Blood_Messages_DLC = 107,

    [Display(Name = "Event Text", ShortName = "Common", Description = "DLC 1")]
    Event_Text_DLC = 101,

    [Display(Name = "Common Text", ShortName = "Common", Description = "DLC 1")]
    Menu_Common_DLC = 124,

    [Display(Name = "Other Text", ShortName = "Common", Description = "DLC 1")]
    Menu_Other_DLC = 123,

    [Display(Name = "Dialog Text", ShortName = "Common", Description = "DLC 1")]
    Menu_Dialog_DLC = 102,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "DLC 1")]
    Menu_Key_Guide_DLC = 122,

    [Display(Name = "Line Help", ShortName = "Common", Description = "DLC 1")]
    Menu_Line_Help_DLC = 121,

    [Display(Name = "Messages: PC", ShortName = "Common", Description = "DLC 1")]
    Win32_Messages_DLC = 103,
}

/// <summary>
/// FMG IDs for BB
/// </summary>
public enum Item_MsgBndID_BB
{
    [Display(Name = "Goods", ShortName = "Title", Description = "")]
    Title_Goods = 10,

    [Display(Name = "Weapons", ShortName = "Title", Description = "")]
    Title_Weapons = 11,

    [Display(Name = "Armor", ShortName = "Title", Description = "")]
    Title_Armor = 12,

    [Display(Name = "Accessories", ShortName = "Title", Description = "")]
    Title_Accessories = 13,

    [Display(Name = "Magic", ShortName = "Title", Description = "")]
    Title_Magic = 14,

    [Display(Name = "Characters", ShortName = "Common", Description = "")]
    Title_Characters = 18,

    [Display(Name = "Locations", ShortName = "Common", Description = "")]
    Title_Locations = 19,

    [Display(Name = "Goods", ShortName = "Summary", Description = "")]
    Summary_Goods = 20,

    [Display(Name = "Weapons", ShortName = "Summary", Description = "")]
    Summary_Weapons = 21,

    [Display(Name = "Armor", ShortName = "Summary", Description = "")]
    Summary_Armor = 22,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "")]
    Summary_Accessories = 23,

    [Display(Name = "Magic", ShortName = "Summary", Description = "")]
    Summary_Magic = 28,

    [Display(Name = "Goods", ShortName = "Description", Description = "")]
    Description_Goods = 24,

    [Display(Name = "Weapons", ShortName = "Description", Description = "")]
    Description_Weapons = 25,

    [Display(Name = "Armor", ShortName = "Description", Description = "")]
    Description_Armor = 26,

    [Display(Name = "Accessories", ShortName = "Description", Description = "")]
    Description_Accessories = 27,

    [Display(Name = "Magic", ShortName = "Description", Description = "")]
    Description_Magic = 29
}

public enum Menu_MsgBndID_BB
{
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "")]
    Talk_Messages = 1,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "")]
    Blood_Messages = 2,

    [Display(Name = "Movie Subtitles", ShortName = "Common", Description = "")]
    Movie_Subtitles = 3,

    [Display(Name = "Causes of Death", ShortName = "Common", Description = "")]
    Death_Causes = 4,

    [Display(Name = "Event Text", ShortName = "Common", Description = "")]
    Event_Text = 30,

    [Display(Name = "Blood Gems", ShortName = "Title", Description = "")]
    Title_Blood_Gem = 31,

    [Display(Name = "Blood Gems", ShortName = "Summary", Description = "")]
    Summary_Blood_Gem = 32,

    [Display(Name = "Blood Gems", ShortName = "Description", Description = "")]
    Description_Blood_Gem = 33,

    [Display(Name = "Blood Gem Prefixes", ShortName = "Common", Description = "")]
    Prefix_Blood_Gem = 34,

    [Display(Name = "Blood Gem Effects", ShortName = "Common", Description = "")]
    Effect_Blood_Gem = 35,

    [Display(Name = "In-game Text", ShortName = "Common", Description = "")]
    Menu_Ingame = 70,

    [Display(Name = "Common Text", ShortName = "Common", Description = "")]
    Menu_Common = 76,

    [Display(Name = "Other Text", ShortName = "Common", Description = "")]
    Menu_Other = 77,

    [Display(Name = "Dialog Text", ShortName = "Common", Description = "")]
    Menu_Dialog = 78,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "")]
    Menu_Key_Guide = 79,

    [Display(Name = "Line Help", ShortName = "Common", Description = "")]
    Menu_Line_Help = 80,

    [Display(Name = "Tags", ShortName = "Common", Description = "")]
    Menu_Tags = 90,

    [Display(Name = "Tags: PC", ShortName = "Common", Description = "")]
    Win32_Tags = 91,

    [Display(Name = "Messages: PC", ShortName = "Common", Description = "")]
    Win32_Messages = 92,

    [Display(Name = "Menu Text", ShortName = "Menu", Description = "")]
    Modern_Menu_Text = 200,

    [Display(Name = "Line Help", ShortName = "Menu", Description = "")]
    Modern_Menu_Line_Help = 201,

    [Display(Name = "Key Guide", ShortName = "Menu", Description = "")]
    Modern_Menu_Key_Guide = 202,

    [Display(Name = "System Messages: PC", ShortName = "Common", Description = "")]
    Modern_Menu_System_Message = 203,

    [Display(Name = "Dialogue", ShortName = "Menu", Description = "")]
    Modern_Menu_Dialogue = 204
}

/// <summary>
/// FMG IDs for DS3
/// </summary>
public enum Item_MsgBndID_DS3
{
    [Display(Name = "Goods", ShortName = "Title", Description = "")]
    Title_Goods = 10,

    [Display(Name = "Weapons", ShortName = "Title", Description = "")]
    Title_Weapons = 11,

    [Display(Name = "Armor", ShortName = "Title", Description = "")]
    Title_Armor = 12,

    [Display(Name = "Accessories", ShortName = "Title", Description = "")]
    Title_Accessories = 13,

    [Display(Name = "Magic", ShortName = "Title", Description = "")]
    Title_Magic = 14,

    [Display(Name = "Characters", ShortName = "Common", Description = "")]
    Title_Characters = 18,

    [Display(Name = "Locations", ShortName = "Common", Description = "")]
    Title_Locations = 19,

    [Display(Name = "Goods", ShortName = "Summary", Description = "")]
    Summary_Goods = 20,

    [Display(Name = "Weapons", ShortName = "Summary", Description = "")]
    Summary_Weapons = 21,

    [Display(Name = "Armor", ShortName = "Summary", Description = "")]
    Summary_Armor = 22,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "")]
    Summary_Accessories = 23,

    [Display(Name = "Magic", ShortName = "Summary", Description = "")]
    Summary_Magic = 28,

    [Display(Name = "Goods", ShortName = "Description", Description = "")]
    Description_Goods = 24,

    [Display(Name = "Weapons", ShortName = "Description", Description = "")]
    Description_Weapons = 25,

    [Display(Name = "Armor", ShortName = "Description", Description = "")]
    Description_Armor = 26,

    [Display(Name = "Accessories", ShortName = "Description", Description = "")]
    Description_Accessories = 27,

    [Display(Name = "Magic", ShortName = "Description", Description = "")]
    Description_Magic = 29,

    // DLC1
    [Display(Name = "Goods", ShortName = "Title", Description = "DLC 1")]
    Title_Goods_DLC1 = 210,

    [Display(Name = "Weapons", ShortName = "Title", Description = "DLC 1")]
    Title_Weapons_DLC1 = 211,

    [Display(Name = "Armor", ShortName = "Title", Description = "DLC 1")]
    Title_Armor_DLC1 = 212,

    [Display(Name = "Accessories", ShortName = "Title", Description = "DLC 1")]
    Title_Accessories_DLC1 = 213,

    [Display(Name = "Magic", ShortName = "Title", Description = "DLC 1")]
    Title_Magic_DLC1 = 214,

    [Display(Name = "Characters", ShortName = "Common", Description = "DLC 1")]
    Title_Character_DLC1 = 215,

    [Display(Name = "Locations", ShortName = "Common", Description = "DLC 1")]
    Title_Locations_DLC1 = 216,

    [Display(Name = "Goods", ShortName = "Summary", Description = "DLC 1")]
    Summary_Goods_DLC1 = 217,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "DLC 1")]
    Summary_Accessories_DLC1 = 220,

    [Display(Name = "Goods", ShortName = "Description", Description = "DLC 1")]
    Description_Goods_DLC1 = 221,

    [Display(Name = "Weapons", ShortName = "Description", Description = "DLC 1")]
    Description_Weapons_DLC1 = 222,

    [Display(Name = "Armor", ShortName = "Description", Description = "DLC 1")]
    Description_Armor_DLC1 = 223,

    [Display(Name = "Accessories", ShortName = "Description", Description = "DLC 1")]
    Description_Accessories_DLC1 = 224,

    [Display(Name = "Magic", ShortName = "Summary", Description = "DLC 1")]
    Summary_Magic_DLC1 = 225,

    [Display(Name = "Magic", ShortName = "Description", Description = "DLC 1")]
    Description_Magic_DLC1 = 226,

    // DLC2
    [Display(Name = "Goods", ShortName = "Title", Description = "DLC 2")]
    Title_Goods_DLC2 = 250,

    [Display(Name = "Weapons", ShortName = "Title", Description = "DLC 2")]
    Title_Weapons_DLC2 = 251,

    [Display(Name = "Armor", ShortName = "Title", Description = "DLC 2")]
    Title_Armor_DLC2 = 252,

    [Display(Name = "Accessories", ShortName = "Title", Description = "DLC 2")]
    Title_Accessories_DLC2 = 253,

    [Display(Name = "Magic", ShortName = "Title", Description = "DLC 2")]
    Title_Magic_DLC2 = 254,

    [Display(Name = "Characters", ShortName = "Common", Description = "DLC 2")]
    Title_Character_DLC2 = 255,

    [Display(Name = "Locations", ShortName = "Common", Description = "DLC 2")]
    Title_Locations_DLC2 = 256,

    [Display(Name = "Goods", ShortName = "Summary", Description = "DLC 2")]
    Summary_Goods_DLC2 = 257,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "DLC 2")]
    Summary_Accessories_DLC2 = 260,

    [Display(Name = "Goods", ShortName = "Description", Description = "DLC 2")]
    Description_Goods_DLC2 = 261,

    [Display(Name = "Weapons", ShortName = "Description", Description = "DLC 2")]
    Description_Weapons_DLC2 = 262,

    [Display(Name = "Armor", ShortName = "Description", Description = "DLC 2")]
    Description_Armor_DLC2 = 263,

    [Display(Name = "Accessories", ShortName = "Description", Description = "DLC 2")]
    Description_Accessories_DLC2 = 264,

    [Display(Name = "Magic", ShortName = "Summary", Description = "DLC 2")]
    Summary_Magic_DLC2 = 265,

    [Display(Name = "Magic", ShortName = "Description", Description = "DLC 2")]
    Description_Magic_DLC2 = 266
}

public enum Menu_MsgBndID_DS3
{
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "")]
    Talk_Messages = 1,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "")]
    Blood_Messages = 2,

    [Display(Name = "Movie Subtitles", ShortName = "Common", Description = "")]
    Movie_Subtitles = 3,

    [Display(Name = "Event Text", ShortName = "Common", Description = "")]
    Event_Text = 30,

    [Display(Name = "In-game Text", ShortName = "Common", Description = "")]
    Menu_Ingame = 70,

    [Display(Name = "Common Text", ShortName = "Common", Description = "")]
    Menu_Common = 76,

    [Display(Name = "Other Text", ShortName = "Common", Description = "")]
    Menu_Other = 77,

    [Display(Name = "Dialog Text", ShortName = "Common", Description = "")]
    Menu_Dialog = 78,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "")]
    Menu_Key_Guide = 79,

    [Display(Name = "Line Help", ShortName = "Common", Description = "")]
    Menu_Line_Help = 80,

    [Display(Name = "Context", ShortName = "Common", Description = "")]
    Menu_Context = 81,

    [Display(Name = "Tags", ShortName = "Common", Description = "")]
    Menu_Tags = 90,

    [Display(Name = "Tags: PC", ShortName = "Common", Description = "")]
    Win32_Tags = 91,

    [Display(Name = "Messages: PC", ShortName = "Common", Description = "")]
    Win32_Messages = 92,

    [Display(Name = "Menu Text", ShortName = "Menu", Description = "")]
    Modern_Menu_Text = 200,

    [Display(Name = "Line Help", ShortName = "Menu", Description = "")]
    Modern_Menu_Line_Help = 201,

    [Display(Name = "Key Guide", ShortName = "Menu", Description = "")]
    Modern_Menu_Key_Guide = 202,

    [Display(Name = "System Messages: PC", ShortName = "Common", Description = "")]
    Modern_Menu_System_Message = 203,

    [Display(Name = "Dialogue", ShortName = "Menu", Description = "")]
    Modern_Menu_Dialogue = 204,

    [Display(Name = "System Messages: PS4", ShortName = "Common", Description = "")]
    System_Message_PS4 = 205,

    [Display(Name = "System Messages: XB1", ShortName = "Common", Description = "")]
    System_Message_XB1 = 206,

    // DLC1
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "DLC 1")]
    Talk_Messages_DLC1 = 230,

    [Display(Name = "Event Text", ShortName = "Common", Description = "DLC 1")]
    Event_Text_DLC1 = 231,

    [Display(Name = "Menu Text", ShortName = "Menu", Description = "DLC 1")]
    Modern_Menu_Text_DLC1 = 232,

    [Display(Name = "Line Help", ShortName = "Menu", Description = "DLC 1")]
    Modern_Menu_Line_Help_DLC1 = 233,

    [Display(Name = "System Messages: PC", ShortName = "Common", Description = "DLC 1")]
    Modern_Menu_System_Message_DLC1 = 235,

    [Display(Name = "Dialogue", ShortName = "Menu", Description = "DLC 1")]
    Modern_Menu_Dialogue_DLC1 = 236,

    [Display(Name = "System Messages: PS4", ShortName = "Common", Description = "DLC 1")]
    System_Message_PS4_DLC1 = 237,

    [Display(Name = "System Messages: XB1", ShortName = "Common", Description = "DLC 1")]
    System_Message_XB1_DLC1 = 238,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "DLC 1")]
    Blood_Messages_DLC1 = 239,

    // DLC2
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "DLC 2")]
    Talk_Messages_DLC2 = 270,

    [Display(Name = "Event Text", ShortName = "Common", Description = "DLC 2")]
    Event_Text_DLC2 = 271,

    [Display(Name = "Menu Text", ShortName = "Menu", Description = "DLC 2")]
    Modern_Menu_Text_DLC2 = 272,

    [Display(Name = "Line Help", ShortName = "Menu", Description = "DLC 2")]
    Modern_Menu_Line_Help_DLC2 = 273,

    [Display(Name = "System Messages: PC", ShortName = "Common", Description = "DLC 2")]
    Modern_Menu_System_Message_DLC2 = 275,

    [Display(Name = "Dialogue", ShortName = "Menu", Description = "DLC 2")]
    Modern_Menu_Dialogue_DLC2 = 276,

    [Display(Name = "System Messages: PS4", ShortName = "Common", Description = "DLC 2")]
    System_Message_PS4_DLC2 = 277,

    [Display(Name = "System Messages: XB1", ShortName = "Common", Description = "DLC 2")]
    System_Message_XB1_DLC2 = 278,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "DLC 2")]
    Blood_Messages_DLC2 = 279
}

public enum NgWord_MsgBndID_DS3
{
    [Display(Name = "Blocked Words", ShortName = "Common", Description = "")]
    NgWord = 1
}

/// <summary>
/// FMG IDs for SDT
/// </summary>
public enum Item_MsgBndID_SDT
{
    [Display(Name = "Goods", ShortName = "Title", Description = "")]
    Title_Goods = 10,

    [Display(Name = "Weapons", ShortName = "Title", Description = "")]
    Title_Weapons = 11,

    [Display(Name = "Armor", ShortName = "Title", Description = "")]
    Title_Armor = 12,

    [Display(Name = "Accessories", ShortName = "Title", Description = "")]
    Title_Accessories = 13,

    [Display(Name = "Magic", ShortName = "Title", Description = "")]
    Title_Magic = 14,

    [Display(Name = "Characters", ShortName = "Common", Description = "")]
    Title_Characters = 18,

    [Display(Name = "Locations", ShortName = "Title", Description = "")]
    Title_Locations = 19,

    [Display(Name = "Goods", ShortName = "Summary", Description = "")]
    Summary_Goods = 20,

    [Display(Name = "Weapons", ShortName = "Summary", Description = "")]
    Summary_Weapons = 21,

    [Display(Name = "Armor", ShortName = "Summary", Description = "")]
    Summary_Armor = 22,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "")]
    Summary_Accessories = 23,

    [Display(Name = "Magic", ShortName = "Summary", Description = "")]
    Summary_Magic = 28,

    [Display(Name = "Goods", ShortName = "Description", Description = "")]
    Description_Goods = 24,

    [Display(Name = "Weapons", ShortName = "Description", Description = "")]
    Description_Weapons = 25,

    [Display(Name = "Armor", ShortName = "Description", Description = "")]
    Description_Armor = 26,

    [Display(Name = "Accessories", ShortName = "Description", Description = "")]
    Description_Accessories = 27,

    [Display(Name = "Magic", ShortName = "Description", Description = "")]
    Description_Magic = 29,

    [Display(Name = "Combat Skills", ShortName = "Common", Description = "")]
    Description_Combat_Skills = 40
}

public enum Menu_MsgBndID_SDT
{
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "")]
    Talk_Messages = 1,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "")]
    Blood_Messages = 2,

    [Display(Name = "Movie Subtitles", ShortName = "Common", Description = "")]
    Movie_Subtitles = 3,

    [Display(Name = "Event Text", ShortName = "Common", Description = "")]
    Event_Text = 30,

    [Display(Name = "In-game Text", ShortName = "Common", Description = "")]
    Menu_Ingame = 70,

    [Display(Name = "Common Text", ShortName = "Common", Description = "")]
    Menu_Common = 76,

    [Display(Name = "Other Text", ShortName = "Common", Description = "")]
    Menu_Other = 77,

    [Display(Name = "Dialog Text", ShortName = "Common", Description = "")]
    Menu_Dialog = 78,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "")]
    Menu_Key_Guide = 79,

    [Display(Name = "Line Help", ShortName = "Common", Description = "")]
    Menu_Line_Help = 80,

    [Display(Name = "Context", ShortName = "Common", Description = "")]
    Menu_Context = 81,

    [Display(Name = "Tags", ShortName = "Common", Description = "")]
    Menu_Tags = 90,

    [Display(Name = "Tags: PC", ShortName = "Common", Description = "")]
    Win32_Tags = 91,

    [Display(Name = "Messages: PC", ShortName = "Common", Description = "")]
    Win32_Messages = 92,

    [Display(Name = "Menu Text", ShortName = "Menu", Description = "")]
    Modern_Menu_Text = 200,

    [Display(Name = "Line Help", ShortName = "Menu", Description = "")]
    Modern_Menu_Line_Help = 201,

    [Display(Name = "Key Guide", ShortName = "Menu", Description = "")]
    Modern_Menu_Key_Guide = 202,

    [Display(Name = "System Messages: PC", ShortName = "Common", Description = "")]
    Modern_Menu_System_Message = 203,

    [Display(Name = "Dialogue", ShortName = "Menu", Description = "")]
    Modern_Menu_Dialogue = 204,

    [Display(Name = "System Messages: PS4", ShortName = "Common", Description = "")]
    System_Message_PS4 = 205,

    [Display(Name = "System Messages: XB1", ShortName = "Common", Description = "")]
    System_Message_XB1 = 206
}

public enum SellRegion_MsgBndID_SDT
{
    [Display(Name = "Sell Region", ShortName = "Common", Description = "")]
    SellRegion = 300
}

/// <summary>
/// FMG IDs for ER
/// </summary>
public enum Item_MsgBndID_ER 
{
    [Display(Name = "Goods", ShortName = "Title", Description = "")]
    Title_Goods = 10,

    [Display(Name = "Weapons", ShortName = "Title", Description = "")]
    Title_Weapons = 11,

    [Display(Name = "Armor", ShortName = "Title", Description = "")]
    Title_Armor = 12,

    [Display(Name = "Accessories", ShortName = "Title", Description = "")]
    Title_Accessories = 13,

    [Display(Name = "Magic", ShortName = "Title", Description = "")]
    Title_Magic = 14,

    [Display(Name = "Characters", ShortName = "Common", Description = "")]
    Title_Characters = 18,

    [Display(Name = "Locations", ShortName = "Common", Description = "")]
    Title_Locations = 19,

    [Display(Name = "Goods", ShortName = "Summary", Description = "")]
    Summary_Goods = 20,

    [Display(Name = "Weapons", ShortName = "Summary", Description = "")]
    Summary_Weapons = 21,

    [Display(Name = "Armor", ShortName = "Summary", Description = "")]
    Summary_Armor = 22,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "")]
    Summary_Accessories = 23,

    [Display(Name = "Magic", ShortName = "Summary", Description = "")]
    Summary_Magic = 28,

    [Display(Name = "Goods", ShortName = "Description", Description = "")]
    Description_Goods = 24,

    [Display(Name = "Weapons", ShortName = "Description", Description = "")]
    Description_Weapons = 25,

    [Display(Name = "Armor", ShortName = "Description", Description = "")]
    Description_Armor = 26,

    [Display(Name = "Accessories", ShortName = "Description", Description = "")]
    Description_Accessories = 27,

    [Display(Name = "Magic", ShortName = "Description", Description = "")]
    Description_Magic = 29,

    [Display(Name = "Ash of War", ShortName = "Title", Description = "")]
    Title_Ash_of_War = 35,

    [Display(Name = "Ash of War", ShortName = "Summary", Description = "")]
    Summary_Ash_of_War = 36,

    [Display(Name = "Ash of War", ShortName = "Description", Description = "")]
    Description_Ash_of_War = 37,

    [Display(Name = "Goods Dialog", ShortName = "Common", Description = "")]
    Dialog_Goods = 41,

    [Display(Name = "Skills", ShortName = "Title", Description = "")]
    Title_Skill = 42,

    [Display(Name = "Skills", ShortName = "Description", Description = "")]
    Description_Skill = 43,

    [Display(Name = "Weapons Effects", ShortName = "Common", Description = "")]
    Effect_Weapons = 44,

    [Display(Name = "Ash of War", ShortName = "Effect", Description = "")]
    Effect_Ash_of_War = 45,

    [Display(Name = "Goods", ShortName = "Effect", Description = "")]
    Effect_Goods = 46,

    // DLC1
    [Display(Name = "Weapons", ShortName = "Title", Description = "DLC 1")]
    Title_Weapons_DLC1 = 310,

    [Display(Name = "Weapons", ShortName = "Summary", Description = "DLC 1")]
    Summary_Weapons_DLC1 = 311,

    [Display(Name = "Weapons", ShortName = "Description", Description = "DLC 1")]
    Description_Weapons_DLC1 = 312,

    [Display(Name = "Armor", ShortName = "Title", Description = "DLC 1")]
    Title_Armor_DLC1 = 313,

    [Display(Name = "Armor", ShortName = "Summary", Description = "DLC 1")]
    Summary_Armor_DLC1 = 314,

    [Display(Name = "Armor", ShortName = "Description", Description = "DLC 1")]
    Description_Armor_DLC1 = 315,

    [Display(Name = "Accessories", ShortName = "Title", Description = "DLC 1")]
    Title_Accessories_DLC1 = 316,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "DLC 1")]
    Summary_Accessories_DLC1 = 317,

    [Display(Name = "Accessories", ShortName = "Description", Description = "DLC 1")]
    Description_Accessories_DLC1 = 318,

    [Display(Name = "Goods", ShortName = "Title", Description = "DLC 1")]
    Title_Goods_DLC1 = 319,

    [Display(Name = "Goods", ShortName = "Summary", Description = "DLC 1")]
    Summary_Goods_DLC1 = 320,

    [Display(Name = "Goods", ShortName = "Description", Description = "DLC 1")]
    Description_Goods_DLC1 = 321,

    [Display(Name = "Ash of War", ShortName = "Title", Description = "DLC 1")]
    Title_Ash_of_War_DLC1 = 322,

    [Display(Name = "Ash of War", ShortName = "Summary", Description = "DLC 1")]
    Summary_Ash_of_War_DLC1 = 323,

    [Display(Name = "Ash of War", ShortName = "Description", Description = "DLC 1")]
    Description_Ash_of_War_DLC1 = 324,

    [Display(Name = "Magic", ShortName = "Title", Description = "DLC 1")]
    Title_Magic_DLC1 = 325,

    [Display(Name = "Magic", ShortName = "Summary", Description = "DLC 1")]
    Summary_Magic_DLC1 = 326,

    [Display(Name = "Magic", ShortName = "Description", Description = "DLC 1")]
    Description_Magic_DLC1 = 327,

    [Display(Name = "Characters", ShortName = "Common", Description = "DLC 1")]
    Title_Characters_DL1 = 328,

    [Display(Name = "Locations", ShortName = "Common", Description = "DLC 1")]
    Title_Locations_DL1 = 329,

    [Display(Name = "Goods Dialog", ShortName = "Common", Description = "DLC 1")]
    Dialog_Goods_DLC1 = 330,

    [Display(Name = "Skills", ShortName = "Title", Description = "DLC 1")]
    Title_Skill_DLC1 = 331,

    [Display(Name = "Skills", ShortName = "Description", Description = "DLC 1")]
    Description_Skill_DLC1 = 332,

    [Display(Name = "Weapon Effects", ShortName = "Common", Description = "DLC 1")]
    Effect_Weapons_DLC1 = 333,

    [Display(Name = "Ash of War", ShortName = "Effect", Description = "DLC 1")]
    Effect_Ash_of_War_DLC1 = 334,

    [Display(Name = "Goods", ShortName = "Effect", Description = "DLC 1")]
    Effect_Goods_DLC1 = 335,

    // DLC2
    [Display(Name = "Weapons", ShortName = "Title", Description = "DLC 2")]
    Title_Weapons_DLC2 = 410,

    [Display(Name = "Weapons", ShortName = "Summary", Description = "DLC 2")]
    Summary_Weapons_DLC2 = 411,

    [Display(Name = "Weapons", ShortName = "Description", Description = "DLC 2")]
    Description_Weapons_DLC2 = 412,

    [Display(Name = "Armor", ShortName = "Title", Description = "DLC 2")]
    Title_Armor_DLC2 = 413,

    [Display(Name = "Armor", ShortName = "Summary", Description = "DLC 2")]
    Summary_Armor_DLC2 = 414,

    [Display(Name = "Armor", ShortName = "Description", Description = "DLC 2")]
    Description_Armor_DLC2 = 415,

    [Display(Name = "Accessories", ShortName = "Title", Description = "DLC 2")]
    Title_Accessories_DLC2 = 416,

    [Display(Name = "Accessories", ShortName = "Summary", Description = "DLC 2")]
    Summary_Accessories_DLC2 = 417,

    [Display(Name = "Accessories", ShortName = "Description", Description = "DLC 2")]
    Description_Accessories_DLC2 = 418,

    [Display(Name = "Goods", ShortName = "Title", Description = "DLC 2")]
    Title_Goods_DLC2 = 419,

    [Display(Name = "Goods", ShortName = "Summary", Description = "DLC 2")]
    Summary_Goods_DLC2 = 420,

    [Display(Name = "Goods", ShortName = "Description", Description = "DLC 2")]
    Description_Goods_DLC2 = 421,

    [Display(Name = "Ash of War", ShortName = "Title", Description = "DLC 2")]
    Title_Ash_of_War_DLC2 = 422,

    [Display(Name = "Ash of War", ShortName = "Summary", Description = "DLC 2")]
    Summary_Ash_of_War_DLC2 = 423,

    [Display(Name = "Ash of War", ShortName = "Description", Description = "DLC 2")]
    Description_Ash_of_War_DLC2 = 424,

    [Display(Name = "Magic", ShortName = "Title", Description = "DLC 2")]
    Title_Magic_DLC2 = 425,

    [Display(Name = "Magic", ShortName = "Summary", Description = "DLC 2")]
    Summary_Magic_DLC2 = 426,

    [Display(Name = "Magic", ShortName = "Description", Description = "DLC 2")]
    Description_Magic_DLC2 = 427,

    [Display(Name = "Characters", ShortName = "Common", Description = "DLC 2")]
    Title_Characters_DLC2 = 428,

    [Display(Name = "Locations", ShortName = "Common", Description = "DLC 2")]
    Title_Locations_DLC2 = 429,

    [Display(Name = "Goods Dialog", ShortName = "Common", Description = "DLC 2")]
    Dialog_Goods_DLC2 = 430,

    [Display(Name = "Skills", ShortName = "Title", Description = "DLC 2")]
    Title_Skill_DLC2 = 431,

    [Display(Name = "Skills", ShortName = "Description", Description = "DLC 2")]
    Description_Skill_DLC2 = 432,

    [Display(Name = "Weapon Effects", ShortName = "Common", Description = "DLC 2")]
    Effect_Weapons_DLC2 = 433,

    [Display(Name = "Ash of War", ShortName = "Effect", Description = "DLC 2")]
    Effect_Ash_of_War_DLC2 = 434,

    [Display(Name = "Goods", ShortName = "Effect", Description = "DLC 2")]
    Effect_Goods_DLC2 = 435,
}

public enum Menu_MsgBndID_ER
{
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "")]
    Talk_Messages = 1,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "")]
    Blood_Messages = 2,

    [Display(Name = "Movie Subtitles", ShortName = "Common", Description = "")]
    Movie_Subtitles = 3,

    [Display(Name = "Talk Messages (Female)", ShortName = "Common", Description = "")]
    Talk_Messages_FemalePC = 4,

    [Display(Name = "Network Messages", ShortName = "Common", Description = "")]
    Network_Messages = 31,

    [Display(Name = "Action Button Text", ShortName = "Common", Description = "")]
    Action_Button_Text = 32,

    [Display(Name = "Event Text for Talk", ShortName = "Common", Description = "")]
    Event_Text_for_Talk = 33,

    [Display(Name = "Event Text for Map", ShortName = "Common", Description = "")]
    Event_Text_for_Map = 34,

    [Display(Name = "Menu Text", ShortName = "Common", Description = "")]
    Modern_Menu_Text = 200,

    [Display(Name = "Line Help", ShortName = "Common", Description = "")]
    Modern_Menu_Line_Help = 201,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "")]
    Modern_Menu_Key_Guide = 202,

    [Display(Name = "System Messages: PC", ShortName = "Common", Description = "")]
    Modern_Menu_System_Message = 203,

    [Display(Name = "Dialogue", ShortName = "Common", Description = "")]
    Modern_Menu_Dialogue = 204,

    [Display(Name = "Loading Screen", ShortName = "Title", Description = "")]
    Loading_Title = 205,

    [Display(Name = "Loading Screen", ShortName = "Description", Description = "")]
    Loading_Text = 206,

    [Display(Name = "Tutorial", ShortName = "Title", Description = "")]
    Tutorial_Title = 207,

    [Display(Name = "Tutorial", ShortName = "Description", Description = "")]
    Tutorial_Text = 208,

    [Display(Name = "Text Embed Image Name", ShortName = "Common", Description = "")]
    Text_Embed_Image_Name_PC = 209,

    [Display(Name = "Terms of Service", ShortName = "Common", Description = "")]
    Terms_of_Service = 210,

    // DLC1
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "DLC 1")]
    Talk_Messages_DLC1 = 360,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "DLC 1")]
    Blood_Messages_DLC1 = 361,

    [Display(Name = "Movie Subtitles", ShortName = "Common", Description = "DLC 1")]
    Movie_Subtitles_DLC1 = 362,

    [Display(Name = "Talk Messages (Female)", ShortName = "Common", Description = "DLC 1")]
    Talk_Messages_FemalePC_DLC1 = 363,

    [Display(Name = "Network Messages", ShortName = "Common", Description = "DLC 1")]
    Network_Messages_DLC1 = 364,

    [Display(Name = "Action Button Text", ShortName = "Common", Description = "DLC 1")]
    Action_Button_Text_DLC1 = 365,

    [Display(Name = "Event Text for Talk", ShortName = "Common", Description = "DLC 1")]
    Event_Text_for_Talk_DLC1 = 366,

    [Display(Name = "Event Text for Map", ShortName = "Common", Description = "DLC 1")]
    Event_Text_for_Map_DLC1 = 367,

    [Display(Name = "Menu Text", ShortName = "Common", Description = "DLC 1")]
    Modern_Menu_Text_DLC1 = 368,

    [Display(Name = "Line Help", ShortName = "Common", Description = "DLC 1")]
    Modern_Menu_Line_Help_DLC1 = 369,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "DLC 1")]
    Modern_Menu_Key_Guide_DLC1 = 370,

    [Display(Name = "System Message: PC", ShortName = "Common", Description = "DLC 1")]
    Modern_Menu_System_Message_DLC1 = 371,

    [Display(Name = "Dialogue", ShortName = "Common", Description = "DLC 1")]
    Modern_Menu_Dialogue_DLC1 = 372,

    [Display(Name = "Loading Screen", ShortName = "Title", Description = "DLC 1")]
    Loading_Title_DLC1 = 373,

    [Display(Name = "Loading Screen", ShortName = "Description", Description = "DLC 1")]
    Loading_Text_DLC1 = 374,

    [Display(Name = "Tutorial", ShortName = "Title", Description = "DLC 1")]
    Tutorial_Title_DLC1 = 375,

    [Display(Name = "Tutorial Text", ShortName = "Description", Description = "DLC 1")]
    Tutorial_Text_DLC1 = 376,

    // DLC2
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "DLC 2")]
    Talk_Messages_DLC2 = 460,

    [Display(Name = "Blood Messages", ShortName = "Common", Description = "DLC 2")]
    Blood_Messages_DLC2 = 461,

    [Display(Name = "Movie Subtitles", ShortName = "Common", Description = "DLC 2")]
    Movie_Subtitles_DLC2 = 462,

    [Display(Name = "Talk Messages (Female)", ShortName = "Common", Description = "DLC 2")]
    Talk_Messages_FemalePC_DLC2 = 463,

    [Display(Name = "Network Messages", ShortName = "Common", Description = "DLC 2")]
    Network_Messages_DLC2 = 464,

    [Display(Name = "Action Button Text", ShortName = "Common", Description = "DLC 2")]
    Action_Button_Text_DLC2 = 465,

    [Display(Name = "Event Text for Talk", ShortName = "Common", Description = "DLC 2")]
    Event_Text_for_Talk_DLC2 = 466,

    [Display(Name = "Event Text for Map", ShortName = "Common", Description = "DLC 2")]
    Event_Text_for_Map_DLC2 = 467,

    [Display(Name = "Menu Text", ShortName = "Common", Description = "DLC 2")]
    Modern_Menu_Text_DLC2 = 468,

    [Display(Name = "Line Help", ShortName = "Common", Description = "DLC 2")]
    Modern_Menu_Line_Help_DLC2 = 469,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "DLC 2")]
    Modern_Menu_Key_Guide_DLC2 = 470,

    [Display(Name = "System Message: PC", ShortName = "Common", Description = "DLC 2")]
    Modern_Menu_System_Message_DLC2 = 471,

    [Display(Name = "Dialogue", ShortName = "Common", Description = "DLC 2")]
    Modern_Menu_Dialogue_DLC2 = 472,

    [Display(Name = "Loading Screen", ShortName = "Title", Description = "DLC 2")]
    Loading_Title_DLC2 = 473,

    [Display(Name = "Loading Screen", ShortName = "Description", Description = "DLC 2")]
    Loading_Text_DLC2 = 474,

    [Display(Name = "Tutorial", ShortName = "Title", Description = "DLC 2")]
    Tutorial_Title_DLC2 = 475,

    [Display(Name = "Tutorial", ShortName = "Description", Description = "DLC 2")]
    Tutorial_Text_DLC2 = 476,
}

public enum NgWord_MsgBndID_ER
{
    [Display(Name = "Blocked Words", ShortName = "Common", Description = "")]
    NgWord = 1
}

public enum SellRegion_MsgBndID_ER
{
    [Display(Name = "Sell Region", ShortName = "Common", Description = "")]
    SellRegion = 300
}

/// <summary>
/// FMG IDs for AC6
/// </summary>
public enum Item_MsgBndID_AC6
{
    [Display(Name = "Goods", ShortName = "Title", Description = "")]
    Title_Goods = 10,

    [Display(Name = "Weapons", ShortName = "Title", Description = "")]
    Title_Weapons = 11,

    [Display(Name = "Armor", ShortName = "Title", Description = "")]
    Title_Armor = 12,

    [Display(Name = "Characters", ShortName = "Common", Description = "")]
    Title_Characters = 18,

    [Display(Name = "Weapons", ShortName = "Description", Description = "")]
    Description_Weapons = 21,

    [Display(Name = "Armor", ShortName = "Description", Description = "")]
    Description_Armor = 22,

    [Display(Name = "Generator", ShortName = "Title", Description = "")]
    Title_Generator = 35,

    [Display(Name = "Generator", ShortName = "Description", Description = "")]
    Description_Generator = 36,

    [Display(Name = "Booster", ShortName = "Title", Description = "")]
    Title_Booster = 38,

    [Display(Name = "Booster", ShortName = "Description", Description = "")]
    Description_Booster = 39,

    [Display(Name = "FCS", ShortName = "Title", Description = "")]
    Title_FCS = 41,

    [Display(Name = "FCS", ShortName = "Description", Description = "")]
    Description_FCS = 42,

}

public enum Menu_MsgBndID_AC6
{
    [Display(Name = "Talk Messages", ShortName = "Common", Description = "")]
    Talk_Messages = 1,

    [Display(Name = "Movie Subtitles", ShortName = "Common", Description = "")]
    Movie_Subtitles = 3,

    [Display(Name = "Event Text", ShortName = "Common", Description = "")]
    Event_Text = 30,

    [Display(Name = "Ranker Profile", ShortName = "Common", Description = "")]
    Ranker_Profile = 50,

    [Display(Name = "Mission Name", ShortName = "Title", Description = "")]
    Mission_Name = 60,

    [Display(Name = "Mission Overview", ShortName = "Description", Description = "")]
    Mission_Overview = 61,

    [Display(Name = "Mission Objective", ShortName = "Summary", Description = "")]
    Mission_Objective = 62,

    [Display(Name = "Mission Location Name", ShortName = "Common", Description = "")]
    Mission_Location_Name = 63,

    [Display(Name = "Archive", ShortName = "Title", Description = "")]
    Archive_Name = 65,

    [Display(Name = "Archive", ShortName = "Description", Description = "")]
    Archive_Content = 66,

    [Display(Name = "Tutorial", ShortName = "Title", Description = "")]
    Tutorial_Title = 73,

    [Display(Name = "Tutorial", ShortName = "Description", Description = "")]
    Tutorial_Text = 74,

    [Display(Name = "Menu Text", ShortName = "Common", Description = "")]
    Modern_Menu_Text = 200,

    [Display(Name = "Line Help", ShortName = "Common", Description = "")]
    Modern_Menu_Line_Help = 201,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "")]
    Modern_Menu_Key_Guide = 202,

    [Display(Name = "System Message: PC", ShortName = "Common", Description = "")]
    Modern_Menu_System_Message = 203,

    [Display(Name = "Dialogue", ShortName = "Common", Description = "")]
    Modern_Menu_Dialogue = 204,

    [Display(Name = "Item Help", ShortName = "Common", Description = "")]
    Item_Help = 205,

    [Display(Name = "Loading Screen", ShortName = "Title", Description = "")]
    Loading_Title = 207,

    [Display(Name = "Loading Screen", ShortName = "Description", Description = "")]
    Loading_Text = 208,

    [Display(Name = "Text Embed Image Name", ShortName = "Common", Description = "")]
    Text_Embed_Image_Name_PC = 210

}

public enum NgWord_MsgBndID_AC6
{
    [Display(Name = "Blocked Words", ShortName = "Common", Description = "")]
    NgWord = 1
}

// DS2 doesn't use msgbnds, so FMGs are matched via name.
public enum CommonFmgName_DS2
{
    [Display(Name = "Blood Message Conjunction", ShortName = "Common", Description = "")]
    bloodmessageconjunction,

    [Display(Name = "Blood Message Sentence", ShortName = "Common", Description = "")]
    bloodmessagesentence,

    [Display(Name = "Blood Message Word", ShortName = "Common", Description = "")]
    bloodmessageword,

    [Display(Name = "Blood Message Word Category", ShortName = "Common", Description = "")]
    bloodmessagewordcategory,

    [Display(Name = "Bonfire", ShortName = "Common", Description = "")]
    bofire,

    [Display(Name = "Bonfire Name", ShortName = "Common", Description = "")]
    bonfirename,

    [Display(Name = "Character Maker", ShortName = "Common", Description = "")]
    charamaking,

    [Display(Name = "Common", ShortName = "Common", Description = "")]
    common,

    [Display(Name = "Multiplayer", ShortName = "Common", Description = "")]
    dconlymessage,

    [Display(Name = "Item Descriptions", ShortName = "Common", Description = "")]
    detailedexplanation,

    [Display(Name = "Icon Help", ShortName = "Common", Description = "")]
    iconhelp,

    [Display(Name = "In-game Menu", ShortName = "Common", Description = "")]
    ingamemenu,

    [Display(Name = "In-game System", ShortName = "Common", Description = "")]
    ingamesystem,

    [Display(Name = "Item Names", ShortName = "Common", Description = "")]
    itemname,

    [Display(Name = "Key Guide", ShortName = "Common", Description = "")]
    keyguide,

    [Display(Name = "Map Event", ShortName = "Common", Description = "")]
    mapevent,

    [Display(Name = "Map Name", ShortName = "Common", Description = "")]
    mapname,

    [Display(Name = "NPC Menu", ShortName = "Common", Description = "")]
    npcmenu,

    [Display(Name = "Plural Select", ShortName = "Common", Description = "")]
    pluralselect,

    [Display(Name = "Prologue", ShortName = "Common", Description = "")]
    prologue,

    [Display(Name = "Shop", ShortName = "Common", Description = "")]
    shop,

    [Display(Name = "Item Summaries", ShortName = "Common", Description = "")]
    simpleexplanation,

    [Display(Name = "Staff Roll", ShortName = "Common", Description = "")]
    staffroll,

    [Display(Name = "Title Flow", ShortName = "Common", Description = "")]
    titleflow,

    [Display(Name = "Title Menu", ShortName = "Common", Description = "")]
    titlemenu,

    [Display(Name = "Weapon Type", ShortName = "Common", Description = "")]
    weapontype,

    [Display(Name = "Messages: PC", ShortName = "Common", Description = "")]
    win32onlymessage,
}

public enum BloodMessageFmgName_DS2
{
    [Display(Name = "Things Betwixt", ShortName = "Blood Message", Description = "")]
    m10_02_00_00,

    [Display(Name = "Forest of Fallen Giants", ShortName = "Blood Message", Description = "")]
    m10_10_00_00,

    [Display(Name = "Brightstone Cove Tseldora", ShortName = "Blood Message", Description = "")]
    m10_14_00_00,

    [Display(Name = "Aldia's Keep", ShortName = "Blood Message", Description = "")]
    m10_15_00_00,

    [Display(Name = "The Lost Bastille & Belfry Luna", ShortName = "Blood Message", Description = "")]
    m10_16_00_00,

    [Display(Name = "Harvest Valley & Earthen Peak", ShortName = "Blood Message", Description = "")]
    m10_17_00_00,

    [Display(Name = "No-man's Wharf", ShortName = "Blood Message", Description = "")]
    m10_18_00_00,

    [Display(Name = "Iron Keep & Belfry Sol", ShortName = "Blood Message", Description = "")]
    m10_19_00_00,

    [Display(Name = "Huntsman's Copse & Undead Purgatory", ShortName = "Blood Message", Description = "")]
    m10_23_00_00,

    [Display(Name = "Dragon Aerie & Dragon Shrine", ShortName = "Blood Message", Description = "")]
    m10_27_00_00,

    [Display(Name = "Memory of Vammar, Orro, and Jeigh", ShortName = "Blood Message", Description = "")]
    m20_10_00_00,

    [Display(Name = "Shrine of Amana", ShortName = "Blood Message", Description = "")]
    m20_11_00_00,

    [Display(Name = "Drangleic Castle & Throne of Want", ShortName = "Blood Message", Description = "")]
    m20_21_00_00,

    [Display(Name = "Undead Crypt", ShortName = "Blood Message", Description = "")]
    m20_24_00_00,

    [Display(Name = "Shulva, Sanctum City", ShortName = "Blood Message", Description = "")]
    m50_35_00_00,

    [Display(Name = "Brume Tower", ShortName = "Blood Message", Description = "")]
    m50_36_00_00,

    [Display(Name = "Frozen Eleum Loyce", ShortName = "Blood Message", Description = "")]
    m50_37_00_00
}

public enum TalkFmgName_DS2
{
    [Display(Name = "Character Names", ShortName = "Common", Description = "")]
    charaname,

    [Display(Name = "Things Betwixt", ShortName = "Talk", Description = "")]
    m10_02_00_00,

    [Display(Name = "Majula", ShortName = "Talk", Description = "")]
    m10_04_00_00,

    [Display(Name = "Forest of Fallen Giants", ShortName = "Talk", Description = "")]
    m10_10_00_00,

    [Display(Name = "Brightstone Cove Tseldora", ShortName = "Talk", Description = "")]
    m10_14_00_00,

    [Display(Name = "Aldia's Keep", ShortName = "Talk", Description = "")]
    m10_15_00_00,

    [Display(Name = "The Lost Bastille & Belfry Luna", ShortName = "Talk", Description = "")]
    m10_16_00_00,

    [Display(Name = "Harvest Valley & Earthen Peak", ShortName = "Talk", Description = "")]
    m10_17_00_00,

    [Display(Name = "No-man's Wharf", ShortName = "Talk", Description = "")]
    m10_18_00_00,

    [Display(Name = "Iron Keep & Belfry Sol", ShortName = "Talk", Description = "")]
    m10_19_00_00,

    [Display(Name = "Huntsman's Copse & Undead Purgatory", ShortName = "Talk", Description = "")]
    m10_23_00_00,

    [Display(Name = "The Gutter & Black Gulch", ShortName = "Talk", Description = "")]
    m10_25_00_00,

    [Display(Name = "Dragon Aerie & Dragon Shrine", ShortName = "Talk", Description = "")]
    m10_27_00_00,

    [Display(Name = "Majula to Shaded Woods", ShortName = "Talk", Description = "")]
    m10_29_00_00,

    [Display(Name = "Heide's Tower to No-man's Wharf", ShortName = "Talk", Description = "")]
    m10_31_00_00,

    [Display(Name = "Shaded Woods & Shrine of Winter", ShortName = "Talk", Description = "")]
    m10_32_00_00,

    [Display(Name = "Doors of Pharros", ShortName = "Talk", Description = "")]
    m10_33_00_00,

    [Display(Name = "Grave of Saints", ShortName = "Talk", Description = "")]
    m10_34_00_00,

    [Display(Name = "Memory of Vammar, Orro, and Jeigh", ShortName = "Talk", Description = "")]
    m20_10_00_00,

    [Display(Name = "Shrine of Amana", ShortName = "Talk", Description = "")]
    m20_11_00_00,

    [Display(Name = "Drangleic Castle & Throne of Want", ShortName = "Talk", Description = "")]
    m20_21_00_00,

    [Display(Name = "Undead Crypt", ShortName = "Talk", Description = "")]
    m20_24_00_00,

    [Display(Name = "Shulva, Sanctum City", ShortName = "Talk", Description = "")]
    m50_35_00_00,

    [Display(Name = "Brume Tower", ShortName = "Talk", Description = "")]
    m50_36_00_00,

    [Display(Name = "Frozen Eleum Loyce", ShortName = "Talk", Description = "")]
    m50_37_00_00,

    [Display(Name = "Memory of the King", ShortName = "Talk", Description = "")]
    m50_38_00_00
}

public enum ContainerSubCategory
{
    [Display(Name = "None")] None, // Used by all non-DS2 files
    [Display(Name = "Common")] common,
    [Display(Name = "Blood Message")] bloodmes,
    [Display(Name = "Talk")] talk,
}