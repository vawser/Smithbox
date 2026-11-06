// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpCdBodyData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpCdBody>
{
    public hkpCdBodyData(Havok.hk2010.hkpCdBody instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_shapeKey":
            case "shapeKey":
            {
                value = instance.m_shapeKey;
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
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is Havok.hk2010.hkpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeKey":
            case "shapeKey":
            {
                if (value is not uint castValue) return false;
                instance.m_shapeKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
