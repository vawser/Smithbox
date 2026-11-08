using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
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

    public override bool HasTransform => Type is ModelEntityType.Dummy or ModelEntityType.Node;

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
    }

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