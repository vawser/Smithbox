using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// ----------------------------------
/// Keep the enum names consistent between each game as the ParamMETA 
/// uses them to link the FmgRef to the actual FMG.
/// ----------------------------------

/// <summary>
/// FMG IDs for DS1
/// </summary>
public enum Item_MsgBndID_DS1
{
    [Display(Name= "Title: Goods")] Title_Goods = 10,
    [Display(Name = "Title: Weapons")] Title_Weapons = 11,
    [Display(Name = "Title: Armor")] Title_Armor = 12,
    [Display(Name = "Title: Accessories")] Title_Accessories = 13,
    [Display(Name = "Title: Magic")] Title_Magic = 14,
    [Display(Name = "Title: Test")] Title_Test = 15,
    [Display(Name = "Title: Test 2")] Title_Test_2 = 16,
    [Display(Name = "Title: Test 3")] Title_Test_3 = 17,
    [Display(Name = "Title: Characters")] Title_Characters = 18,
    [Display(Name = "Title: Locations")] Title_Locations = 19,

    [Display(Name = "Summary: Goods")] Summary_Goods = 20,
    [Display(Name = "Summary: Weapons")] Summary_Weapons = 21,
    [Display(Name = "Summary: Armor")] Summary_Armor = 22,
    [Display(Name = "Summary: Accessories")] Summary_Accessories = 23,
    [Display(Name = "Summary: Magic")] Summary_Magic = 28,

    [Display(Name = "Description: Goods")] Description_Goods = 24,
    [Display(Name = "Description: Weapons")] Description_Weapons = 25,
    [Display(Name = "Description: Armor")] Description_Armor = 26,
    [Display(Name = "Description: Accessories")] Description_Accessories = 27,
    [Display(Name = "Description: Magic")] Description_Magic = 29,

    // DLC
    [Display(Name = "[DLC] Title: Goods")] Title_Goods_DLC = 111,
    [Display(Name = "[DLC] Title: Weapons")] Title_Weapons_DLC = 115,
    [Display(Name = "[DLC] Title: Armor")] Title_Armor_DLC = 117,
    [Display(Name = "[DLC] Title: Accessories")] Title_Accessories_DLC = 113,
    [Display(Name = "[DLC] Title: Magic")] Title_Magic_DLC = 118,
    [Display(Name = "[DLC] Title: Characters")] Title_Characters_DLC = 119,
    [Display(Name = "[DLC] Title: Locations")] Title_Locations_DLC = 120,

    [Display(Name = "[DLC] Summary: Goods")] Summary_Goods_DLC = 110,
    [Display(Name = "[DLC] Summary: Weapons")] Summary_Weapons_DLC = 114,
    [Display(Name = "[DLC] Summary: Armor")] Summary_Armor_DLC = 116,
    [Display(Name = "[DLC] Summary: Accessories")] Summary_Accessories_DLC = 112,

    [Display(Name = "[DLC] Description: Goods")] Description_Goods_DLC = 100,
    [Display(Name = "[DLC] Description: Weapons")] Description_Weapons_DLC = 106,
    [Display(Name = "[DLC] Description: Armor")] Description_Armor_DLC = 108,
    [Display(Name = "[DLC] Description: Accessories")] Description_Accessories_DLC = 109,
    [Display(Name = "[DLC] Description: Magic")] Description_Magic_DLC = 105
}

public enum Menu_MsgBndID_DS1
{
    [Display(Name = "Talk Messages")] Talk_Messages = 1,
    [Display(Name = "Blood Messages")] Blood_Messages = 2,
    [Display(Name = "Movie Subtitles")] Movie_Subtitles = 3,
    [Display(Name = "Event Text")] Event_Text = 30,

    [Display(Name = "Menu: In-game")] Menu_Ingame = 70,
    [Display(Name = "Menu: Common")] Menu_Common = 76,
    [Display(Name = "Menu: Other")] Menu_Other = 77,
    [Display(Name = "Menu: Dialog")] Menu_Dialog = 78,
    [Display(Name = "Menu: Key Guide")] Menu_Key_Guide = 79,
    [Display(Name = "Menu: Line Help")] Menu_Line_Help = 80,
    [Display(Name = "Menu: Context")] Menu_Context = 81,
    [Display(Name = "Menu: Tags")] Menu_Tags = 90,

    [Display(Name = "Windows: Tags")] Win32_Tags = 91,
    [Display(Name = "Windows: Messages")] Win32_Messages = 92,

    // DLC
    [Display(Name = "[DLC] Talk Messages")] Talk_Messages_DLC = 104,
    [Display(Name = "[DLC] Blood Messages")] Blood_Messages_DLC = 107,
    [Display(Name = "[DLC] Event Text")] Event_Text_DLC = 101,

    [Display(Name = "[DLC] Menu: Common")] Menu_Common_DLC = 124,
    [Display(Name = "[DLC] Menu: Other")] Menu_Other_DLC = 123,
    [Display(Name = "[DLC] Menu: Dialog")] Menu_Dialog_DLC = 102,
    [Display(Name = "[DLC] Menu: Key Guide")] Menu_Key_Guide_DLC = 122,
    [Display(Name = "[DLC] Menu: Line Help")] Menu_Line_Help_DLC = 121,

