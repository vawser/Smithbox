#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The clock shared by the event graph and the viewport. While the selected animation has a clip, the
/// animator keeps the time so that both show the same frame. An animation without one, such as in a game
/// that can't play clips or while a clip is loading, still runs on a clock of its own over its events.
/// </summary>
public class TimeActPlayback
{
    private const float DefaultFrameDuration = 1f / 30f;

    // How long an animation has to stay selected before it plays by itself
    private const float AutoPlayDelay = 0.5f;

    // Animations can import ones that import others, but not endlessly
    private const int MaxImportDepth = 8;

    public TimeActEditorView View;
    public ProjectEntry Project;

    private readonly AnimationPreviewPanel _panel;

    private readonly Dictionary<Entity, System.Numerics.Matrix4x4> _markerRestPose = new();

    private TimeActContainer? _target;
    private AnimationEntry? _boundClip;

    // Only the latest request plays, however the loads end up finishing
    private int _request;

    // The latest request has yet to pick its clip, and the clip it picked
    private bool _clipPending;
    private AnimationEntry? _expectedClip;

    private float _time;
    private bool _playing;

    // Counts down while an animation waits to play by itself. Selecting another one starts it over.
    private float _autoPlayCountdown;

    public bool Loop = true;
    public float Speed = 1;

    /// <summary>
    /// The selected animation has no clip that could be found
    /// </summary>
    public bool IsClipMissing { get; private set; }

    /// <summary>
    /// The clip of the selected animation is still on its way to the model. Once this is false, the
    /// clock runs on the clip, or on the events if there is no clip.
    /// </summary>
    public bool IsClipLoading => _clipPending ||
        !IsClipMissing && (Animator == null || Animator.CurrentAnimation != _expectedClip);

    public TimeActEventDisplay EventDisplay { get; }

    public TimeActPlayback(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        _panel = new AnimationPreviewPanel(project);

        EventDisplay = new TimeActEventDisplay(view, project);
        RootMotionDisplay = new TimeActRootMotionDisplay(view);
        Sounds = new TimeActSoundPlayback(view, project);
    }

    /// <summary>
    /// Plays the sound events as the clock passes them
    /// </summary>
    public TimeActSoundPlayback Sounds { get; }

    public TimeActRootMotionDisplay RootMotionDisplay { get; }

    private ModelAnimator? Animator => _target != null ? _panel.GetAnimator(_target) : null;

    public bool Playing => _playing;
    public float Time => _time;

    /// <summary>
    /// How long the clip of the animation is, or 0 if there is none
    /// </summary>
    public float ClipDuration => Animator?.Duration ?? 0;

    /// <summary>
    /// How long the animation really plays for. Events are free to run past this.
    /// </summary>
    public float PlaybackDuration => ClipDuration > 0 ? ClipDuration : GetEventDuration();

    public float FrameDuration => Animator?.CurrentClip is { FrameDuration: > 0 } clip
        ? clip.FrameDuration
        : DefaultFrameDuration;

    public void Update()
    {
        _panel.Update();

        UpdateAutoPlay();

        ModelAnimator? animator = Animator;

        if (animator?.CurrentAnimation != _boundClip)
        {
            _boundClip = animator?.CurrentAnimation;

            // The clip may have taken a while to load, so it joins in wherever the clock has got to
            if (animator != null)
            {
                animator.Seek(_time);
                animator.SetPlaying(_playing);
            }
        }

        if (animator != null)
        {
            animator.Loop = Loop;
            animator.Speed = Speed;

            // Takes effect with the next pose, which a change of this asks for
            animator.ApplyRootMotion = CFG.Current.TimeActEditor_Apply_Root_Motion;

            _time = animator.Time;
            _playing = animator.Playing;
        }
        else if (_playing)
        {
            AdvanceClock();
        }

        EventDisplay.Update(animator, _time, PlaybackDuration);
        RootMotionDisplay.Update(animator);

        // While a clip is on its way the clock stands still, and the sounds wait with it
        Sounds.Update(_time, _playing && !IsClipLoading);
    }

