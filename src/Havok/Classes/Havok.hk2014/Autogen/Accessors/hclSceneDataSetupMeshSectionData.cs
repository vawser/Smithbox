// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSceneDataSetupMeshSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSceneDataSetupMeshSection>
{
    public hclSceneDataSetupMeshSectionData(Havok.hk2014.hclSceneDataSetupMeshSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_setupMesh":
            case "setupMesh":
            {
                if (value is null)
                {
                    instance.m_setupMesh = default;
                    return true;
                }
                if (value is Havok.hk2014.hclSceneDataSetupMesh castValue)
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
                if (value is Havok.hk2014.hkxMeshSection castValue)
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
