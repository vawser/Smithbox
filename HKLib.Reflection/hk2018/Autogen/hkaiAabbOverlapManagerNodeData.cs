// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiAabbOverlapManagerNodeData : HavokData<hkaiAabbOverlapManager.Node> 
{
    public hkaiAabbOverlapManagerNodeData(HavokType type, hkaiAabbOverlapManager.Node instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_curAabb":
            case "curAabb":
            {
                if (instance.m_curAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_newAabb":
            case "newAabb":
            {
                if (instance.m_newAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasNewAabb":
            case "hasNewAabb":
            {
                if (instance.m_hasNewAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_treeHandle":
            case "treeHandle":
            {
                if (instance.m_treeHandle is not TGet castValue) return false;
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
            case "m_curAabb":
            case "curAabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_curAabb = castValue;
                return true;
            }
            case "m_newAabb":
            case "newAabb":
            {
                if (value is not hkAabb castValue) return false;
                instance.m_newAabb = castValue;
                return true;
            }
            case "m_hasNewAabb":
            case "hasNewAabb":
            {
                if (value is not bool castValue) return false;
                instance.m_hasNewAabb = castValue;
                return true;
            }
            case "m_treeHandle":
            case "treeHandle":
            {
                if (value is not uint castValue) return false;
                instance.m_treeHandle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
