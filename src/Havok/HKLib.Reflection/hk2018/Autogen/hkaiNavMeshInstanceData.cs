// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshInstanceData : HavokData<hkaiNavMeshInstance> 
{
    public hkaiNavMeshInstanceData(HavokType type, hkaiNavMeshInstance instance) : base(type, instance) {}

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
            case "m_instanceData":
            case "instanceData":
            {
                if (instance.m_instanceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceCaches":
            case "clearanceCaches":
            {
                if (instance.m_clearanceCaches is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (instance.m_referenceFrame is not TGet castValue) return false;
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
            case "m_instanceData":
            case "instanceData":
            {
                if (value is not hkaiCopyOnWritePtr<HKLib.hk2018.hkaiNavMeshInstanceData> castValue) return false;
                instance.m_instanceData = castValue;
                return true;
            }
            case "m_clearanceCaches":
            case "clearanceCaches":
            {
                if (value is not List<hkaiCopyOnWritePtr<hkaiNavMeshClearanceCache>> castValue) return false;
                instance.m_clearanceCaches = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not hkaiReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