    private void UpdateAutoPlay()
    {
        if (_autoPlayCountdown <= 0)
            return;

        _autoPlayCountdown -= ImGui.GetIO().DeltaTime;

        if (_autoPlayCountdown <= 0)
        {
            // If the clip is still loading, it picks this up once it arrives
            _playing = true;
            Animator?.SetPlaying(true);
        }
    }

    private void AdvanceClock()
    {
        var duration = GetEventDuration();

        _time += ImGui.GetIO().DeltaTime * Speed;

        if (_time >= duration)
        {
            _time = Loop ? 0 : duration;
            _playing = Loop;
        }
    }

    /// <summary>
    /// How far the events of the animation reach, leaving out the ones that last until it ends
    /// </summary>
    public float GetEventDuration()
    {
        var events = View.Selection.SelectedAnimation?.Events;

        if (events == null || events.Count == 0)
            return 1;

        return Math.Max(events.Max(e => e.EndTime < TAE.Event.EldenRingInfiniteLengthEventPlaceholder ? e.EndTime : e.StartTime), DefaultFrameDuration);
    }

    public void TogglePlaying()
    {
        _autoPlayCountdown = 0;

        var finished = Animator is { } animator
            ? animator.Time >= animator.Duration
            : _time >= GetEventDuration();

        // A finished animation starts over
        if (!_playing && finished)
        {
            Seek(0);
        }

        _playing = !_playing;
        Animator?.SetPlaying(_playing);
    }

    public void Stop()
    {
        _autoPlayCountdown = 0;
        _playing = false;
        Animator?.SetPlaying(false);

        Sounds.StopAll();

        Seek(0);
    }

    /// <summary>
    /// Plays from the current time, without starting over at the end as <see cref="TogglePlaying"/> does
    /// </summary>
    public void Play()
    {
        _autoPlayCountdown = 0;
        _playing = true;
        Animator?.SetPlaying(true);
    }

    public void Seek(float time)
    {
        _time = Math.Max(time, 0);

        Animator?.Seek(_time);
    }

    public void Pause()
    {
        _autoPlayCountdown = 0;
        _playing = false;
        Animator?.SetPlaying(false);
    }

    /// <summary>
    /// Forgets about the current animation, for when the source changes
    /// </summary>
    public void Clear()
    {
        _request++;
        _panel.StopAll();
        Sounds.StopAll();

        _target = null;
        _boundClip = null;
        _time = 0;
        _playing = false;
        _autoPlayCountdown = 0;
        _clipPending = false;
        _expectedClip = null;
        IsClipMissing = false;
    }

    /// <summary>
    /// Shows the first frame of the selected animation on the loaded model
    /// </summary>
    public void OnAnimationChanged()
    {
        _time = 0;
        _playing = false;
        IsClipMissing = false;

        _autoPlayCountdown = CFG.Current.TimeActEditor_Auto_Play_Animation && View.Selection.SelectedAnimation != null
            ? AutoPlayDelay
            : 0;

        Animator?.Seek(0);
        Animator?.SetPlaying(false);

        QueueClip();
    }

    /// <summary>
    /// Changing what a character wears replaces the meshes that were being animated, so the clip
    /// starts over on the new ones. It picks up at the current time, as with any clip that loads late.
    /// </summary>
    public void OnPartsChanged()
    {
        if (View.Selection.SelectedAnimation == null)
            return;

        // Attacks of a player go by the weapon, which may have just changed
        EventDisplay.Invalidate();

        // Queued first, as the animations would otherwise be released along with the last thing playing them
        QueueClip();

        if (_target != null)
        {
            _panel.Stop(_target);
        }

        // It's the same clip that comes back, so this is what makes it join in at the current time
        _boundClip = null;
    }

