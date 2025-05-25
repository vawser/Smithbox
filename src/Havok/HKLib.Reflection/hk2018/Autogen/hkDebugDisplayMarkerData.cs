// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkDebugDisplayMarkerData : HavokData<hkDebugDisplayMarker> 
{
    public hkDebugDisplayMarkerData(HavokType type, hkDebugDisplayMarker instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_type is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (value is hkDebugDisplayMarker.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (hkDebugDisplayMarker.Type)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