    [Display(Name = "[DLC] Windows: Messages")] Win32_Messages_DLC = 103,
}

/// <summary>
/// FMG IDs for BB
/// </summary>
public enum Item_MsgBndID_BB
{
    [Display(Name = "Title: Goods")] Title_Goods = 10,
    [Display(Name = "Title: Weapons")] Title_Weapons = 11,
    [Display(Name = "Title: Armor")] Title_Armor = 12,
    [Display(Name = "Title: Accessories")] Title_Accessories = 13,
    [Display(Name = "Title: Magic")] Title_Magic = 14,
    [Display(Name = "Title: Characters")] Title_Characters = 18,
    [Display(Name = "Title: Locations")] Title_Locations = 19,

    [Display(Name = "Summary: Goods")] Summary_Goods = 20,
    [Display(Name = "Summary: Weapons")] Summary_Weapons = 21,
    [Display(Name = "Summary: Armor")] Summary_Armor = 22,
    [Display(Name = "Summary: Accessories")] Summary_Accessories = 23,
    [Display(Name = "Summary: Magic")] Summary_Magic = 28,

    [Display(Name = "Description: Goods")] Description_Goods = 24,
    [Display(Name = "Description: Weapons")] Description_Weapons = 25,
    [Display(Name = "Description: Armor")] Description_Armor = 26,
    [Display(Name = "Description: Accessories")] Description_Accessories = 27,
    [Display(Name = "Description: Magic")] Description_Magic = 29
}

public enum Menu_MsgBndID_BB
{
    [Display(Name = "Talk Messages")] Talk_Messages = 1,
    [Display(Name = "Blood Messages")] Blood_Messages = 2,
    [Display(Name = "Movie Subtitles")] Movie_Subtitles = 3,
    [Display(Name = "Causes of Death")] Death_Causes = 4,

    [Display(Name = "Event Text")] Event_Text = 30,
    [Display(Name = "Title: Blood Gem")] Title_Blood_Gem = 31,
    [Display(Name = "Summary Blood Gem")] Summary_Blood_Gem = 32,
    [Display(Name = "Description: Blood Gem")] Description_Blood_Gem = 33,
    [Display(Name = "Prefix: Blood Gem")] Prefix_Blood_Gem = 34,
    [Display(Name = "Effect: Blood Gem")] Effect_Blood_Gem = 35,

    [Display(Name = "Menu: In-game")] Menu_Ingame = 70,
    [Display(Name = "Menu: Common")] Menu_Common = 76,
    [Display(Name = "Menu: Other")] Menu_Other = 77,
    [Display(Name = "Menu: Dialog")] Menu_Dialog = 78,
    [Display(Name = "Menu: Key Guide")] Menu_Key_Guide = 79,
    [Display(Name = "Menu: Line Help")] Menu_Line_Help = 80,
    [Display(Name = "Menu: Tags")] Menu_Tags = 90,

    [Display(Name = "Windows: Tags")] Win32_Tags = 91,
    [Display(Name = "Windows: Messages")] Win32_Messages = 92,

    [Display(Name = "Menu Text")] Modern_Menu_Text = 200,
    [Display(Name = "Line Help")] Modern_Menu_Line_Help = 201,
    [Display(Name = "Key Guide")] Modern_Menu_Key_Guide = 202,
    [Display(Name = "System Message")] Modern_Menu_System_Message = 203,
    [Display(Name = "Dialogue")] Modern_Menu_Dialogue = 204
}

/// <summary>
/// FMG IDs for DS3
/// </summary>
public enum Item_MsgBndID_DS3
{
    [Display(Name = "Title: Goods")] Title_Goods = 10,
    [Display(Name = "Title: Weapons")] Title_Weapons = 11,
    [Display(Name = "Title: Armor")] Title_Armor = 12,
    [Display(Name = "Title: Accessories")] Title_Accessories = 13,
    [Display(Name = "Title: Magic")] Title_Magic = 14,
    [Display(Name = "Title: Characters")] Title_Characters = 18,
    [Display(Name = "Title: Locations")] Title_Locations = 19,

    [Display(Name = "Summary: Goods")] Summary_Goods = 20,
    [Display(Name = "Summary: Weapons")] Summary_Weapons = 21,
    [Display(Name = "Summary: Armor")] Summary_Armor = 22,
    [Display(Name = "Summary: Accessories")] Summary_Accessories = 23,
    [Display(Name = "Summary: Magic")] Summary_Magic = 28,

    [Display(Name = "Description: Goods")] Description_Goods = 24,
    [Display(Name = "Description: Weapons")] Description_Weapons = 25,
    [Display(Name = "Description: Armor")] Description_Armor = 26,
    [Display(Name = "Description: Accessories")] Description_Accessories = 27,
    [Display(Name = "Description: Magic")] Description_Magic = 29,

