using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using static SoulsFormats.HKXPWV;
using static StudioCore.TextEditor.FMGBank;
using StudioCore.UserProject;
using StudioCore.AssetLocator;

namespace StudioCore.Editors.TextEditor;

[JsonSourceGenerationOptions(WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata, IncludeFields = true)]
[JsonSerializable(typeof(JsonFMG))]
internal partial class FmgSerializerContext : JsonSerializerContext
{
}

[Obsolete]
public class JsonFMG
{
    public FMG Fmg;
    public FmgIDType FmgID;

    [JsonConstructor]
    public JsonFMG()
    {
    }

    public JsonFMG(FmgIDType fmg_id, FMG fmg)
    {
        FmgID = fmg_id;
        Fmg = fmg;
    }
}

public static partial class FMGBank
{
    /// <summary>
    ///     Imports and exports FMGs using external formats.
    /// </summary>
    public static class FmgExporter
    {
        private const string _entrySeparator = "###";

        private static Dictionary<FmgIDType, FMG> GetFmgs(string msgBndPath)
        {
            Dictionary<FmgIDType, FMG> fmgs = new();
            IBinder fmgBinder;
            if (Project.Type is ProjectType.DES or ProjectType.DS1
                or ProjectType.DS1R)
            {
                fmgBinder = BND3.Read(msgBndPath);
            }
            else
            {
                fmgBinder = BND4.Read(msgBndPath);
            }
            foreach (var file in fmgBinder.Files)
            {
                var fmg = FMG.Read(file.Bytes);
                fmgs.Add((FmgIDType)file.ID, fmg);
            }

            fmgBinder.Dispose();

            return fmgs;
        }

        /// <summary>
        ///     Exports jsons that only contains entries that differ between game and mod directories.
        /// </summary>
        public static void ExportFmgTxt(bool moddedOnly)
        {
            if (!PlatformUtils.Instance.OpenFolderDialog("Choose Export Folder", out var path))
            {
                return;
            }

            if (moddedOnly && Project.GameModDirectory == Project.GameRootDirectory)
            {
                TaskLogs.AddLog("Error: Game directory is identical to mod directory. Cannot export modded files without vanilla FMGs to compare to.",
                    LogLevel.Warning, TaskLogs.LogPriority.High);
                return;
            }

            var itemPath = TextAssetLocator.GetItemMsgbnd(LanguageFolder).AssetPath;
            var menuPath = TextAssetLocator.GetMenuMsgbnd(LanguageFolder).AssetPath;
            var itemPath_Vanilla = itemPath.Replace(Project.GameModDirectory, Project.GameRootDirectory);
            var menuPath_Vanilla = menuPath.Replace(Project.GameModDirectory, Project.GameRootDirectory);

            Dictionary<FmgIDType, FMG> fmgs_vanilla = new();
            fmgs_vanilla.AddAll(GetFmgs(itemPath_Vanilla));
            fmgs_vanilla.AddAll(GetFmgs(menuPath_Vanilla));

            Dictionary<FmgIDType, FMG> fmgs_mod = new();
            foreach (var info in FmgInfoBank)
            {
                fmgs_mod.Add(info.FmgID, info.Fmg);
            }

            Dictionary<FmgIDType, FMG> fmgs_out;

            if (!moddedOnly)
            {
                // Export all entries
                fmgs_out = fmgs_mod;
            }
            else
            {
                // Export modded entries only
                fmgs_out = new();
                foreach (var kvp in fmgs_mod)
                {
                    var fmg_mod = kvp.Value;
                    var entries_vanilla = fmgs_vanilla[kvp.Key].Entries.ToList();
                    FMG entries_out = new(fmg_mod.Version);

                    foreach (var entry in fmg_mod.Entries)
                    {
                        FMG.Entry entry_vanilla = null;
                        for (var i = 0; i < entries_vanilla.Count; i++)
                        {
                            if (entries_vanilla[i].ID == entry.ID)
                            {
                                entry_vanilla = entries_vanilla[i];
                                entries_vanilla.RemoveAt(i);
                                break;
                            }
                        }

                        if (entry_vanilla != null && entry.Text == entry_vanilla.Text)
                        {
                            continue;
                        }

                        entries_out.Entries.Add(entry);
                    }

                    if (entries_out.Entries.Count > 0)
                    {
                        fmgs_out.Add(kvp.Key, entries_out);
                    }
                }
            }

            if (fmgs_out.Count == 0)
            {
                TaskLogs.AddLog("All FMG entries in mod folder are identical to game folder. No files have been exported.",
                    LogLevel.Information, TaskLogs.LogPriority.High);
                return;
            }

            foreach (var kvp in fmgs_out)
            {
                var fileName = kvp.Key.ToString();
                Dictionary<string, HashSet<int>> sharedText = new();
                foreach (var entry in kvp.Value.Entries)
                {
                    // Combine shared text

                    if (entry.Text == null)
                        continue;

                    entry.Text = entry.Text.TrimEnd('\n');

                    if (!sharedText.TryGetValue(entry.Text, out var ids))
                    {
                        sharedText[entry.Text] = ids = new();
                    }
                    ids.Add(entry.ID);
                }

                List<string> output = [];
                output.Add($"###ID {(int)kvp.Key}");
                foreach (var sharedKvp in sharedText)
                {
                    var text = sharedKvp.Key;
                    HashSet<int> ids = sharedKvp.Value;

                    text = text.Replace("\r", "");
                    text = text.TrimEnd('\n');

                    output.Add("");
                    string idsString = "";
                    foreach (var id in ids)
                    {
                        idsString += _entrySeparator + id.ToString();
                    }
                    output.Add(idsString);
                    output.AddRange(text.Split("\n"));
                }

                File.WriteAllLines($@"{path}\{fileName}.fmgmerge.txt", output);
            }

            TaskLogs.AddLog("Finished exporting FMG txt files",
                LogLevel.Information, TaskLogs.LogPriority.High);
        }

