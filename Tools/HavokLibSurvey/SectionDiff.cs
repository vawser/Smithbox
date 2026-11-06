using System.Buffers.Binary;
using System.Text;

/// <summary>
/// Where two tagfiles differ, at the level of sections and index items, for working out why a rewrite isn't
/// byte-identical.
/// </summary>
public static class SectionDiff
{
    public static string Describe(byte[] original, byte[] written)
    {
        var a = Summarise(original);
        var b = Summarise(written);
        var names = TypeNames(original);
        var parts = new List<string>();
        foreach (var key in a.Keys.Union(b.Keys))
        {
            a.TryGetValue(key, out var va);
            b.TryGetValue(key, out var vb);
            if (va == vb) continue;
            // name the type index, from the original's own type table
            var label = key;
            var hash = key.LastIndexOf('#');
            if (hash >= 0 && int.TryParse(key[(hash + 1)..], out var index) && index < names.Count)
                label = $"{key}({names[index]})";
            parts.Add($"{label} {va}→{vb}");
        }
        return parts.Count == 0 ? "sections equal in size" : string.Join(", ", parts);
    }

    /// <summary>
    /// The section tree of a tagfile, e.g. TAG0[SDKV,DATA,TYPE[TPTR,TSTR,…],INDX[ITEM,PTCH]], with the padding byte of
    /// the string sections, to work out an SDK's dialect.
    /// </summary>
    public static string Layout(byte[] d)
    {
        try
        {
            string Walk(int start)
            {
                uint header = BinaryPrimitives.ReadUInt32BigEndian(d.AsSpan(start));
                int size = (int)(header & 0x3FFFFFFF);
                string id = Encoding.ASCII.GetString(d, start + 4, 4);
                if (id is "TSTR" or "TST1" or "FSTR" or "FST1" && size > 8)
                    id += $"(pad {d[start + size - 1]:X2})";
                if ((header & 0x40000000) != 0) return id;
                var children = new List<string>();
                for (int pos = start + 8; pos + 8 <= start + size;)
                {
                    int childSize = (int)(BinaryPrimitives.ReadUInt32BigEndian(d.AsSpan(pos)) & 0x3FFFFFFF);
                    if (childSize < 8) break;
                    children.Add(Walk(pos));
                    pos += childSize;
                }
                return children.Count == 0 ? id : $"{id}[{string.Join(",", children)}]";
            }
            return Walk(0);
        }
        catch
        {
            return "<unreadable>";
        }
    }

    /// <summary>
    /// The type names by index from the file's TYPE section (TSTR/TST1 + TNA1); empty if it refers to a compendium.
    /// </summary>
    private static List<string> TypeNames(byte[] d)
    {
        var names = new List<string>();
        try
        {
            var reader = new Havok.Formats.Tagfile.TagfileReader(d);
            reader.EnterSection("TAG0");
            while (reader.Position < reader.GetSectionEnd())
            {
                var id = reader.GetSectionId();
                if (id != "TYPE") { reader.EnterSection(id); reader.SkipSection(); continue; }
                reader.EnterSection("TYPE");
                var strings = new List<string>();
                while (reader.Position < reader.GetSectionEnd())
                {
                    var child = reader.GetSectionId();
                    reader.EnterSection(child);
                    if (child is "TSTR" or "TST1")
                    {
                        while (reader.Position < reader.GetSectionEnd() && reader.GetByte(reader.Position) != 0xFF)
                            strings.Add(reader.ReadASCII());
                    }
                    else if (child is "TNA1" or "TNAM")
                    {
                        var count = (int)reader.ReadHavokVarUInt();
                        names.Add("NULL");
                        for (int i = 1; i < count; i++)
                        {
                            var name = strings[(int)reader.ReadHavokVarUInt()];
                            var parameters = (int)reader.ReadHavokVarUInt();
                            var args = new List<string>();
                            for (int j = 0; j < parameters; j++)
                            {
                                var param = strings[(int)reader.ReadHavokVarUInt()];
                                var value = reader.ReadHavokVarUInt();
                                args.Add(param[0] == 't' ? $"#{value}" : value.ToString());
                            }
                            names.Add(args.Count == 0 ? name : $"{name}<{string.Join(",", args)}>");
                        }
                    }
                    reader.SkipSection();
                }
                return names;
            }
        }
        catch { }
        return names;
    }

    /// <summary>
    /// Section sizes by path (TAG0/DATA, TAG0/TYPE/TST1…), plus ITEM counts by kind and the number of distinct
    /// strings / string items.
    /// </summary>
    private static Dictionary<string, long> Summarise(byte[] d)
    {
        var map = new Dictionary<string, long>();
        void Walk(int start, string path)
        {
            int size = (int)(BinaryPrimitives.ReadUInt32BigEndian(d.AsSpan(start)) & 0x3FFFFFFF);
            string id = Encoding.ASCII.GetString(d, start + 4, 4);
            string p = path.Length == 0 ? id : path + "/" + id;
            map[p] = size;
            bool hasChildren = (BinaryPrimitives.ReadUInt32BigEndian(d.AsSpan(start)) & 0x40000000) == 0;
            if (id == "ITEM")
            {
                int pointers = 0, arrays = 0;
                for (int pos = start + 8; pos + 12 <= start + size; pos += 12)
                {
                    uint kind = BinaryPrimitives.ReadUInt32LittleEndian(d.AsSpan(pos)) >> 28;
                    if (kind == 1) pointers++; else if (kind == 2) arrays++;
                }
                map[p + ":pointers"] = pointers;
                map[p + ":arrays"] = arrays;
                for (int pos = start + 8; pos + 12 <= start + size; pos += 12)
                {
                    uint word = BinaryPrimitives.ReadUInt32LittleEndian(d.AsSpan(pos));
                    string k = $"{p}:{((word >> 28) == 1 ? "ptr" : "arr")}#{word & 0xFFFFFF}";
                    map[k] = map.GetValueOrDefault(k) + 1;
                }
            }
            if (id == "PTCH")
            {
                for (int pos = start + 8; pos + 8 <= start + size;)
                {
                    int type = BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(pos));
                    int count = BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(pos + 4));
                    map[$"{p}#{type}"] = count;
                    pos += 8 + count * 4;
                }
            }
            if (!hasChildren) return;
            for (int pos = start + 8; pos + 8 <= start + size;)
            {
                int childSize = (int)(BinaryPrimitives.ReadUInt32BigEndian(d.AsSpan(pos)) & 0x3FFFFFFF);
                if (childSize < 8) break;
                Walk(pos, p);
                pos += childSize;
            }
        }
        try { Walk(0, ""); } catch { map["<unreadable>"] = 1; }
        return map;
    }
}
