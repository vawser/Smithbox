#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Draws what the events of the selected animation are doing at the current time: a marker
/// that grows over the event for effects and sounds, and the true size of attacks and bullets.
/// </summary>
public class TimeActEventDisplay
{
    // Enough for every hit of a few attacks at once, along with their effects
    private const int MaxShapes = 64;

    // How much a marker has grown by the end of its event
    private const float MarkerGrowth = 0.5f;

    private const int FillAlpha = 70;
    private const int WireAlpha = 220;

    private static readonly string[] BodyCenterNodes = { "Spine1", "Spine", "Pelvis", "Master" };

    private readonly TimeActEditorView _view;
    private readonly ProjectEntry _project;
    private readonly TimeActEventResolver _resolver;

    // Only the visuals of the animation on show are kept, as a character can have hundreds of thousands of events
    private readonly Dictionary<TAE.Event, TimeActEventVisual?> _visuals = new();
    private TAE.Animation? _visualsAnimation;

    // Attacks and bullets resolve through the params, so an edit in the Param Editor resolves them again
    private ActionManager? _paramActionManager;

    private readonly List<Shape> _shapes = new();
    private int _usedShapes;

    private TimeActContainer? _dummySource;
    private readonly Dictionary<int, List<Entity>> _bodyDummies = new();
    private readonly Dictionary<Entity, Vector3> _dummyRest = new();
    private Entity? _bodyCenter;

    // Weapons aren't part of the container, so their dummy polys come from their own files
    private readonly Dictionary<string, Dictionary<int, List<Vector3>>> _weaponDummies = new();

    private DbgPrimSolidCapsulePart? _solidDome;
    private DbgPrimSolidCapsulePart? _solidTube;
    private DbgPrimWireCapsulePart? _wireDome;
    private DbgPrimWireCapsulePart? _wireTube;

    public TimeActEventDisplay(TimeActEditorView view, ProjectEntry project)
    {
        _view = view;
        _project = project;
        _resolver = new TimeActEventResolver(project);
    }

    /// <summary>
    /// Forgets what the events resolved to, for when something they depend on has changed
    /// </summary>
    public void Invalidate()
    {
        _visuals.Clear();
    }

    /// <summary>
    /// Gets how an event was resolved, to show in the properties
    /// </summary>
    public string? GetStatus(TAE.Event ev)
    {
        var container = _view.Selection.SelectedTimeActModelWrapper?.Container;

        return container != null ? GetVisual(ev, container)?.Status : null;
    }

    /// <summary>
    /// Gets the BehaviorParam rows a judge ID of an event can pick, best first. The character in the viewport
    /// gives the weapon of the player, when it is the character the time act belongs to.
    /// </summary>
    public List<Andre.Formats.Param.Row> GetBehaviorRows(TAE.Event ev, string characterName, long judge, bool common)
    {
        var container = _view.Selection.SelectedTimeActModelWrapper?.Container;

        if (container != null && !string.Equals(container.Name, characterName, StringComparison.OrdinalIgnoreCase))
        {
            container = null;
        }

        return _resolver.GetBehaviorRows(ev, characterName, container, judge, common);
    }

    private TimeActEventVisual? GetVisual(TAE.Event ev, TimeActContainer container)
    {
        if (!_visuals.TryGetValue(ev, out var visual))
        {
            visual = _resolver.Resolve(ev, container);
            _visuals[ev] = visual;
        }

        return visual;
    }

