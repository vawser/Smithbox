// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshInstanceCutterData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshInstanceCutter>
{
    public hkaiNavMeshInstanceCutterData(Havok.hk2019.hkaiNavMeshInstanceCutter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_streamingCollection":
            case "streamingCollection":
            {
                value = instance.m_streamingCollection;
                return true;
            }
            case "m_sectionIdx":
            case "sectionIdx":
            {
                value = instance.m_sectionIdx;
                return true;
            }
            case "m_cutConfiguration":
            case "cutConfiguration":
            {
                value = instance.m_cutConfiguration;
                return true;
            }
            case "m_addStreamingFaces":
            case "addStreamingFaces":
            {
                value = instance.m_addStreamingFaces;
                return true;
            }
            case "m_removeStreamingFaces":
            case "removeStreamingFaces":
            {
                value = instance.m_removeStreamingFaces;
                return true;
            }
            case "m_modifiedFaces":
            case "modifiedFaces":
            {
                value = instance.m_modifiedFaces;
                return true;
            }
            case "m_needMatchStreamingFaceIndices":
            case "needMatchStreamingFaceIndices":
            {
                value = instance.m_needMatchStreamingFaceIndices;
                return true;
            }
            case "m_modifiedAabbs":
            case "modifiedAabbs":
            {
                value = instance.m_modifiedAabbs;
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
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (value is null)
                {
                    instance.m_streamingCollection = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiStreamingCollection castValue)
                {
                    instance.m_streamingCollection = castValue;
                    return true;
                }
                return false;
            }
            case "m_sectionIdx":
            case "sectionIdx":
            {
                if (value is not int castValue) return false;
                instance.m_sectionIdx = castValue;
                return true;
            }
            case "m_cutConfiguration":
            case "cutConfiguration":
            {
                if (value is not Havok.hk2019.hkaiNavMeshCutConfiguration castValue) return false;
                instance.m_cutConfiguration = castValue;
                return true;
            }
            case "m_addStreamingFaces":
            case "addStreamingFaces":
            {
                if (value is not List<int> castValue) return false;
                instance.m_addStreamingFaces = castValue;
                return true;
            }
            case "m_removeStreamingFaces":
            case "removeStreamingFaces":
            {
                if (value is not List<int> castValue) return false;
                instance.m_removeStreamingFaces = castValue;
                return true;
            }
            case "m_modifiedFaces":
            case "modifiedFaces":
            {
                if (value is not Havok.hk2019.hkHashSet<int> castValue) return false;
                instance.m_modifiedFaces = castValue;
                return true;
            }
            case "m_needMatchStreamingFaceIndices":
            case "needMatchStreamingFaceIndices":
            {
                if (value is not Havok.hk2019.hkHashSet<int> castValue) return false;
                instance.m_needMatchStreamingFaceIndices = castValue;
                return true;
            }
            case "m_modifiedAabbs":
            case "modifiedAabbs":
            {
                if (value is not List<Havok.hk2019.hkAabb> castValue) return false;
                instance.m_modifiedAabbs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
