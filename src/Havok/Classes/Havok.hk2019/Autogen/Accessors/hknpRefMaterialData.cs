// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpRefMaterialData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpRefMaterial>
{
    public hknpRefMaterialData(Havok.hk2019.hknpRefMaterial instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_material":
            case "material":
            {
                value = instance.m_material;
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
            case "m_material":
            case "material":
            {
                if (value is not Havok.hk2019.hknpMaterial castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
