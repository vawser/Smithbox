// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBoneSpaceDeformerData : HavokData<hclBoneSpaceDeformer> 
{
    public hclBoneSpaceDeformerData(HavokType type, hclBoneSpaceDeformer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fourBlendEntries":
            case "fourBlendEntries":
            {
                if (instance.m_fourBlendEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_threeBlendEntries":
            case "threeBlendEntries":
            {
                if (instance.m_threeBlendEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_twoBlendEntries":
            case "twoBlendEntries":
            {
                if (instance.m_twoBlendEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_oneBlendEntries":
            case "oneBlendEntries":
            {
                if (instance.m_oneBlendEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_controlBytes":
            case "controlBytes":
            {
                if (instance.m_controlBytes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startVertexIndex":
            case "startVertexIndex":
            {
                if (instance.m_startVertexIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endVertexIndex":
            case "endVertexIndex":
            {
                if (instance.m_endVertexIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_partialWrite":
            case "partialWrite":
            {
                if (instance.m_partialWrite is not TGet castValue) return false;
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
            case "m_fourBlendEntries":
            case "fourBlendEntries":
            {
                if (value is not List<hclBoneSpaceDeformer.FourBlendEntryBlock> castValue) return false;
                instance.m_fourBlendEntries = castValue;
                return true;
            }
            case "m_threeBlendEntries":
            case "threeBlendEntries":
            {
                if (value is not List<hclBoneSpaceDeformer.ThreeBlendEntryBlock> castValue) return false;
                instance.m_threeBlendEntries = castValue;
                return true;
            }
            case "m_twoBlendEntries":
            case "twoBlendEntries":
            {
                if (value is not List<hclBoneSpaceDeformer.TwoBlendEntryBlock> castValue) return false;
                instance.m_twoBlendEntries = castValue;
                return true;
            }
            case "m_oneBlendEntries":
            case "oneBlendEntries":
            {
                if (value is not List<hclBoneSpaceDeformer.OneBlendEntryBlock> castValue) return false;
                instance.m_oneBlendEntries = castValue;
                return true;
            }
            case "m_controlBytes":
            case "controlBytes":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_controlBytes = castValue;
                return true;
            }
            case "m_startVertexIndex":
            case "startVertexIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_startVertexIndex = castValue;
                return true;
            }
            case "m_endVertexIndex":
            case "endVertexIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_endVertexIndex = castValue;
                return true;
            }
            case "m_partialWrite":
            case "partialWrite":
            {
                if (value is not bool castValue) return false;
                instance.m_partialWrite = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
