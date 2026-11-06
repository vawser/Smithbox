// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavVolumeDebugUtilsDebugInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavVolumeDebugUtils.DebugInfo>
{
    public hkaiNavVolumeDebugUtilsDebugInfoData(Havok.hk2018.hkaiNavVolumeDebugUtils.DebugInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transparency":
            case "transparency":
            {
                value = instance.m_transparency;
                return true;
            }
            case "m_shrinkCells":
            case "shrinkCells":
            {
                value = instance.m_shrinkCells;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                value = instance.m_displayOffset;
                return true;
            }
            case "m_showCells":
            case "showCells":
            {
                value = instance.m_showCells;
                return true;
            }
            case "m_showCellBorders":
            case "showCellBorders":
            {
                value = instance.m_showCellBorders;
                return true;
            }
            case "m_showAabb":
            case "showAabb":
            {
                value = instance.m_showAabb;
                return true;
            }
            case "m_labelEdges":
            case "labelEdges":
            {
                value = instance.m_labelEdges;
                return true;
            }
            case "m_labelCells":
            case "labelCells":
            {
                value = instance.m_labelCells;
                return true;
            }
            case "m_showEdgeConnections":
            case "showEdgeConnections":
            {
                value = instance.m_showEdgeConnections;
                return true;
            }
            case "m_showUserEdgePortals":
            case "showUserEdgePortals":
            {
                value = instance.m_showUserEdgePortals;
                return true;
            }
            case "m_showUserEdgeConnections":
            case "showUserEdgeConnections":
            {
                value = instance.m_showUserEdgeConnections;
                return true;
            }
            case "m_colorCellsRandomly":
            case "colorCellsRandomly":
            {
                value = instance.m_colorCellsRandomly;
                return true;
            }
            case "m_colorRegions":
            case "colorRegions":
            {
                value = instance.m_colorRegions;
                return true;
            }
            case "m_numMaterialColors":
            case "numMaterialColors":
            {
                value = instance.m_numMaterialColors;
                return true;
            }
            case "m_materialColors":
            case "materialColors":
            {
                value = instance.m_materialColors;
                return true;
            }
            case "m_showCellData":
            case "showCellData":
            {
                value = instance.m_showCellData;
                return true;
            }
            case "m_cellColor":
            case "cellColor":
            {
                value = instance.m_cellColor;
                return true;
            }
            case "m_cellBorderColor":
            case "cellBorderColor":
            {
                value = instance.m_cellBorderColor;
                return true;
            }
            case "m_internalEdgeColor":
            case "internalEdgeColor":
            {
                value = instance.m_internalEdgeColor;
                return true;
            }
            case "m_externalEdgeColor":
            case "externalEdgeColor":
            {
                value = instance.m_externalEdgeColor;
                return true;
            }
            case "m_userEdgeEntryColor":
            case "userEdgeEntryColor":
            {
                value = instance.m_userEdgeEntryColor;
                return true;
            }
            case "m_userEdgeExitColor":
            case "userEdgeExitColor":
            {
                value = instance.m_userEdgeExitColor;
                return true;
            }
            case "m_userEdgeConnectionColor":
            case "userEdgeConnectionColor":
            {
                value = instance.m_userEdgeConnectionColor;
                return true;
            }
            case "m_showSingleRegionIndex":
            case "showSingleRegionIndex":
            {
                value = instance.m_showSingleRegionIndex;
                return true;
            }
            case "m_showSingleSliceIndex":
            case "showSingleSliceIndex":
            {
                value = instance.m_showSingleSliceIndex;
                return true;
            }
            case "m_sliceAxis":
            case "sliceAxis":
            {
                value = instance.m_sliceAxis;
                return true;
            }
            case "m_instanceEnabled":
            case "instanceEnabled":
            {
                value = instance.m_instanceEnabled;
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
                if (value is object castValue)
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
                if (value is not Havok.hk2018.hkBitField castValue) return false;
                instance.m_instanceEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
