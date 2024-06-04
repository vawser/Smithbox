// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiPhysicsBodySilhouetteGeneratorBaseData : HavokData<hkaiPhysicsBodySilhouetteGeneratorBase> 
{
    public hkaiPhysicsBodySilhouetteGeneratorBaseData(HavokType type, hkaiPhysicsBodySilhouetteGeneratorBase instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lazyRecomputeDisplacementThreshold":
            case "lazyRecomputeDisplacementThreshold":
            {
                if (instance.m_lazyRecomputeDisplacementThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialId":
            case "materialId":
            {
                if (instance.m_materialId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cachedSilhouettes":
            case "cachedSilhouettes":
            {
                if (instance.m_cachedSilhouettes is null)
                {
                    return true;
                }
                if (instance.m_cachedSilhouettes is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localAabb":
            case "localAabb":
            {
                if (instance.m_localAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localPoints":
            case "localPoints":
            {
                if (instance.m_localPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_silhouetteSizes":
            case "silhouetteSizes":
            {
                if (instance.m_silhouetteSizes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weldTolerance":
            case "weldTolerance":
            {
                if (instance.m_weldTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_silhouetteDetailLevel":
            case "silhouetteDetailLevel":
            {
                if (instance.m_silhouetteDetailLevel is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_silhouetteDetailLevel is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_flags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_localPointsChanged":
            case "localPointsChanged":
            {
                if (instance.m_localPointsChanged is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isEnabledChanged":
            case "isEnabledChanged":
            {
                if (instance.m_isEnabledChanged is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (instance.m_isEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearVelocityAndThreshold":
            case "linearVelocityAndThreshold":
            {
                if (instance.m_linearVelocityAndThreshold is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is hkaiConvexSilhouetteSet castValue)
                {
                    instance.m_cachedSilhouettes = castValue;
                    return true;
                }
                return false;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not hkQTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_localAabb":
            case "localAabb":
            {
                if (value is not hkAabb castValue) return false;
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
                if (value is hkaiPointCloudSilhouetteGenerator.DetailLevel castValue)
                {
                    instance.m_silhouetteDetailLevel = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_silhouetteDetailLevel = (hkaiPointCloudSilhouetteGenerator.DetailLevel)byteValue;
                    return true;
                }
                return false;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hkaiPointCloudSilhouetteGenerator.PointCloudFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (hkaiPointCloudSilhouetteGenerator.PointCloudFlagBits)byteValue;
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
            case "m_isEnabledChanged":
            case "isEnabledChanged":
            {
                if (value is not bool castValue) return false;
                instance.m_isEnabledChanged = castValue;
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
