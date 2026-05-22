using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapDataEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class MsbEntryPropertyEdit : EditorAction
{
    private MapDataEditorView View;
    private ProjectEntry Project { get; set; }

    public MsbEntryPropertyEdit(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public override ActionEvent Execute()
    {

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {

        return ActionEvent.NoEvent;
    }
}
