// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiStreamingCollectionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiStreamingCollection>
{
    private static readonly System.Reflection.FieldInfo _erosionRadiiInfo = typeof(Havok.hk2019.hkaiStreamingCollection).GetField("m_erosionRadii")!;
    public hkaiStreamingCollectionData(Havok.hk2019.hkaiStreamingCollection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isTemporary":
            case "isTemporary":
            {
                value = instance.m_isTemporary;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                value = instance.m_tree;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                value = instance.m_instances;
                return true;
            }
            case "m_streamingSetInstances":
            case "streamingSetInstances":
            {
                value = instance.m_streamingSetInstances;
                return true;
            }
            case "m_clearanceCacheManager":
            case "clearanceCacheManager":
            {
                value = instance.m_clearanceCacheManager;
                return true;
            }
            case "m_erosionRadii":
            case "erosionRadii":
            {
                value = instance.m_erosionRadii;
                return true;
            }
            case "m_dynamicNavMeshMediator":
            case "dynamicNavMeshMediator":
            {
                value = instance.m_dynamicNavMeshMediator;
                return true;
            }
            case "m_dynamicNavVolumeMediator":
            case "dynamicNavVolumeMediator":
            {
                value = instance.m_dynamicNavVolumeMediator;
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
            case "m_isTemporary":
            case "isTemporary":
            {
                if (value is not bool castValue) return false;
                instance.m_isTemporary = castValue;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                if (value is not Havok.hk2019.hkaiCopyOnWritePtr<Havok.hk2019.hkcdDynamicAabbTree> castValue) return false;
                instance.m_tree = castValue;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                if (value is not List<Havok.hk2019.hkaiStreamingCollectionInstanceInfo> castValue) return false;
                instance.m_instances = castValue;
                return true;
            }
            case "m_streamingSetInstances":
            case "streamingSetInstances":
            {
                if (value is not List<Havok.hk2019.hkaiCopyOnWritePtr<Havok.hk2019.hkaiStreamingSetInstance>> castValue) return false;
                instance.m_streamingSetInstances = castValue;
                return true;
            }
            case "m_clearanceCacheManager":
            case "clearanceCacheManager":
            {
                if (value is null)
                {
                    instance.m_clearanceCacheManager = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavMeshClearanceCacheManager castValue)
                {
                    instance.m_clearanceCacheManager = castValue;
                    return true;
                }
                return false;
            }
            case "m_erosionRadii":
            case "erosionRadii":
            {
                if (value is not float[] castValue || castValue.Length != 32) return false;
                try
                {
                    _erosionRadiiInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_dynamicNavMeshMediator":
            case "dynamicNavMeshMediator":
            {
                if (value is null)
                {
                    instance.m_dynamicNavMeshMediator = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavMeshCollectionQueryMediator castValue)
                {
                    instance.m_dynamicNavMeshMediator = castValue;
                    return true;
                }
                return false;
            }
            case "m_dynamicNavVolumeMediator":
            case "dynamicNavVolumeMediator":
            {
                if (value is null)
                {
                    instance.m_dynamicNavVolumeMediator = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiNavVolumeCollectionQueryMediator castValue)
                {
                    instance.m_dynamicNavVolumeMediator = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
