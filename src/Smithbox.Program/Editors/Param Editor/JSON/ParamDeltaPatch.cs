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

    /// <summary>
    /// Row table params (rows sharing an ID, see Table Params.json) are stored as whole groups instead of as
    /// rows, so an insert or delete inside a group can't shift the identity of the other rows in it.
    /// Older patches have no Groups and still load.
    /// </summary>
    public List<RowGroupDelta> Groups { get; set; } = new();
}

/// <summary>
/// The full contents of one row table group (all rows with one ID, in order).
/// Added: the ID isn't in vanilla. Modified: the group replaces the existing one. Deleted: the ID is removed (no rows).
/// Each row holds every field, so applying a group doesn't depend on the rows it replaces.
/// </summary>
public class RowGroupDelta
{
    public int ID { get; set; }
    public RowDeltaState State { get; set; } = RowDeltaState.Modified;
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