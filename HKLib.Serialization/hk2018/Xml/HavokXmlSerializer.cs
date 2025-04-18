using System.Xml.Linq;
using HKLib.hk2018;
using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Xml.FormatHandlers;

namespace HKLib.Serialization.hk2018.Xml;

public class HavokXmlSerializer : HavokSerializer
{
    public HavokXmlSerializer() : this(HavokTypeRegistry.Instance) { }
    public HavokXmlSerializer(HavokTypeRegistry typeRegistry) : base(typeRegistry) { }

    public override void LoadCompendium(Stream stream) { }

    public override void LoadCompendium(HavokCompendium compendium) { }

    public override HavokCompendium ReadCompendium(Stream stream)
    {
        throw new NotImplementedException();
    }

    public override void Write(HavokCompendium compendium, Stream stream)
    {
        throw new NotImplementedException();
    }

    public override IEnumerable<IHavokObject> ReadAllObjects(Stream stream)
    {
        XElement tagfile;
        try
        {
            tagfile = XElement.Load(stream);
        }
        catch (Exception e)
        {
            throw new InvalidDataException("The given stream does not contain valid xml data.", e);
        }

        if ((int?)tagfile.Attribute("version") is not 3)
        {
            throw new InvalidDataException("Invalid version, this serializer only supports xml tagfile version 3");
        }

        Dictionary<string, (XElement Type, HavokTypeBuilder Builder)> typeElements = new();
        List<XElement> objectElements = new();
        foreach (XElement element in tagfile.Elements())
        {
            switch (element.Name.ToString())
            {
                case "type":
                {
                    string id = element.Attribute("id")!.Value;
                    typeElements.Add(id, (element, new HavokTypeBuilder()));
                    break;
                }
                case "object":
                {
                    objectElements.Add(element);
                    break;
                }
            }
        }

        Dictionary<string, HavokType> types = ReadTypes(typeElements);

        List<ObjectItem> objectItems = ReadObjects(objectElements, types);

        XmlDeserializeContext context = new(objectItems.ToDictionary(x => x.Id!), types);

        return objectItems.Select(x => x.Object ?? x.ReadObject(context)).Where(x => x is IHavokObject)
            .Cast<IHavokObject>().ToList();
    }

    private Dictionary<string, HavokType> ReadTypes(
        Dictionary<string, (XElement Type, HavokTypeBuilder Builder)> typeElements)
    {
        foreach ((string id, (XElement type, HavokTypeBuilder builder)) in typeElements)
        {
            string name = type.Element("name")?.Attribute("value")?.Value ??
                          throw new InvalidDataException($"Type with id \"{id}\" has no name.");
            builder.WithName(name);

            if (type.Element("parameters") is not { } parameters) continue;
            foreach (XElement param in parameters.Elements())
            {
                switch (param.Name.ToString())
                {
                    case "typeparam":
                        string paramTypeId = param.Attribute("id")?.Value ??
                                             throw new InvalidDataException(
                                                 $"Encountered typeparam with missing id in type with id \"{id}\"");
                        if (!typeElements.TryGetValue(paramTypeId,
                                out (XElement Type, HavokTypeBuilder Builder) paramType))
                        {
                            throw new InvalidDataException(
                                $"Encountered reference to missing type with id \"{paramTypeId}\"");
                        }

                        builder.WithTemplateParameter(string.Empty, paramType.Builder);
                        break;
                    case "valueparam":
                        if ((uint?)param.Attribute("value") is not { } value)
                        {
                            throw new InvalidDataException(
                                $"Encountered valueparam with missing value in type with id \"{id}\"");
                        }

                        builder.WithTemplateParameter(string.Empty, (int)value);
                        break;
                    default:
                        throw new InvalidDataException(
                            $"Encountered unexpected parameter of type \"{param.Name}\" in type with id \"{id}\"");
                }
            }
        }

        Dictionary<string, HavokType> types = new();
        foreach ((string id, (_, HavokTypeBuilder builder)) in typeElements)
        {
            HavokType type = builder.Build();
            if (TypeRegistry.GetType(type.Identity) is not { } registryType)
            {
                throw new InvalidDataException(
                    $"No matching type found in the type registry for type with identity \"{type.Identity}\"");
            }

            types.Add(id, registryType);
        }

        return types;
    }

    private static List<ObjectItem> ReadObjects(List<XElement> objectElements, Dictionary<string, HavokType> types)
    {
        List<ObjectItem> objectItems = new();
        foreach (XElement objectElement in objectElements)
        {
            string objectId = objectElement.Attribute("id")!.Value;
            string typeId = objectElement.Attribute("typeid")?.Value ??
                            throw new InvalidDataException($"Object with id \"{objectId}\" has no typeid.");

            if (!types.TryGetValue(typeId, out HavokType? type))
            {
                throw new InvalidDataException($"Encountered reference to missing type with id \"{typeId}\"");
            }

            ObjectItem item = new(type, objectElement, objectId);
            objectItems.Add(item);
        }

        return objectItems;
    }

