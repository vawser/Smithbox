using Andre.IO.VFS;
using SoulsFormats;

namespace StudioCore.Editors.LuaEditor;

public enum LuaBinderKind
{
    Battle,
    Logic,
    Common,
    /// <summary>A map's binder (SDT, BB, DS3, DS1R: m10_00_00_00), holding the battle and logic scripts of its characters</summary>
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

    /// <summary>Project files of deleted binders, moved into the project's backup folder on the next save</summary>
    public List<string> PendingDeletes { get; } = new();

    public List<string> GetDirtyFiles()
    {
        return Containers.Where(c => c.IsDirty || c.PendingRevert).Select(c => c.Path).Concat(PendingDeletes).Distinct().ToList();
    }

    /// <summary>
    /// Saves every binder with unsaved changes. Deleted binders' project files, and reverted ones', are moved into the
    /// project's backup folder first (a reverted binder then reads the game's file again). False if any failed.
    /// </summary>
    public bool SaveAllDirty()
    {
        var ok = true;

        foreach (var path in PendingDeletes.ToList())
        {
            if (MoveToBackup(path))
                PendingDeletes.Remove(path);
            else
                ok = false;
        }

        foreach (var c in Containers.Where(c => c.PendingRevert).ToList())
        {
            if (MoveToBackup(c.Path))
                c.ResetToVanilla();
            else
                ok = false;
        }

        foreach (var c in Containers.Where(c => c.IsDirty).ToList())
            if (!c.Save())
                ok = false;
        return ok;
    }

