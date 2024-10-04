using SoulsFormats;
using StudioCore.TalkEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.ESD;
using static StudioCore.Editors.TalkEditor.EsdBank;

namespace StudioCore.Editors.EsdEditor;
public class EsdSelectionManager
{
    private EsdEditorScreen Screen;

    public EsdScriptInfo _selectedFileInfo;
    public IBinder _selectedBinder;
    public string _selectedBinderKey;

    public ESD _selectedEsdScript;
    public int _selectedEsdScriptKey;

    public long _selectedStateGroupKey;
    public Dictionary<long, State> _selectedStateGroups;

    public long _selectedStateGroupNodeKey;
    public State _selectedStateGroupNode;

    public EsdSelectionManager(EsdEditorScreen screen)
    {
        Screen = screen;
    }

    /// <summary>
    /// Set selected ESD file
    /// </summary>
    public void SetFile(EsdScriptInfo info, IBinder binder)
    {
        _selectedBinderKey = info.Name;
        _selectedFileInfo = info;
        _selectedBinder = binder;
    }

    /// <summary>
    /// Set selected ESD script
    /// </summary>
    public void SetScript(int key, ESD entry)
    {
        _selectedEsdScriptKey = key;
        _selectedEsdScript = entry;
    }

    /// <summary>
    /// Clear selected ESD script
    /// </summary>
    public void ResetScript()
    {
        _selectedEsdScriptKey = -1;
        _selectedEsdScript = null;
    }

    /// <summary>
    /// Set selected state group
    /// </summary>
    public void SetStateGroup(long key, Dictionary<long, State> entry)
    {
        _selectedStateGroupKey = key;
        _selectedStateGroups = entry;
    }

    /// <summary>
    /// Clear selected state group
    /// </summary>
    public void ResetStateGroup()
    {
        _selectedStateGroupKey = -1;
        _selectedStateGroups = null;
    }

    /// <summary>
    /// Set selected sub state group
    /// </summary>
    public void SetStateGroupNode(long key, State entry)
    {
        _selectedStateGroupNodeKey = key;
        _selectedStateGroupNode = entry;
    }

    /// <summary>
    /// Clear selected sub state group
    /// </summary>
    public void ResetStateGroupNode()
    {
        _selectedStateGroupNodeKey = -1;
        _selectedStateGroupNode = null;
    }
}

