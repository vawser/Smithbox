// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbContextData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbContext>
{
    public hkbContextData(Havok.hk2014.hkbContext instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rootBehavior":
            case "rootBehavior":
            {
                value = instance.m_rootBehavior;
                return true;
            }
            case "m_generatorOutputListener":
            case "generatorOutputListener":
            {
                value = instance.m_generatorOutputListener;
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
            case "m_rootBehavior":
            case "rootBehavior":
            {
                if (value is null)
                {
                    instance.m_rootBehavior = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbBehaviorGraph castValue)
                {
                    instance.m_rootBehavior = castValue;
                    return true;
                }
                return false;
            }
            case "m_generatorOutputListener":
            case "generatorOutputListener":
            {
                if (value is null)
                {
                    instance.m_generatorOutputListener = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbGeneratorOutputListener castValue)
                {
                    instance.m_generatorOutputListener = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
