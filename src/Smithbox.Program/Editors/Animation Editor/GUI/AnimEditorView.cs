using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.AnimEditor;

public class AnimEditorView
{
    public AnimEditorScreen Editor;
    public ProjectEntry Project;

    public Sdl2Window Window;
    public GraphicsDevice Device;
    public RenderScene RenderScene;

    public ViewportActionManager ViewportActionManager = new();
    public ActionManager ActionManager = new();

    public int ViewIndex;

    public AnimViewType EditorType;

    public AnimUniverse Universe;
    public ViewportSelection ViewportSelection = new();

    public AnimViewportWindow ViewportWindow;

    public AnimEditorView(AnimEditorScreen editor, ProjectEntry project, int imguiId, AnimViewType type)
    {
        Editor = editor;
        Project = project;

        Window = Smithbox.Instance._context.Window;
        Device = Smithbox.Instance._context.Device;
        RenderScene = new();

        ViewIndex = imguiId;

        EditorType = type;

        Universe = new AnimUniverse(this, project);
        ViewportWindow = new(this, project);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        if(EditorType is AnimViewType.TAE)
        {
            DisplayTimeActView(doFocus, isActiveView);
        }

        if (EditorType is AnimViewType.BEH)
        {
            DisplayBehaviorView(doFocus, isActiveView);
        }
    }

    public void DisplayTimeActView(bool doFocus, bool isActiveView)
    {
        // TODO: after behavior editor
    }

    public void DisplayBehaviorView(bool doFocus, bool isActiveView)
    {
    }
}
