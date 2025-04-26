using SoulsFormats;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.Core.Util
{
    public partial class BhdDictionary
    {
        private const uint PRIME = 37;
        private const ulong PRIME64 = 0x85ul;

        private readonly Dictionary<ulong, string> hashes;
        private BHD5.Game game;
        public BhdDictionary(string dictionary, BHD5.Game game)
        {
            this.game = game;
            hashes = new Dictionary<ulong, string>();
            foreach (string line in NewlineRegex().Split(dictionary))
            {

                if (line.StartsWith('#'))
                    continue;

                string trimmed = line.Trim();
                if (trimmed.Length <= 0)
                    continue;

                ulong hash = ComputeHash(trimmed, game);
                hashes[hash] = trimmed;
            }
        }

        public static ulong ComputeHash(string path, BHD5.Game game)
        {
            string hashable = path.Trim().Replace('\\', '/').ToLowerInvariant();
            if (!hashable.StartsWith('/'))
                hashable = '/' + hashable;
            return game >= BHD5.Game.EldenRing ? hashable.Aggregate(0ul, (i, c) => i * PRIME64 + c) : hashable.Aggregate(0u, (i, c) => i * PRIME + c);
        }

        public ulong ComputeHash(string path) => ComputeHash(path, game);

        public bool GetPath(ulong hash, [MaybeNullWhen(false)] out string path)
        {
            return hashes.TryGetValue(hash, out path);
        }

        [GeneratedRegex("[\r\n]+")]
        private static partial Regex NewlineRegex();
    }
}