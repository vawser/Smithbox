// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclLocalRangeConstraintSetData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclLocalRangeConstraintSet>
{
    public hclLocalRangeConstraintSetData(Havok.hk2014.hclLocalRangeConstraintSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
                if (value is not List<Havok.hk2014.hclLocalRangeConstraintSet.LocalConstraint> castValue) return false;
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
                if (value is Havok.hk2014.hclLocalRangeConstraintSet.ShapeType castValue)
                {
                    instance.m_shapeType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_shapeType = (Havok.hk2014.hclLocalRangeConstraintSet.ShapeType)uintValue;
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
