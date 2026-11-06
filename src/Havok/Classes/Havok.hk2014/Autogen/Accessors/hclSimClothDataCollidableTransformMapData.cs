// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSimClothDataCollidableTransformMapData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSimClothData.CollidableTransformMap>
{
    public hclSimClothDataCollidableTransformMapData(Havok.hk2014.hclSimClothData.CollidableTransformMap instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                value = instance.m_transformSetIndex;
                return true;
            }
            case "m_transformIndices":
            case "transformIndices":
            {
                value = instance.m_transformIndices;
                return true;
            }
            case "m_offsets":
            case "offsets":
            {
                value = instance.m_offsets;
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
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                if (value is not int castValue) return false;
                instance.m_transformSetIndex = castValue;
                return true;
            }
            case "m_transformIndices":
            case "transformIndices":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_transformIndices = castValue;
                return true;
            }
            case "m_offsets":
            case "offsets":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_offsets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
