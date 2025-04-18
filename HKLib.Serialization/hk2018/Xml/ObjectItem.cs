using System.Xml.Linq;
using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Xml.FormatHandlers;

namespace HKLib.Serialization.hk2018.Xml;

public class ObjectItem
{
    public ObjectItem(HavokType type, XElement xmlObject, string id)
    {
        Type = type;
        XmlObject = xmlObject;
        Id = id;
    }

    public ObjectItem(HavokType type, object obj, string id)
    {
        Type = type;
        Object = obj;
        Id = id;
    }

    public XElement? XmlObject { get; set; }

    public string? Id { get; }

    public object? Object { get; set; }

    public HavokType Type { get; }

    public object ReadObject(XmlDeserializeContext context)
    {
        if (XmlObject is null)
        {
            throw new InvalidOperationException("Object cannot be read as the corresponding XmlObject was not set");
        }

        if (XmlObject.Elements().SingleOrDefault() is not { } objectValue)
        {
            throw new InvalidDataException("The provided xml object contains no value.");
        }

        Object = FormatHandler.Read(objectValue, Type, context);
        return Object;
    }

    public void WriteObject(XElement parentElement, XmlSerializeContext context)
    {
        if (Object is null)
        {
            throw new InvalidOperationException("Object cannot be written as it is null.");
        }

        if (Id is null)
        {
            throw new InvalidOperationException("Object cannot be written as it has no id.");
        }

        XElement objElement = new("object", new XAttribute("id", Id),
            new XAttribute("typeid", context.GetTypeId(Type)));
        objElement.Add(new XComment($" {FormatHandler.GetXmlName(Type)} "));
        FormatHandler.Write(objElement, Type, Object, context);
        parentElement.Add(objElement);
    }
}