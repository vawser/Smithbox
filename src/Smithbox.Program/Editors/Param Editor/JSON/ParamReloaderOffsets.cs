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

    public string ERItemGiveFuncOffset { get; set; }
    public string ERMapItemManOffset { get; set; }

    public List<string> paramOffsets { get; set; }
    public List<string> itemIDCategories { get; set; }
}