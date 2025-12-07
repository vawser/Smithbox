using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Program.Editors.MapEditor;

public enum RelationType
{
    Unknown,
    Ancestor,
    Parent,
    Child,
    Descendant,
    Connection
}