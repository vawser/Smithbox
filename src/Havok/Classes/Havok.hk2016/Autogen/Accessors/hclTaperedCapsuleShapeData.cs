// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclTaperedCapsuleShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclTaperedCapsuleShape>
{
    public hclTaperedCapsuleShapeData(Havok.hk2016.hclTaperedCapsuleShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_small":
            case "small":
            {
                value = instance.m_small;
                return true;
            }
            case "m_big":
            case "big":
            {
                value = instance.m_big;
                return true;
            }
            case "m_coneApex":
            case "coneApex":
            {
                value = instance.m_coneApex;
                return true;
            }
            case "m_coneAxis":
            case "coneAxis":
            {
                value = instance.m_coneAxis;
                return true;
            }
            case "m_lVec":
            case "lVec":
            {
                value = instance.m_lVec;
                return true;
            }
            case "m_dVec":
            case "dVec":
            {
                value = instance.m_dVec;
                return true;
            }
            case "m_tanThetaVecNeg":
            case "tanThetaVecNeg":
            {
                value = instance.m_tanThetaVecNeg;
                return true;
            }
            case "m_smallRadius":
            case "smallRadius":
            {
                value = instance.m_smallRadius;
                return true;
            }
            case "m_bigRadius":
            case "bigRadius":
            {
                value = instance.m_bigRadius;
                return true;
            }
            case "m_l":
            case "l":
            {
                value = instance.m_l;
                return true;
            }
            case "m_d":
            case "d":
            {
                value = instance.m_d;
                return true;
            }
            case "m_cosTheta":
            case "cosTheta":
            {
                value = instance.m_cosTheta;
                return true;
            }
            case "m_sinTheta":
            case "sinTheta":
            {
                value = instance.m_sinTheta;
                return true;
            }
            case "m_tanTheta":
            case "tanTheta":
            {
                value = instance.m_tanTheta;
                return true;
            }
            case "m_tanThetaSqr":
            case "tanThetaSqr":
            {
                value = instance.m_tanThetaSqr;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
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
