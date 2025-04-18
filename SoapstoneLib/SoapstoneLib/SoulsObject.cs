using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SoapstoneLib.Proto;

namespace SoapstoneLib
{
    /// <summary>
    /// Represents an entity in FromSoft game data with a unique key and various properties.
    /// 
    /// Properties are represented as a series of key-value pairs. Multiple values for a
    /// given property will result in multiple pairs with that key.
    /// </summary>
    public sealed partial class SoulsObject
    {
        /// <summary>
        /// All supported scalar property types. IEnumerable of any of these is also supported via repeated properties.
        /// </summary>
        public static readonly IReadOnlyCollection<Type> ScalarPropertyTypes = ImmutableHashSet.Create(
            typeof(sbyte), typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint),
            typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(bool), typeof(string));

        internal IList<Proto.Internal.GameProperty> properties;

        /// <summary>
        /// Creates a new SoulsObject with the given key and no initial properties.
        /// 
        /// In a request context, properties can be most easily added with AddRequestedProperties.
        /// </summary>
        public SoulsObject(SoulsKey Key)
        {
            this.Key = Key;
            properties = new List<Proto.Internal.GameProperty>();
        }

        internal SoulsObject(SoulsKey Key, IList<Proto.Internal.GameProperty> properties)
        {
            this.Key = Key;
            this.properties = properties;
        }

        /// <summary>
        /// The key identifying this object.
        /// </summary>
        public SoulsKey Key { get; }

