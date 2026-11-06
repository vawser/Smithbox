// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclCompressibleLinkConstraintSetMxData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclCompressibleLinkConstraintSetMx>
{
    public hclCompressibleLinkConstraintSetMxData(Havok.hk2018.hclCompressibleLinkConstraintSetMx instance) : base(instance) { }

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
            case "m_batches":
            case "batches":
            {
                value = instance.m_batches;
                return true;
            }
            case "m_singles":
            case "singles":
            {
                value = instance.m_singles;
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
            case "m_batches":
            case "batches":
            {
                if (value is not List<Havok.hk2018.hclCompressibleLinkConstraintSetMx.Batch> castValue) return false;
                instance.m_batches = castValue;
                return true;
            }
            case "m_singles":
            case "singles":
            {
                if (value is not List<Havok.hk2018.hclCompressibleLinkConstraintSetMx.Single> castValue) return false;
                instance.m_singles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
