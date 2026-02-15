using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
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

    public IAnimView AssignedView;

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

        if (EditorType is AnimViewType.TAE)
        {
            AssignedView = new TimeActView(this, Project);
        }

        if (EditorType is AnimViewType.BEH)
        {
            AssignedView = new BehaviorView(this, Project);
        }
    }

    public bool IsBehaviorView()
    {
        if (AssignedView is BehaviorView)
        {
            return true;
        }

        return false;
    }

    public bool IsTimeActView()
    {
        if (AssignedView is TimeActView)
        {
            return true;
        }

        return false;
    }

    public BehaviorView GetBehaviorView()
    {
        if(AssignedView is BehaviorView)
        {
            return (BehaviorView)AssignedView;
        }

        return null;
    }

    public TimeActView GetTimeActView()
    {
        if (AssignedView is TimeActView)
        {
            return (TimeActView)AssignedView;
        }

        return null;
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        if (AssignedView is BehaviorView)
        {
            var view = GetBehaviorView();

            view.Display();
        }

        if (AssignedView is TimeActView)
        {
            var view = GetTimeActView();

            view.Display();
        }
    }
}
