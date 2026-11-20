namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// Go to definition: a name is looked up in its own script, then the other scripts of its binder, then aicommon (the
/// shared library: helpers, goal IDs in goal_list.lua, enums in ai_define.lua). aicommon is decompiled in the
/// background the first time it is needed; the lookup finishes when it's ready.
/// </summary>
public class LuaDefinitionIndex
{
    private readonly LuaEditorScreen Editor;

    private Dictionary<string, List<(LuaScript Script, int Line)>> Common;
    private Task BuildTask;

    // A lookup waiting for aicommon
    private (LuaScript From, string Name)? Pending;

    /// <summary>A line for the script view while aicommon is being read, or a lookup found nothing; else null</summary>
    public string Status { get; private set; }

    /// <summary>aicommon's numeric globals (goal IDs, enums: NAME = number at the top level); null until it has been read</summary>
    public IReadOnlyDictionary<string, double> Constants { get; private set; }

    /// <summary>
    /// aicommon's global functions (function Name(…) at the top level, first definition kept), for the simulation to run
    /// the helpers scripts call (SpaceCheck); null until aicommon has been read
    /// </summary>
    public IReadOnlyDictionary<string, LuaFunctionExpr> Functions { get; private set; }

    /// <summary>Starts reading aicommon in the background, if not yet (for <see cref="Constants"/>)</summary>
    public void EnsureIndex()
    {
        if (Common == null)
            StartBuild();
    }

    public LuaDefinitionIndex(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    public void GoTo(LuaScript from, string name)
    {
        Status = null;

        if (Find(from, name) is { } own)
        {
            Editor.ShowLine(own.Script, own.Line);
            return;
        }

        foreach (var other in from.Container.Scripts)
        {
            if (other != from && Find(other, name) is { } found)
            {
                Editor.ShowLine(found.Script, found.Line);
                return;
            }
        }

        if (Common == null)
        {
            Pending = (from, name);
            StartBuild();
            return;
        }

        FromCommon(name);
    }

    /// <summary>Called each frame: finishes a lookup that waited for aicommon</summary>
    public void Update()
    {
        if (Pending is { } p && Common != null)
        {
            Pending = null;
            FromCommon(p.Name);
        }
    }

    private void FromCommon(string name)
    {
        if (Common.TryGetValue(name, out var list) && list.Count > 0)
        {
            Status = null;
            Editor.ShowLine(list[0].Script, list[0].Line);
        }
        else
            Status = LOC.Get("LUA_Definition_Not_Found", name);
    }

    private static (LuaScript Script, int Line)? Find(LuaScript script, string name)
    {
        var parse = script.Parse;
        if (parse == null)
            return null;
        foreach (var d in parse.Definitions)
        {
            // "Goal.Activate" is found by either part ("Goal.Activate" or the last name)
            if (d.Name == name || d.Name.EndsWith("." + name) || d.Name.EndsWith(":" + name))
                return (script, d.Line);
        }
        return null;
    }

    private void StartBuild()
    {
        if (BuildTask != null)
            return;

        var common = Editor.Bank?.Containers.FirstOrDefault(c => c.Kind == LuaBinderKind.Common);
        if (common == null)
        {
            Common = new();
            Functions = new Dictionary<string, LuaFunctionExpr>();
            Constants = new Dictionary<string, double>();
            return;
        }

        Status = LOC.Get("LUA_Definition_Indexing", common.Name);
        BuildTask = Task.Run(() =>
        {
            var index = new Dictionary<string, List<(LuaScript, int)>>();
            var constants = new Dictionary<string, double>();
            var functions = new Dictionary<string, LuaFunctionExpr>();
            try
            {
                if (common.EnsureLoaded())
                {
                    foreach (var script in common.Scripts)
                    {
                        var parse = script.Parse;
                        if (parse == null)
                            continue;
                        // Not from a script the decompiler couldn't fully turn into text: its functions are missing code, so
                        // running one would give a wrong answer (DB_NT common_func_PHX: ReturnBuddyOwnerLockOnTarget is empty)
                        if (!script.IsDecompileIncomplete)
                            parse.Tree?.AddGlobalFunctions(functions);
                        if (parse.Tree != null)
                        {
                            foreach (var s in parse.Tree.Stats)
                            {
                                if (s is LuaAssignStat { Targets.Count: 1, Values.Count: 1 } a && a.Targets[0] is LuaNameExpr n &&
                                    a.Values[0].AsNumber() is { } v)
                                    constants[n.Name] = v;
                            }
                        }
                        foreach (var d in parse.Definitions)
                        {
                            foreach (var key in new[] { d.Name, d.Name.Split('.', ':')[^1] }.Distinct())
                            {
                                if (!index.TryGetValue(key, out var list))
                                    index[key] = list = new();
                                list.Add((script, d.Line));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("LUA_Definition_Index_Failed", common.Name), e);
            }
            // Functions before Constants: the battle view rebuilds once Constants is set
            Functions = functions;
            Constants = constants;
            Common = index;
            Status = null;
        });
    }
}
