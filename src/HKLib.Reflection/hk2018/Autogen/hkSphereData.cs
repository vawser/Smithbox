// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkSphereData : HavokData<hkSphere> 
{
    public hkSphereData(HavokType type, hkSphere instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pos":
            case "pos":
            {
                if (instance.m_pos is not TGet castValue) return false;
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
            case "m_pos":
            case "pos":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pos = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