    // DLC1
    [Display(Name = "[DLC1] Title: Goods")] Title_Goods_DLC1 = 210,
    [Display(Name = "[DLC1] Title: Weapons")] Title_Weapons_DLC1 = 211,
    [Display(Name = "[DLC1] Title: Armor")] Title_Armor_DLC1 = 212,
    [Display(Name = "[DLC1] Title: Accessories")] Title_Accessories_DLC1 = 213,
    [Display(Name = "[DLC1] Title: Magic")] Title_Magic_DLC1 = 214,
    [Display(Name = "[DLC1] Title: Characters")] Title_Character_DLC1 = 215,
    [Display(Name = "[DLC1] Title: Locations")] Title_Locations_DLC1 = 216,
    [Display(Name = "[DLC1] Summary: Goods")] Summary_Goods_DLC1 = 217,
    [Display(Name = "[DLC1] Summary: Accessories")] Summary_Accessories_DLC1 = 220,
    [Display(Name = "[DLC1] Description: Goods")] Description_Goods_DLC1 = 221,
    [Display(Name = "[DLC1] Description: Weapons")] Description_Weapons_DLC1 = 222,
    [Display(Name = "[DLC1] Description: Armor")] Description_Armor_DLC1 = 223,
    [Display(Name = "[DLC1] Description: Accessories")] Description_Accessories_DLC1 = 224,
    [Display(Name = "[DLC1] Summary: Magic")] Summary_Magic_DLC1 = 225,
    [Display(Name = "[DLC1] Description: Magic")] Description_Magic_DLC1 = 226,

    // DLC2
    [Display(Name = "[DLC2] Title: Goods")] Title_Goods_DLC2 = 250,
    [Display(Name = "[DLC2] Title: Weapons")] Title_Weapons_DLC2 = 251,
    [Display(Name = "[DLC2] Title: Armor")] Title_Armor_DLC2 = 252,
    [Display(Name = "[DLC2] Title: Accessories")] Title_Accessories_DLC2 = 253,
    [Display(Name = "[DLC2] Title: Magic")] Title_Magic_DLC2 = 254,
    [Display(Name = "[DLC2] Title: Characters")] Title_Character_DLC2 = 255,
    [Display(Name = "[DLC2] Title: Locations")] Title_Locations_DLC2 = 256,
    [Display(Name = "[DLC2] Summary: Goods")] Summary_Goods_DLC2 = 257,
    [Display(Name = "[DLC2] Summary: Accessories")] Summary_Accessories_DLC2 = 260,
    [Display(Name = "[DLC2] Description: Goods")] Description_Goods_DLC2 = 261,
    [Display(Name = "[DLC2] Description: Weapons")] Description_Weapons_DLC2 = 262,
    [Display(Name = "[DLC2] Description: Armor")] Description_Armor_DLC2 = 263,
    [Display(Name = "[DLC2] Description: Accessories")] Description_Accessories_DLC2 = 264,
    [Display(Name = "[DLC2] Summary: Magic")] Summary_Magic_DLC2 = 265,
    [Display(Name = "[DLC2] Description: Magic")] Description_Magic_DLC2 = 266
}

public enum Menu_MsgBndID_DS3
{
    [Display(Name = "Talk Messages")] Talk_Messages = 1,
    [Display(Name = "Blood Messages")] Blood_Messages = 2,
    [Display(Name = "Movie Subtitles")] Movie_Subtitles = 3,
    [Display(Name = "Event Text")] Event_Text = 30,

    [Display(Name = "Menu: In-game")] Menu_Ingame = 70,
    [Display(Name = "Menu: Common")] Menu_Common = 76,
    [Display(Name = "Menu: Other")] Menu_Other = 77,
    [Display(Name = "Menu: Dialog")] Menu_Dialog = 78,
    [Display(Name = "Menu: Key Guide")] Menu_Key_Guide = 79,
    [Display(Name = "Menu: Line Help")] Menu_Line_Help = 80,
    [Display(Name = "Menu: Context")] Menu_Context = 81,
    [Display(Name = "Menu: Tags")] Menu_Tags = 90,

    [Display(Name = "Windows: Tags")] Win32_Tags = 91,
    [Display(Name = "Windows: Messages")] Win32_Messages = 92,

    [Display(Name = "Menu Text")] Modern_Menu_Text = 200,
    [Display(Name = "Line Help")] Modern_Menu_Line_Help = 201,
    [Display(Name = "Key Guide")] Modern_Menu_Key_Guide = 202,
    [Display(Name = "System Message: PC")] Modern_Menu_System_Message = 203,
    [Display(Name = "Dialogue")] Modern_Menu_Dialogue = 204,
    [Display(Name = "System Message: PS4")] System_Message_PS4 = 205,
    [Display(Name = "System Message: XB1")] System_Message_XB1 = 206,

    // DLC1
    [Display(Name = "[DLC1] Talk Messages")] Talk_Messages_DLC1 = 230,
    [Display(Name = "[DLC1] Event Text")] Event_Text_DLC1 = 231,
    [Display(Name = "[DLC1] Menu Text")] Modern_Menu_Text_DLC1 = 232,
    [Display(Name = "[DLC1] Line Help")] Modern_Menu_Line_Help_DLC1 = 233,
    [Display(Name = "[DLC1] System Message: PC")] Modern_Menu_System_Message_DLC1 = 235,
    [Display(Name = "[DLC1] Dialogue")] Modern_Menu_Dialogue_DLC1 = 236,
    [Display(Name = "[DLC1] System Message: PS4")] System_Message_PS4_DLC1 = 237,
    [Display(Name = "[DLC1] System Message: XB1")] System_Message_XB1_DLC1 = 238,
    [Display(Name = "[DLC1] Blood Messages")] Blood_Messages_DLC1 = 239,

