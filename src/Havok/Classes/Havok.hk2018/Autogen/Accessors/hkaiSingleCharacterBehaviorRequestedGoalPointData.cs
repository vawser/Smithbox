// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiSingleCharacterBehaviorRequestedGoalPointData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiSingleCharacterBehavior.RequestedGoalPoint>
{
    public hkaiSingleCharacterBehaviorRequestedGoalPointData(Havok.hk2018.hkaiSingleCharacterBehavior.RequestedGoalPoint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_sectionId":
            case "sectionId":
            {
                value = instance.m_sectionId;
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
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_sectionId":
            case "sectionId":
            {
                if (value is not int castValue) return false;
                instance.m_sectionId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
