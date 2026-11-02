// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpParticlesShapeViewerOptionsData : HavokData<HKLib.hk2019.hknpParticlesShapeViewer.Options> 
{
    public hknpParticlesShapeViewerOptionsData(HavokType type, HKLib.hk2019.hknpParticlesShapeViewer.Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showParticleIndex":
            case "showParticleIndex":
            {
                if (instance.m_showParticleIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexToShow":
            case "indexToShow":
            {
                if (instance.m_indexToShow is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_displayDisabledParticles":
            case "displayDisabledParticles":
            {
                if (instance.m_displayDisabledParticles is not TGet castValue) return false;
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
            case "m_showParticleIndex":
            case "showParticleIndex":
            {
                if (value is not bool castValue) return false;
                instance.m_showParticleIndex = castValue;
                return true;
            }
            case "m_indexToShow":
            case "indexToShow":
            {
                if (value is not int castValue) return false;
                instance.m_indexToShow = castValue;
                return true;
            }
            case "m_displayDisabledParticles":
            case "displayDisabledParticles":
            {
                if (value is not bool castValue) return false;
                instance.m_displayDisabledParticles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
