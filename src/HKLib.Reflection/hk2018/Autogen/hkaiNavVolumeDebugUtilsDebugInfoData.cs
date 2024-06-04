// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavVolumeDebugUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeDebugUtilsDebugInfoData : HavokData<DebugInfo> 
{
    public hkaiNavVolumeDebugUtilsDebugInfoData(HavokType type, DebugInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transparency":
            case "transparency":
            {
                if (instance.m_transparency is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shrinkCells":
            case "shrinkCells":
            {
                if (instance.m_shrinkCells is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (instance.m_displayOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showCells":
            case "showCells":
            {
                if (instance.m_showCells is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showCellBorders":
            case "showCellBorders":
            {
                if (instance.m_showCellBorders is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showAabb":
            case "showAabb":
            {
                if (instance.m_showAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_labelEdges":
            case "labelEdges":
            {
                if (instance.m_labelEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_labelCells":
            case "labelCells":
            {
                if (instance.m_labelCells is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showEdgeConnections":
            case "showEdgeConnections":
            {
                if (instance.m_showEdgeConnections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showUserEdgePortals":
            case "showUserEdgePortals":
            {
                if (instance.m_showUserEdgePortals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showUserEdgeConnections":
            case "showUserEdgeConnections":
            {
                if (instance.m_showUserEdgeConnections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_colorCellsRandomly":
            case "colorCellsRandomly":
            {
                if (instance.m_colorCellsRandomly is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_colorRegions":
            case "colorRegions":
            {
                if (instance.m_colorRegions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numMaterialColors":
            case "numMaterialColors":
            {
                if (instance.m_numMaterialColors is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialColors":
            case "materialColors":
            {
                if (instance.m_materialColors is null)
                {
                    return true;
                }
                if (instance.m_materialColors is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_showCellData":
            case "showCellData":
            {
                if (instance.m_showCellData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cellColor":
            case "cellColor":
            {
                if (instance.m_cellColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cellBorderColor":
            case "cellBorderColor":
            {
                if (instance.m_cellBorderColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_internalEdgeColor":
            case "internalEdgeColor":
            {
                if (instance.m_internalEdgeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_externalEdgeColor":
            case "externalEdgeColor":
            {
                if (instance.m_externalEdgeColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeEntryColor":
            case "userEdgeEntryColor":
            {
                if (instance.m_userEdgeEntryColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeExitColor":
            case "userEdgeExitColor":
            {
                if (instance.m_userEdgeExitColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeConnectionColor":
            case "userEdgeConnectionColor":
            {
                if (instance.m_userEdgeConnectionColor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showSingleRegionIndex":
            case "showSingleRegionIndex":
            {
                if (instance.m_showSingleRegionIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showSingleSliceIndex":
            case "showSingleSliceIndex":
            {
                if (instance.m_showSingleSliceIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sliceAxis":
            case "sliceAxis":
            {
                if (instance.m_sliceAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceEnabled":
            case "instanceEnabled":
            {
                if (instance.m_instanceEnabled is not TGet castValue) return false;
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
            case "m_transparency":
            case "transparency":
            {
                if (value is not float castValue) return false;
                instance.m_transparency = castValue;
                return true;
            }
            case "m_shrinkCells":
            case "shrinkCells":
            {
                if (value is not bool castValue) return false;
                instance.m_shrinkCells = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_displayOffset = castValue;
                return true;
            }
            case "m_showCells":
            case "showCells":
            {
                if (value is not bool castValue) return false;
                instance.m_showCells = castValue;
                return true;
            }
            case "m_showCellBorders":
            case "showCellBorders":
            {
                if (value is not bool castValue) return false;
                instance.m_showCellBorders = castValue;
                return true;
            }
            case "m_showAabb":
            case "showAabb":
            {
                if (value is not bool castValue) return false;
                instance.m_showAabb = castValue;
                return true;
            }
            case "m_labelEdges":
            case "labelEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_labelEdges = castValue;
                return true;
            }
            case "m_labelCells":
            case "labelCells":
            {
                if (value is not bool castValue) return false;
                instance.m_labelCells = castValue;
                return true;
            }
            case "m_showEdgeConnections":
            case "showEdgeConnections":
            {
                if (value is not bool castValue) return false;
                instance.m_showEdgeConnections = castValue;
                return true;
            }
            case "m_showUserEdgePortals":
            case "showUserEdgePortals":
            {
                if (value is not bool castValue) return false;
                instance.m_showUserEdgePortals = castValue;
                return true;
            }
            case "m_showUserEdgeConnections":
            case "showUserEdgeConnections":
            {
                if (value is not bool castValue) return false;
                instance.m_showUserEdgeConnections = castValue;
                return true;
            }
            case "m_colorCellsRandomly":
            case "colorCellsRandomly":
            {
                if (value is not bool castValue) return false;
                instance.m_colorCellsRandomly = castValue;
                return true;
            }
            case "m_colorRegions":
            case "colorRegions":
            {
                if (value is not bool castValue) return false;
                instance.m_colorRegions = castValue;
                return true;
            }
            case "m_numMaterialColors":
            case "numMaterialColors":
            {
                if (value is not int castValue) return false;
                instance.m_numMaterialColors = castValue;
                return true;
            }
            case "m_materialColors":
            case "materialColors":
            {
                if (value is null)
                {
                    instance.m_materialColors = default;
                    return true;
                }
                if (value is Color castValue)
                {
                    instance.m_materialColors = castValue;
                    return true;
                }
                return false;
            }
            case "m_showCellData":
            case "showCellData":
            {
                if (value is not bool castValue) return false;
                instance.m_showCellData = castValue;
                return true;
            }
            case "m_cellColor":
            case "cellColor":
            {
                if (value is not Color castValue) return false;
                instance.m_cellColor = castValue;
                return true;
            }
            case "m_cellBorderColor":
            case "cellBorderColor":
            {
                if (value is not Color castValue) return false;
                instance.m_cellBorderColor = castValue;
                return true;
            }
            case "m_internalEdgeColor":
            case "internalEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_internalEdgeColor = castValue;
                return true;
            }
            case "m_externalEdgeColor":
            case "externalEdgeColor":
            {
                if (value is not Color castValue) return false;
                instance.m_externalEdgeColor = castValue;
                return true;
            }
            case "m_userEdgeEntryColor":
            case "userEdgeEntryColor":
            {
                if (value is not Color castValue) return false;
                instance.m_userEdgeEntryColor = castValue;
                return true;
            }
            case "m_userEdgeExitColor":
            case "userEdgeExitColor":
            {
                if (value is not Color castValue) return false;
                instance.m_userEdgeExitColor = castValue;
                return true;
            }
            case "m_userEdgeConnectionColor":
            case "userEdgeConnectionColor":
            {
                if (value is not Color castValue) return false;
                instance.m_userEdgeConnectionColor = castValue;
                return true;
            }
            case "m_showSingleRegionIndex":
            case "showSingleRegionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_showSingleRegionIndex = castValue;
                return true;
            }
            case "m_showSingleSliceIndex":
            case "showSingleSliceIndex":
            {
                if (value is not int castValue) return false;
                instance.m_showSingleSliceIndex = castValue;
                return true;
            }
            case "m_sliceAxis":
            case "sliceAxis":
            {
                if (value is not int castValue) return false;
                instance.m_sliceAxis = castValue;
                return true;
            }
            case "m_instanceEnabled":
            case "instanceEnabled":
            {
                if (value is not hkBitField castValue) return false;
                instance.m_instanceEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