    public void Update(ModelAnimator? animator, float time, float playbackEnd)
    {
        _usedShapes = 0;

        var animation = _view.Selection.SelectedAnimation;
        var container = _view.Selection.SelectedTimeActModelWrapper?.Container;

        WatchParamEdits();

        if (animation != _visualsAnimation)
        {
            _visualsAnimation = animation;
            _visuals.Clear();
        }

        var showMarkers = CFG.Current.TimeActEditor_Display_Event_Markers;
        var showAttacks = CFG.Current.TimeActEditor_Display_Attack_Capsules;

        if (animation != null && container != null && (showMarkers || showAttacks))
        {
            PrepareDummies(container);

            foreach (TAE.Event ev in animation.Events)
            {
                if (time < ev.StartTime || time >= ev.MemeEndTime)
                    continue;

                if (GetVisual(ev, container) is not { } visual)
                    continue;

                var isMarker = visual.Kind is TimeActVisualKind.Effect or TimeActVisualKind.Sound;

                if (isMarker ? !showMarkers : !showAttacks)
                    continue;

                var end = Math.Min(ev.MemeEndTime, Math.Max(playbackEnd, ev.StartTime));
                var progress = end > ev.StartTime ? Math.Clamp((time - ev.StartTime) / (end - ev.StartTime), 0, 1) : 0;

                // Attacks and bullets are shown at the size they really hit at
                var scale = isMarker ? CFG.Current.TimeActEditor_Event_Marker_Size * (1 + progress * MarkerGrowth) : 1;

                foreach (TimeActVisualShape shape in visual.Shapes)
                {
                    DrawShape(shape, visual.Kind, scale, container, animator);
                }
            }
        }

        for (var i = _usedShapes; i < _shapes.Count; i++)
        {
            _shapes[i].Hide();
        }
    }

    /// <summary>
    /// The Param Editor can be opened after this view, so its history is picked up once it exists
    /// </summary>
    private void WatchParamEdits()
    {
        var manager = _project.Handler.ParamEditor?.ActionManager;

        if (manager == _paramActionManager)
            return;

        if (_paramActionManager != null)
        {
            _paramActionManager.ActionPerformed -= OnParamsEdited;
        }

        _paramActionManager = manager;

        if (manager != null)
        {
            manager.ActionPerformed += OnParamsEdited;
        }
    }

    private void OnParamsEdited()
    {
        _resolver.ClearParamCache();

        Invalidate();
    }

    private void DrawShape(TimeActVisualShape shape, TimeActVisualKind kind, float scale, TimeActContainer container,
        ModelAnimator? animator)
    {
        List<Vector3> starts = GetPositions(shape.Dummy, shape.WeaponPart, container, animator);
        List<Vector3> ends = shape.EndDummy >= 0
            ? GetPositions(shape.EndDummy, shape.WeaponPart, container, animator)
            : starts;

        for (var i = 0; i < starts.Count; i++)
        {
            // A dummy poly ID can repeat, and each one then makes a hit of its own. Without
            // a matching end, they all reach for the same one.
            Vector3 end = ends.Count == 0 ? starts[i] : ends[Math.Min(i, ends.Count - 1)];

            if (_usedShapes >= MaxShapes)
                return;

            GetShape(_usedShapes++).Set(starts[i], end, shape.Radius * scale, GetColor(kind));
        }
    }

    private static Color GetColor(TimeActVisualKind kind)
    {
        return kind switch
        {
            TimeActVisualKind.Effect => Color.Orange,
            TimeActVisualKind.Sound => Color.DeepSkyBlue,
            TimeActVisualKind.Bullet => Color.Yellow,
            _ => Color.Red
        };
    }

    private void PrepareDummies(TimeActContainer container)
    {
        if (_dummySource == container)
            return;

        _dummySource = container;
        _bodyDummies.Clear();
        _dummyRest.Clear();
        _visuals.Clear();

        foreach (Entity dummy in container.Dummies)
        {
            var id = (int)((FLVER.Dummy)dummy.WrappedObject).ReferenceID;

            if (!_bodyDummies.TryGetValue(id, out var list))
            {
                list = new();
                _bodyDummies[id] = list;
            }

            list.Add(dummy);
        }

        _bodyCenter = BodyCenterNodes
            .Select(name => container.Nodes.FirstOrDefault(n => ((FLVER.Node)n.WrappedObject).Name == name))
            .FirstOrDefault(n => n != null);
    }

    private List<Vector3> GetPositions(int dummy, int weaponPart, TimeActContainer container, ModelAnimator? animator)
    {
        if (dummy < 0)
        {
            return new List<Vector3> { GetBodyCenter(container, animator) };
        }

        if (weaponPart >= 0 && GetWeaponPositions(dummy, weaponPart, container) is { Count: > 0 } onWeapon)
        {
            return onWeapon;
        }

        // Anything the weapon doesn't have is on the body, as with a kick
        if (!_bodyDummies.TryGetValue(dummy, out var entities))
        {
            return new List<Vector3>();
        }

        return entities.Select(e => GetBodyPosition(e, animator)).ToList();
    }

