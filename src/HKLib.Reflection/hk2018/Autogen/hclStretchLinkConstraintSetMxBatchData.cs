// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStretchLinkConstraintSetMxBatchData : HavokData<hclStretchLinkConstraintSetMx.Batch> 
{
    private static readonly System.Reflection.FieldInfo _restLengthsInfo = typeof(hclStretchLinkConstraintSetMx.Batch).GetField("m_restLengths")!;
    private static readonly System.Reflection.FieldInfo _stiffnessesInfo = typeof(hclStretchLinkConstraintSetMx.Batch).GetField("m_stiffnesses")!;
    private static readonly System.Reflection.FieldInfo _particlesAInfo = typeof(hclStretchLinkConstraintSetMx.Batch).GetField("m_particlesA")!;
    private static readonly System.Reflection.FieldInfo _particlesBInfo = typeof(hclStretchLinkConstraintSetMx.Batch).GetField("m_particlesB")!;
    public hclStretchLinkConstraintSetMxBatchData(HavokType type, hclStretchLinkConstraintSetMx.Batch instance) : base(type, instance) {}

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
            case "m_stiffnesses":
            case "stiffnesses":
            {
                if (instance.m_stiffnesses is not TGet castValue) return false;
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
            case "m_stiffnesses":
            case "stiffnesses":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _stiffnessesInfo.SetValue(instance, value);
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
