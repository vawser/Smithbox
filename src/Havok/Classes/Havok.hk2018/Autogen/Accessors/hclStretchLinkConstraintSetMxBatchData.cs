// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStretchLinkConstraintSetMxBatchData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStretchLinkConstraintSetMx.Batch>
{
    private static readonly System.Reflection.FieldInfo _restLengthsInfo = typeof(Havok.hk2018.hclStretchLinkConstraintSetMx.Batch).GetField("m_restLengths")!;
    private static readonly System.Reflection.FieldInfo _stiffnessesInfo = typeof(Havok.hk2018.hclStretchLinkConstraintSetMx.Batch).GetField("m_stiffnesses")!;
    private static readonly System.Reflection.FieldInfo _particlesAInfo = typeof(Havok.hk2018.hclStretchLinkConstraintSetMx.Batch).GetField("m_particlesA")!;
    private static readonly System.Reflection.FieldInfo _particlesBInfo = typeof(Havok.hk2018.hclStretchLinkConstraintSetMx.Batch).GetField("m_particlesB")!;
    public hclStretchLinkConstraintSetMxBatchData(Havok.hk2018.hclStretchLinkConstraintSetMx.Batch instance) : base(instance) { }

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
            case "m_stiffnesses":
            case "stiffnesses":
            {
                value = instance.m_stiffnesses;
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
