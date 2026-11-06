// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclMoveParticlesOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclMoveParticlesOperator>
{
    public hclMoveParticlesOperatorData(Havok.hk2016.hclMoveParticlesOperator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_operatorID":
            case "operatorID":
            {
                value = instance.m_operatorID;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                value = instance.m_usedBuffers;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                value = instance.m_usedTransformSets;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
            case "m_operatorID":
            case "operatorID":
            {
                if (value is not uint castValue) return false;
                instance.m_operatorID = castValue;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                if (value is not List<Havok.hk2016.hclClothState.BufferAccess> castValue) return false;
                instance.m_usedBuffers = castValue;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                if (value is not List<Havok.hk2016.hclClothState.TransformSetAccess> castValue) return false;
                instance.m_usedTransformSets = castValue;
                return true;
            }
            case "m_vertexParticlePairs":
            case "vertexParticlePairs":
            {
                if (value is not List<Havok.hk2016.hclMoveParticlesOperator.VertexParticlePair> castValue) return false;
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
