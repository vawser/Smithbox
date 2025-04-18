// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiOverlappingTrianglesSettingsData : HavokData<hkaiOverlappingTriangles.Settings> 
{
    public hkaiOverlappingTrianglesSettingsData(HavokType type, hkaiOverlappingTriangles.Settings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_coplanarityTolerance":
            case "coplanarityTolerance":
            {
                if (instance.m_coplanarityTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_raycastLengthMultiplier":
            case "raycastLengthMultiplier":
            {
                if (instance.m_raycastLengthMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_walkableTriangleSettings":
            case "walkableTriangleSettings":
            {
                if (instance.m_walkableTriangleSettings is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_walkableTriangleSettings is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_coplanarityTolerance":
            case "coplanarityTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_coplanarityTolerance = castValue;
                return true;
            }
            case "m_raycastLengthMultiplier":
            case "raycastLengthMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_raycastLengthMultiplier = castValue;
                return true;
            }
            case "m_walkableTriangleSettings":
            case "walkableTriangleSettings":
            {
                if (value is hkaiOverlappingTriangles.WalkableTriangleSettings castValue)
                {
                    instance.m_walkableTriangleSettings = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_walkableTriangleSettings = (hkaiOverlappingTriangles.WalkableTriangleSettings)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
