// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVolumeConstraintMxApplySingleDataData : HavokData<hclVolumeConstraintMx.ApplySingleData> 
{
    public hclVolumeConstraintMxApplySingleDataData(HavokType type, hclVolumeConstraintMx.ApplySingleData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_frameVector":
            case "frameVector":
            {
                if (instance.m_frameVector is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                if (instance.m_particleIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (instance.m_stiffness is not TGet castValue) return false;
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
            case "m_frameVector":
            case "frameVector":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_frameVector = castValue;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleIndex = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
