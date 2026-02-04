using Andre.Formats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public static class RowNameHelper
{
    #region Row Name Import
    public static async void ImportRowNames(ProjectEntry project, ParamBank bank, ParamRowNameImportType sourceType, string filepath = "")
    {
        Task<bool> importRowNameTask = ImportRowNamesTask(project, bank, sourceType, filepath, "");
        bool rowNamesImported = await importRowNameTask;

        if (rowNamesImported)
        {
            TaskLogs.AddLog($"Imported row names.");
        }
        else
        {
            TaskLogs.AddError($"Failed to import row names.");
        }
    }

    public static async void ImportRowNamesForParam(ProjectEntry project, ParamBank bank, ParamRowNameImportType sourceType, string targetParam = "", string filepath = "")
    {
        Task<bool> importRowNameTask = ImportRowNamesTask(project, bank, sourceType, filepath, targetParam);
        bool rowNamesImported = await importRowNameTask;

        if (rowNamesImported)
        {
            TaskLogs.AddLog($"Imported row names for {targetParam}");
        }
        else
        {
            TaskLogs.AddError($"Failed to import row names for {targetParam}");
        }
    }

    public static async Task<bool> ImportRowNamesTask(ProjectEntry project, ParamBank bank, ParamRowNameImportType sourceType, string filepath = "", string targetParam = "")
    {
        await Task.Yield();

        var sourceDirectory = filepath;
        var folder = @$"{StudioCore.Common.FileLocations.Assets}/PARAM/{ProjectUtils.GetGameDirectory(project)}";

        switch (sourceType)
        {
            case ParamRowNameImportType.Community:
                sourceDirectory = Path.Combine(folder, "Community Row Names");
                break;
            case ParamRowNameImportType.Developer:
                sourceDirectory = Path.Combine(folder, "Developer Row Names");
                break;
        }

        // For user-explicit imports
        if (filepath != "")
        {
            sourceDirectory = filepath;
        }

        if (!Directory.Exists(sourceDirectory))
        {
            TaskLogs.AddError($"Failed to find {sourceDirectory}");
            return false;
        }

        RowNameStore store = new RowNameStore();
        store.Params = new();

        if (targetParam != "")
        {
            var sourceFile = Path.Combine(sourceDirectory, $"{targetParam}.json");

            if (File.Exists(sourceFile))
            {
                try
                {
                    var filestring = File.ReadAllText(sourceFile);

                    RowNameParam item = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.RowNameParam);

                    if (item != null)
                    {
                        store.Params.Add(item);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"Failed to load {sourceFile} for row name import.", e);
                }
            }
        }
        else
        {
            foreach (var file in Directory.EnumerateFiles(sourceDirectory))
            {
                try
                {
                    var filestring = File.ReadAllText(file);

                    RowNameParam item = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.RowNameParam);

                    if (item != null)
                    {
                        store.Params.Add(item);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"Failed to load {file} for row name import.", e);
                }
            }
        }

        if (store == null)
            return false;

        if (store.Params == null)
            return false;

        var storeDict = store.Params.ToDictionary(e => e.Name);

        foreach (KeyValuePair<string, Param> p in bank.Params)
        {
            if (!storeDict.ContainsKey(p.Key))
                continue;

            if (targetParam != "")
            {
                if (p.Key != targetParam)
                    continue;
            }

            SetParamNames(p.Value, storeDict[p.Key]);
        }

        return true;
    }

    private static void SetParamNames(Param param, RowNameParam rowNames)
    {
        if (rowNames == null)
            return;

        var nameEntriesByID = new Dictionary<int, RowNameEntry>();
        foreach (var entry in rowNames.Entries)
        {
            if (!nameEntriesByID.ContainsKey(entry.ID))
            {
                nameEntriesByID.Add(entry.ID, entry);
            }
        }
        var idCounts = new Dictionary<int, int>();

        foreach (var row in param.Rows)
        {
            if (!idCounts.TryGetValue(row.ID, out int index))
                index = 0;

            idCounts[row.ID] = index + 1;

            if (nameEntriesByID.TryGetValue(row.ID, out var nameEntry))
            {
                if (nameEntry.Entries != null)
                {
                    if (index < nameEntry.Entries.Count)
                    {
                        if (CFG.Current.Param_RowNameImport_ReplaceEmptyNamesOnly)
                        {
                            if (row.Name == null || row.Name == "")
                            {
                                row.Name = nameEntry.Entries[index];
                            }
                        }
                        else
                        {
                            row.Name = nameEntry.Entries[index];
                        }
                    }
                }
            }
        }
    }

    private static void SetParamNamesLegacy(Param param, RowNameParamLegacy rowNames)
    {
        var rowsByID = param.Rows.ToLookup(e => e.ID);
        var rowNamesByID = rowNames.Entries.ToLookup(e => e.ID);

        foreach (var entry in rowsByID)
        {
            foreach (var (row, nameEntry) in entry.Zip(rowNamesByID[entry.Key]))
            {
                if (CFG.Current.Param_RowNameImport_ReplaceEmptyNamesOnly)
                {
                    if (row.Name == null || row.Name == "")
                    {
                        row.Name = nameEntry.Name;
                    }
                }
                else
                {
                    row.Name = nameEntry.Name;
                }
            }
        }
    }

    #endregion

    #region Row Name Export
    public static async void ExportRowNames(ProjectEntry project, ParamRowNameExportType exportType, string filepath, string paramName = "")
    {
        var exportDir = Path.Combine(filepath, "Row Name Export");

        if (!Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }

        Task<bool> exportRowNameTask = ExportRowNamesTask(project, exportDir, exportType, filepath, paramName);
        bool rowNamesExported = await exportRowNameTask;


        if (rowNamesExported)
        {
            TaskLogs.AddLog($"Exported row names to {exportDir}");
        }
        else
        {
            TaskLogs.AddError($"Failed to export row names to {exportDir}");
        }
    }

    public static async Task<bool> ExportRowNamesTask(ProjectEntry project, string exportDir, ParamRowNameExportType exportType, string filepath, string paramName = "")
    {
        await Task.Yield();

        var store = new RowNameStore();
        store.Params = new();

        foreach (KeyValuePair<string, Param> p in project.Handler.ParamData.PrimaryBank.Params)
        {
            if (paramName != "")
            {
                if (p.Key != paramName)
                    continue;
            }

            var paramEntry = new RowNameParam();
            paramEntry.Name = p.Key;
            paramEntry.Entries = new();

            var groupedRows = p.Value.Rows
                .GroupBy(r => r.ID)
                .ToDictionary(g => g.Key, g => g.Select(r => r.Name ?? "").ToList());

            paramEntry.Entries = groupedRows.Select(kvp => new RowNameEntry
            {
                ID = kvp.Key,
                Entries = kvp.Value
            }).ToList();

            // JSON
            if (exportType is ParamRowNameExportType.JSON)
            {
                var fullPath = Path.Combine(exportDir, $"{p.Key}.json");

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IncludeFields = true
                };
                var json = JsonSerializer.Serialize(paramEntry, typeof(RowNameParam), options);

                File.WriteAllText(fullPath, json);

                TaskLogs.AddLog($"Exported row names to {fullPath}");
            }

            if (exportType is ParamRowNameExportType.Text)
            {
                store.Params.Add(paramEntry);
            }
        }

        // Text
        if (exportType is ParamRowNameExportType.Text)
        {
            var textOuput = "";

            foreach (var entry in store.Params)
            {
                var fullPath = Path.Combine(exportDir, $"{entry.Name}.txt");

                if (paramName != "")
                {
                    if (entry.Name != paramName)
                        continue;
                }
                textOuput = $"{textOuput}\n##{entry.Name}";

                foreach (var row in entry.Entries)
                {
                    textOuput = $"{textOuput}\n{row.ID};";

                    foreach (var indexRow in row.Entries)
                    {
                        textOuput = $"{textOuput};{indexRow}";
                    }
                }

                File.WriteAllText(fullPath, textOuput);
            }
        }

        return true;
    }

    #endregion

    #region Row Name Strip / Restore
    public static void RowNameStrip(ProjectEntry project)
    {
        var exportDir = Path.Combine(ProjectUtils.GetLocalProjectFolder(project), "Stripped Row Names");

        if (!Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }

        var store = new RowNameStore();
        store.Params = new();

        foreach (KeyValuePair<string, Param> p in project.Handler.ParamData.PrimaryBank.Params)
        {
            var paramEntry = new RowNameParam();
            paramEntry.Name = p.Key;
            paramEntry.Entries = new();

            var groupedRows = p.Value.Rows
                .GroupBy(r => r.ID)
                .ToDictionary(g => g.Key, g => g.Select(r => r.Name ?? "").ToList());

            paramEntry.Entries = groupedRows.Select(kvp => new RowNameEntry
            {
                ID = kvp.Key,
                Entries = kvp.Value
            }).ToList();

            var fullPath = Path.Combine(exportDir, $"{p.Key}.json");

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true
            };
            var json = JsonSerializer.Serialize(paramEntry, typeof(RowNameParam), options);

            File.WriteAllText(fullPath, json);

            for (int i = 0; i < p.Value.Rows.Count; i++)
            {
                // Strip
                p.Value.Rows[i].Name = "";
            }

            //TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor:{Name}] Stripped row names and stored them in {fullPath}");
        }

    }

    public static void RowNameRestore(ProjectEntry project)
    {
        RowNameStore store = new();
        store.Params = new();

        var importDir = Path.Combine(ProjectUtils.GetLocalProjectFolder(project), "Stripped Row Names");

        // Fallback to pre 2.0.6 method if the Stripped Row Names folder doesn't exist
        if (!Directory.Exists(importDir))
        {
            var importFile = Path.Combine(ProjectUtils.GetLocalProjectFolder(project), "Stripped Row Names.json");

            if (File.Exists(importFile))
            {
                var filestring = File.ReadAllText(importFile);

                RowNameStoreLegacy legacyStore = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.RowNameStoreLegacy);

                if (legacyStore == null)
                {
                    TaskLogs.AddError($"Failed to located {importDir} for row name restore.");
                }
                else
                {
                    if (legacyStore == null)
                        return;

                    if (legacyStore.Params == null)
                        return;

                    var storeDict = legacyStore.Params.ToDictionary(e => e.Name);

                    foreach (KeyValuePair<string, Param> p in project.Handler.ParamData.PrimaryBank.Params)
                    {
                        if (!storeDict.ContainsKey(p.Key))
                            continue;

                        SetParamNamesLegacy(
                            p.Value,
                            storeDict[p.Key]
                        );
                    }

                    TaskLogs.AddLog($"Restored row names");
                }
            }
        }
        else
        {
            foreach (var file in Directory.EnumerateFiles(importDir))
            {
                try
                {
                    var filestring = File.ReadAllText(file);

                    RowNameParam item = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.RowNameParam);

                    if (item != null)
                    {
                        store.Params.Add(item);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"Failed to load {file} for row name restore.", e);
                }
            }

            if (store == null)
                return;

            if (store.Params == null)
                return;

            var storeDict = store.Params.ToDictionary(e => e.Name);

            foreach (KeyValuePair<string, Param> p in project.Handler.ParamData.PrimaryBank.Params)
            {
                if (!storeDict.ContainsKey(p.Key))
                    continue;

                SetParamNames(
                    p.Value,
                    storeDict[p.Key]
                );
            }

            TaskLogs.AddLog($"Restored row names");

            var legacyFile = Path.Combine(ProjectUtils.GetLocalProjectFolder(project), "Stripped Row Names.json");
            if (File.Exists(legacyFile))
            {
                File.Delete(legacyFile);
            }
        }
    }

    #endregion

    #region Row Name CSV Import
    public static async void ImportRowNamesForParam_CSV(ProjectEntry project, string filepath = "", string targetParam = "")
    {
        Task<bool> importRowNameTask = ImportRowNamesTask_CSV(project, filepath, targetParam);
        bool rowNamesImported = await importRowNameTask;

        if (rowNamesImported)
        {
            TaskLogs.AddLog($"Imported row names for {targetParam}");
        }
        else
        {
            TaskLogs.AddError($"Failed to import row names for {targetParam}");
        }
    }

    public static async Task<bool> ImportRowNamesTask_CSV(ProjectEntry project, string filepath = "", string targetParam = "")
    {
        await Task.Yield();

        var sourceFilepath = filepath;

        if (!File.Exists(sourceFilepath))
        {
            TaskLogs.AddError($"Failed to find {sourceFilepath}");
            return false;
        }

        try
        {
            var filestring = File.ReadAllText(sourceFilepath);

            var entries = filestring.Split("\n");
            var mapping = new Dictionary<int, string>();
            foreach (var entry in entries)
            {
                var parts = entry.Split(CFG.Current.Param_Import_Delimiter);
                if (parts.Length > 1)
                {
                    var id = parts[0];
                    var name = parts[1];

                    try
                    {
                        var realID = int.Parse(id);

                        mapping.Add(realID, name);
                    }
                    catch { }
                }
            }

            foreach (KeyValuePair<string, Param> p in project.Handler.ParamData.PrimaryBank.Params)
            {
                if (targetParam != "")
                {
                    if (p.Key != targetParam)
                        continue;
                }

                for (var i = 0; i < p.Value.Rows.Count; i++)
                {
                    foreach (var entry in mapping)
                    {
                        if (entry.Key == p.Value.Rows[i].ID)
                        {
                            p.Value.Rows[i].Name = entry.Value;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddError($"Failed to load {sourceFilepath} for row name import.", e);
        }

        return true;
    }
    #endregion

    #region Row Name Import (Legacy)
    public static async void ImportRowNamesForParam_Legacy(ProjectEntry project, string folderPath = "", string targetParam = "")
    {
        Task<bool> importRowNameTask = ImportRowNamesTask_Legacy(project, folderPath, targetParam);
        bool rowNamesImported = await importRowNameTask;

        if (rowNamesImported)
        {
            TaskLogs.AddLog($"Imported row names from legacy row name storage.");
        }
        else
        {
            TaskLogs.AddError($"Failed to import row names from legacy row name storage.");
        }
    }

    public static async Task<bool> ImportRowNamesTask_Legacy(ProjectEntry project, string folderPath = "", string targetParam = "")
    {
        await Task.Yield();

        var sourceFolderPath = folderPath;

        if (!Directory.Exists(sourceFolderPath))
        {
            TaskLogs.AddError($"Failed to find {sourceFolderPath}");
            return false;
        }

        try
        {
            foreach (var file in Directory.EnumerateFiles(sourceFolderPath))
            {
                var filename = Path.GetFileNameWithoutExtension(file);

                if (targetParam != "")
                {
                    if (targetParam != filename)
                        continue;
                }

                if (project.Handler.ParamData.PrimaryBank.Params.ContainsKey(filename))
                {
                    var param = project.Handler.ParamData.PrimaryBank.Params[filename];

                    var lines = File.ReadLines(file);
                    Dictionary<int, string> nameDict = lines
                        .Select((value, index) => new { index, value })
                        .ToDictionary(x => x.index, x => x.value);

                    for (int i = 0; i < param.Rows.Count; i++)
                    {
                        var curRow = param.Rows[i];

                        if (nameDict.ContainsKey(i))
                        {
                            var name = nameDict[i];

                            Match match = Regex.Match(name, @"^\d+\s*(.*)$");
                            if (match.Success)
                            {
                                string nonNumericPart = match.Groups[1].Value;
                                curRow.Name = nonNumericPart;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddError($"Failed to load {sourceFolderPath} for row name import.", e);
        }

        return true;
    }
    #endregion

}
