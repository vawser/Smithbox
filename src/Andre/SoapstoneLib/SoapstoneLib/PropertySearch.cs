using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SoapstoneLib.Proto;

namespace SoapstoneLib
{
    /// <summary>
    /// Represents a search query to execute against SoulsObjects.
    /// 
    /// At present, the representation for this query is limited to conjunctive normal form (CNF),
    /// meaning it passes if all clauses succeed, where each clause can succeed if any individual
    /// property checks succeed.
    /// 
    /// In C-like syntax, this looks like: Map == "m10_00_00_00" &amp;&amp; (EntityID == 450 || EntityID == 451)
    /// This would be created like: PropertySearch.AllOf(mapCond, PropertySearch.AnyOf(entityCond, entityCond2))
    /// 
    /// The representation may change in the future to support arbitrary nested queries. Editors may
    /// look for top-level keys to do high-level filtering (e.g. map names for maps, param names for parmas),
    /// so keeping those keys in their own distinct clauses may lead to significantly better performance.
    /// </summary>
    public sealed class PropertySearch
    {
        internal PropertySearch(List<List<Condition>> AllOfAnyConditions)
        {
            this.AllOfAnyConditions = AllOfAnyConditions;
        }

        /// <summary>
        /// Creates a PropertySearch only checking a single condition.
        /// </summary>
        public static PropertySearch Of(Condition cond)
        {
            return new PropertySearch(new List<List<Condition>> { new List<Condition> { cond } });
        }

        /// <summary>
        /// Creates a PropertySearch requiring all of a given set of conditions.
        /// 
        /// The obejcts can be a mix of Conditions and/or PropertySearches.
        /// 
        /// Order matters for efficiency, because property checks can stop when
        /// any conjunctive clause evaluates to false.
        /// </summary>
        /// <exception cref="ArgumentException">Object is not Condition or PropertySearch</exception>
        public static PropertySearch AllOf(params object[] objs)
        {
            List<List<Condition>> all = new List<List<Condition>>();
            // Each entry is assumed to be a single any-of clause
            foreach (object obj in objs)
            {
                if (obj is Condition cond)
                {
                    all.Add(new List<Condition> { cond });
                }
                else if (obj is PropertySearch search)
                {
                    // Normally there should be one entry here, but it should be fine to AddRange to
                    // intersect the conditions, if for some reason there's an AllOf AllOf.
                    all.AddRange(search.AllOfAnyConditions);
                }
                else
                {
                    throw new ArgumentException($"Unknown AllOf argument type {obj?.GetType()}, expected Condition or PropertySearch");
                }
            }
            return new PropertySearch(all);
        }

        /// <summary>
        /// Creates a PropertySearch which passes when any of given conditions are met.
        /// 
        /// This can be chained into AllOf.
        /// 
        /// Order matters for efficiency, because property checks can stop when
        /// any disjunctive clause evaluates to true.
        /// </summary>
        /// <exception cref="ArgumentException">No Conditions are given</exception>
        public static PropertySearch AnyOf(params Condition[] conds)
        {
            if (conds.Length == 0)
            {
                throw new ArgumentException($"Given no conditions");
            }
            return new PropertySearch(new List<List<Condition>> { conds.ToList() });
        }

        /// <summary>
        /// IEnumerable version of AnyOf.
        /// </summary>
        public static PropertySearch AnyOf(IEnumerable<Condition> conds)
        {
            List<Condition> condList = conds.ToList();
            if (condList.Count == 0)
            {
                throw new ArgumentException($"Given no conditions");
            }
            return new PropertySearch(new List<List<Condition>> { condList });
        }

        /// <summary>
        /// All conditions in conjunctive normal form.
        /// 
        /// This representation may change in the future.
        /// </summary>
        public List<List<Condition>> AllOfAnyConditions { get; set; }

        /// <summary>
        /// Returns the first condition in the search, or none if it's empty.
        /// </summary>
        public Condition FirstCondition => AllOfAnyConditions?.FirstOrDefault()?.FirstOrDefault();

