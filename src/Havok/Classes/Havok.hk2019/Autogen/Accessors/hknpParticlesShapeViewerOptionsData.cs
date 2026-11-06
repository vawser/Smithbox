// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpParticlesShapeViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpParticlesShapeViewer.Options>
{
    public hknpParticlesShapeViewerOptionsData(Havok.hk2019.hknpParticlesShapeViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showParticleIndex":
            case "showParticleIndex":
            {
                value = instance.m_showParticleIndex;
                return true;
            }
            case "m_indexToShow":
            case "indexToShow":
            {
                value = instance.m_indexToShow;
                return true;
            }
            case "m_displayDisabledParticles":
            case "displayDisabledParticles":
            {
                value = instance.m_displayDisabledParticles;
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
