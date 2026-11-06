// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkRPCOnAfterSerializeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.RPC.OnAfterSerialize>
{
    public hkRPCOnAfterSerializeData(Havok.hk2018.hk.RPC.OnAfterSerialize instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_onAfterSerialize":
            case "onAfterSerialize":
            {
                value = instance.m_onAfterSerialize;
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
            case "m_onAfterSerialize":
            case "onAfterSerialize":
            {
                if (value is null)
                {
                    instance.m_onAfterSerialize = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_onAfterSerialize = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
