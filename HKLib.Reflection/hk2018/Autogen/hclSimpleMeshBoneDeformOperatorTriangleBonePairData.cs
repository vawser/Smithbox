// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimpleMeshBoneDeformOperatorTriangleBonePairData : HavokData<hclSimpleMeshBoneDeformOperator.TriangleBonePair> 
{
    public hclSimpleMeshBoneDeformOperatorTriangleBonePairData(HavokType type, hclSimpleMeshBoneDeformOperator.TriangleBonePair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneOffset":
            case "boneOffset":
            {
                if (instance.m_boneOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleOffset":
            case "triangleOffset":
            {
                if (instance.m_triangleOffset is not TGet castValue) return false;
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
