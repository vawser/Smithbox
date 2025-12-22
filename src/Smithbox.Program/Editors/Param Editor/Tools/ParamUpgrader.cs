using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamUpgrader
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamUpgrader(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (Project.ParamData == null)
            return;

        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);

        if (ImGui.CollapsingHeader("Param Upgrader"))
        {
            UpgraderMenu();
        }
    }

    public bool SupportsParamUpgrading(ProjectEntry curProject)
    {
        if (curProject.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            return true;

        return false;
    }

    public void ParamUpgradeWarning(ProjectEntry curProject)
    {
        if (SupportsParamUpgrading(curProject) && curProject.Initialized && curProject.IsSelected)
        {
            if (curProject.ParamData.PrimaryBank.ParamVersion < curProject.ParamData.VanillaBank.ParamVersion)
            {
                var primaryVersion = ParamUtils.ParseRegulationVersion(curProject.ParamData.PrimaryBank.ParamVersion);
                var vanillaVersion = ParamUtils.ParseRegulationVersion(curProject.ParamData.VanillaBank.ParamVersion);

                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, $"Project primary bank version is below current game version: {primaryVersion} < {vanillaVersion} -- Use the Param Upgrader in the Tool window.");
            }
        }
    }

    private bool DisplayActions = false;
    private bool ConflictsChecked = false;
    private bool UpgradePerformed = false;
    private bool MassEditsPerformed = false;

    private ParamUpgraderInfo UpgraderInfo;
    private BND4 OldRegulationBinder;

    private Dictionary<string, HashSet<int>> ConflictParams;
    private Dictionary<string, Param> OldRegulationParams;
    private Dictionary<string, Param> NewParams;

    public void UpgraderMenu()
    {
        var windowWidth = 620;

        var primaryBank = Project.ParamData.PrimaryBank;
        var vanillaBank = Project.ParamData.VanillaBank;

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        UIHelper.SimpleHeader("versionHeader", "Version", "", UI.Current.ImGui_AliasName_Text);

        if (ImGui.BeginTable($"upgraderInfo", 2, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

            // Primary
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Primary Param Version");

            ImGui.TableSetColumnIndex(1);

            ImGui.Text($"{primaryBank.ParamVersion}");

            // Source
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Source Param Version");

            ImGui.TableSetColumnIndex(1);

            ImGui.Text($"{vanillaBank.ParamVersion}");

            ImGui.EndTable();
        }

        UIHelper.SimpleHeader("actionHeader", "Actions", "", UI.Current.ImGui_AliasName_Text);

        // Start
        if (!DisplayActions && Project.ParamData.PrimaryBank.ParamVersion < Project.ParamData.VanillaBank.ParamVersion)
        {
            if (ImGui.Button("Start", DPI.WholeWidthButton(windowWidth, 24)))
            {
                Start();
            }
        }

        if(!DisplayActions && Project.ParamData.PrimaryBank.ParamVersion >= Project.ParamData.VanillaBank.ParamVersion 
            || DisplayActions && ConflictsChecked && UpgradePerformed && MassEditsPerformed)
        {
            UIHelper.WrappedText("No need to upgrade params.");
        }

        // Conflicts
        if (DisplayActions && !ConflictsChecked)
        {
            if (ImGui.Button("Check for Conflicts", DPI.WholeWidthButton(windowWidth, 24)))
            {
                CheckForConflicts();
            }
        }

        // Apply Upgrade
        if (DisplayActions && ConflictsChecked && !UpgradePerformed)
        {
            if (ImGui.Button("Apply Upgrade", DPI.WholeWidthButton(windowWidth, 24)))
            {
                UpgradeParams();
            }
        }

        // Apply Mass Edit
        if (DisplayActions && ConflictsChecked && UpgradePerformed && !MassEditsPerformed)
        {
            if (ImGui.Button("Apply Mass Edits", DPI.WholeWidthButton(windowWidth, 24)))
            {
                ApplyMassEdits();
            }
        }

        // Conflict List
        if (ConflictsChecked)
        {
            if (ConflictParams.Count > 0)
            {
                UIHelper.SimpleHeader("conflictHeader", "Conflicts", "", UI.Current.ImGui_Warning_Text_Color);

                if (ImGui.BeginTable($"conflictTable", 2, tblFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    foreach (var entry in ConflictParams)
                    {
                        // Primary
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);

                        ImGui.AlignTextToFramePadding();
                        ImGui.Text($"{entry.Key}");

                        ImGui.TableSetColumnIndex(1);

                        foreach(var cEntry in entry.Value)
                        {
                            ImGui.Text($"{cEntry}");
                        }
                    }

                    ImGui.EndTable();
                }
            }
        }
    }

    public async void Start()
    {
        UpgraderInfo = null;
        OldRegulationBinder = null;

        DisplayActions = false;
        ConflictsChecked = false;
        UpgradePerformed = false;

        NewParams = new();
        OldRegulationParams = new();
        ConflictParams = new();

        await SetupUpgraderInformation();
        await SetupOldRegulation();

        DisplayActions = true;
    }

    public async Task<bool> SetupUpgraderInformation()
    {
        Task<bool> upgraderInfoTask = LoadUpgraderInformation();
        bool upgraderInfoTaskResult = await upgraderInfoTask;

        if (!upgraderInfoTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to find load upgrader information.");
        }

        return true;
    }

    public async Task<bool> SetupOldRegulation()
    {
        Task<bool> oldRegTask = LoadOldRegulation();
        bool oldRegTaskFinished = await oldRegTask;

        if (!oldRegTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to find old regulation file.");
        }

        return true;
    }

    private bool ApplySpecialHandlingForNR = false;

    public async void UpgradeParams()
    {
        // Special handling for the moved fields in the NR 1.03.1 -> 1.03.2 update
        if(Project.ProjectType is ProjectType.NR && Project.ParamData.PrimaryBank.ParamVersion == 10310025)
        {
            ApplySpecialHandlingForNR = true;

            await StoreParamData();
        }

        Task<bool> upgradeTask = UpgradeParamsTask();
        bool upgradeTaskFinished = await upgradeTask;

        if (upgradeTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Upgraded primary bank params successfully.");

            UpgradePerformed = true;
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Primary bank is already fully upgraded.");
        }

        UICache.ClearCaches();
        Project.ParamData.RefreshAllParamDiffCaches(false);

        await Project.ParamData.PrimaryBank.Save();
    }

    // Special handling for the moved fields in the NR 1.03.1 -> 1.03.2 update
    public async Task<bool> StoreParamData()
    {
        Task<bool> spEffectTask = StoreSpEffectData();
        bool spEffectTaskResult = await spEffectTask;

        if (!spEffectTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to store SpEffect data.");
        }

        return true;
    }

    private Dictionary<int, Dictionary<string, object>> SpEffectData = new();

    // Special handling for the moved fields in the NR 1.03.1 -> 1.03.2 update
    public async Task<bool> StoreSpEffectData()
    {
        await Task.Yield();

        SpEffectData.Clear();

        var spEffectParam = Project.ParamData.PrimaryBank.Params["SpEffectParam"];
        var spEffectParamVanilla = Project.ParamData.VanillaBank.Params["SpEffectParam"];

        foreach (var row in spEffectParam.Rows)
        {
            // Only add 'added' rows, the default mass edit covers the vanilla ones
            if (spEffectParamVanilla.Rows.Any(e => e.ID == row.ID))
                continue;

            var frostInflict = row["frostInflictRate_old"];
            var sleepInflict = row["sleepInflictRate_old"];
            var madnessInflict = row["madnessInflictRate_old"];
            var applyOnKillSp = row["applyIdOnKillSp_old"];

            if (!SpEffectData.ContainsKey(row.ID))
            {
                SpEffectData.Add(row.ID, new());
            }

            SpEffectData[row.ID].Add("frostInflictRate", frostInflict.Value.Value);
            SpEffectData[row.ID].Add("sleepInflictRate", sleepInflict.Value.Value);
            SpEffectData[row.ID].Add("madnessInflictRate", madnessInflict.Value.Value);
            SpEffectData[row.ID].Add("applyIdOnKillSp", applyOnKillSp.Value.Value);
        }

        return true;
    }

    public async void ApplyMassEdits()
    {
        var newVersion = Project.ParamData.VanillaBank.ParamVersion;

        // Update fields
        var massEditCmds = UpgraderInfo.UpgradeCommands.Where(e => e.Version == $"{newVersion}").ToList();

        var commandString = "";
        foreach (var cmd in massEditCmds)
        {
            commandString = $"{commandString}{cmd.Command};\n";
        }

        Project.ParamEditor.MassEditHandler.ApplyMassEdit(commandString);
        TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Applied upgrader mass edit commands", LogLevel.Information, LogPriority.Normal);

        // Special handling for the moved fields in the NR 1.03.1 -> 1.03.2 update
        if (Project.ProjectType is ProjectType.NR && ApplySpecialHandlingForNR)
        {
            commandString = "";

            foreach (var entry in SpEffectData)
            {
                var rowID = entry.Key;
                var fieldData = entry.Value;

                var command = $"param SpEffectParam: id {rowID}: frostInflictRate: = {fieldData["frostInflictRate"]};" +
                    $"param SpEffectParam: id {rowID}: sleepInflictRate: = {fieldData["sleepInflictRate"]};" +
                    $"param SpEffectParam: id {rowID}: madnessInflictRate: = {fieldData["madnessInflictRate"]};" +
                    $"param SpEffectParam: id {rowID}: applyIdOnKillSp: = {fieldData["applyIdOnKillSp"]};";

                commandString = $"{commandString}\n{command}";
            }

            Project.ParamEditor.MassEditHandler.ApplyMassEdit(commandString);
        }

        await Project.ParamData.PrimaryBank.Save();

        MassEditsPerformed = true;
    }

    public async Task<bool> UpgradeParamsTask()
    {
        await Task.Yield();

        // Backup original
        var data = Project.ProjectFS.GetFile(@"regulation.bin")?.GetData().ToArray();

        if (CFG.Current.EnableBackupSaves)
        {
            File.WriteAllBytes(Path.Join(Project.ProjectPath, "regulation.bin.prev"), data);
        }

        NewParams = new();

        var anyUpgrades = false;

        var primaryBank = Project.ParamData.PrimaryBank;
        var vanillaBank = Project.ParamData.VanillaBank;

        foreach (var k in vanillaBank.Params.Keys)
        {
            // If the param is completely new, just take it
            if (!OldRegulationParams.ContainsKey(k) || !primaryBank.Params.ContainsKey(k))
            {
                NewParams.Add(k, vanillaBank.Params[k]);
                continue;
            }

            // Otherwise try to upgrade
            HashSet<int> conflicts = new();

            // Process the param
            Param res = ProcessParam(
                primaryBank.Params[k], OldRegulationParams[k], vanillaBank.Params[k],
                conflicts);

            if (res != OldRegulationParams[k])
                anyUpgrades = true;

            NewParams.Add(k, res);
        }

        if (!anyUpgrades)
        {
            return false;
        }

        // Set new params
        primaryBank._params = NewParams;
        primaryBank._paramVersion = vanillaBank.ParamVersion;
        primaryBank._pendingUpgrade = true;

        UICache.ClearCaches();
        Project.ParamData.RefreshAllParamDiffCaches(false);

        return true;
    }

    public async void CheckForConflicts()
    {
        Task<bool> conflictTask = ConflictCheckTask();
        bool conflictTaskFinished = await conflictTask;

        if (conflictTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Checked for conflicts successfully.");

            ConflictsChecked = true;
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to check for conflicts.");
        }
    }

    public async Task<bool> ConflictCheckTask()
    {
        await Task.Yield();

        var primaryBank = Project.ParamData.PrimaryBank;
        var vanillaBank = Project.ParamData.VanillaBank;

        foreach (var k in vanillaBank.Params.Keys)
        {
            // If the param is completely new, just take it
            if (!OldRegulationParams.ContainsKey(k) || !primaryBank.Params.ContainsKey(k))
            {
                continue;
            }

            // Otherwise try to upgrade
            HashSet<int> conflicts = new();

            // Process the param
            ProcessConflicts(primaryBank.Params[k], OldRegulationParams[k], vanillaBank.Params[k], conflicts);

            if (conflicts.Count > 0)
            {
                ConflictParams.Add(k, conflicts);
            }
        }

        return true;
    }

    public async Task<bool> LoadUpgraderInformation()
    {
        await Task.Yield();

        var oldRegInfoPath = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Upgrader Information.json");

        try
        {
            var filestring = File.ReadAllText(oldRegInfoPath);

            try
            {
                UpgraderInfo = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.ParamUpgraderInfo);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deserialize Upgrader Information.", LogLevel.Error, LogPriority.High, e);
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to load Upgrader Information.", LogLevel.Error, LogPriority.High, e);
        }

        return true;
    }

    public async Task<bool> LoadOldRegulation()
    {
        await Task.Yield();

        var oldRegPath = GetOldRegulationPath();

        if (!File.Exists(oldRegPath))
        {
            return false;
        }

        if (Project.ProjectType == ProjectType.ER)
        {
            OldRegulationBinder = SFUtil.DecryptERRegulation(oldRegPath);
        }
        else if (Project.ProjectType == ProjectType.AC6)
        {
            OldRegulationBinder = SFUtil.DecryptAC6Regulation(oldRegPath);
        }
        else if (Project.ProjectType == ProjectType.NR)
        {
            OldRegulationBinder = SFUtil.DecryptNightreignRegulation(oldRegPath);
        }

        OldRegulationParams = new();
        ulong version;

        var success = LoadParamFromBinder(OldRegulationBinder, ref OldRegulationParams, out version);

        return success;
    }

    private string GetOldRegulationPath()
    {
        if (UpgraderInfo == null)
            return "";

        var primaryBank = Project.ParamData.PrimaryBank;
        var oldVersionString = ParamUtils.ParseRegulationVersion(primaryBank.ParamVersion);

        var oldRegulationPath = "";

        var oldRegDirectory = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Regulations");

        var targetRegInfo = UpgraderInfo.RegulationEntries.Where(e => e.Version == oldVersionString).FirstOrDefault();

        if (targetRegInfo != null)
        {
            oldRegulationPath = Path.Join(oldRegDirectory, targetRegInfo.Folder, "regulation.bin");
        }

        return oldRegulationPath;
    }

    /// <summary>
    /// Non-VFS version just for the old regulation
    /// </summary>
    private bool LoadParamFromBinder(IBinder parambnd, ref Dictionary<string, Param> paramBank, out ulong version)
    {
        var successfulLoad = true;

        var success = ulong.TryParse(parambnd.Version, out version);

        if (!success)
        {
            throw new Exception(@"Failed to get regulation version. Params might be corrupt.");
        }

        // Load every param in the regulation
        foreach (BinderFile f in parambnd.Files)
        {
            var paramName = Path.GetFileNameWithoutExtension(f.Name);

            if (!f.Name.ToUpper().EndsWith(".PARAM"))
            {
                continue;
            }

            if (paramBank.ContainsKey(paramName))
            {
                continue;
            }

            Param p;

            // AC6/SDT - Tentative ParamTypes
            if (Project.ProjectType is ProjectType.AC6 or ProjectType.SDT)
            {
                p = Param.ReadIgnoreCompression(f.Bytes);
                if (!string.IsNullOrEmpty(p.ParamType))
                {
                    if (!Project.ParamData.ParamDefs.ContainsKey(p.ParamType) || paramName == "EquipParamWeapon_Npc")
                    {
                        if (Project.ParamData.ParamTypeInfo.Mapping.TryGetValue(paramName, out var newParamType))
                        {
                            p.ParamType = newParamType;
                        }
                        else
                        {
                            TaskLogs.AddLog(
                                $"Couldn't find ParamDef for param {paramName} and no tentative ParamType exists.",
                                LogLevel.Error);

                            successfulLoad = false;
                            continue;
                        }
                    }
                }
                else
                {
                    if (Project.ParamData.ParamTypeInfo.Mapping.TryGetValue(paramName, out var newParamType))
                    {
                        p.ParamType = newParamType;
                    }
                    else
                    {
                        TaskLogs.AddLog(
                            $"Couldn't read ParamType for {paramName} and no tentative ParamType exists.",
                            LogLevel.Error);

                        successfulLoad = false;
                        continue;
                    }
                }
            }
            else
            {
                p = Param.ReadIgnoreCompression(f.Bytes);
                if (!Project.ParamData.ParamDefs.ContainsKey(p.ParamType ?? ""))
                {
                    TaskLogs.AddLog(
                        $"Couldn't find ParamDef for param {paramName} with ParamType \"{p.ParamType}\".",
                        LogLevel.Warning);

                    successfulLoad = false;
                    continue;
                }
            }

            // Try to fixup Elden Ring ChrModelParam for ER 1.06 because many have been saving botched params and
            // it's an easy fixup
            if (Project.ProjectType == ProjectType.ER && version >= 10601000)
            {
                if (p.ParamType == "CHR_MODEL_PARAM_ST")
                {
                    if (p.ExpandParamSize(12, 16))
                        TaskLogs.AddLog($"CHR_MODEL_PARAM_ST fixed up.");
                }
            }

            // Add in the new data for these two params added in 1.12.1
            if (Project.ProjectType == ProjectType.ER && version >= 11210015)
            {
                if (p.ParamType == "GAME_SYSTEM_COMMON_PARAM_ST")
                {
                    if (p.ExpandParamSize(880, 1024))
                        TaskLogs.AddLog($"GAME_SYSTEM_COMMON_PARAM_ST fixed up.");
                }
                if (p.ParamType == "POSTURE_CONTROL_PARAM_WEP_RIGHT_ST")
                {
                    if (p.ExpandParamSize(112, 144))
                        TaskLogs.AddLog($"POSTURE_CONTROL_PARAM_WEP_RIGHT_ST fixed up.");
                }
                if (p.ParamType == "SIGN_PUDDLE_PARAM_ST")
                {
                    if (p.ExpandParamSize(32, 48))
                        TaskLogs.AddLog($"SIGN_PUDDLE_PARAM_ST fixed up.");
                }
            }

            if (p.ParamType == null)
            {
                throw new Exception("Param type is unexpectedly null");
            }

            // Skip these for DS1 so the param load is not slowed down by the catching
            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                if (paramName is "m99_ToneCorrectBank" or "m99_ToneMapBank" or "default_ToneCorrectBank")
                {
                    TaskLogs.AddLog($"Skipped this param: {paramName}");
                    continue;
                }
            }

            // VAW: this is grabbing the defs from the primary/vanilla param bank stuff
            PARAMDEF def = Project.ParamData.ParamDefs[p.ParamType];

            try
            {
                p.ApplyParamdef(def, version);
                paramBank.Add(paramName, p);
            }
            catch (Exception e)
            {
                var name = f.Name.Split("\\").Last();
                var message = $"Could not apply ParamDef for {name}";

                successfulLoad = false;
                TaskLogs.AddLog(message, LogLevel.Warning, LogPriority.Normal, e);
            }
        }

        return successfulLoad;
    }

    private void ProcessConflicts(Param source, Param oldVanilla, Param newVanilla, HashSet<int> rowConflicts)
    {
        // List of rows that are in source but not oldVanilla
        Dictionary<int, List<Param.Row>> addedRows = new(source.Rows.Count);

        // List of rows in oldVanilla that aren't in source
        Dictionary<int, List<Param.Row>> deletedRows = new(source.Rows.Count);

        // List of rows that are in source and oldVanilla, but are modified
        Dictionary<int, List<Param.Row>> modifiedRows = new(source.Rows.Count);

        // List of rows that only had the name changed
        Dictionary<int, List<Param.Row>> renamedRows = new(source.Rows.Count);

        // List of ordered edit operations for each ID
        Dictionary<int, List<ParamUpgradeEditOperation>> editOperations = new(source.Rows.Count);

        // First off we go through source and everything starts as an added param
        foreach (Param.Row row in source.Rows)
        {
            if (!addedRows.ContainsKey(row.ID))
            {
                addedRows.Add(row.ID, new List<Param.Row>());
            }

            addedRows[row.ID].Add(row);
        }

        // Next we go through oldVanilla to determine if a row is added, deleted, modified, or unmodified
        foreach (Param.Row row in oldVanilla.Rows)
        {
            // First off if the row did not exist in the source, it's deleted
            if (!addedRows.ContainsKey(row.ID))
            {
                if (!deletedRows.ContainsKey(row.ID))
                {
                    deletedRows.Add(row.ID, new List<Param.Row>());
                }

                deletedRows[row.ID].Add(row);

                if (!editOperations.ContainsKey(row.ID))
                {
                    editOperations.Add(row.ID, new List<ParamUpgradeEditOperation>());
                }

                editOperations[row.ID].Add(ParamUpgradeEditOperation.Delete);

                continue;
            }

            // Otherwise the row exists in source. Time to classify it.
            List<Param.Row> list = addedRows[row.ID];

            // First we see if we match the first target row. If so we can remove it.
            if (row.DataEquals(list[0]))
            {
                Param.Row modrow = list[0];
                list.RemoveAt(0);
                if (list.Count == 0)
                {
                    addedRows.Remove(row.ID);
                }

                if (!editOperations.ContainsKey(row.ID))
                {
                    editOperations.Add(row.ID, new List<ParamUpgradeEditOperation>());
                }

                // See if the name was not updated
                if (modrow.Name == null && row.Name == null ||
                    modrow.Name != null && row.Name != null && modrow.Name == row.Name)
                {
                    editOperations[row.ID].Add(ParamUpgradeEditOperation.Match);
                    continue;
                }

                // Name was updated
                editOperations[row.ID].Add(ParamUpgradeEditOperation.NameChange);

                if (!renamedRows.ContainsKey(row.ID))
                {
                    renamedRows.Add(row.ID, new List<Param.Row>());
                }

                renamedRows[row.ID].Add(modrow);

                continue;
            }

            // Otherwise it is modified
            if (!modifiedRows.ContainsKey(row.ID))
            {
                modifiedRows.Add(row.ID, new List<Param.Row>());
            }

            modifiedRows[row.ID].Add(list[0]);
            list.RemoveAt(0);
            if (list.Count == 0)
            {
                addedRows.Remove(row.ID);
            }

            if (!editOperations.ContainsKey(row.ID))
            {
                editOperations.Add(row.ID, new List<ParamUpgradeEditOperation>());
            }

            editOperations[row.ID].Add(ParamUpgradeEditOperation.Modify);
        }

        // Mark all remaining rows as added
        foreach (KeyValuePair<int, List<Param.Row>> entry in addedRows)
        {
            if (!editOperations.ContainsKey(entry.Key))
            {
                editOperations.Add(entry.Key, new List<ParamUpgradeEditOperation>());
            }

            foreach (List<ParamUpgradeEditOperation> k in editOperations.Values)
            {
                editOperations[entry.Key].Add(ParamUpgradeEditOperation.Add);
            }
        }

        // Now try to build the destination from the new regulation with the edit operations in mind
        var pendingAdds = addedRows.Keys.OrderBy(e => e).ToArray();
        var currPendingAdd = 0;
        var lastID = 0;

        foreach (Param.Row row in newVanilla.Rows)
        {
            // See if we have any pending adds we can slot in
            while (currPendingAdd < pendingAdds.Length &&
                   pendingAdds[currPendingAdd] >= lastID &&
                   pendingAdds[currPendingAdd] < row.ID)
            {
                if (!addedRows.ContainsKey(pendingAdds[currPendingAdd]))
                {
                    currPendingAdd++;
                    continue;
                }

                foreach (Param.Row arow in addedRows[pendingAdds[currPendingAdd]])
                {
                    // ADDED ROW
                    //TargetLog(source, $"newVanilla - AddRow");
                }

                addedRows.Remove(pendingAdds[currPendingAdd]);
                editOperations.Remove(pendingAdds[currPendingAdd]);
                currPendingAdd++;
            }

            lastID = row.ID;

            if (!editOperations.ContainsKey(row.ID))
            {
                // ADDED ROW
                continue;
            }

            // Pop the latest operation we need to do
            ParamUpgradeEditOperation operation = editOperations[row.ID][0];
            editOperations[row.ID].RemoveAt(0);

            if (editOperations[row.ID].Count == 0)
            {
                editOperations.Remove(row.ID);
            }

            if (operation == ParamUpgradeEditOperation.Add)
            {
                // Getting here means both the mod and the updated regulation added a row. Our current strategy is
                // to overwrite the new vanilla row with the modded one and add to the conflict log to give the user
                rowConflicts.Add(row.ID);

                // ADDED CONFLICTED ROW

                addedRows[row.ID].RemoveAt(0);

                if (addedRows[row.ID].Count == 0)
                {
                    addedRows.Remove(row.ID);
                }
            }
            else if (operation == ParamUpgradeEditOperation.Match)
            {
                // Match means we inherit updated param
                // INHERIT NEW ROW
            }
            else if (operation == ParamUpgradeEditOperation.Delete)
            {
                // deleted means we don't add anything
                deletedRows[row.ID].RemoveAt(0);
                if (deletedRows[row.ID].Count == 0)
                {
                    deletedRows.Remove(row.ID);
                }
            }
            else if (operation == ParamUpgradeEditOperation.Modify)
            {
                // Modified means we use the modded regulation's param
                // MODIFIED PARAM
                modifiedRows[row.ID].RemoveAt(0);
                if (modifiedRows[row.ID].Count == 0)
                {
                    modifiedRows.Remove(row.ID);
                }
            }
            else if (operation == ParamUpgradeEditOperation.NameChange)
            {
                // Inherit name
                // INHERIT ROW NAME
                renamedRows[row.ID].RemoveAt(0);
                if (renamedRows[row.ID].Count == 0)
                {
                    renamedRows.Remove(row.ID);
                }
            }
        }
    }

    private Param ProcessParam(Param source, Param oldVanilla, Param newVanilla, HashSet<int> rowConflicts)
    {
        // Presorting this would make it easier, but we're trying to preserve order as much as possible
        // Unfortunately given that rows aren't guaranteed to be sorted and there can be duplicate IDs,
        // we try to respect the existing order and IDs as much as possible.

        // In order to assemble the final param, the param needs to know where to sort rows from given the
        // following rules:
        // 1. If a row with a given ID is unchanged from source to oldVanilla, we source from newVanilla
        // 2. If a row with a given ID is deleted from source compared to oldVanilla, we don't take any row
        // 3. If a row with a given ID is changed from source compared to oldVanilla, we source from source
        // 4. If a row has duplicate IDs, we treat them as if the rows were deduplicated and process them
        //    in the order they appear.

        // List of rows that are in source but not oldVanilla
        Dictionary<int, List<Param.Row>> addedRows = new(source.Rows.Count);

        // List of rows in oldVanilla that aren't in source
        Dictionary<int, List<Param.Row>> deletedRows = new(source.Rows.Count);

        // List of rows that are in source and oldVanilla, but are modified
        Dictionary<int, List<Param.Row>> modifiedRows = new(source.Rows.Count);

        // List of rows that only had the name changed
        Dictionary<int, List<Param.Row>> renamedRows = new(source.Rows.Count);

        // List of ordered edit operations for each ID
        Dictionary<int, List<ParamUpgradeEditOperation>> editOperations = new(source.Rows.Count);

        // First off we go through source and everything starts as an added param
        foreach (Param.Row row in source.Rows)
        {
            if (!addedRows.ContainsKey(row.ID))
            {
                addedRows.Add(row.ID, new List<Param.Row>());
            }

            addedRows[row.ID].Add(row);
            //TargetLog(source, $"Source - Add row: {row.ID}");
        }

        // Next we go through oldVanilla to determine if a row is added, deleted, modified, or unmodified
        foreach (Param.Row row in oldVanilla.Rows)
        {
            // First off if the row did not exist in the source, it's deleted
            if (!addedRows.ContainsKey(row.ID))
            {
                if (!deletedRows.ContainsKey(row.ID))
                {
                    deletedRows.Add(row.ID, new List<Param.Row>());
                }

                deletedRows[row.ID].Add(row);

                if (!editOperations.ContainsKey(row.ID))
                {
                    editOperations.Add(row.ID, new List<ParamUpgradeEditOperation>());
                }

                editOperations[row.ID].Add(ParamUpgradeEditOperation.Delete);
                //TargetLog(source, $"oldVanilla - EditOperation.Delete: {row.ID}");

                continue;
            }

            // Otherwise the row exists in source. Time to classify it.
            List<Param.Row> list = addedRows[row.ID];

            // First we see if we match the first target row. If so we can remove it.
            if (row.DataEquals(list[0]))
            {
                Param.Row modrow = list[0];
                list.RemoveAt(0);
                if (list.Count == 0)
                {
                    addedRows.Remove(row.ID);
                }

                if (!editOperations.ContainsKey(row.ID))
                {
                    editOperations.Add(row.ID, new List<ParamUpgradeEditOperation>());
                }

                // See if the name was not updated
                if (modrow.Name == null && row.Name == null ||
                    modrow.Name != null && row.Name != null && modrow.Name == row.Name)
                {
                    editOperations[row.ID].Add(ParamUpgradeEditOperation.Match);
                    //TargetLog(source, $"oldVanilla - EditOperation.Match: {row.ID}");
                    continue;
                }

                // Name was updated
                editOperations[row.ID].Add(ParamUpgradeEditOperation.NameChange);
                //TargetLog(source, $"oldVanilla - EditOperation.NameChange: {row.ID}");

                if (!renamedRows.ContainsKey(row.ID))
                {
                    renamedRows.Add(row.ID, new List<Param.Row>());
                }

                renamedRows[row.ID].Add(modrow);

                continue;
            }

            // Otherwise it is modified
            if (!modifiedRows.ContainsKey(row.ID))
            {
                modifiedRows.Add(row.ID, new List<Param.Row>());
            }

            modifiedRows[row.ID].Add(list[0]);
            list.RemoveAt(0);
            if (list.Count == 0)
            {
                addedRows.Remove(row.ID);
            }

            if (!editOperations.ContainsKey(row.ID))
            {
                editOperations.Add(row.ID, new List<ParamUpgradeEditOperation>());
            }

            editOperations[row.ID].Add(ParamUpgradeEditOperation.Modify);
            //TargetLog(source, $"oldVanilla - EditOperation.Modify: {row.ID}");
        }

        // Mark all remaining rows as added
        foreach (KeyValuePair<int, List<Param.Row>> entry in addedRows)
        {
            if (!editOperations.ContainsKey(entry.Key))
            {
                editOperations.Add(entry.Key, new List<ParamUpgradeEditOperation>());
            }

            foreach (List<ParamUpgradeEditOperation> k in editOperations.Values)
            {
                editOperations[entry.Key].Add(ParamUpgradeEditOperation.Add);
                //TargetLog(source, $"oldVanilla - EditOperation.Add: {entry.Key}");
            }
        }

        Param dest = new(newVanilla);

        // Now try to build the destination from the new regulation with the edit operations in mind
        var pendingAdds = addedRows.Keys.OrderBy(e => e).ToArray();
        var currPendingAdd = 0;
        var lastID = 0;
        foreach (Param.Row row in newVanilla.Rows)
        {
            //TargetLog(source, $"newVanilla row");

            // See if we have any pending adds we can slot in
            while (currPendingAdd < pendingAdds.Length &&
                   pendingAdds[currPendingAdd] >= lastID &&
                   pendingAdds[currPendingAdd] < row.ID)
            {
                if (!addedRows.ContainsKey(pendingAdds[currPendingAdd]))
                {
                    currPendingAdd++;
                    //TargetLog(source, $"newVanilla - currPendingAdd: {pendingAdds[currPendingAdd-1]}");
                    continue;
                }

                foreach (Param.Row arow in addedRows[pendingAdds[currPendingAdd]])
                {
                    dest.AddRow(new Param.Row(arow, dest));
                    //TargetLog(source, $"newVanilla - AddRow");
                }

                addedRows.Remove(pendingAdds[currPendingAdd]);
                editOperations.Remove(pendingAdds[currPendingAdd]);
                currPendingAdd++;
            }

            lastID = row.ID;

            if (!editOperations.ContainsKey(row.ID))
            {
                // No edit operations for this ID, so just add it (likely a new row in the update)
                dest.AddRow(new Param.Row(row, dest));
                //TargetLog(source, $"newVanilla - AddRow (New)");
                continue;
            }

            // Pop the latest operation we need to do
            ParamUpgradeEditOperation operation = editOperations[row.ID][0];
            editOperations[row.ID].RemoveAt(0);

            if (editOperations[row.ID].Count == 0)
            {
                editOperations.Remove(row.ID);
            }

            if (operation == ParamUpgradeEditOperation.Add)
            {
                // Getting here means both the mod and the updated regulation added a row. Our current strategy is
                // to overwrite the new vanilla row with the modded one and add to the conflict log to give the user
                rowConflicts.Add(row.ID);
                dest.AddRow(new Param.Row(addedRows[row.ID][0], dest));
                addedRows[row.ID].RemoveAt(0);

                if (addedRows[row.ID].Count == 0)
                {
                    addedRows.Remove(row.ID);
                }
            }
            else if (operation == ParamUpgradeEditOperation.Match)
            {
                // Match means we inherit updated param
                dest.AddRow(new Param.Row(row, dest));
            }
            else if (operation == ParamUpgradeEditOperation.Delete)
            {
                // deleted means we don't add anything
                deletedRows[row.ID].RemoveAt(0);
                if (deletedRows[row.ID].Count == 0)
                {
                    deletedRows.Remove(row.ID);
                }
            }
            else if (operation == ParamUpgradeEditOperation.Modify)
            {
                // Modified means we use the modded regulation's param
                dest.AddRow(new Param.Row(modifiedRows[row.ID][0], dest));
                modifiedRows[row.ID].RemoveAt(0);
                if (modifiedRows[row.ID].Count == 0)
                {
                    modifiedRows.Remove(row.ID);
                }
            }
            else if (operation == ParamUpgradeEditOperation.NameChange)
            {
                // Inherit name
                Param.Row newRow = new(row, dest);
                newRow.Name = renamedRows[row.ID][0].Name;
                dest.AddRow(newRow);
                renamedRows[row.ID].RemoveAt(0);
                if (renamedRows[row.ID].Count == 0)
                {
                    renamedRows.Remove(row.ID);
                }
            }
        }

        // Take care of any more pending adds
        for (; currPendingAdd < pendingAdds.Length; currPendingAdd++)
        {
            // If the pending add doesn't exist in the added rows list, it was a conflicting row
            if (!addedRows.ContainsKey(pendingAdds[currPendingAdd]))
            {
                continue;
            }

            foreach (Param.Row arow in addedRows[pendingAdds[currPendingAdd]])
            {
                dest.AddRow(new Param.Row(arow, dest));
            }

            addedRows.Remove(pendingAdds[currPendingAdd]);
            editOperations.Remove(pendingAdds[currPendingAdd]);
        }

        return dest;
    }
}
