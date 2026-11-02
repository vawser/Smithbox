// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpShapeViewerOptionsData : HavokData<HKLib.hk2019.hknpShapeViewer.Options> 
{
    public hknpShapeViewerOptionsData(HavokType type, HKLib.hk2019.hknpShapeViewer.Options instance) : base(type, instance) {}

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
            case "m_drawInteriorTriangles":
            case "drawInteriorTriangles":
            {
                if (instance.m_drawInteriorTriangles is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hknpShapeViewer.Options.LevelOfDetail castValue) return false;
                instance.m_levelOfDetail = castValue;
                return true;
            }
            case "m_convexRadiusDisplayMode":
            case "convexRadiusDisplayMode":
            {
                if (value is not HKLib.hk2019.hknpShapeViewer.Options.ConvexRadiusDisplayMode castValue) return false;
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
            case "m_drawInteriorTriangles":
            case "drawInteriorTriangles":
            {
                if (value is not bool castValue) return false;
                instance.m_drawInteriorTriangles = castValue;
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
