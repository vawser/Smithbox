// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpParticle4FacesData : HavokData<hknpParticle4Faces> 
{
    public hknpParticle4FacesData(HavokType type, hknpParticle4Faces instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_directions":
            case "directions":
            {
                if (instance.m_directions is not TGet castValue) return false;
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
            case "m_directions":
            case "directions":
            {
                if (value is not hkFourTransposedPointsf castValue) return false;
                instance.m_directions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
