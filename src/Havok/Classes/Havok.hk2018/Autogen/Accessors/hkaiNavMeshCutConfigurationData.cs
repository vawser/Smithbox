// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshCutConfigurationData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshCutConfiguration>
{
    public hkaiNavMeshCutConfigurationData(Havok.hk2018.hkaiNavMeshCutConfiguration instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectionId":
            case "sectionId":
            {
                value = instance.m_sectionId;
                return true;
            }
            case "m_navMesh":
            case "navMesh":
            {
                value = instance.m_navMesh;
                return true;
            }
            case "m_faceInfos":
            case "faceInfos":
            {
                value = instance.m_faceInfos;
                return true;
            }
            case "m_bigFaceInfos":
            case "bigFaceInfos":
            {
                value = instance.m_bigFaceInfos;
                return true;
            }
            case "m_dynamicUserEdges":
            case "dynamicUserEdges":
            {
                value = instance.m_dynamicUserEdges;
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
            case "m_sectionId":
            case "sectionId":
            {
                if (value is not int castValue) return false;
                instance.m_sectionId = castValue;
                return true;
            }
            case "m_navMesh":
            case "navMesh":
            {
                if (value is null)
                {
                    instance.m_navMesh = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavMesh castValue)
                {
                    instance.m_navMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_faceInfos":
            case "faceInfos":
            {
                if (value is not Havok.hk2018.hkHashMap<int, Havok.hk2018.hkaiNavMeshCutConfiguration.FaceInfo> castValue) return false;
                instance.m_faceInfos = castValue;
                return true;
            }
            case "m_bigFaceInfos":
            case "bigFaceInfos":
            {
                if (value is not Havok.hk2018.hkHashMap<int, Havok.hk2018.hkaiNavMeshCutConfiguration.BigFaceInfo> castValue) return false;
                instance.m_bigFaceInfos = castValue;
                return true;
            }
            case "m_dynamicUserEdges":
            case "dynamicUserEdges":
            {
                if (value is not Havok.hk2018.hkHashMap<int, List<Havok.hk2018.hkaiNavMeshCutConfiguration.DynamicUserEdge>> castValue) return false;
                instance.m_dynamicUserEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
