using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

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