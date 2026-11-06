// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclObjectSpaceSkinPOperatorData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclObjectSpaceSkinPOperator>
{
    public hclObjectSpaceSkinPOperatorData(Havok.hk2014.hclObjectSpaceSkinPOperator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_boneFromSkinMeshTransforms":
            case "boneFromSkinMeshTransforms":
            {
                value = instance.m_boneFromSkinMeshTransforms;
                return true;
            }
            case "m_transformSubset":
            case "transformSubset":
            {
                value = instance.m_transformSubset;
                return true;
            }
            case "m_outputBufferIndex":
            case "outputBufferIndex":
            {
                value = instance.m_outputBufferIndex;
                return true;
            }
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                value = instance.m_transformSetIndex;
                return true;
            }
            case "m_objectSpaceDeformer":
            case "objectSpaceDeformer":
            {
                value = instance.m_objectSpaceDeformer;
                return true;
            }
            case "m_localPs":
            case "localPs":
            {
                value = instance.m_localPs;
                return true;
            }
            case "m_localUnpackedPs":
            case "localUnpackedPs":
            {
                value = instance.m_localUnpackedPs;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneFromSkinMeshTransforms":
            case "boneFromSkinMeshTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_boneFromSkinMeshTransforms = castValue;
                return true;
            }
            case "m_transformSubset":
            case "transformSubset":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_transformSubset = castValue;
                return true;
            }
            case "m_outputBufferIndex":
            case "outputBufferIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_outputBufferIndex = castValue;
                return true;
            }
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_transformSetIndex = castValue;
                return true;
            }
            case "m_objectSpaceDeformer":
            case "objectSpaceDeformer":
            {
                if (value is not Havok.hk2014.hclObjectSpaceDeformer castValue) return false;
                instance.m_objectSpaceDeformer = castValue;
                return true;
            }
            case "m_localPs":
            case "localPs":
            {
                if (value is not List<Havok.hk2014.hclObjectSpaceDeformer.LocalBlockP> castValue) return false;
                instance.m_localPs = castValue;
                return true;
            }
            case "m_localUnpackedPs":
            case "localUnpackedPs":
            {
                if (value is not List<Havok.hk2014.hclObjectSpaceDeformer.LocalBlockUnpackedP> castValue) return false;
                instance.m_localUnpackedPs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
