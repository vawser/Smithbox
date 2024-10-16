﻿using SoulsFormats;
using StudioCore.EmevdEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.EmevdEditor.EmevdBank;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Holds the current selection state for the editor
/// </summary>
public class EmevdSelectionManager
{
    private EmevdEditorScreen Screen;

    public EventScriptInfo SelectedFileInfo { get; set; }
    public EMEVD SelectedScript { get; set; }
    public string SelectedScriptKey { get; set; }

    public EMEVD.Event SelectedEvent { get; set; }
    public int SelectedEventIndex { get; set; }
    public EMEVD.Instruction SelectedInstruction { get; set; }
    public int SelectedInstructionIndex { get; set; }

    public bool SelectNextScript { get; set; }
    public bool SelectNextEvent { get; set; }
    public bool SelectNextInstruction { get; set; }

    public EmevdSelectionManager(EmevdEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {
        SelectedFileInfo = null;
        SelectedScript = null;
        SelectedScriptKey = null;

        SelectedEvent = null;
        SelectedEventIndex = -1;
        SelectedInstruction = null;
        SelectedInstructionIndex = -1;

        SelectNextScript = false;
        SelectNextEvent = false;
        SelectNextInstruction = false;
    }
}
