// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSingleShapeContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSingleShapeContainer>
{
    public hkpSingleShapeContainerData(Havok.hk2014.hkpSingleShapeContainer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_childShape":
            case "childShape":
            {
                value = instance.m_childShape;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_childShape":
            case "childShape":
            {
                if (value is null)
                {
                    instance.m_childShape = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpShape castValue)
                {
                    instance.m_childShape = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
