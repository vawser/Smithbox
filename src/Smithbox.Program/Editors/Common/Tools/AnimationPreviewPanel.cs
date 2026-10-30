#nullable enable
using Andre.IO.VFS;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
/// <param name="PoseChanged">Called with each new pose, then with null when playback ends.</param>
/// <param name="Parts">Further models that are skinned to the same skeleton, each with bones of its own.</param>
public sealed record AnimationTarget(
    object Key,
    string ModelName,
    AnimatedModelKind Kind,
    VirtualFileSystem FileSystem,
    string ContainerPath,
    Func<IReadOnlyList<MeshRenderableProxy>> GetMeshes,
    Func<bool> IsAlive,
    Action<ModelAnimator?>? PoseChanged = null,
    IReadOnlyList<AnimatedPart>? Parts = null);

/// <summary>
///     A model that follows the animations of the character wearing it.
/// </summary>
/// <param name="ModelName">Name of the model within its binder.</param>
/// <param name="ContainerPath">The binder holding the model.</param>
/// <param name="Mesh">The mesh to drive.</param>
/// <param name="AttachNode">
///     The node of the character that the whole model is held by, as with a weapon. Null if it is skinned instead.
/// </param>
/// <param name="AttachOffset">How the model sits on <paramref name="AttachNode" />.</param>
public sealed record AnimatedPart(string ModelName, string ContainerPath, MeshRenderableProxy Mesh,
    string? AttachNode = null, Matrix4x4? AttachOffset = null);

/// <summary>
///     Previews the Havok animations of a model in the viewport.
///     Nothing here touches the edited data: poses only exist in the renderer.
/// </summary>
public class AnimationPreviewPanel
{
    private const int PendingBudgetMilliseconds = 8;

    private readonly ProjectEntry _project;

    // Every placement of a model shares the same animations
    private readonly Dictionary<string, Task<LoadedModel?>> _models = new(StringComparer.OrdinalIgnoreCase);

    // Characters tend to have equipment in common
    private readonly Dictionary<string, Task<ModelRig?>> _partRigs = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<object, ActiveAnimation> _active = new();

