// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpParticleSpheresViewerOptionsData : HavokData<HKLib.hk2019.hknpParticleSpheresViewer.Options> 
{
    public hknpParticleSpheresViewerOptionsData(HavokType type, HKLib.hk2019.hknpParticleSpheresViewer.Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_inner":
            case "inner":
            {
                if (instance.m_inner is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outer":
            case "outer":
            {
                if (instance.m_outer is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleParticleCollision":
            case "particleParticleCollision":
            {
                if (instance.m_particleParticleCollision is not TGet castValue) return false;
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
