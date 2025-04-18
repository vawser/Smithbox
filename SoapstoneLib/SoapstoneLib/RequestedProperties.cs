using System.Collections.Generic;
using SoapstoneLib.Proto;

namespace SoapstoneLib
{
    /// <summary>
    /// A list of requested property keys for a search or get request.
    /// </summary>
    public sealed class RequestedProperties
    {
        /// <summary>
        /// The properties, using the protobuf RequestedProperty class.
        /// </summary>
        public IList<RequestedProperty> Properties { get; set; }

        /// <summary>
        /// Adds the given property keys as properties to always fetch when present.
        /// </summary>
        public RequestedProperties Add(params string[] names)
        {
            if (Properties == null)
            {
                Properties = new List<RequestedProperty>();
            }
            foreach (string name in names)
            {
                Properties.Add(new RequestedProperty { Key = name });
            }
            return this;
        }

        /// <summary>
        /// Adds the given property keys as properties to fetch when the values are non-trivial (see IsNonTrivialValue).
        /// 
        /// This may help performance in cases where a given property key is expected to be missing
        /// most of the time, or where the property is for a fixed-size array whose entries are 0 or
        /// -1 if unspecified.
        /// </summary>
        public RequestedProperties AddNonTrivial(params string[] names)
        {
            if (Properties == null)
            {
                Properties = new List<RequestedProperty>();
            }
            foreach (string name in names)
            {
                Properties.Add(new RequestedProperty { Key = name, NonTrivialOnly = true });
            }
            return this;
        }

        /// <summary>
        /// Returns whether a given value is non-trivial for the purpose of filtering results.
        /// 
        /// This checks that integer-like values are positive and strings are non-empty. Floating
        /// point values are currently never considered trivial.
        /// </summary>
        public static bool IsNonTrivialValue(object obj)
        {
            switch (obj)
            {
                case sbyte s8Value:
                    return s8Value > 0;
                case byte u8Value:
                    return u8Value > 0;
                case short s16Value:
                    return s16Value > 0;
                case ushort u16Value:
                    return u16Value > 0;
                case int s32Value:
                    return s32Value > 0;
                case uint u32Value:
                    return u32Value > 0;
                case long s64Value:
                    return s64Value > 0;
                case ulong u64Value:
                    return u64Value > 0;
                case string stringValue:
                    return !string.IsNullOrEmpty(stringValue);
            }
            return true;
        }
    }
}
