// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpParticleCacheViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpParticleCacheViewerOptionsData : HavokData<Options> 
{
    public hknpParticleCacheViewerOptionsData(HavokType type, Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collisionAgainstStatic":
            case "collisionAgainstStatic":
            {
                if (instance.m_collisionAgainstStatic is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionAgainstDynamic":
            case "collisionAgainstDynamic":
            {
                if (instance.m_collisionAgainstDynamic is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_colorTriangles":
            case "colorTriangles":
            {
                if (instance.m_colorTriangles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_neighbors":
            case "neighbors":
            {
                if (instance.m_neighbors is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (instance.m_domain is not TGet castValue) return false;
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
