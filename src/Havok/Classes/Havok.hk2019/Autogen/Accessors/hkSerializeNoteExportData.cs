// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkSerializeNoteExportData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkSerialize.Note.Export>
{
    public hkSerializeNoteExportData(Havok.hk2019.hkSerialize.Note.Export instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
