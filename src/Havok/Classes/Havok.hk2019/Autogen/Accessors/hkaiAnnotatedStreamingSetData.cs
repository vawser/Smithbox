// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiAnnotatedStreamingSetData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiAnnotatedStreamingSet>
{
    public hkaiAnnotatedStreamingSetData(Havok.hk2019.hkaiAnnotatedStreamingSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_side":
            case "side":
            {
                value = instance.m_side;
                return true;
            }
            case "m_streamingSet":
            case "streamingSet":
            {
                value = instance.m_streamingSet;
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
            case "m_side":
            case "side":
            {
                if (value is Havok.hk2019.hkaiAnnotatedStreamingSet.Side castValue)
                {
                    instance.m_side = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_side = (Havok.hk2019.hkaiAnnotatedStreamingSet.Side)byteValue;
                    return true;
                }
                return false;
            }
            case "m_streamingSet":
            case "streamingSet":
            {
                if (value is null)
                {
                    instance.m_streamingSet = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiStreamingSet castValue)
                {
                    instance.m_streamingSet = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
