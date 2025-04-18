// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpShapeViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpShapeViewerOptionsData : HavokData<Options> 
{
    public hknpShapeViewerOptionsData(HavokType type, Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_levelOfDetail":
            case "levelOfDetail":
            {
                if (instance.m_levelOfDetail is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_convexRadiusDisplayMode":
            case "convexRadiusDisplayMode":
            {
                if (instance.m_convexRadiusDisplayMode is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drawEdges":
            case "drawEdges":
            {
                if (instance.m_drawEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_usePreIntegrationTransform":
            case "usePreIntegrationTransform":
            {
                if (instance.m_usePreIntegrationTransform is not TGet castValue) return false;
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
            case "m_levelOfDetail":
            case "levelOfDetail":
            {
                if (value is not Options.LevelOfDetail castValue) return false;
                instance.m_levelOfDetail = castValue;
                return true;
            }
            case "m_convexRadiusDisplayMode":
            case "convexRadiusDisplayMode":
            {
                if (value is not Options.ConvexRadiusDisplayMode castValue) return false;
                instance.m_convexRadiusDisplayMode = castValue;
                return true;
            }
            case "m_drawEdges":
            case "drawEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_drawEdges = castValue;
                return true;
            }
            case "m_usePreIntegrationTransform":
            case "usePreIntegrationTransform":
            {
                if (value is not bool castValue) return false;
                instance.m_usePreIntegrationTransform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