        [Obsolete]
        public static bool ExportJsonFMGs()
        {
            if (!PlatformUtils.Instance.OpenFolderDialog("Choose Export Folder", out var path))
            {
                return false;
            }

            var filecount = 0;
            if (Project.Type == ProjectType.DS2S)
            {
                Directory.CreateDirectory(path);

                foreach (FMGInfo info in FmgInfoBank)
                {
                    JsonFMG fmgPair = new(info.FmgID, info.Fmg);
                    var json = JsonSerializer.Serialize(fmgPair, FmgSerializerContext.Default.JsonFMG);
                    json = FormatJson(json);

                    var fileName = info.Name;
                    if (CFG.Current.FMG_ShowOriginalNames)
                    {
                        fileName = info.FileName;
                    }

                    File.WriteAllText($@"{path}\{fileName}.fmg.json", json);

                    filecount++;
                }
            }
            else
            {
                var itemPath = $@"{path}\Item Text";
                var menuPath = $@"{path}\Menu Text";
                Directory.CreateDirectory(itemPath);
                Directory.CreateDirectory(menuPath);
                foreach (FMGInfo info in FmgInfoBank)
                {
                    if (info.UICategory == FmgUICategory.Item)
                    {
                        path = itemPath;
                    }
                    else if (info.UICategory == FmgUICategory.Menu)
                    {
                        path = menuPath;
                    }

                    JsonFMG fmgPair = new(info.FmgID, info.Fmg);
                    var json = JsonSerializer.Serialize(fmgPair, FmgSerializerContext.Default.JsonFMG);
                    json = FormatJson(json);

                    var fileName = info.Name;
                    if (CFG.Current.FMG_ShowOriginalNames)
                    {
                        fileName = info.FileName;
                    }

                    File.WriteAllText($@"{path}\{fileName}.fmg.json", json);

                    filecount++;
                }
            }

            TaskLogs.AddLog($"Finished exporting {filecount} text files",
                LogLevel.Information, TaskLogs.LogPriority.High);
            return true;
        }

        private static bool ImportFmg(FmgIDType fmgId, FMG fmg, bool merge)
        {
            foreach (FMGInfo info in FmgInfoBank)
            {
                if (info.FmgID == fmgId)
                {
                    if (merge)
                    {
                        // Merge mode. Add and replace FMG entries instead of overwriting FMG entirely
                        foreach (var entry in fmg.Entries.ToList())
                        {
                            var currentEntry = info.Fmg.Entries.Find(e => e.ID == entry.ID);
                            if (currentEntry == null)
                            {
                                fmg.Entries.Add(entry);
                            }
                            else if (currentEntry.Text != entry.Text)
                            {
                                currentEntry.Text = entry.Text;
                            }
                        }
                    }
                    else
                    {
                        // Overwrite mode. Replace FMG with imported json
                        info.Fmg = fmg;
                    }

                    return true;
                }
            }

            TaskLogs.AddLog($"FMG import error: No loaded FMGs have an ID of \"{fmgId}\"",
                LogLevel.Error, TaskLogs.LogPriority.Normal);

            return false;
        }

