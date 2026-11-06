// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclSceneDataSetupMeshSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclSceneDataSetupMeshSection>
{
    public hclSceneDataSetupMeshSectionData(Havok.hk2018.hclSceneDataSetupMeshSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_setupMesh":
            case "setupMesh":
            {
                value = instance.m_setupMesh;
                return true;
            }
            case "m_meshSection":
            case "meshSection":
            {
                value = instance.m_meshSection;
                return true;
            }
            case "m_skinnedSection":
            case "skinnedSection":
            {
                value = instance.m_skinnedSection;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2018.hclSceneDataSetupMesh castValue)
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
                if (value is Havok.hk2018.hkxMeshSection castValue)
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
