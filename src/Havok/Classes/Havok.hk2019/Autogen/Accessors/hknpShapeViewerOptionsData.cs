// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpShapeViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpShapeViewer.Options>
{
    public hknpShapeViewerOptionsData(Havok.hk2019.hknpShapeViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_levelOfDetail":
            case "levelOfDetail":
            {
                value = instance.m_levelOfDetail;
                return true;
            }
            case "m_convexRadiusDisplayMode":
            case "convexRadiusDisplayMode":
            {
                value = instance.m_convexRadiusDisplayMode;
                return true;
            }
            case "m_drawEdges":
            case "drawEdges":
            {
                value = instance.m_drawEdges;
                return true;
            }
            case "m_drawInteriorTriangles":
            case "drawInteriorTriangles":
            {
                value = instance.m_drawInteriorTriangles;
                return true;
            }
            case "m_usePreIntegrationTransform":
            case "usePreIntegrationTransform":
            {
                value = instance.m_usePreIntegrationTransform;
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
            case "m_levelOfDetail":
            case "levelOfDetail":
            {
                if (value is not Havok.hk2019.hknpShapeViewer.Options.LevelOfDetail castValue) return false;
                instance.m_levelOfDetail = castValue;
                return true;
            }
            case "m_convexRadiusDisplayMode":
            case "convexRadiusDisplayMode":
            {
                if (value is not Havok.hk2019.hknpShapeViewer.Options.ConvexRadiusDisplayMode castValue) return false;
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