    // Requests wait here for their model to load
    private readonly List<PendingAnimation> _pending = new();

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
        StartPendingAnimations();

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
                OnPoseChanged(active);
            }
        }
    }

    private static void OnPoseChanged(ActiveAnimation active)
    {
        if (active.Animator.HasBoneFollowingMeshes)
        {
            foreach (MeshRenderableProxy mesh in active.Meshes)
            {
                mesh.Animator = active.Animator;
            }
        }

        foreach (ActivePart part in active.Parts)
        {
            part.Animator.Follow(active.Animator);

            if (part.Animator.HasBoneFollowingMeshes)
            {
                part.Mesh.Animator = part.Animator;
            }
        }

        UpdateAttachments(active);

        active.Target.PoseChanged?.Invoke(active.Animator);
    }

    private static void UpdateAttachments(ActiveAnimation active)
    {
        foreach (ActiveAttachment attachment in active.Attachments)
        {
            attachment.Mesh.AttachTransform = attachment.Offset * active.Animator.GetNodeWorld(attachment.Node);
        }
    }

    /// <summary>
    ///     Plays an animation once the model has loaded, for starting many at once without waiting on each.
    /// </summary>
    /// <param name="target">What to animate.</param>
    /// <param name="selectAnimation">Picks the animation. Null leaves the target alone.</param>
    public void QueueAnimation(AnimationTarget target, Func<AnimationPackage, AnimationEntry?> selectAnimation)
    {
        _pending.Add(new PendingAnimation(target, GetOrLoadModel(target), selectAnimation));
    }

    private void StartPendingAnimations()
    {
        if (_pending.Count == 0)
        {
            return;
        }

        // Decoding clips and uploading skin data adds up over a whole map, so spread it across frames
        var timer = Stopwatch.StartNew();

        for (var i = _pending.Count - 1; i >= 0 && timer.ElapsedMilliseconds < PendingBudgetMilliseconds; i--)
        {
            PendingAnimation pending = _pending[i];
            if (!pending.Load.IsCompleted)
            {
                continue;
            }

            _pending.RemoveAt(i);

            if (pending.Load.IsCompletedSuccessfully && pending.Load.Result is { } model && pending.Target.IsAlive() &&
                pending.SelectAnimation(model.Package) is { } entry)
            {
                Play(pending.Target, model, entry);
            }
        }
    }

    /// <summary>
    ///     Displays the list of playing animations, for editors that can animate several things at once.
    /// </summary>
    public void DisplayActiveSummary()
    {
        if (_pending.Count > 0)
        {
            GUI.WrappedText($"Starting {_pending.Count} more...");
        }

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
            // Armor can come with hundreds of megabytes of textures to skip over, so read its bones on the side
            Task<ModelRig?[]> rigs = GetOrLoadPartRigs(target);
            if (!rigs.IsCompleted)
            {
                _pending.Add(new PendingAnimation(target, GetOrLoadModel(target), _ => entry));
                return;
            }

            IReadOnlyList<MeshRenderableProxy> meshes = target.GetMeshes();
            List<ActivePart> parts = CreateParts(target, model, rigs);
            List<ActiveAttachment> attachments = CreateAttachments(target, model);
            if (meshes.Count == 0 && parts.Count == 0 && attachments.Count == 0)
            {
                return;
            }

            active = new ActiveAnimation(target, meshes, new ModelAnimator(model.Package, model.Rig), parts, attachments);
            _active.Add(target.Key, active);
        }

        if (active.Animator.Play(entry))
        {
            active.Animator.Update(0);

            foreach (MeshRenderableProxy mesh in active.Meshes)
            {
                mesh.Animator = active.Animator;
            }

            foreach (ActivePart part in active.Parts)
            {
                part.Animator.Follow(active.Animator);
                part.Mesh.Animator = part.Animator;
            }

            UpdateAttachments(active);

            active.Target.PoseChanged?.Invoke(active.Animator);
        }
        else if (active.Animator.CurrentClip == null)
        {
            Stop(target.Key);
        }
    }

    private static List<ActivePart> CreateParts(AnimationTarget target, LoadedModel model, Task<ModelRig?[]> rigs)
    {
        List<ActivePart> parts = new();

        if (target.Parts == null || !rigs.IsCompletedSuccessfully)
        {
            return parts;
        }

        for (var i = 0; i < target.Parts.Count; i++)
        {
            // A part that can't be read stays in its rest pose
            if (rigs.Result[i] is { } rig)
            {
                parts.Add(new ActivePart(target.Parts[i].Mesh, new ModelAnimator(model.Package, rig)));
            }
        }

        return parts;
    }

    private static List<ActiveAttachment> CreateAttachments(AnimationTarget target, LoadedModel model)
    {
        List<ActiveAttachment> attachments = new();

        foreach (AnimatedPart part in target.Parts ?? Array.Empty<AnimatedPart>())
        {
            var node = part.AttachNode != null ? model.Rig.FindNode(part.AttachNode) : -1;

            if (node >= 0)
            {
                attachments.Add(new ActiveAttachment(part.Mesh, node, part.AttachOffset ?? Matrix4x4.Identity,
                    part.Mesh.AttachTransform));
            }
        }

        return attachments;
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

        foreach (ActivePart part in active.Parts)
        {
            part.Mesh.Animator = null;
            part.Animator.Dispose();
        }

        foreach (ActiveAttachment attachment in active.Attachments)
        {
            attachment.Mesh.AttachTransform = attachment.Rest;
        }

        active.Target.PoseChanged?.Invoke(null);
        active.Animator.Dispose();

        ReleaseUnusedModels();
    }

    public void StopAll()
    {
        _pending.Clear();

        foreach (var key in _active.Keys.ToArray())
        {
            Stop(key);
        }
    }

    private void ReleaseUnusedModels()
    {
        foreach ((var name, Task<LoadedModel?> load) in _models.ToArray())
        {
            if (!load.IsCompleted || _pending.Any(p => p.Load == load))
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

        if (_active.Count == 0 && _pending.Count == 0)
        {
            foreach ((var key, Task<ModelRig?> rig) in _partRigs.ToArray())
            {
                if (rig.IsCompleted)
                {
                    _partRigs.Remove(key);
                }
            }
        }
    }

    private Task<ModelRig?[]> GetOrLoadPartRigs(AnimationTarget target)
    {
        if (target.Parts == null)
        {
            return Task.FromResult(Array.Empty<ModelRig?>());
        }

        return Task.WhenAll(target.Parts.Select(part =>
        {
            // These move as a whole, so their own bones don't matter
            if (part.AttachNode != null)
            {
                return Task.FromResult<ModelRig?>(null);
            }

            var key = $"{part.ContainerPath}|{part.ModelName}|{target.FileSystem.GetHashCode()}";

            if (!_partRigs.TryGetValue(key, out Task<ModelRig?>? load))
            {
                load = Task.Run(() => LoadPartRig(target, part));
                _partRigs.Add(key, load);
            }

            return load;
        }));
    }

    private ModelRig? LoadPartRig(AnimationTarget target, AnimatedPart part)
    {
        try
        {
            return target.FileSystem.ReadFile(part.ContainerPath) is { } container
                ? ModelRig.Read(_project.Descriptor.ProjectType, container, part.ModelName)
                : null;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, $"Failed to read the bones of {part.ModelName}", LogPriority.High, ex);
            return null;
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
                ? ModelRig.Read(package.Game, container, target.ModelName)
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

    private sealed record PendingAnimation(
        AnimationTarget Target,
        Task<LoadedModel?> Load,
        Func<AnimationPackage, AnimationEntry?> SelectAnimation);

    private sealed record ActiveAnimation(
        AnimationTarget Target,
        IReadOnlyList<MeshRenderableProxy> Meshes,
        ModelAnimator Animator,
        IReadOnlyList<ActivePart> Parts,
        IReadOnlyList<ActiveAttachment> Attachments);

    private sealed record ActivePart(MeshRenderableProxy Mesh, ModelAnimator Animator);

    private sealed record ActiveAttachment(MeshRenderableProxy Mesh, int Node, Matrix4x4 Offset, Matrix4x4 Rest);
}
