// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiOverlapManagerGeneratorData : HavokData<hkaiOverlapManager.Generator> 
{
    public hkaiOverlapManagerGeneratorData(HavokType type, hkaiOverlapManager.Generator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generator":
            case "generator":
            {
                if (instance.m_generator is null)
                {
                    return true;
                }
                if (instance.m_generator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_rotatedAabb":
            case "rotatedAabb":
            {
                if (instance.m_rotatedAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
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
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is hkaiSilhouetteGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
            }
            case "m_rotatedAabb":
            case "rotatedAabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_rotatedAabb = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not hkQTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
