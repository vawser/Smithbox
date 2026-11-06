// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclScratchBufferSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclScratchBufferSetupObject>
{
    public hclScratchBufferSetupObjectData(Havok.hk2014.hclScratchBufferSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_setupMesh":
            case "setupMesh":
            {
                value = instance.m_setupMesh;
                return true;
            }
            case "m_storeNormals":
            case "storeNormals":
            {
                value = instance.m_storeNormals;
                return true;
            }
            case "m_storeTangentsAndBiTangents":
            case "storeTangentsAndBiTangents":
            {
                value = instance.m_storeTangentsAndBiTangents;
                return true;
            }
            case "m_storeTriangles":
            case "storeTriangles":
            {
                value = instance.m_storeTriangles;
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
                if (value is Havok.hk2014.hclSetupMesh castValue)
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