        public static bool ImportFmgJson(bool merge)
        {
            if (!PlatformUtils.Instance.OpenMultiFileDialog("Choose Files to Import",
                    new[] { FilterStrings.FmgJsonFilter }, out IReadOnlyList<string> files))
            {
                return false;
            }

            if (files.Count == 0)
            {
                return false;
            }

            var filecount = 0;
            foreach (var filePath in files)
            {
                try
                {
                    var file = File.ReadAllText(filePath);
                    JsonFMG json = JsonSerializer.Deserialize(file, FmgSerializerContext.Default.JsonFMG);
                    bool success = ImportFmg(json.FmgID, json.Fmg, merge);
                    if (success)
                    {
                        filecount++;
                    }
                }
                catch (JsonException e)
                {
                    TaskLogs.AddLog($"FMG import error: Couldn't import \"{filePath}\"",
                        LogLevel.Error, TaskLogs.LogPriority.Normal, e);
                }
            }

            if (filecount == 0)
            {
                return false;
            }

            HandleDuplicateEntries();
            PlatformUtils.Instance.MessageBox($"Imported {filecount} json files", "Finished", MessageBoxButtons.OK);
            return true;
        }

        public static bool ImportFmgTxt(bool merge)
        {
            if (!PlatformUtils.Instance.OpenMultiFileDialog("Choose Files to Import",
                    new[] { FilterStrings.TxtFilter }, out IReadOnlyList<string> files))
            {
                return false;
            }

            if (files.Count == 0)
            {
                return false;
            }

            var filecount = 0;
            foreach (var filePath in files)
            {
                try
                {
                    string fileName = Path.GetFileName(filePath);
                    FMG fmg = new();
                    int fmgId = 0;
                    var file = File.ReadAllLines(filePath);
                    try
                    {
                        fmgId = int.Parse(file[0].Replace(_entrySeparator, "").Replace("ID", ""));
                    }
                    catch
                    {
                        TaskLogs.AddLog($"FMG import error for file {fileName}: Cannot parse FMG ID on line 1.",
                            LogLevel.Error, TaskLogs.LogPriority.Normal);
                        return false;
                    }

                    Queue<int> entryIds = new();
                    List<string> text = new();
                    for (var i = 1; i < file.Length; i++)
                    {
                        var line = file[i];

                        if (i + 1 == file.Length)
                        {
                            text.Add(line);
                            string str = string.Join("\r\n", text);
                            while (entryIds.Count > 0)
                            {
                                fmg.Entries.Add(new(entryIds.Dequeue(), str));
                            }
                        }
                        else if (line.StartsWith(_entrySeparator) || i + 1 == file.Length)
                        {
                            if (text.Count > 0)
                            {
                                string str = string.Join("\r\n", text);

                                while (entryIds.Count > 0)
                                {
                                    fmg.Entries.Add(new(entryIds.Dequeue(), str));
                                }

                                if (i + 1 == file.Length)
                                    break;

                                try
                                {
                                    var ids = line.Split(_entrySeparator);
                                    foreach (var id in ids)
                                    {
                                        if (string.IsNullOrEmpty(id))
                                            continue;

                                        entryIds.Enqueue(int.Parse(id));
                                    }
                                }
                                catch
                                {
                                    TaskLogs.AddLog($"FMG import error for file {fileName}: Cannot parse entry ID on line {i + 1}.",
                                        LogLevel.Error, TaskLogs.LogPriority.High);
                                    return false;
                                }
                                text = new();
                            }
                        }
                        else
                        {
                            text.Add(line);
                        }
                    }

                    bool success = ImportFmg((FmgIDType)fmgId, fmg, merge);
                    if (success)
                    {
                        filecount++;
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"FMG import error: Couldn't import \"{filePath}\"",
                        LogLevel.Error, TaskLogs.LogPriority.Normal, e);
                }
            }

            if (filecount == 0)
            {
                return false;
            }

            HandleDuplicateEntries();

            TaskLogs.AddLog($"FMG import: Finished importing {filecount} txt files",
                LogLevel.Information, TaskLogs.LogPriority.Normal);
            return true;
        }
    }
}
