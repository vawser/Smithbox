// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMeshBoneIndexMappingData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMeshBoneIndexMapping>
{
    public hkMeshBoneIndexMappingData(Havok.hk2019.hkMeshBoneIndexMapping instance) : base(instance) { }

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
