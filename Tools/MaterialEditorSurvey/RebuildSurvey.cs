using SoulsFormats;

/// <summary>
/// rebuild GAME: what MaterialBinderWrapper.SaveDirtyFiles does to a binder, done to each vanilla one: a new
/// file list in order (here with the first material duplicated after itself and the last removed), fresh
/// BinderFile objects numbered 0..n-1 with the source file's flags and compression, written and read back.
/// </summary>
public static class RebuildSurvey
{
    public static void Run(GameMaterials g)
    {
        Console.WriteLine("== rebuild (binder save with a duplicate and a delete)");

        foreach (var b in g.Binders.Where(b => b.Selected && b.Binder != null && b.Binder.Files.Count >= 2))
        {
            try
            {
                var order = b.Binder.Files.Select(f => f.Name).ToList();
                var onDisk = new Dictionary<string, BinderFile>();
                foreach (var f in b.Binder.Files) onDisk.TryAdd(f.Name, f);

                // Duplicate the first file after itself, under a new name; remove the last
                var first = order[0];
                var copyName = first.Insert(first.LastIndexOf('.'), "_copy");
                order.Insert(1, copyName);
                var removed = order[^1];
                order.RemoveAt(order.Count - 1);

                var template = b.Binder.Files[0];
                var files = new List<BinderFile>();

                foreach (var key in order)
                {
                    onDisk.TryGetValue(key, out var source);
                    var bytes = key == copyName ? onDisk[first].Bytes : source.Bytes;
                    files.Add(new BinderFile(source?.Flags ?? template.Flags, files.Count, key, bytes)
                    {
                        CompressionType = source?.CompressionType ?? template.CompressionType
                    });
                }

                IBinder rebuilt = b.Binder is BND3 ? BND3.Read(b.Data) : BND4.Read(b.Data);
                rebuilt.Files = files;

                var written = rebuilt is BND3 b3 ? b3.Write() : ((BND4)rebuilt).Write();
                IBinder back = rebuilt is BND3 ? BND3.Read(written) : BND4.Read(written);

                var namesOk = back.Files.Select(f => f.Name).SequenceEqual(order);
                var idsOk = back.Files.Select((f, i) => f.ID == i).All(x => x);
                var bytesOk = back.Files.All(f => f.Bytes.Span.SequenceEqual((f.Name == copyName ? onDisk[first] : onDisk[f.Name]).Bytes.Span));
                var flagsOk = back.Files.All(f => f.Flags == (f.Name == copyName ? onDisk[first] : onDisk[f.Name]).Flags);
                var parses = back.Files.Count(f =>
                {
                    try { if (b.Kind == MaterialKind.MTD) MTD.Read(f.Bytes); else MATBIN.Read(f.Bytes); return true; }
                    catch { return false; }
                });

                Console.WriteLine($"  {b.Path}: {back.Files.Count} files (was {b.Binder.Files.Count}, +1 -1), " +
                    $"names {Ok(namesOk)}, ids {Ok(idsOk)}, bytes {Ok(bytesOk)}, flags {Ok(flagsOk)}, {parses}/{back.Files.Count} read; " +
                    $"removed {GameMaterials.ShortName(removed)} {Ok(!back.Files.Any(f => f.Name == removed))}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"  {b.Path}: FAILED {e.GetType().Name} {e.Message}");
            }
        }
    }

    private static string Ok(bool ok) => ok ? "ok" : "WRONG";
}
