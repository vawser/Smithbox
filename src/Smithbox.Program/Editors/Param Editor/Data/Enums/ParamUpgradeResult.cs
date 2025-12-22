using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public enum ParamUpgradeResult
{
    Success = 0,
    RowConflictsFound = -1,
    OldRegulationNotFound = -2,
    OldRegulationVersionMismatch = -3,
    OldRegulationMatchesCurrent = -4
}