    // DLC2
    [Display(Name = "[DLC2] Talk Messages")] Talk_Messages_DLC2 = 270,
    [Display(Name = "[DLC2] Event Text")] Event_Text_DLC2 = 271,
    [Display(Name = "[DLC2] Menu Text")] Modern_Menu_Text_DLC2 = 272,
    [Display(Name = "[DLC2] Line Help")] Modern_Menu_Line_Help_DLC2 = 273,
    [Display(Name = "[DLC2] System Message: PC")] Modern_Menu_System_Message_DLC2 = 275,
    [Display(Name = "[DLC2] Dialogue")] Modern_Menu_Dialogue_DLC2 = 276,
    [Display(Name = "[DLC2] System Message: PS4")] System_Message_PS4_DLC2 = 277,
    [Display(Name = "[DLC2] System Message: XB1")] System_Message_XB1_DLC2 = 278,
    [Display(Name = "[DLC2] Blood Messages")] Blood_Messages_DLC2 = 279
}

public enum NgWord_MsgBndID_DS3
{
    [Display(Name = "Blocked Words")] NgWord = 1
}

/// <summary>
/// FMG IDs for SDT
/// </summary>
public enum Item_MsgBndID_SDT
{
    [Display(Name = "Title: Goods")] Title_Goods = 10,
    [Display(Name = "Title: Weapons")] Title_Weapons = 11,
    [Display(Name = "Title: Armor")] Title_Armor = 12,
    [Display(Name = "Title: Accessories")] Title_Accessories = 13,
    [Display(Name = "Title: Magic")] Title_Magic = 14,
    [Display(Name = "Title: Characters")] Title_Characters = 18,
    [Display(Name = "Title: Locations")] Title_Locations = 19,

    [Display(Name = "Summary: Goods")] Summary_Goods = 20,
    [Display(Name = "Summary: Weapons")] Summary_Weapons = 21,
    [Display(Name = "Summary: Armor")] Summary_Armor = 22,
    [Display(Name = "Summary: Accessories")] Summary_Accessories = 23,
    [Display(Name = "Summary: Magic")] Summary_Magic = 28,

    [Display(Name = "Description: Goods")] Description_Goods = 24,
    [Display(Name = "Description: Weapons")] Description_Weapons = 25,
    [Display(Name = "Description: Armor")] Description_Armor = 26,
    [Display(Name = "Description: Accessories")] Description_Accessories = 27,
    [Display(Name = "Description: Magic")] Description_Magic = 29,

    [Display(Name = "Description: Combat Skills")] Description_Combat_Skills = 40
}

public enum Menu_MsgBndID_SDT
{
    [Display(Name = "Talk Messages")] Talk_Messages = 1,
    [Display(Name = "Blood Messages")] Blood_Messages = 2,
    [Display(Name = "Movie Subtitles")] Movie_Subtitles = 3,
    [Display(Name = "Event Text")] Event_Text = 30,

    [Display(Name = "Menu: In-game")] Menu_Ingame = 70,
    [Display(Name = "Menu: Common")] Menu_Common = 76,
    [Display(Name = "Menu: Other")] Menu_Other = 77,
    [Display(Name = "Menu: Dialog")] Menu_Dialog = 78,
    [Display(Name = "Menu: Key Guide")] Menu_Key_Guide = 79,
    [Display(Name = "Menu: Line Help")] Menu_Line_Help = 80,
    [Display(Name = "Menu: Context")] Menu_Context = 81,
    [Display(Name = "Menu: Tags")] Menu_Tags = 90,

    [Display(Name = "Windows: Tags")] Win32_Tags = 91,
    [Display(Name = "Windows: Messages")] Win32_Messages = 92,

    [Display(Name = "Menu Text")] Modern_Menu_Text = 200,
    [Display(Name = "Line Help")] Modern_Menu_Line_Help = 201,
    [Display(Name = "Key Guide")] Modern_Menu_Key_Guide = 202,
    [Display(Name = "System Message: PC")] Modern_Menu_System_Message = 203,
    [Display(Name = "Dialogue")] Modern_Menu_Dialogue = 204,
    [Display(Name = "System Message: PS4")] System_Message_PS4 = 205,
    [Display(Name = "System Message: XB1")] System_Message_XB1 = 206
}

public enum SellRegion_MsgBndID_SDT
{
    [Display(Name = "Sell Region")] SellRegion = 300
}

/// <summary>
/// FMG IDs for ER
/// </summary>
public enum Item_MsgBndID_ER 
{
    [Display(Name = "Title: Goods")] Title_Goods = 10,
    [Display(Name = "Title: Weapons")] Title_Weapons = 11,
    [Display(Name = "Title: Armor")] Title_Armor = 12,
    [Display(Name = "Title: Accessories")] Title_Accessories = 13,
    [Display(Name = "Title: Magic")] Title_Magic = 14,
    [Display(Name = "Title: Characters")] Title_Characters = 18,
    [Display(Name = "Title: Locations")] Title_Locations = 19,

