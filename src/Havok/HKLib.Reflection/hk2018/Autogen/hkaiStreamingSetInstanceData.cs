// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiStreamingSetInstanceData : HavokData<hkaiStreamingSetInstance> 
{
    private static readonly System.Reflection.FieldInfo _sectionIndicesInfo = typeof(hkaiStreamingSetInstance).GetField("m_sectionIndices")!;
    public hkaiStreamingSetInstanceData(HavokType type, hkaiStreamingSetInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is hkaiStreamingSet castValue)
                {
                    instance.m_streamingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_dynUserEdgeConnections":
            case "dynUserEdgeConnections":
            {
                if (value is not List<hkaiStreamingSetInstance.DynUserEdgeConnection> castValue) return false;
                instance.m_dynUserEdgeConnections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
