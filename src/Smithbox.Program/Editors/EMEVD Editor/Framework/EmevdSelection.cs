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

    /// <summary>
    /// The primary selected instruction (the last one clicked: what the Properties window shows, and the anchor of a
    /// Shift range), or -1 for the event header
    /// </summary>
    public int InstructionIndex { get; private set; } = -1;

    /// <summary>Every selected instruction, the primary one included. Structural edits act on all of them.</summary>
    public IReadOnlySet<int> Lines => SelectedLines;

    private readonly HashSet<int> SelectedLines = new();

    // The end a Shift range is drawn from
    private int Anchor = -1;

    public bool IsSelected(int line) => SelectedLines.Contains(line);

    public int Count => SelectedLines.Count;

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
        SelectInstruction(-1);

        file?.EnsureLoaded();
    }

    public void SelectEvent(EMEVD.Event ev)
    {
        if (ev == SelectedEvent)
            return;

        SelectedEvent = ev;
        SelectInstruction(-1);
    }

    private bool Valid(int index) => SelectedEvent != null && index >= 0 && index < SelectedEvent.Instructions.Count;

    /// <summary>Selects one instruction (or the header, with -1), dropping the rest of the selection</summary>
    public void SelectInstruction(int index)
    {
        SelectedLines.Clear();
        InstructionIndex = Valid(index) ? index : -1;
        Anchor = InstructionIndex;

        if (InstructionIndex >= 0)
            SelectedLines.Add(InstructionIndex);
    }

    /// <summary>Ctrl+click: adds or removes one instruction</summary>
    public void Toggle(int index)
    {
        if (!Valid(index))
            return;

        if (SelectedLines.Remove(index))
        {
            if (InstructionIndex == index)
                InstructionIndex = SelectedLines.Count > 0 ? SelectedLines.Max() : -1;
        }
        else
        {
            SelectedLines.Add(index);
            InstructionIndex = index;
        }

        Anchor = InstructionIndex;
    }

    /// <summary>Shift+click / Shift+arrow: every line from the anchor to this one (rows are in line order)</summary>
    public void SelectRange(int index)
    {
        if (!Valid(index))
            return;

        var anchor = Valid(Anchor) ? Anchor : index;

        SelectedLines.Clear();
        for (var i = Math.Min(anchor, index); i <= Math.Max(anchor, index); i++)
            SelectedLines.Add(i);

        InstructionIndex = index;
        Anchor = anchor;
    }

    /// <summary>Replaces the selection, e.g. with the lines an edit moved or added; the first is primary</summary>
    public void SelectLines(IEnumerable<int> lines)
    {
        var valid = lines.Where(Valid).OrderBy(l => l).ToList();

        SelectedLines.Clear();
        SelectedLines.UnionWith(valid);
        InstructionIndex = valid.Count > 0 ? valid[0] : -1;
        Anchor = InstructionIndex;
    }

    /// <summary>Drops lines an undo or edit took away</summary>
    public void Validate()
    {
        SelectedLines.RemoveWhere(l => !Valid(l));

        if (!Valid(InstructionIndex))
            InstructionIndex = SelectedLines.Count > 0 ? SelectedLines.Min() : -1;

        if (!Valid(Anchor))
            Anchor = InstructionIndex;
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
