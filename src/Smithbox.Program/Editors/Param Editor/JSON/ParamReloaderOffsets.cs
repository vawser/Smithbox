using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamReloaderOffsets
{
    public List<ParamReloaderOffsetEntry> Groups { get; set; } = new();
}

public class ParamReloaderOffsetEntry
{
    public string exeName { get; set; }
    public string exeVersion { get; set; }

    public List<GameOffsetBase> bases { get; set; }
}

public class GameOffsetBase
{
    public string paramBaseAob { get; set; }
    public string paramBaseAobRelativeOffset { get; set; }
    public string paramBase { get; set; }
    public string paramInnerPath { get; set; }

    // Optional: hops after [paramBase] and before the per-param offset ("0x280/0x308").
    public string paramOuterPath { get; set; }

    // Optional, for per-map params named <prefix>_mAA_BB_CC_DD: offsets keyed by prefix, and where the object
    // reached by paramOuterPath stores the loaded map's ID (u32 0xAABBCCDD). A param is only written when its
    // map is the loaded one.
    public string mapIdOffset { get; set; }
    public List<string> mapParamOffsets { get; set; }
    public string paramCountOffset { get; set; }
    public string paramDataOffset { get; set; }
    public string rowPointerOffset { get; set; }
    public string rowHeaderSize { get; set; }

    // Item Gib: the game's give-item function and the manager singleton it is called on.
    // itemGiveLayout selects the item table layout: "Durability" ({id, qty, durability}, DS3),
    // "Gem" ({id, qty, -1, gemId}, ER), "Source" ({id, qty, -1, itemGiveSource}, SDT)
    // or "Descriptor" ({byte count; 0x50-byte item descriptors from +8}, NR).
    // itemGiveR9 / itemGiveStackArgs: the 4th argument and the arguments after it ("0x0,0x1" = 5th, 6th).
    // A value of "scratch" passes a pointer to a 0x40-byte block whose first int is -1.
    // Each address is an AOB (preferred) or a hard module offset.
    public string itemGiveLayout { get; set; }
    public string itemGiveMaxEntries { get; set; }
    public string itemGiveSource { get; set; }
    public string itemGiveR9 { get; set; }
    public string itemGiveStackArgs { get; set; }
    public string itemGiveFuncAob { get; set; }
    public string itemGiveFuncAobRelativeOffset { get; set; }
    public string itemGiveFuncOffset { get; set; }
    public string itemGiveManagerAob { get; set; }
    public string itemGiveManagerAobRelativeOffset { get; set; }
    public string itemGiveManagerOffset { get; set; }

    // Optional, "Descriptor" layout only: per-param calls made before the give, in the same game thread:
    // lookup(&rowRef, rowId) then prepare(&rowRef, &descriptor). Used for NR relics (rolls their effects).
    public List<ItemGivePrepareData> itemGivePrepare { get; set; }

    // Optional: SpEffect Gib. func(player ChrIns*, spEffectId, flag), with the player found at
    // [chrManager] followed through playerPath.
    public SpEffectGiveData spEffectGive { get; set; }

    public List<string> paramOffsets { get; set; }
    public List<string> itemIDCategories { get; set; }
}

public class SpEffectGiveData
{
    // The param whose active row is given (e.g. "SpEffectParam").
    public string param { get; set; }
    public string funcAob { get; set; }
    public string funcAobRelativeOffset { get; set; }
    public string funcOffset { get; set; }
    public string chrManagerAob { get; set; }
    public string chrManagerAobRelativeOffset { get; set; }
    public string chrManagerOffset { get; set; }
    // Hops from [chrManager] to the player ChrIns, e.g. "0x174E8" (each hop: add, then read a pointer).
    public string playerPath { get; set; }
    // Third argument (r8), e.g. "0x0".
    public string flag { get; set; }
}

public class ItemGivePrepareData
{
    public string param { get; set; }
    public string lookupFuncAob { get; set; }
    public string lookupFuncAobRelativeOffset { get; set; }
    public string lookupFuncOffset { get; set; }
    public string prepareFuncAob { get; set; }
    public string prepareFuncAobRelativeOffset { get; set; }
    public string prepareFuncOffset { get; set; }

    // Optional: slots the user can set after prepare (Item Gib designer), as
    // "<row field>:<descriptor offset>[:<label kind>:<label number>]".
    // The row field holds a pool ID: the rows of slotPoolParam with that ID, whose slotPoolField is a row ID
    // of slotValueParam. The chosen value is written as an int at the descriptor offset.
    public List<string> slots { get; set; }
    public string slotPoolParam { get; set; }
    public string slotPoolField { get; set; }
    public string slotPoolWeightField { get; set; }
    public string slotValueParam { get; set; }
}