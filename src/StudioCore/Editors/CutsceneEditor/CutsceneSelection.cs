using SoulsFormats;
using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneSelection
{
    public Project Project;
    public CutsceneEditor Editor;

    public int _selectedFileIndex = -1;
    public string _selectedFileName = "";

    public MQB _selectedCutscene;

    public CutsceneSelection(Project curProject, CutsceneEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }
    public bool IsFileSelected(int index, string fileName)
    {
        if (_selectedFileIndex == index)
        {
            return true;
        }

        return false;
    }

    public void SelectFile(int index, string fileName)
    {
        _selectedFileIndex = index;
        _selectedFileName = fileName;

        _selectedCutscene = null;
    }
}
