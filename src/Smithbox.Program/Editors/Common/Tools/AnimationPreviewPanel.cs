#nullable enable
using Andre.IO.VFS;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.Common;

/// <summary>
///     What the panel animates, as described by the editor hosting it.
/// </summary>
/// <param name="Key">Identifies the animated thing across frames, e.g. a map entity or a loaded model.</param>
/// <param name="ModelName">Name of the model within its binder.</param>
/// <param name="Kind">Decides where the animations of the model are stored.</param>
/// <param name="FileSystem">The file system to read the model from.</param>
/// <param name="ContainerPath">The binder holding the model.</param>
/// <param name="GetMeshes">The meshes to drive. Empty while the model is still loading.</param>
/// <param name="IsAlive">False once the meshes have gone, which ends playback.</param>
public sealed record AnimationTarget(
    object Key,
    string ModelName,
    AnimatedModelKind Kind,
    VirtualFileSystem FileSystem,
    string ContainerPath,
    Func<IReadOnlyList<MeshRenderableProxy>> GetMeshes,
    Func<bool> IsAlive);

/// <summary>
///     Previews the Havok animations of a model in the viewport.
///     Nothing here touches the edited data: poses only exist in the renderer.
/// </summary>
public class AnimationPreviewPanel
{
    private readonly ProjectEntry _project;

    // Every placement of a model shares the same animations
    private readonly Dictionary<string, Task<LoadedModel?>> _models = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<object, ActiveAnimation> _active = new();

    private string _filter = "";

    public AnimationPreviewPanel(ProjectEntry project)
    {
        _project = project;
    }

    public bool IsSupported => AnimationPackage.IsSupportedGame(_project.Descriptor.ProjectType);

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

        foreach ((var key, ActiveAnimation active) in _active.ToArray())
        {
            if (!active.Target.IsAlive())
            {
                Stop(key);
                continue;
            }

            if (active.Animator.Update(deltaTime))
            {
                foreach (MeshRenderableProxy mesh in active.Meshes)
                {
                    mesh.Animator = active.Animator;
                }
            }
        }
    }

    /// <summary>
    ///     Displays the list of playing animations, for editors that can animate several things at once.
    /// </summary>
    public void DisplayActiveSummary()
    {
        if (_active.Count == 0)
        {
            return;
        }

        GUI.WrappedText($"Playing on {_active.Count} {(_active.Count == 1 ? "entity" : "entities")}.");
        GUI.MultiButtonInput("animationGlobalActions",
            "stopAll", "Stop All", "Stop every animation and return to the rest pose.", StopAll);
        GUI.Spacer();
    }

    public void Display(AnimationTarget target)
    {
        GUI.SimpleHeader($"Model: {target.ModelName}", "The model being previewed.");

        Task<LoadedModel?> load = GetOrLoadModel(target);

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
            DisplayControls(target, load.Result);
        }
    }

    private void DisplayControls(AnimationTarget target, LoadedModel model)
    {
        _active.TryGetValue(target.Key, out ActiveAnimation? active);
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
                Stop(target.Key);
                return;
            }

            ImGui.SameLine();
            ImGui.Checkbox("Loop##animationLoop", ref animator.Loop);

            ImGui.SetNextItemWidth(-1);
            ImGui.SliderFloat("##animationSpeed", ref animator.Speed, 0.05f, 2f, "Speed: %.2f");

            GUI.Spacer();
        }

        GUI.SimpleHeader($"Animations ({model.Package.Animations.Count})", "Click an animation to play it.");

        if (!model.Package.SourceModel.Equals(target.ModelName, StringComparison.OrdinalIgnoreCase))
        {
            GUI.WrappedText($"Shared from {model.Package.SourceModel}.");
        }

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
                    Play(target, model, entry);
                }
            }
        }

        ImGui.EndChild();
    }

    private void Play(AnimationTarget target, LoadedModel model, AnimationEntry entry)
    {
        if (!_active.TryGetValue(target.Key, out ActiveAnimation? active))
        {
            IReadOnlyList<MeshRenderableProxy> meshes = target.GetMeshes();
            if (meshes.Count == 0)
            {
                return;
            }

            active = new ActiveAnimation(target, meshes, new ModelAnimator(model.Package, model.Rig));
            _active.Add(target.Key, active);
        }

        if (active.Animator.Play(entry))
        {
            active.Animator.Update(0);

            foreach (MeshRenderableProxy mesh in active.Meshes)
            {
                mesh.Animator = active.Animator;
            }
        }
        else if (active.Animator.CurrentClip == null)
        {
            Stop(target.Key);
        }
    }

    private void Stop(object key)
    {
        if (!_active.Remove(key, out ActiveAnimation? active))
        {
            return;
        }

        foreach (MeshRenderableProxy mesh in active.Meshes)
        {
            mesh.Animator = null;
        }

        active.Animator.Dispose();

        ReleaseUnusedModels();
    }

    public void StopAll()
    {
        foreach (var key in _active.Keys.ToArray())
        {
            Stop(key);
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

    private Task<LoadedModel?> GetOrLoadModel(AnimationTarget target)
    {
        // The same model can come from different file systems in the Model Editor
        var key = $"{target.ContainerPath}|{target.ModelName}|{target.FileSystem.GetHashCode()}";

        if (!_models.TryGetValue(key, out Task<LoadedModel?>? load))
        {
            load = Task.Run(() => LoadModel(target));
            _models.Add(key, load);
        }

        return load;
    }

    private LoadedModel? LoadModel(AnimationTarget target)
    {
        try
        {
            AnimationPackage? package = AnimationPackage.Load(target.FileSystem, _project.Descriptor.ProjectType,
                target.Kind, target.ModelName, target.ContainerPath);

            if (package == null)
            {
                return null;
            }

            ModelRig? rig = target.FileSystem.ReadFile(target.ContainerPath) is { } container
                ? ModelRig.Read(package, container, target.ModelName)
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
            Smithbox.LogError(this, $"Failed to load the animations of {target.ModelName}", LogPriority.High, ex);
            throw;
        }
    }

    private sealed record LoadedModel(AnimationPackage Package, ModelRig Rig);

    private sealed record ActiveAnimation(
        AnimationTarget Target,
        IReadOnlyList<MeshRenderableProxy> Meshes,
        ModelAnimator Animator);
}
