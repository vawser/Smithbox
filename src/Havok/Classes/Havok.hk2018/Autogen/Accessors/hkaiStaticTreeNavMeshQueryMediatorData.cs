// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiStaticTreeNavMeshQueryMediatorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiStaticTreeNavMeshQueryMediator>
{
    public hkaiStaticTreeNavMeshQueryMediatorData(Havok.hk2018.hkaiStaticTreeNavMeshQueryMediator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_tree":
            case "tree":
            {
                value = instance.m_tree;
                return true;
            }
            case "m_navMesh":
            case "navMesh":
            {
                value = instance.m_navMesh;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2018.hkcdStaticAabbTree castValue)
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
                if (value is Havok.hk2018.hkaiNavMesh castValue)
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
