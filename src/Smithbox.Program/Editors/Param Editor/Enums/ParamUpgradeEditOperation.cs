using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamUpgradeEditOperation
{
    Add,
    Delete,
    Modify,
    NameChange,
    Match
}