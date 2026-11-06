// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpVdbShapeDisplayMarkerData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpVdbShapeDisplayMarker>
{
    public hknpVdbShapeDisplayMarkerData(Havok.hk2019.hknpVdbShapeDisplayMarker instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hkDebugDisplayMarker.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_type = (Havok.hk2019.hkDebugDisplayMarker.Type)intValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
