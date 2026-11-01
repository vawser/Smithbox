using SoulsFormats;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Renderer;
using System.Numerics;

namespace StudioCore.Editors.Common;

/// <summary>
/// Entity used for the model displayed within the Time Act Editor.
/// </summary>
public class TimeActModelEntity : Entity
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

    public TimeActModelEntity(IUniverse owner) : base(owner)
    {
        Owner = owner;
    }

    public TimeActModelEntity(IUniverse owner, ObjectContainer container, object internalObject) : base(owner, container, internalObject)
    {
        Owner = owner;
        Container = (TimeActContainer)container;
        WrappedObject = internalObject;
    }

    public TimeActModelEntity(IUniverse owner, ObjectContainer container, object internalObject, ModelEntityType entityType) : base(owner, container, internalObject)
    {
        Owner = owner;
        Container = (TimeActContainer)container;
        WrappedObject = internalObject;
        Type = entityType;
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
        return new TimeActModelEntity(Owner, Container, clone);
    }

    public override Entity Clone()
    {
        var c = (TimeActModelEntity)base.Clone();
        c.Type = Type;
        return c;
    }
}
