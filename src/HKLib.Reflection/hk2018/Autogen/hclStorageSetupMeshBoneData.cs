// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStorageSetupMeshBoneData : HavokData<hclStorageSetupMesh.Bone> 
{
    public hclStorageSetupMeshBoneData(HavokType type, hclStorageSetupMesh.Bone instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneFromSkin":
            case "boneFromSkin":
            {
                if (instance.m_boneFromSkin is not TGet castValue) return false;
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
            case "m_boneFromSkin":
            case "boneFromSkin":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_boneFromSkin = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
