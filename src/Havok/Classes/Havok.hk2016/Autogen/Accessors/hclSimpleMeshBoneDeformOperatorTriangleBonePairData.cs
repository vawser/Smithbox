// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclSimpleMeshBoneDeformOperatorTriangleBonePairData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclSimpleMeshBoneDeformOperator.TriangleBonePair>
{
    public hclSimpleMeshBoneDeformOperatorTriangleBonePairData(Havok.hk2016.hclSimpleMeshBoneDeformOperator.TriangleBonePair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneOffset":
            case "boneOffset":
            {
                value = instance.m_boneOffset;
                return true;
            }
            case "m_triangleOffset":
            case "triangleOffset":
            {
                value = instance.m_triangleOffset;
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
            case "m_boneOffset":
            case "boneOffset":
            {
                if (value is not ushort castValue) return false;
                instance.m_boneOffset = castValue;
                return true;
            }
            case "m_triangleOffset":
            case "triangleOffset":
            {
                if (value is not ushort castValue) return false;
                instance.m_triangleOffset = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
