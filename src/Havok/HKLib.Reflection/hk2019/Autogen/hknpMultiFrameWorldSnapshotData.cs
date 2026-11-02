// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpMultiFrameWorldSnapshotData : HavokData<HKLib.hk2019.hknpMultiFrameWorldSnapshot> 
{
    public hknpMultiFrameWorldSnapshotData(HavokType type, HKLib.hk2019.hknpMultiFrameWorldSnapshot instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_snapshots":
            case "snapshots":
            {
                if (instance.m_snapshots is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyIdShapeMap":
            case "bodyIdShapeMap":
            {
                if (instance.m_bodyIdShapeMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_snapshots":
            case "snapshots":
            {
                if (value is not List<HKLib.hk2019.hknpWorldSnapshot?> castValue) return false;
                instance.m_snapshots = castValue;
                return true;
            }
            case "m_bodyIdShapeMap":
            case "bodyIdShapeMap":
            {
                if (value is not HKLib.hk2019.hkHashMap<uint, HKLib.hk2019.hknpShape?> castValue) return false;
                instance.m_bodyIdShapeMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
