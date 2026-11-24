using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// One view of the FX Editor: its own selection, undo history and viewport, with the Effect List, Effect Tree,
/// Action Inspector, Curve Editor, Viewport and Tools windows. The views share the effect bank, and the notes,
/// favorites, templates and scans kept on <see cref="FxEditorScreen"/>.
/// </summary>
public class FxEditorView : IEditorView
{
    public FxEditorScreen Editor;
    public ProjectEntry Project;

    public int ViewIndex;

    public ActionManager ActionManager = new();

    public FxSelection Selection;

    public FxEffectList EffectList;
    public FxEffectTree EffectTree;
    public FxInspector Inspector;
    public FxCurveEditor CurveEditor;

    // Viewport
    public RenderScene RenderScene;
    public ViewportSelection ViewportSelection = new();
    public ViewportActionManager ViewportActionManager = new();
    public FxUniverse Universe;
    public FxViewportWindow ViewportWindow;
    public FxPlayback Playback = new();
    public FxLiveEdit Live = new();

    // Tools
    public FxToolWindow ToolWindow;
    public FxPreviewTool PreviewTool;
    public FxSearchTool SearchTool;
    public FxReferencesTool ReferencesTool;
    public FxResourcesTool ResourcesTool;
    public FxReloaderTool ReloaderTool;
    public ViewportGridTool GridTool;
    public ViewportRenderingTool RenderingTool;

    public FxEditorView(FxEditorScreen editor, ProjectEntry project, int viewIndex)
    {
        Editor = editor;
        Project = project;
        ViewIndex = viewIndex;

        // The edit version every view's caches and viewport rebuild from, and other editors showing this project's
        // effects build them again on
        ActionManager.ActionPerformed += () =>
        {
            if (Data != null)
                Data.EditVersion++;
        };

        Selection = new FxSelection(this);
        EffectList = new FxEffectList(this);
        EffectTree = new FxEffectTree(this);
        Inspector = new FxInspector(this);
        CurveEditor = new FxCurveEditor(this);

        RenderScene = Smithbox.Instance._context.Device != null ? new RenderScene() : null;
        Universe = new FxUniverse(this, project);
        ViewportWindow = new FxViewportWindow(this, project);

        PreviewTool = new FxPreviewTool(this);
        SearchTool = new FxSearchTool(this);
        ReferencesTool = new FxReferencesTool(this);
        ResourcesTool = new FxResourcesTool(this);
        ReloaderTool = new FxReloaderTool(this);
        GridTool = new ViewportGridTool(ViewportGridEditor.Fx, () => ViewportWindow?.Viewport as VulkanViewport);
        RenderingTool = new ViewportRenderingTool(() => ViewportWindow?.Viewport as VulkanViewport);
        ToolWindow = new FxToolWindow(this);
    }

    public FxData Data => Project.Handler.FxData;

    /// <summary>
    /// Releases the view's viewport (its render pipeline's GPU targets and the effect display), once the view has been
    /// closed. Also called when the project closes. The view must not be displayed or updated afterwards.
    /// </summary>
    public void Dispose()
    {
        ViewportSelection.ClearSelection();
        ViewportActionManager.Clear();

        ViewportWindow?.Dispose();
    }

    // ---------------------------------------------------------------- windows

    public void Display(uint dockspaceId)
    {
        Window($"{LOC.Get("FX_Window_Effect_List")}###fxEditor_EffectList_{ViewIndex}", dockspaceId, EditorFocusContext.FxEditor_EffectList, EffectList.Display);
        Window($"{LOC.Get("FX_Window_Effect_Tree")}###fxEditor_EffectTree_{ViewIndex}", dockspaceId, EditorFocusContext.FxEditor_EffectTree, EffectTree.Display);
        Window($"{LOC.Get("FX_Window_Inspector")}###fxEditor_Inspector_{ViewIndex}", dockspaceId, EditorFocusContext.FxEditor_Inspector, Inspector.Display);

        if (CurveEditorFocusRequested)
        {
            CurveEditorFocusRequested = false;
            if (CFG.Current.Interface_FxEditor_CurveEditor)
                ImGui.SetNextWindowFocus();
        }

        if (CFG.Current.Interface_FxEditor_CurveEditor)
            Window($"{LOC.Get("FX_Window_Curve_Editor")}###fxEditor_CurveEditor_{ViewIndex}", dockspaceId, EditorFocusContext.FxEditor_Inspector, CurveEditor.Display);

        // Viewport: the shared viewport window sets the focus (and the active view) itself
        ViewportWindow.Display(dockspaceId);

        if (CFG.Current.Interface_FxEditor_ToolWindow)
            Window($"{LOC.Get("FX_Window_Tools")}###fxEditor_Tools_{ViewIndex}", dockspaceId, EditorFocusContext.FxEditor_Tools, ToolWindow.Display, GUI.GetInnerWindowFlags());
    }

    private void Window(string title, uint dockspace, EditorFocusContext focus, Action draw, ImGuiWindowFlags? flags = null)
    {
        ImGui.SetNextWindowDockID(dockspace, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_FxEditorView);
        if (ImGui.Begin(title, flags ?? GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(focus);
                Editor.ViewHandler.ActiveView = this;
            }
            draw();
        }
        ImGui.End();
    }

    private bool CurveEditorFocusRequested;

    /// <summary>
    /// Brings the Curve Editor to the front on the next frame (selecting its tab when docked), showing it first if
    /// it is hidden. The inspector calls this when a property is opened in it.
    /// </summary>
    public void FocusCurveEditor()
    {
        CFG.Current.Interface_FxEditor_CurveEditor = true;
        CurveEditorFocusRequested = true;
    }

    // ---------------------------------------------------------------- viewport

    private FxEntry PlaybackEntry;

    public void Update(float dt)
    {
        // A new effect starts playing from the beginning
        if (Selection.Entry != PlaybackEntry)
        {
            PlaybackEntry = Selection.Entry;
            Playback.Restart();
        }

        Playback.Update(dt);

        ViewportWindow.Update(dt);
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewportWindow.Viewport is VulkanViewport { Visible: true })
            ViewportWindow.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        ViewportWindow.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        return ViewportWindow.InputCaptured();
    }

    // ---------------------------------------------------------------- editing

    /// <summary>Selects an effect by id, as a jump: the list scrolls to it. False if no binder holds it.</summary>
    public bool JumpTo(int id)
    {
        if (!Data.Bank.Entries.TryGetValue(id, out var entry))
        {
            Smithbox.LogWarning(this, LOC.Get("FX_Jump_Not_Found", $"f{id:D9}"));
            return false;
        }

        Selection.SelectEntry(entry);
        Selection.FocusEntry = Selection.Entry == entry;
        return Selection.FocusEntry;
    }

    /// <summary>Commits values still being edited in the Inspector and Curve Editor (and takes back their live change).</summary>
    public void FlushEdits()
    {
        Inspector.FlushPending();
        CurveEditor.FlushPending();
        Live.Revert();
    }

    /// <summary>Drops the effect's unsaved changes, in every view (see <see cref="FxEditorScreen.ReloadEntry"/>).</summary>
    public void ReloadEntry(FxEntry entry) => Editor.ReloadEntry(entry);
}
