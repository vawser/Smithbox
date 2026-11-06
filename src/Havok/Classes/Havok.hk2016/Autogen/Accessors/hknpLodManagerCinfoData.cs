// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpLodManagerCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpLodManagerCinfo>
{
    public hknpLodManagerCinfoData(Havok.hk2016.hknpLodManagerCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_registerDefaultConfig":
            case "registerDefaultConfig":
            {
                value = instance.m_registerDefaultConfig;
                return true;
            }
            case "m_autoBuildLodOnDynamicBodyAdded":
            case "autoBuildLodOnDynamicBodyAdded":
            {
                value = instance.m_autoBuildLodOnDynamicBodyAdded;
                return true;
            }
            case "m_autoBuildLodOnMeshBodyAdded":
            case "autoBuildLodOnMeshBodyAdded":
            {
                value = instance.m_autoBuildLodOnMeshBodyAdded;
                return true;
            }
            case "m_lodAccuray":
            case "lodAccuray":
            {
                value = instance.m_lodAccuray;
                return true;
            }
            case "m_slowToFastThreshold":
            case "slowToFastThreshold":
            {
                value = instance.m_slowToFastThreshold;
                return true;
            }
            case "m_fastToSlowThreshold":
            case "fastToSlowThreshold":
            {
                value = instance.m_fastToSlowThreshold;
                return true;
            }
            case "m_bodyIsBigThreshold":
            case "bodyIsBigThreshold":
            {
                value = instance.m_bodyIsBigThreshold;
                return true;
            }
            case "m_avgVelocityGain":
            case "avgVelocityGain":
            {
                value = instance.m_avgVelocityGain;
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
            case "m_registerDefaultConfig":
            case "registerDefaultConfig":
            {
                if (value is not bool castValue) return false;
                instance.m_registerDefaultConfig = castValue;
                return true;
            }
            case "m_autoBuildLodOnDynamicBodyAdded":
            case "autoBuildLodOnDynamicBodyAdded":
            {
                if (value is not bool castValue) return false;
                instance.m_autoBuildLodOnDynamicBodyAdded = castValue;
                return true;
            }
            case "m_autoBuildLodOnMeshBodyAdded":
            case "autoBuildLodOnMeshBodyAdded":
            {
                if (value is not bool castValue) return false;
                instance.m_autoBuildLodOnMeshBodyAdded = castValue;
                return true;
            }
            case "m_lodAccuray":
            case "lodAccuray":
            {
                if (value is not float castValue) return false;
                instance.m_lodAccuray = castValue;
                return true;
            }
            case "m_slowToFastThreshold":
            case "slowToFastThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_slowToFastThreshold = castValue;
                return true;
            }
            case "m_fastToSlowThreshold":
            case "fastToSlowThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_fastToSlowThreshold = castValue;
                return true;
            }
            case "m_bodyIsBigThreshold":
            case "bodyIsBigThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_bodyIsBigThreshold = castValue;
                return true;
            }
            case "m_avgVelocityGain":
            case "avgVelocityGain":
            {
                if (value is not float castValue) return false;
                instance.m_avgVelocityGain = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
