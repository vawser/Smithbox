using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Util;
public static class PropFinderUtil
{
    /// <summary>
    ///     Stores PropertyInfo and the relevant object which contains that property.
    /// </summary>
    /// <param name="PropInfo">Property's Info.</param>
    /// <param name="Obj">Object that contains property.</param>
    private record PropData(PropertyInfo PropInfo, object Obj);

    /// <summary>
    ///     Search an object's properties and return a PropData containing the targeted property's information.
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="obj"></param>
    /// <param name="classIndex"></param>
    /// <param name="onlyCheckPropName">If true, search only checks property name. Otherwise, it checks unique MetadataToken.</param>
    /// <returns>PropData that has the property if found, otherwise null.</returns>
    /// <summary>
    private static PropData? GetPropData(PropertyInfo prop, object obj, int classIndex = -1, bool onlyCheckPropName = false)
    {
        foreach (PropertyInfo p in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (onlyCheckPropName)
            {
                if (p.Name.ToLower() == prop.Name.ToLower())
                    return new PropData(p, obj);
            }
            else
            {
                if (p.MetadataToken == prop.MetadataToken)
                    return new PropData(prop, obj);
            }

            if (p.PropertyType.IsNested)
            {
                var retObj = GetPropData(prop, p.GetValue(obj), classIndex);
                if (retObj != null)
                    return retObj;
            }
            else if (p.PropertyType.IsArray)
            {
                Type pType = p.PropertyType.GetElementType();
                if (pType.IsNested)
                {
                    var array = (Array)p.GetValue(obj);
                    if (classIndex != -1)
                    {
                        var retObj = GetPropData(prop, array.GetValue(classIndex), classIndex);
                        if (retObj != null)
                            return retObj;
                    }
                    else
                    {
                        foreach (var arrayObj in array)
                        {
                            var retObj = GetPropData(prop, arrayObj, classIndex);
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
    public static PropertyInfo? FindProperty(string prop, object obj, int classIndex = -1)
    {
        var proppy = obj.GetType().GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
        if (proppy != null)
            return proppy;

        foreach (var p in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (p.PropertyType.IsNested)
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
    public static object? FindPropertyObject(PropertyInfo prop, object obj, int classIndex = -1, bool onlyCheckPropName = false)
    {
        var result = GetPropData(prop, obj, classIndex, onlyCheckPropName);

        if (result == null)
            return null;

        return result.Obj;
    }

    /// <summary>
    ///     Searches an object to find a property, then obtains the value.
    /// </summary>
    /// <returns>Value of the property within given object if found, otherwise null.</returns>
    public static object? FindPropertyValue(PropertyInfo prop, object obj, bool onlyCheckPropName = false)
    {
        var propData = GetPropData(prop, obj, -1, onlyCheckPropName);

        if (propData == null)
            return null;

        return propData.PropInfo.GetValue(propData.Obj);
    }
}
