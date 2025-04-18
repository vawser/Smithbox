// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclMoveParticlesOperatorVertexParticlePairData : HavokData<hclMoveParticlesOperator.VertexParticlePair> 
{
    public hclMoveParticlesOperatorVertexParticlePairData(HavokType type, hclMoveParticlesOperator.VertexParticlePair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexIndex":
            case "vertexIndex":
            {
                if (instance.m_vertexIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                if (instance.m_particleIndex is not TGet castValue) return false;
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
            case "m_vertexIndex":
            case "vertexIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_vertexIndex = castValue;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
