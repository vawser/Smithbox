// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiStreamingCollectionInstanceInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiStreamingCollectionInstanceInfo>
{
    public hkaiStreamingCollectionInstanceInfoData(Havok.hk2018.hkaiStreamingCollectionInstanceInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_instance":
            case "instance":
            {
                value = instance.m_instance;
                return true;
            }
            case "m_volumeInstance":
            case "volumeInstance":
            {
                value = instance.m_volumeInstance;
                return true;
            }
            case "m_clusterGraphInstance":
            case "clusterGraphInstance":
            {
                value = instance.m_clusterGraphInstance;
                return true;
            }
            case "m_mediator":
            case "mediator":
            {
                value = instance.m_mediator;
                return true;
            }
            case "m_volumeMediator":
            case "volumeMediator":
            {
                value = instance.m_volumeMediator;
                return true;
            }
            case "m_treeNode":
            case "treeNode":
            {
                value = instance.m_treeNode;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                value = instance.m_sectionUid;
                return true;
            }
            case "m_sectionReferenceFrame":
            case "sectionReferenceFrame":
            {
                value = instance.m_sectionReferenceFrame;
                return true;
            }
            case "m_volumeTransform":
            case "volumeTransform":
            {
                value = instance.m_volumeTransform;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                value = instance.m_streamingSets;
                return true;
            }
            case "m_graphStreamingSets":
            case "graphStreamingSets":
            {
                value = instance.m_graphStreamingSets;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_instance":
            case "instance":
            {
                if (value is null)
                {
                    instance.m_instance = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavMeshInstance castValue)
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
                if (value is Havok.hk2018.hkaiNavVolumeInstance castValue)
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
                if (value is Havok.hk2018.hkaiDirectedGraphInstance castValue)
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
                if (value is Havok.hk2018.hkaiNavMeshQueryMediator castValue)
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
                if (value is Havok.hk2018.hkaiNavVolumeMediator castValue)
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
                if (value is not Havok.hk2018.hkaiReferenceFrame castValue) return false;
                instance.m_sectionReferenceFrame = castValue;
                return true;
            }
            case "m_volumeTransform":
            case "volumeTransform":
            {
                if (value is not Havok.hk2018.hkAxialTransform castValue) return false;
                instance.m_volumeTransform = castValue;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (value is not Havok.hk2018.hkHashMap<int, int> castValue) return false;
                instance.m_streamingSets = castValue;
                return true;
            }
            case "m_graphStreamingSets":
            case "graphStreamingSets":
            {
                if (value is not Havok.hk2018.hkHashMap<int, Havok.hk2018.hkaiStreamingSet?> castValue) return false;
                instance.m_graphStreamingSets = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2018.hkaiStreamingCollectionInstanceInfo.Flags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2018.hkaiStreamingCollectionInstanceInfo.Flags)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
