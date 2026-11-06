using Andre.Core;
using SoulsFormats;
using System.Diagnostics;

/// <summary>
/// The compendium held by the container a file came from, raw and parsed per library (parsed on first use).
/// </summary>
public sealed class ContainerContext
{
    public Game Game;
    public byte[] Compendium;
    public string CompendiumSdk = "";

    private readonly object _lock = new();
    private readonly Dictionary<string, object> _parsed = new();
    private readonly Dictionary<string, string> _errors = new();

    /// <summary>
    /// The compendium parsed by <paramref name="parse"/>, cached under <paramref name="key"/>. Throws the parse error
    /// again on later calls.
    /// </summary>
    public T Get<T>(string key, Func<byte[], T> parse) where T : class
    {
        if (Compendium == null)
            return null;
        lock (_lock)
        {
            if (_parsed.TryGetValue(key, out var v)) return (T)v;
            if (_errors.TryGetValue(key, out var err)) throw new InvalidDataException("compendium: " + err);
            try
            {
                var p = parse(Compendium);
                _parsed[key] = p;
                return p;
            }
            catch (Exception e)
            {
                _errors[key] = e.Message;
                throw new InvalidDataException("compendium: " + e.Message);
            }
        }
    }
}

public sealed class LibResult
{
    public string Lib;
    public bool Read;
    public string ReadError;
    public double ReadMs;
    public bool WriteSupported;
    public bool Write;
    public string WriteError;
    public double WriteMs;
    public bool ByteIdentical;
    public long FirstByteDiff = -1;
    public int SizeDelta;
    public bool Reread;
    public string RereadError;
    public bool GraphCompared;
    public string GraphDiff;     // null when equal
    public bool FixedPoint;      // write(reread) == first write
    [NonSerialized] public object Root;
    [NonSerialized] public byte[] Written;

    // Unified engine vs the HKLib serializer it replaces (set on unified results only)
    public string ParityWith;        // the HKLib library compared against
    public bool ParityCompared;
    public string ParityGraphDiff;   // unified read vs HKLib read; null when equal
    public bool? ParityBytesEqual;   // unified write vs HKLib write
    public string SectionDiff;       // unified write vs original, by section, when not byte-identical
    public string MeshDecodeDiff;    // hk2014 compressed mesh triangles vs HKX2's; null when equal
    public int MeshTriangles;        // triangles compared
}

/// <summary>
/// One of the current libraries, wrapped so every file goes through the same read → write → re-read → compare steps.
/// </summary>
public abstract class Library
{
    public abstract string Name { get; }
    public abstract bool Applies(SniffInfo sniff, Game game);
    public virtual bool CanWrite => true;
    public virtual bool CanGraphCompare => true;
    public abstract object Read(byte[] data, ContainerContext ctx, bool comp);
    public virtual byte[] Write(object root, ContainerContext ctx, bool comp) => throw new NotSupportedException();

    public LibResult Run(byte[] data, ContainerContext ctx, bool comp)
    {
        var r = new LibResult { Lib = Name, WriteSupported = CanWrite };
        var sw = Stopwatch.StartNew();
        try
        {
            r.Root = Read(data, ctx, comp);
            r.Read = r.Root != null;
            if (!r.Read) r.ReadError = "returned null";
        }
        catch (Exception e)
        {
            r.ReadError = Describe(e);
        }
        r.ReadMs = sw.Elapsed.TotalMilliseconds;
        if (!r.Read || !CanWrite)
            return r;

        byte[] written;
        sw.Restart();
        try
        {
            written = Write(r.Root, ctx, comp);
            r.Write = true;
            r.Written = written;
        }
        catch (Exception e)
        {
            r.WriteError = Describe(e);
            return r;
        }
        r.WriteMs = sw.Elapsed.TotalMilliseconds;

        r.SizeDelta = written.Length - data.Length;
        r.FirstByteDiff = FirstDiff(data, written);
        r.ByteIdentical = r.FirstByteDiff < 0;

        object reread;
        try
        {
            reread = Read(written, ctx, comp);
            r.Reread = reread != null;
        }
        catch (Exception e)
        {
            r.RereadError = Describe(e);
            return r;
        }

        if (CanGraphCompare)
        {
            r.GraphCompared = true;
            try { r.GraphDiff = GraphCompare.Compare(r.Root, reread); }
            catch (Exception e) { r.GraphDiff = "compare threw: " + Describe(e); }
        }

        try { r.FixedPoint = FirstDiff(written, Write(reread, ctx, comp)) < 0; }
        catch { r.FixedPoint = false; }

        return r;
    }

