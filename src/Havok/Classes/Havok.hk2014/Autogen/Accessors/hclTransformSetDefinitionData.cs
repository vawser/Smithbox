// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclTransformSetDefinitionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclTransformSetDefinition>
{
    public hclTransformSetDefinitionData(Havok.hk2014.hclTransformSetDefinition instance) : base(instance) { }

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
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_numTransforms":
            case "numTransforms":
            {
                value = instance.m_numTransforms;
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
            case "m_type":
            case "type":
            {
                if (value is not int castValue) return false;
                instance.m_type = castValue;
                return true;
            }
            case "m_numTransforms":
            case "numTransforms":
            {
                if (value is not uint castValue) return false;
                instance.m_numTransforms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
