// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclTaperedCapsuleShapeData : HavokData<hclTaperedCapsuleShape> 
{
    public hclTaperedCapsuleShapeData(HavokType type, hclTaperedCapsuleShape instance) : base(type, instance) {}

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
            case "m_small":
            case "small":
            {
                if (instance.m_small is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_big":
            case "big":
            {
                if (instance.m_big is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_coneApex":
            case "coneApex":
            {
                if (instance.m_coneApex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_coneAxis":
            case "coneAxis":
            {
                if (instance.m_coneAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lVec":
            case "lVec":
            {
                if (instance.m_lVec is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dVec":
            case "dVec":
            {
                if (instance.m_dVec is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tanThetaVecNeg":
            case "tanThetaVecNeg":
            {
                if (instance.m_tanThetaVecNeg is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_smallRadius":
            case "smallRadius":
            {
                if (instance.m_smallRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bigRadius":
            case "bigRadius":
            {
                if (instance.m_bigRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_l":
            case "l":
            {
                if (instance.m_l is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_d":
            case "d":
            {
                if (instance.m_d is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cosTheta":
            case "cosTheta":
            {
                if (instance.m_cosTheta is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sinTheta":
            case "sinTheta":
            {
                if (instance.m_sinTheta is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tanTheta":
            case "tanTheta":
            {
                if (instance.m_tanTheta is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tanThetaSqr":
            case "tanThetaSqr":
            {
                if (instance.m_tanThetaSqr is not TGet castValue) return false;
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
            case "m_small":
            case "small":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_small = castValue;
                return true;
            }
            case "m_big":
            case "big":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_big = castValue;
                return true;
            }
            case "m_coneApex":
            case "coneApex":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_coneApex = castValue;
                return true;
            }
            case "m_coneAxis":
            case "coneAxis":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_coneAxis = castValue;
                return true;
            }
            case "m_lVec":
            case "lVec":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lVec = castValue;
                return true;
            }
            case "m_dVec":
            case "dVec":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_dVec = castValue;
                return true;
            }
            case "m_tanThetaVecNeg":
            case "tanThetaVecNeg":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_tanThetaVecNeg = castValue;
                return true;
            }
            case "m_smallRadius":
            case "smallRadius":
            {
                if (value is not float castValue) return false;
                instance.m_smallRadius = castValue;
                return true;
            }
            case "m_bigRadius":
            case "bigRadius":
            {
                if (value is not float castValue) return false;
                instance.m_bigRadius = castValue;
                return true;
            }
            case "m_l":
            case "l":
            {
                if (value is not float castValue) return false;
                instance.m_l = castValue;
                return true;
            }
            case "m_d":
            case "d":
            {
                if (value is not float castValue) return false;
                instance.m_d = castValue;
                return true;
            }
            case "m_cosTheta":
            case "cosTheta":
            {
                if (value is not float castValue) return false;
                instance.m_cosTheta = castValue;
                return true;
            }
            case "m_sinTheta":
            case "sinTheta":
            {
                if (value is not float castValue) return false;
                instance.m_sinTheta = castValue;
                return true;
            }
            case "m_tanTheta":
            case "tanTheta":
            {
                if (value is not float castValue) return false;
                instance.m_tanTheta = castValue;
                return true;
            }
            case "m_tanThetaSqr":
            case "tanThetaSqr":
            {
                if (value is not float castValue) return false;
                instance.m_tanThetaSqr = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
