// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiCornerPredictorUserEdgeTraversalData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiCornerPredictor.UserEdgeTraversal>
{
    public hkaiCornerPredictorUserEdgeTraversalData(Havok.hk2018.hkaiCornerPredictor.UserEdgeTraversal instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entrancePointLocal":
            case "entrancePointLocal":
            {
                value = instance.m_entrancePointLocal;
                return true;
            }
            case "m_edgeDataPtr":
            case "edgeDataPtr":
            {
                value = instance.m_edgeDataPtr;
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
            case "m_entrancePointLocal":
            case "entrancePointLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_entrancePointLocal = castValue;
                return true;
            }
            case "m_edgeDataPtr":
            case "edgeDataPtr":
            {
                if (value is null)
                {
                    instance.m_edgeDataPtr = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_edgeDataPtr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
