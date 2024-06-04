// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclMeshBoneDeformOperatorTriangleBonePairData : HavokData<hclMeshBoneDeformOperator.TriangleBonePair> 
{
    public hclMeshBoneDeformOperatorTriangleBonePairData(HavokType type, hclMeshBoneDeformOperator.TriangleBonePair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localBoneTransform":
            case "localBoneTransform":
            {
                if (instance.m_localBoneTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                if (instance.m_weight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleIndex":
            case "triangleIndex":
            {
                if (instance.m_triangleIndex is not TGet castValue) return false;
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
