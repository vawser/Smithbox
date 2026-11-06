// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclObjectSpaceDeformerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclObjectSpaceDeformer>
{
    public hclObjectSpaceDeformerData(Havok.hk2014.hclObjectSpaceDeformer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_batchSizeSpu":
            case "batchSizeSpu":
            {
                value = instance.m_batchSizeSpu;
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
            case "m_fourBlendEntries":
            case "fourBlendEntries":
            {
                if (value is not List<Havok.hk2014.hclObjectSpaceDeformer.FourBlendEntryBlock> castValue) return false;
                instance.m_fourBlendEntries = castValue;
                return true;
            }
            case "m_threeBlendEntries":
            case "threeBlendEntries":
            {
                if (value is not List<Havok.hk2014.hclObjectSpaceDeformer.ThreeBlendEntryBlock> castValue) return false;
                instance.m_threeBlendEntries = castValue;
                return true;
            }
            case "m_twoBlendEntries":
            case "twoBlendEntries":
            {
                if (value is not List<Havok.hk2014.hclObjectSpaceDeformer.TwoBlendEntryBlock> castValue) return false;
                instance.m_twoBlendEntries = castValue;
                return true;
            }
            case "m_oneBlendEntries":
            case "oneBlendEntries":
            {
                if (value is not List<Havok.hk2014.hclObjectSpaceDeformer.OneBlendEntryBlock> castValue) return false;
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
            case "m_batchSizeSpu":
            case "batchSizeSpu":
            {
                if (value is not ushort castValue) return false;
                instance.m_batchSizeSpu = castValue;
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
