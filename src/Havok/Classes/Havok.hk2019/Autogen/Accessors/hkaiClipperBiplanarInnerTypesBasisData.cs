// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiClipperBiplanarInnerTypesBasisData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiClipper.BiplanarInnerTypes.Basis>
{
    public hkaiClipperBiplanarInnerTypesBasisData(Havok.hk2019.hkaiClipper.BiplanarInnerTypes.Basis instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_facePlane":
            case "facePlane":
            {
                value = instance.m_facePlane;
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
            case "m_facePlane":
            case "facePlane":
            {
                if (value is not Havok.hk2019.hkaiClipper.BiplanarInnerTypes.FacePlane castValue) return false;
                instance.m_facePlane = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
