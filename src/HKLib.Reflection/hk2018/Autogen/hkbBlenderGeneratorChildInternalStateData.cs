// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBlenderGeneratorChildInternalStateData : HavokData<hkbBlenderGenerator.ChildInternalState> 
{
    public hkbBlenderGeneratorChildInternalStateData(HavokType type, hkbBlenderGenerator.ChildInternalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isActive":
            case "isActive":
            {
                if (instance.m_isActive is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_syncNextFrame":
            case "syncNextFrame":
            {
                if (instance.m_syncNextFrame is not TGet castValue) return false;
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
            case "m_isActive":
            case "isActive":
            {
                if (value is not bool castValue) return false;
                instance.m_isActive = castValue;
                return true;
            }
            case "m_syncNextFrame":
            case "syncNextFrame":
            {
                if (value is not bool castValue) return false;
                instance.m_syncNextFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
