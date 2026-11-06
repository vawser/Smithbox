// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdPlanarSolidNodeStorageData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdPlanarSolid.NodeStorage>
{
    public hkcdPlanarSolidNodeStorageData(Havok.hk2018.hkcdPlanarSolid.NodeStorage instance) : base(instance) { }

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
            case "m_storage":
            case "storage":
            {
                value = instance.m_storage;
                return true;
            }
            case "m_aabbs":
            case "aabbs":
            {
                value = instance.m_aabbs;
                return true;
            }
            case "m_firstFreeNodeId":
            case "firstFreeNodeId":
            {
                value = instance.m_firstFreeNodeId;
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
            case "m_storage":
            case "storage":
            {
                if (value is not List<Havok.hk2018.hkcdPlanarSolid.Node> castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            case "m_aabbs":
            case "aabbs":
            {
                if (value is not List<Havok.hk2018.hkAabb> castValue) return false;
                instance.m_aabbs = castValue;
                return true;
            }
            case "m_firstFreeNodeId":
            case "firstFreeNodeId":
            {
                if (value is not uint castValue) return false;
                instance.m_firstFreeNodeId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
