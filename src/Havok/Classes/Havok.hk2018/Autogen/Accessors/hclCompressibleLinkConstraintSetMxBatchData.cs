// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclCompressibleLinkConstraintSetMxBatchData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclCompressibleLinkConstraintSetMx.Batch>
{
    private static readonly System.Reflection.FieldInfo _restLengthsInfo = typeof(Havok.hk2018.hclCompressibleLinkConstraintSetMx.Batch).GetField("m_restLengths")!;
    private static readonly System.Reflection.FieldInfo _compressionLengthsInfo = typeof(Havok.hk2018.hclCompressibleLinkConstraintSetMx.Batch).GetField("m_compressionLengths")!;
    private static readonly System.Reflection.FieldInfo _stiffnessesAInfo = typeof(Havok.hk2018.hclCompressibleLinkConstraintSetMx.Batch).GetField("m_stiffnessesA")!;
    private static readonly System.Reflection.FieldInfo _stiffnessesBInfo = typeof(Havok.hk2018.hclCompressibleLinkConstraintSetMx.Batch).GetField("m_stiffnessesB")!;
    private static readonly System.Reflection.FieldInfo _particlesAInfo = typeof(Havok.hk2018.hclCompressibleLinkConstraintSetMx.Batch).GetField("m_particlesA")!;
    private static readonly System.Reflection.FieldInfo _particlesBInfo = typeof(Havok.hk2018.hclCompressibleLinkConstraintSetMx.Batch).GetField("m_particlesB")!;
    public hclCompressibleLinkConstraintSetMxBatchData(Havok.hk2018.hclCompressibleLinkConstraintSetMx.Batch instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_restLengths":
            case "restLengths":
            {
                value = instance.m_restLengths;
                return true;
            }
            case "m_compressionLengths":
            case "compressionLengths":
            {
                value = instance.m_compressionLengths;
                return true;
            }
            case "m_stiffnessesA":
            case "stiffnessesA":
            {
                value = instance.m_stiffnessesA;
                return true;
            }
            case "m_stiffnessesB":
            case "stiffnessesB":
            {
                value = instance.m_stiffnessesB;
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
