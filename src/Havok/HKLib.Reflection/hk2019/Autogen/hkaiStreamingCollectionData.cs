// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiStreamingCollectionData : HavokData<HKLib.hk2019.hkaiStreamingCollection> 
{
    private static readonly System.Reflection.FieldInfo _erosionRadiiInfo = typeof(HKLib.hk2019.hkaiStreamingCollection).GetField("m_erosionRadii")!;
    public hkaiStreamingCollectionData(HavokType type, HKLib.hk2019.hkaiStreamingCollection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isTemporary":
            case "isTemporary":
            {
                if (instance.m_isTemporary is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                if (instance.m_tree is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                if (instance.m_instances is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_streamingSetInstances":
            case "streamingSetInstances":
            {
                if (instance.m_streamingSetInstances is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceCacheManager":
            case "clearanceCacheManager":
            {
                if (instance.m_clearanceCacheManager is null)
                {
                    return true;
                }
                if (instance.m_clearanceCacheManager is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_erosionRadii":
            case "erosionRadii":
            {
                if (instance.m_erosionRadii is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dynamicNavMeshMediator":
            case "dynamicNavMeshMediator":
            {
                if (instance.m_dynamicNavMeshMediator is null)
                {
                    return true;
                }
                if (instance.m_dynamicNavMeshMediator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_dynamicNavVolumeMediator":
            case "dynamicNavVolumeMediator":
            {
                if (instance.m_dynamicNavVolumeMediator is null)
                {
                    return true;
                }
                if (instance.m_dynamicNavVolumeMediator is TGet castValue)
                {
                    value = castValue;
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
                if (value is not HKLib.hk2019.hkaiCopyOnWritePtr<HKLib.hk2019.hkcdDynamicAabbTree> castValue) return false;
                instance.m_tree = castValue;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                if (value is not List<HKLib.hk2019.hkaiStreamingCollectionInstanceInfo> castValue) return false;
                instance.m_instances = castValue;
                return true;
            }
            case "m_streamingSetInstances":
            case "streamingSetInstances":
            {
                if (value is not List<HKLib.hk2019.hkaiCopyOnWritePtr<HKLib.hk2019.hkaiStreamingSetInstance>> castValue) return false;
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
                if (value is HKLib.hk2019.hkaiNavMeshClearanceCacheManager castValue)
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
                if (value is HKLib.hk2019.hkaiNavMeshCollectionQueryMediator castValue)
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
                if (value is HKLib.hk2019.hkaiNavVolumeCollectionQueryMediator castValue)
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
