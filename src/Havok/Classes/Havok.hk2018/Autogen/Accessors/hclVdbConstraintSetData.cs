// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVdbConstraintSetData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVdbConstraintSet>
{
    public hclVdbConstraintSetData(Havok.hk2018.hclVdbConstraintSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_simClothId":
            case "simClothId":
            {
                value = instance.m_simClothId;
                return true;
            }
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_simClothId":
            case "simClothId":
            {
                if (value is not ushort castValue) return false;
                instance.m_simClothId = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (value is not Havok.hk2018.hkHandle<uint> castValue) return false;
                instance.m_id = castValue;
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
            case "m_type":
            case "type":
            {
                if (value is null)
                {
                    instance.m_type = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
