// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkUi_CombinedEditorData : HavokData<Ui_CombinedEditor> 
{
    public hkUi_CombinedEditorData(HavokType type, Ui_CombinedEditor instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_editorTypeName":
            case "editorTypeName":
            {
                if (instance.m_editorTypeName is null)
                {
                    return true;
                }
                if (instance.m_editorTypeName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
