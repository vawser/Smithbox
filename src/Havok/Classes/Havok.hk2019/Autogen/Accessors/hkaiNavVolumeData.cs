// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolume>
{
    private static readonly System.Reflection.FieldInfo _resInfo = typeof(Havok.hk2019.hkaiNavVolume).GetField("m_res")!;
    public hkaiNavVolumeData(Havok.hk2019.hkaiNavVolume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cells":
            case "cells":
            {
                value = instance.m_cells;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                value = instance.m_edges;
                return true;
            }
            case "m_userEdgeInfos":
            case "userEdgeInfos":
            {
                value = instance.m_userEdgeInfos;
                return true;
            }
            case "m_userEdgeData":
            case "userEdgeData":
            {
                value = instance.m_userEdgeData;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                value = instance.m_streamingSets;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                value = instance.m_aabb;
                return true;
            }
            case "m_quantizationScale":
            case "quantizationScale":
            {
                value = instance.m_quantizationScale;
                return true;
            }
            case "m_quantizationOffset":
            case "quantizationOffset":
            {
                value = instance.m_quantizationOffset;
                return true;
            }
            case "m_res":
            case "res":
            {
                value = instance.m_res;
                return true;
            }
            case "m_userEdgeDataStriding":
            case "userEdgeDataStriding":
            {
                value = instance.m_userEdgeDataStriding;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_cachedCellIterator":
            case "cachedCellIterator":
            {
                value = instance.m_cachedCellIterator;
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
            case "m_cells":
            case "cells":
            {
                if (value is not List<Havok.hk2019.hkaiNavVolume.Cell> castValue) return false;
                instance.m_cells = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<Havok.hk2019.hkaiNavVolume.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_userEdgeInfos":
            case "userEdgeInfos":
            {
                if (value is not List<Havok.hk2019.hkaiNavVolume.UserEdgeInfo> castValue) return false;
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
                if (value is not List<Havok.hk2019.hkaiAnnotatedStreamingSet> castValue) return false;
                instance.m_streamingSets = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not Havok.hk2019.hkAabb castValue) return false;
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
                if (value is Havok.hk2019.hkaiNavVolumeStaticTreeCellIterator castValue)
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
