// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiPathData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiPath>
{
    public hkaiPathData(Havok.hk2019.hkaiPath instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_points":
            case "points":
            {
                value = instance.m_points;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                value = instance.m_referenceFrame;
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
            case "m_points":
            case "points":
            {
                if (value is not List<Havok.hk2019.hkaiPath.PathPoint> castValue) return false;
                instance.m_points = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is Havok.hk2019.hkaiPath.ReferenceFrame castValue)
                {
                    instance.m_referenceFrame = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_referenceFrame = (Havok.hk2019.hkaiPath.ReferenceFrame)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
