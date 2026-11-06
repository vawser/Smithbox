// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclLocalRangeConstraintSetData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclLocalRangeConstraintSet>
{
    public hclLocalRangeConstraintSetData(Havok.hk2016.hclLocalRangeConstraintSet instance) : base(instance) { }

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
            case "m_localConstraints":
            case "localConstraints":
            {
                value = instance.m_localConstraints;
                return true;
            }
            case "m_referenceMeshBufferIdx":
            case "referenceMeshBufferIdx":
            {
                value = instance.m_referenceMeshBufferIdx;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                value = instance.m_stiffness;
                return true;
            }
            case "m_shapeType":
            case "shapeType":
            {
                value = instance.m_shapeType;
                return true;
            }
            case "m_applyNormalComponent":
            case "applyNormalComponent":
            {
                value = instance.m_applyNormalComponent;
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
            case "m_localConstraints":
            case "localConstraints":
            {
                if (value is not List<Havok.hk2016.hclLocalRangeConstraintSet.LocalConstraint> castValue) return false;
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
                if (value is Havok.hk2016.hclLocalRangeConstraintSet.ShapeType castValue)
                {
                    instance.m_shapeType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_shapeType = (Havok.hk2016.hclLocalRangeConstraintSet.ShapeType)uintValue;
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
