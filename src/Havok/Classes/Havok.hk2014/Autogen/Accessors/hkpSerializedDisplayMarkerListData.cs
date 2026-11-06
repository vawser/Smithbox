// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSerializedDisplayMarkerListData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSerializedDisplayMarkerList>
{
    public hkpSerializedDisplayMarkerListData(Havok.hk2014.hkpSerializedDisplayMarkerList instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_markers":
            case "markers":
            {
                value = instance.m_markers;
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
            case "m_markers":
            case "markers":
            {
                if (value is not List<Havok.hk2014.hkpSerializedDisplayMarker?> castValue) return false;
                instance.m_markers = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
