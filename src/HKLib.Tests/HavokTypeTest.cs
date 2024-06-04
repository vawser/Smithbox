using System.Reflection;
using HKLib.Reflection.hk2018;

namespace HKLib.Tests;

public class HavokTypeTest
{
    private static HavokTypeRegistry? _registry;

    private static HavokTypeRegistry GetRegistry()
    {
        _registry ??= HavokTypeRegistry.Instance;
        return _registry;
    }

    private static IEnumerable<HavokType> ImplementedHavokTypes() =>
        GetRegistry().Types.Where(x => x.Type is not null);

    // Certain record types which are non-serializable are not implemented
    private static IEnumerable<HavokType> ImplementedHavokRecordTypes() =>
        ImplementedHavokTypes().Where(x => x.Kind == HavokType.TypeKind.Record);

    private static IEnumerable<object[]> ImplementedHavokTypeFields() => ImplementedHavokRecordTypes()
        .Where(x => !x.Type!.IsInterface).SelectMany(type =>
        {
            object instance = Activator.CreateInstance(type.Type!)!;
            return type.Fields.Where(x => !x.NonSerializable).Select(
                field => { return new[] { type, field, instance }; });
        });

    [TestCaseSource(nameof(ImplementedHavokTypes))]
    public void ReflectionInstantiateTest(HavokType havokType)
    {
        if (havokType.Type!.IsInterface || havokType.Type!.IsAbstract) return;
        Assert.That(havokType.Instantiate, Throws.Nothing,
            $"Could not instantiate type with identity {havokType.Identity}.");
    }

    [TestCaseSource(nameof(ImplementedHavokRecordTypes))]
    public void ReflectionInheritanceTest(HavokType havokType)
    {
        if (havokType.Parent is not null)
        {
            Type? parentType = havokType.Parent!.Type;
            Assert.That(parentType, Is.Not.Null,
                $"Parent of type with identity {havokType.Identity} does not have a C# representation");
            Assert.That(havokType.Type!.IsAssignableTo(parentType),
                $"Type with identity {havokType.Identity} and C# representation {havokType.Type!} is not assignable to parent with identity {havokType.Parent.Identity} and C# representation {havokType.Parent.Type!}");
        }

        foreach (HavokType.Interface typeInterface in havokType.Interfaces)
        {
            Type? interfaceType = typeInterface.Type.Type;
            Assert.That(interfaceType, Is.Not.Null,
                $"Interface of type with identity {havokType.Identity} does not have a C# representation");
            Assert.That(havokType.Type!.IsAssignableTo(interfaceType),
                $"Type with identity {havokType.Identity} and C# representation {havokType.Type!} is not assignable to interface with identity {typeInterface.Type.Identity} and C# representation {interfaceType}");
        }
    }

    [TestCaseSource(nameof(ImplementedHavokTypeFields))]
    public void ReflectionFieldSetTest(HavokType havokType, HavokType.Member field, object instance)
    {
        string nameWithPrefix = "m_" + field.Name;
        FieldInfo? fieldInfo = havokType.Type!.GetField(nameWithPrefix);
        Assert.That(fieldInfo, Is.Not.Null,
            $"Field named \"{field.Name}\" in type {havokType.Identity} was not found");
        Assert.That(field.Type.Type, Is.Not.Null,
            $"Field named \"{field.Name}\" in type {havokType.Identity} does not have a C# representation");
        Assert.That(fieldInfo!.FieldType, Is.EqualTo(field.Type.Type!),
            $"Field type mismatch.");

        if (field.Type.Type!.IsInterface) return;
        object fieldValue;
        switch (field.Type.Kind)
        {
            case HavokType.TypeKind.String:
                fieldValue = "";
                break;
            case HavokType.TypeKind.Array:
                if (field.Type.Type.IsArray)
                {
                    int length = field.Type.TemplateParameters[1].Value!.Value;
                    if (field.Type.SubType!.Type!.IsArray)
                    {
                        int innerLength = field.Type.SubType!.TemplateParameters[1].Value!.Value;
                        fieldValue = Array.CreateInstance(field.Type.Type.GetElementType()!, length, innerLength);
                    }
                    else
                    {
                        fieldValue = Array.CreateInstance(field.Type.Type.GetElementType()!, length);
                    }
                }
                else
                {
                    fieldValue = Activator.CreateInstance(field.Type.Type!)!;
                }

                break;
            case HavokType.TypeKind.Void:
            case HavokType.TypeKind.Opaque:
            case HavokType.TypeKind.Bool:
            case HavokType.TypeKind.Int:
            case HavokType.TypeKind.Float:
            case HavokType.TypeKind.Pointer:
            case HavokType.TypeKind.Record:
            default:
                fieldValue = Activator.CreateInstance(field.Type.Type!)!;
                break;
        }

        Assert.DoesNotThrow(() => fieldInfo.SetValue(instance, fieldValue));
    }
}