    private static long FirstDiff(byte[] a, byte[] b)
    {
        var n = Math.Min(a.Length, b.Length);
        var i = a.AsSpan(0, n).CommonPrefixLength(b.AsSpan(0, n));
        if (i < n) return i;
        return a.Length == b.Length ? -1 : n;
    }

    /// <summary>
    /// Exception type, message and the innermost frame in a Havok library, so failures group by cause.
    /// </summary>
    public static string Describe(Exception e)
    {
        while (e is AggregateException or System.Reflection.TargetInvocationException && e.InnerException != null)
            e = e.InnerException;
        var frame = new StackTrace(e, false).GetFrames()?
            .Select(f => f.GetMethod())
            .FirstOrDefault(m => m?.DeclaringType?.Namespace is { } ns && (ns.StartsWith("HKLib") || ns.StartsWith("HKX2") || ns == "SoulsFormats"));
        var where = frame == null ? "" : $" @ {frame.DeclaringType.Name}.{frame.Name}";
        var msg = e.Message.Length > 160 ? e.Message[..160] : e.Message;
        return $"{e.GetType().Name}: {msg}{where}";
    }
}

public sealed class SoulsFormatsHkx : Library
{
    public override string Name => "SoulsFormats HKX";
    public override bool Applies(SniffInfo s, Game game) => s.Container == "Packfile";
    public override bool CanGraphCompare => false;

    private static HKX.HKXVariation Variation(Game game) => game switch
    {
        Game.DES => HKX.HKXVariation.HKXDeS,
        Game.DS1 or Game.DS1R => HKX.HKXVariation.HKXDS1,
        Game.BB => HKX.HKXVariation.HKXBloodBorne,
        Game.DS2 or Game.DS2S => HKX.HKXVariation.HKXDS2,
        _ => HKX.HKXVariation.HKXDS3
    };

    public override object Read(byte[] data, ContainerContext ctx, bool comp) => HKX.Read(data, Variation(ctx.Game));

    public override byte[] Write(object root, ContainerContext ctx, bool comp)
    {
        var hkx = (HKX)root;
        hkx.Compression = DCX.Type.None;
        return hkx.Write();
    }
}

// HKX2 (packfile and tagfile) and HKLib (2018, 2019) were run here as the libraries to match until Phase 7 retired them;
// their adapters are in .archive/Havok Phase 7/HavokLibSurvey.

/// <summary>
/// The unified tagfile engine (Havok.Formats) reading into a generated class set, with one dialect.
/// </summary>
public sealed class UnifiedTagfile : Library
{
    private readonly string _name;
    private readonly string _sdkPrefix;
    private readonly Havok.Formats.Tagfile.TagfileDialect _dialect;

    private readonly Func<Havok.HavokProfile, Havok.Formats.IHavokClassSet> _classSet;

    /// <param name="classSet">The generated class set to read into</param>
    public UnifiedTagfile(string name, string sdkPrefix, Havok.Formats.Tagfile.TagfileDialect dialect, string parityWith,
        Func<Havok.HavokProfile, Havok.Formats.IHavokClassSet> classSet = null)
    {
        _name = name;
        _sdkPrefix = sdkPrefix;
        _dialect = dialect;
        ParityWith = parityWith;
        _classSet = classSet;
    }

    public override string Name => _name;

    /// <summary>
    /// The library this one must match, if any (none since Phase 7).
    /// </summary>
    public string ParityWith { get; }

    /// <summary>
    /// Reads into a generated class set, so a parity check compares graphs across class sets.
    /// </summary>
    public bool CrossClassSet => true;

    public override bool Applies(SniffInfo s, Game game) => s.Container == "Tagfile" && s.Sdk.StartsWith(_sdkPrefix);

    private Havok.Formats.Tagfile.HavokTagfileSerializer New(Havok.HavokProfile profile) =>
        new(_classSet(profile), _dialect);

