// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiCornerPredictorFaceTraversalData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiCornerPredictor.FaceTraversal>
{
    public hkaiCornerPredictorFaceTraversalData(Havok.hk2019.hkaiCornerPredictor.FaceTraversal instance) : base(instance) { }

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
            case "m_distance":
            case "distance":
            {
                value = instance.m_distance;
                return true;
            }
            case "m_faceDataPtr":
            case "faceDataPtr":
            {
                value = instance.m_faceDataPtr;
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
            case "m_distance":
            case "distance":
            {
                if (value is not float castValue) return false;
                instance.m_distance = castValue;
                return true;
            }
            case "m_faceDataPtr":
            case "faceDataPtr":
            {
                if (value is null)
                {
                    instance.m_faceDataPtr = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_faceDataPtr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
