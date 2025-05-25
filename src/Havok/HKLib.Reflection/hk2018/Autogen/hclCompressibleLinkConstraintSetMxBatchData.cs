// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclCompressibleLinkConstraintSetMxBatchData : HavokData<hclCompressibleLinkConstraintSetMx.Batch> 
{
    private static readonly System.Reflection.FieldInfo _restLengthsInfo = typeof(hclCompressibleLinkConstraintSetMx.Batch).GetField("m_restLengths")!;
    private static readonly System.Reflection.FieldInfo _compressionLengthsInfo = typeof(hclCompressibleLinkConstraintSetMx.Batch).GetField("m_compressionLengths")!;
    private static readonly System.Reflection.FieldInfo _stiffnessesAInfo = typeof(hclCompressibleLinkConstraintSetMx.Batch).GetField("m_stiffnessesA")!;
    private static readonly System.Reflection.FieldInfo _stiffnessesBInfo = typeof(hclCompressibleLinkConstraintSetMx.Batch).GetField("m_stiffnessesB")!;
    private static readonly System.Reflection.FieldInfo _particlesAInfo = typeof(hclCompressibleLinkConstraintSetMx.Batch).GetField("m_particlesA")!;
    private static readonly System.Reflection.FieldInfo _particlesBInfo = typeof(hclCompressibleLinkConstraintSetMx.Batch).GetField("m_particlesB")!;
    public hclCompressibleLinkConstraintSetMxBatchData(HavokType type, hclCompressibleLinkConstraintSetMx.Batch instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_restLengths":
            case "restLengths":
            {
                if (instance.m_restLengths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_compressionLengths":
            case "compressionLengths":
            {
                if (instance.m_compressionLengths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffnessesA":
            case "stiffnessesA":
            {
                if (instance.m_stiffnessesA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffnessesB":
            case "stiffnessesB":
            {
                if (instance.m_stiffnessesB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particlesA":
            case "particlesA":
            {
                if (instance.m_particlesA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particlesB":
            case "particlesB":
            {
                if (instance.m_particlesB is not TGet castValue) return false;
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
            case "m_restLengths":
            case "restLengths":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _restLengthsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_compressionLengths":
            case "compressionLengths":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _compressionLengthsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_stiffnessesA":
            case "stiffnessesA":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _stiffnessesAInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_stiffnessesB":
            case "stiffnessesB":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _stiffnessesBInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_particlesA":
            case "particlesA":
            {
                if (value is not ushort[] castValue || castValue.Length != 16) return false;
                try
                {
                    _particlesAInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_particlesB":
            case "particlesB":
            {
                if (value is not ushort[] castValue || castValue.Length != 16) return false;
                try
                {
                    _particlesBInfo.SetValue(instance, value);
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
