using Andre.IO.VFS;
using SoulsFormats;

namespace StudioCore.Editors.LuaEditor;

public enum LuaBinderKind
{
    Battle,
    Logic,
    Common,
    /// <summary>A map's binder (SDT, BB, DS3: m10_00_00_00), holding the battle and logic scripts of its characters</summary>
    Map,
    Other,
}

/// <summary>
/// The project's Lua binders (*.luabnd: the AI scripts of ER and NR). Listed at setup, each read on first use.
/// </summary>
public class LuaBank
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public List<LuaContainer> Containers { get; private set; } = new();

    private Dictionary<string, LuaContainer> ByName = new(StringComparer.OrdinalIgnoreCase);

    public LuaBank(ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        TargetFS = targetFs;
    }

    public bool Setup()
    {
        var entries = Project.Locator.FileDictionary.Entries
            .Where(e => string.Equals(e.Extension, "luabnd", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var containers = new List<LuaContainer>();

        // A dictionary can list both an uncompressed copy and a .dcx: keep one per file, the .dcx
        foreach (var group in entries.GroupBy(e => $"{e.Folder}/{e.Filename}", StringComparer.OrdinalIgnoreCase))
        {
            var entry = group.OrderByDescending(e => e.Path.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase)).First();
            var inProject = Project.VFS.ProjectFS?.FileExists(entry.Path) ?? false;
            containers.Add(new LuaContainer(this, entry.Path, entry.Filename, KindOf(entry.Filename), inProject));
        }

        Containers = containers
            .OrderBy(c => c.Kind)
            .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ByName = new(StringComparer.OrdinalIgnoreCase);
        foreach (var c in Containers)
            ByName.TryAdd(c.Name, c);

        return true;
    }

    public static LuaBinderKind KindOf(string name)
    {
        var n = name.ToLowerInvariant();
        if (n.EndsWith("_battle"))
            return LuaBinderKind.Battle;
        if (n.EndsWith("_logic"))
            return LuaBinderKind.Logic;
        if (n == "aicommon")
            return LuaBinderKind.Common;
        if (System.Text.RegularExpressions.Regex.IsMatch(n, @"^m\d\d_\d\d_\d\d_\d\d$"))
            return LuaBinderKind.Map;
        return LuaBinderKind.Other;
    }

    /// <summary>The number before the first '_' of a binder or script name (430000_battle → 430000), or null</summary>
    public static int? GoalIdOf(string name)
    {
        var i = name.IndexOf('_');
        return i > 0 && int.TryParse(name[..i], out var id) ? id : null;
    }

    /// <summary>
    /// The character a battle goal belongs to, from its goal ID: the first four of its six digits (430000 → c4300,
    /// 214001 → c2140). Null for other kinds, or a goal ID that isn't six digits.
    /// </summary>
    public static string CharacterIdOf(LuaBinderKind kind, int? goalId) =>
        kind == LuaBinderKind.Battle && goalId is { } id && id is >= 100000 and <= 999999 ? $"c{id / 100:D4}" : null;

    public LuaContainer FindContainer(string name)
    {
        return name != null && ByName.TryGetValue(name, out var c) ? c : null;
    }

    public List<string> GetDirtyFiles()
    {
        return Containers.Where(c => c.IsDirty).Select(c => c.Path).ToList();
    }

    /// <summary>Saves every binder with unsaved changes; false if any failed</summary>
    public bool SaveAllDirty()
    {
        var ok = true;
        foreach (var c in Containers.Where(c => c.IsDirty).ToList())
            if (!c.Save())
                ok = false;
        return ok;
    }
}

/// <summary>
/// One .luabnd: its Lua scripts, plus the .luagnl (global names) and .luainfo (goal table) beside them.
/// </summary>
public class LuaContainer
{
    public LuaBank Bank { get; }

    /// <summary>The VFS path, e.g. /script/430000_battle.luabnd.dcx</summary>
    public string Path { get; }

