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
/// The FaceGen Editor's viewport: a player character (c0000's body, as the other editors build it, bare or in the
/// Character Preview tool's outfit) wearing the parts the active face names, in their rest pose.
/// <para>
/// The mesh is made again when the set of part models changes (another face, a part picked, the body's sex, the
/// outfit), and
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

    // A head is a fraction of a metre across, so the shared speeds (20 m/s normal) are slowed to about 1 m/s
    private const float MoveSpeedFactor = 0.05f;

    /// <summary>The character shown, null with no face; made again when its models change (for the Animation Browser)</summary>
    public MeshRenderableProxy? Mesh { get; private set; }

    private string? MeshKey;

    /// <summary>The parts shown, as last resolved (for the Character Preview tool's list)</summary>
    public CharacterAppearance? Appearance { get; private set; }

    // Raised by the Character Preview tool, so the mesh is made again in the new outfit
    private int OutfitVersion;
    private string? MeshUid;
    private bool FramePending = true;

    // The face's shape on its parts: what was last written, and whether a part was still loading then
    private readonly FaceMorpher Morpher;
    private float[]? AppliedShape;
    private bool MorphPending;

    // The face's hair, beard, brow and lash colours on their parts' materials
    private readonly FaceTinter Tinter;

    private static int Serial;

    /// <summary>The parts of the shown face, as last resolved (for the Properties window's part pickers)</summary>
    public IReadOnlyList<FacePart> Parts { get; private set; } = Array.Empty<FacePart>();

    public FaceViewportWindow(FaceGenEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        Morpher = new FaceMorpher(project);
        Tinter = new FaceTinter(project);

        var window = Smithbox.Instance._context.Window;
        Rect = window.Bounds;

        // Without a renderer the viewport is still shown, as a disabled placeholder
        if (Smithbox.Instance.HasRenderer && view.RenderScene != null)
            Viewport = new VulkanViewport(view.Universe, "FaceGenViewport", Rect.Width, Rect.Height, view.RenderScene);
        else
            Viewport = new NullViewport(view.Universe, "FaceGenViewport", Rect.Width, Rect.Height, view.RenderScene);

        if (Viewport is VulkanViewport vk)
        {
            vk.ViewportCamera.MoveSpeedFactor = MoveSpeedFactor;
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

    /// <summary>Whether the body is the female one: as the Viewport Filters tool says, or as the first character wearing the face is</summary>
    public bool IsFemale()
    {
        switch (CFG.Current.FaceGenEditor.FaceGenEditor_Body)
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
        return wearer != null && FaceGenParams.CellValue(wearer.Row, "npcPlayerSex") is IConvertible sex &&
            sex.ToInt32(null) == 0;
    }

    /// <summary>The Character Preview tool's choice changed: the mesh is made again on the next update</summary>
    public void OutfitChanged()
    {
        OutfitVersion++;
    }

    // ---- camera

    /// <summary>The angles the camera presets look at the character from, as a turn about the vertical axis</summary>
    public enum CameraAngle
    {
        Front,
        ThreeQuarter,
        Profile,
        Back
    }

    private static float AngleYaw(CameraAngle angle) => angle switch
    {
        CameraAngle.ThreeQuarter => MathF.PI / 4,
        CameraAngle.Profile => MathF.PI / 2,
        CameraAngle.Back => MathF.PI,
        _ => 0
    };

    // What was last framed (the head or the body), which the presets and the turntable turn about
    private bool FramingBody;
    private float Yaw;

    /// <summary>Whether the camera turns slowly about what was last framed. It owns the camera while on.</summary>
    public bool Turntable;

    /// <summary>The turntable's speed, in degrees per second</summary>
    public float TurntableSpeed = 20f;

    /// <summary>Frames the head, from the front</summary>
    public void FrameFace()
    {
        FramingBody = false;
        Yaw = 0;
        Frame();
    }

    /// <summary>Frames the whole character, from the front</summary>
    public void FrameBody()
    {
        FramingBody = true;
        Yaw = 0;
        Frame();
    }

    /// <summary>Looks at what was last framed (the head or the body) from one of the preset angles</summary>
    public void SetCameraAngle(CameraAngle angle)
    {
        Yaw = AngleYaw(angle);
        Frame();
    }

    /// <summary>Frames the head or the body, as last chosen, from the current turn</summary>
    private void Frame()
    {
        if (Viewport is VulkanViewport vk)
            vk.ViewportCamera.CameraTransform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, Yaw) * FrontRotation;

        var (center, radius) = FramingTarget();
        FrameSphere(center, radius);
    }

    /// <summary>The sphere around the head (once it has loaded) or the body</summary>
    private (Vector3 Center, float Radius) FramingTarget()
    {
        if (FramingBody)
        {
            if (Mesh != null && Mesh.HasRenderableGeometry())
            {
                var body = Mesh.GetBounds();
                return ((body.Min + body.Max) * 0.5f, MathF.Max(Vector3.Distance(body.Min, body.Max) * 0.5f, 0.5f));
            }

            return (new Vector3(0, 0.9f, 0), 1.1f);
        }

        if (HeadBounds() is { } bounds)
            return ((bounds.Min + bounds.Max) * 0.5f, MathF.Max(Vector3.Distance(bounds.Min, bounds.Max) * 0.5f, HeadRadius * 0.5f));

        return (DefaultHeadCenter, HeadRadius);
    }

    /// <summary>Turns the camera about what was last framed, keeping its distance</summary>
    private void UpdateTurntable(float deltatime)
    {
        if (!Turntable || Viewport is not VulkanViewport vk)
            return;

        Yaw = (Yaw + float.DegreesToRadians(TurntableSpeed) * deltatime) % (MathF.PI * 2);

        var camera = vk.ViewportCamera;
        var (center, _) = FramingTarget();
        var distance = Vector3.Distance(camera.CameraTransform.Position, center);
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, Yaw) * FrontRotation;

        camera.CameraTransform.Rotation = rotation;
        camera.CameraTransform.Position = center - Vector3.Transform(Vector3.UnitZ, rotation) * distance;
    }

    /// <summary>The bounds of the head part, once it has loaded</summary>
    private Veldrid.Utilities.BoundingBox? HeadBounds()
    {
        if (Mesh == null || Appearance == null)
            return null;

        // The face's head part, else the face model the head equipment chose (SDT's Wolf, who has no face parts)
        var head = Parts.FirstOrDefault(p => FaceComposition.IsHead(p.Model))?.Model
                   ?? Appearance.Head;
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
        var fov = float.DegreesToRadians(camera.FovOverrideDegrees ?? CFG.Current.Viewport.Viewport_Camera_FOV);

        camera.CameraTransform.Position = center - forward * (radius / MathF.Sin(fov * 0.5f));
    }

    // ---- update

    public void Update(float deltatime)
    {
        if (Viewport != null)
            ViewportUsingKeyboard = Viewport.Update(Smithbox.Instance._context.Window, deltatime);

        UpdateMesh();
        UpdateShape();
        UpdateColors();

        // The head is framed once its geometry is in, which is a few frames after the mesh is made
        if (FramePending && HeadBounds() != null)
        {
            FrameFace();
            FramePending = false;
        }

        UpdateTurntable(deltatime);
    }

    private void UpdateMesh()
    {
        var row = View.ActiveRow;
        var isFemale = IsFemale();

        Parts = row != null ? FaceComposition.Resolve(Project, row, isFemale) : Array.Empty<FacePart>();

        if (View.RenderScene == null)
            return;

        // The face's models, and the body its face parts value names (DS3)
        var models = Parts.SelectMany(p => new[] { p.Body, p.Model }).Where(m => m != null).Select(m => m!).Distinct().ToList();
        var dressed = !View.CharacterPreview.IsBare;
        var key = row == null ? null : $"{(isFemale ? 'f' : 'm')}|{string.Join(",", models)}|{(dressed ? OutfitVersion : -1)}";

        if (key == MeshKey)
            return;

        ClearMesh();
        MeshKey = key;

        if (key == null)
            return;

        // The tool's outfit, on the face's sex; nothing chosen is the bare character (SDT's default outfit, ER's bare masks)
        var outfit = dressed ? View.CharacterPreview.GetAppearance(isFemale) : null;

        Appearance = CharacterComposition.Resolve(Project, isFemale, models, outfit);
        MeshUid = $"FaceGenPreview_{System.Threading.Interlocked.Increment(ref Serial)}";
        Mesh = CharacterComposition.CreateMesh(Project, View.RenderScene, Appearance, MeshUid, LOC.Get("FACEGEN_Load_Job_Parts"));

        if (Mesh != null)
            Mesh.DrawFilter = RenderFilter.Meshes;

        LoadCommonTextures();

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

        var shape = FaceGenModel.ShapeCoefficients(Project, param, row,
            View.PreviewOverrides.Count > 0 ? View.PreviewOverrides : null);

        if (shape == null)
            return;

        // Without the shape, the base head: still written, so its edge stays pulled onto the body (FaceMorpher's seam)
        if (!CFG.Current.FaceGenEditor.FaceGenEditor_Apply_Shape)
            shape = new float[shape.Length];

        if (!MorphPending && AppliedShape != null && shape.AsSpan().SequenceEqual(AppliedShape))
            return;

        MorphPending = !Morpher.Apply(Mesh, Appearance, shape);
        AppliedShape = shape;
    }

    /// <summary>
    /// Colours the hair-type parts by the face's colour fields, with a colour being edited in place of its stored
    /// value. Called every frame: only materials whose colour differs are written.
    /// </summary>
    private void UpdateColors()
    {
        if (Mesh == null || Appearance == null || View.ActiveRow is not { } row)
            return;

        Tinter.Apply(Mesh, Appearance, row, Parts, View.PreviewOverrides.Count > 0 ? View.PreviewOverrides : null,
            View.ActiveParam);
    }

    private void ClearMesh()
    {
        // The parts are shared with anything else that shows them, so their own shape and colours go back first
        Morpher.Restore();
        Tinter.Restore();
        AppliedShape = null;
        MorphPending = false;

        Mesh?.Dispose();
        Mesh = null;
        Appearance = null;

        if (MeshUid != null)
            MeshProviderCache.InvalidateUidEntries(MeshUid);

        MeshUid = null;
    }

    private bool CommonTexturesRequested;

    /// <summary>
    /// The textures faces, hair and bodies share (common_body: the AAT500 hair, beard and skin maps, the eye shadows,
    /// the eye patches' cloth and leather), which no part's own textures include. Loaded once, as the Model and
    /// Cutscene Editors load them beside a player character.
    /// </summary>
    private void LoadCommonTextures()
    {
        if (CommonTexturesRequested || !CFG.Current.Viewport.Viewport_Enable_Texturing)
            return;

        var common = CharacterComposition.GetCommonTexture(Project);

        // The shared head's textures are in the fgbnd (DS1, DS1R), only wanted when it is worn
        var fgbnd = Parts.Any(p => p.Model != null && (p.Model.Equals(FaceComposition.SharedHead, StringComparison.OrdinalIgnoreCase) ||
                                                       p.Model.Equals(FaceComposition.BundleHead, StringComparison.OrdinalIgnoreCase)))
            ? FaceGenModelData.Get(Project).BinderPath?.TrimStart('/')
            : null;

        if (common == null && fgbnd == null)
            return;

        CommonTexturesRequested = true;
        _ = LoadCommonTexturesAsync(common, fgbnd);
    }

    private async System.Threading.Tasks.Task LoadCommonTexturesAsync(ResourceDescriptor? common, string? fgbnd)
    {
        try
        {
            var options = new LoadOptions
            {
                JobName = LOC.Get("FACEGEN_Load_Job_Textures"),
                Types = ResourceType.Texture,
                AccessLevel = AccessLevel.AccessGPUOptimizedOnly
            };

            if (common != null)
                await ResourceManager.DefaultPool.LoadAssets(Project, new HashSet<ResourceDescriptor> { common }, options);

            if (fgbnd != null)
                await ResourceManager.DefaultPool.LoadContainer(fgbnd, options);

            await ResourceManager.LoadMissingTexturesAsync();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FACEGEN_Load_Textures_Failed"), e);
        }
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
