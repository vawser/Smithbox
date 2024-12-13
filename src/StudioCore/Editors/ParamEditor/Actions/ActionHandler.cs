using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Tasks;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.BHD5;

namespace StudioCore.Editors.ParamEditor.Actions;
public enum TargetType
{
    [Display(Name = "Selected Rows")] SelectedRows,
    [Display(Name = "Selected Param")] SelectedParam,
    [Display(Name = "All Params")] AllParams
}

public enum SourceType
{
    [Display(Name = "Smithbox")] Smithbox,
    [Display(Name = "Project")] Project,
    [Display(Name = "Developer")] Developer
}

public class ActionHandler
{
    private ParamEditorScreen Screen;

    public TargetType CurrentTargetCategory = TargetType.SelectedParam;
    public SourceType CurrentSourceCategory = SourceType.Smithbox;

    public bool _rowNameImporter_VanillaOnly = false;
    public bool _rowNameImporter_EmptyOnly = false;

    public ActionHandler(ParamEditorScreen screen)
    {
        Screen = screen;
    }

    public void DuplicateHandler()
    {
        Param param = ParamBank.PrimaryBank.Params[Screen._activeView._selection.GetActiveParam()];
        List<Param.Row> rows = Screen._activeView._selection.GetSelectedRows();

        if (rows.Count == 0)
        {
            return;
        }

        List<Param.Row> rowsToInsert = new();

        foreach (Param.Row r in rows)
        {
            Param.Row newrow = new(r);
            rowsToInsert.Add(newrow);
        }

        List<EditorAction> actions = new List<EditorAction>();

        for (int i = 0; i < CFG.Current.Param_Toolbar_Duplicate_Amount; i++)
        {
            actions.Add(new AddParamsAction(param, "legacystring", rowsToInsert, false, false, -1, CFG.Current.Param_Toolbar_Duplicate_Offset));
        }

        var compoundAction = new CompoundAction(actions);

        Screen.EditorActionManager.ExecuteAction(compoundAction);
    }

