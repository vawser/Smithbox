// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpDeactivationViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpDeactivationViewerOptionsData : HavokData<Options> 
{
    public hknpDeactivationViewerOptionsData(HavokType type, Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showAabbs":
            case "showAabbs":
            {
                if (instance.m_showAabbs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showActivationPriority":
            case "showActivationPriority":
            {
                if (instance.m_showActivationPriority is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showIslandConnections":
            case "showIslandConnections":
            {
                if (instance.m_showIslandConnections is not TGet castValue) return false;
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
            case "m_showAabbs":
            case "showAabbs":
            {
                if (value is not bool castValue) return false;
                instance.m_showAabbs = castValue;
                return true;
            }
            case "m_showActivationPriority":
            case "showActivationPriority":
            {
                if (value is not bool castValue) return false;
                instance.m_showActivationPriority = castValue;
                return true;
            }
            case "m_showIslandConnections":
            case "showIslandConnections":
            {
                if (value is not bool castValue) return false;
                instance.m_showIslandConnections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