    private Havok.Formats.Tagfile.HavokTagfileSerializer Create(ContainerContext ctx, bool useComp)
    {
        var profile = Havok.HavokProfile.FromGame(ctx.Game.ToString())
                      ?? throw new InvalidOperationException($"No profile for {ctx.Game}");
        var s = New(profile);
        if (useComp)
        {
            // keyed by SDK and class set: the compendium's types come from the registry of the class set that parsed it
            var compendium = ctx.Get("unified" + _sdkPrefix + "generated", bytes =>
                New(profile).ReadCompendium(bytes));
            if (compendium != null) s.LoadCompendium(compendium);
        }
        return s;
    }

    public override object Read(byte[] data, ContainerContext ctx, bool comp) => Create(ctx, comp).Read(data);

    public override byte[] Write(object root, ContainerContext ctx, bool comp) => Create(ctx, comp).Write(root);
}

/// <summary>
/// The unified packfile engine (Havok.Formats) reading hk_2014 packfiles into the generated Havok.hk2014 classes.
/// Parity is with HKX2's packfile reader and writer.
/// </summary>
public sealed class GeneratedPackfile : Library
{
    private readonly bool _preserveSource;

    /// <param name="preserveSource">Write back the header and __types__ section read (the default), or HKX2's</param>
    public GeneratedPackfile(bool preserveSource = true)
    {
        _preserveSource = preserveSource;
    }

    public override string Name => _preserveSource ? "Generated 2014 packfile" : "Generated 2014 packfile (HKX2 header)";

    public string ParityWith => null; // HKX2 packfile until Phase 7

    public override bool Applies(SniffInfo s, Game game) => s.Container == "Packfile" && s.Sdk.StartsWith("hk_2014");

    private Havok.Formats.Packfile.HavokPackfileSerializer Create(ContainerContext ctx)
    {
        var profile = Havok.HavokProfile.FromGame(ctx.Game.ToString()) is { Sdk: Havok.HavokSdk.Hk2014 } p ? p : Havok.HavokProfile.DS3;
        return new Havok.Formats.Packfile.HavokPackfileSerializer(Havok.hk2014.ClassSet.For(profile)) { PreserveSource = _preserveSource };
    }

    public override object Read(byte[] data, ContainerContext ctx, bool comp) => Create(ctx).Read(data);

    public override byte[] Write(object root, ContainerContext ctx, bool comp) => Create(ctx).Write(root);
}

/// <summary>
/// The unified packfile engine reading the 32-bit packfiles (DS1 hk_2010 into Havok.hk2010; DeS Havok-5.5 and 5.1 into
/// Havok.hk55) with registries built from the games' own hkClass reflection. Parity is with SoulsFormats' HKX.
/// </summary>
public sealed class GeneratedPackfile32 : Library
{
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, Havok.HavokProfile> Profiles = new();

    public override string Name => "Generated 32-bit packfile";

    public override bool Applies(SniffInfo s, Game game) =>
        s.Container == "Packfile" && (s.Sdk.StartsWith("hk_2010") || s.Sdk.StartsWith("Havok-5.5") || s.Sdk.StartsWith("Havok-5.1"));

    private static Havok.Formats.Packfile.HavokPackfileSerializer Create(Havok.HavokProfile profile) =>
        new(profile.Sdk == Havok.HavokSdk.Hk2010 ? Havok.hk2010.ClassSet.For(profile) : Havok.hk55.ClassSet.For(profile));

    public override object Read(byte[] data, ContainerContext ctx, bool comp)
    {
        string sdk = System.Text.Encoding.ASCII.GetString(data, 0x28, 16).TrimEnd('\0', '\xFF');
        var profile = Havok.HavokProfile.For(ctx.Game.ToString(), sdk)
                      ?? throw new InvalidOperationException($"No profile for {ctx.Game}");
        object root = Create(profile).Read(data);
        Profiles.AddOrUpdate(root, profile);
        return root;
    }

    public override byte[] Write(object root, ContainerContext ctx, bool comp) =>
        Create(Profiles.TryGetValue(root, out var p) ? p : Havok.HavokProfile.FromGame(ctx.Game.ToString())!).Write(root);
}
