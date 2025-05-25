// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStandardLinkConstraintSetMxData : HavokData<hclStandardLinkConstraintSetMx> 
{
    public hclStandardLinkConstraintSetMxData(HavokType type, hclStandardLinkConstraintSetMx instance) : base(type, instance) {}

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
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_constraintId":
            case "constraintId":
            {
                if (instance.m_constraintId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_batches":
            case "batches":
            {
                if (instance.m_batches is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_singles":
            case "singles":
            {
                if (instance.m_singles is not TGet castValue) return false;
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
                if (value is not hkHandle<uint> castValue) return false;
                instance.m_constraintId = castValue;
                return true;
            }
            case "m_batches":
            case "batches":
            {
                if (value is not List<hclStandardLinkConstraintSetMx.Batch> castValue) return false;
                instance.m_batches = castValue;
                return true;
            }
            case "m_singles":
            case "singles":
            {
                if (value is not List<hclStandardLinkConstraintSetMx.Single> castValue) return false;
                instance.m_singles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
