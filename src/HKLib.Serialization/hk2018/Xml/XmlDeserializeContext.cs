using System.Xml.Linq;
using HKLib.hk2018;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Xml;

public class XmlDeserializeContext
{
    private readonly Dictionary<string, ObjectItem> _objects;
    private readonly Dictionary<string, HavokType> _types;

    public XmlDeserializeContext(Dictionary<string, ObjectItem> objects, Dictionary<string, HavokType> types)
    {
        _objects = objects;
        _types = types;
    }

    public object GetObject(string id)
    {
        if (id == "object0")
        {
            throw new ArgumentException("The requested object is null.", nameof(id));
        }

        if (!_objects.TryGetValue(id, out ObjectItem? objectItem))
        {
            throw new ArgumentException($"No object found with id {id}");
        }

        return objectItem.Object ?? objectItem.ReadObject(this);
    }


    public HavokType GetType(string typeId)
    {
        if (typeId == "type0")
        {
            throw new ArgumentException("The requested type is null.", nameof(typeId));
        }

        if (_types.TryGetValue(typeId, out HavokType? havokType))
        {
            return havokType;
        }

        throw new ArgumentException($"Cannot get type for id {typeId}.", nameof(typeId));
    }
}