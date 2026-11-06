// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiViewerNodesObstacleGeneratorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiViewerNodes.ObstacleGenerator>
{
    public hkaiViewerNodesObstacleGeneratorData(Havok.hk2019.hkaiViewerNodes.ObstacleGenerator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_index":
            case "index":
            {
                value = instance.m_index;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
            case "m_index":
            case "index":
            {
                if (value is not int castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
