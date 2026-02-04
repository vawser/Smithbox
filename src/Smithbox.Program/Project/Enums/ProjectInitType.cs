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
    TextEditorOnly
}