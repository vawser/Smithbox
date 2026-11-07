using Andre.IO.VFS;
using SoulsFormats;
using System.Text.RegularExpressions;

// emevdnav : every use of 2003[13] Modify Navimesh Connection Bitflag (entity, type bit, bit op) in the game's EMEVD,
// and what its entity ID names in the NVAs (node banks, navmeshes)
static class EmevdNav
{
    public static void Run(VirtualFileSystem fs, string dict, string[] args)
    {
        var events = Regex.Matches(dict, @"""(/event/[^""]+\.emevd\.dcx)""").Select(m => m.Groups[1].Value).Distinct().ToList();
        var uses = new List<(string File, int Event, int Entity, int Type, int Op)>();

        foreach (var path in events)
        {
            var f = fs.ReadFile(path);
            if (f == null) continue;
            EMEVD emevd;
            try { emevd = EMEVD.Read(f.Value.ToArray()); } catch { continue; }

            foreach (var ev in emevd.Events)
                foreach (var ins in ev.Instructions.Where(i => i.Bank == 2003 && i.ID == 13))
                {
                    var a = ins.ArgData;
                    uses.Add((Path.GetFileName(path), (int)ev.ID, BitConverter.ToInt32(a, 0), BitConverter.ToInt32(a, 4), a.Length > 8 ? a[8] : -1));
                }
        }

        Console.WriteLine($"{uses.Count} uses in {uses.Select(u => u.File).Distinct().Count()} files");

        // The events that use it, instruction by instruction, to see when a bank is changed
        if (args.Length > 1 && args[1] == "events")
        {
            foreach (var path in events.Where(p => uses.Any(u => u.File == Path.GetFileName(p))))
            {
                var emevd = EMEVD.Read(fs.ReadFile(path).Value.ToArray());
                foreach (var ev in emevd.Events.Where(e => e.Instructions.Any(i => i.Bank == 2003 && i.ID == 13)))
                {
                    Console.WriteLine($"== {Path.GetFileName(path)} event {ev.ID} ({ev.Parameters.Count} parameters)");
                    foreach (var ins in ev.Instructions)
                    {
                        var ints = Enumerable.Range(0, ins.ArgData.Length / 4).Select(k => BitConverter.ToInt32(ins.ArgData, k * 4));
                        Console.WriteLine($"   {ins.Bank}[{ins.ID:D2}] {string.Join(" ", ints)}");
                    }
                }
            }
            return;
        }
        foreach (var g in uses.GroupBy(u => u.Type).OrderBy(g => g.Key))
            Console.WriteLine($"  type {g.Key} (0x{g.Key:X}): {g.Count()} uses, ops {string.Join(",", g.Select(u => u.Op).Distinct().OrderBy(x => x))}");

        // Literal entity IDs (event parameters show as 0 here) against the NVAs' node banks and navmesh NameIDs
        var literal = uses.Where(u => u.Entity > 0).Select(u => u.Entity).ToHashSet();
        Console.WriteLine($"{literal.Count} literal entity IDs, e.g. {string.Join(" ", literal.Take(8))}");

        var maps = Regex.Matches(dict, @"""/map/m\d\d/(m\d\d_\d\d_\d\d_\d\d)/\1\.nva\.dcx""").Select(m => m.Groups[1].Value).Distinct().ToList();
        var banks = new Dictionary<int, List<(string Map, NVA_ER.NodeBank Bank)>>();
        int bankCount = 0, bankWithEntity = 0;
        foreach (var map in maps)
        {
            var f = fs.ReadFile(StudioCore.Editors.MapEditor.NavigationReader.NvaPath(map));
            if (f == null) continue;
            var nva = NVA_ER.Read(f.Value.ToArray());
            foreach (var bank in nva.NodeBanks ?? new())
            {
                bankCount++;
                if (bank.EntityID > 0) { bankWithEntity++; (banks.TryGetValue(bank.EntityID, out var l) ? l : banks[bank.EntityID] = new()).Add((map, bank)); }
            }
        }
        Console.WriteLine($"node banks: {bankCount}, {bankWithEntity} with an entity ID");
        int found = literal.Count(banks.ContainsKey);
        Console.WriteLine($"literal 2003[13] entity IDs that are node bank entity IDs: {found} of {literal.Count}");
        foreach (var id in literal.Where(banks.ContainsKey).Take(10))
            foreach (var (map, bank) in banks[id])
                Console.WriteLine($"  {id}: {map} bank {bank.BankID}, {bank.Faces.Count} faces, collision IDs {string.Join(",", bank.Faces.Select(x => x.CollisionID).Distinct().Take(4))}");
        foreach (var id in banks.Keys.Where(k => !literal.Contains(k)).Take(5))
            Console.WriteLine($"  node bank entity {id} ({banks[id][0].Map}) isn't a literal 2003[13] argument");

        foreach (var u in uses.Take(12)) Console.WriteLine($"  e.g. {u.File} event {u.Event}: entity {u.Entity}, type {u.Type}, op {u.Op}");

        // The face data of every node bank's faces: the second field of a bank face looks like a navmesh NameID
        var profile = Havok.HavokProfile.FromGame("ER");
        var counts = new Dictionary<string, int>();
        void C(string k) => counts[k] = counts.GetValueOrDefault(k) + 1;
        foreach (var map in maps)
        {
            var f = fs.ReadFile(StudioCore.Editors.MapEditor.NavigationReader.NvaPath(map));
            if (f == null) continue;
            var nva = NVA_ER.Read(f.Value.ToArray());
            if (nva.NodeBanks == null || nva.NodeBanks.Count == 0) continue;
            var nav = StudioCore.Editors.MapEditor.NavigationReader.Read(fs, map, profile, Array.Empty<string>());

            foreach (var bank in nav.Nva.NodeBanks)
                foreach (var face in bank.Faces)
                {
                    var entry = nav.FindNavmesh(face.CollisionID);
                    C(entry == null ? "bank face: second field is NOT a NameID" : $"bank face: NameID of a {StudioCore.Editors.MapEditor.NavigationMap.SetOf(entry)} entry");
                    var mesh = entry == null ? null : nav.FileOf(entry)?.Mesh;
                    if (mesh == null || face.FaceIndex >= mesh.m_faces.Count) { C("bank face: no face"); continue; }
                    uint data = (uint)mesh.m_faceData[face.FaceIndex * mesh.m_faceDataStriding];
                    C($"bank face data {data:X8} ({(bank.EntityID > 0 ? "bank with entity" : "bank without entity")})");
                }
        }
        foreach (var (k, v) in counts.OrderBy(x => x.Key)) Console.WriteLine($"{k}: {v}");
    }
}
