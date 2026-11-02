// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshCutConfigurationFaceInfoData : HavokData<HKLib.hk2019.hkaiNavMeshCutConfiguration.FaceInfo> 
{
    public hkaiNavMeshCutConfigurationFaceInfoData(HavokType type, HKLib.hk2019.hkaiNavMeshCutConfiguration.FaceInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_storage":
            case "storage":
            {
                if (instance.m_storage is not TGet castValue) return false;
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
            case "m_storage":
            case "storage":
            {
                if (value is not uint castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
