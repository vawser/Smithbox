// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiCollisionAvoidanceCharacterSensorSizeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiCollisionAvoidance.Character.SensorSize>
{
    public hkaiCollisionAvoidanceCharacterSensorSizeData(Havok.hk2019.hkaiCollisionAvoidance.Character.SensorSize instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_halfWidth":
            case "halfWidth":
            {
                value = instance.m_halfWidth;
                return true;
            }
            case "m_topExtent":
            case "topExtent":
            {
                value = instance.m_topExtent;
                return true;
            }
            case "m_bottomExtent":
            case "bottomExtent":
            {
                value = instance.m_bottomExtent;
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
            case "m_halfWidth":
            case "halfWidth":
            {
                if (value is not float castValue) return false;
                instance.m_halfWidth = castValue;
                return true;
            }
            case "m_topExtent":
            case "topExtent":
            {
                if (value is not float castValue) return false;
                instance.m_topExtent = castValue;
                return true;
            }
            case "m_bottomExtent":
            case "bottomExtent":
            {
                if (value is not float castValue) return false;
                instance.m_bottomExtent = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
