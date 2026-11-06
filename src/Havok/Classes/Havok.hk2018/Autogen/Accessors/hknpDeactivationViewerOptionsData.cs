// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpDeactivationViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpDeactivationViewer.Options>
{
    public hknpDeactivationViewerOptionsData(Havok.hk2018.hknpDeactivationViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showAabbs":
            case "showAabbs":
            {
                value = instance.m_showAabbs;
                return true;
            }
            case "m_showActivationPriority":
            case "showActivationPriority":
            {
                value = instance.m_showActivationPriority;
                return true;
            }
            case "m_showIslandConnections":
            case "showIslandConnections":
            {
                value = instance.m_showIslandConnections;
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
