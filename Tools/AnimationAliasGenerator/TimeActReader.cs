using Andre.Core;
using Andre.IO.VFS;
using SoulsFormats;

/// <summary>
/// Reads the names of the animations in a character's time acts, as the Time Act Editor shows them.
/// Only these need an alias, as they are what the animation list holds.
/// </summary>
public static class TimeActReader
{
    public static int CategorySize(Game game) => game is Game.DES or Game.DS1 or Game.DS1R ? 10000 : 1000000;

    public static string FormatName(Game game, long id)
    {
        var size = CategorySize(game);

        return size == 10000 ? $"a{id / size:D2}_{id % size:D4}" : $"a{id / size:D3}_{id % size:D6}";
    }

    /// <summary>
    /// Returns null if the character has no anibnd, or it holds no time acts
    /// </summary>
    public static HashSet<string> ReadAnimationNames(VirtualFileSystem fs, Game game, string behaviorBinderPath, string character)
    {
        var folder = Path.GetDirectoryName(behaviorBinderPath)?.Replace('\\', '/') ?? "/chr";

        var path = new[] { $"{folder}/{character}.anibnd.dcx", $"{folder}/{character}.anibnd" }
            .FirstOrDefault(fs.FileExists);

        if (path == null)
            return null;

        BND4 bnd;

        try
        {
            var raw = fs.ReadFile(path).Value.ToArray();

            bnd = BND4.Read(DCX.Is(raw) ? DCX.Decompress(raw).ToArray() : raw);
        }
        catch
        {
            return null;
        }

        // Binders with a version marker moved their time acts
        var firstID = bnd.Files.Any(e => e.ID == 9999999) ? 5000000 : 3000000;

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var size = CategorySize(game);
        var found = false;

        foreach (var file in bnd.Files)
        {
            if (!file.Name.Contains(".tae", StringComparison.OrdinalIgnoreCase) || file.Bytes.Length < 4)
                continue;

            TAE tae;

            try
            {
                tae = TAE.Read(file.Bytes);
            }
            catch
            {
                continue;
            }

            found = true;

            var bindIndex = file.ID % firstID;

            foreach (var animation in tae.Animations)
            {
                var id = animation.ID >= size ? animation.ID : (long)bindIndex * size + animation.ID;

                names.Add(FormatName(game, id));
            }
        }

        return found ? names : null;
    }
}