    private Vector3 GetBodyPosition(Entity entity, ModelAnimator? animator)
    {
        // Working out an entity's matrix walks all of its parents
        if (!_dummyRest.TryGetValue(entity, out Vector3 rest))
        {
            rest = entity.GetWorldMatrix().Translation;
            _dummyRest[entity] = rest;
        }

        var dummy = (FLVER.Dummy)entity.WrappedObject;

        // The parent bone is often a static helper, and only the attach bone really moves
        var node = dummy.AttachBoneIndex >= 0 ? dummy.AttachBoneIndex : dummy.ParentBoneIndex;

        return animator != null && node >= 0 && node < animator.NodeCount
            ? Vector3.Transform(rest, animator.GetNodeDelta(node))
            : rest;
    }

    private Vector3 GetBodyCenter(TimeActContainer container, ModelAnimator? animator)
    {
        if (_bodyCenter != null)
        {
            Vector3 rest = _bodyCenter.GetWorldMatrix().Translation;
            var node = container.Nodes.IndexOf(_bodyCenter);

            return animator != null && node < animator.NodeCount
                ? Vector3.Transform(rest, animator.GetNodeDelta(node))
                : rest;
        }

        return container.TryGetMeshBounds(out BoundingBox bounds) ? bounds.GetCenter() : Vector3.Zero;
    }

    private List<Vector3> GetWeaponPositions(int dummy, int weaponPart, TimeActContainer container)
    {
        var positions = new List<Vector3>();

        if (container.CharacterMesh is not { } mesh || container.CharacterAppearance is not { } appearance ||
            weaponPart >= mesh.Submeshes.Count)
        {
            return positions;
        }

        if (!GetWeaponDummies(appearance.Parts[weaponPart].Name).TryGetValue(dummy, out var points))
        {
            return positions;
        }

        Matrix4x4 attach = mesh.Submeshes[weaponPart].AttachTransform;

        foreach (Vector3 point in points)
        {
            positions.Add(Vector3.Transform(point, attach));
        }

        return positions;
    }

