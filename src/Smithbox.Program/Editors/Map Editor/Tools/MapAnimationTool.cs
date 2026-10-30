#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
///     Previews the Havok animations of characters and assets in the viewport.
///     Nothing here touches the map: poses only exist in the renderer.
/// </summary>
public class MapAnimationTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    // Keyed by model name, as every placement of a model shares the same animations
    private readonly Dictionary<string, Task<LoadedModel?>> _models = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<MsbEntity, ActiveAnimation> _active = new();

    private string _filter = "";

    public MapAnimationTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    ///     Advances every playing animation. This runs each frame even with the tool window
    ///     closed, as animations keep playing once they have been started.
    /// </summary>
    public void Update()
    {
        if (_active.Count == 0)
        {
            return;
        }

        var deltaTime = ImGui.GetIO().DeltaTime;

        foreach ((MsbEntity entity, ActiveAnimation active) in _active.ToArray())
        {
            // The mesh is replaced or removed when the model is changed or the map is unloaded
            if (entity.RenderSceneMesh != active.Mesh)
            {
                Stop(entity);
                continue;
            }

            if (active.Animator.Update(deltaTime))
            {
                active.Mesh.Animator = active.Animator;
            }
        }
    }

    /// <summary>
    ///     Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        ImGui.BeginChild("MapAnimationToolSection", ImGuiChildFlags.Borders);

        if (!AnimationPackage.IsSupportedGame(Project.Descriptor.ProjectType))
        {
            GUI.WrappedText("Animation playback is not available for this game.");
            ImGui.EndChild();
            return;
        }

        if (_active.Count > 0)
        {
            GUI.WrappedText($"Playing on {_active.Count} {(_active.Count == 1 ? "entity" : "entities")}.");
            GUI.MultiButtonInput("animationGlobalActions",
                "stopAll", "Stop All", "Stop every animation and return to the rest pose.", StopAll);
            GUI.Spacer();
        }

        MsbEntity? entity = GetSelectedEntity();
        if (entity == null)
        {
            GUI.WrappedText("Select a character or asset to preview its animations.");
            ImGui.EndChild();
            return;
        }

        var modelName = GetModelName(entity);
        GUI.SimpleHeader($"Model: {modelName}", "The model of the selected entity.");

        Task<LoadedModel?> load = GetOrLoadModel(entity, modelName);

        if (!load.IsCompleted)
        {
            GUI.WrappedText("Loading animations...");
        }
        else if (load.IsFaulted)
        {
            GUI.WrappedText("The animations of this model could not be read. See the log for details.");
        }
        else if (load.Result == null)
        {
            GUI.WrappedText("This model has no animations.");
        }
        else
        {
            DisplayControls(entity, load.Result);
        }

        ImGui.EndChild();
    }

    private void DisplayControls(MsbEntity entity, LoadedModel model)
    {
        _active.TryGetValue(entity, out ActiveAnimation? active);
        ModelAnimator? animator = active?.Animator;

        if (animator is { CurrentClip: not null })
        {
            GUI.SimpleHeader("Playback", "");

            ImGui.SetNextItemWidth(-1);
            var time = animator.Time;
            if (ImGui.SliderFloat("##animationTime", ref time, 0, animator.Duration,
                    $"{time:F2} / {animator.Duration:F2} s (frame {(int)animator.Frame})"))
            {
                animator.SetPlaying(false);
                animator.Seek(time);
            }

            if (ImGui.Button(animator.Playing ? "Pause##animationPause" : "Play##animationPause"))
            {
                // A finished clip starts over
                if (!animator.Playing && animator.Time >= animator.Duration)
                {
                    animator.Seek(0);
                }

                animator.SetPlaying(!animator.Playing);
            }

            ImGui.SameLine();
            if (ImGui.Button("Stop##animationStop"))
            {
                Stop(entity);
                return;
            }

            ImGui.SameLine();
            ImGui.Checkbox("Loop##animationLoop", ref animator.Loop);

            ImGui.SetNextItemWidth(-1);
            ImGui.SliderFloat("##animationSpeed", ref animator.Speed, 0.05f, 2f, "Speed: %.2f");

            GUI.Spacer();
        }

        GUI.SimpleHeader($"Animations ({model.Package.Animations.Count})", "Click an animation to play it.");

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##animationFilter", "Filter...", ref _filter, 255);

        if (ImGui.BeginChild("animationList", default, ImGuiChildFlags.Borders))
        {
            foreach (AnimationEntry entry in model.Package.Animations)
            {
                if (_filter.Length > 0 && !entry.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var selected = animator?.CurrentAnimation == entry;
                if (ImGui.Selectable($"{entry.Name}{(entry.Failed ? " (unreadable)" : "")}##{entry.Id}", selected))
                {
                    Play(entity, model, entry);
                }
            }
        }

        ImGui.EndChild();
    }

    private void Play(MsbEntity entity, LoadedModel model, AnimationEntry entry)
    {
        if (entity.RenderSceneMesh is not MeshRenderableProxy mesh)
        {
            return;
        }

        if (!_active.TryGetValue(entity, out ActiveAnimation? active))
        {
            active = new ActiveAnimation(mesh, new ModelAnimator(model.Package, model.Rig));
            _active.Add(entity, active);
        }

        if (active.Animator.Play(entry))
        {
            active.Animator.Update(0);
            mesh.Animator = active.Animator;
        }
        else if (active.Animator.CurrentClip == null)
        {
            Stop(entity);
        }
    }

    private void Stop(MsbEntity entity)
    {
        if (!_active.Remove(entity, out ActiveAnimation? active))
        {
            return;
        }

        active.Mesh.Animator = null;
        active.Animator.Dispose();

        ReleaseUnusedModels();
    }

    private void StopAll()
    {
        foreach (MsbEntity entity in _active.Keys.ToArray())
        {
            Stop(entity);
        }
    }

    private void ReleaseUnusedModels()
    {
        foreach ((var name, Task<LoadedModel?> load) in _models.ToArray())
        {
            if (!load.IsCompleted)
            {
                continue;
            }

            LoadedModel? model = load.IsCompletedSuccessfully ? load.Result : null;
            if (model != null && _active.Values.Any(a => a.Animator.Package == model.Package))
            {
                continue;
            }

            model?.Package.Dispose();
            _models.Remove(name);
        }
    }

    private MsbEntity? GetSelectedEntity()
    {
        HashSet<MsbEntity> selection = View.ViewportSelection.GetFilteredSelection<MsbEntity>();
        if (selection.Count != 1)
        {
            return null;
        }

        MsbEntity entity = selection.First();
        return IsAnimatable(entity) ? entity : null;
    }

    private static bool IsAnimatable(MsbEntity entity)
    {
        return IsCharacter(entity) || EntityHelper.IsPartAsset(entity) || EntityHelper.IsPartDummyAsset(entity);
    }

    private static bool IsCharacter(MsbEntity entity)
    {
        return EntityHelper.IsPartEnemy(entity) || EntityHelper.IsPartDummyEnemy(entity);
    }

    private static string GetModelName(MsbEntity entity)
    {
        return entity.GetProperty("ModelName")?.GetValue(entity.WrappedObject) as string ?? "";
    }

    private Task<LoadedModel?> GetOrLoadModel(MsbEntity entity, string modelName)
    {
        if (_models.TryGetValue(modelName, out Task<LoadedModel?>? load))
        {
            return load;
        }

        AnimatedModelKind kind = IsCharacter(entity) ? AnimatedModelKind.Character : AnimatedModelKind.Asset;

        ResourceDescriptor asset = kind == AnimatedModelKind.Character
            ? ModelLocator.GetChrModel(Project, modelName, modelName)
            : ModelLocator.GetObjModel(Project, modelName, modelName);

        var containerPath = PathBuilder.GetRelativePath(Project, asset.AssetVirtualPath);

        load = Task.Run(() => LoadModel(kind, modelName, containerPath));
        _models.Add(modelName, load);

        return load;
    }

    private LoadedModel? LoadModel(AnimatedModelKind kind, string modelName, string containerPath)
    {
        try
        {
            ProjectType game = Project.Descriptor.ProjectType;

            AnimationPackage? package = AnimationPackage.Load(Project.VFS.FS, game, kind, modelName, containerPath);
            if (package == null)
            {
                return null;
            }

            ModelRig? rig = Project.VFS.FS.ReadFile(containerPath) is { } container
                ? ModelRig.Read(package, container, modelName)
                : null;

            if (rig == null)
            {
                package.Dispose();
                return null;
            }

            return new LoadedModel(package, rig);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, $"Failed to load the animations of {modelName}", LogPriority.High, ex);
            throw;
        }
    }

    private sealed record LoadedModel(AnimationPackage Package, ModelRig Rig);

    private sealed record ActiveAnimation(MeshRenderableProxy Mesh, ModelAnimator Animator);
}
