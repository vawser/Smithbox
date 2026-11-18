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

    // The face's hair, beard, brow and lash colours on their parts' materials
    private readonly FaceTinter Tinter;
    private string? LastColorReportKey;

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
        UpdateColors();

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
        if (!CFG.Current.FaceGenEditor_Apply_Shape)
            shape = new float[shape.Length];

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

    /// <summary>
    /// Colours the hair-type parts by the face's colour fields, with a colour being edited in place of its stored
    /// value. Called every frame: only materials whose colour differs are written.
    /// </summary>
    private void UpdateColors()
    {
        if (Mesh == null || Appearance == null || View.ActiveRow is not { } row)
            return;

        var done = Tinter.Apply(Mesh, Appearance, row, Parts, View.PreviewOverrides.Count > 0 ? View.PreviewOverrides : null);

        // TEMPORARY (FaceGen colour check): once per face shown, when every coloured part has loaded
        var reportKey = $"{View.ParamName}/{row.ID}/{MeshKey}";

        if (done && reportKey != LastColorReportKey && View.PreviewOverrides.Count == 0)
        {
            LastColorReportKey = reportKey;
            Smithbox.Log(this, $"[FaceGen colours] {View.ParamName} {row.ID}: " +
                               $"{(Tinter.LastReport.Length > 0 ? Tinter.LastReport : "no coloured parts")}");
        }
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

    /// <summary>
    /// TEMPORARY (FaceGen texture check): for each part of the shown character, each material's shader, whether the
    /// project's material bank has its MATBIN, and each texture slot: requested or not, the texture it names, and
    /// whether that texture has arrived. Written to the log.
    /// </summary>
    public void LogTextureReport()
    {
        var lines = new List<string> { $"[FaceGen textures] material bank: {(Project.Handler.MaterialData?.PrimaryBank != null ? "loaded" : "NOT LOADED")}, " +
                                       $"texturing {(CFG.Current.Viewport_Enable_Texturing ? "on" : "OFF")}, common_body requested {CommonTexturesRequested}" };

        if (CharacterComposition.GetCommonTexture(Project) is { } common)
            lines.Add($"  common_body: {common.Name} at {common.AssetVirtualPath}");
        else
            lines.Add("  common_body: not found");

        if (Mesh == null || Appearance == null)
        {
            lines.Add("  no mesh");
        }
        else
        {
            for (var i = 0; i < Appearance.Parts.Count && i < Mesh.Submeshes.Count; i++)
            {
                var part = Appearance.Parts[i].Name;

                if (Mesh.Submeshes[i]._meshProvider.ResourceHandle is not ResourceHandle<FlverResource> handle)
                {
                    lines.Add($"  {part}: no FLVER handle");
                    continue;
                }

                if (!handle.IsLoaded || handle.Get()?.GPUMaterials is not { } materials)
                {
                    lines.Add($"  {part}: not loaded ({handle.AccessLevel})");
                    continue;
                }

                lines.Add($"  {part}: {materials.Length} materials");

                foreach (var material in materials)
                {
                    var matbin = Project.Handler.MaterialData?.PrimaryBank?.GetMatbin(
                        System.IO.Path.GetFileNameWithoutExtension(material.MaterialName ?? ""));

                    lines.Add($"    {material.FlverMaterialName} | {material.MaterialName} | shader {material.ShaderName} | " +
                              $"definition {(material.Definition != null ? "yes" : "NO")} | matbin {(matbin != null ? $"{matbin.Samplers.Count} samplers" : "NOT FOUND")}");

                    // TEMPORARY (FaceGen body hair check): how the material is coloured and cut out
                    var slots = material.Definition?.Slots;
                    lines.Add($"      shader key {material.Definition?.ShaderName} | slots {(slots != null ? "yes" : "none")} | " +
                              $"customize {FlverResource.CustomizeMode(material.Definition)} (recipe {slots?.Customize ?? 0}, colour {(slots?.Color != null ? "yes" : "no")}) | " +
                              $"deformer {material.Definition?.Deformer} blend {(material.Definition?.Blend != null)} detail {(material.Definition?.Detail != null)} " +
                              $"fur {(material.Definition?.Fur != null)} | blending {material.Blending} | alpha ref {material.Shading.DiffuseAlpha.W:0.000} | " +
                              $"from colour {material.AlbedoFromColor} | body hair {material.IsBodyHair} | " +
                              $"customize data {(material.Customize is { } c ? $"colour {c.Color} params {c.Params}" : "none")}");

                    for (var t = 0; t < material.TextureResources.Length; t++)
                    {
                        if (!material.TextureResourceFilled[t])
                            continue;

                        var texture = material.TextureResources[t];
                        var state = texture == null ? "requested, not arrived" : texture.IsLoaded ? $"loaded {texture.Name}" : $"handle {texture.Name}, not loaded";
                        lines.Add($"      {(FlverResource.FlverMaterial.TextureType)t}: {state}");
                    }
                }
            }
        }

        Smithbox.Log(this, string.Join("\n", lines));
    }

    private bool CommonTexturesRequested;

    /// <summary>
    /// The textures faces, hair and bodies share (common_body: the AAT500 hair, beard and skin maps, the eye shadows,
    /// the eye patches' cloth and leather), which no part's own textures include. Loaded once, as the Model and
    /// Cutscene Editors load them beside a player character.
    /// </summary>
    private void LoadCommonTextures()
    {
        if (CommonTexturesRequested || !CFG.Current.Viewport_Enable_Texturing ||
            CharacterComposition.GetCommonTexture(Project) is not { } common)
        {
            return;
        }

        CommonTexturesRequested = true;
        _ = LoadCommonTexturesAsync(common);
    }

    private async System.Threading.Tasks.Task LoadCommonTexturesAsync(ResourceDescriptor common)
    {
        try
        {
            var options = new LoadOptions
            {
                JobName = LOC.Get("FACEGEN_Load_Job_Textures"),
                Types = ResourceType.Texture,
                AccessLevel = AccessLevel.AccessGPUOptimizedOnly
            };

            await ResourceManager.DefaultPool.LoadAssets(Project, new HashSet<ResourceDescriptor> { common }, options);
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
