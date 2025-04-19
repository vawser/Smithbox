using HKLib.hk2018.hkAtomic;
using Org.BouncyCastle.Asn1.X509;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

public class MsbUtils
{
    static bool Matches(string? name, Type refType, IMsbEntry entry) => entry.Name == name && refType.IsAssignableFrom(entry.GetType());
    /// <summary>
    /// This will yield the value and a setter for that value for each field in `entry` that's a MSBReference
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public static IEnumerable<(string?, Type, Action<string?>)> GetMsbReferences(IMsbEntry entry)
    {
        foreach (var property in entry.GetType().GetProperties())
        {
            if (property.GetCustomAttribute<MSBReference>() is not MSBReference msbReference) 
                continue;

            var value = property.GetValue(entry);
            if (value is string?[] array)
            {
                var index = 0;
                foreach (var element in array)
                {
                    yield return (array[index], msbReference.ReferenceType, (newName) => array[index] = newName);
                    index += 1;
                }
            }
            else if (value is string str)
            {
                yield return (str, msbReference.ReferenceType, (newName) => property.SetValue(entry, newName));
            }
        }
    }

    /// <summary>
    /// Renames the target, and every reference to it within 'entries'
    /// </summary>
    public static void RenameWithRefs(IEnumerable<IMsbEntry> entries, IMsbEntry target, string newName)
    {
        foreach (var entry in entries)
        {
            foreach (var (current, type, set) in GetMsbReferences(entry))
            {
                if (Matches(current, type, target)) { set(newName); }
            }
        }
        target.Name = newName;
    }

    /// <summary>
    /// Empties out every reference in 'obj' that's not pointing to an entity that's part of 'entities'
    /// </summary>
    public static void StripMsbReference(IEnumerable<IMsbEntry> entities, IMsbEntry obj)
    {
        foreach (var (current, type, set) in GetMsbReferences(obj))
        {
            if (current is null or "") 
                continue;

            if (!entities.Any((ent) => Matches(current, type, ent)))
                set(null);
        }
    }

}