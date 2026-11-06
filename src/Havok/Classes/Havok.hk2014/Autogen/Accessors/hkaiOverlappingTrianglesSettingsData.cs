// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiOverlappingTrianglesSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiOverlappingTriangles.Settings>
{
    public hkaiOverlappingTrianglesSettingsData(Havok.hk2014.hkaiOverlappingTriangles.Settings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_coplanarityTolerance":
            case "coplanarityTolerance":
            {
                value = instance.m_coplanarityTolerance;
                return true;
            }
            case "m_raycastLengthMultiplier":
            case "raycastLengthMultiplier":
            {
                value = instance.m_raycastLengthMultiplier;
                return true;
            }
            case "m_walkableTriangleSettings":
            case "walkableTriangleSettings":
            {
                value = instance.m_walkableTriangleSettings;
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
                if (value is Havok.hk2014.hkaiOverlappingTriangles.WalkableTriangleSettings castValue)
                {
                    instance.m_walkableTriangleSettings = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_walkableTriangleSettings = (Havok.hk2014.hkaiOverlappingTriangles.WalkableTriangleSettings)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
