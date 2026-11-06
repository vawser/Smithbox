// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclMoveParticlesOperatorVertexParticlePairData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclMoveParticlesOperator.VertexParticlePair>
{
    public hclMoveParticlesOperatorVertexParticlePairData(Havok.hk2018.hclMoveParticlesOperator.VertexParticlePair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexIndex":
            case "vertexIndex":
            {
                value = instance.m_vertexIndex;
                return true;
            }
            case "m_particleIndex":
            case "particleIndex":
            {
                value = instance.m_particleIndex;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
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
