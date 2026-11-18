#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Linq;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The Animation Browser: plays c0000's animations on the viewport's character, face and outfit included. Poses only
/// exist in the renderer, so nothing here touches the face.
/// <para>
/// The viewport makes its mesh again whenever the set of models changes (another face, a part, the sex, the outfit),
/// which ends playback on the old mesh. The animation playing then is started again on the new mesh, at the same
/// time, speed and looping, so faces can be browsed in motion.
/// </para>
/// </summary>
public class FaceGenAnimationTool
{
    private readonly FaceGenEditorView View;
    private readonly ProjectEntry Project;

    private readonly AnimationPreviewPanel Panel;

    // The mesh the panel was last given, which is the key of its animation
    private MeshRenderableProxy? AnimatedMesh;

    // The playback to put back once the animation has started on a new mesh
    private Resume? PendingResume;

    private sealed record Resume(MeshRenderableProxy Mesh, int AnimationId, float Time, bool Playing, bool Loop, float Speed);

    public FaceGenAnimationTool(FaceGenEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        Panel = new AnimationPreviewPanel(project);
    }

    /// <summary>Every frame, after the viewport's update (which makes the mesh), even with the tool hidden</summary>
    public void Update()
    {
        if (Panel.IsSupported)
            FollowMesh();

        Panel.Update();

        ApplyResume();
    }

    public void StopAll()
    {
        PendingResume = null;
        AnimatedMesh = null;
        Panel.StopAll();
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("FACEGEN_Tool_Animation_Browser")}##faceGenAnimationToolHeader"))
            return;

        ImGui.BeginChild("FaceGenAnimationToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        if (!Panel.IsSupported)
        {
            GUI.WrappedText(LOC.Get("FACEGEN_AnimationBrowser_Unsupported"));
            return;
        }

        if (View.ActiveRow == null || View.ViewportWindow.Mesh is not { } mesh || View.ViewportWindow.Appearance is not { } appearance)
        {
            GUI.WrappedText(LOC.Get("FACEGEN_AnimationBrowser_No_Face"));
            return;
        }

        Panel.Display(CreateTarget(mesh, appearance));
    }

    /// <summary>
    /// When the viewport has made a new mesh while an animation played on the old one: the old one is stopped, and
    /// the same animation queued on the new one
    /// </summary>
    private void FollowMesh()
    {
        var mesh = View.ViewportWindow.Mesh;

        if (mesh == AnimatedMesh)
            return;

        var previous = AnimatedMesh;
        AnimatedMesh = mesh;

        if (previous == null)
            return;

        // The animation on the old mesh, or the one still waiting to resume on it
        Resume? resume = null;

        if (Panel.GetAnimator(previous) is { CurrentAnimation: { } entry } animator)
            resume = new Resume(previous, entry.Id, animator.Time, animator.Playing, animator.Loop, animator.Speed);
        else if (PendingResume?.Mesh == previous)
            resume = PendingResume;

        Panel.Stop(previous);
        PendingResume = null;

        if (resume == null || mesh == null || View.ViewportWindow.Appearance is not { } appearance)
            return;

        var id = resume.AnimationId;
        Panel.QueueAnimation(CreateTarget(mesh, appearance), package => package.Animations.FirstOrDefault(a => a.Id == id));

        PendingResume = resume with { Mesh = mesh };
    }

    /// <summary>Puts the playback back once the queued animation has started on the new mesh</summary>
    private void ApplyResume()
    {
        if (PendingResume is not { } resume)
            return;

        if (Panel.GetAnimator(resume.Mesh) is not { CurrentClip: not null } animator)
            return;

        animator.Loop = resume.Loop;
        animator.Speed = resume.Speed;
        animator.Seek(System.Math.Min(resume.Time, animator.Duration));
        animator.SetPlaying(resume.Playing);
        animator.Update(0);

        PendingResume = null;
    }

    /// <summary>
    /// The viewport's character: c0000's skeleton, with every part (the body, the outfit and the face's models)
    /// skinned to it, as the Map Editor animates a composite character
    /// </summary>
    private AnimationTarget CreateTarget(MeshRenderableProxy mesh, CharacterAppearance appearance)
    {
        var model = CharacterComposition.CompositeModel;
        var asset = ModelLocator.GetChrModel(Project, model, model);

        return new AnimationTarget(
            mesh,
            model,
            AnimatedModelKind.Character,
            Project.VFS.FS,
            PathBuilder.GetRelativePath(Project, asset.AssetVirtualPath),
            // The skeleton's model has no meshes of its own: all of them are parts
            () => System.Array.Empty<MeshRenderableProxy>(),
            // The viewport replaces the mesh when its models change, and drops it with no face shown
            () => View.ViewportWindow.Mesh == mesh,
            null,
            CharacterComposition.GetAnimatedParts(Project, appearance, mesh));
    }
}
