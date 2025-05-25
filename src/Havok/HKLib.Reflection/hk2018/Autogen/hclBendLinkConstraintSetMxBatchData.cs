// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBendLinkConstraintSetMxBatchData : HavokData<hclBendLinkConstraintSetMx.Batch> 
{
    private static readonly System.Reflection.FieldInfo _bendMinLengthsInfo = typeof(hclBendLinkConstraintSetMx.Batch).GetField("m_bendMinLengths")!;
    private static readonly System.Reflection.FieldInfo _stretchMaxLengthsInfo = typeof(hclBendLinkConstraintSetMx.Batch).GetField("m_stretchMaxLengths")!;
    private static readonly System.Reflection.FieldInfo _stretchStiffnessesInfo = typeof(hclBendLinkConstraintSetMx.Batch).GetField("m_stretchStiffnesses")!;
    private static readonly System.Reflection.FieldInfo _bendStiffnessesInfo = typeof(hclBendLinkConstraintSetMx.Batch).GetField("m_bendStiffnesses")!;
    private static readonly System.Reflection.FieldInfo _invMassesAInfo = typeof(hclBendLinkConstraintSetMx.Batch).GetField("m_invMassesA")!;
    private static readonly System.Reflection.FieldInfo _invMassesBInfo = typeof(hclBendLinkConstraintSetMx.Batch).GetField("m_invMassesB")!;
    private static readonly System.Reflection.FieldInfo _particlesAInfo = typeof(hclBendLinkConstraintSetMx.Batch).GetField("m_particlesA")!;
    private static readonly System.Reflection.FieldInfo _particlesBInfo = typeof(hclBendLinkConstraintSetMx.Batch).GetField("m_particlesB")!;
    public hclBendLinkConstraintSetMxBatchData(HavokType type, hclBendLinkConstraintSetMx.Batch instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bendMinLengths":
            case "bendMinLengths":
            {
                if (instance.m_bendMinLengths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stretchMaxLengths":
            case "stretchMaxLengths":
            {
                if (instance.m_stretchMaxLengths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stretchStiffnesses":
            case "stretchStiffnesses":
            {
                if (instance.m_stretchStiffnesses is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bendStiffnesses":
            case "bendStiffnesses":
            {
                if (instance.m_bendStiffnesses is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invMassesA":
            case "invMassesA":
            {
                if (instance.m_invMassesA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invMassesB":
            case "invMassesB":
            {
                if (instance.m_invMassesB is not TGet castValue) return false;
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
            case "m_bendMinLengths":
            case "bendMinLengths":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _bendMinLengthsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_stretchMaxLengths":
            case "stretchMaxLengths":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _stretchMaxLengthsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_stretchStiffnesses":
            case "stretchStiffnesses":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _stretchStiffnessesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_bendStiffnesses":
            case "bendStiffnesses":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _bendStiffnessesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_invMassesA":
            case "invMassesA":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _invMassesAInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_invMassesB":
            case "invMassesB":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _invMassesBInfo.SetValue(instance, value);
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
