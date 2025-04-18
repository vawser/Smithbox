// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class PolygonSetup2dData : HavokData<PolygonSetup2d> 
{
    public PolygonSetup2dData(HavokType type, PolygonSetup2d instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pad":
            case "pad":
            {
                if (instance.m_pad is not TGet castValue) return false;
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
            case "m_pad":
            case "pad":
            {
                if (value is not int castValue) return false;
                instance.m_pad = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
