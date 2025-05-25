// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiStaticTreeNavMeshQueryMediatorData : HavokData<hkaiStaticTreeNavMeshQueryMediator> 
{
    public hkaiStaticTreeNavMeshQueryMediatorData(HavokType type, hkaiStaticTreeNavMeshQueryMediator instance) : base(type, instance) {}

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
            case "m_tree":
            case "tree":
            {
                if (instance.m_tree is null)
                {
                    return true;
                }
                if (instance.m_tree is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_tree":
            case "tree":
            {
                if (value is null)
                {
                    instance.m_tree = default;
                    return true;
                }
                if (value is hkcdStaticAabbTree castValue)
                {
                    instance.m_tree = castValue;
                    return true;
                }
                return false;
            }
            case "m_navMesh":
            case "navMesh":
            {
                if (value is null)
                {
                    instance.m_navMesh = default;
                    return true;
                }
                if (value is hkaiNavMesh castValue)
                {
                    instance.m_navMesh = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
