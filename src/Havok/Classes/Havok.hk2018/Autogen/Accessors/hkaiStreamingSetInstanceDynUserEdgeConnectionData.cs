// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiStreamingSetInstanceDynUserEdgeConnectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiStreamingSetInstance.DynUserEdgeConnection>
{
    private static readonly System.Reflection.FieldInfo _faceIndicesInfo = typeof(Havok.hk2018.hkaiStreamingSetInstance.DynUserEdgeConnection).GetField("m_faceIndices")!;
    private static readonly System.Reflection.FieldInfo _edgeOffsetsInfo = typeof(Havok.hk2018.hkaiStreamingSetInstance.DynUserEdgeConnection).GetField("m_edgeOffsets")!;
    public hkaiStreamingSetInstanceDynUserEdgeConnectionData(Havok.hk2018.hkaiStreamingSetInstance.DynUserEdgeConnection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIndices":
            case "faceIndices":
            {
                value = instance.m_faceIndices;
                return true;
            }
            case "m_edgeOffsets":
            case "edgeOffsets":
            {
                value = instance.m_edgeOffsets;
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
