// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiStreamingCollectionData : HavokData<hkaiStreamingCollection> 
{
    private static readonly System.Reflection.FieldInfo _erosionRadiiInfo = typeof(hkaiStreamingCollection).GetField("m_erosionRadii")!;
    public hkaiStreamingCollectionData(HavokType type, hkaiStreamingCollection instance) : base(type, instance) {}

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is not hkaiCopyOnWritePtr<hkcdDynamicAabbTree> castValue) return false;
                instance.m_tree = castValue;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                if (value is not List<hkaiStreamingCollectionInstanceInfo> castValue) return false;
                instance.m_instances = castValue;
                return true;
            }
            case "m_streamingSetInstances":
            case "streamingSetInstances":
            {
                if (value is not List<hkaiCopyOnWritePtr<hkaiStreamingSetInstance>> castValue) return false;
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
                if (value is hkaiNavMeshClearanceCacheManager castValue)
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
                if (value is hkaiDynamicNavMeshQueryMediator castValue)
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
                if (value is hkaiDynamicNavVolumeMediator castValue)
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
