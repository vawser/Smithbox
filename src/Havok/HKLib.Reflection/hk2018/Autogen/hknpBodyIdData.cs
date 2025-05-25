// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpBodyIdData : HavokData<hknpBodyId> 
{
    public hknpBodyIdData(HavokType type, hknpBodyId instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_serialAndIndex":
            case "serialAndIndex":
            {
                if (instance.m_serialAndIndex is not TGet castValue) return false;
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
            case "m_serialAndIndex":
            case "serialAndIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_serialAndIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
