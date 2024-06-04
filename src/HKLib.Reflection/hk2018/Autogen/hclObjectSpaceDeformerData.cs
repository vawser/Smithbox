// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclObjectSpaceDeformerData : HavokData<hclObjectSpaceDeformer> 
{
    public hclObjectSpaceDeformerData(HavokType type, hclObjectSpaceDeformer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_eightBlendEntries":
            case "eightBlendEntries":
            {
                if (instance.m_eightBlendEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sevenBlendEntries":
            case "sevenBlendEntries":
            {
                if (instance.m_sevenBlendEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sixBlendEntries":
            case "sixBlendEntries":
            {
                if (instance.m_sixBlendEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fiveBlendEntries":
            case "fiveBlendEntries":
            {
                if (instance.m_fiveBlendEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_eightBlendEntries":
            case "eightBlendEntries":
            {
                if (value is not List<hclObjectSpaceDeformer.EightBlendEntryBlock> castValue) return false;
                instance.m_eightBlendEntries = castValue;
                return true;
            }
            case "m_sevenBlendEntries":
            case "sevenBlendEntries":
            {
                if (value is not List<hclObjectSpaceDeformer.SevenBlendEntryBlock> castValue) return false;
                instance.m_sevenBlendEntries = castValue;
                return true;
            }
            case "m_sixBlendEntries":
            case "sixBlendEntries":
            {
                if (value is not List<hclObjectSpaceDeformer.SixBlendEntryBlock> castValue) return false;
                instance.m_sixBlendEntries = castValue;
                return true;
            }
            case "m_fiveBlendEntries":
            case "fiveBlendEntries":
            {
                if (value is not List<hclObjectSpaceDeformer.FiveBlendEntryBlock> castValue) return false;
                instance.m_fiveBlendEntries = castValue;
                return true;
            }
            case "m_fourBlendEntries":
            case "fourBlendEntries":
            {
                if (value is not List<hclObjectSpaceDeformer.FourBlendEntryBlock> castValue) return false;
                instance.m_fourBlendEntries = castValue;
                return true;
            }
            case "m_threeBlendEntries":
            case "threeBlendEntries":
            {
                if (value is not List<hclObjectSpaceDeformer.ThreeBlendEntryBlock> castValue) return false;
                instance.m_threeBlendEntries = castValue;
                return true;
            }
            case "m_twoBlendEntries":
            case "twoBlendEntries":
            {
                if (value is not List<hclObjectSpaceDeformer.TwoBlendEntryBlock> castValue) return false;
                instance.m_twoBlendEntries = castValue;
                return true;
            }
            case "m_oneBlendEntries":
            case "oneBlendEntries":
            {
                if (value is not List<hclObjectSpaceDeformer.OneBlendEntryBlock> castValue) return false;
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
