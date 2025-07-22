using StudioCore.Core;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;
namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Holds the tool functions used by this editor.
/// </summary>
public class EmevdActionHandler
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdActionHandler(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

}
