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

    public ModelContainer ModelContainer
    {
        get => ModelContainer;
        set => ModelContainer = value;
    }

    public override bool HasTransform => Type is ModelEntityType.Dummy or ModelEntityType.Node;

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
        if (!CFG.Current.Viewport_Enable_Rendering)
        {
            return;
        }

        base.UpdateRenderModel();
    }

    public override Transform GetLocalTransform()
    {
        Transform t = base.GetLocalTransform();

        // Prevent zero scale since it won't render
        if (t.Scale == Vector3.Zero)
        {
            t.Scale = new Vector3(0.1f);
        }

        return t;
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