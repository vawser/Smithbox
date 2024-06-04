// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSequenceInternalStateData : HavokData<hkbSequenceInternalState> 
{
    public hkbSequenceInternalStateData(HavokType type, hkbSequenceInternalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextSampleEvents":
            case "nextSampleEvents":
            {
                if (instance.m_nextSampleEvents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextSampleReals":
            case "nextSampleReals":
            {
                if (instance.m_nextSampleReals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextSampleBools":
            case "nextSampleBools":
            {
                if (instance.m_nextSampleBools is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextSampleInts":
            case "nextSampleInts":
            {
                if (instance.m_nextSampleInts is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_time":
            case "time":
            {
                if (instance.m_time is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (instance.m_isEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
