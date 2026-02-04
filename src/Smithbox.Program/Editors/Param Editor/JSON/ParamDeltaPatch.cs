using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamDeltaPatch
{
    public ProjectType ProjectType { get; set; }
    public ulong ParamVersion { get; set; }
    public string Tag { get; set; }

    public List<ParamDelta> Params { get; set; } = new();
}

public class ParamDelta
{
    public string Name { get; set; }
    public List<RowDelta> Rows { get; set; } = new();
}

public class RowDelta
{
    public int ID { get; set; }
    public int Index { get; set; }
    public string Name { get; set; }
    public List<FieldDelta> Fields { get; set; } = new();
    public RowDeltaState State { get; set; } = RowDeltaState.Modified;
}

public class FieldDelta
{
    public string Field { get; set; }
    public string Value { get; set; }
}

public enum RowDeltaState
{
    Added = 0,
    Modified = 1,
    Deleted = 2
}