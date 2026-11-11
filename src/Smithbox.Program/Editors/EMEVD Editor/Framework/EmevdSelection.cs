using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The selected file, event and instruction, with a back history for Initialize Event jumps
/// </summary>
public class EmevdSelection
{
    private readonly EmevdEditorView View;

    public EmevdFileWrapper SelectedFile { get; private set; }
    public EMEVD.Event SelectedEvent { get; private set; }

    /// <summary>The selected instruction's index in the event, or -1 for the event header</summary>
    public int InstructionIndex { get; private set; } = -1;

    /// <summary>Set by jumps, so the lists and script scroll to the selection once</summary>
    public bool ScrollToEvent;
    public bool ScrollToInstruction;

    private readonly Stack<(EmevdFileWrapper file, EMEVD.Event ev, int instr)> BackHistory = new();

    public EmevdSelection(EmevdEditorView view)
    {
        View = view;
    }

    public EMEVD.Instruction SelectedInstruction =>
        SelectedEvent != null && InstructionIndex >= 0 && InstructionIndex < SelectedEvent.Instructions.Count
            ? SelectedEvent.Instructions[InstructionIndex]
            : null;

    public bool CanGoBack => BackHistory.Count > 0;

    public void SelectFile(EmevdFileWrapper file)
    {
        if (file == SelectedFile)
            return;

        SelectedFile = file;
        SelectedEvent = null;
        InstructionIndex = -1;

        file?.EnsureLoaded();
    }

    public void SelectEvent(EMEVD.Event ev)
    {
        if (ev == SelectedEvent)
            return;

        SelectedEvent = ev;
        InstructionIndex = -1;
    }

    public void SelectInstruction(int index)
    {
        InstructionIndex = SelectedEvent != null && index >= 0 && index < SelectedEvent.Instructions.Count ? index : -1;
    }

    /// <summary>Selects an event in any file, remembering where the jump came from</summary>
    public void JumpTo(EmevdFileWrapper file, EMEVD.Event ev, int instruction = -1)
    {
        if (file == null || ev == null)
            return;

        if (SelectedFile != null && SelectedEvent != null)
            BackHistory.Push((SelectedFile, SelectedEvent, InstructionIndex));

        Show(file, ev, instruction);
    }

    public void GoBack()
    {
        if (BackHistory.Count == 0)
            return;

        var (file, ev, instr) = BackHistory.Pop();
        Show(file, ev, instr);
    }

    private void Show(EmevdFileWrapper file, EMEVD.Event ev, int instruction)
    {
        SelectFile(file);
        SelectEvent(ev);
        SelectInstruction(instruction);

        View.FileList.ScrollToSelection = true;
        ScrollToEvent = true;
        ScrollToInstruction = instruction >= 0;
    }
}
