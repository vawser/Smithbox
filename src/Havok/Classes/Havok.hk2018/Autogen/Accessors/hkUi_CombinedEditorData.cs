// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkUi_CombinedEditorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.Ui_CombinedEditor>
{
    public hkUi_CombinedEditorData(Havok.hk2018.hk.Ui_CombinedEditor instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_editorTypeName":
            case "editorTypeName":
            {
                value = instance.m_editorTypeName;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_editorTypeName":
            case "editorTypeName":
            {
                if (value is null)
                {
                    instance.m_editorTypeName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_editorTypeName = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
