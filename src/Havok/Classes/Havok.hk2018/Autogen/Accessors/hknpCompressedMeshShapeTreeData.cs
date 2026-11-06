// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpCompressedMeshShapeTreeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpCompressedMeshShapeTree>
{
    public hknpCompressedMeshShapeTreeData(Havok.hk2018.hknpCompressedMeshShapeTree instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                value = instance.m_domain;
                return true;
            }
            case "m_numPrimitiveKeys":
            case "numPrimitiveKeys":
            {
                value = instance.m_numPrimitiveKeys;
                return true;
            }
            case "m_bitsPerKey":
            case "bitsPerKey":
            {
                value = instance.m_bitsPerKey;
                return true;
            }
            case "m_maxKeyValue":
            case "maxKeyValue":
            {
                value = instance.m_maxKeyValue;
                return true;
            }
            case "m_primitiveStoresIsFlatConvex":
            case "primitiveStoresIsFlatConvex":
            {
                value = instance.m_primitiveStoresIsFlatConvex;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                value = instance.m_sections;
                return true;
            }
            case "m_primitives":
            case "primitives":
            {
                value = instance.m_primitives;
                return true;
            }
            case "m_sharedVerticesIndex":
            case "sharedVerticesIndex":
            {
                value = instance.m_sharedVerticesIndex;
                return true;
            }
            case "m_packedVertices":
            case "packedVertices":
            {
                value = instance.m_packedVertices;
                return true;
            }
            case "m_sharedVertices":
            case "sharedVertices":
            {
                value = instance.m_sharedVertices;
                return true;
            }
            case "m_primitiveDataRuns":
            case "primitiveDataRuns":
            {
                value = instance.m_primitiveDataRuns;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<Havok.hk2018.hkcdCompressedAabbCodecs.Aabb5BytesCodec> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_domain = castValue;
                return true;
            }
            case "m_numPrimitiveKeys":
            case "numPrimitiveKeys":
            {
                if (value is not int castValue) return false;
                instance.m_numPrimitiveKeys = castValue;
                return true;
            }
            case "m_bitsPerKey":
            case "bitsPerKey":
            {
                if (value is not int castValue) return false;
                instance.m_bitsPerKey = castValue;
                return true;
            }
            case "m_maxKeyValue":
            case "maxKeyValue":
            {
                if (value is not uint castValue) return false;
                instance.m_maxKeyValue = castValue;
                return true;
            }
            case "m_primitiveStoresIsFlatConvex":
            case "primitiveStoresIsFlatConvex":
            {
                if (value is not byte castValue) return false;
                instance.m_primitiveStoresIsFlatConvex = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<Havok.hk2018.hkcdStaticMeshTree.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_primitives":
            case "primitives":
            {
                if (value is not List<Havok.hk2018.hkcdStaticMeshTree.Primitive> castValue) return false;
                instance.m_primitives = castValue;
                return true;
            }
            case "m_sharedVerticesIndex":
            case "sharedVerticesIndex":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_sharedVerticesIndex = castValue;
                return true;
            }
            case "m_packedVertices":
            case "packedVertices":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_packedVertices = castValue;
                return true;
            }
            case "m_sharedVertices":
            case "sharedVertices":
            {
                if (value is not List<ulong> castValue) return false;
                instance.m_sharedVertices = castValue;
                return true;
            }
            case "m_primitiveDataRuns":
            case "primitiveDataRuns":
            {
                if (value is not List<Havok.hk2018.hkcdDefaultStaticMeshTree.PrimitiveDataRun> castValue) return false;
                instance.m_primitiveDataRuns = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
