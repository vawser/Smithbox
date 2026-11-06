// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperBiplanarInnerTypesProjectedPlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.BiplanarInnerTypes.ProjectedPlane>
{
    public hkaiClipperBiplanarInnerTypesProjectedPlaneData(Havok.hk2019.hkaiClipper.BiplanarInnerTypes.ProjectedPlane instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_uv":
            case "uv":
            {
                value = instance.m_uv;
                return true;
            }
            case "m_w":
            case "w":
            {
                value = instance.m_w;
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
            case "m_uv":
            case "uv":
            {
                if (value is not Havok.hk2019.hkFpMath.FpIntVec2_3 castValue) return false;
                instance.m_uv = castValue;
                return true;
            }
            case "m_w":
            case "w":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_w = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
