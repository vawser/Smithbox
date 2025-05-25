// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hke;

namespace HKLib.Reflection.hk2018;

internal class hkeCanAddToParentData : HavokData<CanAddToParent> 
{
    public hkeCanAddToParentData(HavokType type, CanAddToParent instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_canAddToParentFunc":
            case "canAddToParentFunc":
            {
                if (instance.m_canAddToParentFunc is null)
                {
                    return true;
                }
                if (instance.m_canAddToParentFunc is TGet castValue)
                {
                    value = castValue;
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
