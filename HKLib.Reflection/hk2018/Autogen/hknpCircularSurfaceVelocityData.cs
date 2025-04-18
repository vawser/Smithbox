// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpCircularSurfaceVelocityData : HavokData<hknpCircularSurfaceVelocity> 
{
    public hknpCircularSurfaceVelocityData(HavokType type, hknpCircularSurfaceVelocity instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocityIsLocalSpace":
            case "velocityIsLocalSpace":
            {
                if (instance.m_velocityIsLocalSpace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pivot":
            case "pivot":
            {
                if (instance.m_pivot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (instance.m_angularVelocity is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_velocityIsLocalSpace":
            case "velocityIsLocalSpace":
            {
                if (value is not bool castValue) return false;
                instance.m_velocityIsLocalSpace = castValue;
                return true;
            }
            case "m_pivot":
            case "pivot":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pivot = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
