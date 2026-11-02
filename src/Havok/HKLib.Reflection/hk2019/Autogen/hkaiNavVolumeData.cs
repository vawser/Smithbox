// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavVolumeData : HavokData<HKLib.hk2019.hkaiNavVolume> 
{
    private static readonly System.Reflection.FieldInfo _resInfo = typeof(HKLib.hk2019.hkaiNavVolume).GetField("m_res")!;
    public hkaiNavVolumeData(HavokType type, HKLib.hk2019.hkaiNavVolume instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cells":
            case "cells":
            {
                if (instance.m_cells is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (instance.m_edges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeInfos":
            case "userEdgeInfos":
            {
                if (instance.m_userEdgeInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeData":
            case "userEdgeData":
            {
                if (instance.m_userEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (instance.m_streamingSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (instance.m_aabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quantizationScale":
            case "quantizationScale":
            {
                if (instance.m_quantizationScale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quantizationOffset":
            case "quantizationOffset":
            {
                if (instance.m_quantizationOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_res":
            case "res":
            {
                if (instance.m_res is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeDataStriding":
            case "userEdgeDataStriding":
            {
                if (instance.m_userEdgeDataStriding is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cachedCellIterator":
            case "cachedCellIterator":
            {
                if (instance.m_cachedCellIterator is null)
                {
                    return true;
                }
                if (instance.m_cachedCellIterator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_cells":
            case "cells":
            {
                if (value is not List<HKLib.hk2019.hkaiNavVolume.Cell> castValue) return false;
                instance.m_cells = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<HKLib.hk2019.hkaiNavVolume.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_userEdgeInfos":
            case "userEdgeInfos":
            {
                if (value is not List<HKLib.hk2019.hkaiNavVolume.UserEdgeInfo> castValue) return false;
                instance.m_userEdgeInfos = castValue;
                return true;
            }
            case "m_userEdgeData":
            case "userEdgeData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_userEdgeData = castValue;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (value is not List<HKLib.hk2019.hkaiAnnotatedStreamingSet> castValue) return false;
                instance.m_streamingSets = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not HKLib.hk2019.hkAabb castValue) return false;
                instance.m_aabb = castValue;
                return true;
            }
            case "m_quantizationScale":
            case "quantizationScale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_quantizationScale = castValue;
                return true;
            }
            case "m_quantizationOffset":
            case "quantizationOffset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_quantizationOffset = castValue;
                return true;
            }
            case "m_res":
            case "res":
            {
                if (value is not int[] castValue || castValue.Length != 3) return false;
                try
                {
                    _resInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_userEdgeDataStriding":
            case "userEdgeDataStriding":
            {
                if (value is not int castValue) return false;
                instance.m_userEdgeDataStriding = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_cachedCellIterator":
            case "cachedCellIterator":
            {
                if (value is null)
                {
                    instance.m_cachedCellIterator = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavVolumeStaticTreeCellIterator castValue)
                {
                    instance.m_cachedCellIterator = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