        /// <summary>
        /// Produces the first value of any property with the given key.
        /// </summary>
        public bool TryGetValue(string key, out object value)
        {
            foreach (Proto.Internal.GameProperty prop in properties)
            {
                if (prop.Key == key)
                {
                    value = InternalConversions.FromPropertyValue(prop.Value);
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Produces the first value of any property with the given key and output type.
        /// 
        /// This has to match the type exactly. Use TryGetInt if the value is int-convertible.
        /// </summary>
        public bool TryGetValue<T>(string key, out T value)
        {
            foreach (Proto.Internal.GameProperty prop in properties)
            {
                if (prop.Key == key)
                {
                    object val = InternalConversions.FromPropertyValue(prop.Value);
                    if (val is T tval)
                    {
                        value = tval;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Produces the first value of any property with the given key, if the value can be converted to an int.
        /// </summary>
        public bool TryGetInt(string key, out int value)
        {
            value = default;
            if (!TryGetValue(key, out object obj))
            {
                return false;
            }
            if (obj is int ival)
            {
                value = ival;
                return true;
            }
            // We could also special-case uint for performance reasons, but would also have to check for out-of-range values.
            // Everything else should be convertible, if not out of range. string does parse here.
            try
            {
                value = Convert.ToInt32(obj);
                return true;
            }
            catch
            {
                // This is very expensive
                return false;
            }
        }

        /// <summary>
        /// Produces all values for properties with the given key.
        /// </summary>
        public bool TryGetValues(string key, out List<object> values)
        {
            values = null;
            foreach (Proto.Internal.GameProperty prop in properties)
            {
                if (prop.Key == key)
                {
                    object value = InternalConversions.FromPropertyValue(prop.Value);
                    if (values == null)
                    {
                        values = new List<object> { value };
                    }
                    else
                    {
                        values.Add(value);
                    }
                }
            }
            return values != null;
        }

        /// <summary>
        /// Produces all values for properties with the given key and output type.
        /// 
        /// The type must match exactly.
        /// </summary>
        public bool TryGetValues<T>(string key, out List<T> values)
        {
            values = null;
            foreach (Proto.Internal.GameProperty prop in properties)
            {
                if (prop.Key == key)
                {
                    object value = InternalConversions.FromPropertyValue(prop.Value);
                    if (value is T tval)
                    {
                        if (values == null)
                        {
                            values = new List<T> { tval };
                        }
                        else
                        {
                            values.Add(tval);
                        }
                    }
                }
            }
            return values != null;
        }

        /// <summary>
        /// Produces all values for properties with the given key, if the values can be converted to an int.
        /// </summary>
        public bool TryGetInts(string key, out List<int> values)
        {
            values = null;
            foreach (Proto.Internal.GameProperty prop in properties)
            {
                if (prop.Key == key)
                {
                    object obj = InternalConversions.FromPropertyValue(prop.Value);
                    // Adapted from TryGetInt
                    if (obj is not int value)
                    {
                        try
                        {
                            value = Convert.ToInt32(obj);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    if (values == null)
                    {
                        values = new List<int> { value };
                    }
                    else
                    {
                        values.Add(value);
                    }
                }
            }
            return values != null;
        }

        /// <summary>
        /// Ordered enumerable of all key-value property pairs.
        /// </summary>
        public IEnumerable<Property> Properties => properties.Select(InternalConversions.FromGameProperty);

        /// <summary>
        /// Ordered enumerable of all distinct property key names.
        /// </summary>
        public IEnumerable<string> PropertyKeys => properties.Select(p => p.Key).Distinct();

        /// <summary>
        /// A key-value dictionary for properties.
        /// 
        /// If a key has multiple values, only the first one is returned.
        /// </summary>
        public Dictionary<string, object> FirstPropertyValues
        {
            get
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                foreach (Proto.Internal.GameProperty prop in properties)
                {
                    if (!dict.ContainsKey(prop.Key))
                    {
                        dict[prop.Key] = InternalConversions.FromPropertyValue(prop.Value);
                    }
                }
                return dict;
            }
        }

        /// <summary>
        /// Adds the given key-value property. The value's type should be in ScalarPropertyTypes.
        /// </summary>
        public void AddProperty(string key, object value)
        {
            properties.Add(new Proto.Internal.GameProperty { Key = key, Value = InternalConversions.ToPropertyValue(value) });
        }

        /// <summary>
        /// Adds the given Property instance. The property value's type should be in ScalarPropertyTypes.
        /// </summary>
        /// <param name="prop"></param>
        public void AddProperty(Property prop)
        {
            properties.Add(new Proto.Internal.GameProperty { Key = prop.Key, Value = InternalConversions.ToPropertyValue(prop.Value) });
        }

        /// <summary>
        /// Adds the key property with associated values. The values' types should be in ScalarPropertyTypes.
        /// </summary>
        public void AddProperties(string key, IEnumerable<object> values)
        {
            foreach (object value in values)
            {
                properties.Add(new Proto.Internal.GameProperty { Key = key, Value = InternalConversions.ToPropertyValue(value) });
            }
        }

        /// <summary>
        /// Adds the given Property instances. The property value types should be in ScalarPropertyTypes.
        /// </summary>
        /// <param name="props"></param>
        public void AddProperties(IEnumerable<Property> props)
        {
            foreach (Property prop in props)
            {
                properties.Add(new Proto.Internal.GameProperty { Key = prop.Key, Value = InternalConversions.ToPropertyValue(prop.Value) });
            }
        }

        /// <summary>
        /// Adds all properties corresponding to RequestedProperties given by a client.
        /// 
        /// The accessor function takes a property key and returns a value. If it returns null or
        /// an unsupported scalar type (not present in ScalarPropertyTypes),  the property is not added.
        /// If it returns an IEnumerable of values, each value is added individually.
        /// </summary>
        public void AddRequestedProperties(RequestedProperties props, Func<string, object> accessor)
        {
            foreach (RequestedProperty requested in props.Properties)
            {
                string key = requested.Key;
                object res = accessor.Invoke(key);
                if (res == null)
                {
                    continue;
                }
                if (res is not string && res is System.Collections.IEnumerable multi)
                {
                    foreach (object val in multi)
                    {
                        if (val == null)
                        {
                            continue;
                        }
                        if (ScalarPropertyTypes.Contains(val.GetType())
                            && (!requested.NonTrivialOnly || RequestedProperties.IsNonTrivialValue(val)))
                        {
                            AddProperty(key, val);
                        }
                    }
                }
                else if (ScalarPropertyTypes.Contains(res.GetType())
                    && (!requested.NonTrivialOnly || RequestedProperties.IsNonTrivialValue(res)))
                {
                    AddProperty(key, res);
                }
            }
        }

        /// <summary>
        /// Reset all properties.
        /// </summary>
        public void ClearProperties()
        {
            properties.Clear();
        }

        /// <inheritdoc />
        public override string ToString() => $"{Key}[{string.Join(",", Properties)}]";

        /// <summary>
        /// Represents a key-value pair of a game object.
        /// </summary>
        public sealed class Property
        {
            /// <summary>
            /// Creates a Property with the given fields.
            /// </summary>
            public Property(string Key, object Value)
            {
                this.Key = Key;
                this.Value = Value;
            }

            /// <summary>
            /// Lookup key for the property.
            /// </summary>
            public string Key { get; }

            /// <summary>
            /// Value for the property, which must be one of ScalarPropertyTypes.
            /// </summary>
            public object Value { get; }

            /// <inheritdoc />
            public override string ToString() => $"{Key}={Value}";
        }
    }
}
