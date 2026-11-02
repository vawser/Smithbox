// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiStreamingSetInstanceDynUserEdgeConnectionData : HavokData<HKLib.hk2019.hkaiStreamingSetInstance.DynUserEdgeConnection> 
{
    private static readonly System.Reflection.FieldInfo _faceIndicesInfo = typeof(HKLib.hk2019.hkaiStreamingSetInstance.DynUserEdgeConnection).GetField("m_faceIndices")!;
    private static readonly System.Reflection.FieldInfo _edgeOffsetsInfo = typeof(HKLib.hk2019.hkaiStreamingSetInstance.DynUserEdgeConnection).GetField("m_edgeOffsets")!;
    public hkaiStreamingSetInstanceDynUserEdgeConnectionData(HavokType type, HKLib.hk2019.hkaiStreamingSetInstance.DynUserEdgeConnection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIndices":
            case "faceIndices":
            {
                if (instance.m_faceIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeOffsets":
            case "edgeOffsets":
            {
                if (instance.m_edgeOffsets is not TGet castValue) return false;
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
            case "m_faceIndices":
            case "faceIndices":
            {
                if (value is not int[] castValue || castValue.Length != 2) return false;
                try
                {
                    _faceIndicesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_edgeOffsets":
            case "edgeOffsets":
            {
                if (value is not ushort[] castValue || castValue.Length != 2) return false;
                try
                {
                    _edgeOffsetsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
