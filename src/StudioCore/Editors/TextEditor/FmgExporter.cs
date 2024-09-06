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
using StudioCore.Locators;
using StudioCore.Banks.AliasBank;
using StudioCore.Core.Project;
using StudioCore.Utilities;

namespace StudioCore.Editors.TextEditor;

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
        if (Smithbox.ProjectType is ProjectType.DES or ProjectType.DS1
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
    public static void ExportFmgTxt(FMGLanguage lang, bool moddedOnly)
    {
        if (!PlatformUtils.Instance.OpenFolderDialog("Choose Export Folder", out var path))
        {
            return;
        }

        if (moddedOnly && Smithbox.ProjectRoot == Smithbox.GameRoot)
        {
            TaskLogs.AddLog("Error: Game directory is identical to mod directory. Cannot export modded files without vanilla FMGs to compare to.",
                LogLevel.Warning, TaskLogs.LogPriority.High);
            return;
        }

        var itemMsgPath = TextLocator.GetItemMsgbnd(lang.LanguageFolder, false, "");
        var menuMsgPath = TextLocator.GetMenuMsgbnd(lang.LanguageFolder, false, "");

        if (Smithbox.ProjectType is ProjectType.ER)
        {
            switch (Smithbox.EditorHandler.TextEditor.CurrentTargetOutputMode)
            {
                case TextEditorScreen.TargetOutputMode.Vanilla:
                    break;
                case TextEditorScreen.TargetOutputMode.DLC1:
                    itemMsgPath = TextLocator.GetItemMsgbnd(lang.LanguageFolder, false, "_dlc01");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(lang.LanguageFolder, false, "_dlc01");
                    break;
                case TextEditorScreen.TargetOutputMode.DLC2:
                    itemMsgPath = TextLocator.GetItemMsgbnd(lang.LanguageFolder, false, "_dlc02");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(lang.LanguageFolder, false, "_dlc02");
                    break;
            }
        }
        if (Smithbox.ProjectType is ProjectType.DS3)
        {
            switch (Smithbox.EditorHandler.TextEditor.CurrentTargetOutputMode)
            {
                case TextEditorScreen.TargetOutputMode.Vanilla:
                    break;
                case TextEditorScreen.TargetOutputMode.DLC1:
                    itemMsgPath = TextLocator.GetItemMsgbnd(lang.LanguageFolder, false, "_dlc1");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(lang.LanguageFolder, false, "_dlc1");
                    break;
                case TextEditorScreen.TargetOutputMode.DLC2:
                    itemMsgPath = TextLocator.GetItemMsgbnd(lang.LanguageFolder, false, "_dlc2");
                    menuMsgPath = TextLocator.GetMenuMsgbnd(lang.LanguageFolder, false, "_dlc2");
                    break;
            }
        }

        var itemPath = itemMsgPath.AssetPath;
        var menuPath = menuMsgPath.AssetPath;

        var itemPath_Vanilla = itemPath.Replace(Smithbox.ProjectRoot, Smithbox.GameRoot);
        var menuPath_Vanilla = menuPath.Replace(Smithbox.ProjectRoot, Smithbox.GameRoot);

        Dictionary<FmgIDType, FMG> fmgs_vanilla = new();
        fmgs_vanilla.AddAll(GetFmgs(itemPath_Vanilla));
        fmgs_vanilla.AddAll(GetFmgs(menuPath_Vanilla));

        Dictionary<FmgIDType, FMG> fmgs_mod = new();
        foreach (var info in lang._FmgInfoBanks.SelectMany((x) => x.Value.FmgInfos))
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
                if (fmgs_vanilla.ContainsKey(kvp.Key))
                {
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
                {
                    entry.Text = "%null%"; // Write this since %null% entries are important
                }

                entry.Text = entry.Text.TrimEnd('\n');

                if (!sharedText.TryGetValue(entry.Text, out var ids))
                {
                    sharedText[entry.Text] = ids = new();
                }
                ids.Add(entry.ID);
            }

            // Make Json sub-entry objects
            List<FmgMergeJsonEntry> mergeEntries = new();

            foreach (var sharedKvp in sharedText)
            {
                var text = sharedKvp.Key;
                text = text.Replace("\r", "");
                text = text.TrimEnd('\n');

                var fmgMergeJsonEntry = new FmgMergeJsonEntry();
                fmgMergeJsonEntry.Text = text;
                fmgMergeJsonEntry.IDList = sharedKvp.Value.ToList();
                mergeEntries.Add(fmgMergeJsonEntry);
            }

            // Make JSON object
            var fmgMergeJson = new FmgMergeJson();
            fmgMergeJson.FMG_ID = (int)kvp.Key;
            fmgMergeJson.Entries = mergeEntries;

            // Write JSON
            string jsonString = JsonSerializer.Serialize(fmgMergeJson, typeof(FmgMergeJson), FmgMergeJsonSerializationContext.Default);

            try
            {
                var fs = new FileStream($@"{path}\{fileName}.fmgmerge.json", System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }

        TaskLogs.AddLog("Finished exporting FMG Merge files", LogLevel.Information, TaskLogs.LogPriority.High);
    }

    private static bool ImportFmg(FmgIDType fmgId, FMG fmg, bool merge)
    {
        foreach (FMGInfo info in Smithbox.BankHandler.FMGBank.FmgInfoBank)
        {
            if (info.FmgID == fmgId)
            {
                if (merge)
                {
                    // Merge mode. Add and replace FMG entries instead of overwriting FMG entirely
                    foreach (var entry in fmg.Entries)
                    {
                        var currentEntry = info.Fmg.Entries.Find(e => e.ID == entry.ID);
                        if (currentEntry == null)
                        {
                            info.Fmg.Entries.Add(entry);
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

    public static bool ImportFmgTxt(FMGLanguage lang, bool merge)
    {
        if (!PlatformUtils.Instance.OpenMultiFileDialog("Choose Files to Import",
                new[] { FilterStrings.FmgMergeJsonFilter }, out IReadOnlyList<string> files))
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
                var jsonString = File.ReadAllText(filePath);
                var resource = JsonSerializer.Deserialize<FmgMergeJson>(jsonString, FmgMergeJsonSerializationContext.Default.FmgMergeJson);

                FMG fmg = new();
                int fmgId = 0;

                fmgId = resource.FMG_ID;

                foreach(var entry in resource.Entries)
                {
                    var entryText = entry.Text;
                    var idList = entry.IDList;

                    foreach(var id in idList)
                    {
                        var newFmgEntry = new FMG.Entry(id, entryText);
                        fmg.Entries.Add(newFmgEntry);
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

        //HandleDuplicateEntries();

        TaskLogs.AddLog($"FMG import: Finished importing {filecount} txt files",
            LogLevel.Information, TaskLogs.LogPriority.Normal);
        return true;
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)
]
[JsonSerializable(typeof(FmgMergeJson))]
[JsonSerializable(typeof(FmgMergeJsonEntry))]
public partial class FmgMergeJsonSerializationContext
    : JsonSerializerContext
{ }

public class FmgMergeJson
{
    public int FMG_ID { get; set; }
    public List<FmgMergeJsonEntry> Entries { get; set; }

    public FmgMergeJson() { }
}

public class FmgMergeJsonEntry
{
    public string Text { get; set; }
    public List<int> IDList { get; set; }

    public FmgMergeJsonEntry() { }
}
