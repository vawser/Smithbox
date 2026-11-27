using Hexa.NET.ImGui;
using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.LuaEditor;

/// <summary>A goal-list constant a script owns, with the list script that holds it</summary>
public readonly record struct LuaOwnedConstant(LuaScript List, LuaListConstant Constant);

/// <summary>
/// The goals a battle or logic script owns: the goal-list constants its registrations name or that equal its goal ID
/// (GOAL_… in goal_list for a battle script, LOGIC_ID_… in logic_list for a logic), and its binder's goal-table
/// entries whose functions it defines
/// </summary>
public sealed class LuaScriptGoals
{
    public List<LuaOwnedConstant> Constants { get; } = new();
    public List<LUAINFO.Goal> TableGoals { get; } = new();
}

/// <summary>What a create or duplicate will add, worked out before it is applied (or why it can't be)</summary>
public sealed class LuaScriptPlan
{
    public string Refusal;

    public string EntryName;
    public string Text;

    /// <summary>The binder the script is added to (a map's binder, aicommon); null when it gets a new binder</summary>
    public LuaContainer Into;
    public int InsertIndex;

    /// <summary>A new binder: copied from the template, under this path and name, with this goal table (null: none)</summary>
    public LuaContainer Template;
    public string BinderPath;
    public string BinderName;
    public List<LUAINFO.Goal> BinderGoals;

    /// <summary>Lines added to list scripts (after a line), and goal-table entries added to <see cref="Into"/></summary>
    public List<(LuaScript List, int AfterLine, string Name, double Value)> Constants { get; } = new();
    public List<LUAINFO.Goal> Goals { get; } = new();

    public static LuaScriptPlan Refuse(string text) => new() { Refusal = text };
}

/// <summary>
/// Creating, duplicating and deleting battle and logic scripts, with the goals they register (see
/// Claude/Documentation/Lua and AI.md (Text editing)). These are structural edits: written on save,
/// not on the undo stack (a list script's added constant lines are, as an edit of that script).
/// </summary>
public class LuaScriptCommands
{
    private readonly LuaEditorScreen Editor;

