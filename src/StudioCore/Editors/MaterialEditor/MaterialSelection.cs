using SoulsFormats;
using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialSelection
{
    public Project Project;
    public MaterialEditor Editor;

    public MaterialSourceType _selectedSourceType;

    public int _selectedFileIndex = -1;
    public string _selectedFileName = "";

    public int _selectedInternalFileIndex = -1;
    public string _selectedInternalFileName = "";

    public MTD _selectedMaterial;
    public MATBIN _selectedMatbin;

    public bool AutoSelectFile;
    public bool AutoSelectInternalFile;

    public MaterialSelection(Project curProject, MaterialEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public bool IsFileSelected(int index)
    {
        return _selectedFileIndex == index;
    }

    public void SelectFile(int index, string filename)
    {
        _selectedFileIndex = index;
        _selectedFileName = filename;
    }

    public bool IsInternalFileSelected(int index)
    {
        return _selectedInternalFileIndex == index;
    }

    public void SelectInternalFile(int index, string filename)
    {
        _selectedInternalFileIndex = index;
        _selectedInternalFileName = filename;
    }
}
