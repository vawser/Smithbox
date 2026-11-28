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
    /// (Claude/Documentation/Gparam.md §7.2)
    /// </summary>
    GparamEditorOnly,

    /// <summary>
    /// The VFS and file locator only, no editor data: Time Act Merge reads the project's own anibnds itself
    /// (Claude/Documentation/Time Act.md §5.2)
    /// </summary>
    TimeActEditorOnly
}