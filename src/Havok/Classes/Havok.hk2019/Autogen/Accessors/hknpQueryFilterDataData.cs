// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpQueryFilterDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpQueryFilterData>
{
    public hknpQueryFilterDataData(Havok.hk2019.hknpQueryFilterData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_materialId":
            case "materialId":
            {
                value = instance.m_materialId;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
            case "m_materialId":
            case "materialId":
            {
                if (value is not Havok.hk2019.hknpMaterialId castValue) return false;
                instance.m_materialId = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
