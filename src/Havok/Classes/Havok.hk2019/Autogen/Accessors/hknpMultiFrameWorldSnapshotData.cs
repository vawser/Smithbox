// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMultiFrameWorldSnapshotData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMultiFrameWorldSnapshot>
{
    public hknpMultiFrameWorldSnapshotData(Havok.hk2019.hknpMultiFrameWorldSnapshot instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_snapshots":
            case "snapshots":
            {
                value = instance.m_snapshots;
                return true;
            }
            case "m_bodyIdShapeMap":
            case "bodyIdShapeMap":
            {
                value = instance.m_bodyIdShapeMap;
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
            case "m_snapshots":
            case "snapshots":
            {
                if (value is not List<Havok.hk2019.hknpWorldSnapshot?> castValue) return false;
                instance.m_snapshots = castValue;
                return true;
            }
            case "m_bodyIdShapeMap":
            case "bodyIdShapeMap":
            {
                if (value is not Havok.hk2019.hkHashMap<uint, Havok.hk2019.hknpShape?> castValue) return false;
                instance.m_bodyIdShapeMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
