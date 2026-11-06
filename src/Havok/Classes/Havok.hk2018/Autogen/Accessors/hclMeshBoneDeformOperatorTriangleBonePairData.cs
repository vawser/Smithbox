// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclMeshBoneDeformOperatorTriangleBonePairData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclMeshBoneDeformOperator.TriangleBonePair>
{
    public hclMeshBoneDeformOperatorTriangleBonePairData(Havok.hk2018.hclMeshBoneDeformOperator.TriangleBonePair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localBoneTransform":
            case "localBoneTransform":
            {
                value = instance.m_localBoneTransform;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_triangleIndex":
            case "triangleIndex":
            {
                value = instance.m_triangleIndex;
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
            case "m_localBoneTransform":
            case "localBoneTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_localBoneTransform = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_triangleIndex":
            case "triangleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_triangleIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
