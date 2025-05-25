// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpSpringActionData : HavokData<hknpSpringAction> 
{
    public hknpSpringActionData(HavokType type, hknpSpringAction instance) : base(type, instance) {}

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
            case "m_bodyIdA":
            case "bodyIdA":
            {
                if (instance.m_bodyIdA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (instance.m_bodyIdB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lastForce":
            case "lastForce":
            {
                if (instance.m_lastForce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_positionAinA":
            case "positionAinA":
            {
                if (instance.m_positionAinA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_positionBinB":
            case "positionBinB":
            {
                if (instance.m_positionBinB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_restLength":
            case "restLength":
            {
                if (instance.m_restLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_strength":
            case "strength":
            {
                if (instance.m_strength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (instance.m_damping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onCompression":
            case "onCompression":
            {
                if (instance.m_onCompression is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onExtension":
            case "onExtension":
            {
                if (instance.m_onExtension is not TGet castValue) return false;
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
            case "m_bodyIdA":
            case "bodyIdA":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyIdA = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyIdB = castValue;
                return true;
            }
            case "m_lastForce":
            case "lastForce":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lastForce = castValue;
                return true;
            }
            case "m_positionAinA":
            case "positionAinA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_positionAinA = castValue;
                return true;
            }
            case "m_positionBinB":
            case "positionBinB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_positionBinB = castValue;
                return true;
            }
            case "m_restLength":
            case "restLength":
            {
                if (value is not float castValue) return false;
                instance.m_restLength = castValue;
                return true;
            }
            case "m_strength":
            case "strength":
            {
                if (value is not float castValue) return false;
                instance.m_strength = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (value is not float castValue) return false;
                instance.m_damping = castValue;
                return true;
            }
            case "m_onCompression":
            case "onCompression":
            {
                if (value is not bool castValue) return false;
                instance.m_onCompression = castValue;
                return true;
            }
            case "m_onExtension":
            case "onExtension":
            {
                if (value is not bool castValue) return false;
                instance.m_onExtension = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
