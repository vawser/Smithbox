using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace StudioCore.Editors.Common;

public static class PropFinderUtil
{
    /// <summary>
    ///     Stores PropertyInfo and the relevant object which contains that property.
    /// </summary>
    /// <param name="PropInfo">Property's Info.</param>
    /// <param name="Obj">Object that contains property.</param>
    private record PropData(PropertyInfo PropInfo, object Obj);

    private record FieldData(FieldInfo PropInfo, object Obj);

    /// <summary>
    ///     If the given type is List&lt;T&gt; (or a type derived from it), returns T. Otherwise returns null.
    /// </summary>
    private static Type GetListElementType(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return type.GetGenericArguments()[0];

        if (type.BaseType != null && type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(List<>))
            return type.BaseType.GetGenericArguments()[0];

        return null;
    }

    /// <summary>
    ///     Search an object's properties and return a PropData containing the targeted property's information.
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="obj"></param>
    /// <param name="classIndex"></param>
    /// <param name="onlyCheckPropName">If true, search only checks property name. Otherwise, it checks unique MetadataToken.</param>
    /// <returns>PropData that has the property if found, otherwise null.</returns>
    private static PropData GetPropData(PropertyInfo prop, object obj, int arrayIndex = -1, int classIndex = -1, bool onlyCheckPropName = false)
    {
        if (obj == null) return null;

        foreach (PropertyInfo p in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (p.GetIndexParameters().Length > 0)
                continue;

            if (onlyCheckPropName)
            {
                if (string.Equals(p.Name, prop.Name, StringComparison.OrdinalIgnoreCase))
                    return new PropData(p, obj);
            }
            else
            {
                if (p.MetadataToken == prop.MetadataToken)
                    return new PropData(p, obj);
            }

            Type listElemType = GetListElementType(p.PropertyType);
            if (listElemType != null)
            {
                var list = p.GetValue(obj) as IList;
                if (list == null) continue;

                var containerResult = GetPropData(prop, list, arrayIndex, classIndex, onlyCheckPropName);
                if (containerResult != null)
                    return containerResult;

                if (listElemType.IsNested)
                {
                    if (arrayIndex != -1)
                    {
                        if (arrayIndex < list.Count)
                        {
                            var retObj = GetPropData(prop, list[arrayIndex], arrayIndex, classIndex);
                            if (retObj != null)
                                return retObj;
                        }
                    }
                    else if (classIndex != -1)
                    {
                        if (classIndex < list.Count)
                        {
                            var retObj = GetPropData(prop, list[classIndex], arrayIndex, classIndex);
                            if (retObj != null)
                                return retObj;
                        }
                    }
                    else
                    {
                        foreach (var listObj in list)
                        {
                            var retObj = GetPropData(prop, listObj, arrayIndex, classIndex);
                            if (retObj != null)
                                return retObj;
                        }
                    }
                }
            }
            else if (p.PropertyType.IsNested)
            {
                var check = p.GetValue(obj);
                if (check == null) continue;

                var retObj = GetPropData(prop, p.GetValue(obj), arrayIndex, classIndex);
                if (retObj != null)
                    return retObj;
            }
            else if (p.PropertyType.IsArray)
            {
                Type pType = p.PropertyType.GetElementType();
                if (pType.IsNested)
                {
                    var array = (Array)p.GetValue(obj);
                    if (array == null) continue;

                    if (arrayIndex != -1)
                    {
                        var retObj = GetPropData(prop, array.GetValue(arrayIndex), arrayIndex, classIndex);
                        if (retObj != null)
                            return retObj;

                    }
                    else if (classIndex != -1)
                    {
                        var retObj = GetPropData(prop, array.GetValue(classIndex), arrayIndex, classIndex);
                        if (retObj != null)
                            return retObj;
                    }
                    else
                    {
                        foreach (var arrayObj in array)
                        {
                            var retObj = GetPropData(prop, arrayObj, arrayIndex, classIndex);
                            if (retObj != null)
                                return retObj;
                        }
                    }
                }
            }
        }

        return null;
    }

