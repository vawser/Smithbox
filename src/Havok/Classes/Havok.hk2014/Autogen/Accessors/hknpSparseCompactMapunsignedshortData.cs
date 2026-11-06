// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpSparseCompactMapunsignedshortData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpSparseCompactMapunsignedshort>
{
    public hknpSparseCompactMapunsignedshortData(Havok.hk2014.hknpSparseCompactMapunsignedshort instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_secondaryKeyMask":
            case "secondaryKeyMask":
            {
                value = instance.m_secondaryKeyMask;
                return true;
            }
            case "m_sencondaryKeyBits":
            case "sencondaryKeyBits":
            {
                value = instance.m_sencondaryKeyBits;
                return true;
            }
            case "m_primaryKeyToIndex":
            case "primaryKeyToIndex":
            {
                value = instance.m_primaryKeyToIndex;
                return true;
            }
            case "m_valueAndSecondaryKeys":
            case "valueAndSecondaryKeys":
            {
                value = instance.m_valueAndSecondaryKeys;
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
            case "m_secondaryKeyMask":
            case "secondaryKeyMask":
            {
                if (value is not uint castValue) return false;
                instance.m_secondaryKeyMask = castValue;
                return true;
            }
            case "m_sencondaryKeyBits":
            case "sencondaryKeyBits":
            {
                if (value is not uint castValue) return false;
                instance.m_sencondaryKeyBits = castValue;
                return true;
            }
            case "m_primaryKeyToIndex":
            case "primaryKeyToIndex":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_primaryKeyToIndex = castValue;
                return true;
            }
            case "m_valueAndSecondaryKeys":
            case "valueAndSecondaryKeys":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_valueAndSecondaryKeys = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