    [Display(Name = "Summary: Goods")] Summary_Goods = 20,
    [Display(Name = "Summary: Weapons")] Summary_Weapons = 21,
    [Display(Name = "Summary: Armor")] Summary_Armor = 22,
    [Display(Name = "Summary: Accessories")] Summary_Accessories = 23,
    [Display(Name = "Summary: Magic")] Summary_Magic = 28,

    [Display(Name = "Description: Goods")] Description_Goods = 24,
    [Display(Name = "Description: Weapons")] Description_Weapons = 25,
    [Display(Name = "Description: Armor")] Description_Armor = 26,
    [Display(Name = "Description: Accessories")] Description_Accessories = 27,
    [Display(Name = "Description: Magic")] Description_Magic = 29,

    [Display(Name = "Title: Ash of War")] Title_Ash_of_War = 35,
    [Display(Name = "Summary: Ash of War")] Summary_Ash_of_War = 36,
    [Display(Name = "Description: Ash of War")] Description_Ash_of_War = 37,

    [Display(Name = "Dialog: Goods")] Dialog_Goods = 41,

    [Display(Name = "Title: Skill")] Title_Skill = 42,
    [Display(Name = "Description: Skill")] Description_Skill = 43,

    [Display(Name = "Effect: Weapons")] Effect_Weapons = 44,
    [Display(Name = "Effect: Ash of War")] Effect_Ash_of_War = 45,
    [Display(Name = "Effect: Goods")] Effect_Goods = 46,

    // DLC1
    [Display(Name = "[DLC1] Title: Weapons")] Title_Weapons_DLC1 = 310,
    [Display(Name = "[DLC1] Summary: Weapons")] Summary_Weapons_DLC1 = 311,
    [Display(Name = "[DLC1] Description: Weapons")] Description_Weapons_DLC1 = 312,

    [Display(Name = "[DLC1] Title: Armor")] Title_Armor_DLC1 = 313,
    [Display(Name = "[DLC1] Summary: Armor")] Summary_Armor_DLC1 = 314,
    [Display(Name = "[DLC1] Description: Armor")] Description_Armor_DLC1 = 315,

    [Display(Name = "[DLC1] Title: Accessories")] Title_Accessories_DLC1 = 316,
    [Display(Name = "[DLC1] Summary: Accessories")] Summary_Accessories_DLC1 = 317,
    [Display(Name = "[DLC1] Description: Accessories")] Description_Accessories_DLC1 = 318,

    [Display(Name = "[DLC1] Title: Goods")] Title_Goods_DLC1 = 319,
    [Display(Name = "[DLC1] Summary: Goods")] Summary_Goods_DLC1 = 320,
    [Display(Name = "[DLC1] Description: Goods")] Description_Goods_DLC1 = 321,

    [Display(Name = "[DLC1] Title: Ash of War")] Title_Ash_of_War_DLC1 = 322,
    [Display(Name = "[DLC1] Summary: Ash of War")] Summary_Ash_of_War_DLC1 = 323,
    [Display(Name = "[DLC1] Description: Ash of War")] Description_Ash_of_War_DLC1 = 324,

    [Display(Name = "[DLC1] Title: Magic")] Title_Magic_DLC1 = 325,
    [Display(Name = "[DLC1] Summary: Magic")] Summary_Magic_DLC1 = 326,
    [Display(Name = "[DLC1] Description: Magic")] Description_Magic_DLC1 = 327,

    [Display(Name = "[DLC1] Title: Characters")] Title_Characters_DL1 = 328,
    [Display(Name = "[DLC1] Title: Locations")] Title_Locations_DL1 = 329,

    [Display(Name = "[DLC1] Dialog: Goods")] Dialog_Goods_DLC1 = 330,

    [Display(Name = "[DLC1] Title: Skill")] Title_Skill_DLC1 = 331,
    [Display(Name = "[DLC1] Description: Skill")] Description_Skill_DLC1 = 332,

    [Display(Name = "[DLC1] Effect: Weapons")] Effect_Weapons_DLC1 = 333,
    [Display(Name = "[DLC1] Effect: Ash of War")] Effect_Ash_of_War_DLC1 = 334,
    [Display(Name = "[DLC1] Effect: Goods")] Effect_Goods_DLC1 = 335,

    // DLC2
    [Display(Name = "[DLC2] Title: Weapons")] Title_Weapons_DLC2 = 410,
    [Display(Name = "[DLC2] Summary: Weapons")] Summary_Weapons_DLC2 = 411,
    [Display(Name = "[DLC2] Description: Weapons")] Description_Weapons_DLC2 = 412,

    [Display(Name = "[DLC2] Title: Armor")] Title_Armor_DLC2 = 413,
    [Display(Name = "[DLC2] Summary: Armor")] Summary_Armor_DLC2 = 414,
    [Display(Name = "[DLC2] Description: Armor")] Description_Armor_DLC2 = 415,

    [Display(Name = "[DLC2] Title: Accessories")] Title_Accessories_DLC2 = 416,
    [Display(Name = "[DLC2] Summary: Accessories")] Summary_Accessories_DLC2 = 417,
    [Display(Name = "[DLC2] Description: Accessories")] Description_Accessories_DLC2 = 418,

