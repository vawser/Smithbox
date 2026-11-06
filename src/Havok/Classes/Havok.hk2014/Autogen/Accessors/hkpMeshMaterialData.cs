// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpMeshMaterialData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpMeshMaterial>
{
    public hkpMeshMaterialData(Havok.hk2014.hkpMeshMaterial instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_filterInfo":
            case "filterInfo":
            {
                value = instance.m_filterInfo;
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
            case "m_filterInfo":
            case "filterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_filterInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
