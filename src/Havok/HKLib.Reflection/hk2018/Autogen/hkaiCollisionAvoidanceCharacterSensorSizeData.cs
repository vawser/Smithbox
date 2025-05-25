// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiCollisionAvoidance;

namespace HKLib.Reflection.hk2018;

internal class hkaiCollisionAvoidanceCharacterSensorSizeData : HavokData<Character.SensorSize> 
{
    public hkaiCollisionAvoidanceCharacterSensorSizeData(HavokType type, Character.SensorSize instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_halfWidth":
            case "halfWidth":
            {
                if (instance.m_halfWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_topExtent":
            case "topExtent":
            {
                if (instance.m_topExtent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bottomExtent":
            case "bottomExtent":
            {
                if (instance.m_bottomExtent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
