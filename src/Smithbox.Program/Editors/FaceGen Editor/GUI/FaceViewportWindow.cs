#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Rectangle = Veldrid.Rectangle;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The FaceGen Editor's viewport: a bare player character (c0000's body, as the other editors build it) wearing the
/// parts the active face names, in their rest pose. The face is not morphed yet: every head shows its base shape.
/// <para>
/// The mesh is made again when the set of part models changes (another face, a part picked, the body's sex), and
/// the head is framed the first time it is shown.
/// </para>
/// </summary>
public class FaceViewportWindow
{
    public FaceGenEditorView View;
    public ProjectEntry Project;

    public IViewport? Viewport;
    public Rectangle Rect;

    public bool ViewportUsingKeyboard;

    /// <summary>The window's id, for the editor's default layout</summary>
    public const string WindowId = "###FaceGenViewport";

    // Looking at the face from the front: characters face -Z, so the camera sits on that side looking along +Z
    private static readonly Quaternion FrontRotation = Quaternion.Identity;

    // Where a head is, when there is no head model to frame (the height of the player's head)
    private static readonly Vector3 DefaultHeadCenter = new(0, 1.58f, 0);
    private const float HeadRadius = 0.22f;

    private MeshRenderableProxy? Mesh;
    private CharacterAppearance? Appearance;
    private string? MeshKey;
    private string? MeshUid;
    private bool FramePending = true;

    // The face's shape on its parts: what was last written, and whether a part was still loading then
    private readonly FaceMorpher Morpher;
    private float[]? AppliedShape;
    private bool MorphPending;

    private static int Serial;

    /// <summary>The parts of the shown face, as last resolved (for the Properties window's part pickers)</summary>
    public IReadOnlyList<FacePart> Parts { get; private set; } = Array.Empty<FacePart>();

    public FaceViewportWindow(FaceGenEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        Morpher = new FaceMorpher(project);

        var window = Smithbox.Instance._context.Window;
        Rect = window.Bounds;

        // Without a renderer the viewport is still shown, as a disabled placeholder
        if (Smithbox.Instance.HasRenderer && view.RenderScene != null)
            Viewport = new VulkanViewport(view.Universe, "FaceGenViewport", Rect.Width, Rect.Height, view.RenderScene);
        else
            Viewport = new NullViewport(view.Universe, "FaceGenViewport", Rect.Width, Rect.Height, view.RenderScene);

        if (Viewport is VulkanViewport vk)
        {
            vk.ViewportCamera.CameraTransform.Rotation = FrontRotation;
            FrameSphere(DefaultHeadCenter, HeadRadius);
        }
    }

    public void Display(uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_FaceGenEditorView);

