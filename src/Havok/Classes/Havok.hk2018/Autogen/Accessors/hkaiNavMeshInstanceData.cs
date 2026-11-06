// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshInstance>
{
    public hkaiNavMeshInstanceData(Havok.hk2018.hkaiNavMeshInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                value = instance.m_instanceData;
                return true;
            }
            case "m_clearanceCaches":
            case "clearanceCaches":
            {
                value = instance.m_clearanceCaches;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                value = instance.m_referenceFrame;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                if (value is not Havok.hk2018.hkaiCopyOnWritePtr<Havok.hk2018.hkaiNavMeshInstanceData> castValue) return false;
                instance.m_instanceData = castValue;
                return true;
            }
            case "m_clearanceCaches":
            case "clearanceCaches":
            {
                if (value is not List<Havok.hk2018.hkaiCopyOnWritePtr<Havok.hk2018.hkaiNavMeshClearanceCache>> castValue) return false;
                instance.m_clearanceCaches = castValue;
                return true;
            }
            case "m_referenceFrame":
            case "referenceFrame":
            {
                if (value is not Havok.hk2018.hkaiReferenceFrame castValue) return false;
                instance.m_referenceFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
