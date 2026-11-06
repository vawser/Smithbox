// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbBoneWeightArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbBoneWeightArray>
{
    public hkbBoneWeightArrayData(Havok.hk2014.hkbBoneWeightArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                value = instance.m_boneWeights;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (value is not List<float> castValue) return false;
                instance.m_boneWeights = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
