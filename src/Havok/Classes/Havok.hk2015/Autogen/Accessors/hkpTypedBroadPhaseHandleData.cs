// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpTypedBroadPhaseHandleData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpTypedBroadPhaseHandle>
{
    public hkpTypedBroadPhaseHandleData(Havok.hk2015.hkpTypedBroadPhaseHandle instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_objectQualityType":
            case "objectQualityType":
            {
                value = instance.m_objectQualityType;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
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
            case "m_type":
            case "type":
            {
                if (value is not sbyte castValue) return false;
                instance.m_type = castValue;
                return true;
            }
            case "m_objectQualityType":
            case "objectQualityType":
            {
                if (value is not sbyte castValue) return false;
                instance.m_objectQualityType = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
