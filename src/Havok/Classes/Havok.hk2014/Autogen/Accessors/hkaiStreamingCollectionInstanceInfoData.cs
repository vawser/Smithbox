// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiStreamingCollectionInstanceInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiStreamingCollectionInstanceInfo>
{
    public hkaiStreamingCollectionInstanceInfoData(Havok.hk2014.hkaiStreamingCollectionInstanceInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_instancePtr":
            case "instancePtr":
            {
                value = instance.m_instancePtr;
                return true;
            }
            case "m_volumeInstancePtr":
            case "volumeInstancePtr":
            {
                value = instance.m_volumeInstancePtr;
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
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_instancePtr":
            case "instancePtr":
            {
                if (value is null)
                {
                    instance.m_instancePtr = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiNavMeshInstance castValue)
                {
                    instance.m_instancePtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_volumeInstancePtr":
            case "volumeInstancePtr":
            {
                if (value is null)
                {
                    instance.m_volumeInstancePtr = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiNavVolumeInstance castValue)
                {
                    instance.m_volumeInstancePtr = castValue;
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
                if (value is Havok.hk2014.hkaiDirectedGraphInstance castValue)
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
                if (value is Havok.hk2014.hkaiNavMeshQueryMediator castValue)
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
                if (value is Havok.hk2014.hkaiNavVolumeMediator castValue)
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
            default:
            return false;
        }
    }

}
