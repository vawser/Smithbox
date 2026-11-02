// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshModifiedAabbsData : HavokData<HKLib.hk2019.hkaiNavMeshModifiedAabbs> 
{
    public hkaiNavMeshModifiedAabbsData(HavokType type, HKLib.hk2019.hkaiNavMeshModifiedAabbs instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_modifiedAabbs":
            case "modifiedAabbs":
            {
                if (instance.m_modifiedAabbs is not TGet castValue) return false;
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
            case "m_modifiedAabbs":
            case "modifiedAabbs":
            {
                if (value is not List<List<HKLib.hk2019.hkAabb>> castValue) return false;
                instance.m_modifiedAabbs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
