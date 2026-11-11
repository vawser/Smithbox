using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.EsdEditor;

public enum EsdRoleKind
{
    None,
    /// An event flag: linked to the EMEVD Editor's Search and this editor's flag search
    EventFlag,
    /// A row of a param (the first candidate the project has)
    Param,
    /// A row of the param an item-type arg of the same call picks (Weapon / Protector / Accessory / Goods...)
    Item,
    /// An entry of an FMG
    Text,
    /// An MSB entity ID (in the talk file's map)
    Entity,
    /// A machine of the same file
    Machine,
}

/// <summary>What an arg's value refers to</summary>
public class EsdRole
{
    public EsdRoleKind Kind;

    /// For Param: candidate params, best first
    public List<string> Params = new();

    /// For Text: the FMG's internal name in the Text Editor (TextUtils.GetFmgInternalName; it matches by Contains, so
    /// the DLC FMGs come with it)
    public string Fmg;

    /// For Item: the index of the call's arg holding the item type
    public int Selector = -1;

    public static readonly EsdRole None = new() { Kind = EsdRoleKind.None };
}

/// <summary>
/// Arg roles for P5 links (Claude/Documentation/ESD Block Editor Analysis.md §4.3): inferred from the catalogue's
/// role hint (ESDLang's) and the arg's name, the same for commands and functions. The param and FMG each role names
/// were chosen by EsdSurvey roles, which scores every rule against the values vanilla passes (param row IDs from
/// Assets/PARAM/&lt;GAME&gt;/Param Row Names, FMG IDs from the game's msgbnds).
/// Pure: compiled into Claude/Tools/EsdSurvey.
/// </summary>
public static class EsdRoles
{
    /// <summary>The talk menu text FMG per game, by the Text Editor's internal name (Menu_MsgBndID_* enums)</summary>
    public static string TalkTextFmg(string game) => game switch
    {
        "ER" or "NR" => "Event_Text_for_Talk",
        "DB_NT" => "Event_Text_For_Talk",
        _ => "Event_Text"
    };

    /// <summary>The same FMG's binder IDs in menu.msgbnd (with its DLC FMGs), for EsdSurvey roles</summary>
    public static int[] TalkTextBinderIds(string game) => game switch
    {
        "ER" => new[] { 33, 366, 466 },
        "NR" => new[] { 33, 507 },
        "DB_NT" => new[] { 223 },
        _ => new[] { 30, 101, 231, 271 }
    };

    /// <summary>The params an item type's enum value name means</summary>
    public static string ItemParam(string typeName) => typeName?.ToLowerInvariant() switch
    {
        "weapon" => "EquipParamWeapon",
        "protector" or "armor" => "EquipParamProtector",
        "accessory" or "ring" or "talisman" => "EquipParamAccessory",
        "goods" or "item" => "EquipParamGoods",
        "gem" or "ashofwar" => "EquipParamGem",
        _ => null
    };

    private static readonly Regex FlagName = new(@"flag(id)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>The role of a call's arg</summary>
    public static EsdRole Of(string game, EsdCallDef def, int index)
    {
        if (def == null || index < 0 || index >= def.Args.Count)
            return EsdRole.None;

        var arg = def.Args[index];
        var name = (arg.Name ?? "").Replace("_", "");
        var lower = name.ToLowerInvariant();
        var hint = arg.Role;

        EsdRole P(params string[] ps) => new() { Kind = EsdRoleKind.Param, Params = ps.ToList() };

        // The end of a row range (shops): a bound, not a row
        if (lower.StartsWith("end") && lower.Contains("param"))
            return EsdRole.None;

        // Explicit param rows first: their names say which param
        if (hint == "Talk" || lower == "talkparamrow" || lower == "talkid" && game != "DS1R")
            return P("TalkParam");
        if (hint == "Shop" || lower.Contains("shoplineupparam") || lower.Contains("shopid"))
            return P("ShopLineupParam", "ShopLineupParam_Recipe");
        if (hint == "Lot" || lower.Contains("itemlotparam") || lower == "itemlotid")
            return P("ItemLotParam_map", "ItemLotParam", "ItemLotParam_enemy");
        if (hint == "ActionButton" || lower.Contains("actionbuttonparam"))
            return P("ActionButtonParam");
        if (lower.Contains("speffect"))
            return P("SpEffectParam");
        if (hint == "Goods" || lower == "goodsid")
            return P("EquipParamGoods");
        if (hint == "Gesture" || lower == "gestureid")
            return P("GestureParam", "EquipParamGesture");
        if (lower == "bonfirewarpid" || lower.Contains("bonfirewarpparam"))
            return P("BonfireWarpParam");

        // An item ID whose param another arg picks
        if (hint == "Item" || lower is "itemid" or "newitemid" or "itemidtoreplace")
        {
            var sel = def.Args.FindIndex(a => a.Role == "ItemType" || (a.Name ?? "").ToLowerInvariant() is "itemcategory" or "itemtype");
            if (sel >= 0 && sel != index)
                return new EsdRole { Kind = EsdRoleKind.Item, Selector = sel };
            return P("EquipParamGoods");
        }

        if (hint == "EventFlag" || FlagName.IsMatch(name) && !lower.Contains("state") && !lower.Contains("status") && !lower.Contains("bits"))
            return new EsdRole { Kind = EsdRoleKind.EventFlag };

        // Text effects (AC6) use another FMG: no role until one is found
        if ((lower == "textid" || lower.EndsWith("textid") || lower == "msgid") && def.Name?.Contains("TextEffect") != true)
            return new EsdRole { Kind = EsdRoleKind.Text, Fmg = TalkTextFmg(game) };

        if (lower.EndsWith("entityid"))
            return new EsdRole { Kind = EsdRoleKind.Entity };

        if (hint == "Machine" || lower == "machineid")
            return new EsdRole { Kind = EsdRoleKind.Machine };

        return EsdRole.None;
    }
}
