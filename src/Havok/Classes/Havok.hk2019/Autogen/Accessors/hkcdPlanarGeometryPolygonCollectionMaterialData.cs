// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdPlanarGeometryPolygonCollectionMaterialData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdPlanarGeometryPolygonCollection.Material>
{
    public hkcdPlanarGeometryPolygonCollectionMaterialData(Havok.hk2019.hkcdPlanarGeometryPolygonCollection.Material instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_val":
            case "val":
            {
                value = instance.m_val;
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
            case "m_val":
            case "val":
            {
                if (value is not ulong castValue) return false;
                instance.m_val = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
