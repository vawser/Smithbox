// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperBiplanarInnerTypesImplicitPlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.BiplanarInnerTypes.ImplicitPlane>
{
    public hkaiClipperBiplanarInnerTypesImplicitPlaneData(Havok.hk2019.hkaiClipper.BiplanarInnerTypes.ImplicitPlane instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_xyz":
            case "xyz":
            {
                value = instance.m_xyz;
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
            case "m_xyz":
            case "xyz":
            {
                if (value is not Havok.hk2019.hkFpMath.FpIntVec3_3 castValue) return false;
                instance.m_xyz = castValue;
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
