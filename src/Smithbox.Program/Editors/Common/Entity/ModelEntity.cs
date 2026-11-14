using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Editors.Common;

/// <summary>
/// Entity used within the Model Editor.
/// </summary>
public class ModelEntity : Entity
{
    protected IUniverse Owner;
    public ModelEntityType Type { get; set; }

    // Meshes have no transform in the FLVER: theirs is always identity, and a move is written into their vertices
    // (see GetUpdateTransformAction), but having one lets the gizmo move them
    public override bool HasTransform => Type is ModelEntityType.Dummy or ModelEntityType.Node or ModelEntityType.Mesh;

    /// <summary>
    /// For meshes: how the mesh's data differs from the geometry its drawable was built with. A move is written into
    /// the vertices at once, but the drawable only catches up with the next preview of the edited model, so until
    /// then it is drawn moved by this.
    /// </summary>
    public Matrix4x4 DrawableOffset { get; set; } = Matrix4x4.Identity;

    public override string ViewportLabel => WrappedObject switch
    {
        FLVER.Dummy dummy => dummy.ReferenceID.ToString(),
        FLVER.Node node => node.Name,
        _ => null
    };

    public ModelEntity(IUniverse owner) : base(owner)
    {
        Owner = owner;
    }

    public ModelEntity(IUniverse owner, ObjectContainer container, object internalObject) : base(owner, container, internalObject)
    {
        Owner = owner;
        Container = (ModelContainer)container;
        WrappedObject = internalObject;
    }

    public ModelEntity(IUniverse owner, ObjectContainer container, object internalObject, ModelEntityType entityType) : base(owner, container, internalObject)
    {
        Owner = owner;
        Container = (ModelContainer)container;
        WrappedObject = internalObject;
        Type = entityType;
    }

    public override void UpdateRenderModel()
    {
        base.UpdateRenderModel();

        if (Type is not ModelEntityType.Mesh || RenderSceneMesh == null || Container is not ModelContainer container)
            return;

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL || !CFG.Current.Viewport_Enable_Rendering)
            return;

        // A drag in progress is drawn as the move it will write, beneath it the move the drawable hasn't caught up
        // with yet, and above both the viewport's display rotation
        var drag = ActiveTemporaryTransform is { } temp ? temp.WorldMatrix : Matrix4x4.Identity;

