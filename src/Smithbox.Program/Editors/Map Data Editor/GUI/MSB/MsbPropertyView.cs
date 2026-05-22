using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

/// <summary>
/// The 'fields' view: the properties for the currently selected entry
/// </summary>
public class MsbPropertyView
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    private string PropertyListFilter = "";
    private bool ExactPropertyListFilter = false;

    public MsbPropertyView(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {

    }
}
