// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVolumeConstraintMxApplyBatchDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVolumeConstraintMx.ApplyBatchData>
{
    private static readonly System.Reflection.FieldInfo _frameVectorInfo = typeof(Havok.hk2018.hclVolumeConstraintMx.ApplyBatchData).GetField("m_frameVector")!;
    private static readonly System.Reflection.FieldInfo _particleIndexInfo = typeof(Havok.hk2018.hclVolumeConstraintMx.ApplyBatchData).GetField("m_particleIndex")!;
    private static readonly System.Reflection.FieldInfo _stiffnessInfo = typeof(Havok.hk2018.hclVolumeConstraintMx.ApplyBatchData).GetField("m_stiffness")!;
    public hclVolumeConstraintMxApplyBatchDataData(Havok.hk2018.hclVolumeConstraintMx.ApplyBatchData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_frameVector":
            case "frameVector":
            {
                value = instance.m_frameVector;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                value = instance.m_particleIndex;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                value = instance.m_stiffness;
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
