#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
///     Previews the animations of the loaded model.
/// </summary>
public class ModelAnimationTool
{
    public ModelEditorView View;
    public ProjectEntry Project;

    private readonly AnimationPreviewPanel _panel;

    private readonly Dictionary<Entity, Matrix4x4> _markerRestPose = new();

    public ModelAnimationTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        _panel = new AnimationPreviewPanel(project);
    }

    public void Update()
    {
        _panel.Update();
    }

    public void StopAll()
    {
        _panel.StopAll();
    }

    public void OnToolWindow()
    {
        if (ToolSections.Header($"{LOC.Get("MODEL_Animation_Header")}##modelAnimationHeader"))
        {
            ImGui.BeginChild("ModelAnimationToolSection");
            Display();
            ImGui.EndChild();
        }
    }

    private void Display()
    {
        if (!_panel.IsSupported)
        {
            GUI.WrappedText("Animation playback is not available for this game.");
            return;
        }

        ModelWrapper? wrapper = View.Selection.SelectedModelWrapper;
        if (wrapper?.Container == null)
        {
            GUI.WrappedText("Load a character or asset to preview its animations.");
            return;
        }

        AnimatedModelKind? kind = GetKind(wrapper);
        if (kind == null)
        {
            GUI.WrappedText("This type of model has no animations.");
            return;
        }

        _panel.Display(CreateTarget(wrapper, kind.Value));
    }

    private AnimationTarget CreateTarget(ModelWrapper wrapper, AnimatedModelKind kind)
    {
        ModelContainer container = wrapper.Container;
        MeshRenderableProxy? characterMesh = container.CharacterMesh;
        var meshGeneration = container.MeshGeneration;

        return new AnimationTarget(
            container,
            wrapper.Name,
            kind,
            wrapper.Parent.TargetFS,
            wrapper.Parent.Path,
            () => GetMeshes(container),
            // Loading another model discards the container, and the meshes along with it.
            // Changing the equipment of a character replaces its part meshes, and a preview of the edited model
            // replaces the model's own meshes.
            () => wrapper.Container == container && View.Selection.SelectedModelWrapper == wrapper &&
                  container.CharacterMesh == characterMesh && container.MeshGeneration == meshGeneration,
            animator => UpdateMarkers(container, animator),
            characterMesh != null
                ? CharacterComposition.GetAnimatedParts(Project, container.CharacterAppearance, characterMesh)
                : null);
    }

    /// <summary>
    ///     Moves the bone and dummy markers along with the pose. Only the proxies move: the entities
    ///     keep their rest transforms, so a pose never finds its way into the saved model.
    /// </summary>
    private void UpdateMarkers(ModelContainer container, ModelAnimator? animator)
    {
        if (animator == null)
        {
            foreach (Entity marker in _markerRestPose.Keys)
            {
                if (marker.RenderSceneMesh != null)
                {
                    marker.RenderSceneMesh.World = marker.GetWorldMatrix();
                }
            }

            _markerRestPose.Clear();
            return;
        }

        // There can be hundreds of markers, and working out an entity's matrix walks all of its parents.
        // The rest pose is kept in model space: the bone deltas are in model space, and the display
        // rotation is applied after them, so the markers stay on the rotated model.
        if (_markerRestPose.Count == 0)
        {
            foreach (Entity marker in container.Nodes.Concat(container.Dummies))
            {
                _markerRestPose[marker] = marker is ModelEntity modelMarker
                    ? modelMarker.GetModelMatrix()
                    : marker.GetWorldMatrix();
            }
        }

        RenderFilter filter = View.RenderScene.DrawFilter;
        Matrix4x4 display = container.DisplayMatrix;

        if ((filter & RenderFilter.Nodes) != 0)
        {
            for (var i = 0; i < container.Nodes.Count && i < animator.NodeCount; i++)
            {
                MoveMarker(container.Nodes[i], animator, i, display);
            }
        }

        if ((filter & RenderFilter.Dummies) != 0)
        {
            foreach (Entity marker in container.Dummies)
            {
                // The parent bone is often a static helper, and only the attach bone really moves
                var dummy = (FLVER.Dummy)marker.WrappedObject;
                MoveMarker(marker, animator, dummy.AttachBoneIndex >= 0 ? dummy.AttachBoneIndex : dummy.ParentBoneIndex, display);
            }
        }
    }

    private void MoveMarker(Entity marker, ModelAnimator animator, int node, Matrix4x4 display)
    {
        if (marker.RenderSceneMesh != null && node >= 0 && node < animator.NodeCount)
        {
            marker.RenderSceneMesh.World = _markerRestPose[marker] * animator.GetNodeDelta(node) * display;
        }
    }

    // Each mesh of the FLVER is an entity with a proxy of its own here, so all of them follow the same animator
    private static IReadOnlyList<MeshRenderableProxy> GetMeshes(ModelContainer container)
    {
        return container.Meshes.Select(e => e.RenderSceneMesh).OfType<MeshRenderableProxy>().ToList();
    }

    private static AnimatedModelKind? GetKind(ModelWrapper wrapper)
    {
        var path = wrapper.Parent.Path.TrimStart('/');

        if (path.StartsWith("chr", StringComparison.OrdinalIgnoreCase))
        {
            return AnimatedModelKind.Character;
        }

        if (path.StartsWith("obj", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("asset", StringComparison.OrdinalIgnoreCase))
        {
            return AnimatedModelKind.Asset;
        }

        return null;
    }
}
