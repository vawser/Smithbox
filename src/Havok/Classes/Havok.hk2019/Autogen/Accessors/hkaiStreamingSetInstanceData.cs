// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiStreamingSetInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiStreamingSetInstance>
{
    private static readonly System.Reflection.FieldInfo _sectionIndicesInfo = typeof(Havok.hk2019.hkaiStreamingSetInstance).GetField("m_sectionIndices")!;
    public hkaiStreamingSetInstanceData(Havok.hk2019.hkaiStreamingSetInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectionIndices":
            case "sectionIndices":
            {
                value = instance.m_sectionIndices;
                return true;
            }
            case "m_streamingSet":
            case "streamingSet":
            {
                value = instance.m_streamingSet;
                return true;
            }
            case "m_dynUserEdgeConnections":
            case "dynUserEdgeConnections":
            {
                value = instance.m_dynUserEdgeConnections;
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
            case "m_sectionIndices":
            case "sectionIndices":
            {
                if (value is not int[] castValue || castValue.Length != 2) return false;
                try
                {
                    _sectionIndicesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_streamingSet":
            case "streamingSet":
            {
                if (value is null)
                {
                    instance.m_streamingSet = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiStreamingSet castValue)
                {
                    instance.m_streamingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_dynUserEdgeConnections":
            case "dynUserEdgeConnections":
            {
                if (value is not List<Havok.hk2019.hkaiStreamingSetInstance.DynUserEdgeConnection> castValue) return false;
                instance.m_dynUserEdgeConnections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
