using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MassEditResult
{
    public string Information;
    public ParamMassEditResultType Type;

    public MassEditResult(ParamMassEditResultType result, string info)
    {
        Type = result;
        Information = info;
    }
}