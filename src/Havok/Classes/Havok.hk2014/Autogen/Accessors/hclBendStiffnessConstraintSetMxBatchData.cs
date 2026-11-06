// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBendStiffnessConstraintSetMxBatchData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch>
{
    private static readonly System.Reflection.FieldInfo _weightsAInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_weightsA")!;
    private static readonly System.Reflection.FieldInfo _weightsBInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_weightsB")!;
    private static readonly System.Reflection.FieldInfo _weightsCInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_weightsC")!;
    private static readonly System.Reflection.FieldInfo _weightsDInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_weightsD")!;
    private static readonly System.Reflection.FieldInfo _bendStiffnessesInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_bendStiffnesses")!;
    private static readonly System.Reflection.FieldInfo _restCurvaturesInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_restCurvatures")!;
    private static readonly System.Reflection.FieldInfo _invMassesAInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_invMassesA")!;
    private static readonly System.Reflection.FieldInfo _invMassesBInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_invMassesB")!;
    private static readonly System.Reflection.FieldInfo _invMassesCInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_invMassesC")!;
    private static readonly System.Reflection.FieldInfo _invMassesDInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_invMassesD")!;
    private static readonly System.Reflection.FieldInfo _particlesAInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_particlesA")!;
    private static readonly System.Reflection.FieldInfo _particlesBInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_particlesB")!;
    private static readonly System.Reflection.FieldInfo _particlesCInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_particlesC")!;
    private static readonly System.Reflection.FieldInfo _particlesDInfo = typeof(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch).GetField("m_particlesD")!;
    public hclBendStiffnessConstraintSetMxBatchData(Havok.hk2014.hclBendStiffnessConstraintSetMx.Batch instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_weightsA":
            case "weightsA":
            {
                value = instance.m_weightsA;
                return true;
            }
            case "m_weightsB":
            case "weightsB":
            {
                value = instance.m_weightsB;
                return true;
            }
            case "m_weightsC":
            case "weightsC":
            {
                value = instance.m_weightsC;
                return true;
            }
            case "m_weightsD":
            case "weightsD":
            {
                value = instance.m_weightsD;
                return true;
            }
            case "m_bendStiffnesses":
            case "bendStiffnesses":
            {
                value = instance.m_bendStiffnesses;
                return true;
            }
            case "m_restCurvatures":
            case "restCurvatures":
            {
                value = instance.m_restCurvatures;
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
            case "m_invMassesC":
            case "invMassesC":
            {
                value = instance.m_invMassesC;
                return true;
            }
            case "m_invMassesD":
            case "invMassesD":
            {
                value = instance.m_invMassesD;
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
            case "m_particlesC":
            case "particlesC":
            {
                value = instance.m_particlesC;
                return true;
            }
            case "m_particlesD":
            case "particlesD":
            {
                value = instance.m_particlesD;
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
