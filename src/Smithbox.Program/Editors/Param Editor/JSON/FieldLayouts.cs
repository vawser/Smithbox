using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class FieldLayouts
{
    public List<FieldLayout> Entries { get; set; } = new();
}

public class FieldLayout
{
    public string Name { get; set; }
    public bool UngroupedAtBottom { get; set; }

    public List<FieldLayoutEntry> Groups { get; set; }
}

public class FieldLayoutEntry
{
    public string Title { get; set; }
    public List<string> Fields { get; set; }
}