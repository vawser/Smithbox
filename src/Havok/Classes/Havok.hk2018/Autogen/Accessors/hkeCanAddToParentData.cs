// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkeCanAddToParentData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hke.CanAddToParent>
{
    public hkeCanAddToParentData(Havok.hk2018.hke.CanAddToParent instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canAddToParentFunc":
            case "canAddToParentFunc":
            {
                value = instance.m_canAddToParentFunc;
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
            case "m_canAddToParentFunc":
            case "canAddToParentFunc":
            {
                if (value is null)
                {
                    instance.m_canAddToParentFunc = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_canAddToParentFunc = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