    [Display(Name = "[DLC2] Title: Goods")] Title_Goods_DLC2 = 419,
    [Display(Name = "[DLC2] Summary: Goods")] Summary_Goods_DLC2 = 420,
    [Display(Name = "[DLC2] Description: Goods")] Description_Goods_DLC2 = 421,

    [Display(Name = "[DLC2] Title: Ash of War")] Title_Ash_of_War_DLC2 = 422,
    [Display(Name = "[DLC2] Summary: Ash of War")] Summary_Ash_of_War_DLC2 = 423,
    [Display(Name = "[DLC2] Description: Ash of War")] Description_Ash_of_War_DLC2 = 424,

    [Display(Name = "[DLC2] Title: Magic")] Title_Magic_DLC2 = 425,
    [Display(Name = "[DLC2] Summary: Magic")] Summary_Magic_DLC2 = 426,
    [Display(Name = "[DLC2] Description: Magic")] Description_Magic_DLC2 = 427,

    [Display(Name = "[DLC2] Title: Characters")] Title_Characters_DL2 = 428,
    [Display(Name = "[DLC2] Title: Locations")] Title_Locations_DL2 = 429,

    [Display(Name = "[DLC2] Dialog: Goods")] Dialog_Goods_DLC2 = 430,

    [Display(Name = "[DLC2] Title: Skill")] Title_Skill_DLC2 = 431,
    [Display(Name = "[DLC2] Description: Skill")] Description_Skill_DLC2 = 432,

    [Display(Name = "[DLC2] Effect: Weapons")] Effect_Weapons_DLC2 = 433,
    [Display(Name = "[DLC2] Effect: Ash of War")] Effect_Ash_of_War_DLC2 = 434,
    [Display(Name = "[DLC2] Effect: Goods")] Effect_Goods_DLC2 = 435,
}

public enum Menu_MsgBndID_ER
{
    [Display(Name = "Talk Messages")] Talk_Messages = 1,
    [Display(Name = "Blood Messages")] Blood_Messages = 2,
    [Display(Name = "Movie Subtitles")] Movie_Subtitles = 3,
    [Display(Name = "Network Messages")] Network_Messages = 31,
    [Display(Name = "Action Button Text")] Action_Button_Text = 32,
    [Display(Name = "Event Text for Talk")] Event_Text_for_Talk = 33,
    [Display(Name = "Event Text for Map")] Event_Text_for_Map = 34,

    [Display(Name = "Menu Text")] Modern_Menu_Text = 200,
    [Display(Name = "Line Help")] Modern_Menu_Line_Help = 201,
    [Display(Name = "Key Guide")] Modern_Menu_Key_Guide = 202,
    [Display(Name = "System Message: PC")] Modern_Menu_System_Message = 203,
    [Display(Name = "Dialogue")] Modern_Menu_Dialogue = 204,
    [Display(Name = "Loading Title")] Loading_Title = 205,
    [Display(Name = "Loading Text")] Loading_Text = 206,
    [Display(Name = "Tutorial Title")] Tutorial_Title = 207,
    [Display(Name = "Tutorial Text")] Tutorial_Text = 208,
    [Display(Name = "Text Embed Image Name")] Text_Embed_Image_Name_PC = 209,
    [Display(Name = "Terms of Service")] Terms_of_Service = 210,

    // DLC1
    [Display(Name = "[DLC1] Talk Messages")] Talk_Messages_DLC1 = 360,
    [Display(Name = "[DLC1] Blood Messages")] Blood_Messages_DLC1 = 361,
    [Display(Name = "[DLC1] Movie Subtitles")] Movie_Subtitles_DLC1 = 362,
    [Display(Name = "[DLC1] Network Messages")] Network_Messages_DLC1 = 364,
    [Display(Name = "[DLC1] Action Button Text")] Action_Button_Text_DLC1 = 365,
    [Display(Name = "[DLC1] Event Text for Talk")] Event_Text_for_Talk_DLC1 = 366,
    [Display(Name = "[DLC1] Event Text for Map")] Event_Text_for_Map_DLC1 = 367,

    [Display(Name = "[DLC1] Menu Text")] Modern_Menu_Text_DLC1 = 368,
    [Display(Name = "[DLC1] Line Help")] Modern_Menu_Line_Help_DLC1 = 369,
    [Display(Name = "[DLC1] Key Guide")] Modern_Menu_Key_Guide_DLC1 = 370,
    [Display(Name = "[DLC1] System Message: PC")] Modern_Menu_System_Message_DLC1 = 371,
    [Display(Name = "[DLC1] Dialogue")] Modern_Menu_Dialogue_DLC1 = 372,
    [Display(Name = "[DLC1] Loading Title")] Loading_Title_DLC1 = 373,
    [Display(Name = "[DLC1] Loading Text")] Loading_Text_DLC1 = 374,
    [Display(Name = "[DLC1] Tutorial Title")] Tutorial_Title_DLC1 = 375,
    [Display(Name = "[DLC1] Tutorial Text")] Tutorial_Text_DLC1 = 376,

