// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbSequenceInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbSequenceInternalState>
{
    public hkbSequenceInternalStateData(Havok.hk2014.hkbSequenceInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nextSampleEvents":
            case "nextSampleEvents":
            {
                value = instance.m_nextSampleEvents;
                return true;
            }
            case "m_nextSampleReals":
            case "nextSampleReals":
            {
                value = instance.m_nextSampleReals;
                return true;
            }
            case "m_nextSampleBools":
            case "nextSampleBools":
            {
                value = instance.m_nextSampleBools;
                return true;
            }
            case "m_nextSampleInts":
            case "nextSampleInts":
            {
                value = instance.m_nextSampleInts;
                return true;
            }
            case "m_time":
            case "time":
            {
                value = instance.m_time;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                value = instance.m_isEnabled;
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
            case "m_nextSampleEvents":
            case "nextSampleEvents":
            {
                if (value is not List<int> castValue) return false;
                instance.m_nextSampleEvents = castValue;
                return true;
            }
            case "m_nextSampleReals":
            case "nextSampleReals":
            {
                if (value is not List<int> castValue) return false;
                instance.m_nextSampleReals = castValue;
                return true;
            }
            case "m_nextSampleBools":
            case "nextSampleBools":
            {
                if (value is not List<int> castValue) return false;
                instance.m_nextSampleBools = castValue;
                return true;
            }
            case "m_nextSampleInts":
            case "nextSampleInts":
            {
                if (value is not List<int> castValue) return false;
                instance.m_nextSampleInts = castValue;
                return true;
            }
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
