// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiOverlapManagerOverlapData : HavokData<hkaiOverlapManager.Overlap> 
{
    public hkaiOverlapManagerOverlapData(HavokType type, hkaiOverlapManager.Overlap instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generator":
            case "generator":
            {
                if (instance.m_generator is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_section":
            case "section":
            {
                if (instance.m_section is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_generating":
            case "generating":
            {
                if (instance.m_generating is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_queryAabb":
            case "queryAabb":
            {
                if (instance.m_queryAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_context":
            case "context":
            {
                if (instance.m_context is not TGet castValue) return false;
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
            case "m_generator":
            case "generator":
            {
                if (value is not int castValue) return false;
                instance.m_generator = castValue;
                return true;
            }
            case "m_section":
            case "section":
            {
                if (value is not int castValue) return false;
                instance.m_section = castValue;
                return true;
            }
            case "m_generating":
            case "generating":
            {
                if (value is not bool castValue) return false;
                instance.m_generating = castValue;
                return true;
            }
            case "m_queryAabb":
            case "queryAabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_queryAabb = castValue;
                return true;
            }
            case "m_context":
            case "context":
            {
                if (value is not hkaiSilhouetteGeneratorSectionContext castValue) return false;
                instance.m_context = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
