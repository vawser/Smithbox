// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpPointToPathConstraintDataData : HavokData<hkpPointToPathConstraintData> 
{
    private static readonly System.Reflection.FieldInfo _transform_OS_KSInfo = typeof(hkpPointToPathConstraintData).GetField("m_transform_OS_KS")!;
    public hkpPointToPathConstraintDataData(HavokType type, hkpPointToPathConstraintData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                if (instance.m_atoms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_path":
            case "path":
            {
                if (instance.m_path is null)
                {
                    return true;
                }
                if (instance.m_path is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                if (instance.m_maxFrictionForce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularConstrainedDOF":
            case "angularConstrainedDOF":
            {
                if (instance.m_angularConstrainedDOF is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_angularConstrainedDOF is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_transform_OS_KS":
            case "transform_OS_KS":
            {
                if (instance.m_transform_OS_KS is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                if (value is not hkpBridgeAtoms castValue) return false;
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
                if (value is hkpParametricCurve castValue)
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
                if (value is hkpPointToPathConstraintData.OrientationConstraintType castValue)
                {
                    instance.m_angularConstrainedDOF = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_angularConstrainedDOF = (hkpPointToPathConstraintData.OrientationConstraintType)sbyteValue;
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
