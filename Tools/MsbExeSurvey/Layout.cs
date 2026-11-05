using SoulsFormats;
using System.Collections;
using System.Numerics;
using System.Reflection;
using System.Text;

/// <summary>
/// Where each SoulsFormats MSBE property is written: an MSB holding one entry is written, a property changed, and
/// the bytes compared. Positions are given in the kinds the flow scan uses (entry header or sub-struct, plus offset).
/// </summary>
public static class Layout
{
    public sealed class Field
    {
        public string Entry;    // e.g. Part.Asset
        public string Kind;     // e.g. Part.Type
        public int Off;
        public int Size;
        public string Path;     // property path
        public string Type;
    }

    public static List<Field> Build()
    {
        var result = new List<Field>();
        foreach (var (group, baseType) in new[] { ("Part", typeof(MSBE.Part)), ("Point", typeof(MSBE.Region)), ("Event", typeof(MSBE.Event)) })
        {
            foreach (var t in baseType.GetNestedTypes(BindingFlags.Public).Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)))
            {
                object entry;
                try { entry = Activator.CreateInstance(t); }
                catch { Console.Error.WriteLine($"cannot create {t.Name}"); continue; }

                byte[] Write()
                {
                    var msb = new MSBE();
                    switch (entry)
                    {
                        case MSBE.Part p: msb.Parts.Add(p); break;
                        case MSBE.Region r: msb.Regions.Add(r); break;
                        case MSBE.Event e: msb.Events.Add(e); break;
                    }
                    return msb.Write();
                }

                byte[] baseline;
                try { baseline = Write(); }
                catch (Exception ex) { Console.Error.WriteLine($"cannot write {t.Name}: {ex.Message}"); continue; }
                var subs = SubStructs(baseline, group);

                foreach (var (path, get, set, type) in Leaves(entry, t, ""))
                {
                    var old = get();
                    var changed = Mutate(old, type);
                    if (changed == null) continue;
                    try
                    {
                        set(changed);
                        var bytes = Write();
                        set(old);
                        if (bytes.Length != baseline.Length) continue;
                        int first = -1, last = -1;
                        for (var i = 0; i < bytes.Length; i++)
                            if (bytes[i] != baseline[i]) { if (first < 0) first = i; last = i; }
                        if (first < 0) continue;
                        var (kind, start) = subs.Where(s => s.Start <= first).OrderByDescending(s => s.Start).First();
                        result.Add(new Field
                        {
                            Entry = $"{group}.{t.Name}", Kind = kind, Off = first - start, Size = SizeOf(type, last - first + 1),
                            Path = path, Type = type.Name,
                        });
                    }
                    catch (Exception ex) { set(old); Console.Error.WriteLine($"{t.Name}.{path}: {ex.Message}"); }
                }
            }
        }
        return result;
    }

    static int SizeOf(Type t, int diff)
    {
        if (t.IsEnum) t = Enum.GetUnderlyingType(t);
        if (t == typeof(byte) || t == typeof(sbyte) || t == typeof(bool)) return 1;
        if (t == typeof(short) || t == typeof(ushort)) return 2;
        if (t == typeof(int) || t == typeof(uint) || t == typeof(float)) return 4;
        if (t == typeof(long) || t == typeof(ulong) || t == typeof(double)) return 8;
        return diff;
    }

    static object Mutate(object v, Type t)
    {
        if (t.IsEnum)
        {
            var u = Convert.ToInt64(v);
            return Enum.ToObject(t, u ^ 1);
        }
        return v switch
        {
            bool b => !b,
            byte b => (byte)(b ^ 0x5A),
            sbyte b => (sbyte)(b ^ 0x5A),
            short s => (short)(s ^ 0x5A5),
            ushort s => (ushort)(s ^ 0x5A5),
            int i => i ^ 0x5A5A5,
            uint i => i ^ 0x5A5A5u,
            float f => BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(f) ^ 0x00400001),
            long l => l ^ 0x5A5A5,
            _ => null,
        };
    }

    /// <summary>
    /// Every settable scalar reachable from the entry: its properties, array elements, vector components and the
    /// properties of the nested struct objects
    /// </summary>
    static IEnumerable<(string Path, Func<object> Get, Action<object> Set, Type Type)> Leaves(object obj, Type t, string prefix)
    {
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;
            var pt = p.PropertyType;
            var name = prefix + p.Name;
            if (pt == typeof(string)) continue;

            if (pt.IsPrimitive || pt.IsEnum)
            {
                if (!p.CanWrite || p.SetMethod == null || !p.SetMethod.IsPublic) continue;
                yield return (name, () => p.GetValue(obj), v => p.SetValue(obj, v), pt);
            }
            else if (pt == typeof(Vector3))
            {
                if (!p.CanWrite) continue;
                foreach (var c in new[] { "X", "Y", "Z" })
                {
                    var cc = c;
                    yield return ($"{name}.{c}",
                        () => { var v = (Vector3)p.GetValue(obj); return cc == "X" ? v.X : cc == "Y" ? v.Y : v.Z; },
                        x => { var v = (Vector3)p.GetValue(obj); if (cc == "X") v.X = (float)x; else if (cc == "Y") v.Y = (float)x; else v.Z = (float)x; p.SetValue(obj, v); },
                        typeof(float));
                }
            }
            else if (pt.IsArray && (pt.GetElementType().IsPrimitive || pt.GetElementType().IsEnum))
            {
                if (p.GetValue(obj) is not Array arr) continue;
                var et = pt.GetElementType();
                for (var i = 0; i < arr.Length; i++)
                {
                    var ii = i;
                    yield return ($"{name}[{i}]", () => arr.GetValue(ii), v => arr.SetValue(v, ii), et);
                }
            }
            else if (pt.IsClass && pt.Namespace == "SoulsFormats" && !typeof(IEnumerable).IsAssignableFrom(pt) && prefix.Count(c => c == '.') < 2)
            {
                var child = p.GetValue(obj);
                if (child == null) continue;
                foreach (var leaf in Leaves(child, child.GetType(), name + "."))
                    yield return leaf;
            }
        }
    }

    /// <summary>
    /// The entry header and each sub-struct of the one entry in the written MSB, by file position
    /// </summary>
    static List<(string Kind, int Start)> SubStructs(byte[] b, string group)
    {
        long R64(long at) => BitConverter.ToInt64(b, (int)at);
        // params: models, events, points, routes, layers, parts
        var param = 0x10L;
        var index = group switch { "Event" => 1, "Point" => 2, _ => 5 };
        for (var i = 0; i < index; i++)
        {
            var count = BitConverter.ToInt32(b, (int)param + 4);
            param = R64(param + 8 + 8 * count);
        }
        var entry = R64(param + 0x10);
        var list = new List<(string, int)> { (group, (int)entry) };
        void Sub(string kind, int field)
        {
            var o = R64(entry + field);
            if (o != 0) list.Add((kind, (int)(entry + o)));
        }

        switch (group)
        {
            case "Part":
                Sub("Part.Display", 0x50); Sub("Part.DisplayGroup", 0x58); Sub("Part.Entity", 0x60); Sub("Part.Type", 0x68);
                Sub("Part.Gparam", 0x70); Sub("Part.SceneGparam", 0x78); Sub("Part.Grass", 0x80); Sub("Part.Unk8", 0x88);
                Sub("Part.Unk9", 0x90); Sub("Part.TileLoad", 0x98); Sub("Part.Unk11", 0xA0);
                // the asset and enemy type data hold offsets of their own, from the type data
                var td = entry + R64(entry + 0x68);
                var type = BitConverter.ToInt32(b, (int)entry + 0xC);
                if (type == 13)
                    foreach (var f in new[] { 0x68, 0x70, 0x78, 0x80 })
                        list.Add(($"Part.Type.{f:X}", (int)(td + R64(td + f))));
                if (type == 2 || type == 10)
                    list.Add(("Part.Type.78", (int)(td + R64(td + 0x78))));
                break;
            case "Point":
                Sub("Point.A", 0x30); Sub("Point.B", 0x38); Sub("Point.Shape", 0x48); Sub("Point.Entity", 0x50);
                Sub("Point.Type", 0x58); Sub("Point.Unk4", 0x60);
                break;
            case "Event":
                Sub("Event.Entity", 0x18); Sub("Event.Type", 0x20); Sub("Event.Unk3", 0x28);
                break;
        }
        return list;
    }

    public static void Dump(string file)
    {
        var sb = new StringBuilder();
        foreach (var f in Build())
            sb.AppendLine($"{f.Entry}\t{f.Kind}\t0x{f.Off:X}\t{f.Size}\t{f.Path}\t{f.Type}");
        File.WriteAllText(file, sb.ToString());
    }
}