    private static FieldData GetPropData(FieldInfo prop, object obj, int arrayIndex = -1, int classIndex = -1, bool onlyCheckPropName = false)
    {
        if (obj == null) return null;

        foreach (FieldInfo p in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (onlyCheckPropName)
            {
                if (string.Equals(p.Name, prop.Name, StringComparison.OrdinalIgnoreCase))
                    return new FieldData(p, obj);
            }
            else
            {
                if (p.MetadataToken == prop.MetadataToken)
                    return new FieldData(p, obj);
            }

            Type listElemType = GetListElementType(p.FieldType);
            if (listElemType != null)
            {
                var list = p.GetValue(obj) as IList;
                if (list == null) continue;

                var containerResult = GetPropData(prop, list, arrayIndex, classIndex, onlyCheckPropName);
                if (containerResult != null)
                    return containerResult;

                if (listElemType.IsNested)
                {
                    if (arrayIndex != -1)
                    {
                        if (arrayIndex < list.Count)
                        {
                            var retObj = GetPropData(prop, list[arrayIndex], arrayIndex, classIndex);
                            if (retObj != null)
                                return retObj;
                        }
                    }
                    else if (classIndex != -1)
                    {
                        if (classIndex < list.Count)
                        {
                            var retObj = GetPropData(prop, list[classIndex], arrayIndex, classIndex);
                            if (retObj != null)
                                return retObj;
                        }
                    }
                    else
                    {
                        foreach (var listObj in list)
                        {
                            var retObj = GetPropData(prop, listObj, arrayIndex, classIndex);
                            if (retObj != null)
                                return retObj;
                        }
                    }
                }
            }
            else if (p.FieldType.IsNested)
            {
                var check = p.GetValue(obj);
                if (check == null) continue;

                var retObj = GetPropData(prop, p.GetValue(obj), arrayIndex, classIndex);
                if (retObj != null)
                    return retObj;
            }
            else if (p.FieldType.IsArray)
            {
                Type pType = p.FieldType.GetElementType();
                if (pType.IsNested)
                {
                    var array = (Array)p.GetValue(obj);
                    if (array == null) continue;

                    if (arrayIndex != -1)
                    {
                        var retObj = GetPropData(prop, array.GetValue(arrayIndex), arrayIndex, classIndex);
                        if (retObj != null)
                            return retObj;

                    }
                    else if (classIndex != -1)
                    {
                        var retObj = GetPropData(prop, array.GetValue(classIndex), arrayIndex, classIndex);
                        if (retObj != null)
                            return retObj;
                    }
                    else
                    {
                        foreach (var arrayObj in array)
                        {
                            var retObj = GetPropData(prop, arrayObj, arrayIndex, classIndex);
                            if (retObj != null)
                                return retObj;
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Finds property within provided object that matches given name.
    /// </summary>
    /// <returns>PropertyInfo if found, otherwise null.</returns>
    public static PropertyInfo FindProperty(string prop, object obj, int classIndex = -1)
    {
        if (obj == null)
            return null;

        var proppy = obj.GetType().GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
        if (proppy != null)
            return proppy;

        foreach (var p in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (p.GetIndexParameters().Length > 0)
                continue;

            var listElemType = GetListElementType(p.PropertyType);
            if (listElemType != null)
            {
                var list = p.GetValue(obj) as IList;
                if (list == null) continue;

                // Properties declared directly on the list container itself (e.g. MemberList.Unk00).
                var containerPp = FindProperty(prop, list, classIndex);
                if (containerPp != null)
                    return containerPp;

                if (listElemType.IsNested)
                {
                    if (classIndex != -1)
                    {
                        if (classIndex < list.Count)
                        {
                            var pp = FindProperty(prop, list[classIndex], classIndex);
                            if (pp != null)
                                return pp;
                        }
                    }
                    else
                    {
                        foreach (var listObj in list)
                        {
                            var pp = FindProperty(prop, listObj, classIndex);
                            if (pp != null)
                                return pp;
                        }
                    }
                }
            }
            else if (p.PropertyType.IsNested)
            {
                var pp = FindProperty(prop, p.GetValue(obj), classIndex);
                if (pp != null)
                    return pp;
            }
            else if (p.PropertyType.IsArray)
            {
                var pType = p.PropertyType.GetElementType();
                if (pType.IsNested)
                {
                    Array array = (Array)p.GetValue(obj);
                    if (classIndex != -1)
                    {
                        var pp = FindProperty(prop, array.GetValue(classIndex), classIndex);
                        if (pp != null)
                            return pp;
                    }
                    else
                    {
                        foreach (var arrayObj in array)
                        {
                            var pp = FindProperty(prop, arrayObj, classIndex);
                            if (pp != null)
                                return pp;
                        }
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Searches an object to find exactly which object contains the property.
    /// </summary>
    /// <returns>Object containing property if found, otherwise null.</returns>
    public static object FindPropertyObject(PropertyInfo prop, object obj, int arrayIndex = -1, int classIndex = -1, bool onlyCheckPropName = false)
    {
        var result = GetPropData(prop, obj, arrayIndex, classIndex, onlyCheckPropName);

        if (result == null)
            return null;

        return result.Obj;
    }

    public static object FindFieldObject(FieldInfo prop, object obj, int arrayIndex = -1, int classIndex = -1, bool onlyCheckPropName = false)
    {
        var result = GetPropData(prop, obj, arrayIndex, classIndex, onlyCheckPropName);

        if (result == null)
            return null;

        return result.Obj;
    }

    /// <summary>
    ///     Searches an object to find a property, then obtains the value.
    /// </summary>
    /// <returns>Value of the property within given object if found, otherwise null.</returns>
    public static object FindPropertyValue(PropertyInfo prop, object obj, bool onlyCheckPropName = false)
    {
        var propData = GetPropData(prop, obj, -1, -1, onlyCheckPropName);

        if (propData == null)
            return null;

        return propData.PropInfo.GetValue(propData.Obj);
    }

    public static object FindPropertyValue(string propName, object obj, bool onlyCheckPropName = false)
    {
        var prop = FindProperty(propName, obj);
        if (prop == null)
            return null;
        var val = FindPropertyValue(prop, obj, onlyCheckPropName);
        return val;
    }

    public static object CreateDefaultListElement(Type elementType)
    {
        if (elementType == typeof(string))
            return string.Empty;

        if (elementType.IsValueType)
            return Activator.CreateInstance(elementType);

        if (elementType.IsAbstract || elementType.IsInterface)
            return null;

        return elementType.GetConstructor(Type.EmptyTypes) != null
            ? Activator.CreateInstance(elementType)
            : null;
    }
}