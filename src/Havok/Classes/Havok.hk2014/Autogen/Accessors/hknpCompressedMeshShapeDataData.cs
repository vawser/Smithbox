// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpCompressedMeshShapeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpCompressedMeshShapeData>
{
    public hknpCompressedMeshShapeDataData(Havok.hk2014.hknpCompressedMeshShapeData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_meshTree":
            case "meshTree":
            {
                value = instance.m_meshTree;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                value = instance.m_simdTree;
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
            case "m_meshTree":
            case "meshTree":
            {
                if (value is not Havok.hk2014.hknpCompressedMeshShapeTree castValue) return false;
                instance.m_meshTree = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (value is not Havok.hk2014.hkcdSimdTree castValue) return false;
                instance.m_simdTree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
