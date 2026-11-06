// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBonePlanesConstraintSetData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBonePlanesConstraintSet>
{
    public hclBonePlanesConstraintSetData(Havok.hk2014.hclBonePlanesConstraintSet instance) : base(instance) { }

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
            case "m_bonePlanes":
            case "bonePlanes":
            {
                value = instance.m_bonePlanes;
                return true;
            }
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                value = instance.m_transformSetIndex;
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
            case "m_bonePlanes":
            case "bonePlanes":
            {
                if (value is not List<Havok.hk2014.hclBonePlanesConstraintSet.BonePlane> castValue) return false;
                instance.m_bonePlanes = castValue;
                return true;
            }
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_transformSetIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
