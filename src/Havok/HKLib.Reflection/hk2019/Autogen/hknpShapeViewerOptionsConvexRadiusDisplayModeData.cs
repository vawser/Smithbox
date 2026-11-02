// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpShapeViewerOptionsConvexRadiusDisplayModeData : HavokData<HKLib.hk2019.hknpShapeViewer.Options.ConvexRadiusDisplayMode> 
{
    public hknpShapeViewerOptionsConvexRadiusDisplayModeData(HavokType type, HKLib.hk2019.hknpShapeViewer.Options.ConvexRadiusDisplayMode instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rounded":
            case "rounded":
            {
                if (instance.m_rounded is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_planar":
            case "planar":
            {
                if (instance.m_planar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_none":
            case "none":
            {
                if (instance.m_none is not TGet castValue) return false;
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
