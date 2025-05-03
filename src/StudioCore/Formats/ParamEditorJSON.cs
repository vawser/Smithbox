using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ParamEditor.Tools;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Formats.JSON;

// -------------- Param Memory Offsets --------------
public class GameOffsetResource
{
    public string exeName { get; set; }
    public List<GameOffsetReference> list { get; set; }
}

public class GameOffsetReference
{
    public string exeVersion { get; set; }
    public string paramBaseAob { get; set; }
    public string paramBaseAobRelativeOffset { get; set; }
    public string paramBase { get; set; }
    public string paramInnerPath { get; set; }
    public string paramCountOffset { get; set; }
    public string paramDataOffset { get; set; }
    public string rowPointerOffset { get; set; }
    public string rowHeaderSize { get; set; }

    public List<string> paramOffsets { get; set; }
    public List<string> itemGibOffsets { get; set; }
}

// -------------- Param Categories --------------
public class ParamCategoryResource
{
    public List<ParamCategoryEntry> Categories { get; set; }
}

public class ParamCategoryEntry
{
    public bool ForceBottom { get; set; } = false;
    public bool ForceTop { get; set; } = false;

    public string DisplayName { get; set; }
    public List<string> Params { get; set; }
}

// -------------- Commutative Param Groups --------------
public class ParamCommutativeResource
{
    public List<ParamCommutativeEntry> Groups { get; set; }
}

public class ParamCommutativeEntry
{
    public string Name { get; set; }
    public List<string> Params { get; set; }
}
