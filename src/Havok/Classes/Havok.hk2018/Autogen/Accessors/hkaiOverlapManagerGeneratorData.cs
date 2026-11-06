// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiOverlapManagerGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiOverlapManager.Generator>
{
    public hkaiOverlapManagerGeneratorData(Havok.hk2018.hkaiOverlapManager.Generator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generator":
            case "generator":
            {
                value = instance.m_generator;
                return true;
            }
            case "m_rotatedAabb":
            case "rotatedAabb":
            {
                value = instance.m_rotatedAabb;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
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
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiSilhouetteGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
            }
            case "m_rotatedAabb":
            case "rotatedAabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_rotatedAabb = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Havok.hk2018.hkQTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
