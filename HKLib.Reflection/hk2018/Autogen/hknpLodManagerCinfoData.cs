// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpLodManagerCinfoData : HavokData<hknpLodManagerCinfo> 
{
    public hknpLodManagerCinfoData(HavokType type, hknpLodManagerCinfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_registerDefaultConfig":
            case "registerDefaultConfig":
            {
                if (instance.m_registerDefaultConfig is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_autoBuildLodOnDynamicBodyAdded":
            case "autoBuildLodOnDynamicBodyAdded":
            {
                if (instance.m_autoBuildLodOnDynamicBodyAdded is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_autoBuildLodOnMeshBodyAdded":
            case "autoBuildLodOnMeshBodyAdded":
            {
                if (instance.m_autoBuildLodOnMeshBodyAdded is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lodAccuray":
            case "lodAccuray":
            {
                if (instance.m_lodAccuray is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_slowToFastThreshold":
            case "slowToFastThreshold":
            {
                if (instance.m_slowToFastThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fastToSlowThreshold":
            case "fastToSlowThreshold":
            {
                if (instance.m_fastToSlowThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyIsBigThreshold":
            case "bodyIsBigThreshold":
            {
                if (instance.m_bodyIsBigThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_avgVelocityGain":
            case "avgVelocityGain":
            {
                if (instance.m_avgVelocityGain is not TGet castValue) return false;
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
