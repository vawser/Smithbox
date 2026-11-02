// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiStreamingSetInstanceData : HavokData<HKLib.hk2019.hkaiStreamingSetInstance> 
{
    private static readonly System.Reflection.FieldInfo _sectionIndicesInfo = typeof(HKLib.hk2019.hkaiStreamingSetInstance).GetField("m_sectionIndices")!;
    public hkaiStreamingSetInstanceData(HavokType type, HKLib.hk2019.hkaiStreamingSetInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectionIndices":
            case "sectionIndices":
            {
                if (instance.m_sectionIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_streamingSet":
            case "streamingSet":
            {
                if (instance.m_streamingSet is null)
                {
                    return true;
                }
                if (instance.m_streamingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_dynUserEdgeConnections":
            case "dynUserEdgeConnections":
            {
                if (instance.m_dynUserEdgeConnections is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkaiStreamingSet castValue)
                {
                    instance.m_streamingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_dynUserEdgeConnections":
            case "dynUserEdgeConnections":
            {
                if (value is not List<HKLib.hk2019.hkaiStreamingSetInstance.DynUserEdgeConnection> castValue) return false;
                instance.m_dynUserEdgeConnections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