    private Dictionary<int, List<Vector3>> GetWeaponDummies(string partName)
    {
        if (_weaponDummies.TryGetValue(partName, out var dummies))
        {
            return dummies;
        }

        dummies = new();

        try
        {
            ResourceDescriptor asset = ModelLocator.GetPartsModel(_project, partName, partName);
            var path = PathBuilder.GetRelativePath(_project, asset.AssetVirtualPath);

            if (_project.VFS.FS.ReadFile(path) is { } data)
            {
                using IBinder binder = _project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES
                    ? BND3.Read(data)
                    : BND4.Read(data);

                if (binder.Files.FirstOrDefault(f => f.Name.Contains(".flv", StringComparison.OrdinalIgnoreCase)) is { } file)
                {
                    FLVER2 flver = StudioCore.Editors.ModelEditor.Flver0Converter.ReadEditorFlver(_project, file.Bytes);
                    ModelRig rig = ModelRig.FromFlver(flver);

                    foreach (FLVER.Dummy dummy in flver.Dummies)
                    {
                        Vector3 point = dummy.ParentBoneIndex >= 0 && dummy.ParentBoneIndex < rig.NodeReferenceWorld.Length
                            ? Vector3.Transform(dummy.Position, rig.NodeReferenceWorld[dummy.ParentBoneIndex])
                            : dummy.Position;

                        if (!dummies.TryGetValue(dummy.ReferenceID, out var list))
                        {
                            list = new();
                            dummies[dummy.ReferenceID] = list;
                        }

                        list.Add(point);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_EventDisplay_Failed_To_Read_Weapon", partName), e);
        }

        _weaponDummies[partName] = dummies;

        return dummies;
    }

    private Shape GetShape(int index)
    {
        if (index < _shapes.Count)
        {
            return _shapes[index];
        }

        _solidDome ??= new DbgPrimSolidCapsulePart(true);
        _solidTube ??= new DbgPrimSolidCapsulePart(false);
        _wireDome ??= new DbgPrimWireCapsulePart(true);
        _wireTube ??= new DbgPrimWireCapsulePart(false);

        var scene = _view.RenderScene;

        Shape shape = new(scene, _solidDome, _solidTube, _wireDome, _wireTube);
        _shapes.Add(shape);

        return shape;
    }

    /// <summary>
    /// A capsule, made up of a fill and a wireframe. These are reused from frame to frame, as
    /// creating a renderable takes more than moving one, and the same few are nearly always in use.
    /// </summary>
    private sealed class Shape
    {
        private readonly DebugPrimitiveRenderableProxy[] _domes;
        private readonly DebugPrimitiveRenderableProxy[] _tubes;

        private Color _color = Color.Empty;

        public Shape(RenderScene scene, IDbgPrim solidDome, IDbgPrim solidTube, IDbgPrim wireDome, IDbgPrim wireTube)
        {
            _domes = new[]
            {
                CreatePiece(scene, solidDome),
                CreatePiece(scene, solidDome),
                CreatePiece(scene, wireDome),
                CreatePiece(scene, wireDome)
            };

            _tubes = new[] { CreatePiece(scene, solidTube), CreatePiece(scene, wireTube) };
        }

        // In the overlay, as what these mark is often inside of the model
        private static DebugPrimitiveRenderableProxy CreatePiece(RenderScene scene, IDbgPrim prim)
        {
            return new DebugPrimitiveRenderableProxy(scene.OverlayRenderables, prim)
            {
                RenderTranslucent = true,
                Pickable = false,
                Visible = false
            };
        }

        public void Set(Vector3 start, Vector3 end, float radius, Color color)
        {
            if (color != _color)
            {
                _color = color;

                Color fill = Color.FromArgb(FillAlpha, color);
                Color wire = Color.FromArgb(WireAlpha, color);

                _domes[0].BaseColor = fill;
                _domes[1].BaseColor = fill;
                _domes[2].BaseColor = wire;
                _domes[3].BaseColor = wire;
                _tubes[0].BaseColor = fill;
                _tubes[1].BaseColor = wire;
            }

            var length = Vector3.Distance(start, end);
            Vector3 direction = length > 0 ? (end - start) / length : Vector3.UnitY;

            Matrix4x4 towards = RotationTo(direction);
            Matrix4x4 away = RotationTo(-direction);
            Matrix4x4 scale = Matrix4x4.CreateScale(radius);

            Matrix4x4 startDome = scale * away * Matrix4x4.CreateTranslation(start);
            Matrix4x4 endDome = scale * towards * Matrix4x4.CreateTranslation(end);
            Matrix4x4 tube = Matrix4x4.CreateScale(radius, length, radius) * towards * Matrix4x4.CreateTranslation(start);

            for (var i = 0; i < _domes.Length; i++)
            {
                _domes[i].World = i % 2 == 0 ? startDome : endDome;
                _domes[i].Visible = true;
            }

            foreach (DebugPrimitiveRenderableProxy piece in _tubes)
            {
                piece.World = tube;
                piece.Visible = length > 0;
            }
        }

        public void Hide()
        {
            foreach (DebugPrimitiveRenderableProxy piece in _domes.Concat(_tubes))
            {
                piece.Visible = false;
            }
        }

        private static Matrix4x4 RotationTo(Vector3 direction)
        {
            var dot = Vector3.Dot(Vector3.UnitY, direction);

            // Pointing straight down has no one axis to turn around
            if (dot < -0.9999f)
            {
                return Matrix4x4.CreateRotationX(Utils.Pi);
            }

            Vector3 axis = Vector3.Cross(Vector3.UnitY, direction);

            if (axis.LengthSquared() < 1e-8f)
            {
                return Matrix4x4.Identity;
            }

            return Matrix4x4.CreateFromAxisAngle(Vector3.Normalize(axis), MathF.Acos(Math.Clamp(dot, -1, 1)));
        }
    }
}
