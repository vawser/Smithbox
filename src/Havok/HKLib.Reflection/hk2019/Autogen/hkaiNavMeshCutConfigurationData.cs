// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshCutConfigurationData : HavokData<HKLib.hk2019.hkaiNavMeshCutConfiguration> 
{
    public hkaiNavMeshCutConfigurationData(HavokType type, HKLib.hk2019.hkaiNavMeshCutConfiguration instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectionId":
            case "sectionId":
            {
                if (instance.m_sectionId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_navMesh":
            case "navMesh":
            {
                if (instance.m_navMesh is null)
                {
                    return true;
                }
                if (instance.m_navMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_faceInfos":
            case "faceInfos":
            {
                if (instance.m_faceInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bigFaceInfos":
            case "bigFaceInfos":
            {
                if (instance.m_bigFaceInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicUserEdges":
            case "dynamicUserEdges":
            {
                if (instance.m_dynamicUserEdges is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkaiNavMesh castValue)
                {
                    instance.m_navMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_faceInfos":
            case "faceInfos":
            {
                if (value is not HKLib.hk2019.hkHashMap<int, HKLib.hk2019.hkaiNavMeshCutConfiguration.FaceInfo> castValue) return false;
                instance.m_faceInfos = castValue;
                return true;
            }
            case "m_bigFaceInfos":
            case "bigFaceInfos":
            {
                if (value is not HKLib.hk2019.hkHashMap<int, HKLib.hk2019.hkaiNavMeshCutConfiguration.BigFaceInfo> castValue) return false;
                instance.m_bigFaceInfos = castValue;
                return true;
            }
            case "m_dynamicUserEdges":
            case "dynamicUserEdges":
            {
                if (value is not HKLib.hk2019.hkHashMap<int, List<HKLib.hk2019.hkaiNavMeshCutConfiguration.DynamicUserEdge>> castValue) return false;
                instance.m_dynamicUserEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
