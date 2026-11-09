using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The FX Editor's viewport (FX Editor Plan, phase V1): the grid, the shared viewport environment's lighting, and the
/// selected effect's node origins and emitter shapes as debug wireframes (<see cref="FxDebugShapes"/>).
/// </summary>
public class FxViewportWindow
{
    public FxEditorScreen Editor;
    public ProjectEntry Project;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool ViewportUsingKeyboard;

    public FxDebugRenderer DebugRenderer = new();

    /// <summary>What the drawn shapes were built from, to rebuild them only when it changes.</summary>
    private (FxEntry Entry, object Fxr, object Item, int State, int Edits, int Playback, bool Shapes, bool Axes) BuiltKey;

    /// <summary>The effect last framed, so selecting another item of the same effect keeps the camera.</summary>
    private FxEntry FramedEntry;

    /// <summary>Bumped by every edit, undo and redo.</summary>
    private int EditVersion;

    public int ShownNodes { get; private set; }
    public int ShownShapes { get; private set; }

    public FxViewportWindow(FxEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        var window = Smithbox.Instance._context.Window;
        Rect = window.Bounds;

        if (Smithbox.Instance._context.Device != null)
        {
            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
                Viewport = new VulkanViewport(editor.Universe, "FxViewport", Rect.Width, Rect.Height, editor.RenderScene);
            else
                Viewport = new NullViewport(editor.Universe, "FxViewport", Rect.Width, Rect.Height, editor.RenderScene);
        }

        editor.ActionManager.ActionPerformed += () => EditVersion++;
    }

    public void Display(uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_FxEditor);

        if (Viewport is VulkanViewport vulkanViewport)
            vulkanViewport.Display();
        else if (Viewport is NullViewport nullViewport)
            nullViewport.Display();
    }

    public void Update(float deltatime)
    {
        if (Viewport != null)
            ViewportUsingKeyboard = Viewport.Update(Smithbox.Instance._context.Window, deltatime);

        UpdateShapes();
        DebugRenderer.Update();
    }

    /// <summary>Rebuilds the debug shapes when the effect, the selection, an edit or the preview inputs change.</summary>
    private void UpdateShapes()
    {
        if (Editor.RenderScene == null)
            return;

        var entry = Editor.Selection.Entry;
        var key = (entry, (object)entry?.Fxr, Editor.Selection.Item, Editor.Playback.PreviewState, EditVersion,
            Editor.Playback.Version, CFG.Current.FxEditor_Viewport_Display_Emitter_Shapes,
            CFG.Current.FxEditor_Viewport_Display_Node_Axes);

        if (key.Equals(BuiltKey))
            return;

        BuiltKey = key;

        var shapes = FxDebugShapes.Build(Editor.Data, entry?.Fxr != null ? entry : null, Editor.Selection.Item,
            Editor.Playback.PreviewState, key.Item7, key.Item8);

        ShownNodes = shapes.Nodes;
        ShownShapes = shapes.Shapes;

        DebugRenderer.Show(Editor.RenderScene, shapes);

        // A newly selected effect is framed once its shapes are known
        if (entry?.Fxr != null && entry != FramedEntry)
        {
            FramedEntry = entry;
            Frame(shapes);
        }
    }

    /// <summary>Fits the effect's shapes into view, with a minimum size so a point-sized effect isn't zoomed into.</summary>
    public void Frame(FxDebugShapes.Result shapes = null)
    {
        if (Viewport == null)
            return;

        shapes ??= FxDebugShapes.Build(Editor.Data, Editor.Selection.Entry, null, Editor.Playback.PreviewState, true, true);

        var min = shapes.HasBounds ? shapes.Min : Vector3.Zero;
        var max = shapes.HasBounds ? shapes.Max : Vector3.Zero;

        // Include the origin, and at least a 2 m box around the centre
        min = Vector3.Min(min, Vector3.Zero);
        max = Vector3.Max(max, Vector3.Zero);
        var centre = (min + max) / 2;
        min = Vector3.Min(min, centre - Vector3.One);
        max = Vector3.Max(max, centre + Vector3.One);

        Viewport.FitBox(new BoundingBox(min, max));
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        Viewport?.Draw(device, cl);
    }

    public bool InputCaptured()
    {
        return Viewport != null && Viewport.IsViewportSelected;
    }

    public void Dispose()
    {
        DebugRenderer.Clear();
    }
}
