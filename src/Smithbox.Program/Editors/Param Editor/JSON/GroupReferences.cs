using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class GroupReferences
{
    public List<GroupRefList> Entries { get; set; }
}

public class GroupRefList
{
    public string Name { get; set; }
    public List<GroupRefEntry> Instances { get; set; }
}

public class GroupRefEntry
{
    public string Param { get; set; }
    public string DisplayName { get; set; }
}