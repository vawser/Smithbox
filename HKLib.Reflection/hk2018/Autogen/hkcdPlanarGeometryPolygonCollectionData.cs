// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkcdPlanarGeometryPolygonCollectionData : HavokData<hkcdPlanarGeometryPolygonCollection> 
{
    private static readonly System.Reflection.FieldInfo _secondaryBitmapsInfo = typeof(hkcdPlanarGeometryPolygonCollection).GetField("m_secondaryBitmaps")!;
    private static readonly System.Reflection.FieldInfo _freeBlocksInfo = typeof(hkcdPlanarGeometryPolygonCollection).GetField("m_freeBlocks")!;
    public hkcdPlanarGeometryPolygonCollectionData(HavokType type, hkcdPlanarGeometryPolygonCollection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_storage":
            case "storage":
            {
                if (instance.m_storage is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primaryBitmap":
            case "primaryBitmap":
            {
                if (instance.m_primaryBitmap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_secondaryBitmaps":
            case "secondaryBitmaps":
            {
                if (instance.m_secondaryBitmaps is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_freeBlocks":
            case "freeBlocks":
            {
                if (instance.m_freeBlocks is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_storage":
            case "storage":
            {
                if (value is not List<int> castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            case "m_primaryBitmap":
            case "primaryBitmap":
            {
                if (value is not uint castValue) return false;
                instance.m_primaryBitmap = castValue;
                return true;
            }
            case "m_secondaryBitmaps":
            case "secondaryBitmaps":
            {
                if (value is not uint[] castValue || castValue.Length != 26) return false;
                try
                {
                    _secondaryBitmapsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_freeBlocks":
            case "freeBlocks":
            {
                if (value is not uint[,] castValue || castValue.GetLength(0) != 32 || castValue.GetLength(1) != 26) return false;
                try
                {
                    _freeBlocksInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
