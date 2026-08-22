using HKLib.hk2018;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokBehaviorView
{
    private HavokEditorView View;
    private ProjectEntry Project;

    public HavokBehaviorView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Draw(object sourceObject)
    {
        // TODO: structured editing of behavior elements:
        // ClipGenerators
        // etc
        GUI.WrappedText("TEST");
    }
}
