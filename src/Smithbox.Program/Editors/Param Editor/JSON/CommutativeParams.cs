using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamCommutativityGroups
{
    public List<ParamCommutativityEntry> Groups { get; set; } = new();
}

public class ParamCommutativityEntry
{
    public string Name { get; set; }
    public List<string> Params { get; set; }
}