    // DLC2
    [Display(Name = "[DLC2] Talk Messages")] Talk_Messages_DLC2 = 460,
    [Display(Name = "[DLC2] Blood Messages")] Blood_Messages_DLC2 = 461,
    [Display(Name = "[DLC2] Movie Subtitles")] Movie_Subtitles_DLC2 = 462,
    [Display(Name = "[DLC2] Network Messages")] Network_Messages_DLC2 = 464,
    [Display(Name = "[DLC2] Action Button Text")] Action_Button_Text_DLC2 = 465,
    [Display(Name = "[DLC2] Event Text for Talk")] Event_Text_for_Talk_DLC2 = 466,
    [Display(Name = "[DLC2] Event Text for Map")] Event_Text_for_Map_DLC2 = 467,

    [Display(Name = "[DLC2] Menu Text")] Modern_Menu_Text_DLC2 = 468,
    [Display(Name = "[DLC2] Line Help")] Modern_Menu_Line_Help_DLC2 = 469,
    [Display(Name = "[DLC2] Key Guide")] Modern_Menu_Key_Guide_DLC2 = 470,
    [Display(Name = "[DLC2] System Message: PC")] Modern_Menu_System_Message_DLC2 = 471,
    [Display(Name = "[DLC2] Dialogue")] Modern_Menu_Dialogue_DLC2 = 472,
    [Display(Name = "[DLC2] Loading Title")] Loading_Title_DLC2 = 473,
    [Display(Name = "[DLC2] Loading Text")] Loading_Text_DLC2 = 474,
    [Display(Name = "[DLC2] Tutorial Title")] Tutorial_Title_DLC2 = 475,
    [Display(Name = "[DLC2] Tutorial Text")] Tutorial_Text_DLC2 = 476,
}

public enum NgWord_MsgBndID_ER
{
    [Display(Name = "Blocked Words")] NgWord = 1
}

public enum SellRegion_MsgBndID_ER
{
    [Display(Name = "Sell Region")] SellRegion = 300
}

/// <summary>
/// FMG IDs for AC6
/// </summary>
public enum Item_MsgBndID_AC6
{
    [Display(Name = "Title: Goods")] Title_Goods = 10,
    [Display(Name = "Title: Weapons")] Title_Weapons = 11,
    [Display(Name = "Title: Armor")] Title_Armor = 12,
    [Display(Name = "Title: Characters")] Title_Characters = 18,

    [Display(Name = "Description: Weapons")] Description_Weapons = 21,
    [Display(Name = "Description: Armor")] Description_Armor = 22,

    [Display(Name = "Title: Generator")] Title_Generator = 35,
    [Display(Name = "Description: Generator")] Description_Generator = 36,

    [Display(Name = "Title: Booster")] Title_Booster = 38,
    [Display(Name = "Description: Booster")] Description_Booster = 39,

    [Display(Name = "Title: FCS")] Title_FCS = 41,
    [Display(Name = "Description: FCS")] Description_FCS = 42,

}

public enum Menu_MsgBndID_AC6
{
    [Display(Name = "Talk Messages")] Talk_Messages = 1,
    [Display(Name = "Movie Subtitles")] Movie_Subtitles = 3,
    [Display(Name = "Event Text")] Event_Text = 30,
    [Display(Name = "Ranker Profile")] Ranker_Profile = 50,
    [Display(Name = "Mission Name")] Mission_Name = 60,
    [Display(Name = "Mission Overview")] Mission_Overview = 61,
    [Display(Name = "Mission Objective")] Mission_Objective = 62,
    [Display(Name = "Mission Location Name")] Mission_Location_Name = 63,
    [Display(Name = "Archive Name")] Archive_Name = 65,
    [Display(Name = "Archive Content")] Archive_Content = 66,

    [Display(Name = "Tutorial Title")] Tutorial_Title = 73,
    [Display(Name = "Tutorial Text")] Tutorial_Text = 74,

    [Display(Name = "Menu Text")] Modern_Menu_Text = 200,
    [Display(Name = "Line Help")] Modern_Menu_Line_Help = 201,
    [Display(Name = "Key Guide")] Modern_Menu_Key_Guide = 202,
    [Display(Name = "System Message: PC")] Modern_Menu_System_Message = 203,
    [Display(Name = "Dialogue")] Modern_Menu_Dialogue = 204,
    [Display(Name = "Item Help")] Item_Help = 205,
    [Display(Name = "Loading Title")] Loading_Title = 207,
    [Display(Name = "Loading Text")] Loading_Text = 208,
    [Display(Name = "Text Embed Image Name")] Text_Embed_Image_Name_PC = 210

}

public enum NgWord_MsgBndID_AC6
{
    [Display(Name = "Blocked Words")] NgWord = 1
}

