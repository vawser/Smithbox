using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public enum MapConnectionRelationType
{
    Unknown,
    Ancestor,
    Parent,
    Child,
    Descendant,
    Connection
}