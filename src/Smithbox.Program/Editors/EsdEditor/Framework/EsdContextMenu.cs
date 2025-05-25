using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.EsdEditor;
using StudioCore.Formats.JSON;
using System.Collections.Generic;

namespace StudioCore.EzStateEditorNS;

/// <summary>
/// Handles the context menus used by the view classes.
/// </summary>
public class EsdContextMenu
{
    public EsdEditorScreen Editor;
    public ProjectEntry Project;

    public EsdContextMenu(EsdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void FileContextMenu(FileDictionaryEntry entry)
    {

    }

    public void ScriptContextMenu(ESD entry)
    {

    }

    public void StateGroupContextMenu(KeyValuePair<long, Dictionary<long, ESD.State>> entry)
    {

    }

    public void StateNodeContextMenu(ESD.State entry)
    {

    }
}
