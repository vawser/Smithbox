// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclClothStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclClothState>
{
    public hclClothStateData(Havok.hk2014.hclClothState instance) : base(instance) { }

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
            case "m_operators":
            case "operators":
            {
                value = instance.m_operators;
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
            case "m_usedSimCloths":
            case "usedSimCloths":
            {
                value = instance.m_usedSimCloths;
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
            case "m_operators":
            case "operators":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_operators = castValue;
                return true;
            }
            case "m_usedBuffers":
            case "usedBuffers":
            {
                if (value is not List<Havok.hk2014.hclClothState.BufferAccess> castValue) return false;
                instance.m_usedBuffers = castValue;
                return true;
            }
            case "m_usedTransformSets":
            case "usedTransformSets":
            {
                if (value is not List<Havok.hk2014.hclClothState.TransformSetAccess> castValue) return false;
                instance.m_usedTransformSets = castValue;
                return true;
            }
            case "m_usedSimCloths":
            case "usedSimCloths":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_usedSimCloths = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