        /// <summary>
        /// Returns whether the search succeeds for the given property accessor.
        /// 
        /// The accessor function takes a property key and returns a value. If it returns null,
        /// the condition evaluates to false. If it returns an IEnumerable of values, the condition
        /// passes when any of the value passes.
        /// </summary>
        public bool IsMatch(Func<string, object> accessor)
        {
            // For now, each value is accessed every time it appears.
            // It would be straightforward to cache this if a value appears multiple times,
            // but for now just do the simple thing.
            foreach (List<Condition> anyConds in AllOfAnyConditions)
            {
                if (!anyConds.Any(cond => cond.IsMatch(accessor.Invoke(cond.Key))))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns all distinct property keys in the search query.
        /// </summary>
        public IEnumerable<string> SearchKeys => AllOfAnyConditions.SelectMany(any => any.Select(cond => cond.Key)).Distinct();

        /// <summary>
        /// Returns whether the given property key is used in this search.
        /// </summary>
        public bool IsFiltered(string key) => AllOfAnyConditions.Any(any => any.Any(cond => cond.Key == key));

        /// <summary>
        /// Returns a special search predicate which is limited to the given property key.
        /// 
        /// If the predicate returns true for a value of the given key, the full search may
        /// or may not succeed. If the predicate returns false, the full search will definitely
        /// not succeed.
        /// 
        /// This can be used to pre-check a given high-level property, before evaluating more
        /// expensive ones, especially if this allows skipping expensive traversals. The predicate
        /// always returns true if the key is not part of the search query.
        /// </summary>
        public Predicate<object> GetKeyFilter(string key)
        {
            List<List<Condition>> filteredConds = new List<List<Condition>>();
            foreach (List<Condition> anyConds in AllOfAnyConditions)
            {
                if (anyConds.Any(cond => cond.Key == key))
                {
                    if (anyConds.All(cond => cond.Key == key))
                    {
                        filteredConds.Add(anyConds);
                    }
                    else
                    {
                        // If mixed conditions between key and non-key, key no longer uniquely filters
                        filteredConds = null;
                        break;
                    }
                }
            }
            return test =>
            {
                if (filteredConds == null)
                {
                    return true;
                }
                foreach (List<Condition> anyConds in filteredConds)
                {
                    if (!anyConds.Any(cond => cond.IsMatch(test)))
                    {
                        return false;
                    }
                }
                return true;
            };
        }

        /// <summary>
        /// Represents a condition which can be checked against a specific property.
        /// 
        /// For instance, EntityID > 0, ModelName ~= "^c", FMG == "NpcName".
        /// </summary>
        public sealed class Condition
        {
            /// <summary>
            /// Creates a Condition with the given fields.
            /// 
            /// If it is a regex search, the Value may be compiled into a C# Regex.
            /// </summary>
            public Condition(PropertyComparisonType Type, string Key, object Value)
            {
                this.Type = Type;
                this.Key = Key;
                // Optimization: Regexes get precomputed. Not sure if compilation is worth it (should benchmark).
                if ((Type == PropertyComparisonType.Matches || Type == PropertyComparisonType.NotMatches)
                    && Value is string reStr)
                {
                    Value = new Regex(reStr, RegexOptions.Compiled);
                }
                this.Value = Value;
            }

            /// <summary>
            /// The comparison to perform between the value and IsMatch input.
            /// </summary>
            public PropertyComparisonType Type { get; set; }

            /// <summary>
            /// The property key to access.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            /// The value to compare the property against.
            /// 
            /// For non-commutative comparisons, this is the left-hand side.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Tests the given value on this condition.
            /// </summary>
            public bool IsMatch(object test)
            {
                // Match any of a list of scalars
                if (test is not string && test is System.Collections.IEnumerable multi)
                {
                    foreach (object val in multi)
                    {
                        if (IsMatchScalar(val))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                return IsMatchScalar(test);
            }

            private bool IsMatchScalar(object test)
            {
                // null operations are not supported under any circumstances
                if (test == null || Value == null)
                {
                    return false;
                }
                switch (Type)
                {
                    case PropertyComparisonType.Unspecified:
                        return false;
                    case PropertyComparisonType.Equal:
                        // Beware of floating point equality, there is no specific check for it here.
                        return Value.Equals(test);
                    case PropertyComparisonType.NotEqual:
                        return !Value.Equals(test);
                    case PropertyComparisonType.Greater:
                        return IsLessThan(test, Value, true, true);
                    case PropertyComparisonType.Less:
                        return IsLessThan(test, Value, true, false);
                    case PropertyComparisonType.GreaterOrEqual:
                        return IsLessThan(test, Value, false, true);
                    case PropertyComparisonType.LessOrEqual:
                        return IsLessThan(test, Value, false, false);
                    case PropertyComparisonType.Matches:
                        return IsRegexMatch(test, Value);
                    case PropertyComparisonType.NotMatches:
                        return !IsRegexMatch(test, Value);
                    default:
                        return false;
                }
            }

            private static bool IsRegexMatch(object test, object value)
            {
                if (value is Regex re)
                {
                    return re.IsMatch(test.ToString());
                }
                // It is a bit weird to treat non-strings like regex, but it is what was requested
                return Regex.IsMatch(test.ToString(), value.ToString());
            }

            private static bool IsLessThan(object test, object value, bool strict, bool invert)
            {
                int result;
                if (test.GetType() == value.GetType() && test is IComparable testCmp)
                {
                    result = testCmp.CompareTo(value);
                }
                else if (test is float or double && value is float or double)
                {
                    result = Convert.ToDouble(test).CompareTo(Convert.ToDouble(value));
                }
                else if (test is IConvertible testConv && value is IConvertible valueConv)
                {
                    // Unknown, assume integer types; use decimal for massive precision, at the cost of performance.
                    try
                    {
                        result = testConv.ToDecimal(null).CompareTo(valueConv.ToDecimal(null));
                    }
                    catch (Exception)
                    {
                        // This is incredibly expensive, especially with hundreds or thousands of comparisons.
                        // Unfortunately, there is no TryConvert in C# standard library.
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                // This logic is a bit tricky. Negating the result also changes strictness,
                // so account for it during the strictness check itself.
                return (strict != invert ? result < 0 : result <= 0) != invert;
            }
        }
    }
}
