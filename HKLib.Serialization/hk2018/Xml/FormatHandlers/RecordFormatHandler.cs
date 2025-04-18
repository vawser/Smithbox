using System.Xml.Linq;
using HKLib.hk2018;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Xml.FormatHandlers;

public static class RecordFormatHandler
{
    public static object Read(XElement element, HavokType type, XmlDeserializeContext context)
    {
        HavokData data = HavokData.Instantiate(type) ??
                         throw new ArgumentException($"Failed to instantiate type {type.Identity}", nameof(type));

        Queue<XElement> items = new(element.Elements());

        foreach (HavokType.Member field in type.Fields)
        {
            if (field.NonSerializable) continue;

            if (!items.TryDequeue(out XElement? item))
            {
                throw new InvalidDataException(
                    $"Missing \"{field.Name}\" field for record of type \"{data.Type.Identity}\".");
            }

            if (item.Attribute("name")?.Value is not { } itemName)
            {
                throw new InvalidDataException("Missing \"name\" attribute for record field.");
            }

            if (itemName != field.Name)
            {
                throw new InvalidDataException(
                    $"Field name mismatch in object of type {type.Identity}. Expected: {field.Name} | Encountered: {itemName}.");
            }

            object fieldValue = FormatHandler.Read(item.Elements().Single(), field.Type, context);
            if (!data.TrySetField(field.Name, fieldValue))
            {
                throw new InvalidOperationException(
                    $"Unable to set field \"{field.Name}\" of type \"{field.Type.Identity}\" in record of type \"{data.Type.Identity}\".");
            }
        }

        return data.GetObject<IHavokObject>()!;
    }

    public static void Write(XElement parentElement, HavokType type, object? value, XmlSerializeContext context)
    {
        // non-nullable nested-structs which were not instantiated are instantiated here
        value ??= type.Instantiate();

        if (value is not IHavokObject havokObject)
        {
            throw new ArgumentException($"Value of type {value.GetType()} does not implement {nameof(IHavokObject)}",
                nameof(value));
        }

        HavokData data = HavokData.Of(havokObject, type)
                         ?? throw new ArgumentException(
                             $"Failed to get a HavokData representation for object of type {value.GetType()}",
                             nameof(value));

        string typeName = FormatHandler.GetXmlName(type);
        XElement element = new("record", new XComment($" {typeName} "));
        foreach (HavokType.Member field in type.Fields)
        {
            if (field.NonSerializable) continue;

            XElement xmlField = new("field", new XAttribute("name", field.Name));

            if (!data.TryGetField(field.Name, out object? fieldValue))
            {
                throw new InvalidOperationException(
                    $"Unable to get field \"{field.Name}\" of type \"{field.Type.Identity}\" in object of type \"{data.Type.Identity}\"");
            }

            FormatHandler.Write(xmlField, field.Type, fieldValue!, context);
            element.Add(xmlField);
        }

        parentElement.Add(element);
    }
}