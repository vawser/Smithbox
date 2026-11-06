// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclVolumeConstraintMxFrameBatchDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclVolumeConstraintMx.FrameBatchData>
{
    private static readonly System.Reflection.FieldInfo _frameVectorInfo = typeof(Havok.hk2016.hclVolumeConstraintMx.FrameBatchData).GetField("m_frameVector")!;
    private static readonly System.Reflection.FieldInfo _particleIndexInfo = typeof(Havok.hk2016.hclVolumeConstraintMx.FrameBatchData).GetField("m_particleIndex")!;
    private static readonly System.Reflection.FieldInfo _weightInfo = typeof(Havok.hk2016.hclVolumeConstraintMx.FrameBatchData).GetField("m_weight")!;
    public hclVolumeConstraintMxFrameBatchDataData(Havok.hk2016.hclVolumeConstraintMx.FrameBatchData instance) : base(instance) { }

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
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
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
            case "m_weight":
            case "weight":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _weightInfo.SetValue(instance, value);
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