        RenderSceneMesh.World = DrawableOffset * drag * container.DisplayMatrix;
    }

    #region Gizmo
    private bool IsGizmoMovable => Type is ModelEntityType.Dummy or ModelEntityType.Node or ModelEntityType.Mesh;

    /// <summary>
    /// The matrix the parents place this object with, built the way its marker is drawn: the bones above it, then
    /// the root's display rotation
    /// </summary>
    private Matrix4x4 GetParentMatrix()
    {
        var m = Matrix4x4.Identity;

        for (Entity p = Parent; p != null; p = p.Parent)
        {
            m *= p.GetLocalTransform().WorldMatrix;
        }

        return m;
    }

    // A mesh's own transform is always identity: its moves are written into its vertices
    private Matrix4x4 GetGizmoLocalMatrix()
    {
        return Type is ModelEntityType.Mesh ? Matrix4x4.Identity : GetLocalTransform().WorldMatrix;
    }

    /// <summary>
    /// Works the gizmo's drag out in matrices. Bones and dummies sit within their parent bones, so the drag, made in
    /// the viewport's space, is carried into the parent's space: new local = local · parent · drag · parent⁻¹.
    /// The drag turns and scales about the gizmo's origin, which for a mesh is the middle of its geometry.
    /// </summary>
    public override Transform ProjectGizmoDelta(Transform original, Transform current)
    {
        if (!IsGizmoMovable || !Matrix4x4.Invert(original.WorldMatrix, out var toGizmo))
            return base.ProjectGizmoDelta(original, current);

        var parent = GetParentMatrix();

        if (!Matrix4x4.Invert(parent, out var toParent))
            return base.ProjectGizmoDelta(original, current);

        var drag = toGizmo * current.WorldMatrix;
        var local = GetGizmoLocalMatrix() * parent * drag * toParent;

        // Meshes write the whole matrix into their vertices, shear and all
        if (Type is ModelEntityType.Mesh)
            return new Transform(local);

        if (!Matrix4x4.Decompose(local, out var scale, out var rotation, out var position))
            return new Transform(local.Translation, GetLocalTransform().Rotation, GetLocalTransform().Scale);

        return new Transform(position, rotation, scale);
    }

    /// <summary>
    /// Where the gizmo sits for this object, in the viewport's space: the drawn position and rotation of a bone or
    /// dummy, and the middle of a mesh's geometry
    /// </summary>
    public override Transform GetRootLocalTransform()
    {
        if (!IsGizmoMovable)
            return base.GetRootLocalTransform();

        var parent = GetParentMatrix();

        if (Type is ModelEntityType.Mesh)
        {
            var rotation = Matrix4x4.Decompose(parent, out _, out var parentRotation, out _)
                ? parentRotation
                : Quaternion.Identity;

            return new Transform(Vector3.Transform(GetMeshPivot(), parent), rotation, Vector3.One);
        }

        var world = GetLocalTransform().WorldMatrix * parent;

        if (Matrix4x4.Decompose(world, out _, out var worldRotation, out var worldPosition))
            return new Transform(worldPosition, worldRotation, Vector3.One);

        return new Transform(world.Translation, Quaternion.Identity, Vector3.One);
    }

    /// <summary>
    /// Writes a gizmo move: into the vertices of a mesh, into the translation, rotation and scale of a bone, and into
    /// the position and direction vectors of a dummy. Parts the move left as they were aren't written, so a move
    /// doesn't add rounding noise to a rotation it didn't change.
    /// </summary>
    public override ViewportAction GetUpdateTransformAction(Transform newt, bool includeScale = false)
    {
        if (Type is ModelEntityType.Mesh && Container is ModelContainer container)
        {
            return new MeshTransformAction(container, this, newt.WorldMatrix);
        }

        if (WrappedObject is FLVER.Node node)
        {
            return GetNodeTransformAction(node, newt, includeScale);
        }

        if (WrappedObject is FLVER.Dummy dummy)
        {
            return GetDummyTransformAction(dummy, newt);
        }

        return base.GetUpdateTransformAction(newt, includeScale);
    }

    private ViewportAction GetNodeTransformAction(FLVER.Node node, Transform newt, bool includeScale)
    {
        var current = GetLocalTransform();
        var type = node.GetType();

        var act = new PropChangeAction(this, node);

        act.AddPropertyChange(type.GetProperty(nameof(FLVER.Node.Translation)), newt.Position);

        // FLVER bones store XZY euler radians
        if (!IsSameRotation(current.Rotation, newt.Rotation))
        {
            var euler = EulerUtils.MatrixToEulerXZY(Matrix4x4.CreateFromQuaternion(newt.Rotation));
            act.AddPropertyChange(type.GetProperty(nameof(FLVER.Node.Rotation)), euler);
        }

        if (includeScale && Vector3.DistanceSquared(node.Scale, newt.Scale) > 1e-12f)
        {
            act.AddPropertyChange(type.GetProperty(nameof(FLVER.Node.Scale)), newt.Scale);
        }

        act.SetPostExecutionAction(_ => UpdateRenderModel());

        return act;
    }

    private ViewportAction GetDummyTransformAction(FLVER.Dummy dummy, Transform newt)
    {
        var current = GetLocalTransform();
        var type = dummy.GetType();

        var act = new PropChangeAction(this, dummy);

        act.AddPropertyChange(type.GetProperty(nameof(FLVER.Dummy.Position)), newt.Position);

        // The direction vectors are turned by the change in rotation, which keeps their lengths (and the angle
        // between them, which needn't be square)
        if (!IsSameRotation(current.Rotation, newt.Rotation))
        {
            // Undo the old rotation, then apply the new one
            var turn = Matrix4x4.CreateFromQuaternion(Quaternion.Inverse(current.Rotation)) *
                       Matrix4x4.CreateFromQuaternion(newt.Rotation);

            act.AddPropertyChange(type.GetProperty(nameof(FLVER.Dummy.Forward)),
                Vector3.TransformNormal(dummy.Forward, turn));
            act.AddPropertyChange(type.GetProperty(nameof(FLVER.Dummy.Upward)),
                Vector3.TransformNormal(dummy.Upward, turn));
        }

        act.SetPostExecutionAction(_ => UpdateRenderModel());

        return act;
    }

    private static bool IsSameRotation(Quaternion a, Quaternion b)
    {
        return MathF.Abs(Quaternion.Dot(Quaternion.Normalize(a), Quaternion.Normalize(b))) > 1 - 1e-7f;
    }
    #endregion

    #region Mesh Moves
    // The middle of the mesh in model space, worked out from its vertices. The data doesn't change during a drag, so
    // the pivot the drag starts about is the one the move is written about.
    private Vector3? _meshPivot;

    /// <summary>
    /// Call when the mesh's vertices have changed
    /// </summary>
    public void InvalidateMeshPivot()
    {
        _meshPivot = null;
    }

    private Vector3 GetMeshPivot()
    {
        if (_meshPivot == null && WrappedObject is FLVER2.Mesh mesh && Container is ModelContainer container &&
            container.Flver != null)
        {
            _meshPivot = MeshTransformAction.GetDrawnCenter(container.Flver, mesh,
                container.Project.Descriptor.ProjectType) ?? Vector3.Zero;
        }

        return _meshPivot ?? Vector3.Zero;
    }

    #endregion

    public override Transform GetLocalTransform()
    {
        // The root carries the viewport-only display rotation, which is never written to the model
        if (Type is ModelEntityType.ModelRoot && Container is ModelContainer root)
        {
            return new Transform(Vector3.Zero, root.DisplayRotation, Vector3.One);
        }

        Transform t = base.GetLocalTransform();

        // Prevent zero scale since it won't render
        if (t.Scale == Vector3.Zero)
        {
            t.Scale = new Vector3(0.1f);
        }

        return t;
    }

    /// <summary>
    /// The world matrix, including the root's display rotation. The root has no transform of its own as far as
    /// the base walk is concerned (so it isn't moved by the gizmo), so it is applied here.
    /// </summary>
    public override Matrix4x4 GetWorldMatrix()
    {
        Matrix4x4 m = GetModelMatrix();

        // A map EDGE file and the collision drawn for it are in the collision part's space, placed into the map
        // piece's by the MSB
        if (Type is ModelEntityType.EDGE && Container is ModelContainer edgeContainer)
        {
            m = edgeContainer.EdgeSpace * m;
        }

        if (Container is ModelContainer root && root.RootObject != null && this != root.RootObject)
        {
            m *= root.DisplayMatrix;
        }

        return m;
    }

    /// <summary>
    /// The matrix in the model's own space, without the display rotation.
    /// </summary>
    public Matrix4x4 GetModelMatrix()
    {
        return base.GetWorldMatrix();
    }

    internal override Entity DuplicateEntity(object clone)
    {
        return new ModelEntity(Owner, Container, clone);
    }

    public override Entity Clone()
    {
        var c = (ModelEntity)base.Clone();
        c.Type = Type;
        return c;
    }
}