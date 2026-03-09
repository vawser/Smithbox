using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class FieldReferenceGroups
{
    public List<FieldReferenceGroup> Entries { get; set; } = new();
}

public class FieldReferenceGroup
{
    public string Name { get; set; }
    public List<FieldReferenceSet> Instances { get; set; }
}

public class FieldReferenceSet
{
    public string Param { get; set; }
    public string DisplayName { get; set; }
}