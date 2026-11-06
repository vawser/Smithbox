// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxBlobMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxBlobMeshShape>
{
    public hkxBlobMeshShapeData(Havok.hk2019.hkxBlobMeshShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_blob":
            case "blob":
            {
                value = instance.m_blob;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_blob":
            case "blob":
            {
                if (value is not Havok.hk2019.hkxBlob castValue) return false;
                instance.m_blob = castValue;
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
            default:
            return false;
        }
    }

}
