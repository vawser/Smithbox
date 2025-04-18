// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeData : HavokData<hkaiNavVolume> 
{
    private static readonly System.Reflection.FieldInfo _resInfo = typeof(hkaiNavVolume).GetField("m_res")!;
    public hkaiNavVolumeData(HavokType type, hkaiNavVolume instance) : base(type, instance) {}

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
            case "m_cells":
            case "cells":
            {
                if (value is not List<hkaiNavVolume.Cell> castValue) return false;
                instance.m_cells = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<hkaiNavVolume.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_userEdgeInfos":
            case "userEdgeInfos":
            {
                if (value is not List<hkaiNavVolume.UserEdgeInfo> castValue) return false;
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
                if (value is not List<hkaiAnnotatedStreamingSet> castValue) return false;
                instance.m_streamingSets = castValue;
                return true;
            }
            case "m_aabb":
            case "aabb":
            {
                if (value is not hkAabb castValue) return false;
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
            default:
            return false;
        }
    }

}
