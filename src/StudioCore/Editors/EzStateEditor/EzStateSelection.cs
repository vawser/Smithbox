using SoulsFormats;
using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateSelection
{
    public Project Project;
    public EzStateEditor Editor;

    public int SelectedFileIndex;
    public string SelectedFilename;

    public int SelectedInternalFileIndex;
    public string SelectedInternalFilename;

    public ESD SelectedScript;

    public long SelectedStateGroupIndex;
    public Dictionary<long, ESD.State> SelectedStateGroups;

    public long SelectedStateGroupNodeIndex;
    public ESD.State SelectedStateGroupNode;

    public bool SelectNextFile = false;
    public bool SelectNextScript = false;
    public bool SelectNextStateGroup = false;
    public bool SelectNextStateNode = false;

    public EzStateSelection(Project curProject, EzStateEditor editor)
    {
        Project = curProject;
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
        SelectedFilename = fileName;

        SelectedScript = null;
    }

    public bool IsInternalFileSelected(int index, string fileName)
    {
        if (SelectedInternalFileIndex == index)
        {
            return true;
        }

        return false;
    }

    public void SelectScript(int key, string name, BinderFile internalFile)
    {
        SelectedInternalFileIndex = key;
        SelectedInternalFilename = name;

        SelectedScript = ESD.Read(internalFile.Bytes);
    }

    public bool IsStateGroupSelected(int index)
    {
        if (SelectedStateGroupIndex == index)
        {
            return true;
        }

        return false;
    }

    public void SelectStateGroup(long key, Dictionary<long, ESD.State> entry)
    {
        SelectedStateGroupIndex = key;
        SelectedStateGroups = entry;
    }

    public void ClearStateGroup()
    {
        SelectedStateGroupIndex = -1;
        SelectedStateGroups = null;
    }

    public bool IsStateGroupNodeSelected(int index)
    {
        if (SelectedStateGroupNodeIndex == index)
        {
            return true;
        }

        return false;
    }

    public void SelectStateGroupNode(long key, ESD.State entry)
    {
        SelectedStateGroupNodeIndex = key;
        SelectedStateGroupNode = entry;
    }

    public void ClearStateGroupNode()
    {
        SelectedStateGroupNodeIndex = -1;
        SelectedStateGroupNode = null;
    }
}
