// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkcdPlanarGeometryPolygonCollectionMaterialData : HavokData<hkcdPlanarGeometryPolygonCollection.Material> 
{
    public hkcdPlanarGeometryPolygonCollectionMaterialData(HavokType type, hkcdPlanarGeometryPolygonCollection.Material instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_val":
            case "val":
            {
                if (instance.m_val is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
