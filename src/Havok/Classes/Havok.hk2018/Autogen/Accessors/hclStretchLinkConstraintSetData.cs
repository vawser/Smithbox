// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStretchLinkConstraintSetData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStretchLinkConstraintSet>
{
    public hclStretchLinkConstraintSetData(Havok.hk2018.hclStretchLinkConstraintSet instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_constraintId":
            case "constraintId":
            {
                value = instance.m_constraintId;
                return true;
            }
            case "m_links":
            case "links":
            {
                value = instance.m_links;
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
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_constraintId":
            case "constraintId":
            {
                if (value is not Havok.hk2018.hkHandle<uint> castValue) return false;
                instance.m_constraintId = castValue;
                return true;
            }
            case "m_links":
            case "links":
            {
                if (value is not List<Havok.hk2018.hclStretchLinkConstraintSet.Link> castValue) return false;
                instance.m_links = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
