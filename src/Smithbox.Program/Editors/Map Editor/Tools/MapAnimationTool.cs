#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
///     Previews the animations of the selected character or asset in the map.
/// </summary>
public class MapAnimationTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    private readonly AnimationPreviewPanel _panel;

    public MapAnimationTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        _panel = new AnimationPreviewPanel(project);
    }

    public void Update()
    {
        _panel.Update();
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        ImGui.BeginChild("MapAnimationToolSection", ImGuiChildFlags.Borders);

        if (!_panel.IsSupported)
        {
            GUI.WrappedText("Animation playback is not available for this game.");
            ImGui.EndChild();
            return;
        }

        _panel.DisplayActiveSummary();

        MsbEntity? entity = GetSelectedEntity();
        if (entity?.RenderSceneMesh is MeshRenderableProxy mesh)
        {
            _panel.Display(CreateTarget(entity, mesh));
        }
        else
        {
            GUI.WrappedText("Select a character or asset to preview its animations.");
        }

        ImGui.EndChild();
    }

    private AnimationTarget CreateTarget(MsbEntity entity, MeshRenderableProxy mesh)
    {
        var modelName = entity.GetProperty("ModelName")?.GetValue(entity.WrappedObject) as string ?? "";
        var isCharacter = IsCharacter(entity);

        ResourceDescriptor asset = isCharacter
            ? ModelLocator.GetChrModel(Project, modelName, modelName)
            : ModelLocator.GetObjModel(Project, modelName, modelName);

        return new AnimationTarget(
            entity,
            modelName,
            isCharacter ? AnimatedModelKind.Character : AnimatedModelKind.Asset,
            Project.VFS.FS,
            PathBuilder.GetRelativePath(Project, asset.AssetVirtualPath),
            () => new[] { mesh },
            // The mesh is replaced or removed when the model is changed or the map is unloaded
            () => entity.RenderSceneMesh == mesh);
    }

    private MsbEntity? GetSelectedEntity()
    {
        HashSet<MsbEntity> selection = View.ViewportSelection.GetFilteredSelection<MsbEntity>();
        if (selection.Count != 1)
        {
            return null;
        }

        MsbEntity entity = selection.First();
        return IsCharacter(entity) || EntityHelper.IsPartAsset(entity) || EntityHelper.IsPartDummyAsset(entity)
            ? entity
            : null;
    }

    private static bool IsCharacter(MsbEntity entity)
    {
        return EntityHelper.IsPartEnemy(entity) || EntityHelper.IsPartDummyEnemy(entity);
    }
}
