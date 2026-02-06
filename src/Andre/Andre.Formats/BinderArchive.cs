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
        private static int threadsForDecryption =
            Environment.ProcessorCount > 4 ? Environment.ProcessorCount / 2 : 4;

        public static int ThreadsForDecryption
        {
            get => threadsForDecryption;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Thread count must be >= 1.");
                threadsForDecryption = value;
            }
        }

        private BHD5 bhd;
        public FileStream? BdtStream { get; }

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
            Path.Join("sd", "sd"),
            Path.Join("sd", "sd_dlc02"),
        ];

        public static readonly string[] ArmoredCore6ArchiveNames =
        [
            "Data0",
            "Data1",
            "Data2",
            "Data3",
            Path.Join("sd", "sd"),
        ];

        public static readonly string[] NightreignArchiveNames =
        [
            "data0",
            "data1",
            "data2",
            "data3",
            "dlc01",
            Path.Join("sd", "sd"),
            Path.Join("sd", "sd_dlc01"),
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
                Game.NR => NightreignArchiveNames,
                _ => throw new ArgumentOutOfRangeException(nameof(game), game, null)
            };

        public static IEnumerable<string> FindBHDs(string gameRoot, Game game)
            => GetArchiveNames(game)
                .Select(archive => Path.Combine(gameRoot, archive + (game == Game.DS1 ? ".bhd5" : ".bhd")))
                .Where(File.Exists);
        
        /// <summary>
        /// Decrypts a BHD file using a native library. Fast, but not available on all platforms.
        /// </summary>
        /// <param name="encryptedBhd"></param>
        /// <param name="bhdPath"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static byte[] DecryptNative(Memory<byte> encryptedBhd, string bhdPath, Game game)
        {
            return NativeRsa.Decrypt(encryptedBhd, ArchiveKeys.GetKey(bhdPath, game), ThreadsForDecryption);
        }

        /// <summary>
        /// Decrypts a BHD file using the slow .NET implementation. Available on all platforms.
        /// </summary>
        /// <param name="bhdPath"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static byte[] DecryptSlow(string bhdPath, Game game)
        {
            return SoulsFormats.Util.CryptographyUtility.DecryptRsaParallel(bhdPath, ArchiveKeys.GetKey(bhdPath, game), ThreadsForDecryption).ToArray();
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
        
        /// <summary>
        /// Creates a new BinderArchive from a BHD5 and a FileStream.
        ///
        /// Does not take ownership of bdtStream.
        /// </summary>
        /// <param name="bhd"></param>
        /// <param name="bdtStream"></param>
        /// <param name="wasEncrypted"></param>
        public BinderArchive(BHD5 bhd, FileStream bdtStream, bool wasEncrypted = false)
        {
            this.bhd = bhd;
            BdtStream = bdtStream;
            BhdWasEncrypted = wasEncrypted;
            BdtMmf = MemoryMappedFile.CreateFromFile(bdtStream, bdtStream.Name, bdtStream.Length, MemoryMappedFileAccess.Read,
                HandleInheritability.None, true);
        }

        /// <summary>
        /// Creates a new BinderArchive from paths to a BHD and BDT file.
        ///
        /// This constructor performs decryption if necessary, which may be slow.
        /// </summary>
        /// <param name="bhdPath"></param>
        /// <param name="bdtPath"></param>
        /// <param name="game"></param>
        public BinderArchive(string bhdPath, string bdtPath, Game game)
        {

            using var fs = new FileStream(bhdPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var mmf = MemoryMappedFile.CreateFromFile(fs, null, 0, MemoryMappedFileAccess.Read,
                HandleInheritability.None, leaveOpen: false);
            using var accessor = mmf.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);

            BHD5.Game bhdGame = game.AsBhdGame()
                                ?? throw new ArgumentException($"Game {game} cannot be converted to a BHD game.", nameof(game));
            
            if (IsBhdEncrypted(accessor.Memory))
            {
                //encrypted
#if DEBUG
                Console.WriteLine($"Decrypting {Path.GetFileName(bhdPath)}");
#endif
                byte[] decrypted;
                try {
                    decrypted = DecryptNative(accessor.Memory, bhdPath, game);
                }
                catch {
                    decrypted = DecryptSlow(bhdPath, game);
                }
                bhd = BHD5.Read(decrypted, bhdGame);
                BhdWasEncrypted = true;
            }
            else
            {
                bhd = BHD5.Read(accessor.Memory, bhdGame);
                BhdWasEncrypted = true;
            }
            BdtStream = File.OpenRead(bdtPath);
            BdtMmf = MemoryMappedFile.CreateFromFile(BdtStream, null, 0, MemoryMappedFileAccess.Read,
                HandleInheritability.None, true);
        }
        
        public BHD5.FileHeader? TryGetFileFromHash(ulong hash)
        {
            return bhd.Buckets.SelectMany(b => b.Where(f => f.FileNameHash == hash)).FirstOrDefault();
        }

        public byte[] ReadFile(BHD5.FileHeader file) => file.ReadFile(BdtStream);

        public byte[]? TryReadFileFromHash(ulong hash) => TryGetFileFromHash(hash)?.ReadFile(BdtStream);

        public List<BHD5.Bucket> Buckets => bhd.Buckets;

        public IEnumerable<BHD5.FileHeader> EnumerateFiles() =>
            Buckets.Select(b => b.AsEnumerable()).Aggregate(Enumerable.Concat);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            BdtMmf.Dispose();
            BdtStream?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            BdtMmf.Dispose();
            if (BdtStream != null) await BdtStream.DisposeAsync();
        }
    }
}