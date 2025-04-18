using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SoapstoneLib.Proto;
using SoapstoneLib.Proto.Internal;
using static SoapstoneLib.Proto.Internal.PropertyValue;

namespace SoapstoneLib
{
    internal static class InternalConversions
    {
        private static readonly LRACache<string, SoulsKey.FileKey> fileKeyCache = new LRACache<string, SoulsKey.FileKey>(2000);

        internal static GameObject ToGameObject(SoulsObject obj)
        {
            return new GameObject
            {
                Key = ToPrimaryKey(obj.Key),
                Properties = { obj.properties },
            };
        }

        internal static SoulsObject FromGameObject(GameObject obj)
        {
            return new SoulsObject(FromPrimaryKey(obj.Key), obj.Properties);
        }

        internal static PrimaryKey ToPrimaryKey(SoulsKey key)
        {
            PrimaryKey ret = new PrimaryKey
            {
                File = key.File.FileName,
            };
            if (key is SoulsKey.ObjectKey objKey)
            {
                object id = objKey.InternalID;
                if (id is string strId)
                {
                    ret.StrId = strId;
                }
                else
                {
                    // This may throw if the id is not int64-compatible.
                    // Currently, these are all contained within SoapstoneLib, so this should not happen.
                    ret.IntId = Convert.ToInt64(id);
                }
                // Because this is proto3, setting default values should be a no-op
                ret.Namespace = objKey.Namespace;
                ret.Index = objKey.Index;
            }
            return ret;
        }

        internal static SoulsKey FromPrimaryKey(PrimaryKey key)
        {
            if (!fileKeyCache.TryGetValue(key.File, out SoulsKey.FileKey file))
            {
                file = SoulsKey.ParseFileKey(key.File);
                fileKeyCache.Add(key.File, file);
            }
            if (key.IdCase == PrimaryKey.IdOneofCase.None)
            {
                return file;
            }
            if (file is SoulsKey.GameParamKey gameParamKey
                && key.Namespace == KeyNamespace.Unspecified
                && key.IdCase == PrimaryKey.IdOneofCase.IntId)
            {
                return new SoulsKey.GameParamRowKey(gameParamKey, Convert.ToInt32(key.IntId), key.Index);
            }
            else if (file is SoulsKey.MsbKey msbKey
                && SoulsKey.MsbEntryKey.Namespaces.Contains(key.Namespace)
                && key.IdCase == PrimaryKey.IdOneofCase.StrId)
            {
                return new SoulsKey.MsbEntryKey(msbKey, key.Namespace, key.StrId);
            }
            else if (file is SoulsKey.FmgKey fmgKey
                && key.Namespace == KeyNamespace.Unspecified
                && key.IdCase == PrimaryKey.IdOneofCase.IntId)
            {
                return new SoulsKey.FmgEntryKey(fmgKey, Convert.ToInt32(key.IntId), key.Index);
            }
            return new SoulsKey.UnknownKey();
        }

        internal static PrimaryKeyType ToPrimaryKeyType(SoulsKeyType t)
        {
            return new PrimaryKeyType { File = t.File, Category = t.Category };
        }

        // At the moment, this is a sealed collection only from SoulsKey.
        // More flexibility can probably be added at the API level within SoulsKey, if necessary.
        internal static bool FromPrimaryKeyType(PrimaryKeyType type, out SoulsKeyType result)
        {
            // Create an invalid instance for matching purposes
            SoulsKeyType cand = new SoulsKeyType(type.File, type.Category, null);
            // This lookup may fail if there is no match.
            // This would mainly happen if a server is older than a client and supports fewer types.
            return SoulsKey.KeyTypes.TryGetValue(cand, out result);
        }

        internal static GameProperty ToGameProperty(SoulsObject.Property prop)
        {
            return new GameProperty { Key = prop.Key, Value = ToPropertyValue(prop.Value) };
        }

        internal static SoulsObject.Property FromGameProperty(GameProperty prop)
        {
            return new SoulsObject.Property(prop.Key, FromPropertyValue(prop.Value));
        }

        internal static Proto.Internal.PropertySearch ToPropertySearch(PropertySearch search)
        {
            // Blindly fail on null values here
            return new Proto.Internal.PropertySearch
            {
                AllOfAnyConditions = { search.AllOfAnyConditions.Select(ToMultiPropertyCondition) },
            };
        }

        internal static PropertySearch FromPropertySearch(Proto.Internal.PropertySearch search)
        {
            return new PropertySearch(search.AllOfAnyConditions.Select(FromMultiPropertyCondition).ToList());
        }

