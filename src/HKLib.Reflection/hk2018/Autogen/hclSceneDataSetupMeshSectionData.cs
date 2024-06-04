// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSceneDataSetupMeshSectionData : HavokData<hclSceneDataSetupMeshSection> 
{
    public hclSceneDataSetupMeshSectionData(HavokType type, hclSceneDataSetupMeshSection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_setupMesh":
            case "setupMesh":
            {
                if (instance.m_setupMesh is null)
                {
                    return true;
                }
                if (instance.m_setupMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_meshSection":
            case "meshSection":
            {
                if (instance.m_meshSection is null)
                {
                    return true;
                }
                if (instance.m_meshSection is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_skinnedSection":
            case "skinnedSection":
            {
                if (instance.m_skinnedSection is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_setupMesh":
            case "setupMesh":
            {
                if (value is null)
                {
                    instance.m_setupMesh = default;
                    return true;
                }
                if (value is hclSceneDataSetupMesh castValue)
                {
                    instance.m_setupMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_meshSection":
            case "meshSection":
            {
                if (value is null)
                {
                    instance.m_meshSection = default;
                    return true;
                }
                if (value is hkxMeshSection castValue)
                {
                    instance.m_meshSection = castValue;
                    return true;
                }
                return false;
            }
            case "m_skinnedSection":
            case "skinnedSection":
            {
                if (value is not bool castValue) return false;
                instance.m_skinnedSection = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
