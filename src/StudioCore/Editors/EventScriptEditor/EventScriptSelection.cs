using SoulsFormats;
using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptSelection
{
    public Project Project;
    public EventScriptEditor Editor;

    public int SelectedFileIndex { get; set; }
    public string SelectedFileKey { get; set; }

    public EMEVD SelectedScript { get; set; }
    public string SelectedScriptKey { get; set; }

    public EMEVD.Event SelectedEvent { get; set; }
    public int SelectedEventIndex { get; set; }
    public EMEVD.Instruction SelectedInstruction { get; set; }
    public int SelectedInstructionIndex { get; set; }

    public bool SelectNextScript { get; set; }
    public bool SelectNextEvent { get; set; }
    public bool SelectNextInstruction { get; set; }

    public EventScriptSelection(Project project, EventScriptEditor editor)
    {
        Project = project;
        Editor = editor;
    }

    public bool IsFileSelected(int index, string fileName)
    {
        if (SelectedFileIndex == index)
        {
            return true;
        }

        return false;
    }

    public void SelectFile(int index, string fileName)
    {
        SelectedFileIndex = index;
        SelectedFileKey = fileName;

        SelectedScript = null;
    }
}