        internal static MultiPropertyCondition ToMultiPropertyCondition(IEnumerable<PropertySearch.Condition> cond)
        {
            return new MultiPropertyCondition
            {
                Conditions = { cond.Select(ToPropertyCondition) },
            };
        }

        internal static List<PropertySearch.Condition> FromMultiPropertyCondition(MultiPropertyCondition cond)
        {
            return cond.Conditions.Select(FromPropertyCondition).ToList();
        }

        internal static PropertyCondition ToPropertyCondition(PropertySearch.Condition cond)
        {
            return new PropertyCondition
            {
                Type = cond.Type,
                Key = cond.Key,
                Value = ToPropertyValue(cond.Value),
            };
        }

        internal static PropertySearch.Condition FromPropertyCondition(PropertyCondition cond)
        {
            return new PropertySearch.Condition(cond.Type, cond.Key, FromPropertyValue(cond.Value));
        }

        internal static PropertyValue ToPropertyValue(object obj)
        {
            PropertyValue value = new PropertyValue();
            switch (obj)
            {
                case sbyte s8Value:
                    value.S8Value = s8Value;
                    break;
                case byte u8Value:
                    value.U8Value= u8Value;
                    break;
                case short s16Value:
                    value.S16Value = s16Value;
                    break;
                case ushort u16Value:
                    value.U16Value = u16Value;
                    break;
                case int s32Value:
                    value.S32Value = s32Value;
                    break;
                case uint u32Value:
                    value.U32Value = u32Value;
                    break;
                case long s64Value:
                    value.S64Value = s64Value;
                    break;
                case ulong u64Value:
                    value.U64Value = u64Value;
                    break;
                case float f32Value:
                    value.F32Value = f32Value;
                    break;
                case double f64Value:
                    value.F64Value = f64Value;
                    break;
                case bool boolValue:
                    value.BoolValue = boolValue;
                    break;
                case string stringValue:
                    value.StringValue = stringValue;
                    break;
                case Regex regexValue:
                    // Special case for regexes, mainly in Conditions
                    value.StringValue = regexValue.ToString();
                    break;
                default:
                    if (obj == null)
                    {
                        throw new ArgumentNullException(nameof(obj));
                    }
                    throw new Exception($"Invalid property type {obj.GetType()} (only standard scalar built-in value types and strings are supported)");
            }
            return value;
        }

        internal static object FromPropertyValue(PropertyValue value)
        {
            switch (value.ValueCase)
            {
                case ValueOneofCase.S8Value:
                    return value.S8Value;
                case ValueOneofCase.U8Value:
                    return value.U8Value;
                case ValueOneofCase.S16Value:
                    return value.S16Value;
                case ValueOneofCase.U16Value:
                    return value.U16Value;
                case ValueOneofCase.S32Value:
                    return value.S32Value;
                case ValueOneofCase.U32Value:
                    return value.U32Value;
                case ValueOneofCase.S64Value:
                    return value.S64Value;
                case ValueOneofCase.U64Value:
                    return value.U64Value;
                case ValueOneofCase.F32Value:
                    return value.F32Value;
                case ValueOneofCase.F64Value:
                    return value.F64Value;
                case ValueOneofCase.BoolValue:
                    return value.BoolValue;
                case ValueOneofCase.StringValue:
                    return value.StringValue;
                default:
                    throw new ArgumentException($"Invalid PropertyValue {value}");
            }
        }

        /// <summary>
        /// Very simple "least recently added" cache with a mechanism to avoid unbounded growth.
        /// 
        /// This attempts to enable sharing of values without the full invasive pointer logic
        /// of an least recently updated/accessed cache. In the worst case, looping over a big list
        /// of keys larger than the capacity, most types of caches perform poorly. Overall, this
        /// system works best with both time locality and a fairly cheap cost of replacing lost
        /// entries, so is suitable for storing small objects deterministically given by the key.
        /// </summary>
        private class LRACache<K, V>
        {
            private readonly int capacity;
            private readonly Dictionary<K, V> inner = new Dictionary<K, V>();
            // Could also use an array-based ring buffer here, check traces for time/space tradeoffs.
            private readonly LinkedList<K> order = new LinkedList<K>();

            public LRACache(int capacity)
            {
                this.capacity = capacity;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public bool TryGetValue(K key, out V value) => inner.TryGetValue(key, out value);

            [MethodImpl(MethodImplOptions.Synchronized)]
            public void Add(K key, V value)
            {
                if (inner.ContainsKey(key))
                {
                    inner[key] = value;
                    return;
                }
                if (inner.Count >= capacity)
                {
                    inner.Remove(order.First.Value);
                    order.RemoveFirst();
                }
                inner[key] = value;
                order.AddLast(key);
            }
        }
    }
}