    /// <summary>
    /// Moves a project file into &lt;project&gt;/.backup/&lt;time&gt;_deleted/&lt;its path&gt; (the project scan skips dot
    /// folders). Nothing to do when the project has no such file.
    /// </summary>
    private bool MoveToBackup(string path)
    {
        var projectFs = Project.VFS.ProjectFS;
        try
        {
            if (projectFs == null || !projectFs.FileExists(path))
                return true;

            var stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + "_deleted";
            var relative = $".backup/{stamp}{path}";
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.Join(Project.Descriptor.ProjectPath, relative)));
            projectFs.Move(path, relative);
            Smithbox.Log(this, LOC.Get("LUA_Data_Moved_To_Backup", path, relative));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("LUA_Data_Move_To_Backup_Failed", path), e);
            return false;
        }
    }

    /// <summary>
    /// Adds a binder at its sorted place (kind, then name). The list is replaced, not changed, so a background reader
    /// (the global search) never sees it change under it.
    /// </summary>
    public void Insert(LuaContainer container)
    {
        var list = Containers.ToList();
        var index = list.FindIndex(c => c.Kind > container.Kind || c.Kind == container.Kind && string.Compare(c.Name, container.Name, StringComparison.OrdinalIgnoreCase) > 0);
        list.Insert(index < 0 ? list.Count : index, container);
        Containers = list;
        ByName[container.Name] = container;
    }

    /// <summary>Takes a binder out of the list (replacing the list, as <see cref="Insert"/> does)</summary>
    public void Remove(LuaContainer container)
    {
        Containers = Containers.Where(c => c != container).ToList();
        if (ByName.TryGetValue(container.Name, out var c) && c == container)
            ByName.Remove(container.Name);
    }

    /// <summary>Whether a path is free for a new binder: no binder has it, and neither the project nor the game has the file</summary>
    public bool IsFreePath(string path)
    {
        return !Containers.Any(c => c.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
            && !PendingDeletes.Contains(path, StringComparer.OrdinalIgnoreCase)
            && !(Project.VFS.FS?.FileExists(path) ?? false);
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

    /// <summary>The binder's Lua scripts. Replaced, never changed in place (a background reader may hold it).</summary>
    public List<LuaScript> Scripts { get; private set; } = new();

    public bool IsLoaded { get; private set; }

    public string LoadError { get; private set; }

    /// <summary>Unsaved changes: an edited script, a script added or removed, a changed goal table, or a new binder</summary>
    public bool IsDirty => IsNew || EntriesChanged || GoalsChanged || Scripts.Any(s => s.IsDirty);

    /// <summary>Made in the editor (a new or duplicated binder) and not saved yet: no file has its path</summary>
    public bool IsNew { get; private set; }

    /// <summary>For a new binder: the binder its header and entry layout are copied from, and the stem its entry paths change from</summary>
    public string TemplatePath { get; private set; }
    public string TemplateStem { get; private set; }

    /// <summary>Deleted, but the game ships it too: the next save moves the project copy away and reads the game's</summary>
    public bool PendingRevert { get; set; }

    /// <summary>Whether the game itself has this file (it can't be removed, only reverted to)</summary>
    public bool InGame => Bank.Project.VFS.VanillaFS?.FileExists(Path) ?? false;

    /// <summary>
    /// The binder's goal table (its .luainfo: the goals of its global-function battle scripts and its logic scripts);
    /// null if it has none. Replaced, never changed in place.
    /// </summary>
    public List<LUAINFO.Goal> Goals { get; private set; }

    /// <summary>The .luainfo's format (64-bit offsets and UTF-16 names), for a rewritten or new one</summary>
    private bool GoalsLongFormat = true;
    private bool GoalsBigEndian;

    // Scripts added or removed, and the goal table changed, since the last load or save
    private bool EntriesChanged;
    private bool GoalsChanged;

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
                ReadGoals(bnd);
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

    private void ReadGoals(IBinder bnd)
    {
        Goals = null;
        GoalsLongFormat = bnd is BND4 { Unicode: true };
        var info = bnd.Files.FirstOrDefault(f => f.Name.EndsWith(".luainfo", StringComparison.OrdinalIgnoreCase));
        if (info == null)
            return;
        try
        {
            var li = LUAINFO.Read(info.Bytes);
            Goals = li.Goals.ToList();
            GoalsLongFormat = li.LongFormat;
            GoalsBigEndian = li.BigEndian;
        }
        catch (Exception e)
        {
            // An empty table can't be read (its format can't be told); edits start from an empty list
            Goals = new();
            Smithbox.LogError(this, LOC.Get("LUA_Data_Failed_To_Read_Goals", Name), e);
        }
    }

    /// <summary>
    /// A binder made in the editor: loaded, unsaved. On save it is written as a copy of the template binder (its header
    /// and layout), with the template's stem in entry paths changed to its own, its Lua entries this binder's scripts
    /// and its goal table these goals (none: no .luainfo).
    /// </summary>
    public static LuaContainer CreateNew(LuaBank bank, string path, string name, LuaBinderKind kind, LuaContainer template,
        List<LUAINFO.Goal> goals, Func<LuaContainer, List<LuaScript>> scripts)
    {
        var c = new LuaContainer(bank, path, name, kind, false)
        {
            IsNew = true,
            // A new binder made from another new one starts from the same file
            TemplatePath = template.IsNew ? template.TemplatePath : template.Path,
            TemplateStem = template.IsNew ? template.TemplateStem : template.Name,
            Goals = goals,
            GoalsLongFormat = template.GoalsLongFormat,
            GoalsBigEndian = template.GoalsBigEndian,
            GoalsChanged = true,
        };
        c.Scripts = scripts(c);
        c.IsLoaded = true;
        return c;
    }

    /// <summary>Adds a script at an index (written as a new entry on save)</summary>
    public void InsertScript(LuaScript script, int index)
    {
        var list = Scripts.ToList();
        list.Insert(Math.Clamp(index, 0, list.Count), script);
        Scripts = list;
        EntriesChanged = true;
    }

    /// <summary>Removes a script (its entry goes on save)</summary>
    public void RemoveScript(LuaScript script)
    {
        Scripts = Scripts.Where(s => s != script).ToList();
        EntriesChanged = true;
    }

    /// <summary>Replaces the goal table (written into the .luainfo on save; added if the binder has none, removed when empty)</summary>
    public void SetGoals(List<LUAINFO.Goal> goals)
    {
        Goals = goals;
        GoalsChanged = true;
    }

    /// <summary>After its project copy is moved away: unloaded, to read the game's file on next use</summary>
    public void ResetToVanilla()
    {
        Scripts = new();
        Goals = null;
        IsLoaded = false;
        LoadError = null;
        InProject = false;
        PendingRevert = false;
        EntriesChanged = false;
        GoalsChanged = false;
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
            // A new binder starts as a copy of its template's
            var source = IsNew ? TemplatePath : Path;
            var data = source != null ? fs.ReadFile(source) : null;
            if (data == null)
            {
                Smithbox.LogError(this, LOC.Get("LUA_Data_Failed_To_Find_File", source ?? Path));
                return false;
            }
            // Read from the compressed bytes, so the binder keeps its DCX compression on write
            bnd = ReadBinder(data.Value);

            var newBytes = new Dictionary<LuaScript, byte[]>();
            foreach (var s in edited)
                newBytes[s] = LuaLexer.ShiftJis.GetBytes(s.Text);

            // A new binder's entry paths name it (…\each\430000_battle\430000_battle.lua → …\430500_battle\430500_battle.lua)
            if (IsNew && TemplateStem != null)
            {
                foreach (var f in bnd.Files)
                    f.Name = f.Name?.Replace(TemplateStem, Name, StringComparison.OrdinalIgnoreCase);
            }

            RebuildLuaEntries(bnd, newBytes);

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

            if (GoalsChanged)
                WriteGoals(bnd, gnlFile);
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
        IsNew = false;
        TemplatePath = null;
        TemplateStem = null;
        EntriesChanged = false;
        GoalsChanged = false;
        Smithbox.Log(this, LOC.Get("EDITOR_Data_Save_File", Path));
        return true;
    }

    /// <summary>
    /// Makes the binder's .lua entries match <see cref="Scripts"/>: an entry whose script was deleted goes; a script
    /// with no entry (created or duplicated) gets one, in FromSoftware's layout: the next ID after the other Lua
    /// entries (they count up from 1000; .luagnl and .luainfo are 1000000 and 1000001), the other entries' folder
    /// and flags, after the last Lua entry.
    /// </summary>
    private void RebuildLuaEntries(IBinder bnd, Dictionary<LuaScript, byte[]> newBytes)
    {
        static bool IsLua(BinderFile f) => f.Name?.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) == true;
        string EntryOf(BinderFile f) => System.IO.Path.GetFileName(f.Name);

        var luaFiles = bnd.Files.Where(IsLua).ToList();
        var sample = luaFiles.FirstOrDefault() ?? bnd.Files.FirstOrDefault();
        var folder = sample?.Name is { } n && n.LastIndexOf('\\') is var slash and >= 0 ? n[..(slash + 1)] : "";
        var flags = luaFiles.FirstOrDefault()?.Flags ?? Binder.FileFlags.Flag1;

        foreach (var f in luaFiles)
        {
            if (!Scripts.Any(s => s.EntryName.Equals(EntryOf(f), StringComparison.OrdinalIgnoreCase)))
                bnd.Files.Remove(f);
        }

        foreach (var s in Scripts)
        {
            if (bnd.Files.Any(f => IsLua(f) && s.EntryName.Equals(EntryOf(f), StringComparison.OrdinalIgnoreCase)))
                continue;

            var luaIds = bnd.Files.Where(IsLua).Select(f => f.ID).ToList();
            var id = luaIds.Count > 0 ? luaIds.Max() + 1 : 1000;
            while (bnd.Files.Any(f => f.ID == id))
                id++;

            var bytes = newBytes.TryGetValue(s, out var b) ? b : LuaLexer.ShiftJis.GetBytes(s.Text ?? "");
            var file = new BinderFile(flags, id, folder + s.EntryName, bytes);
            var last = bnd.Files.FindLastIndex(IsLua);
            bnd.Files.Insert(last + 1, file);
        }
    }

    /// <summary>
    /// Writes <see cref="Goals"/> into the binder's .luainfo: added beside the .luagnl if the binder has none, removed
    /// when no goals are left (an empty .luainfo can't be read back)
    /// </summary>
    private void WriteGoals(IBinder bnd, BinderFile gnlFile)
    {
        var info = bnd.Files.FirstOrDefault(f => f.Name?.EndsWith(".luainfo", StringComparison.OrdinalIgnoreCase) == true);

        if (Goals == null || Goals.Count == 0)
        {
            if (info != null)
                bnd.Files.Remove(info);
            return;
        }

        var li = new LUAINFO(GoalsBigEndian, GoalsLongFormat) { Goals = Goals.ToList() };
        var bytes = li.Write();

        if (info != null)
        {
            info.Bytes = bytes;
            return;
        }

        var name = gnlFile != null
            ? System.IO.Path.ChangeExtension(gnlFile.Name, ".luainfo")
            : $"{Name}.luainfo";
        var id = 1000001;
        while (bnd.Files.Any(f => f.ID == id))
            id++;
        var index = gnlFile != null ? bnd.Files.IndexOf(gnlFile) + 1 : bnd.Files.Count;
        bnd.Files.Insert(index, new BinderFile(gnlFile?.Flags ?? Binder.FileFlags.Flag1, id, name, bytes));
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

    /// <summary>A script made in the editor (created or duplicated): source text, unsaved; a new entry on save</summary>
    public static LuaScript CreateNew(LuaContainer container, string entryName, string text)
    {
        var s = new LuaScript(container, entryName, Array.Empty<byte>());
        s._text = text;
        s.SavedText = null;
        s.IsDirty = true;
        return s;
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
                        IsDecompileIncomplete = IsBytecode && LuaDecompile.IsIncomplete(_text);
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

    /// <summary>
    /// The decompiler couldn't place part of the bytecode (its "not used in code generation" warning): the text is
    /// missing code the game runs, so saving it would drop that code
    /// </summary>
    public bool IsDecompileIncomplete { get; private set; }

    /// <summary>Why the current text can't be saved, or null</summary>
    public string SaveProblem()
    {
        if (Text == null)
            return LoadError;
        if (IsDecompileIncomplete)
            return LOC.Get("LUA_Data_Decompile_Incomplete");
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