    public LuaScriptCommands(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    private LuaBank Bank => Editor.Bank;

    public static bool IsGoalScript(LuaScript s) => s != null && s.Kind is LuaBinderKind.Battle or LuaBinderKind.Logic;

    // --- what a script owns ---

    /// <summary>
    /// The list scripts (goal_list* or logic_list*) a binder's goals use: those of every aicommon binder (ER keeps the
    /// DLC's in aicommon_dlc01/02), and the binder's own, where it carries its own copy of the library (DES's world
    /// binders). Null: aicommon's only (a new per-goal binder).
    /// </summary>
    public IEnumerable<LuaScript> ListScripts(bool logic, LuaContainer binder) => Lists(logic, c => c == binder);

    /// <summary>Every list script: aicommon's and every map binder's (DES), for a goal ID or name unique across them all</summary>
    private IEnumerable<LuaScript> AllListScripts(bool logic) => Lists(logic, c => c.Kind == LuaBinderKind.Map);

    private IEnumerable<LuaScript> Lists(bool logic, Func<LuaContainer, bool> own)
    {
        var prefix = logic ? LuaScriptStructure.LogicListPrefix : LuaScriptStructure.GoalListPrefix;
        foreach (var c in Bank?.Containers ?? new())
        {
            var common = c.Name.StartsWith("aicommon", StringComparison.OrdinalIgnoreCase);
            if (!common && !own(c) || !c.EnsureLoaded())
                continue;
            foreach (var s in c.Scripts)
            {
                if (s.EntryName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && s.Parse?.Tree != null)
                    yield return s;
            }
        }
    }

    /// <summary>
    /// The main list a new goal's constant goes into: goal_list.lua (logic_list.lua), the binder's own first (DES), else
    /// aicommon's, else the first list
    /// </summary>
    private LuaScript MainList(bool logic, LuaContainer binder)
    {
        var lists = ListScripts(logic, binder).OrderByDescending(s => s.Container == binder).ToList();
        var exact = (logic ? LuaScriptStructure.LogicListPrefix : LuaScriptStructure.GoalListPrefix) + ".lua";
        return lists.FirstOrDefault(s => s.EntryName.Equals(exact, StringComparison.OrdinalIgnoreCase)) ?? lists.FirstOrDefault();
    }

    public LuaScriptGoals GoalsOf(LuaScript script)
    {
        var goals = new LuaScriptGoals();
        var parse = script?.Parse;
        if (parse?.Tree == null)
            return goals;

        var registered = LuaScriptStructure.Registrations(parse).Select(r => r.Constant).ToHashSet(StringComparer.Ordinal);
        var logic = script.Kind == LuaBinderKind.Logic;

        foreach (var isLogicList in new[] { false, true })
        {
            foreach (var list in ListScripts(isLogicList, script.Container))
            {
                foreach (var c in LuaScriptStructure.ListConstants(list.Parse))
                {
                    var own = registered.Contains(c.Name) || isLogicList == logic && script.GoalId is { } id && c.Value == id;
                    if (own && !goals.Constants.Any(x => x.Constant.Name == c.Name))
                        goals.Constants.Add(new LuaOwnedConstant(list, c));
                }
            }
        }

        if (script.Container.Goals is { } table)
        {
            var functions = LuaScriptStructure.DefinedFunctions(parse);
            goals.TableGoals.AddRange(table.Where(g => LuaScriptStructure.OwnsGoal(g, functions)));
        }
        return goals;
    }

    /// <summary>A constant's name is free for a value: unused, or already that value (made before); else why not</summary>
    private string ConstantRefusal(bool logic, string name, double value, ICollection<(string Name, double Value)> planned)
    {
        foreach (var list in AllListScripts(logic))
        {
            foreach (var c in LuaScriptStructure.ListConstants(list.Parse))
            {
                if (c.Name == name && c.Value != value)
                    return LOC.Get("LUA_Struct_Constant_Taken", name, c.Value, list.EntryName);
                if (c.Name != name && c.Value == value)
                    return LOC.Get("LUA_Struct_Id_Taken", value, c.Name, list.EntryName);
            }
        }
        if (planned.Any(p => p.Name != name && p.Value == value))
            return LOC.Get("LUA_Struct_Id_Taken", value, planned.First(p => p.Value == value).Name, LOC.Get("LUA_Struct_This_Copy"));
        return null;
    }

    private static bool ConstantExists(LuaScript list, string name) =>
        LuaScriptStructure.ListConstants(list.Parse).Any(c => c.Name == name);

    /// <summary>A binder's VFS path under another stem, in the same folder (/script/430000_battle.luabnd.dcx → /script/430500_battle.luabnd.dcx)</summary>
    private static string RenamedPath(LuaContainer c, string stem)
    {
        var slash = c.Path.LastIndexOf('/');
        var fileName = c.Path[(slash + 1)..];
        var dot = fileName.IndexOf('.');
        return c.Path[..(slash + 1)] + stem + (dot >= 0 ? fileName[dot..] : "");
    }

    private string NewBinderRefusal(string path, string name)
    {
        if (Bank.FindContainer(name) != null || !Bank.IsFreePath(path))
            return LOC.Get("LUA_Struct_Binder_Taken", name);
        return null;
    }

    private static string GoalRefusal(IEnumerable<LUAINFO.Goal> table, LUAINFO.Goal goal, string binder)
    {
        var clash = table?.FirstOrDefault(g => g.ID == goal.ID || g.Name == goal.Name);
        return clash != null ? LOC.Get("LUA_Struct_Goal_Taken", clash.ID, clash.Name, binder) : null;
    }

    // --- duplicate ---

    public LuaScriptPlan PlanDuplicate(LuaScript script, int newId)
    {
        if (!IsGoalScript(script))
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_Not_Goal_Script"));
        if (script.GoalId is not { } oldId)
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_No_Goal_Id", script.EntryName));
        if (newId <= 0)
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_Bad_Id"));
        if (newId == oldId)
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_Same_Id"));

        var src = script.Text;
        if (src == null)
            return LuaScriptPlan.Refuse(script.LoadError ?? LOC.Get("LUA_Struct_No_Text"));
        if (script.IsDecompileIncomplete)
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Data_Decompile_Incomplete"));
        var parse = script.Parse;
        if (parse == null || !parse.IsValid)
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_Invalid"));

        var delta = newId - oldId;
        var stem = Path.GetFileNameWithoutExtension(script.EntryName);
        var plan = new LuaScriptPlan { EntryName = LuaScriptStructure.RenameId(stem, oldId, newId) + ".lua" };
        var goals = GoalsOf(script);

        // The IDs that move: the script's, then its other goals' (a helper goal 430001 becomes newId + 1)
        var ids = new List<(int From, int To)> { (oldId, newId) };
        foreach (var (_, c) in goals.Constants)
        {
            if (c.Value is var v && v == Math.Floor(v) && v > 0 && v <= int.MaxValue && (int)v != oldId && !ids.Any(x => x.From == (int)v))
                ids.Add(((int)v, (int)v + delta));
        }

        // The copy's globals named after a moved ID get the new one (they share one Lua state with the original's)
        var renames = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var g in parse.Globals)
        {
            var r = LuaScriptStructure.RenameIds(g, ids);
            if (r != g)
                renames[g] = r;
        }

        var planned = new List<(string Name, double Value)>();
        var constantRenames = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (list, c) in goals.Constants)
        {
            var value = c.Value + delta;
            var name = LuaScriptStructure.CopyConstantName(c.Name, ids, value);
            renames[c.Name] = name;
            constantRenames[c.Name] = name;

            var logic = list.EntryName.StartsWith(LuaScriptStructure.LogicListPrefix, StringComparison.OrdinalIgnoreCase);
            if (ConstantRefusal(logic, name, value, planned) is { } refusal)
                return LuaScriptPlan.Refuse(refusal);
            planned.Add((name, value));
            if (!ConstantExists(list, name))
                plan.Constants.Add((list, c.LastLine, name, value));
        }