    /// <summary>e.g. 430000_battle</summary>
    public string Name { get; }

    public LuaBinderKind Kind { get; }

    /// <summary>The project folder has its own copy (it overrides the game's)</summary>
    public bool InProject { get; private set; }

    public List<LuaScript> Scripts { get; private set; } = new();

    public bool IsLoaded { get; private set; }

    public string LoadError { get; private set; }

    public bool IsDirty => Scripts.Any(s => s.IsDirty);

    private readonly object LoadLock = new();

    public LuaContainer(LuaBank bank, string path, string name, LuaBinderKind kind, bool inProject)
    {
        Bank = bank;
        Path = path;
        Name = name;
        Kind = kind;
        InProject = inProject;
    }

    /// <summary>The number in the binder's name (e.g. 430000 for 430000_battle): the goal ID NpcThinkParam refers to</summary>
    public int? GoalId => LuaBank.GoalIdOf(Name);

    /// <summary>The binder's battle goal character (see <see cref="LuaBank.CharacterIdOf"/>); null for a map's binder</summary>
    public string CharacterId => LuaBank.CharacterIdOf(Kind, GoalId);

    /// <summary>A map's binder or aicommon: several scripts, each with its own goal (shown per script in the list)</summary>
    public bool HoldsManyGoals => Kind is LuaBinderKind.Map or LuaBinderKind.Common;

