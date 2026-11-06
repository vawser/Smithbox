// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpConstraintViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpConstraintViewer.Options>
{
    public hknpConstraintViewerOptionsData(Havok.hk2019.hknpConstraintViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_displaySize":
            case "displaySize":
            {
                value = instance.m_displaySize;
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
            case "m_displaySize":
            case "displaySize":
            {
                if (value is not float castValue) return false;
                instance.m_displaySize = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
