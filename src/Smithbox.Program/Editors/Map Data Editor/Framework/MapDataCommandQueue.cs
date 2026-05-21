using StudioCore.Application;
using StudioCore.Editors.MaterialEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataCommandQueue
{
    public MapDataEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    public MapDataCommandQueue(MapDataEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] initcmd)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        // Parse select commands
        if (initcmd != null && initcmd.Length > 1)
        {

        }
    }
}

