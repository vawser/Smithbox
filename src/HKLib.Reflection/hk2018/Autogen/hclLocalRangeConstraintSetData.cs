// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclLocalRangeConstraintSetData : HavokData<hclLocalRangeConstraintSet> 
{
    public hclLocalRangeConstraintSetData(HavokType type, hclLocalRangeConstraintSet instance) : base(type, instance) {}

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
            case "m_localConstraints":
            case "localConstraints":
            {
                if (instance.m_localConstraints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceMeshBufferIdx":
            case "referenceMeshBufferIdx":
            {
                if (instance.m_referenceMeshBufferIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (instance.m_stiffness is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shapeType":
            case "shapeType":
            {
                if (instance.m_shapeType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_shapeType is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_applyNormalComponent":
            case "applyNormalComponent":
            {
                if (instance.m_applyNormalComponent is not TGet castValue) return false;
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
            case "m_localConstraints":
            case "localConstraints":
            {
                if (value is not List<hclLocalRangeConstraintSet.LocalConstraint> castValue) return false;
                instance.m_localConstraints = castValue;
                return true;
            }
            case "m_referenceMeshBufferIdx":
            case "referenceMeshBufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_referenceMeshBufferIdx = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            case "m_shapeType":
            case "shapeType":
            {
                if (value is hclLocalRangeConstraintSet.ShapeType castValue)
                {
                    instance.m_shapeType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_shapeType = (hclLocalRangeConstraintSet.ShapeType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_applyNormalComponent":
            case "applyNormalComponent":
            {
                if (value is not bool castValue) return false;
                instance.m_applyNormalComponent = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
