// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdPlanarSolidNodeStorageData : HavokData<HKLib.hk2019.hkcdPlanarSolid.NodeStorage> 
{
    public hkcdPlanarSolidNodeStorageData(HavokType type, HKLib.hk2019.hkcdPlanarSolid.NodeStorage instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_storage":
            case "storage":
            {
                if (value is not List<HKLib.hk2019.hkcdPlanarSolid.Node> castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            case "m_aabbs":
            case "aabbs":
            {
                if (value is not List<HKLib.hk2019.hkAabb> castValue) return false;
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
