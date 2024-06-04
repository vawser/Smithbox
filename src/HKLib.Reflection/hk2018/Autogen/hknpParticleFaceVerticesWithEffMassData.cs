// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpParticleFaceVerticesWithEffMassData : HavokData<hknpParticleFaceVerticesWithEffMass> 
{
    public hknpParticleFaceVerticesWithEffMassData(HavokType type, hknpParticleFaceVerticesWithEffMass instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceVertices":
            case "faceVertices":
            {
                if (instance.m_faceVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_effectiveMasses":
            case "effectiveMasses":
            {
                if (instance.m_effectiveMasses is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_contactRadius":
            case "contactRadius":
            {
                if (instance.m_contactRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_distance":
            case "distance":
            {
                if (instance.m_distance is not TGet castValue) return false;
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
            case "m_faceVertices":
            case "faceVertices":
            {
                if (value is not hkFourTransposedPointsf castValue) return false;
                instance.m_faceVertices = castValue;
                return true;
            }
            case "m_effectiveMasses":
            case "effectiveMasses":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_effectiveMasses = castValue;
                return true;
            }
            case "m_contactRadius":
            case "contactRadius":
            {
                if (value is not float castValue) return false;
                instance.m_contactRadius = castValue;
                return true;
            }
            case "m_distance":
            case "distance":
            {
                if (value is not float castValue) return false;
                instance.m_distance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
