#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
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

        GUI.MultiButtonInput("animationMapActions",
            "playCharacterIdles", "Play Character Idles",
            "Plays the idle animation of every character in the loaded maps.",
            PlayCharacterIdles,
            "playAssetPoses", "Play Asset Poses",
            "Holds every corpse and other object in the loaded maps in the animation set on its map part.",
            PlayAssetPoses);
        GUI.Spacer();

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

    private void PlayCharacterIdles()
    {
        foreach ((MsbEntity entity, MeshRenderableProxy mesh) in GetLoadedEntities())
        {
            if (IsCharacter(entity))
            {
                // Sorted by ID, which puts the idle first
                _panel.QueueAnimation(CreateTarget(entity, mesh), package => package.Animations.FirstOrDefault());
            }
        }
    }

    private void PlayAssetPoses()
    {
        // Only Elden Ring has this data, as it is what tells a pose of 0 apart from an unset value
        var poseModels = Project.Handler.MapData.AssetMasks?.List
            .Select(e => e.model)
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>();

        foreach ((MsbEntity entity, MeshRenderableProxy mesh) in GetLoadedEntities())
        {
            if (GetPoseAnimationId(entity.WrappedObject, poseModels) is { } animationId)
            {
                _panel.QueueAnimation(CreateTarget(entity, mesh), package => package.FindAnimation(animationId));
            }
        }
    }

    private IEnumerable<(MsbEntity Entity, MeshRenderableProxy Mesh)> GetLoadedEntities()
    {
        foreach (MapWrapper map in Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            if (map.MapContainer?.OwnerView != View)
            {
                continue;
            }

            foreach (Entity obj in map.MapContainer.Objects)
            {
                if (obj is MsbEntity { RenderSceneMesh: MeshRenderableProxy mesh } entity)
                {
                    yield return (entity, mesh);
                }
            }
        }
    }

    /// <summary>
    ///     Gets the animation a map part starts in, which is how corpses are posed. Null if it has none.
    /// </summary>
    /// <param name="part">The MSB part.</param>
    /// <param name="poseModels">Models for which the pose digits are always meaningful, even when 0.</param>
    public static int? GetPoseAnimationId(object part, IReadOnlySet<string> poseModels)
    {
        int animationId = part switch
        {
            MSB1.Part.ObjectBase obj => obj.InitAnimID,
            MSB3.Part.ObjectBase obj => obj.AnimIDs[0],
            MSBB.Part.ObjectBase obj => obj.AnimIDs[0],
            MSBS.Part.ObjectBase obj => obj.AnimID,
            MSB_AC6.Part.Asset asset => asset.AssetSfxParamRelativeID,
            MSB_NR.Part.Asset asset => GetPoseDigits(asset.Unk58),
            MSBE.Part.Asset asset => GetPoseDigits(asset.UnkModelMaskAndAnimID),
            _ => -1
        };

        if (animationId > 0)
        {
            return animationId;
        }

        return animationId == 0 && part is MSBE.Part.Asset { ModelName: { } model } && poseModels.Contains(model)
            ? 0
            : null;
    }

    // The value is read as decimal digits: form, equipment and position, two digits each
    private static int GetPoseDigits(int value)
    {
        return value < 0 ? -1 : value % 100;
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
