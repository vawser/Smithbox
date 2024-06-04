// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkcdCompressedAabbCodecs;
using HKLib.hk2018.hkcdStaticMeshTree;

namespace HKLib.Reflection.hk2018;

internal class hkcdStaticMeshTreeBaseData : HavokData<Base> 
{
    public hkcdStaticMeshTreeBaseData(HavokType type, Base instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                if (instance.m_nodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (instance.m_domain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numPrimitiveKeys":
            case "numPrimitiveKeys":
            {
                if (instance.m_numPrimitiveKeys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bitsPerKey":
            case "bitsPerKey":
            {
                if (instance.m_bitsPerKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxKeyValue":
            case "maxKeyValue":
            {
                if (instance.m_maxKeyValue is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primitiveStoresIsFlatConvex":
            case "primitiveStoresIsFlatConvex":
            {
                if (instance.m_primitiveStoresIsFlatConvex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (instance.m_sections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primitives":
            case "primitives":
            {
                if (instance.m_primitives is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sharedVerticesIndex":
            case "sharedVerticesIndex":
            {
                if (instance.m_sharedVerticesIndex is not TGet castValue) return false;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<Aabb5BytesCodec> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not hkAabb castValue) return false;
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
                if (value is not List<Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_primitives":
            case "primitives":
            {
                if (value is not List<Primitive> castValue) return false;
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
            default:
            return false;
        }
    }

}
