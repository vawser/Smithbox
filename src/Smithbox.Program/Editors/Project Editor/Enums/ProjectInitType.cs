using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

/// <summary>
/// This is used to control which editors are loaded when initing a project via aux bank functions
/// </summary>
public enum ProjectInitType
{
    ProjectDefined,
    MapEditorOnly,
    ParamEditorOnly,
    TextEditorOnly,

    /// <summary>
    /// The VFS and file locator only, no editor data: Material Merge reads the project's material binders itself,
    /// so loading its MaterialData (and the vanilla compare it starts) would be wasted
    /// </summary>
    MaterialEditorOnly,

    /// <summary>
    /// The VFS and file locator only, no editor data: Gparam Merge reads the project's own GPARAMs itself
    /// (Claude/Documentation/Gparam Merge Plan.md §3)
    /// </summary>
    GparamEditorOnly
}