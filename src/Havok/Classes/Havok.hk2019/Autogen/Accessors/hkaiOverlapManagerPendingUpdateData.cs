// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiOverlapManagerPendingUpdateData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiOverlapManager.PendingUpdate>
{
    public hkaiOverlapManagerPendingUpdateData(Havok.hk2019.hkaiOverlapManager.PendingUpdate instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_priority":
            case "priority":
            {
                value = instance.m_priority;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
                return true;
            }
            case "m_faces":
            case "faces":
            {
                value = instance.m_faces;
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
                if (value is not Havok.hk2019.hkAabb castValue) return false;
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
