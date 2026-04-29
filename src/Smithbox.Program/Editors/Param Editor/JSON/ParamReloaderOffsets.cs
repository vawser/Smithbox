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
    public string paramCountOffset { get; set; }
    public string paramDataOffset { get; set; }
    public string rowPointerOffset { get; set; }
    public string rowHeaderSize { get; set; }

    public string ERItemGiveFuncOffset { get; set; }
    public string ERMapItemManOffset { get; set; }

    public List<string> paramOffsets { get; set; }
    public List<string> itemIDCategories { get; set; }
}