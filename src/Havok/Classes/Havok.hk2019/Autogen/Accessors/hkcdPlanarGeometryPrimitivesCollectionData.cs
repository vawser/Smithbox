// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdPlanarGeometryPrimitivesCollectionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdPlanarGeometryPrimitives.Collection>
{
    private static readonly System.Reflection.FieldInfo _secondaryBitmapsInfo = typeof(Havok.hk2019.hkcdPlanarGeometryPrimitives.Collection).GetField("m_secondaryBitmaps")!;
    private static readonly System.Reflection.FieldInfo _freeBlocksInfo = typeof(Havok.hk2019.hkcdPlanarGeometryPrimitives.Collection).GetField("m_freeBlocks")!;
    public hkcdPlanarGeometryPrimitivesCollectionData(Havok.hk2019.hkcdPlanarGeometryPrimitives.Collection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_storage":
            case "storage":
            {
                value = instance.m_storage;
                return true;
            }
            case "m_primaryBitmap":
            case "primaryBitmap":
            {
                value = instance.m_primaryBitmap;
                return true;
            }
            case "m_secondaryBitmaps":
            case "secondaryBitmaps":
            {
                value = instance.m_secondaryBitmaps;
                return true;
            }
            case "m_freeBlocks":
            case "freeBlocks":
            {
                value = instance.m_freeBlocks;
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
