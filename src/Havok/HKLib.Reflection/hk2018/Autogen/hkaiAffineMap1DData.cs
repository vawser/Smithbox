// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiAffineMap1DData : HavokData<hkaiAffineMap1D> 
{
    public hkaiAffineMap1DData(HavokType type, hkaiAffineMap1D instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_scale":
            case "scale":
            {
                if (instance.m_scale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                if (instance.m_offset is not TGet castValue) return false;
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
            case "m_scale":
            case "scale":
            {
                if (value is not float castValue) return false;
                instance.m_scale = castValue;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                if (value is not float castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