    /// <summary>Reads the binder's entry list, if not yet (scripts are decompiled when first shown)</summary>
    public bool EnsureLoaded()
    {
        if (IsLoaded)
            return true;

        lock (LoadLock)
        {
            if (IsLoaded)
                return true;
            if (LoadError != null)
                return false;

            try
            {
                var data = Bank.TargetFS.ReadFile(Path);
                if (data == null)
                {
                    LoadError = LOC.Get("LUA_Data_Failed_To_Find_File", Path);
                    Smithbox.LogError(this, LoadError);
                    return false;
                }

                using var bnd = ReadBinder(data.Value);
                Scripts = bnd.Files
                    .Where(f => f.Name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
                    .Select(f => new LuaScript(this, System.IO.Path.GetFileName(f.Name), f.Bytes.ToArray()))
                    .ToList();
                IsLoaded = true;
                return true;
            }
            catch (Exception e)
            {
                LoadError = LOC.Get("LUA_Data_Failed_To_Read_File", Path);
                Smithbox.LogError(this, LoadError, e);
                return false;
            }
        }
    }

    private static IBinder ReadBinder(Memory<byte> data)
    {
        // Not BND4.IsRead (the binders implement another Read overload): pick the binder by its magic
        var raw = DCX.Is(data) ? DCX.Decompress(data) : data;
        var s = raw.Span;
        return s.Length >= 4 && s[0] == 'B' && s[1] == 'N' && s[2] == 'D' && s[3] == '4' ? BND4.Read(data) : BND3.Read(data);
    }

    /// <summary>
    /// Writes the binder into the project. It is read again (the project's copy if it has one, else the game's);
    /// edited scripts become Shift-JIS source text (the game loads text Lua) and the .luagnl is rebuilt from every
    /// script in the binder; every other entry keeps its bytes. Refused, with the reason logged, if an edited script
    /// has a syntax error or a character Shift-JIS can't hold.
    /// </summary>
    public bool Save()
    {
        if (!IsLoaded)
            return false;

        var project = Bank.Project;
        var fs = project.VFS.FS;
        var toFs = ProjectUtils.GetFilesystemForWrite(project);

        var edited = Scripts.Where(s => s.IsDirty).ToList();
        foreach (var s in edited)
        {
            var problem = s.SaveProblem();
            if (problem != null)
            {
                Smithbox.LogError(this, LOC.Get("LUA_Data_Save_Refused", Name, s.EntryName, problem));
                return false;
            }
        }

        IBinder bnd;
        try
        {
            var data = fs.ReadFile(Path);
            if (data == null)
            {
                Smithbox.LogError(this, LOC.Get("LUA_Data_Failed_To_Find_File", Path));
                return false;
            }
            // Read from the compressed bytes, so the binder keeps its DCX compression on write
            bnd = ReadBinder(data.Value);

            var newBytes = new Dictionary<LuaScript, byte[]>();
            foreach (var s in edited)
                newBytes[s] = LuaLexer.ShiftJis.GetBytes(s.Text);

            // Lua entries, and the globals in binder order (edited and source scripts from their text, untouched
            // bytecode from its SETGLOBAL names)
            var globals = new List<string>();
            foreach (var f in bnd.Files.Where(f => f.Name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase)))
            {
                var script = Scripts.FirstOrDefault(s => s.EntryName.Equals(System.IO.Path.GetFileName(f.Name), StringComparison.OrdinalIgnoreCase));
                if (script != null && newBytes.TryGetValue(script, out var b))
                    f.Bytes = b;

                if (LuaDecompile.IsBytecode(f.Bytes.Span))
                    globals.AddRange(LuaDecompile.BytecodeGlobals(f.Bytes));
                else
                {
                    var parse = LuaParser.Parse(LuaLexer.ShiftJis.GetString(f.Bytes.Span));
                    if (!parse.IsValid)
                    {
                        Smithbox.LogError(this, LOC.Get("LUA_Data_Save_Refused", Name, f.Name, parse.Problems[0].Message));
                        bnd.Dispose();
                        return false;
                    }
                    globals.AddRange(parse.Globals);
                }
            }

            var gnlFile = bnd.Files.FirstOrDefault(f => f.Name.EndsWith(".luagnl", StringComparison.OrdinalIgnoreCase));
            if (gnlFile != null)
            {
                var gnl = LUAGNL.Read(gnlFile.Bytes);
                var names = globals.Distinct().ToList();
                if (!names.SequenceEqual(gnl.Globals))
                {
                    gnl.Globals = names;
                    gnlFile.Bytes = gnl.Write();
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("LUA_Data_Failed_To_Write_File", Path), e);
            return false;
        }

        // No game type: with one, WriteWithBackup treats a BND4 as an encrypted regulation
        var ok = bnd is BND4 b4
            ? ProjectUtils.WriteWithBackup(project, fs, toFs, Path, b4)
            : ProjectUtils.WriteWithBackup(project, fs, toFs, Path, (BND3)bnd);

        if (!ok)
            return false;

        foreach (var s in edited)
            s.MarkSaved();

        InProject = true;
        Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", Path));
        return true;
    }
}

/// <summary>
/// One Lua entry of a binder. Its text is made when first asked for: decompiled (with readable local names) if the
/// entry is bytecode, read as Shift-JIS if it is source. Edits are committed whole (when the text box is left), each
/// one undoable.
/// </summary>
public class LuaScript
{
    public LuaContainer Container { get; }

    /// <summary>e.g. 430000_battle.lua</summary>
    public string EntryName { get; }

    /// <summary>The entry as last read or saved</summary>
    private byte[] Bytes;

    /// <summary>The entry is compiled Lua (vanilla); false once saved as text, or for a mod's source entry</summary>
    public bool IsBytecode => LuaDecompile.IsBytecode(Bytes);

    private string _text;
    private string SavedText;

    public string LoadError { get; private set; }

    /// <summary>Counts every edit and undo, so views rebuild what they work out from the text</summary>
    public int Revision { get; private set; }

    public bool IsDirty { get; private set; }

    private readonly Stack<string> UndoStack = new();
    private readonly Stack<string> RedoStack = new();

    private LuaParseResult _parse;
    private int _parseRevision = -1;

