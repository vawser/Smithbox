using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System.Linq;
using System.Net;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Holds the current selection state for the editor
/// </summary>
public class EmevdSelection
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public FileDictionaryEntry SelectedFileEntry { get; set; }
    public EMEVD SelectedScript { get; set; }

    public EMEVD.Event SelectedEvent { get; set; }
    public int SelectedEventIndex { get; set; }
    public EMEVD.Instruction SelectedInstruction { get; set; }
    public int SelectedInstructionIndex { get; set; }

    public bool SelectNextScript { get; set; }
    public bool SelectNextEvent { get; set; }
    public bool SelectNextInstruction { get; set; }

    public EmevdSelection(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public EmevdEditorContext CurrentWindowContext = EmevdEditorContext.None;

    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SwitchWindowContext(EmevdEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            CurrentWindowContext = newContext;
            //TaskLogs.AddLog($"Context: {newContext.GetDisplayName()}");
        }
    }

    public async void SelectFile(FileDictionaryEntry newFileEntry)
    {
        await Project.EmevdData.PrimaryBank.LoadScript(newFileEntry);

        SelectedEvent = null;
        SelectedEventIndex = -1;

        SelectedInstruction = null;
        SelectedInstructionIndex = -1;

        SelectedFileEntry = newFileEntry;

        var targetScript = Project.EmevdData.PrimaryBank.Scripts
                .Where(e => e.Key.Filename == newFileEntry.Filename).FirstOrDefault();

        SelectedScript = targetScript.Value;
    }

    public void SelectEvent(EMEVD.Event newEvent, int newIndex)
    {
        Editor.Selection.SelectedEvent = newEvent;
        Editor.Selection.SelectedEventIndex = newIndex;

        Editor.Selection.SelectedInstruction = null;
        Editor.Selection.SelectedInstructionIndex = -1;
    }

    public void SelectInstruction(EMEVD.Instruction newInstruction, int newIndex)
    {
        Editor.Selection.SelectedInstruction = newInstruction;
        Editor.Selection.SelectedInstructionIndex = newIndex;
    }
}
