// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclObjectSpaceDeformerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclObjectSpaceDeformer>
{
    public hclObjectSpaceDeformerData(Havok.hk2018.hclObjectSpaceDeformer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_eightBlendEntries":
            case "eightBlendEntries":
            {
                value = instance.m_eightBlendEntries;
                return true;
            }
            case "m_sevenBlendEntries":
            case "sevenBlendEntries":
            {
                value = instance.m_sevenBlendEntries;
                return true;
            }
            case "m_sixBlendEntries":
            case "sixBlendEntries":
            {
                value = instance.m_sixBlendEntries;
                return true;
            }
            case "m_fiveBlendEntries":
            case "fiveBlendEntries":
            {
                value = instance.m_fiveBlendEntries;
                return true;
            }
            case "m_fourBlendEntries":
            case "fourBlendEntries":
            {
                value = instance.m_fourBlendEntries;
                return true;
            }
            case "m_threeBlendEntries":
            case "threeBlendEntries":
            {
                value = instance.m_threeBlendEntries;
                return true;
            }
            case "m_twoBlendEntries":
            case "twoBlendEntries":
            {
                value = instance.m_twoBlendEntries;
                return true;
            }
            case "m_oneBlendEntries":
            case "oneBlendEntries":
            {
                value = instance.m_oneBlendEntries;
                return true;
            }
            case "m_controlBytes":
            case "controlBytes":
            {
                value = instance.m_controlBytes;
                return true;
            }
            case "m_startVertexIndex":
            case "startVertexIndex":
            {
                value = instance.m_startVertexIndex;
                return true;
            }
            case "m_endVertexIndex":
            case "endVertexIndex":
            {
                value = instance.m_endVertexIndex;
                return true;
            }
            case "m_partialWrite":
            case "partialWrite":
            {
                value = instance.m_partialWrite;
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
            case "m_eightBlendEntries":
            case "eightBlendEntries":
            {
                if (value is not List<Havok.hk2018.hclObjectSpaceDeformer.EightBlendEntryBlock> castValue) return false;
                instance.m_eightBlendEntries = castValue;
                return true;
            }
            case "m_sevenBlendEntries":
            case "sevenBlendEntries":
            {
                if (value is not List<Havok.hk2018.hclObjectSpaceDeformer.SevenBlendEntryBlock> castValue) return false;
                instance.m_sevenBlendEntries = castValue;
                return true;
            }
            case "m_sixBlendEntries":
            case "sixBlendEntries":
            {
                if (value is not List<Havok.hk2018.hclObjectSpaceDeformer.SixBlendEntryBlock> castValue) return false;
                instance.m_sixBlendEntries = castValue;
                return true;
            }
            case "m_fiveBlendEntries":
            case "fiveBlendEntries":
            {
                if (value is not List<Havok.hk2018.hclObjectSpaceDeformer.FiveBlendEntryBlock> castValue) return false;
                instance.m_fiveBlendEntries = castValue;
                return true;
            }
            case "m_fourBlendEntries":
            case "fourBlendEntries":
            {
                if (value is not List<Havok.hk2018.hclObjectSpaceDeformer.FourBlendEntryBlock> castValue) return false;
                instance.m_fourBlendEntries = castValue;
                return true;
            }
            case "m_threeBlendEntries":
            case "threeBlendEntries":
            {
                if (value is not List<Havok.hk2018.hclObjectSpaceDeformer.ThreeBlendEntryBlock> castValue) return false;
                instance.m_threeBlendEntries = castValue;
                return true;
            }
            case "m_twoBlendEntries":
            case "twoBlendEntries":
            {
                if (value is not List<Havok.hk2018.hclObjectSpaceDeformer.TwoBlendEntryBlock> castValue) return false;
                instance.m_twoBlendEntries = castValue;
                return true;
            }
            case "m_oneBlendEntries":
            case "oneBlendEntries":
            {
                if (value is not List<Havok.hk2018.hclObjectSpaceDeformer.OneBlendEntryBlock> castValue) return false;
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
