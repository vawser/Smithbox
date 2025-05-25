// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBlenderGeneratorInternalStateData : HavokData<hkbBlenderGeneratorInternalState> 
{
    public hkbBlenderGeneratorInternalStateData(HavokType type, hkbBlenderGeneratorInternalState instance) : base(type, instance) {}

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
            case "m_childrenInternalStates":
            case "childrenInternalStates":
            {
                if (instance.m_childrenInternalStates is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sortedChildren":
            case "sortedChildren":
            {
                if (instance.m_sortedChildren is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endIntervalWeight":
            case "endIntervalWeight":
            {
                if (instance.m_endIntervalWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numActiveChildren":
            case "numActiveChildren":
            {
                if (instance.m_numActiveChildren is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_beginIntervalIndex":
            case "beginIntervalIndex":
            {
                if (instance.m_beginIntervalIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endIntervalIndex":
            case "endIntervalIndex":
            {
                if (instance.m_endIntervalIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initSync":
            case "initSync":
            {
                if (instance.m_initSync is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_doSubtractiveBlend":
            case "doSubtractiveBlend":
            {
                if (instance.m_doSubtractiveBlend is not TGet castValue) return false;
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
            case "m_childrenInternalStates":
            case "childrenInternalStates":
            {
                if (value is not List<hkbBlenderGenerator.ChildInternalState> castValue) return false;
                instance.m_childrenInternalStates = castValue;
                return true;
            }
            case "m_sortedChildren":
            case "sortedChildren":
            {
                if (value is not List<short> castValue) return false;
                instance.m_sortedChildren = castValue;
                return true;
            }
            case "m_endIntervalWeight":
            case "endIntervalWeight":
            {
                if (value is not float castValue) return false;
                instance.m_endIntervalWeight = castValue;
                return true;
            }
            case "m_numActiveChildren":
            case "numActiveChildren":
            {
                if (value is not int castValue) return false;
                instance.m_numActiveChildren = castValue;
                return true;
            }
            case "m_beginIntervalIndex":
            case "beginIntervalIndex":
            {
                if (value is not short castValue) return false;
                instance.m_beginIntervalIndex = castValue;
                return true;
            }
            case "m_endIntervalIndex":
            case "endIntervalIndex":
            {
                if (value is not short castValue) return false;
                instance.m_endIntervalIndex = castValue;
                return true;
            }
            case "m_initSync":
            case "initSync":
            {
                if (value is not bool castValue) return false;
                instance.m_initSync = castValue;
                return true;
            }
            case "m_doSubtractiveBlend":
            case "doSubtractiveBlend":
            {
                if (value is not bool castValue) return false;
                instance.m_doSubtractiveBlend = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
