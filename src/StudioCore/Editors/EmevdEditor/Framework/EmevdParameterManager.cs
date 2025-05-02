using StudioCore.EmevdEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor.Framework;

/// <summary>
/// Handles the function parameters used in instructions and passed by events
/// </summary>
public class EmevdParameterManager
{
    private EmevdEditorScreen Screen;

    public EmevdParameterManager(EmevdEditorScreen screen)
    {
        Screen = screen;
    }

    // Store all the parameter 'defs' from the events.
    // We can then use this to decorate the instruction and the event.


    // InstructionIndex -> which instruction it affects
    // TargetStartByte -> Which parameter in the instruction if affects (first byte of it)
    // SourceStartByte -> Which byte in the passed parameters from the event is the start
    // ByteCount -> Used to work out the size of the target and source byte arrays

    // Event:
    // -> Affected Instruction
    // -> Target Parameter in Instruction
    // -> Data from Event Parameter

    // TODO:
    // build def data storage
    // add ability to edit event parameters
}
