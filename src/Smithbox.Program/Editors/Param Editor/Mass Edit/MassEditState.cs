using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MassEditState
{
    public MassEdit Parent;

    public MassEditState(MassEdit parent)
    {
        Parent = parent;
    }

    public string CurrentMassEditInput = "";

    public string MassEditResult = "";
    public string CurrentMenuInput = "";
    public string PreviousMenuInput = "";

    public string MassEditInput_CSV = "";
    public string MassEditOutput_CSV = "";
    public string MassEdit_SingleField_CSV = "";

    public string MassEditResult_CSV = "";

    public bool DisplayMassEditPopup;
}
