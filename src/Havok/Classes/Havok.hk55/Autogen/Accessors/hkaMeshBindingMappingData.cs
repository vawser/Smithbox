// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkaMeshBindingMappingData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkaMeshBinding.Mapping>
{
    public hkaMeshBindingMappingData(Havok.hk55.hkaMeshBinding.Mapping instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mapping":
            case "mapping":
            {
                value = instance.m_mapping;
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
            case "m_mapping":
            case "mapping":
            {
                if (value is not List<short> castValue) return false;
                instance.m_mapping = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
