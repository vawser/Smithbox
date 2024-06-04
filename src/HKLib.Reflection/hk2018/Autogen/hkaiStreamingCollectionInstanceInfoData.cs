// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiStreamingCollectionInstanceInfoData : HavokData<hkaiStreamingCollectionInstanceInfo> 
{
    public hkaiStreamingCollectionInstanceInfoData(HavokType type, hkaiStreamingCollectionInstanceInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_instance":
            case "instance":
            {
                if (instance.m_instance is null)
                {
                    return true;
                }
                if (instance.m_instance is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_volumeInstance":
            case "volumeInstance":
            {
                if (instance.m_volumeInstance is null)
                {
                    return true;
                }
                if (instance.m_volumeInstance is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_clusterGraphInstance":
            case "clusterGraphInstance":
            {
                if (instance.m_clusterGraphInstance is null)
                {
                    return true;
                }
                if (instance.m_clusterGraphInstance is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_mediator":
            case "mediator":
            {
                if (instance.m_mediator is null)
                {
                    return true;
                }
                if (instance.m_mediator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_volumeMediator":
            case "volumeMediator":
            {
                if (instance.m_volumeMediator is null)
                {
                    return true;
                }
                if (instance.m_volumeMediator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_treeNode":
            case "treeNode":
            {
                if (instance.m_treeNode is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                if (instance.m_sectionUid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionReferenceFrame":
            case "sectionReferenceFrame":
            {
                if (instance.m_sectionReferenceFrame is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_volumeTransform":
            case "volumeTransform":
            {
                if (instance.m_volumeTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (instance.m_streamingSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_graphStreamingSets":
            case "graphStreamingSets":
            {
                if (instance.m_graphStreamingSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_flags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_instance":
            case "instance":
            {
                if (value is null)
                {
                    instance.m_instance = default;
                    return true;
                }
                if (value is hkaiNavMeshInstance castValue)
                {
                    instance.m_instance = castValue;
                    return true;
                }
                return false;
            }
            case "m_volumeInstance":
            case "volumeInstance":
            {
                if (value is null)
                {
                    instance.m_volumeInstance = default;
                    return true;
                }
                if (value is hkaiNavVolumeInstance castValue)
                {
                    instance.m_volumeInstance = castValue;
                    return true;
                }
                return false;
            }
            case "m_clusterGraphInstance":
            case "clusterGraphInstance":
            {
                if (value is null)
                {
                    instance.m_clusterGraphInstance = default;
                    return true;
                }
                if (value is hkaiDirectedGraphInstance castValue)
                {
                    instance.m_clusterGraphInstance = castValue;
                    return true;
                }
                return false;
            }
            case "m_mediator":
            case "mediator":
            {
                if (value is null)
                {
                    instance.m_mediator = default;
                    return true;
                }
                if (value is hkaiNavMeshQueryMediator castValue)
                {
                    instance.m_mediator = castValue;
                    return true;
                }
                return false;
            }
            case "m_volumeMediator":
            case "volumeMediator":
            {
                if (value is null)
                {
                    instance.m_volumeMediator = default;
                    return true;
                }
                if (value is hkaiNavVolumeMediator castValue)
                {
                    instance.m_volumeMediator = castValue;
                    return true;
                }
                return false;
            }
            case "m_treeNode":
            case "treeNode":
            {
                if (value is not uint castValue) return false;
                instance.m_treeNode = castValue;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                if (value is not uint castValue) return false;
                instance.m_sectionUid = castValue;
                return true;
            }
            case "m_sectionReferenceFrame":
            case "sectionReferenceFrame":
            {
                if (value is not hkaiReferenceFrame castValue) return false;
                instance.m_sectionReferenceFrame = castValue;
                return true;
            }
            case "m_volumeTransform":
            case "volumeTransform":
            {
                if (value is not hkAxialTransform castValue) return false;
                instance.m_volumeTransform = castValue;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (value is not hkHashMap<int, int> castValue) return false;
                instance.m_streamingSets = castValue;
                return true;
            }
            case "m_graphStreamingSets":
            case "graphStreamingSets":
            {
                if (value is not hkHashMap<int, hkaiStreamingSet?> castValue) return false;
                instance.m_graphStreamingSets = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hkaiStreamingCollectionInstanceInfo.Flags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (hkaiStreamingCollectionInstanceInfo.Flags)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
