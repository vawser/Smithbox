using LuaDecompilerCore;
using SoulsFormats;
using StudioCore.Editors.LuaEditor;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using LuaReader = LuaDecompilerCore.Utilities.BinaryReaderEx;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// A character action script (action/script/*.hks) as text, with where each string literal is, which function each
/// line is in, and where each function is defined. Compiled scripts (every vanilla ER, NR and DS3 one but two) are
/// decompiled with the vendored HksDecompiler; source text (modded scripts, DS3 enemycommon.hks) is read as it is.
/// See Claude/Documentation/Havok Behavior HKS Cross-Reference Plan.md.
/// </summary>
public class HavokScript
{
    public string Path { get; init; }

    /// <summary>The file name, e.g. c0000.hks</summary>
    public string Name { get; init; }

    /// <summary>Read from the project's copy (else the game's)</summary>
    public bool FromProject { get; init; }

    /// <summary>Source text in the file (else decompiled bytecode)</summary>
    public bool IsText { get; private set; }

    public volatile bool IsReady;
    public string Error { get; private set; }

    public string[] Lines { get; private set; } = Array.Empty<string>();

    /// <summary>The functions defined, in order, with their line (0-based)</summary>
    public List<(string Name, int Line)> Functions { get; } = new();

    private readonly Dictionary<string, List<int>> Literals = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> Definitions = new(StringComparer.Ordinal);
    private int[] LineFunction = Array.Empty<int>();

