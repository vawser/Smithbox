// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBendStiffnessConstraintSetMxBatchData : HavokData<hclBendStiffnessConstraintSetMx.Batch> 
{
    private static readonly System.Reflection.FieldInfo _weightsAInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_weightsA")!;
    private static readonly System.Reflection.FieldInfo _weightsBInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_weightsB")!;
    private static readonly System.Reflection.FieldInfo _weightsCInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_weightsC")!;
    private static readonly System.Reflection.FieldInfo _weightsDInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_weightsD")!;
    private static readonly System.Reflection.FieldInfo _bendStiffnessesInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_bendStiffnesses")!;
    private static readonly System.Reflection.FieldInfo _restCurvaturesInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_restCurvatures")!;
    private static readonly System.Reflection.FieldInfo _invMassesAInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_invMassesA")!;
    private static readonly System.Reflection.FieldInfo _invMassesBInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_invMassesB")!;
    private static readonly System.Reflection.FieldInfo _invMassesCInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_invMassesC")!;
    private static readonly System.Reflection.FieldInfo _invMassesDInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_invMassesD")!;
    private static readonly System.Reflection.FieldInfo _particlesAInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_particlesA")!;
    private static readonly System.Reflection.FieldInfo _particlesBInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_particlesB")!;
    private static readonly System.Reflection.FieldInfo _particlesCInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_particlesC")!;
    private static readonly System.Reflection.FieldInfo _particlesDInfo = typeof(hclBendStiffnessConstraintSetMx.Batch).GetField("m_particlesD")!;
    public hclBendStiffnessConstraintSetMxBatchData(HavokType type, hclBendStiffnessConstraintSetMx.Batch instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_weightsA":
            case "weightsA":
            {
                if (instance.m_weightsA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weightsB":
            case "weightsB":
            {
                if (instance.m_weightsB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weightsC":
            case "weightsC":
            {
                if (instance.m_weightsC is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weightsD":
            case "weightsD":
            {
                if (instance.m_weightsD is not TGet castValue) return false;
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
            case "m_restCurvatures":
            case "restCurvatures":
            {
                if (instance.m_restCurvatures is not TGet castValue) return false;
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
            case "m_invMassesC":
            case "invMassesC":
            {
                if (instance.m_invMassesC is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invMassesD":
            case "invMassesD":
            {
                if (instance.m_invMassesD is not TGet castValue) return false;
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
            case "m_particlesC":
            case "particlesC":
            {
                if (instance.m_particlesC is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particlesD":
            case "particlesD":
            {
                if (instance.m_particlesD is not TGet castValue) return false;
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
            case "m_weightsA":
            case "weightsA":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _weightsAInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_weightsB":
            case "weightsB":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _weightsBInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_weightsC":
            case "weightsC":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _weightsCInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_weightsD":
            case "weightsD":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _weightsDInfo.SetValue(instance, value);
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
            case "m_restCurvatures":
            case "restCurvatures":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _restCurvaturesInfo.SetValue(instance, value);
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
            case "m_invMassesC":
            case "invMassesC":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _invMassesCInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_invMassesD":
            case "invMassesD":
            {
                if (value is not float[] castValue || castValue.Length != 16) return false;
                try
                {
                    _invMassesDInfo.SetValue(instance, value);
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
            case "m_particlesC":
            case "particlesC":
            {
                if (value is not ushort[] castValue || castValue.Length != 16) return false;
                try
                {
                    _particlesCInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_particlesD":
            case "particlesD":
            {
                if (value is not ushort[] castValue || castValue.Length != 16) return false;
                try
                {
                    _particlesDInfo.SetValue(instance, value);
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
