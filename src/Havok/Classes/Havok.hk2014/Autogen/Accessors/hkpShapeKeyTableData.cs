// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpShapeKeyTableData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpShapeKeyTable>
{
    public hkpShapeKeyTableData(Havok.hk2014.hkpShapeKeyTable instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_lists":
            case "lists":
            {
                value = instance.m_lists;
                return true;
            }
            case "m_occupancyBitField":
            case "occupancyBitField":
            {
                value = instance.m_occupancyBitField;
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
            case "m_lists":
            case "lists":
            {
                if (value is null)
                {
                    instance.m_lists = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpShapeKeyTableBlock castValue)
                {
                    instance.m_lists = castValue;
                    return true;
                }
                return false;
            }
            case "m_occupancyBitField":
            case "occupancyBitField":
            {
                if (value is not uint castValue) return false;
                instance.m_occupancyBitField = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
