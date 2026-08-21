using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokEditorView : IEditorView
{
    public HavokEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager;

    public HavokSelection Selection;

    public HavokToolWindow ToolWindow;

    public int ViewIndex;
    private int _imguiId = -1;

    public bool JumpToSelectedRow = false;
    public bool _isSearchBarActive = false;

    public HavokEditorView(HavokEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;
        _imguiId = imguiId;

        Selection = new(this, project);
        ActionManager = new();

        ToolWindow = new(this, project);
    }

    public void Display(uint dockspaceId, int viewIndex, bool doFocus, bool isActiveView)
    {

    }
}
