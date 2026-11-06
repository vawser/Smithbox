// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpParticleSpheresViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpParticleSpheresViewer.Options>
{
    public hknpParticleSpheresViewerOptionsData(Havok.hk2019.hknpParticleSpheresViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_inner":
            case "inner":
            {
                value = instance.m_inner;
                return true;
            }
            case "m_outer":
            case "outer":
            {
                value = instance.m_outer;
                return true;
            }
            case "m_particleParticleCollision":
            case "particleParticleCollision":
            {
                value = instance.m_particleParticleCollision;
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
            case "m_inner":
            case "inner":
            {
                if (value is not bool castValue) return false;
                instance.m_inner = castValue;
                return true;
            }
            case "m_outer":
            case "outer":
            {
                if (value is not bool castValue) return false;
                instance.m_outer = castValue;
                return true;
            }
            case "m_particleParticleCollision":
            case "particleParticleCollision":
            {
                if (value is not bool castValue) return false;
                instance.m_particleParticleCollision = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
