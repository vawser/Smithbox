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

    /// <summary>The selected effect, running (phase V2), and its particles drawn as small crosses.</summary>
    public FxPreviewRuntime Runtime;
    public FxDebugRenderer ParticleRenderer = new();

    /// <summary>The particles drawn as their billboards (phase V3), and the textures they use.</summary>
    public FxTextureLibrary Textures;
    public FxBillboardRenderer Billboards;

    /// <summary>Model particles and lights (phase V4).</summary>
    public FxModelRenderer Models;
    public FxLightRenderer Lights;
    private DateTime LastParticleBuild;
    private float LastParticleTime = -1;
    private static readonly TimeSpan ParticleInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>Most particles drawn; more are left out (the runtime still simulates them).</summary>
    private const int MaxDrawnParticles = 20000;

    public int DrawnParticles { get; private set; }

    /// <summary>What the drawn shapes were built from, to rebuild them only when it changes.</summary>
    private (FxEntry Entry, object Fxr, object Item, int State, (int Committed, int Live) Edits, int Playback, bool Shapes, bool Axes) BuiltKey;

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

        Runtime = new FxPreviewRuntime(editor);
        Textures = new FxTextureLibrary(editor);
        Billboards = new FxBillboardRenderer(editor, Textures);
        Models = new FxModelRenderer(editor, Textures);
        Lights = new FxLightRenderer(editor);
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

        var camera = (Viewport as VulkanViewport)?.ViewportCamera.CameraTransform.Position ?? Vector3.Zero;
        Runtime.Update(EditVersion, Editor.Live.Version, camera);
        FollowEffect();
        Billboards.Update(Runtime.Instance, camera);
        var forward = Viewport is VulkanViewport vk
            ? Vector3.Transform(Vector3.UnitZ, vk.ViewportCamera.CameraTransform.RotationMatrix)
            : Vector3.UnitZ;
        Models.Update(Runtime.Instance, camera, forward);
        Lights.Update(Runtime.Instance);
        UpdateParticles();
        ParticleRenderer.Update();
    }

    private Vector3? LastEffectPosition;

    /// <summary>
    /// With a moving preview, the camera moves by as much as the effect did, so it stays in view (and comes back
    /// with it when playback restarts).
    /// </summary>
    private void FollowEffect()
    {
        var position = Runtime.EffectWorld.Translation;
        if (Viewport is VulkanViewport vk && LastEffectPosition is { } last && CFG.Current.FxEditor_Preview_Motion_Follow
            && Editor.Playback.Motion != 0)
        {
            var delta = position - last;
            if (delta != Vector3.Zero)
                vk.ViewportCamera.CameraTransform.Position += delta;
        }
        LastEffectPosition = position;
    }

    /// <summary>
    /// Redraws the particles at most every <see cref="ParticleInterval"/>, and only once the last set is shown, so
    /// a slow frame rate doesn't keep replacing a set before it's drawn.
    /// </summary>
    private void UpdateParticles()
    {
        if (Editor.RenderScene == null || ParticleRenderer.IsUpdating)
            return;

        var instance = Runtime.Instance;
        bool markers = CFG.Current.FxEditor_Viewport_Display_Particle_Markers;
        bool lightMarkers = CFG.Current.FxEditor_Viewport_Display_Light_Markers && Lights.DrawnLights > 0;
        var highlightNode = CFG.Current.FxEditor_EffectTree_Highlight_Selected ? SelectedNode() : null;
        if (instance == null || !markers && !lightMarkers && highlightNode == null)
        {
            if (LastParticleTime >= 0)
            {
                ParticleRenderer.Clear();
                LastParticleTime = -1;
                DrawnParticles = 0;
            }
            SelectedBounds = null;
            return;
        }

        var now = DateTime.UtcNow;
        if (now - LastParticleBuild < ParticleInterval
            || instance.Time == LastParticleTime && ReferenceEquals(highlightNode, LastHighlightNode) && HiddenVersion == LastHiddenVersion)
            return;

        LastParticleBuild = now;
        LastParticleTime = instance.Time;
        LastHighlightNode = highlightNode;
        LastHiddenVersion = HiddenVersion;

        var particles = new List<FxParticleOutput>();
        instance.CollectParticles(particles);

        var result = new FxDebugShapes.Result();
        const float size = 0.04f;

        // The selected node's branch: a box round its particles and a cross on each, in the Selected colour
        SelectedBounds = null;
        if (highlightNode != null)
        {
            var branch = BranchInstances(instance, highlightNode);
            Vector3 min = new(float.MaxValue), max = new(float.MinValue);
            int marked = 0;
            const float mark = 0.06f;
            foreach (var p in particles)
            {
                if (!branch.Contains(p.Node))
                    continue;
                var o = p.Position;
                min = Vector3.Min(min, o);
                max = Vector3.Max(max, o);
                if (marked++ < MaxDrawnParticles / 4)
                {
                    result.Add(FxDebugLayer.Selected, o - Vector3.UnitX * mark, o + Vector3.UnitX * mark);
                    result.Add(FxDebugLayer.Selected, o - Vector3.UnitY * mark, o + Vector3.UnitY * mark);
                    result.Add(FxDebugLayer.Selected, o - Vector3.UnitZ * mark, o + Vector3.UnitZ * mark);
                }
            }
            if (marked > 0)
            {
                SelectedBounds = new BoundingBox(min, max);
                AddBox(result, FxDebugLayer.Selected, min - new Vector3(0.05f), max + new Vector3(0.05f));
            }
        }

        int drawn = 0;
        foreach (var p in markers ? particles : new())
        {
            if (drawn++ >= MaxDrawnParticles)
                break;
            var o = p.Position;
            result.Add(FxDebugLayer.Particles, o - Vector3.UnitX * size, o + Vector3.UnitX * size);
            result.Add(FxDebugLayer.Particles, o - Vector3.UnitY * size, o + Vector3.UnitY * size);
            result.Add(FxDebugLayer.Particles, o - Vector3.UnitZ * size, o + Vector3.UnitZ * size);
        }
        DrawnParticles = markers ? Math.Min(particles.Count, MaxDrawnParticles) : 0;

        // Lights: a cross where each is, a horizontal ring at its range, and a spot light's direction
        if (lightMarkers)
        {
            foreach (var light in Lights.Current)
            {
                var o = light.Position;
                const float cross = 0.08f;
                result.Add(FxDebugLayer.Lights, o - Vector3.UnitX * cross, o + Vector3.UnitX * cross);
                result.Add(FxDebugLayer.Lights, o - Vector3.UnitY * cross, o + Vector3.UnitY * cross);
                result.Add(FxDebugLayer.Lights, o - Vector3.UnitZ * cross, o + Vector3.UnitZ * cross);

                float r = Math.Min(light.Range, 50);
                var prev = o + new Vector3(r, 0, 0);
                for (int i = 1; i <= 32; i++)
                {
                    float a = i * MathF.Tau / 32;
                    var next = o + new Vector3(MathF.Cos(a) * r, 0, MathF.Sin(a) * r);
                    result.Add(FxDebugLayer.Lights, prev, next);
                    prev = next;
                }
                if (light.Spot)
                    result.Add(FxDebugLayer.Lights, o, o + light.Direction * Math.Min(light.Range, 2));
            }
        }

        ParticleRenderer.Show(Editor.RenderScene, result);
    }

    private object LastHighlightNode;
    private int LastHiddenVersion;

    /// <summary>Bumped by the Effect Tree when visibility changes, so a paused preview redraws its highlight.</summary>
    public int HiddenVersion;

    /// <summary>Where the selected node's branch's particles are now; null when it has none drawn.</summary>
    public BoundingBox? SelectedBounds { get; private set; }

    /// <summary>The node the selected tree item belongs to (a config or action's node), null for none.</summary>
    private SoulsFormats.FxrNode SelectedNode()
    {
        var root = Editor.Selection.Entry?.Fxr?.Root;
        return root == null || Editor.Selection.Item == null ? null : FxNodeOps.OwnerNode(root, Editor.Selection.Item);
    }

    /// <summary>Every running instance of the node, and everything under them (effects they start included).</summary>
    private static HashSet<FxNodeInstance> BranchInstances(FxEffectInstance instance, SoulsFormats.FxrNode node)
    {
        var set = new HashSet<FxNodeInstance>();
        foreach (var n in instance.AllNodes())
            if (ReferenceEquals(n.Def.Source, node))
                foreach (var d in n.Descendants())
                    set.Add(d);
        return set;
    }

    private static void AddBox(FxDebugShapes.Result result, FxDebugLayer layer, Vector3 a, Vector3 b)
    {
        Vector3 C(int x, int y, int z) => new(x == 0 ? a.X : b.X, y == 0 ? a.Y : b.Y, z == 0 ? a.Z : b.Z);
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 2; j++)
            {
                result.Add(layer, C(0, i, j), C(1, i, j));
                result.Add(layer, C(i, 0, j), C(i, 1, j));
                result.Add(layer, C(i, j, 0), C(i, j, 1));
            }
    }

    /// <summary>Fits the view to the selected node's particles, or to the effect when it has none drawn.</summary>
    public void FrameSelected()
    {
        if (Viewport == null)
            return;
        if (SelectedBounds is { } box)
        {
            var centre = (box.Min + box.Max) / 2;
            Viewport.FitBox(new BoundingBox(Vector3.Min(box.Min, centre - new Vector3(0.5f)), Vector3.Max(box.Max, centre + new Vector3(0.5f))));
        }
        else
        {
            Frame();
        }
    }

    /// <summary>Rebuilds the debug shapes when the effect, the selection, an edit or the preview inputs change.</summary>
    private void UpdateShapes()
    {
        if (Editor.RenderScene == null)
            return;

        var entry = Editor.Selection.Entry;
        // Live changes (a value mid-edit) move the wireframes too
        var key = (entry, (object)entry?.Fxr, Editor.Selection.Item, Editor.Playback.PreviewState, (EditVersion, Editor.Live.Version),
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

        // A newly selected effect is framed once its shapes are known, unless auto-framing is off (the camera then
        // stays where it is, e.g. to compare effects from one viewpoint)
        if (entry?.Fxr != null && entry != FramedEntry)
        {
            FramedEntry = entry;
            if (CFG.Current.FxEditor_Viewport_Auto_Frame)
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
        ParticleRenderer.Clear();
        Billboards.Clear();
        Models.Clear();
        Lights.Clear();
        Textures.Clear();
        Runtime.Clear();

        // The viewport's render pipeline (GPU targets, passes and queues), gizmos and grids
        (Viewport as VulkanViewport)?.Dispose();
        Viewport = null;
    }
}
