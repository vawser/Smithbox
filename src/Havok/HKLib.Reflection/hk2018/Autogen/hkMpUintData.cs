// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMpUintData : HavokData<hkMpUint> 
{
    public hkMpUintData(HavokType type, hkMpUint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_atoms":
            case "atoms":
            {
                if (instance.m_atoms is not TGet castValue) return false;
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
            case "m_atoms":
            case "atoms":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_atoms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
