// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiOverlapManagerPendingUpdateData : HavokData<HKLib.hk2019.hkaiOverlapManager.PendingUpdate> 
{
    public hkaiOverlapManagerPendingUpdateData(HavokType type, HKLib.hk2019.hkaiOverlapManager.PendingUpdate instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_priority":
            case "priority":
            {
                if (instance.m_priority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (instance.m_aabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (instance.m_faces is not TGet castValue) return false;
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
            case "m_priority":
            case "priority":
            {
                if (value is not float castValue) return false;
                instance.m_priority = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not HKLib.hk2019.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                if (value is not List<int> castValue) return false;
                instance.m_faces = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
