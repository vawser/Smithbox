// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclMoveParticlesOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclMoveParticlesOperator>
{
    public hclMoveParticlesOperatorData(Havok.hk2014.hclMoveParticlesOperator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_vertexParticlePairs":
            case "vertexParticlePairs":
            {
                value = instance.m_vertexParticlePairs;
                return true;
            }
            case "m_simClothIndex":
            case "simClothIndex":
            {
                value = instance.m_simClothIndex;
                return true;
            }
            case "m_refBufferIdx":
            case "refBufferIdx":
            {
                value = instance.m_refBufferIdx;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexParticlePairs":
            case "vertexParticlePairs":
            {
                if (value is not List<Havok.hk2014.hclMoveParticlesOperator.VertexParticlePair> castValue) return false;
                instance.m_vertexParticlePairs = castValue;
                return true;
            }
            case "m_simClothIndex":
            case "simClothIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_simClothIndex = castValue;
                return true;
            }
            case "m_refBufferIdx":
            case "refBufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_refBufferIdx = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