        // Goal-table names are the script's function names (X_Logic, XBattle for XBattle_Activate): renamed as they are
        string RenameName(string n) => n == null ? null : renames.TryGetValue(n, out var r) ? r
            : renames.TryGetValue(n + "_Activate", out var a) ? a[..^"_Activate".Length] : LuaScriptStructure.RenameIds(n, ids);
        var newGoals = goals.TableGoals.Select(g => new LUAINFO.Goal(g.ID + delta, RenameName(g.Name),
            g.BattleInterrupt, g.LogicInterrupt, RenameName(g.LogicInterruptName))).ToList();

        var container = script.Container;
        if (container.HoldsManyGoals)
        {
            if (container.Scripts.Any(s => s.EntryName.Equals(plan.EntryName, StringComparison.OrdinalIgnoreCase)))
                return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_Entry_Taken", plan.EntryName, container.Name));
            foreach (var g in newGoals)
            {
                if (GoalRefusal(container.Goals, g, container.Name) is { } refusal)
                    return LuaScriptPlan.Refuse(refusal);
            }
            plan.Into = container;
            plan.InsertIndex = container.Scripts.IndexOf(script) + 1;
            plan.Goals.AddRange(newGoals);
        }
        else
        {
            plan.Template = container;
            plan.BinderName = LuaScriptStructure.RenameId(container.Name, oldId, newId);
            plan.BinderPath = RenamedPath(container, plan.BinderName);
            if (NewBinderRefusal(plan.BinderPath, plan.BinderName) is { } refusal)
                return LuaScriptPlan.Refuse(refusal);
            // Its goal table holds the copy's goals (none when the original's binder has no .luainfo)
            plan.BinderGoals = container.Goals != null ? newGoals : null;
        }

