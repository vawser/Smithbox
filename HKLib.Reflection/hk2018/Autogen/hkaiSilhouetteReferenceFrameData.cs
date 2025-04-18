// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiSilhouetteReferenceFrameData : HavokData<hkaiSilhouetteReferenceFrame> 
{
    public hkaiSilhouetteReferenceFrameData(HavokType type, hkaiSilhouetteReferenceFrame instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceAxis":
            case "referenceAxis":
            {
                if (instance.m_referenceAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_orthogonalAxis":
            case "orthogonalAxis":
            {
                if (instance.m_orthogonalAxis is not TGet castValue) return false;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_referenceAxis":
            case "referenceAxis":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_referenceAxis = castValue;
                return true;
            }
            case "m_orthogonalAxis":
            case "orthogonalAxis":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_orthogonalAxis = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
