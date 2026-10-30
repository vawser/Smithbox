#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
///     Previews the animations of the loaded model.
/// </summary>
public class ModelAnimationTool
{
    public ModelEditorView View;
    public ProjectEntry Project;

    private readonly AnimationPreviewPanel _panel;

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

    public void OnToolWindow()
    {
        if (ImGui.CollapsingHeader($"{LOC.Get("MODEL_Animation_Header")}##modelAnimationHeader"))
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

        return new AnimationTarget(
            container,
            wrapper.Name,
            kind,
            wrapper.Parent.TargetFS,
            wrapper.Parent.Path,
            () => GetMeshes(container),
            // Loading another model discards the container, and the meshes along with it
            () => wrapper.Container == container && View.Selection.SelectedModelWrapper == wrapper);
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
