// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeInstanceDataData : HavokData<HKLib.hk2018.hkaiNavVolumeInstanceData> 
{
    public hkaiNavVolumeInstanceDataData(HavokType type, HKLib.hk2018.hkaiNavVolumeInstanceData instance) : base(type, instance) {}

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
            case "m_originalVolume":
            case "originalVolume":
            {
                if (instance.m_originalVolume is null)
                {
                    return true;
                }
                if (instance.m_originalVolume is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_cellMap":
            case "cellMap":
            {
                if (instance.m_cellMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instancedCells":
            case "instancedCells":
            {
                if (instance.m_instancedCells is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (instance.m_ownedEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedUserEdgeInfos":
            case "ownedUserEdgeInfos":
            {
                if (instance.m_ownedUserEdgeInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ownedUserEdgeData":
            case "ownedUserEdgeData":
            {
                if (instance.m_ownedUserEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                if (instance.m_sectionUid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                if (instance.m_runtimeId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (instance.m_layerIndex is not TGet castValue) return false;
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
            case "m_originalVolume":
            case "originalVolume":
            {
                if (value is null)
                {
                    instance.m_originalVolume = default;
                    return true;
                }
                if (value is hkaiNavVolume castValue)
                {
                    instance.m_originalVolume = castValue;
                    return true;
                }
                return false;
            }
            case "m_cellMap":
            case "cellMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_cellMap = castValue;
                return true;
            }
            case "m_instancedCells":
            case "instancedCells":
            {
                if (value is not List<HKLib.hk2018.hkaiNavVolumeInstanceData.CellInstance> castValue) return false;
                instance.m_instancedCells = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (value is not List<hkaiNavVolume.Edge> castValue) return false;
                instance.m_ownedEdges = castValue;
                return true;
            }
            case "m_ownedUserEdgeInfos":
            case "ownedUserEdgeInfos":
            {
                if (value is not List<hkaiNavVolume.UserEdgeInfo> castValue) return false;
                instance.m_ownedUserEdgeInfos = castValue;
                return true;
            }
            case "m_ownedUserEdgeData":
            case "ownedUserEdgeData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_ownedUserEdgeData = castValue;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                if (value is not uint castValue) return false;
                instance.m_sectionUid = castValue;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                if (value is not int castValue) return false;
                instance.m_runtimeId = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
