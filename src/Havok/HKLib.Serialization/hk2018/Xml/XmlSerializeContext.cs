using HKLib.hk2018;
using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Xml;

public class XmlSerializeContext
{
    private int _currentObjectId = 1;
    private int _currentTypeId = 1;
    private Dictionary<object, string> _ids = new();
    private Dictionary<HavokType, string> _typeIds = new();

    public XmlSerializeContext(HavokTypeRegistry typeRegistry)
    {
        TypeRegistry = typeRegistry;
    }

    public Queue<ObjectItem> ObjectQueue { get; } = new();
    public Queue<HavokType> TypeQueue { get; } = new();

    public HavokTypeRegistry TypeRegistry { get; }

    public string Enqueue(HavokType type, object havokObject)
    {
        if (!_typeIds.ContainsKey(type)) EnqueueTypes(type);
        if (_ids.TryGetValue(havokObject, out string? id))
        {
            return id;
        }

        id = $"object{_currentObjectId}";
        _currentObjectId++;
        _ids.Add(havokObject, id);

        ObjectQueue.Enqueue(new ObjectItem(type, havokObject, id));

        return id;
    }

    public string GetTypeId(HavokType type)
    {
        if (_typeIds.TryGetValue(type, out string? typeId))
        {
            return typeId;
        }

        throw new ArgumentException($"Type {type} was not added to the queue.", nameof(type));
    }

    public void EnqueueTypes(HavokType type)
    {
        Queue<HavokType> typeQueue = new();
        typeQueue.Enqueue(type);
        while (typeQueue.Count > 0)
        {
            HavokType currentType = typeQueue.Dequeue();
            if (_typeIds.ContainsKey(currentType)) continue;

            TypeQueue.Enqueue(currentType);
            _typeIds.Add(currentType, $"type{_currentTypeId}");
            _currentTypeId++;

            if (currentType.Kind == HavokType.TypeKind.String)
            {
                HavokType charType = TypeRegistry.GetType("char") ??
                                     throw new InvalidOperationException("No \"char\" type found in type registry.");
                typeQueue.Enqueue(charType);
            }

            if (currentType.Parent is { } parentType) typeQueue.Enqueue(parentType);
            if (currentType.SubType is { } subType) typeQueue.Enqueue(subType);

            foreach (HavokType.TemplateParameter parameter in currentType.TemplateParameters)
            {
                if (parameter.Type is { } paramType) typeQueue.Enqueue(paramType);
            }

            int startIndex = currentType.Parent?.Properties.Count ?? 0;
            for (int i = startIndex; i < currentType.Properties.Count; i++)
            {
                HavokType.Member? property = currentType.Properties[i];
                if (property?.NonSerializable is not false) continue;
                typeQueue.Enqueue(property.Type);
            }

            startIndex = currentType.Parent?.Fields.Count ?? 0;
            for (int i = startIndex; i < currentType.Fields.Count; i++)
            {
                HavokType.Member field = currentType.Fields[i];
                if (field.NonSerializable) continue;
                typeQueue.Enqueue(field.Type);
            }

            foreach (HavokType.Interface typeInterface in currentType.Interfaces)
            {
                typeQueue.Enqueue(typeInterface.Type);
            }
        }
    }
}