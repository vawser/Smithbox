// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpParticleCacheViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpParticleCacheViewer.Options>
{
    public hknpParticleCacheViewerOptionsData(Havok.hk2018.hknpParticleCacheViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collisionAgainstStatic":
            case "collisionAgainstStatic":
            {
                value = instance.m_collisionAgainstStatic;
                return true;
            }
            case "m_collisionAgainstDynamic":
            case "collisionAgainstDynamic":
            {
                value = instance.m_collisionAgainstDynamic;
                return true;
            }
            case "m_colorTriangles":
            case "colorTriangles":
            {
                value = instance.m_colorTriangles;
                return true;
            }
            case "m_neighbors":
            case "neighbors":
            {
                value = instance.m_neighbors;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                value = instance.m_domain;
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
            case "m_collisionAgainstStatic":
            case "collisionAgainstStatic":
            {
                if (value is not bool castValue) return false;
                instance.m_collisionAgainstStatic = castValue;
                return true;
            }
            case "m_collisionAgainstDynamic":
            case "collisionAgainstDynamic":
            {
                if (value is not bool castValue) return false;
                instance.m_collisionAgainstDynamic = castValue;
                return true;
            }
            case "m_colorTriangles":
            case "colorTriangles":
            {
                if (value is not bool castValue) return false;
                instance.m_colorTriangles = castValue;
                return true;
            }
            case "m_neighbors":
            case "neighbors":
            {
                if (value is not bool castValue) return false;
                instance.m_neighbors = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not bool castValue) return false;
                instance.m_domain = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
