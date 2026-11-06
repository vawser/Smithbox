// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiPhysicsBodySilhouetteGeneratorBaseData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiPhysicsBodySilhouetteGeneratorBase>
{
    public hkaiPhysicsBodySilhouetteGeneratorBaseData(Havok.hk2014.hkaiPhysicsBodySilhouetteGeneratorBase instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_lazyRecomputeDisplacementThreshold":
            case "lazyRecomputeDisplacementThreshold":
            {
                value = instance.m_lazyRecomputeDisplacementThreshold;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_forceGenerateOntoPpu":
            case "forceGenerateOntoPpu":
            {
                value = instance.m_forceGenerateOntoPpu;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                value = instance.m_materialId;
                return true;
            }
            case "m_cachedSilhouettes":
            case "cachedSilhouettes":
            {
                value = instance.m_cachedSilhouettes;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_localAabb":
            case "localAabb":
            {
                value = instance.m_localAabb;
                return true;
            }
            case "m_localPoints":
            case "localPoints":
            {
                value = instance.m_localPoints;
                return true;
            }
            case "m_silhouetteSizes":
            case "silhouetteSizes":
            {
                value = instance.m_silhouetteSizes;
                return true;
            }
            case "m_weldTolerance":
            case "weldTolerance":
            {
                value = instance.m_weldTolerance;
                return true;
            }
            case "m_silhouetteDetailLevel":
            case "silhouetteDetailLevel":
            {
                value = instance.m_silhouetteDetailLevel;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_localPointsChanged":
            case "localPointsChanged":
            {
                value = instance.m_localPointsChanged;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                value = instance.m_isEnabled;
                return true;
            }
            case "m_linearVelocityAndThreshold":
            case "linearVelocityAndThreshold":
            {
                value = instance.m_linearVelocityAndThreshold;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_lazyRecomputeDisplacementThreshold":
            case "lazyRecomputeDisplacementThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_lazyRecomputeDisplacementThreshold = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkaiSilhouetteGenerator.GeneratorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2014.hkaiSilhouetteGenerator.GeneratorType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_forceGenerateOntoPpu":
            case "forceGenerateOntoPpu":
            {
                if (value is not byte castValue) return false;
                instance.m_forceGenerateOntoPpu = castValue;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                if (value is not int castValue) return false;
                instance.m_materialId = castValue;
                return true;
            }
            case "m_cachedSilhouettes":
            case "cachedSilhouettes":
            {
                if (value is null)
                {
                    instance.m_cachedSilhouettes = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiConvexSilhouetteSet castValue)
                {
                    instance.m_cachedSilhouettes = castValue;
                    return true;
                }
                return false;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Havok.hk2014.hkQTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_localAabb":
            case "localAabb":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_localAabb = castValue;
                return true;
            }
            case "m_localPoints":
            case "localPoints":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_localPoints = castValue;
                return true;
            }
            case "m_silhouetteSizes":
            case "silhouetteSizes":
            {
                if (value is not List<int> castValue) return false;
                instance.m_silhouetteSizes = castValue;
                return true;
            }
            case "m_weldTolerance":
            case "weldTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_weldTolerance = castValue;
                return true;
            }
            case "m_silhouetteDetailLevel":
            case "silhouetteDetailLevel":
            {
                if (value is Havok.hk2014.hkaiPointCloudSilhouetteGenerator.DetailLevel castValue)
                {
                    instance.m_silhouetteDetailLevel = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_silhouetteDetailLevel = (Havok.hk2014.hkaiPointCloudSilhouetteGenerator.DetailLevel)byteValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2014.hkaiPointCloudSilhouetteGenerator.PointCloudFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2014.hkaiPointCloudSilhouetteGenerator.PointCloudFlagBits)byteValue;
                    return true;
                }
                return false;
            }
            case "m_localPointsChanged":
            case "localPointsChanged":
            {
                if (value is not bool castValue) return false;
                instance.m_localPointsChanged = castValue;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            case "m_linearVelocityAndThreshold":
            case "linearVelocityAndThreshold":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linearVelocityAndThreshold = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
