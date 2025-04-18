// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVolumeConstraintMxApplyBatchDataData : HavokData<hclVolumeConstraintMx.ApplyBatchData> 
{
    private static readonly System.Reflection.FieldInfo _frameVectorInfo = typeof(hclVolumeConstraintMx.ApplyBatchData).GetField("m_frameVector")!;
    private static readonly System.Reflection.FieldInfo _particleIndexInfo = typeof(hclVolumeConstraintMx.ApplyBatchData).GetField("m_particleIndex")!;
    private static readonly System.Reflection.FieldInfo _stiffnessInfo = typeof(hclVolumeConstraintMx.ApplyBatchData).GetField("m_stiffness")!;
    public hclVolumeConstraintMxApplyBatchDataData(HavokType type, hclVolumeConstraintMx.ApplyBatchData instance) : base(type, instance) {}

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
                if (value is not Vector4[] castValue || castValue.Length != 16) return false;
                try
                {
                    _frameVectorInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                if (value is not ushort[] castValue || castValue.Length != 16) return false;
                try
                {
                    _particleIndexInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _stiffnessInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
