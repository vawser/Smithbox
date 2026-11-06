// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBlenderGeneratorInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBlenderGeneratorInternalState>
{
    public hkbBlenderGeneratorInternalStateData(Havok.hk2018.hkbBlenderGeneratorInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_childrenInternalStates":
            case "childrenInternalStates":
            {
                value = instance.m_childrenInternalStates;
                return true;
            }
            case "m_sortedChildren":
            case "sortedChildren":
            {
                value = instance.m_sortedChildren;
                return true;
            }
            case "m_endIntervalWeight":
            case "endIntervalWeight":
            {
                value = instance.m_endIntervalWeight;
                return true;
            }
            case "m_numActiveChildren":
            case "numActiveChildren":
            {
                value = instance.m_numActiveChildren;
                return true;
            }
            case "m_beginIntervalIndex":
            case "beginIntervalIndex":
            {
                value = instance.m_beginIntervalIndex;
                return true;
            }
            case "m_endIntervalIndex":
            case "endIntervalIndex":
            {
                value = instance.m_endIntervalIndex;
                return true;
            }
            case "m_initSync":
            case "initSync":
            {
                value = instance.m_initSync;
                return true;
            }
            case "m_doSubtractiveBlend":
            case "doSubtractiveBlend":
            {
                value = instance.m_doSubtractiveBlend;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_childrenInternalStates":
            case "childrenInternalStates":
            {
                if (value is not List<Havok.hk2018.hkbBlenderGenerator.ChildInternalState> castValue) return false;
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
