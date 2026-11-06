// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxNodeAnnotationDataData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxNode.AnnotationData>
{
    public hkxNodeAnnotationDataData(Havok.hk55.hkxNode.AnnotationData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_time":
            case "time":
            {
                value = instance.m_time;
                return true;
            }
            case "m_description":
            case "description":
            {
                value = instance.m_description;
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
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            case "m_description":
            case "description":
            {
                if (value is null)
                {
                    instance.m_description = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_description = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
