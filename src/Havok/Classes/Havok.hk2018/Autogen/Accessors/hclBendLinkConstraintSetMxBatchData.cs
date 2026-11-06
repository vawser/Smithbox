// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBendLinkConstraintSetMxBatchData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBendLinkConstraintSetMx.Batch>
{
    private static readonly System.Reflection.FieldInfo _bendMinLengthsInfo = typeof(Havok.hk2018.hclBendLinkConstraintSetMx.Batch).GetField("m_bendMinLengths")!;
    private static readonly System.Reflection.FieldInfo _stretchMaxLengthsInfo = typeof(Havok.hk2018.hclBendLinkConstraintSetMx.Batch).GetField("m_stretchMaxLengths")!;
    private static readonly System.Reflection.FieldInfo _stretchStiffnessesInfo = typeof(Havok.hk2018.hclBendLinkConstraintSetMx.Batch).GetField("m_stretchStiffnesses")!;
    private static readonly System.Reflection.FieldInfo _bendStiffnessesInfo = typeof(Havok.hk2018.hclBendLinkConstraintSetMx.Batch).GetField("m_bendStiffnesses")!;
    private static readonly System.Reflection.FieldInfo _invMassesAInfo = typeof(Havok.hk2018.hclBendLinkConstraintSetMx.Batch).GetField("m_invMassesA")!;
    private static readonly System.Reflection.FieldInfo _invMassesBInfo = typeof(Havok.hk2018.hclBendLinkConstraintSetMx.Batch).GetField("m_invMassesB")!;
    private static readonly System.Reflection.FieldInfo _particlesAInfo = typeof(Havok.hk2018.hclBendLinkConstraintSetMx.Batch).GetField("m_particlesA")!;
    private static readonly System.Reflection.FieldInfo _particlesBInfo = typeof(Havok.hk2018.hclBendLinkConstraintSetMx.Batch).GetField("m_particlesB")!;
    public hclBendLinkConstraintSetMxBatchData(Havok.hk2018.hclBendLinkConstraintSetMx.Batch instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bendMinLengths":
            case "bendMinLengths":
            {
                value = instance.m_bendMinLengths;
                return true;
            }
            case "m_stretchMaxLengths":
            case "stretchMaxLengths":
            {
                value = instance.m_stretchMaxLengths;
                return true;
            }
            case "m_stretchStiffnesses":
            case "stretchStiffnesses":
            {
                value = instance.m_stretchStiffnesses;
                return true;
            }
            case "m_bendStiffnesses":
            case "bendStiffnesses":
            {
                value = instance.m_bendStiffnesses;
                return true;
            }
            case "m_invMassesA":
            case "invMassesA":
            {
                value = instance.m_invMassesA;
                return true;
            }
            case "m_invMassesB":
            case "invMassesB":
            {
                value = instance.m_invMassesB;
                return true;
            }
            case "m_particlesA":
            case "particlesA":
            {
                value = instance.m_particlesA;
                return true;
            }
            case "m_particlesB":
            case "particlesB":
            {
                value = instance.m_particlesB;
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