    private void QueueClip()
    {
        var request = ++_request;

        _clipPending = false;
        _expectedClip = null;

        var selection = View.Selection;

        var source = selection.SelectedTimeActContainerWrapper;
        var entry = selection.SelectedTimeAct;
        var animation = selection.SelectedAnimation;
        var wrapper = selection.SelectedTimeActModelWrapper;

        if (source == null || entry == null || animation == null || wrapper?.Container == null ||
            !_panel.IsSupported || GetKind(wrapper) is not { } kind)
        {
            IsClipMissing = true;
            return;
        }

        var container = wrapper.Container;
        var characterMesh = container.CharacterMesh;

        _target = container;

        var target = new AnimationTarget(
            container,
            wrapper.Name,
            kind,
            wrapper.Parent.TargetFS,
            wrapper.Parent.Path,
            () => container.Meshes.Select(e => e.RenderSceneMesh).OfType<MeshRenderableProxy>().ToList(),
            () => wrapper.Container == container && selection.SelectedTimeActModelWrapper == wrapper &&
                  container.CharacterMesh == characterMesh,
            animator => UpdateMarkers(container, animator),
            characterMesh != null
                ? CharacterComposition.GetAnimatedParts(Project, container.CharacterAppearance!, characterMesh)
                : null);

        _clipPending = true;

        _panel.QueueAnimation(target, package =>
        {
            if (request != _request)
                return null;

            var clip = FindClip(package, source, entry, animation);

            _clipPending = false;
            _expectedClip = clip;

            if (clip == null)
            {
                IsClipMissing = true;

                // Leaving the last pose up would look like it belongs to this animation
                _panel.Stop(container);
            }

            return clip;
        }, true);
    }

    /// <summary>
    /// Follows the imports of an animation until it gets to one that has a clip
    /// </summary>
    private static AnimationEntry? FindClip(AnimationPackage package, TimeActContainerWrapper source,
        TimeActEntry entry, TAE.Animation animation)
    {
        for (var depth = 0; depth < MaxImportDepth; depth++)
        {
            var id = entry.GetFullAnimationID(animation, source.AnimationCategorySize);

            var clipID = animation.MiniHeader switch
            {
                TAE.Animation.AnimMiniHeader.Standard { ImportsHKX: true } standard => standard.ImportHKXSourceAnimID,
                TAE.Animation.AnimMiniHeader.ImportOtherAnim import => import.ImportFromAnimID,
                _ => id
            };

            if (package.FindAnimation((int)clipID) is { } clip)
            {
                return clip;
            }

            if (clipID == id || !source.TryFindAnimation(clipID, out entry!, out animation!))
            {
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Moves the bone and dummy markers along with the pose. Only the proxies move: the
    /// entities keep their rest transforms, as nothing about the pose belongs in the model.
    /// </summary>
    private void UpdateMarkers(TimeActContainer container, ModelAnimator? animator)
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

        // There can be hundreds of markers, and working out an entity's matrix walks all of its parents
        if (_markerRestPose.Count == 0)
        {
            foreach (Entity marker in container.Nodes.Concat(container.Dummies))
            {
                _markerRestPose[marker] = marker.GetWorldMatrix();
            }
        }

        RenderFilter filter = View.RenderScene.DrawFilter;

        if ((filter & RenderFilter.Nodes) != 0)
        {
            for (var i = 0; i < container.Nodes.Count && i < animator.NodeCount; i++)
            {
                MoveMarker(container.Nodes[i], animator, i);
            }
        }

        if ((filter & RenderFilter.Dummies) != 0)
        {
            foreach (Entity marker in container.Dummies)
            {
                // The parent bone is often a static helper, and only the attach bone really moves
                var dummy = (FLVER.Dummy)marker.WrappedObject;
                MoveMarker(marker, animator, dummy.AttachBoneIndex >= 0 ? dummy.AttachBoneIndex : dummy.ParentBoneIndex);
            }
        }
    }

    private void MoveMarker(Entity marker, ModelAnimator animator, int node)
    {
        if (marker.RenderSceneMesh != null && node >= 0 && node < animator.NodeCount)
        {
            marker.RenderSceneMesh.World = _markerRestPose[marker] * animator.GetNodeDelta(node);
        }
    }

    private static AnimatedModelKind? GetKind(TimeActModelWrapper wrapper)
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