    public LuaScript(LuaContainer container, string entryName, byte[] bytes)
    {
        Container = container;
        EntryName = entryName;
        Bytes = bytes;

        // From the entry's own name (a map's binder holds many goals: m10_00_00_00 → 102010_battle.lua), else the binder's
        var stem = System.IO.Path.GetFileNameWithoutExtension(entryName);
        var kind = LuaBank.KindOf(stem);
        Kind = kind is LuaBinderKind.Battle or LuaBinderKind.Logic ? kind
            : container.Kind == LuaBinderKind.Map ? LuaBinderKind.Other : container.Kind;
        GoalId = LuaBank.GoalIdOf(stem) ?? container.GoalId;
        CharacterId = LuaBank.CharacterIdOf(Kind, GoalId);
    }

    /// <summary>Battle or logic by the entry's name (102010_battle.lua), else the binder's kind</summary>
    public LuaBinderKind Kind { get; }

    /// <summary>The goal ID NpcThinkParam refers to, from the entry's name, else the binder's</summary>
    public int? GoalId { get; }

    /// <summary>The battle goal's character (see <see cref="LuaBank.CharacterIdOf"/>)</summary>
    public string CharacterId { get; }

    /// <summary>Identifies the script across binders (map binders can hold scripts of the same name)</summary>
    public string Key => $"{Container.Name}|{EntryName}";

    public bool IsTextReady => _text != null;

    /// <summary>The script's text (decompiled on first use); null if it couldn't be made (see <see cref="LoadError"/>)</summary>
    public string Text
    {
        get
        {
            // The definition index reads aicommon's scripts on a background thread
            lock (TextLock)
            {
                if (_text == null && LoadError == null)
                {
                    try
                    {
                        _text = LuaDecompile.ToText(Bytes, CFG.Current.LuaEditor_Readable_Names);
                        SavedText = _text;
                    }
                    catch (Exception e)
                    {
                        LoadError = LOC.Get("LUA_Data_Failed_To_Decompile", EntryName, Container.Name);
                        Smithbox.LogError(this, LoadError, e);
                    }
                }
                return _text;
            }
        }
    }

    private readonly object TextLock = new();

    /// <summary>The syntax check of the current text (cached per revision)</summary>
    public LuaParseResult Parse
    {
        get
        {
            lock (TextLock)
            {
                if (_parseRevision != Revision || _parse == null)
                {
                    _parse = Text != null ? LuaParser.Parse(Text) : null;
                    _parseRevision = Revision;
                }
                return _parse;
            }
        }
    }

    /// <summary>Replaces the text as one undoable edit; nothing happens if it's unchanged</summary>
    public void Commit(string text)
    {
        if (Text == null || text == _text)
            return;
        UndoStack.Push(_text);
        RedoStack.Clear();
        SetText(text);
    }

    public bool CanUndo => UndoStack.Count > 0;
    public bool CanRedo => RedoStack.Count > 0;

    public void Undo()
    {
        if (!CanUndo)
            return;
        RedoStack.Push(_text);
        SetText(UndoStack.Pop());
    }

    public void Redo()
    {
        if (!CanRedo)
            return;
        UndoStack.Push(_text);
        SetText(RedoStack.Pop());
    }

    private void SetText(string text)
    {
        lock (TextLock)
        {
            _text = text;
            Revision++;
            IsDirty = _text != SavedText;
        }
    }

    /// <summary>Why the current text can't be saved, or null</summary>
    public string SaveProblem()
    {
        if (Text == null)
            return LoadError;
        var parse = Parse;
        if (!parse.IsValid)
        {
            var p = parse.Problems[0];
            return LOC.Get("LUA_Data_Problem_At", p.Line, p.Column, p.Message);
        }
        if (LuaLexer.FindUnencodable(Text) is { } bad)
            return LOC.Get("LUA_Data_Unencodable", bad.Line, bad.Column, bad.Char);
        return null;
    }

    public void MarkSaved()
    {
        Bytes = LuaLexer.ShiftJis.GetBytes(_text);
        SavedText = _text;
        IsDirty = false;
        Revision++;
    }
}
