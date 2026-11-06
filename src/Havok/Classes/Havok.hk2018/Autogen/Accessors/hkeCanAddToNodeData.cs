// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkeCanAddToNodeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hke.CanAddToNode>
{
    public hkeCanAddToNodeData(Havok.hk2018.hke.CanAddToNode instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canAddToNodeFunc":
            case "canAddToNodeFunc":
            {
                value = instance.m_canAddToNodeFunc;
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
            case "m_canAddToNodeFunc":
            case "canAddToNodeFunc":
            {
                if (value is null)
                {
                    instance.m_canAddToNodeFunc = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_canAddToNodeFunc = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