        if (Viewport is VulkanViewport vulkanViewport)
            vulkanViewport.Display();
        else if (Viewport is NullViewport nullViewport)
            nullViewport.Display();
    }

    /// <summary>Whether the body is the female one: as the menu says, or as the first character wearing the face is</summary>
    public bool IsFemale()
    {
        switch (CFG.Current.FaceGenEditor_Body)
        {
            case 1:
                return false;
            case 2:
                return true;
        }

        if (View.ActiveRow is not { } row || View.ParamName != FaceGenParams.GetReferencedFaceParam(Project))
            return false;

        var wearer = FaceGenParams.GetUsers(Project).Get(row.ID).FirstOrDefault();

        // npcPlayerSex: 0 female, 1 male
        return wearer != null && wearer.Row.Cells.FirstOrDefault(c =>
            c.Def.InternalName.Equals("npcPlayerSex", StringComparison.OrdinalIgnoreCase)).Value is IConvertible sex &&
            sex.ToInt32(null) == 0;
    }

    // ---- camera

    /// <summary>Frames the head, from the front</summary>
    public void FrameFace()
    {
        if (Viewport is VulkanViewport vk)
            vk.ViewportCamera.CameraTransform.Rotation = FrontRotation;

        if (HeadBounds() is { } bounds)
            FrameSphere((bounds.Min + bounds.Max) * 0.5f, MathF.Max(Vector3.Distance(bounds.Min, bounds.Max) * 0.5f, HeadRadius * 0.5f));
        else
            FrameSphere(DefaultHeadCenter, HeadRadius);
    }

    /// <summary>Frames the whole character, from the front</summary>
    public void FrameBody()
    {
        if (Viewport is VulkanViewport vk)
            vk.ViewportCamera.CameraTransform.Rotation = FrontRotation;

        if (Mesh != null && Mesh.HasRenderableGeometry())
        {
            var bounds = Mesh.GetBounds();
            FrameSphere((bounds.Min + bounds.Max) * 0.5f, MathF.Max(Vector3.Distance(bounds.Min, bounds.Max) * 0.5f, 0.5f));
        }
        else
        {
            FrameSphere(new Vector3(0, 0.9f, 0), 1.1f);
        }
    }

    /// <summary>The bounds of the head part, once it has loaded</summary>
    private Veldrid.Utilities.BoundingBox? HeadBounds()
    {
        if (Mesh == null || Appearance == null)
            return null;

        var head = Parts.FirstOrDefault(p => p.Model != null && p.Model.StartsWith("fg_a_01", StringComparison.Ordinal))?.Model;
        var index = head != null ? Appearance.Parts.ToList().FindIndex(p => p.Name == head) : -1;

        if (index < 0 || index >= Mesh.Submeshes.Count)
            return null;

        var submesh = Mesh.Submeshes[index];

        if (!submesh._meshProvider.IsAvailable() || !submesh.HasRenderableGeometry())
            return null;

        return submesh.GetBounds();
    }

    private void FrameSphere(Vector3 center, float radius)
    {
        if (Viewport is not VulkanViewport vk)
            return;

        var camera = vk.ViewportCamera;
        var forward = Vector3.Transform(Vector3.UnitZ, camera.CameraTransform.Rotation);
        var fov = float.DegreesToRadians(camera.FovOverrideDegrees ?? CFG.Current.Viewport_Camera_FOV);

        camera.CameraTransform.Position = center - forward * (radius / MathF.Sin(fov * 0.5f));
    }

    // ---- update

    public void Update(float deltatime)
    {
        if (Viewport != null)
            ViewportUsingKeyboard = Viewport.Update(Smithbox.Instance._context.Window, deltatime);

        UpdateMesh();
        UpdateShape();

        // The head is framed once its geometry is in, which is a few frames after the mesh is made
        if (FramePending && HeadBounds() != null)
        {
            FrameFace();
            FramePending = false;
        }
    }

    private void UpdateMesh()
    {
        var row = View.ActiveRow;
        var isFemale = IsFemale();

        Parts = row != null ? FaceComposition.Resolve(Project, row, isFemale) : Array.Empty<FacePart>();

        if (View.RenderScene == null)
            return;

        var models = Parts.Where(p => p.Model != null).Select(p => p.Model!).Distinct().ToList();
        var key = row == null ? null : $"{(isFemale ? 'f' : 'm')}|{string.Join(",", models)}";

        if (key == MeshKey)
            return;

        ClearMesh();
        MeshKey = key;

        if (key == null)
            return;

        Appearance = CharacterComposition.Resolve(Project, isFemale, models);
        MeshUid = $"FaceGenPreview_{System.Threading.Interlocked.Increment(ref Serial)}";
        Mesh = CharacterComposition.CreateMesh(Project, View.RenderScene, Appearance, MeshUid, LOC.Get("FACEGEN_Load_Job_Parts"));

        if (Mesh != null)
            Mesh.DrawFilter = RenderFilter.Meshes;

        // Shared textures (the body's common textures) may have come in with the parts' own
        ResourceManager.SchedulePostTextureRefresh();
    }

    /// <summary>
    /// Shapes the face parts by the face's shape, with the slider being dragged in place of its stored value. The
    /// shape is worked out every frame (it is cheap), and only written when it changes or a part was still loading.
    /// </summary>
    private void UpdateShape()
    {
        if (Mesh == null || Appearance == null || View.ActiveRow is not { } row || View.ActiveParam is not { } param)
            return;

        if (!CFG.Current.FaceGenEditor_Apply_Shape)
        {
            if (AppliedShape != null)
            {
                Morpher.Restore();
                AppliedShape = null;
            }

            return;
        }

        var shape = FaceGenModel.ShapeCoefficients(Project, param, row,
            View.PreviewOverrides.Count > 0 ? View.PreviewOverrides : null);

        if (shape == null)
            return;

        if (!MorphPending && AppliedShape != null && shape.AsSpan().SequenceEqual(AppliedShape))
            return;

        MorphPending = !Morpher.Apply(Mesh, Appearance, shape);
        AppliedShape = shape;

        // TEMPORARY (FaceGen Phase 4 check): once per face shown, when every part has been written
        var reportKey = $"{View.ParamName}/{row.ID}/{MeshKey}";

        if (!MorphPending && reportKey != LastReportKey && View.PreviewOverrides.Count == 0)
        {
            LastReportKey = reportKey;
            Smithbox.Log(this, $"[FaceGen] {View.ParamName} {row.ID}: {Morpher.LastReport}");
        }
    }

    private string? LastReportKey;

    private void ClearMesh()
    {
        // The parts are shared with anything else that shows them, so their own shape goes back first
        Morpher.Restore();
        AppliedShape = null;
        MorphPending = false;

        Mesh?.Dispose();
        Mesh = null;
        Appearance = null;

        if (MeshUid != null)
            MeshProviderCache.InvalidateUidEntries(MeshUid);

        MeshUid = null;
    }

    /// <summary>Whether anything is shown, so the viewport environment is worth keeping up to date</summary>
    public bool HasMesh => Mesh != null;

    // ---- frame

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
        ClearMesh();
        MeshKey = null;
        (Viewport as VulkanViewport)?.Dispose();
    }
}
