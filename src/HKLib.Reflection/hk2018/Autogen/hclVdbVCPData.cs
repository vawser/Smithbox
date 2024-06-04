// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVdbVCPData : HavokData<hclVdbVCP> 
{
    public hclVdbVCPData(HavokType type, hclVdbVCP instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_particleId":
            case "particleId":
            {
                if (instance.m_particleId is not TGet castValue) return false;
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
            case "m_particleId":
            case "particleId":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
