using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SoulsFormats
{
    /// <summary>
    /// Common classes and functions for MSB formats.
    /// </summary>
    public static partial class MSB
    {
        public class MissingReferenceException : Exception
        {
            public IMsbEntry Referrer;
            public string ReferreeName;

            public MissingReferenceException(IMsbEntry referrer, string refereeName)
                : base($"\"{referrer}\" references \"{refereeName}\", which does not exist")
            {
                Referrer = referrer;
                ReferreeName = refereeName;
            }
        }

        internal static void AssertHeader(BinaryReaderEx br)
        {
            br.AssertASCII("MSB ");
            br.AssertInt32(1);
            br.AssertInt32(0x10);
            br.AssertBoolean(false); // isBigEndian
            br.AssertBoolean(false); // isBitBigEndian
            br.AssertByte(1); // textEncoding
            br.AssertByte(0xFF); // is64BitOffset
        }

        internal static void WriteHeader(BinaryWriterEx bw)
        {
            bw.WriteASCII("MSB ");
            bw.WriteInt32(1);
            bw.WriteInt32(0x10);
            bw.WriteBoolean(false);
            bw.WriteBoolean(false);
            bw.WriteByte(1);
            bw.WriteByte(0xFF);
        }

        internal static void DisambiguateNames<T>(List<T> entries, string className = "") where T : IMsbEntry
        {
            bool ambiguous;
            do
            {
                ambiguous = false;
                var nameCounts = new Dictionary<string, int>();
                
                // Some entries have blank names but are referenced, which means they all must be
                // disambiguated.
                nameCounts[""] = 0;
                
                foreach (IMsbEntry entry in entries)
                {
                    string name = entry.Name;
                    if (!nameCounts.ContainsKey(name) && name != "")
                    {
                        nameCounts[name] = 1;
                    }
                    else
                    {
                        ambiguous = true;
                        nameCounts[name]++;
                        entry.Name = $"{className}{name} {{{nameCounts[name]}}}";
                    }
                }
            }
            while (ambiguous);
        }

        internal static string ReambiguateName(string name)
        {
            return Regex.Replace(name, @" \{\d+\}", "");
        }

        internal static string FindName<T>(List<T> list, int index) where T : IMsbEntry
        {
            if (index == -1)
                return null;
            else if (index >= list.Count)
                return null;
            else
                return list[index].Name;
        }

        internal static string[] FindNames<T>(List<T> list, int[] indices) where T : IMsbEntry
        {
            var names = new string[indices.Length];
            for (int i = 0; i < indices.Length; i++)
                names[i] = FindName(list, indices[i]);
            return names;
        }

        internal static string[] FindNames<T>(List<T> list, short[] indices) where T : IMsbEntry
        {
            var names = new string[indices.Length];
            for (int i = 0; i < indices.Length; i++)
                names[i] = FindName(list, indices[i]);
            return names;
        }

        internal static int FindIndex<T>(List<T> list, string name) where T : IMsbEntry
        {
            if (name == null || name == "")
            {
                return -1;
            }
            else
            {
                int result = list.FindIndex(entry => entry.Name == name);
                if (result == -1)
                    throw new KeyNotFoundException($"Name not found: {name}");
                return result;
            }
        }

        internal static int FindIndex<T>(IMsbEntry referrer, List<T> list, string name) where T : IMsbEntry
        {
            if (name == null || name == "")
            {
                return -1;
            }
            else
            {
                int result = list.FindIndex(entry => entry.Name == name);
                if (result == -1)
                {
                    // Fallback case-insensitive check
                    result = list.FindIndex(entry => entry.Name.ToLower() == name.ToLower());
                    if (result == -1)
                    {
                        throw new MissingReferenceException(referrer, name);
                    }
                }
                return result;
            }
        }

        internal static int[] FindIndices<T>(List<T> list, string[] names) where T : IMsbEntry
        {
            var indices = new int[names.Length];
            for (int i = 0; i < names.Length; i++)
                indices[i] = FindIndex(list, names[i]);
            return indices;
        }

        internal static int[] FindIndices<T>(IMsbEntry referrer, List<T> list, string[] names) where T : IMsbEntry
        {
            var indices = new int[names.Length];
            for (int i = 0; i < names.Length; i++)
                indices[i] = FindIndex(referrer, list, names[i]);
            return indices;
        }

        internal static short[] FindShortIndices<T>(IMsbEntry referrer, List<T> list, string[] names) where T : IMsbEntry
        {
            var indices = new short[names.Length];
            for (int i = 0; i < names.Length; i++)
                indices[i] = (short)FindIndex(referrer, list, names[i]);
            return indices;
        }
    }
}
