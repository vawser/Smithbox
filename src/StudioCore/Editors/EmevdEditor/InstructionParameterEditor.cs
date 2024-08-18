using StudioCore.EmevdEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

public class InstructionParameterEditor
{
    private EmevdEditorScreen Screen;

    Dictionary<long, string> InstructionTypes = new Dictionary<long, string>()
    {
        [0] = "byte",
        [1] = "ushort",
        [2] = "uint",
        [3] = "sbyte",
        [4] = "short",
        [5] = "int",
        [6] = "float"
    };

    public InstructionParameterEditor(EmevdEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {

    }

    public void Display()
    {
        if (Screen._selectedInstruction != null)
        {
            // _selectedInstruction
        }
    }
}
