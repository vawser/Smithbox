using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System.Collections.Generic;
using static SoulsFormats.ESD;

namespace StudioCore.EzStateEditorNS;
public class EsdSelection
{
    public EsdEditorScreen Editor;
    public ProjectEntry Project;

    public FileDictionaryEntry SelectedFileEntry;

    public ESD SelectedScript;
    public int SelectedScriptIndex;

    public long SelectedGroupIndex;
    public Dictionary<long, State> SelectedGroup;

    public long SelectNodeIndex;
    public State SelectedNode;

    public bool SelectNextFile = false;
    public bool SelectNextScript = false;
    public bool SelectNextGroup = false;
    public bool SelectNextNode = false;

    public EsdSelection(EsdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Set selected ESD file
    /// </summary>
    public async void SetFile(FileDictionaryEntry newFileEntry)
    {
        SelectedFileEntry = newFileEntry;
        await Project.EsdData.PrimaryBank.LoadScriptBinder(newFileEntry);
    }

    /// <summary>
    /// Set selected ESD script
    /// </summary>
    public void SetScript(int key, ESD entry)
    {
        SelectedScriptIndex = key;
        SelectedScript = entry;
    }

    /// <summary>
    /// Clear selected ESD script
    /// </summary>
    public void ResetScript()
    {
        SelectedScriptIndex = -1;
        SelectedScript = null;
    }

    /// <summary>
    /// Set selected state group
    /// </summary>
    public void SetStateGroup(long key, Dictionary<long, State> entry)
    {
        SelectedGroupIndex = key;
        SelectedGroup = entry;
    }

    /// <summary>
    /// Clear selected state group
    /// </summary>
    public void ResetStateGroup()
    {
        SelectedGroupIndex = -1;
        SelectedGroup = null;
    }

    /// <summary>
    /// Set selected sub state group
    /// </summary>
    public void SetStateGroupNode(long key, State entry)
    {
        SelectNodeIndex = key;
        SelectedNode = entry;
    }

    /// <summary>
    /// Clear selected sub state group
    /// </summary>
    public void ResetStateGroupNode()
    {
        SelectNodeIndex = -1;
        SelectedNode = null;
    }

    public EsdEditorContext CurrentWindowContext = EsdEditorContext.None;

    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SwitchWindowContext(EsdEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            CurrentWindowContext = newContext;
            //TaskLogs.AddLog($"Context: {newContext.GetDisplayName()}");
        }
    }
}

