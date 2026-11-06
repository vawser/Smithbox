// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpMultiRayShapeRayData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpMultiRayShapeRay>
{
    public hkpMultiRayShapeRayData(Havok.hk2014.hkpMultiRayShapeRay instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_start":
            case "start":
            {
                value = instance.m_start;
                return true;
            }
            case "m_end":
            case "end":
            {
                value = instance.m_end;
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
            case "m_start":
            case "start":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_start = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_end = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
