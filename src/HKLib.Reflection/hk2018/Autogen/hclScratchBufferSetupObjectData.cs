// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclScratchBufferSetupObjectData : HavokData<hclScratchBufferSetupObject> 
{
    public hclScratchBufferSetupObjectData(HavokType type, hclScratchBufferSetupObject instance) : base(type, instance) {}

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
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_setupMesh":
            case "setupMesh":
            {
                if (instance.m_setupMesh is null)
                {
                    return true;
                }
                if (instance.m_setupMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_storeNormals":
            case "storeNormals":
            {
                if (instance.m_storeNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_storeTangentsAndBiTangents":
            case "storeTangentsAndBiTangents":
            {
                if (instance.m_storeTangentsAndBiTangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_storeTriangles":
            case "storeTriangles":
            {
                if (instance.m_storeTriangles is not TGet castValue) return false;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_setupMesh":
            case "setupMesh":
            {
                if (value is null)
                {
                    instance.m_setupMesh = default;
                    return true;
                }
                if (value is hclSetupMesh castValue)
                {
                    instance.m_setupMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_storeNormals":
            case "storeNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_storeNormals = castValue;
                return true;
            }
            case "m_storeTangentsAndBiTangents":
            case "storeTangentsAndBiTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_storeTangentsAndBiTangents = castValue;
                return true;
            }
            case "m_storeTriangles":
            case "storeTriangles":
            {
                if (value is not bool castValue) return false;
                instance.m_storeTriangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
