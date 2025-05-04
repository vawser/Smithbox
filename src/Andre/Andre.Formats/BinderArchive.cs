using Andre.Core;
using Andre.Core.Util;
using Andre.Formats.Util;
using DotNext.IO.MemoryMappedFiles;
using SoulsFormats;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.Formats
{
    public class BinderArchive : IDisposable, IAsyncDisposable
    {
        public static int ThreadsForDecryption = Environment.ProcessorCount > 4 ? Environment.ProcessorCount / 2 : 4;

        private BHD5 bhd;
        public FileStream Bdt { get; }

        public MemoryMappedFile BdtMmf { get; }

        public bool BhdWasEncrypted { get; }

        public static readonly string[] DarkSoulsArchiveNames =
        [
            "dvdbnd0",
            "dvdbnd1",
            "dvdbnd2",
            "dvdbnd3",
        ];

        public static readonly string[] DarkSouls2ArchiveNames =
        [
            "GameDataEbl",
            "LqChrEbl",
            "LqMapEbl",
            "LqObjEbl",
            "LqPartsEbl",
        ];

        public static readonly string[] DarkSouls3ArchiveNames =
        [
            "Data1",
            "Data2",
            "Data3",
            "Data4",
            "Data5",
            "DLC1",
            "DLC2",
        ];

        public static readonly string[] SekiroArchiveNames =
        [

            "Data1",
            "Data2",
            "Data3",
            "Data4",
            "Data5",
        ];

        public static readonly string[] EldenRingArchiveNames =
        [
            "Data0",
            "Data1",
            "Data2",
            "Data3",
            "DLC",
            @"sd\sd",
            @"sd\sd_dlc02",
        ];

        public static readonly string[] ArmoredCore6ArchiveNames =
        [
            "Data0",
            "Data1",
            "Data2",
            "Data3",
            @"sd\sd",
        ];

        public static string[] GetArchiveNames(Game game)
            => game switch
            {
                Game.DES => throw new NotImplementedException(),
                Game.DS1 => DarkSoulsArchiveNames,
                Game.DS1R => DarkSoulsArchiveNames,
                Game.DS2S => DarkSouls2ArchiveNames,
                Game.DS3 => DarkSouls3ArchiveNames,
                Game.BB => throw new NotImplementedException(),
                Game.SDT => SekiroArchiveNames,
                Game.ER => EldenRingArchiveNames,
                Game.AC6 => ArmoredCore6ArchiveNames,
                Game.DS2 => DarkSouls2ArchiveNames,
                Game.ERN => throw new NotImplementedException(),
                _ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
            };

        public static IEnumerable<string> FindBHDs(string gameRoot, Game game)
            => GetArchiveNames(game)
                .Select(archive => Path.Combine(gameRoot, archive + (game == Game.DS1 ? ".bhd5" : ".bhd")))
                .Where(File.Exists);

        public BinderArchive(BHD5 bhd, FileStream bdt, MemoryMappedFile bdtMmf, bool wasEncrypted = false)
        {
            this.bhd = bhd;
            this.Bdt = bdt;
            this.BdtMmf = bdtMmf;
            this.BhdWasEncrypted = wasEncrypted;
        }

        public BinderArchive(BHD5 bhd, FileStream bdt, bool wasEncrypted = false)
        {
            this.bhd = bhd;
            Bdt = bdt;
            BhdWasEncrypted = wasEncrypted;
            BdtMmf = MemoryMappedFile.CreateFromFile(bdt, bdt.Name, bdt.Length, MemoryMappedFileAccess.Read,
                HandleInheritability.None, true);
        }

        public static bool IsBhdEncrypted(Memory<byte> bhd)
        {
            string sig = "";
            try
            {
                sig = System.Text.Encoding.ASCII.GetString(bhd.Span[..4]);
            }
            catch
            {
                //assume this means it's encrypted
                return true;
            }

            return sig != "BHD5";
        }

        public static byte[] Decrypt(Memory<byte> encryptedBhd, string bhdPath, Game game)
        {
            return NativeRsa.Decrypt(encryptedBhd, ArchiveKeys.GetKey(bhdPath, game), ThreadsForDecryption);
        }

        public BinderArchive(string bhdPath, string bdtPath, Game game)
        {
            using var file = MemoryMappedFile.CreateFromFile(bhdPath, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);

            if (IsBhdEncrypted(accessor.Memory))
            {
                //encrypted
                var decrypted = Decrypt(accessor.Memory, bhdPath, game);
                bhd = BHD5.Read(decrypted, game.AsBhdGame()!.Value);
                BhdWasEncrypted = true;
            }
            else
            {
                bhd = BHD5.Read(accessor.Memory, game.AsBhdGame()!.Value);
                BhdWasEncrypted = true;
            }
            Bdt = File.OpenRead(bdtPath);
            BdtMmf = MemoryMappedFile.CreateFromFile(Bdt, null, 0, MemoryMappedFileAccess.Read,
                HandleInheritability.None, true);
        }

        public BHD5.FileHeader? TryGetFileFromHash(ulong hash)
        {
            return bhd.Buckets.SelectMany(b => b.Where(f => f.FileNameHash == hash)).FirstOrDefault();
        }

        public byte[] ReadFile(BHD5.FileHeader file) => file.ReadFile(Bdt);

        public byte[]? TryReadFileFromHash(ulong hash) => TryGetFileFromHash(hash)?.ReadFile(Bdt);

        public List<BHD5.Bucket> Buckets => bhd.Buckets;

        public IEnumerable<BHD5.FileHeader> EnumerateFiles() =>
            Buckets.Select(b => b.AsEnumerable()).Aggregate(Enumerable.Concat);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Bdt.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            await Bdt.DisposeAsync();
        }
    }
}