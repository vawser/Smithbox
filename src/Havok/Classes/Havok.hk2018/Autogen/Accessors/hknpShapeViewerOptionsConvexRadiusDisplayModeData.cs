// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpShapeViewerOptionsConvexRadiusDisplayModeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpShapeViewer.Options.ConvexRadiusDisplayMode>
{
    public hknpShapeViewerOptionsConvexRadiusDisplayModeData(Havok.hk2018.hknpShapeViewer.Options.ConvexRadiusDisplayMode instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rounded":
            case "rounded":
            {
                value = instance.m_rounded;
                return true;
            }
            case "m_planar":
            case "planar":
            {
                value = instance.m_planar;
                return true;
            }
            case "m_none":
            case "none":
            {
                value = instance.m_none;
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
            case "m_rounded":
            case "rounded":
            {
                if (value is not bool castValue) return false;
                instance.m_rounded = castValue;
                return true;
            }
            case "m_planar":
            case "planar":
            {
                if (value is not bool castValue) return false;
                instance.m_planar = castValue;
                return true;
            }
            case "m_none":
            case "none":
            {
                if (value is not bool castValue) return false;
                instance.m_none = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
