// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiSilhouetteReferenceFrameData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiSilhouetteReferenceFrame>
{
    public hkaiSilhouetteReferenceFrameData(Havok.hk2019.hkaiSilhouetteReferenceFrame instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_referenceAxis":
            case "referenceAxis":
            {
                value = instance.m_referenceAxis;
                return true;
            }
            case "m_orthogonalAxis":
            case "orthogonalAxis":
            {
                value = instance.m_orthogonalAxis;
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
