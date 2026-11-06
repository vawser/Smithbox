// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpWorldQueryViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpWorldQueryViewer.Options>
{
    public hknpWorldQueryViewerOptionsData(Havok.hk2019.hknpWorldQueryViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_colorBasedOnTime":
            case "colorBasedOnTime":
            {
                value = instance.m_colorBasedOnTime;
                return true;
            }
            case "m_maxTimeMs":
            case "maxTimeMs":
            {
                value = instance.m_maxTimeMs;
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
            case "m_colorBasedOnTime":
            case "colorBasedOnTime":
            {
                if (value is not bool castValue) return false;
                instance.m_colorBasedOnTime = castValue;
                return true;
            }
            case "m_maxTimeMs":
            case "maxTimeMs":
            {
                if (value is not float castValue) return false;
                instance.m_maxTimeMs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
