// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCornerPredictorFaceTraversalData : HavokData<HKLib.hk2019.hkaiCornerPredictor.FaceTraversal> 
{
    public hkaiCornerPredictorFaceTraversalData(HavokType type, HKLib.hk2019.hkaiCornerPredictor.FaceTraversal instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entrancePointLocal":
            case "entrancePointLocal":
            {
                if (instance.m_entrancePointLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_distance":
            case "distance":
            {
                if (instance.m_distance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceDataPtr":
            case "faceDataPtr":
            {
                if (instance.m_faceDataPtr is null)
                {
                    return true;
                }
                if (instance.m_faceDataPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
