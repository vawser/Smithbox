// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkcdPlanarSolidNodeStorageData : HavokData<hkcdPlanarSolid.NodeStorage> 
{
    public hkcdPlanarSolidNodeStorageData(HavokType type, hkcdPlanarSolid.NodeStorage instance) : base(type, instance) {}

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
            case "m_storage":
            case "storage":
            {
                if (instance.m_storage is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabbs":
            case "aabbs":
            {
                if (instance.m_aabbs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstFreeNodeId":
            case "firstFreeNodeId":
            {
                if (instance.m_firstFreeNodeId is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_storage":
            case "storage":
            {
                if (value is not List<hkcdPlanarSolid.Node> castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            case "m_aabbs":
            case "aabbs":
            {
                if (value is not List<hkAabb> castValue) return false;
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