    private static readonly Regex LiteralRegex = new(@"""((?:[^""\\]|\\.)*)""|'((?:[^'\\]|\\.)*)'", RegexOptions.Compiled);
    private static readonly Regex DefinitionRegex = new(@"^(\s*)(?:local\s+)?function\s+([A-Za-z_][\w\.:]*)\s*\(|^(\s*)([A-Za-z_][\w\.]*)\s*=\s*function\s*\(", RegexOptions.Compiled);

    /// <summary>The lines (0-based) holding this string as a literal</summary>
    public IReadOnlyList<int> LinesOf(string literal)
        => Literals.TryGetValue(literal, out var lines) ? lines : Array.Empty<int>();

    public bool Names(string literal) => Literals.ContainsKey(literal);

    public IEnumerable<string> LiteralNames => Literals.Keys;

    /// <summary>The line (0-based) a function is defined on, or -1</summary>
    public int DefinitionOf(string function) => Definitions.TryGetValue(function, out var line) ? line : -1;

    /// <summary>The top-level function a line is in, or null</summary>
    public string FunctionAt(int line)
    {
        if (line < 0 || line >= LineFunction.Length || LineFunction[line] < 0)
            return null;

        return Functions[LineFunction[line]].Name;
    }

    /// <summary>Reads and indexes the script (off the UI thread)</summary>
    public void Load(byte[] data)
    {
        try
        {
            if (DCX.Is(data))
                data = DCX.Decompress(data).ToArray();

            string text;

            if (LuaDecompile.IsBytecode(data))
            {
                var lua = new LuaFile(new LuaReader(false, new MemoryStream(data)));
                var main = new LuaDecompilerCore.IR.Function(lua.MainFunction.FunctionId);
                var result = new LuaDecompiler(new DecompilationOptions { CatchPassExceptions = true })
                    .DecompileLuaFunction(new LuaDecompilerCore.LanguageDecompilers.HksDecompiler(), main, lua.MainFunction);

                if (result.DecompiledSource == null)
                    throw new InvalidOperationException(result.ErrorMessage ?? LOC.Get("HAVOK_Script_Decompile_Failed"));

                text = result.DecompiledSource;
            }
            else
            {
                IsText = true;
                text = LuaLexer.ShiftJis.GetString(data);
            }

            Index(text.Replace("\r\n", "\n").Split('\n'));
        }
        catch (Exception e)
        {
            Error = e.Message;
        }

        IsReady = true;
    }

    private void Index(string[] lines)
    {
        var lineFunction = new int[lines.Length];
        var current = -1;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var definition = DefinitionRegex.Match(line);

            if (definition.Success)
            {
                var indent = definition.Groups[1].Success ? definition.Groups[1].Value : definition.Groups[3].Value;
                var name = definition.Groups[2].Success ? definition.Groups[2].Value : definition.Groups[4].Value;

                Functions.Add((name, i));
                Definitions.TryAdd(name, i);

                // The enclosing function of a line: the last one defined at the top level (the decompiler puts every
                // function there)
                if (indent.Length == 0)
                    current = Functions.Count - 1;
            }

            lineFunction[i] = current;

            if (line.IndexOf('"') < 0 && line.IndexOf('\'') < 0)
                continue;

            foreach (Match m in LiteralRegex.Matches(line))
            {
                var value = m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value;

                if (!Literals.TryGetValue(value, out var at))
                    Literals[value] = at = new List<int>(1);

                if (at.Count == 0 || at[^1] != i)
                    at.Add(i);
            }
        }

        LineFunction = lineFunction;
        Lines = lines;
    }
}

/// <summary>
/// One line of a script that names something: the script, the line (0-based), its function, and the line's text
/// </summary>
public record HavokScriptMention(HavokScript Script, int Line, string Function, string Text);

/// <summary>
/// The action scripts paired with behavior files, read and indexed in the background once per session (Reload reads
/// them again). A behavior file X.hkx pairs with X.hks (the shared c9997.hkx with c9997.hks) and with its character's
/// own scripts (cXXXX.hks, cXXXX_*.hks: the player's c0000.hks and c0000_talk.hks); BehaviorSurvey hkssurvey.
/// </summary>
public class HavokScriptIndex
{
    private readonly ProjectEntry Project;

    private readonly ConcurrentDictionary<string, HavokScript> Scripts = new(StringComparer.OrdinalIgnoreCase);

    // The pairing of the last behavior file asked for (the views ask several times a frame)
    private (FileDictionaryEntry Binder, string File, List<HavokScript> Scripts) LastPairing;

    /// <summary>Counts scripts finishing their load, so callers can rebuild what they work out from them</summary>
    public int Version => Volatile.Read(ref VersionCount);
    private int VersionCount;

    public HavokScriptIndex(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>Forget every script, so they're read again (after editing them outside Smithbox)</summary>
    public void Reload()
    {
        Scripts.Clear();
        LastPairing = default;
        Interlocked.Increment(ref VersionCount);
    }

    /// <summary>
    /// The scripts paired with a behavior file, starting the load of any not read yet
    /// </summary>
    public List<HavokScript> ScriptsFor(FileDictionaryEntry binder, string behaviorPath)
    {
        var result = new List<HavokScript>();

        if (binder == null || behaviorPath == null)
            return result;

        if (ReferenceEquals(LastPairing.Binder, binder) && LastPairing.File == behaviorPath && LastPairing.Scripts != null)
            return LastPairing.Scripts;

        var character = binder.Filename ?? "";
        var behavior = System.IO.Path.GetFileName(behaviorPath.Replace('\\', '/')).Split('.')[0];

        foreach (var entry in Project.Locator.HavokScriptFiles.Entries)
        {
            var name = (entry.Filename ?? "").Split('.')[0];

            var paired = name.Equals(behavior, StringComparison.OrdinalIgnoreCase)
                || name.Equals(character, StringComparison.OrdinalIgnoreCase)
                || name.StartsWith(character + "_", StringComparison.OrdinalIgnoreCase);

            if (paired)
                result.Add(Get(entry));
        }

        // The character's own script first, then the shared one, then the rest (c0000_talk)
        result = result
            .OrderBy(s => System.IO.Path.GetFileNameWithoutExtension(s.Name).Equals(character, StringComparison.OrdinalIgnoreCase) ? 0
                : System.IO.Path.GetFileNameWithoutExtension(s.Name).Equals(behavior, StringComparison.OrdinalIgnoreCase) ? 1 : 2)
            .ThenBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        LastPairing = (binder, behaviorPath, result);
        return result;
    }

    private HavokScript Get(FileDictionaryEntry entry)
    {
        return Scripts.GetOrAdd(entry.Path, path =>
        {
            var script = new HavokScript
            {
                Path = path,
                Name = System.IO.Path.GetFileName(path.Replace('\\', '/')),
                FromProject = Project.VFS.ProjectFS.FileExists(path)
            };

            Task.Run(() =>
            {
                try
                {
                    var data = Project.VFS.FS.ReadFile(path);

                    if (data == null)
                        throw new FileNotFoundException(path);

                    script.Load(data.Value.ToArray());
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("HAVOK_Script_Read_Failed", path), e);
                    script.IsReady = true;
                }

                Interlocked.Increment(ref VersionCount);
            });

            return script;
        });
    }

    /// <summary>
    /// Every line of the scripts that names the string as a literal
    /// </summary>
    public static List<HavokScriptMention> FindMentions(IEnumerable<HavokScript> scripts, string literal, int max = 200)
    {
        var result = new List<HavokScriptMention>();

        foreach (var script in scripts)
        {
            if (!script.IsReady)
                continue;

            foreach (var line in script.LinesOf(literal))
            {
                result.Add(new(script, line, script.FunctionAt(line), script.Lines[line].Trim()));

                if (result.Count >= max)
                    return result;
            }
        }

        return result;
    }

    /// <summary>Whether every script is read</summary>
    public static bool AllReady(IEnumerable<HavokScript> scripts) => scripts.All(s => s.IsReady);
}