    public override void Write(IHavokObject havokObject, Stream stream)
    {
        XmlSerializeContext context = new(TypeRegistry);
        HavokType type = FormatHandler.GetActualType(havokObject, TypeRegistry);
        context.Enqueue(type, havokObject);

        XElement rootElement = new("hktagfile", new XAttribute("version", 3));
        while (context.ObjectQueue.Count > 0)
        {
            while (context.TypeQueue.Count > 0)
            {
                WriteType(rootElement, context.TypeQueue.Dequeue(), context);
            }

            ObjectItem item = context.ObjectQueue.Dequeue();
            item.WriteObject(rootElement, context);
        }

        rootElement.Save(stream);
        stream.Close();
    }

    private static void WriteType(XElement rootElement, HavokType type, XmlSerializeContext context)
    {
        string id = context.GetTypeId(type);
        XElement typeElement = new("type", new XAttribute("id", id),
            new XElement("name", new XAttribute("value", type.Name)));
        rootElement.Add(typeElement);

        if (type.Parent is { } parentType)
        {
            typeElement.Add(new XElement("parent", new XAttribute("id", context.GetTypeId(parentType))));
            typeElement.Add(new XComment($" {FormatHandler.GetXmlName(parentType)} "));
        }

        HavokType.Optional optionals = type.Optionals;
        if (optionals.HasFlag(HavokType.Optional.Format))
        {
            typeElement.Add(new XElement("format", new XAttribute("value", type.Format)));
            typeElement.Add(new XComment($" {type.Kind.ToString().ToLower()} "));
        }

        if (optionals.HasFlag(HavokType.Optional.SubType))
        {
            string subTypeId;
            string subTypeName;
            if (type.SubType is { } subType)
            {
                subTypeId = context.GetTypeId(subType);
                subTypeName = FormatHandler.GetXmlName(subType);
            }
            else
            {
                subTypeId = "type0";
                subTypeName = "null";
            }

            typeElement.Add(new XElement("subtype", new XAttribute("id", subTypeId)));
            typeElement.Add(new XComment($" {subTypeName} "));
        }

        if (optionals.HasFlag(HavokType.Optional.Version))
        {
            typeElement.Add(new XElement("version", new XAttribute("value", type.Version)));
        }

        if (type.TemplateParameters.Count > 0)
        {
            XElement parameters = new("parameters", new XAttribute("count", type.TemplateParameters.Count));
            foreach (HavokType.TemplateParameter parameter in type.TemplateParameters)
            {
                if (parameter.Type is { } typeParam)
                {
                    parameters.Add(new XElement("typeparam", new XAttribute("id", context.GetTypeId(typeParam))));
                    parameters.Add(new XComment($" {FormatHandler.GetXmlName(typeParam)} "));
                }
                else if (parameter.Value is { } valueParam)
                {
                    parameters.Add(new XElement("valueparam", new XAttribute("value", valueParam)));
                }
                else
                {
                    throw new InvalidDataException(
                        $"Invalid template parameter in type with identity \"{type.Identity}\".");
                }
            }

            typeElement.Add(parameters);
        }

        if (optionals.HasFlag(HavokType.Optional.Flags))
        {
            typeElement.Add(new XElement("flags", new XAttribute("value", (uint)type.Flags)));
        }

        if (optionals.HasFlag(HavokType.Optional.Members))
        {
            XElement fieldsElement = new("fields", "");

            int fieldCount = 0;
            int startIndex = type.Parent is not null ? type.Parent.Properties.Count : 0;
            for (int i = startIndex; i < type.Properties.Count; i++)
            {
                HavokType.Member property = type.Properties[i] ??
                                            throw new InvalidDataException("Unable to serialize null property");
                if (property.NonSerializable) continue;

                // flags are serialized as 32 even though they are 16 at runtime for properties
                XElement propertyElement = new("field", new XAttribute("name", property.Name),
                    new XAttribute("typeid", context.GetTypeId(property.Type)),
                    new XAttribute("flags", 32));

                fieldsElement.Add(propertyElement);
                fieldsElement.Add(new XComment($" {FormatHandler.GetXmlName(property.Type)} "));
                fieldCount++;
            }

            startIndex = type.Parent is not null ? type.Parent.Fields.Count : 0;
            for (int i = startIndex; i < type.Fields.Count; i++)
            {
                HavokType.Member field = type.Fields[i];
                if (field.NonSerializable) continue;

                XElement fieldElement = new("field", new XAttribute("name", field.Name),
                    new XAttribute("typeid", context.GetTypeId(field.Type)),
                    new XAttribute("flags", (uint)field.Flags));
                fieldsElement.Add(fieldElement);
                fieldsElement.Add(new XComment($" {FormatHandler.GetXmlName(field.Type)} "));
                fieldCount++;
            }

            fieldsElement.Add(new XAttribute("count", fieldCount));

            typeElement.Add(fieldsElement);
        }
    }
}