// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiDirectedGraphInstanceData : HavokData<HKLib.hk2019.hkaiDirectedGraphInstance> 
{
    public hkaiDirectedGraphInstanceData(HavokType type, HKLib.hk2019.hkaiDirectedGraphInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                if (instance.m_instanceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_used":
            case "used":
            {
                if (instance.m_used is not TGet castValue) return false;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                if (value is not HKLib.hk2019.hkaiCopyOnWritePtr<HKLib.hk2019.hkaiDirectedGraphInstanceData> castValue) return false;
                instance.m_instanceData = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_used":
            case "used":
            {
                if (value is not bool castValue) return false;
                instance.m_used = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
