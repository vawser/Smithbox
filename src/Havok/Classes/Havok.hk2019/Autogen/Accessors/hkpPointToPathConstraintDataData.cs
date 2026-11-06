// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpPointToPathConstraintDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpPointToPathConstraintData>
{
    private static readonly System.Reflection.FieldInfo _transform_OS_KSInfo = typeof(Havok.hk2019.hkpPointToPathConstraintData).GetField("m_transform_OS_KS")!;
    public hkpPointToPathConstraintDataData(Havok.hk2019.hkpPointToPathConstraintData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                value = instance.m_numNonDirectlySolvedAtoms;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                value = instance.m_atoms;
                return true;
            }
            case "m_path":
            case "path":
            {
                value = instance.m_path;
                return true;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                value = instance.m_maxFrictionForce;
                return true;
            }
            case "m_angularConstrainedDOF":
            case "angularConstrainedDOF":
            {
                value = instance.m_angularConstrainedDOF;
                return true;
            }
            case "m_transform_OS_KS":
            case "transform_OS_KS":
            {
                value = instance.m_transform_OS_KS;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                if (value is not byte castValue) return false;
                instance.m_numNonDirectlySolvedAtoms = castValue;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                if (value is not Havok.hk2019.hkpBridgeAtoms castValue) return false;
                instance.m_atoms = castValue;
                return true;
            }
            case "m_path":
            case "path":
            {
                if (value is null)
                {
                    instance.m_path = default;
                    return true;
                }
                if (value is Havok.hk2019.hkpParametricCurve castValue)
                {
                    instance.m_path = castValue;
                    return true;
                }
                return false;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxFrictionForce = castValue;
                return true;
            }
            case "m_angularConstrainedDOF":
            case "angularConstrainedDOF":
            {
                if (value is Havok.hk2019.hkpPointToPathConstraintData.OrientationConstraintType castValue)
                {
                    instance.m_angularConstrainedDOF = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_angularConstrainedDOF = (Havok.hk2019.hkpPointToPathConstraintData.OrientationConstraintType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_transform_OS_KS":
            case "transform_OS_KS":
            {
                if (value is not Matrix4x4[] castValue || castValue.Length != 2) return false;
                try
                {
                    _transform_OS_KSInfo.SetValue(instance, value);
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
