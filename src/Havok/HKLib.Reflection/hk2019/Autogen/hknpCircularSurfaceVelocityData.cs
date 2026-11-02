// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpCircularSurfaceVelocityData : HavokData<HKLib.hk2019.hknpCircularSurfaceVelocity> 
{
    public hknpCircularSurfaceVelocityData(HavokType type, HKLib.hk2019.hknpCircularSurfaceVelocity instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