        plan.Text = LuaScriptStructure.RenameScript(src, parse, s => LuaScriptStructure.CopyRegistrationName(s, ids, constantRenames), renames);
        return plan;
    }

    /// <summary>A free goal ID for a copy: the next IDs after the original's that pass every check</summary>
    public int SuggestDuplicateId(LuaScript script)
    {
        if (script?.GoalId is not { } id)
            return 0;
        for (var i = 1; i < 200; i++)
        {
            if (PlanDuplicate(script, id + i).Refusal == null)
                return id + i;
        }
        return id + 1;
    }

    // --- create ---

    /// <summary>The per-goal binder a new script's binder is copied from: the first of that kind (ER: 011000_battle, 010000_logic)</summary>
    public LuaContainer CreateTemplate(LuaBinderKind kind) =>
        Bank?.Containers.FirstOrDefault(c => c.Kind == kind && !c.HoldsManyGoals && !c.IsNew && c.EnsureLoaded() && c.Scripts.Count == 1);

    /// <summary>
    /// A new battle or logic script that only waits, with its goal: a goal-list constant (when the game has the list),
    /// and for a global-function battle goal or a logic, a goal-table entry (when the binder has a .luainfo). Into a
    /// map's binder when one is given, else a new binder like the first of its kind.
    /// </summary>
    public LuaScriptPlan PlanCreate(LuaBinderKind kind, int id, string name, LuaContainer into)
    {
        name = name?.Trim();
        if (!LuaScriptStructure.IsIdentifier(name))
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_Bad_Name"));
        if (id <= 0)
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_Bad_Id"));

        var logic = kind == LuaBinderKind.Logic;
        var template = into ?? CreateTemplate(kind);
        if (template == null)
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_No_Template"));

        // The battle goal's form: the table form where aicommon defines RegisterTableGoal
        var common = Editor.Definitions;
        common.EnsureIndex();
        if (!logic && common.Functions == null)
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_Reading_Common"));
        var table = !logic && common.Functions.ContainsKey("RegisterTableGoal");

        // The goal ID's width in entry names (6 digits: 011000_battle)
        var sibling = template.Scripts.FirstOrDefault(s => s.GoalId != null);
        var siblingStem = sibling != null ? Path.GetFileNameWithoutExtension(sibling.EntryName) : null;
        var width = siblingStem != null && siblingStem.IndexOf('_') is var u and > 0 ? u : 6;
        var stem = $"{id.ToString().PadLeft(width, '0')}_{(logic ? "logic" : "battle")}";
        var baseName = $"{name}{id}";

        var plan = new LuaScriptPlan { EntryName = stem + ".lua" };

        // A global-form battle goal calls REGISTER_GOAL where its siblings do (DS1R, DES); a logic calls
        // REGISTER_LOGIC_FUNC where its siblings do (DES)
        var registerGoal = !logic && !table && template.Scripts.FirstOrDefault(s => s.Kind == LuaBinderKind.Battle)?.Text?.Contains("REGISTER_GOAL(") == true;
        var registerLogic = logic && template.Scripts.FirstOrDefault(s => s.Kind == LuaBinderKind.Logic)?.Text?.Contains("REGISTER_LOGIC_FUNC(") == true;
        plan.Text = logic ? LuaScriptStructure.LogicTemplate(baseName, registerLogic) : LuaScriptStructure.BattleTemplate(baseName, table, registerGoal);

        // Its constant, at the end of the main list (the binder's own, where it carries the library: DES)
        var list = MainList(logic, into);
        if (list != null)
        {
            var constant = logic ? $"LOGIC_ID_{baseName}" : $"GOAL_{baseName}_Battle";
            if (ConstantRefusal(logic, constant, id, Array.Empty<(string, double)>()) is { } refusal)
                return LuaScriptPlan.Refuse(refusal);
            if (!ConstantExists(list, constant))
                plan.Constants.Add((list, int.MaxValue, constant, id));
        }
        else if (table || registerGoal || registerLogic)
            return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_No_Goal_List"));

        // Its goal-table entry: a global-function battle goal's or a logic's, where the binder keeps a .luainfo
        var goal = !logic && table ? null
            : logic ? new LUAINFO.Goal(id, $"{baseName}_Logic", false, true, $"{baseName}_Interupt")
            : new LUAINFO.Goal(id, $"{baseName}Battle", true, false);

        if (into != null)
        {
            if (into.Scripts.Any(s => s.EntryName.Equals(plan.EntryName, StringComparison.OrdinalIgnoreCase)))
                return LuaScriptPlan.Refuse(LOC.Get("LUA_Struct_Entry_Taken", plan.EntryName, into.Name));
            if (goal != null && into.Goals != null)
            {
                if (GoalRefusal(into.Goals, goal, into.Name) is { } refusal)
                    return LuaScriptPlan.Refuse(refusal);
                plan.Goals.Add(goal);
            }
            plan.Into = into;
            plan.InsertIndex = into.Scripts.Count;
        }
        else
        {
            plan.Template = template;
            plan.BinderName = stem;
            plan.BinderPath = RenamedPath(template, stem);
            if (NewBinderRefusal(plan.BinderPath, plan.BinderName) is { } refusal)
                return LuaScriptPlan.Refuse(refusal);
            plan.BinderGoals = template.Goals == null ? null : goal != null ? new() { goal } : new();
        }
        return plan;
    }

    /// <summary>A free goal ID for a new script: after the highest goal ID of its kind</summary>
    public int SuggestCreateId(LuaBinderKind kind, LuaContainer into)
    {
        var ids = (into != null ? into.Scripts.Where(s => s.Kind == kind).Select(s => s.GoalId)
            : Bank?.Containers.Where(c => c.Kind == kind).Select(c => c.GoalId) ?? Enumerable.Empty<int?>())
            .Where(i => i != null).Select(i => i.Value).ToList();
        var next = ids.Count > 0 ? ids.Max() / 1000 * 1000 + 1000 : 100000;
        return next;
    }

    // --- apply ---

    public void Apply(LuaScriptPlan plan)
    {
        if (plan?.Refusal != null || Bank == null)
            return;

        Editor.ScriptView.CommitPending();

        LuaScript script;
        if (plan.Into != null)
        {
            script = LuaScript.CreateNew(plan.Into, plan.EntryName, plan.Text);
            plan.Into.InsertScript(script, plan.InsertIndex);
            if (plan.Goals.Count > 0)
                plan.Into.SetGoals((plan.Into.Goals ?? new()).Concat(plan.Goals).ToList());
        }
        else
        {
            var kind = LuaBank.KindOf(plan.BinderName);
            LuaScript made = null;
            var binder = LuaContainer.CreateNew(Bank, plan.BinderPath, plan.BinderName, kind, plan.Template, plan.BinderGoals,
                c =>
                {
                    made = LuaScript.CreateNew(c, plan.EntryName, plan.Text);
                    return new List<LuaScript> { made };
                });
            Bank.Insert(binder);
            script = made;
        }

        foreach (var group in plan.Constants.GroupBy(c => c.List))
        {
            var list = group.Key;
            var text = LuaScriptStructure.InsertLines(list.Text, group.Select(c => (c.AfterLine, $"{c.Name} = {FormatNumber(c.Value)}")));
            list.Commit(text);
        }

        Editor.Definitions.Invalidate();
        Editor.SelectScript(script);
        Editor.FileList.ScrollToSelection = true;
        Smithbox.Log(this, LOC.Get("LUA_Struct_Log_Added", plan.EntryName, plan.Into?.Name ?? plan.BinderName));
    }

    private static string FormatNumber(double v) => v == Math.Floor(v) ? ((long)v).ToString() : v.ToString(System.Globalization.CultureInfo.InvariantCulture);

    // --- delete ---

    /// <summary>Why a script can't be deleted, or null</summary>
    public static string DeleteRefusal(LuaScript script)
    {
        if (!IsGoalScript(script))
            return LOC.Get("LUA_Struct_Not_Goal_Script");
        var c = script.Container;
        if (c.HoldsManyGoals)
            return c.Scripts.Count < 2 ? LOC.Get("LUA_Struct_Delete_Last") : null;
        if (c.PendingRevert)
            return LOC.Get("LUA_Struct_Delete_Reverting");
        if (!c.IsNew && !c.InProject)
            return LOC.Get("LUA_Struct_Delete_Vanilla");
        return null;
    }

    /// <summary>
    /// Deletes a script. In a map's binder or aicommon its entry and its goal-table entries go; a per-goal binder goes
    /// whole (an unsaved one at once, a project-only one moved into the backup folder on save, a project copy of a
    /// game binder moved there and the game's read again). Its goal-list constants go too when asked.
    /// </summary>
    public void Delete(LuaScript script, bool removeConstants)
    {
        if (DeleteRefusal(script) != null || Bank == null)
            return;

        Editor.ScriptView.CommitPending();
        var goals = GoalsOf(script);
        var c = script.Container;

        // What to select after
        LuaScript next = null;
        if (c.HoldsManyGoals)
        {
            var i = c.Scripts.IndexOf(script);
            next = c.Scripts.Where(s => s != script).ElementAtOrDefault(Math.Max(0, i - 1));
            c.RemoveScript(script);
            if (goals.TableGoals.Count > 0 && c.Goals != null)
                c.SetGoals(c.Goals.Where(g => !goals.TableGoals.Contains(g)).ToList());
        }
        else if (c.IsNew)
            Bank.Remove(c);
        else if (c.InGame)
            c.PendingRevert = true;
        else
        {
            Bank.Remove(c);
            Bank.PendingDeletes.Add(c.Path);
        }

        if (removeConstants)
        {
            foreach (var group in goals.Constants.GroupBy(x => x.List))
                group.Key.Commit(LuaScriptStructure.RemoveLines(group.Key.Text, group.Select(x => (x.Constant.FirstLine, x.Constant.LastLine))));
        }

        Editor.Definitions.Invalidate();
        if (Editor.SelectedScript == script || Editor.SelectedContainer == c && !c.HoldsManyGoals)
        {
            if (next != null)
                Editor.SelectScript(next);
            else
                Editor.SelectContainer(null);
        }
        Smithbox.Log(this, LOC.Get("LUA_Struct_Log_Deleted", script.EntryName, c.Name));
    }

    // --- dialogs ---

    private enum Mode { Create, Duplicate, Delete }

    private Mode DialogMode;
    private bool OpenDialog;
    private LuaScript Source;
    private LuaContainer Into;
    private LuaBinderKind NewKind = LuaBinderKind.Battle;
    private string IdText = "";
    private string NameText = "";
    private bool RemoveConstants;

    // The plan for the dialog's current inputs, worked out again only when they change
    private (Mode, LuaScript, LuaContainer, LuaBinderKind, string, string) PlanKey;
    private LuaScriptPlan Plan;

    private const string PopupId = "###luaScriptStructureModal";

    /// <summary>Opens the create dialog: into a map's binder when given one, else as a new binder</summary>
    public void AskCreate(LuaContainer into, LuaBinderKind kind = LuaBinderKind.Battle)
    {
        DialogMode = Mode.Create;
        Into = into?.Kind == LuaBinderKind.Map ? into : null;
        NewKind = kind is LuaBinderKind.Logic ? LuaBinderKind.Logic : LuaBinderKind.Battle;
        IdText = SuggestCreateId(NewKind, Into).ToString();
        NameText = "NewGoal";
        Plan = null;
        OpenDialog = true;
    }

    public void AskDuplicate(LuaScript script)
    {
        if (!IsGoalScript(script))
            return;
        DialogMode = Mode.Duplicate;
        Source = script;
        IdText = SuggestDuplicateId(script).ToString();
        Plan = null;
        OpenDialog = true;
    }

    public void AskDelete(LuaScript script)
    {
        if (DeleteRefusal(script) != null)
            return;
        DialogMode = Mode.Delete;
        Source = script;
        DeleteGoals = GoalsOf(script);
        // A goal the game ships may share its constant with copies in other maps' binders: kept unless asked
        RemoveConstants = !(script.Container.InGame && !script.Container.IsNew);
        OpenDialog = true;
    }

    private LuaScriptGoals DeleteGoals;

    /// <summary>The context-menu items for a script (or a binder of one script): new, duplicate, delete</summary>
    public void ScriptMenuItems(LuaScript script)
    {
        var container = script?.Container;

        if (ImGui.MenuItem($"{LOC.Get("LUA_Struct_New")}##luaNewScript"))
            AskCreate(container, script?.Kind ?? LuaBinderKind.Battle);
        GUI.Tooltip(LOC.Get(container?.Kind == LuaBinderKind.Map ? "LUA_Struct_New_Into_TT" : "LUA_Struct_New_TT", container?.Name ?? ""));

        if (ImGui.MenuItem($"{LOC.Get("LUA_Struct_Duplicate")}##luaDuplicateScript", "", false, IsGoalScript(script)))
            AskDuplicate(script);
        GUI.Tooltip(LOC.Get(IsGoalScript(script) ? "LUA_Struct_Duplicate_TT" : "LUA_Struct_Not_Goal_Script"));

        var refusal = script != null ? DeleteRefusal(script) : LOC.Get("LUA_Struct_Not_Goal_Script");
        if (ImGui.MenuItem($"{LOC.Get("LUA_Struct_Delete")}##luaDeleteScript", "", false, refusal == null))
            AskDelete(script);
        GUI.Tooltip(refusal ?? LOC.Get("LUA_Struct_Delete_TT"));
    }

    /// <summary>The context-menu item for a binder of several scripts: a new script in it (a map's binder), else in a new binder</summary>
    public void BinderMenuItems(LuaContainer container)
    {
        var map = container.Kind == LuaBinderKind.Map;
        if (ImGui.MenuItem($"{LOC.Get("LUA_Struct_New")}##luaNewScriptInto", "", false, !map || container.EnsureLoaded()))
            AskCreate(map ? container : null);
        GUI.Tooltip(LOC.Get(map ? "LUA_Struct_New_Into_TT" : "LUA_Struct_New_TT", container.Name));
    }

    public void Draw()
    {
        if (OpenDialog)
        {
            ImGui.OpenPopup(PopupId);
            OpenDialog = false;
        }

        ImGui.SetNextWindowSize(new Vector2(480, 0) * DPI.UIScale(), ImGuiCond.Always);
        var open = true;
        var title = DialogMode switch
        {
            Mode.Create => LOC.Get("LUA_Struct_New_Title"),
            Mode.Duplicate => LOC.Get("LUA_Struct_Duplicate_Title"),
            _ => LOC.Get("LUA_Struct_Delete_Title"),
        };
        if (!ImGui.BeginPopupModal($"{title}{PopupId}", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        if (DialogMode == Mode.Delete)
            DrawDelete();
        else
            DrawCreateOrDuplicate();

        ImGui.EndPopup();
    }

    private void DrawCreateOrDuplicate()
    {
        var duplicate = DialogMode == Mode.Duplicate;
        if (duplicate && (Source == null || !IsGoalScript(Source)))
        {
            ImGui.CloseCurrentPopup();
            return;
        }

        GUI.WrappedText(duplicate
            ? LOC.Get("LUA_Struct_Duplicate_Hint", Source.EntryName, Source.Container.Name)
            : Into != null ? LOC.Get("LUA_Struct_New_Into_Hint", Into.Name) : LOC.Get("LUA_Struct_New_Hint"));

        if (!duplicate)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("LUA_Struct_Kind_Header"), LOC.Get("LUA_Struct_Kind_Header_TT"));
            if (ImGui.RadioButton($"{LOC.Get("LUA_FileList_Kind_Battle")}##luaNewKindBattle", NewKind == LuaBinderKind.Battle))
                NewKind = LuaBinderKind.Battle;
            ImGui.SameLine();
            if (ImGui.RadioButton($"{LOC.Get("LUA_FileList_Kind_Logic")}##luaNewKindLogic", NewKind == LuaBinderKind.Logic))
                NewKind = LuaBinderKind.Logic;

            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("LUA_Struct_Name_Header"), LOC.Get("LUA_Struct_Name_Header_TT"));
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##luaNewName", ref NameText, 64);
        }

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_Struct_Id_Header"), LOC.Get("LUA_Struct_Id_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.InputText("##luaNewId", ref IdText, 12, ImGuiInputTextFlags.CharsDecimal);

        var key = (DialogMode, Source, Into, NewKind, IdText, NameText);
        // Recomputed while aicommon is still being read (the create plan waits for it)
        if (Plan == null || key != PlanKey || Plan.Refusal == LOC.Get("LUA_Struct_Reading_Common"))
        {
            PlanKey = key;
            var id = int.TryParse(IdText, out var v) ? v : 0;
            Plan = duplicate ? PlanDuplicate(Source, id) : PlanCreate(NewKind, id, NameText, Into);
        }

        GUI.Spacer();
        if (Plan.Refusal != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, Plan.Refusal);
        else
            DrawPlanSummary(Plan);

        GUI.Spacer();
        var plan = Plan;
        GUI.ConditionalMultiButtonInput("luaStructActions",
            "luaStructApply", LOC.Get(duplicate ? "LUA_Struct_Duplicate_Button" : "LUA_Struct_Create_Button"), LOC.Get("LUA_Struct_Apply_TT"),
            () =>
            {
                Apply(plan);
                ImGui.CloseCurrentPopup();
            }, plan.Refusal == null,
            "luaStructCancel", LOC.Get("LUA_Struct_Cancel"), LOC.Get("LUA_Struct_Cancel_TT"),
            ImGui.CloseCurrentPopup, true);
    }

    private static void DrawPlanSummary(LuaScriptPlan plan)
    {
        ImGui.TextDisabled(LOC.Get("LUA_Struct_Will_Add"));
        ImGui.BulletText(plan.Into != null
            ? LOC.Get("LUA_Struct_Adds_Entry", plan.EntryName, plan.Into.Name)
            : LOC.Get("LUA_Struct_Adds_Binder", plan.BinderPath, plan.EntryName));
        foreach (var c in plan.Constants)
            ImGui.BulletText(LOC.Get("LUA_Struct_Adds_Constant", $"{c.Name} = {FormatNumber(c.Value)}", c.List.EntryName));
        foreach (var g in plan.Goals.Concat(plan.BinderGoals ?? new()))
            ImGui.BulletText(LOC.Get("LUA_Struct_Adds_Goal", g.ID, g.Name, plan.Into?.Name ?? plan.BinderName));
        if (plan.Constants.Count == 0 && plan.Goals.Count == 0 && (plan.BinderGoals?.Count ?? 0) == 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("LUA_Struct_No_Goal_Found"));
    }

    private void DrawDelete()
    {
        var script = Source;
        if (script == null || DeleteRefusal(script) != null)
        {
            ImGui.CloseCurrentPopup();
            return;
        }

        var c = script.Container;
        GUI.WrappedText(c.HoldsManyGoals ? LOC.Get("LUA_Struct_Delete_Entry_Hint", script.EntryName, c.Name)
            : c.IsNew ? LOC.Get("LUA_Struct_Delete_New_Hint", c.Name)
            : c.InGame ? LOC.Get("LUA_Struct_Delete_Revert_Hint", c.Name)
            : LOC.Get("LUA_Struct_Delete_Project_Hint", c.Name));

        var goals = DeleteGoals ??= GoalsOf(script);
        if (c.HoldsManyGoals)
        {
            foreach (var g in goals.TableGoals)
                ImGui.BulletText(LOC.Get("LUA_Struct_Removes_Goal", g.ID, g.Name, c.Name));
        }

        if (goals.Constants.Count > 0)
        {
            GUI.Spacer();
            ImGui.Checkbox($"{LOC.Get("LUA_Struct_Remove_Constants")}##luaDeleteConstants", ref RemoveConstants);
            GUI.Tooltip(LOC.Get("LUA_Struct_Remove_Constants_TT"));
            foreach (var (list, k) in goals.Constants)
                ImGui.BulletText($"{k.Name} = {FormatNumber(k.Value)}  ({list.EntryName})");
        }

        GUI.Spacer();
        GUI.ConditionalMultiButtonInput("luaDeleteActions",
            "luaDeleteApply", LOC.Get("LUA_Struct_Delete"), LOC.Get("LUA_Struct_Apply_TT"),
            () =>
            {
                Delete(script, RemoveConstants && goals.Constants.Count > 0);
                ImGui.CloseCurrentPopup();
            }, true,
            "luaDeleteCancel", LOC.Get("LUA_Struct_Cancel"), LOC.Get("LUA_Struct_Cancel_TT"),
            ImGui.CloseCurrentPopup, true);
    }
}