// DS2 doesn't use msgbnds, so FMGs are matched via name.
public enum CommonFmgName_DS2
{
    [Display(Name = "Blood Message: Conjunction")] bloodmessageconjunction,
    [Display(Name = "Blood Message: Sentence")] bloodmessagesentence,
    [Display(Name = "Blood Message: Word")] bloodmessageword,
    [Display(Name = "Blood Message: Word Category")] bloodmessagewordcategory,
    [Display(Name = "Bonfire")] bofire,
    [Display(Name = "Bonfire Name")] bonfirename,
    [Display(Name = "Character Maker")] charamaking,
    [Display(Name = "Common")] common,
    [Display(Name = "Multiplayer")] dconlymessage,
    [Display(Name = "Descriptions")] detailedexplanation,
    [Display(Name = "Icon Help")] iconhelp,
    [Display(Name = "In-game Menu")] ingamemenu,
    [Display(Name = "In-game System")] ingamesystem,
    [Display(Name = "Item Name")] itemname,
    [Display(Name = "Key Guide")] keyguide,
    [Display(Name = "Map Event")] mapevent,
    [Display(Name = "Map Name")] mapname,
    [Display(Name = "NPC Menu")] npcmenu,
    [Display(Name = "Plural Select")] pluralselect,
    [Display(Name = "Prologue")] prologue,
    [Display(Name = "Shop")] shop,
    [Display(Name = "Summaries")] simpleexplanation,
    [Display(Name = "Staff Roll")] staffroll,
    [Display(Name = "Title Flow")] titleflow,
    [Display(Name = "Title Menu")] titlemenu,
    [Display(Name = "Weapon Type")] weapontype,
    [Display(Name = "Windows: Messages")] win32onlymessage,
}

public enum BloodMessageFmgName_DS2
{
    [Display(Name = "Blood Message: Things Betwixt")] m10_02_00_00,
    [Display(Name = "Blood Message: Forest of Fallen Giants")] m10_10_00_00,
    [Display(Name = "Blood Message: Brightstone Cove Tseldora")] m10_14_00_00,
    [Display(Name = "Blood Message: Aldia's Keep")] m10_15_00_00,
    [Display(Name = "Blood Message: The Lost Bastille & Belfry Luna")] m10_16_00_00,
    [Display(Name = "Blood Message: Harvest Valley & Earthen Peak")] m10_17_00_00,
    [Display(Name = "Blood Message: No-man's Wharf")] m10_18_00_00,
    [Display(Name = "Blood Message: Iron Keep & Belfry Sol")] m10_19_00_00,
    [Display(Name = "Blood Message: Huntsman's Copse & Undead Purgatory")] m10_23_00_00,
    [Display(Name = "Blood Message: Dragon Aerie & Dragon Shrine")] m10_27_00_00,
    [Display(Name = "Blood Message: Memory of Vammar, Orro, and Jeigh")] m20_10_00_00,
    [Display(Name = "Blood Message: Shrine of Amana")] m20_11_00_00,
    [Display(Name = "Blood Message: Drangleic Castle & Throne of Want")] m20_21_00_00,
    [Display(Name = "Blood Message: Undead Crypt")] m20_24_00_00,
    [Display(Name = "Blood Message: Shulva, Sanctum City")] m50_35_00_00,
    [Display(Name = "Blood Message: Brume Tower")] m50_36_00_00,
    [Display(Name = "Blood Message: Frozen Eleum Loyce")] m50_37_00_00
}

public enum TalkFmgName_DS2
{
    [Display(Name = "Character Names")] charaname,
    [Display(Name = "Talk: Things Betwixt")] m10_02_00_00,
    [Display(Name = "Talk: Majula")] m10_04_00_00,
    [Display(Name = "Talk: Forest of Fallen Giants")] m10_10_00_00,
    [Display(Name = "Talk: Brightstone Cove Tseldora")] m10_14_00_00,
    [Display(Name = "Talk: Aldia's Keep")] m10_15_00_00,
    [Display(Name = "Talk: The Lost Bastille & Belfry Luna")] m10_16_00_00,
    [Display(Name = "Talk: Harvest Valley & Earthen Peak")] m10_17_00_00,
    [Display(Name = "Talk: No-man's Wharf")] m10_18_00_00,
    [Display(Name = "Talk: Iron Keep & Belfry Sol")] m10_19_00_00,
    [Display(Name = "Talk: Huntsman's Copse & Undead Purgatory")] m10_23_00_00,
    [Display(Name = "Talk: The Gutter & Black Gulch")] m10_25_00_00,
    [Display(Name = "Talk: Dragon Aerie & Dragon Shrine")] m10_27_00_00,
    [Display(Name = "Talk: Majula to Shaded Woods")] m10_29_00_00,
    [Display(Name = "Talk: Heide's Tower to No-man's Wharf")] m10_31_00_00,
    [Display(Name = "Talk: Shaded Woods & Shrine of Winter")] m10_32_00_00,
    [Display(Name = "Talk: Doors of Pharros")] m10_33_00_00,
    [Display(Name = "Talk: Grave of Saints")] m10_34_00_00,
    [Display(Name = "Talk: Memory of Vammar, Orro, and Jeigh")] m20_10_00_00,
    [Display(Name = "Talk: Shrine of Amana")] m20_11_00_00,
    [Display(Name = "Talk: Drangleic Castle & Throne of Want")] m20_21_00_00,
    [Display(Name = "Talk: Undead Crypt")] m20_24_00_00,
    [Display(Name = "Talk: Shulva, Sanctum City")] m50_35_00_00,
    [Display(Name = "Talk: Brume Tower")] m50_36_00_00,
    [Display(Name = "Talk: Frozen Eleum Loyce")] m50_37_00_00,
    [Display(Name = "Talk: Memory of the King")] m50_38_00_00
}
