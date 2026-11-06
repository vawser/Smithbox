// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiAabbOverlapManagerNodeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiAabbOverlapManager.Node>
{
    public hkaiAabbOverlapManagerNodeData(Havok.hk2018.hkaiAabbOverlapManager.Node instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_curAabb":
            case "curAabb":
            {
                value = instance.m_curAabb;
                return true;
            }
            case "m_newAabb":
            case "newAabb":
            {
                value = instance.m_newAabb;
                return true;
            }
            case "m_hasNewAabb":
            case "hasNewAabb":
            {
                value = instance.m_hasNewAabb;
                return true;
            }
            case "m_treeHandle":
            case "treeHandle":
            {
                value = instance.m_treeHandle;
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
            case "m_curAabb":
            case "curAabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_curAabb = castValue;
                return true;
            }
            case "m_newAabb":
            case "newAabb":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
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
