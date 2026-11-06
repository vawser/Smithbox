// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbSetBehaviorCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbSetBehaviorCommand>
{
    public hkbSetBehaviorCommandData(Havok.hk2014.hkbSetBehaviorCommand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_behavior":
            case "behavior":
            {
                value = instance.m_behavior;
                return true;
            }
            case "m_rootGenerator":
            case "rootGenerator":
            {
                value = instance.m_rootGenerator;
                return true;
            }
            case "m_referencedBehaviors":
            case "referencedBehaviors":
            {
                value = instance.m_referencedBehaviors;
                return true;
            }
            case "m_startStateIndex":
            case "startStateIndex":
            {
                value = instance.m_startStateIndex;
                return true;
            }
            case "m_randomizeSimulation":
            case "randomizeSimulation":
            {
                value = instance.m_randomizeSimulation;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                value = instance.m_padding;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_behavior":
            case "behavior":
            {
                if (value is null)
                {
                    instance.m_behavior = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbBehaviorGraph castValue)
                {
                    instance.m_behavior = castValue;
                    return true;
                }
                return false;
            }
            case "m_rootGenerator":
            case "rootGenerator":
            {
                if (value is null)
                {
                    instance.m_rootGenerator = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbGenerator castValue)
                {
                    instance.m_rootGenerator = castValue;
                    return true;
                }
                return false;
            }
            case "m_referencedBehaviors":
            case "referencedBehaviors":
            {
                if (value is not List<Havok.hk2014.hkbBehaviorGraph?> castValue) return false;
                instance.m_referencedBehaviors = castValue;
                return true;
            }
            case "m_startStateIndex":
            case "startStateIndex":
            {
                if (value is not int castValue) return false;
                instance.m_startStateIndex = castValue;
                return true;
            }
            case "m_randomizeSimulation":
            case "randomizeSimulation":
            {
                if (value is not bool castValue) return false;
                instance.m_randomizeSimulation = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (value is not int castValue) return false;
                instance.m_padding = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
