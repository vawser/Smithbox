namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// Holds the data for the Lua Editor: the project's Lua binders.
/// Data Flow: the binder list at setup, each binder read on first use, each script decompiled when first shown.
/// </summary>
public class LuaData : IDisposable
{
    public ProjectEntry Project;

    public LuaBank PrimaryBank;

    public LuaData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new LuaBank(Project, Project.VFS.FS);

        var result = PrimaryBank.Setup();

        if (result)
        {
            Smithbox.Log(this, LOC.Get("LUA_Data_Setup_Primary_Bank_PASS", PrimaryBank.Containers.Count));

            // A map's binder (SDT, BB, DS3) and aicommon hold many goals: read their entry lists now (not decompiled),
            // so the file list's filter can find a script by its name, character or NpcThinkParam users
            var bank = PrimaryBank;
            _ = Task.Run(() =>
            {
                foreach (var c in bank.Containers.Where(c => c.HoldsManyGoals))
                    c.EnsureLoaded();
            });
        }
        else
            Smithbox.LogError(this, LOC.Get("LUA_Data_Setup_Primary_Bank_FAIL"));

        return result;
    }

    // NpcThinkParam rows by the goal ID their logicId / battleGoalID name, made on first use
    private Dictionary<int, List<(int Id, string Name)>> _users;
    private object _usersParam;

    /// <summary>
    /// The NpcThinkParam rows whose logicId or battleGoalID is this goal ID (a binder's number). Empty when the
    /// project has no such param or fields (the lookup is driven by the fields, not by game).
    /// </summary>
    public IReadOnlyList<(int Id, string Name)> UsersOf(int goalId)
    {
        var bank = Project.Handler.ParamData?.PrimaryBank;
        if (bank == null || !bank.Params.TryGetValue("NpcThinkParam", out var param))
            return Array.Empty<(int, string)>();

        if (_users == null || !ReferenceEquals(_usersParam, param))
        {
            _users = new();
            _usersParam = param;
            foreach (var row in param.Rows)
            {
                foreach (var field in new[] { "logicId", "battleGoalID" })
                {
                    var cell = row[field];
                    if (cell == null)
                        continue;
                    var id = Convert.ToInt32(cell.Value.Value);
                    if (id <= 0)
                        continue;
                    if (!_users.TryGetValue(id, out var list))
                        _users[id] = list = new();
                    if (!list.Any(u => u.Id == row.ID))
                        list.Add((row.ID, row.Name ?? ""));
                }
            }
        }

        return _users.TryGetValue(goalId, out var users) ? users : Array.Empty<(int, string)>();
    }

    /// <summary>Forget the NpcThinkParam lookup (rows were edited)</summary>
    public void RefreshUsers() => _users = null;

    public void Dispose()
    {
        PrimaryBank = null;
        _users = null;
    }
}