    public void ExportRowNameHandler()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                ExportRowNames();
            }
        }
    }

    private void ExportRowNames()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;
        var activeParam = selectedParam.GetActiveParam();

        switch (CurrentTargetCategory)
        {
            case TargetType.SelectedRows:
                ExportRowNamesForRows(selectedParam.GetSelectedRows());
                PlatformUtils.Instance.MessageBox($"Row names for {activeParam} selected rows have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            case TargetType.SelectedParam:
                ExportRowNamesForParam(activeParam);
                PlatformUtils.Instance.MessageBox($"Row names for {activeParam} have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            case TargetType.AllParams:
                foreach (var param in ParamBank.PrimaryBank.Params)
                {
                    ExportRowNamesForParam(param.Key);
                }
                PlatformUtils.Instance.MessageBox($"Row names for all params have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ExportRowNamesForRows(IEnumerable<Param.Row> rows)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var dialog = NativeFileDialogSharp.Dialog.FileSave("txt");
        if (!dialog.IsOk) return;

        var path = dialog.Path;

        List<string> contents = IterateRows(rows);

        var dir = Path.GetDirectoryName(path)!;
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        File.WriteAllLines(path, contents);
    }

    private void ExportRowNamesForParam(string param)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Names";
        var path = Path.Combine(dir, $"{param}.txt");

        Param p = ParamBank.PrimaryBank.Params[param];

        List<string> contents = IterateRows(p.Rows);

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllLines(path, contents);
    }
    private static List<string> IterateRows(IEnumerable<Param.Row> rows)
    {
        return rows.Select(r => $"{r.ID} {r.Name}").ToList();
    }

    public void ImportRowNameHandler()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            bool _rowNameImport_useProjectNames = CurrentSourceCategory == SourceType.Project;
            bool _rowNameImport_useDeveloperNames = CurrentSourceCategory == SourceType.Developer;

            if (ParamBank.PrimaryBank.Params != null)
            {
                switch (CurrentTargetCategory)
                {
                    case TargetType.SelectedRows:
                        var rows = selectedParam.GetSelectedRows();
                        Screen.EditorActionManager.ExecuteAction(
                            ParamBank.PrimaryBank.LoadParamDefaultNames(
                                selectedParam.GetActiveParam(),
                                _rowNameImporter_EmptyOnly,
                                _rowNameImporter_VanillaOnly,
                                _rowNameImport_useProjectNames,
                                _rowNameImport_useDeveloperNames,
                                rows)
                        );
                        break;
                    case TargetType.SelectedParam:
                        Screen.EditorActionManager.ExecuteAction(
                            ParamBank.PrimaryBank.LoadParamDefaultNames(
                                selectedParam.GetActiveParam(),
                                _rowNameImporter_EmptyOnly,
                                _rowNameImporter_VanillaOnly,
                                _rowNameImport_useProjectNames,
                                _rowNameImport_useDeveloperNames)
                        );
                        break;
                    case TargetType.AllParams:
                        Screen.EditorActionManager.ExecuteAction(
                            ParamBank.PrimaryBank.LoadParamDefaultNames(
                                null,
                                _rowNameImporter_EmptyOnly,
                                _rowNameImporter_VanillaOnly,
                                _rowNameImport_useProjectNames,
                                _rowNameImport_useDeveloperNames)
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public void RowNameTrimHandler()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                var activeParam = selectedParam.GetActiveParam();
                var rows = selectedParam.GetSelectedRows();
                switch (CurrentTargetCategory)
                {
                    case TargetType.SelectedRows:
                        if (!rows.Any()) return;
                        TrimRowNames(rows);
                        PlatformUtils.Instance.MessageBox($"Row names for {rows.Count} selected rows have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case TargetType.SelectedParam:
                        TrimRowNames(activeParam);
                        PlatformUtils.Instance.MessageBox($"Row names for {activeParam} have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case TargetType.AllParams:
                        foreach (var param in ParamBank.PrimaryBank.Params)
                        {
                            TrimRowNames(param.Key);
                        }
                        PlatformUtils.Instance.MessageBox($"Row names for all params have been trimmed.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    private void TrimRowNames(IEnumerable<Param.Row> rows)
    {
        foreach (Param.Row row in rows)
        {
            row.Name = row.Name.Trim();
        }
    }
    private void TrimRowNames(string param)
    {
        Param p = ParamBank.PrimaryBank.Params[param];
        TrimRowNames(p.Rows);
    }

    public void ParamTargetElement(ref TargetType currentTarget, string tooltip, Vector2 size)
    {
        UIHelper.WrappedText("Target Category:");
        ImGui.SetNextItemWidth(size.X);
        if (ImGui.BeginCombo("##Target", currentTarget.GetDisplayName()))
        {
            foreach (TargetType e in Enum.GetValues<TargetType>())
            {
                var name = e.GetDisplayName();
                if (ImGui.Selectable(name))
                {
                    currentTarget = e;
                    break;
                }
            }
            ImGui.EndCombo();
        }
        UIHelper.ShowHoverTooltip(tooltip);
        UIHelper.WrappedText("");
    }

    public void ParamSourceElement(ref SourceType currentSource, string tooltip, Vector2 size)
    {
        UIHelper.WrappedText("Source Category:");
        ImGui.SetNextItemWidth(size.X);
        if (ImGui.BeginCombo("##Source", currentSource.GetDisplayName()))
        {
            foreach (SourceType e in Enum.GetValues<SourceType>())
            {
                var name = e.GetDisplayName();
                if (ImGui.Selectable(name))
                {
                    currentSource = e;
                    break;
                }
            }
            ImGui.EndCombo();
        }
    }

    public void SortRowsHandler()
    {
        if (Screen._activeView._selection.ActiveParamExists())
        {
            TaskLogs.AddLog($"Param rows sorted for {Screen._activeView._selection.GetActiveParam()}");
            Screen.EditorActionManager.ExecuteAction(MassParamEditOther.SortRows(ParamBank.PrimaryBank, Screen._activeView._selection.GetActiveParam()));
        }
    }

    // Merge Params
    public string targetRegulationPath = "";
    public string targetLooseParamPath = "";
    public string targetEnemyParamPath = "";

    public bool targetUniqueOnly = true;

    public string[] allParamTypes =
    {
        FilterStrings.RegulationBinFilter, FilterStrings.Data0Filter, FilterStrings.ParamBndDcxFilter,
        FilterStrings.ParamBndFilter, FilterStrings.EncRegulationFilter
    };

    public void MergeParamHandler()
    {
        if(targetRegulationPath == "")
            {
            PlatformUtils.Instance.MessageBox("Target Regulation path is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        if (!targetRegulationPath.Contains("regulation.bin"))
        {
            PlatformUtils.Instance.MessageBox("Target Regulation path is does not point to a regulation.bin file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ParamBank.LoadAuxBank(targetRegulationPath, null, null);
        if (Smithbox.ProjectType is ProjectType.DS2S or ProjectType.DS2)
        {
            ParamBank.LoadAuxBank(targetRegulationPath, targetLooseParamPath, targetEnemyParamPath);
        }

        var auxBank = ParamBank.AuxBanks.First();

        // Apply the merge massedit script here
        var command = $"auxparam {auxBank.Key} .*: modified && unique ID: paste;";

        if(!targetUniqueOnly)
        {
            command = $"auxparam {auxBank.Key} .*: modified ID: paste;";
        }

        //TaskLogs.AddLog(command);
        ExecuteMassEdit(command);
    }

    public void ExecuteMassEdit(string command)
    {
        Smithbox.EditorHandler.ParamEditor._activeView._selection.SortSelection();
        (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
            command, Smithbox.EditorHandler.ParamEditor._activeView._selection);

        if (child != null)
        {
            Screen.EditorActionManager.PushSubManager(child);
        }

        if (r.Type == MassEditResultType.SUCCESS)
        {
            ParamBank.RefreshParamDifferenceCacheTask();
        }
    }

    // Find Field Instances
    public string _idFieldInstanceFinder_SearchString = "";
    public string _idFieldInstanceFinder_CachedSearchString = "";
    public bool _idFieldInstanceFinder_matchWiki = true;
    public bool _idFieldInstanceFinder_displayCommunityName = false;
    public List<FieldInstanceResult> _idFieldInstanceFinder_Results = new();

    public void FieldInstanceHandler()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                _idFieldInstanceFinder_CachedSearchString = _idFieldInstanceFinder_SearchString;
                _idFieldInstanceFinder_Results = GetFieldInstanceResults(_idFieldInstanceFinder_SearchString);

                _idFieldInstanceFinder_Results.Sort();
            }
        }
    }

    public List<FieldInstanceResult> GetFieldInstanceResults(string searchStr)
    {
        List<FieldInstanceResult> output = new();

        var searchComponents = searchStr.ToLower().Split(" ");

        foreach (var p in ParamBank.PrimaryBank.Params)
        {
            var def = p.Value.AppliedParamdef;
            var meta = ParamMetaData.Get(def);

            foreach (var field in def.Fields)
            {
                bool addResult = false;
                var fieldMeta = FieldMetaData.Get(field);

                foreach (var entry in searchComponents)
                {
                    if (field.InternalName != null)
                    {
                        // Internal Name
                        if (field.InternalName.ToLower().Contains(entry))
                        {
                            addResult = true;
                        }
                    }

                    // Display Name
                    if (fieldMeta.AltName != null)
                    {
                        var displayNameComponents = fieldMeta.AltName.Split(" ");

                        foreach (var displayComponent in displayNameComponents)
                        {
                            if (displayComponent.ToLower().Contains(entry))
                            {
                                addResult = true;
                            }
                        }
                    }

                    // Wiki
                    if (fieldMeta.Wiki != null && _idFieldInstanceFinder_matchWiki)
                    {
                        var descriptionComponents = fieldMeta.Wiki.Split(" ");

                        foreach (var descriptionComponent in descriptionComponents)
                        {
                            if (descriptionComponent.ToLower().Contains(entry))
                            {
                                addResult = true;
                            }
                        }
                    }
                }

                // If anything matches, add the current field (and param) as a result.
                if (addResult)
                {
                    var newResultEntry = new FieldInstanceResult(field.InternalName, fieldMeta.AltName, p.Key);
                    output.Add(newResultEntry);
                }
            }
        }

        return output;
    }

    public void DisplayFieldInstances()
    {
        if (_idFieldInstanceFinder_Results.Count > 0)
        {
            var Size = ImGui.GetWindowSize();
            float EditX = (Size.X / 100) * 95;
            float EditY = (Size.Y / 100) * 25;

            ImGui.BeginChild("##fieldInstances_resultSection", new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));
            UIHelper.WrappedText($"ID {_idFieldInstanceFinder_CachedSearchString}: {_idFieldInstanceFinder_Results.Count} matches");

            foreach (var result in _idFieldInstanceFinder_Results)
            {
                var name = result.FieldInternalName;

                if(_idFieldInstanceFinder_displayCommunityName)
                {
                    name = result.FieldDisplayName;
                }

                if (ImGui.Selectable($"{result.ParamName}: {name}##FieldInstanceRowSearcher"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}");
                }
            }

            ImGui.EndChild();
        }
    }

    public class FieldInstanceResult : IComparable<FieldInstanceResult>
    {
        public string FieldInternalName;
        public string FieldDisplayName;
        public string ParamName;

        public FieldInstanceResult(string fieldInternalName, string fieldDisplayName, string paramName)
        {
            FieldInternalName = fieldInternalName;
            FieldDisplayName = fieldDisplayName;
            ParamName = paramName;
        }

        public int CompareTo(FieldInstanceResult other)
        {
            return this.ParamName.CompareTo(other.ParamName);
        }
    }

    // Find Row ID Instances
    public int _idRowInstanceFinder_SearchID = 0;
    public int _idRowInstanceFinder_CachedSearchID = 0;
    public int _idRowInstanceFinder_SearchIndex = -1;
    public List<string> _idRowInstanceFinder_Results = new();

    public void RowIDInstanceHandler()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                _idRowInstanceFinder_CachedSearchID = _idRowInstanceFinder_SearchID;
                _idRowInstanceFinder_Results = GetParamsWithRowID(_idRowInstanceFinder_SearchID, _idRowInstanceFinder_SearchIndex);

                if (_idRowInstanceFinder_Results.Count > 0)
                {
                    var message = $"Found row ID {_idRowInstanceFinder_SearchID} in the following params:\n";
                    foreach (var line in _idRowInstanceFinder_Results)
                        message += $"  {line}\n";
                    TaskLogs.AddLog(message,
                        LogLevel.Information, LogPriority.Low);
                }
                else
                {
                    TaskLogs.AddLog($"No params found with row ID {_idRowInstanceFinder_SearchID}",
                        LogLevel.Information, LogPriority.High);
                }
            }
        }
    }

    public List<string> GetParamsWithRowID(int id, int index)
    {
        List<string> output = new();
        foreach (var p in ParamBank.PrimaryBank.Params)
        {
            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                var r = p.Value.Rows[i];
                if (r.ID == id && (index == -1 || index == i))
                {
                    output.Add(p.Key);
                    break;
                }
            }
        }

        return output;
    }

    public void DisplayRowIDInstances()
    {
        if (_idRowInstanceFinder_Results.Count > 0)
        {
            var Size = ImGui.GetWindowSize();
            float EditX = (Size.X / 100) * 95;
            float EditY = (Size.Y / 100) * 25;

            ImGui.BeginChild("##resultSection", new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));
            UIHelper.WrappedText($"ID {_idRowInstanceFinder_CachedSearchID}: {_idRowInstanceFinder_Results.Count} matches");

            foreach (var paramName in _idRowInstanceFinder_Results)
            {
                if (ImGui.Selectable($"{paramName}##RowSearcher"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{paramName}/{_idRowInstanceFinder_CachedSearchID}");
                }
            }

            ImGui.EndChild();
        }
    }

    // Find Row Name Instances
    public string _nameRowInstanceFinder_SearchName = "";
    public string _nameRowInstanceFinder_CachedSearchName = "";
    public int _nameRowInstanceFinder_SearchIndex = -1;
    public List<RowNameResult> _nameRowInstanceFinder_Results = new();

    public void RowNameInstanceHandler()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                _nameRowInstanceFinder_CachedSearchName = _nameRowInstanceFinder_SearchName;
                _nameRowInstanceFinder_Results = GetParamsWithRowName(_nameRowInstanceFinder_SearchName, _nameRowInstanceFinder_SearchIndex);
            }
        }
    }

    public List<RowNameResult> GetParamsWithRowName(string searchString, int index)
    {
        List<RowNameResult> output = new();

        var searchComponents = searchString.Split(" ");

        foreach (var p in ParamBank.PrimaryBank.Params)
        {
            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                var r = p.Value.Rows[i];

                bool addResult = false;

                var rowName = "";

                foreach (var entry in searchComponents)
                {
                    if (r.Name != "" || r.Name != null)
                    {
                        var rowNameComponents = r.Name.Split(" ");

                        rowName = r.Name;

                        foreach (var rowNameEntry in rowNameComponents)
                        {
                            if (rowNameEntry.Contains(entry) && (index == -1 || index == i))
                            {
                                addResult = true;
                            }
                        }
                    }
                }

                if(addResult)
                {
                    var newResult = new RowNameResult(rowName, r.ID, i, p.Key);
                    output.Add(newResult);
                }
            }
        }

        return output;
    }

    public void DisplayRowNameInstances()
    {
        if (_nameRowInstanceFinder_Results.Count > 0)
        {
            var Size = ImGui.GetWindowSize();
            float EditX = (Size.X / 100) * 95;
            float EditY = (Size.Y / 100) * 25;

            ImGui.BeginChild("##rowNameResultSection", new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));
            UIHelper.WrappedText($"ID {_nameRowInstanceFinder_CachedSearchName}: {_nameRowInstanceFinder_Results.Count} matches");

            foreach (var result in _nameRowInstanceFinder_Results)
            {
                if (ImGui.Selectable($"{result.ParamName}: {result.RowName}##RowNameSearcher"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}/{result.RowID}");
                }
            }

            ImGui.EndChild();
        }
    }

    public class RowNameResult : IComparable<RowNameResult>
    {
        public string RowName;
        public int RowID;
        public int RowIndex;
        public string ParamName;

        public RowNameResult(string rowName, int rowID, int rowIndex, string paramName)
        {
            RowName = rowName;
            RowID = rowID;
            RowIndex = rowIndex;
            ParamName = paramName;
        }

        public int CompareTo(RowNameResult other)
        {
            return this.ParamName.CompareTo(other.ParamName);
        }
    }

    // Row Value Instances
    public string _searchValue = "";
    public string _cachedSearchValue = "";

    public List<ParamValueResult> ParamResults = new();
    public List<AliasValueResult> AliasResults = new();

    public void RowValueInstanceHandler()
    {
        ParamResults = new();
        AliasResults = new();

        SearchParamValue();
        SearchAliasValue();
    }

    public void DisplayRowValueInstances()
    {
        var Size = ImGui.GetWindowSize();

        float mult = 2;
        if (AliasResults.Count > 0)
        {
            mult = 1;
        }
        float EditX = (Size.X / 100) * 95;
        float EditY = (Size.Y / 100) * (25 * mult);

        if (ParamResults.Count > 0)
        {
            UIHelper.WrappedText("Params:");
            ImGui.BeginChild("##paramResultSection", new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));

            // Param Results
            UIHelper.WrappedText($"Value {_cachedSearchValue}: {ParamResults.Count} matches");

            foreach (var result in ParamResults)
            {
                if (ImGui.Selectable($"{result.Param}: {result.Row}: {result.Field}##ValueSearcher"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.Param}/{result.Row}/{result.Field}");
                }
            }
            ImGui.EndChild();
        }

        // Alias Results
        if (AliasResults.Count > 0)
        {
            UIHelper.WrappedText("");
            UIHelper.WrappedText("Aliases:");
            ImGui.BeginChild("##aliasResultSection", new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));

            UIHelper.WrappedText($"Value {_cachedSearchValue}: {AliasResults.Count} matches");

            foreach (var result in AliasResults)
            {
                if (ImGui.Selectable($"{result.Alias}: {result.ID}: {result.Name}##AliasValueSearcher"))
                {

                }
            }
            ImGui.EndChild();
        }

        UIHelper.WrappedText("");
    }


    public void SearchAliasValue()
    {
        _cachedSearchValue = _searchValue;

        // Cutscene
        if (Smithbox.BankHandler.CutsceneAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.CutsceneAliases.Aliases.list)
            {
                AddAliasResult(entry, _searchValue, "Cutscene");
            }
        }

        // Flag
        if (Smithbox.BankHandler.EventFlagAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.EventFlagAliases.Aliases.list)
            {
                AddAliasResult(entry, _searchValue, "Event Flag");
            }
        }

        // Characters
        if (Smithbox.BankHandler.CharacterAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.CharacterAliases.Aliases.list)
            {
                AddAliasResult(entry, _searchValue, "Character");
            }
        }

        // Assets
        if (Smithbox.BankHandler.AssetAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.AssetAliases.Aliases.list)
            {
                AddAliasResult(entry, _searchValue, "Object");
            }
        }

        // Parts
        if (Smithbox.BankHandler.AssetAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.PartAliases.Aliases.list)
            {
                AddAliasResult(entry, _searchValue, "Part");
            }
        }

        // MapPieces
        if (Smithbox.BankHandler.AssetAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.MapAliases.Aliases.list)
            {
                AddAliasResult(entry, _searchValue, "Map Piece");
            }
        }

        // Movies
        if (Smithbox.BankHandler.MovieAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.MovieAliases.Aliases.list)
            {
                AddAliasResult(entry, _searchValue, "Movie");
            }
        }

        // Particles
        if (Smithbox.BankHandler.ParticleAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.ParticleAliases.Aliases.list)
            {
                AddAliasResult(entry, _searchValue, "Particle");
            }
        }

        // Sounds
        if (Smithbox.BankHandler.SoundAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.SoundAliases.Aliases.list)
            {
                AddAliasResult(entry, _searchValue, "Sound");
            }
        }
    }

    public void AddAliasResult(AliasReference entry, string value, string aliasName)
    {
        if (entry.id == value)
        {
            AliasValueResult valueResult = new AliasValueResult();
            valueResult.Alias = aliasName;
            valueResult.ID = entry.id;
            valueResult.Name = entry.name;
            AliasResults.Add(valueResult);
        }
    }

    public void SearchParamValue()
    {
        var selectedParam = Smithbox.EditorHandler.ParamEditor._activeView._selection;

        if (selectedParam.ActiveParamExists())
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                _cachedSearchValue = _searchValue;
                GetParamsWithValue(_searchValue);

                if (ParamResults.Count > 0)
                {
                    var message = $"Found value {_searchValue} in the following params:\n";
                    foreach (var result in ParamResults)
                    {
                        message += $"  {result.Param}\n";
                        TaskLogs.AddLog(message,
                            LogLevel.Information, LogPriority.Low);
                    }
                }
                else
                {
                    TaskLogs.AddLog($"No params found with value {_searchValue}",
                        LogLevel.Information, LogPriority.High);
                }
            }
        }
    }

    public void GetParamsWithValue(string value)
    {
        foreach (var p in ParamBank.PrimaryBank.Params)
        {
            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                var success = false;
                var isMatch = false;
                var r = p.Value.Rows[i];
                var id = r.ID;
                string fieldName = "";

                foreach (var field in r.Cells)
                {
                    PARAMDEF.DefType type = field.Def.DisplayType;

                    switch (type)
                    {
                        case PARAMDEF.DefType.s8:
                            sbyte sbyteVal;
                            success = sbyte.TryParse(value, out sbyteVal);
                            if (success)
                            {
                                if (sbyteVal == (sbyte)field.Value)
                                {
                                    fieldName = field.Def.InternalName;
                                    isMatch = true;
                                }
                            }
                            break;
                        case PARAMDEF.DefType.u8:
                            byte byteVal;
                            success = byte.TryParse(value, out byteVal);
                            if (success)
                            {
                                if (byteVal == (byte)field.Value)
                                {
                                    fieldName = field.Def.InternalName;
                                    isMatch = true;
                                }
                            }
                            break;
                        case PARAMDEF.DefType.s16:
                            short shortVal;
                            success = short.TryParse(value, out shortVal);
                            if (success)
                            {
                                if (shortVal == (short)field.Value)
                                {
                                    fieldName = field.Def.InternalName;
                                    isMatch = true;
                                }
                            }
                            break;
                        case PARAMDEF.DefType.u16:
                            ushort ushortVal;
                            success = ushort.TryParse(value, out ushortVal);
                            if (success)
                            {
                                if (ushortVal == (ushort)field.Value)
                                {
                                    fieldName = field.Def.InternalName;
                                    isMatch = true;
                                }
                            }
                            break;
                        case PARAMDEF.DefType.s32:
                            int intVal;
                            success = int.TryParse(value, out intVal);
                            if (success)
                            {
                                if (intVal == (int)field.Value)
                                {
                                    fieldName = field.Def.InternalName;
                                    isMatch = true;
                                }
                            }
                            break;
                        case PARAMDEF.DefType.u32:
                            uint uintVal;
                            success = uint.TryParse(value, out uintVal);
                            if (success)
                            {
                                if (uintVal == (uint)field.Value)
                                {
                                    fieldName = field.Def.InternalName;
                                    isMatch = true;
                                }
                            }
                            break;
                        case PARAMDEF.DefType.f32:
                            float floatVal;
                            success = float.TryParse(value, out floatVal);
                            if (success)
                            {
                                if (floatVal == (float)field.Value)
                                {
                                    fieldName = field.Def.InternalName;
                                    isMatch = true;
                                }
                            }
                            break;
                        case PARAMDEF.DefType.b32:
                            bool boolVal;
                            success = bool.TryParse(value, out boolVal);
                            if (success)
                            {
                                if (boolVal == (bool)field.Value)
                                {
                                    fieldName = field.Def.InternalName;
                                    isMatch = true;
                                }
                            }
                            break;
                        case PARAMDEF.DefType.fixstr:
                        case PARAMDEF.DefType.fixstrW:
                            string strVal = value;
                            if (strVal == (string)field.Value)
                            {
                                fieldName = field.Def.InternalName;
                                isMatch = true;
                            }
                            break;
                        default: break;
                    }
                }

                if (isMatch)
                {
                    ParamValueResult paramValueResult = new ParamValueResult();
                    paramValueResult.Row = id.ToString();
                    paramValueResult.Param = p.Key;
                    paramValueResult.Field = fieldName;
                    ParamResults.Add(paramValueResult);

                    // Skip matching more if this is enabled
                    if (CFG.Current.Param_Toolbar_FindValueInstances_InitialMatchOnly)
                    {
                        break;
                    }
                }
            }
        }
    }

    public class ParamValueResult
    {
        public string Param;
        public string Row;
        public string Field;

        public ParamValueResult() { }
    }

    public class AliasValueResult
    {
        public string Alias;
        public string ID;
        public string Name;

        public AliasValueResult() { }
    }
}
