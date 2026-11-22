using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Edits the AI thread info files of the project (AC6): /map/aithreadinfo/{missionId}.aith.dcx, which give each Enemy of
/// a mission a worker slot. A row refers to an Enemy by its index in its map's MSB, so adding or removing enemies leaves
/// the rows pointing at the wrong enemies; Generate from Maps rebuilds them from the loaded maps.
/// Edits are kept until they are saved, with the tool's Save button or with the map editor's save.
/// </summary>
public class AiThreadInfoTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private const string Folder = "/map/aithreadinfo";

    // The files by mission ID, once read
    private SortedDictionary<int, AiThreadInfoFile> Files;

    // The block index of each map, from the rows of every file
    private Dictionary<string, byte> KnownBlocks = new();

    private int SelectedMission = -1;
    private string ListFilter = "";

    private string NewMission = "";
    private string NewFileStatus = "";

    private string NewMapID = "";
    private int NewMapBlock = 0;
    private string NewMapStatus = "";

    public AiThreadInfoTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private IEnumerable<FileDictionaryEntry> Entries =>
        Project.Locator.AiThreadInfoFiles?.Entries ?? Enumerable.Empty<FileDictionaryEntry>();

    public bool CanUse()
    {
        return Project.Descriptor.ProjectType is ProjectType.AC6 && Entries.Any();
    }

    public bool HasModifiedFiles => Files?.Values.Any(e => e.Modified) ?? false;

    public void OnToolWindow()
    {
        ImGui.BeginChild("aiThreadInfoSection", ImGuiChildFlags.Borders);

        var files = GetFiles();

        GUI.WrappedText("Edit the AI thread info of each mission, which gives each Enemy a worker slot out of 5, 6 and 12.");
        GUI.WrappedText("A row refers to an Enemy by its position among the Enemy parts of its map. After adding, removing or reordering enemies, load the mission's maps and generate its file from them.");

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("aiThreadInfoActions",
            "aiThreadInfoSaveButton", "Save",
            "Write the changed files to the project.",
            () => Save(),
            "aiThreadInfoRevertButton", "Revert",
            "Discard the unsaved changes to all of the files.",
            () => Revert());

        GUI.Spacer();
        GUI.SimpleHeader("Missions", "");

        if (!files.ContainsKey(SelectedMission))
            SelectedMission = files.Keys.FirstOrDefault(-1);

        DisplayFileSelection(files);
        DisplayNewFile(files);

        if (!files.TryGetValue(SelectedMission, out var file))
        {
            ImGui.EndChild();
            return;
        }

        GUI.Spacer();
        GUI.SimpleHeader("Maps", "");

        DisplayMaps(file);

        GUI.Spacer();
        GUI.SimpleHeader("Entries", "");

        DisplayEntries(file);

        ImGui.EndChild();
    }

    #region Missions
    private void DisplayFileSelection(SortedDictionary<int, AiThreadInfoFile> files)
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##aiThreadInfoFilter", "Filter by mission...", ref ListFilter, 255);

        var visible = files.Keys
            .Where(e => ListFilter == ""
                || e.ToString().Contains(ListFilter, StringComparison.OrdinalIgnoreCase)
                || GetMissionName(e).Contains(ListFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var height = ImGui.GetTextLineHeightWithSpacing() * 10;

        ImGui.BeginChild("aiThreadInfoFileList", new Vector2(0, height), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(visible.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var mission = visible[i];
                var file = files[mission];
                var label = file.Modified ? $"{mission:D4} *" : $"{mission:D4}";

                if (ImGui.Selectable($"{label}##aiThreadInfo_{mission}", mission == SelectedMission))
                {
                    SelectedMission = mission;
                    NewFileStatus = "";
                    NewMapStatus = "";
                }

                var name = GetMissionName(mission);
                if (name != "")
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, name);
                }

                if (file.Aith == null)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, "(unreadable)");
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private void DisplayNewFile(SortedDictionary<int, AiThreadInfoFile> files)
    {
        GUI.WrappedText("Add a file for a mission that has none, by its MissionParam ID. Then add its maps and generate it.");

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##aiThreadInfoNewMission", "Mission ID, such as 2010", ref NewMission, 16);

        GUI.MultiButtonInput("aiThreadInfoNewFileButtons",
            "aiThreadInfoAddFile", "Add File",
            "Add an empty file for the mission. It is written to the project when the files are saved.",
            () => AddFile(files, NewMission.Trim()));

        if (NewFileStatus != "")
            GUI.WrappedText(NewFileStatus);
    }

    private void AddFile(SortedDictionary<int, AiThreadInfoFile> files, string text)
    {
        if (!int.TryParse(text, out var mission) || mission < 0 || mission > 9999)
        {
            NewFileStatus = "The mission ID must be a number from 0 to 9999.";
            return;
        }

        if (files.ContainsKey(mission))
        {
            NewFileStatus = $"Mission {mission:D4} already has a file.";
            return;
        }

        files[mission] = new AiThreadInfoFile
        {
            Path = $"{Folder}/{mission:D4}.aith.dcx",
            Aith = new AITH(),
            // New files are compressed like the files the project already has
            Compression = files.Values.FirstOrDefault(e => e.Aith != null)?.Compression ?? DCX.Type.DCX_KRAK,
            Modified = true,
        };

        SelectedMission = mission;
        NewMission = "";
        NewFileStatus = $"Added a file for mission {mission:D4}. Add its maps below, then generate it.";
    }

    private string GetMissionName(int mission)
    {
        var bank = Project.Handler.ParamData?.PrimaryBank;

        if (bank?.Params == null || !bank.Params.TryGetValue("MissionParam", out var param))
            return "";

        return param[mission]?.Name ?? "";
    }
    #endregion

    #region Maps
    private void DisplayMaps(AiThreadInfoFile file)
    {
        if (file.Aith == null)
        {
            GUI.WrappedText("This file couldn't be read. See the log for details.");
            return;
        }

        var maps = GetFileMaps(file);

        if (maps.Count == 0)
            GUI.WrappedText("This file covers no maps yet. Add the mission's map below.");

        var tblFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable;

        MapRefRemoval pending = null;

        if (maps.Count > 0 && ImGui.BeginTable("aiThreadInfoMapTable", 5, tblFlags))
        {
            ImGui.TableSetupColumn("Map", ImGuiTableColumnFlags.WidthStretch, 0.35f);
            ImGui.TableSetupColumn("Block", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Rows", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Enemies", ImGuiTableColumnFlags.WidthStretch, 0.45f);
            ImGui.TableSetupColumn("##remove", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            foreach (var map in maps)
            {
                ImGui.PushID(map.MapID);
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.Text(map.MapID);
                var alias = AliasHelper.GetMapNameAlias(Project, map.MapID);
                if (!string.IsNullOrEmpty(alias))
                    GUI.Tooltip(alias);

                ImGui.TableSetColumnIndex(1);
                ImGui.Text($"0x{map.BlockIndex:X2}");
                GUI.Tooltip("The map's slot in the game's map list, which every row of the map carries.");

                ImGui.TableSetColumnIndex(2);
                ImGui.Text(file.Aith.Entries.Count(e => e.MapName == map.MapID).ToString());

                ImGui.TableSetColumnIndex(3);
                var container = View.Selection.GetMapContainerFromMapID(map.MapID);
                if (container == null)
                {
                    ImGui.TextDisabled("Not loaded");
                }
                else
                {
                    // Vanilla files leave some enemies without a row and have a few rows past the last enemy,
                    // so neither is marked as an error
                    var count = AiThreadInfoBuilder.GetEnemies(container).Count;
                    var indices = file.Aith.Entries.Where(e => e.MapName == map.MapID).Select(e => e.Index).ToHashSet();
                    var missing = Enumerable.Range(0, count).Count(i => !indices.Contains(i));
                    var past = indices.Count(i => i >= count);

                    ImGui.Text($"{count}");
                    GUI.Tooltip($"{count} enemies in the loaded map. {missing} have no row; {past} rows are past the last enemy.");

                    if (missing > 0 || past > 0)
                    {
                        ImGui.SameLine();
                        ImGui.TextDisabled($"({missing} without a row, {past} past the end)");
                    }
                }

                ImGui.TableSetColumnIndex(4);
                if (ImGui.SmallButton("Remove"))
                    pending = new MapRefRemoval(map.MapID);
                GUI.Tooltip("Remove this map and its rows from the file.");

                ImGui.PopID();
            }

            ImGui.EndTable();
        }

        if (pending != null)
        {
            file.Aith.Entries.RemoveAll(e => e.MapName == pending.MapID);
            file.PendingMaps.RemoveAll(e => e.MapID == pending.MapID);
            file.Modified = true;
        }

        DisplayGenerate(file, maps);
        DisplayAddMap(file, maps);
    }

    private record MapRefRemoval(string MapID);

    private void DisplayGenerate(AiThreadInfoFile file, List<AiThreadInfoBuilder.MapRef> maps)
    {
        if (maps.Count == 0)
            return;

        var unloaded = maps.Where(e => View.Selection.GetMapContainerFromMapID(e.MapID) == null).Select(e => e.MapID).ToList();

        if (unloaded.Count > 0)
        {
            GUI.WrappedText($"Load {string.Join(", ", unloaded)} to generate this file.");
            return;
        }

        GUI.MultiButtonInput("aiThreadInfoGenerateButtons",
            "aiThreadInfoGenerate", "Generate from Maps",
            "Replace the rows with one row for each Enemy of each map above, including edits that haven't been saved, with the slots handed out in turn. Rows past the last Enemy (characters spawned at runtime) are dropped.",
            () =>
            {
                file.Aith.Entries = AiThreadInfoBuilder.Build(maps.Select(e =>
                    (e, AiThreadInfoBuilder.GetEnemies(View.Selection.GetMapContainerFromMapID(e.MapID)).Count)));
                file.PendingMaps.Clear();
                file.Modified = true;
            });
    }

    private void DisplayAddMap(AiThreadInfoFile file, List<AiThreadInfoBuilder.MapRef> maps)
    {
        GUI.WrappedText("Add a map whose enemies the file should cover: the mission's map, and its placement map (MissionParam placementMapUid) if that has enemies. Its rows are made when the file is generated.");

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputTextWithHint("##aiThreadInfoNewMap", "mXX_XX_XX_XX", ref NewMapID, 64);

        // A map that another file covers already has a block index; use it rather than asking for one
        var known = KnownBlocks.TryGetValue(NewMapID.Trim(), out var knownBlock);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        if (known)
        {
            ImGui.TextDisabled($"Block 0x{knownBlock:X2}");
        }
        else
        {
            ImGui.InputInt("##aiThreadInfoNewMapBlock", ref NewMapBlock);
            NewMapBlock = Math.Clamp(NewMapBlock, 0, 255);
            GUI.Tooltip("The map's slot in the game's map list. No file of the project covers this map, so it can't be looked up. Vanilla maps use 0x08-0x71.");
        }

        GUI.MultiButtonInput("aiThreadInfoAddMapButtons",
            "aiThreadInfoAddMap", "Add Map",
            "Add the map to the file.",
            () =>
            {
                var mapID = NewMapID.Trim();

                if (AiThreadInfoBuilder.ParseMapID(mapID) == null)
                {
                    NewMapStatus = "The map must be a map ID, such as m01_01_10_00.";
                    return;
                }

                if (maps.Any(e => e.MapID == mapID))
                {
                    NewMapStatus = $"The file already covers {mapID}.";
                    return;
                }

                file.PendingMaps.Add(new AiThreadInfoBuilder.MapRef(mapID, known ? knownBlock : (byte)NewMapBlock));
                file.Modified = true;
                NewMapID = "";
                NewMapStatus = $"Added {mapID}. Load it and generate the file to make its rows.";
            });

        if (NewMapStatus != "")
            GUI.WrappedText(NewMapStatus);
    }

    /// <summary>
    /// The maps of a file: those its rows refer to, then those added since that have no rows yet.
    /// </summary>
    private static List<AiThreadInfoBuilder.MapRef> GetFileMaps(AiThreadInfoFile file)
    {
        var maps = AiThreadInfoBuilder.GetMaps(file.Aith);

        foreach (var map in file.PendingMaps)
        {
            if (!maps.Any(e => e.MapID == map.MapID))
                maps.Add(map);
        }

        return maps;
    }
    #endregion

    #region Entries
    private void DisplayEntries(AiThreadInfoFile file)
    {
        if (file.Aith == null)
            return;

        var entries = file.Aith.Entries;
        var enemiesByMap = new Dictionary<string, List<Entity>>();

        ImGui.Text($"{entries.Count} rows");

        var tblFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;
        var height = ImGui.GetTextLineHeightWithSpacing() * 20;

        if (ImGui.BeginTable("aiThreadInfoEntryTable", 6, tblFlags, new Vector2(0, height)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Map", ImGuiTableColumnFlags.WidthStretch, 0.2f);
            ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthStretch, 0.15f);
            ImGui.TableSetupColumn("Slot / 5", ImGuiTableColumnFlags.WidthStretch, 0.12f);
            ImGui.TableSetupColumn("Slot / 6", ImGuiTableColumnFlags.WidthStretch, 0.12f);
            ImGui.TableSetupColumn("Slot / 12", ImGuiTableColumnFlags.WidthStretch, 0.12f);
            ImGui.TableSetupColumn("Enemy", ImGuiTableColumnFlags.WidthStretch, 0.29f);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(entries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var entry = entries[i];

                    ImGui.PushID(i);
                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.Text(entry.MapName);

                    ImGui.TableSetColumnIndex(1);
                    var index = entry.Index;
                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.InputInt("##aiThreadInfoIndex", ref index) && index >= 0)
                    {
                        entry.Index = index;
                        file.Modified = true;
                    }

                    DisplaySlot(file, "##aiThreadInfoSlot5", 2, 5, entry.Slot5, v => entry.Slot5 = v);
                    DisplaySlot(file, "##aiThreadInfoSlot6", 3, 6, entry.Slot6, v => entry.Slot6 = v);
                    DisplaySlot(file, "##aiThreadInfoSlot12", 4, 12, entry.Slot12, v => entry.Slot12 = v);

                    // Enemy, from the loaded map: the row belongs to the Enemy at its index
                    ImGui.TableSetColumnIndex(5);
                    if (!enemiesByMap.TryGetValue(entry.MapName, out var enemies))
                    {
                        var container = View.Selection.GetMapContainerFromMapID(entry.MapName);
                        enemies = container != null ? AiThreadInfoBuilder.GetEnemies(container) : null;
                        enemiesByMap[entry.MapName] = enemies;
                    }

                    if (enemies == null)
                    {
                        ImGui.TextDisabled("-");
                        GUI.Tooltip("Load the map to see the enemy each row belongs to.");
                    }
                    else if (entry.Index >= enemies.Count)
                    {
                        ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, "None");
                        GUI.Tooltip("The loaded map has fewer enemies than this index. Vanilla files have a few such rows, probably for characters spawned at runtime.");
                    }
                    else
                    {
                        var enemy = enemies[entry.Index];

                        if (ImGui.Selectable($"{enemy.Name}##aiThreadInfoEnemy"))
                        {
                            View.ViewportSelection.ClearSelection();
                            View.ViewportSelection.AddSelection(enemy);
                            View.FrameAction.ApplyViewportFrame();
                        }

                        GUI.Tooltip("Click to select this enemy.");
                    }

                    ImGui.PopID();
                }
            }

            clipper.End();

            ImGui.EndTable();
        }

        GUI.MultiButtonInput("aiThreadInfoSlotButtons",
            "aiThreadInfoRedistribute", "Redistribute Slots",
            "Hand out the slots of every row in turn, (0,0,0), (1,1,1), ..., keeping the rows.",
            () =>
            {
                AiThreadInfoBuilder.AssignSlots(entries);
                file.Modified = true;
            });
    }

    private static void DisplaySlot(AiThreadInfoFile file, string id, int column, int count, byte value, Action<byte> set)
    {
        ImGui.TableSetColumnIndex(column);

        var slot = (int)value;
        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputInt(id, ref slot))
        {
            set((byte)Math.Clamp(slot, 0, count - 1));
            file.Modified = true;
        }

        GUI.Tooltip($"The worker slot out of {count}: 0 to {count - 1}.");
    }
    #endregion

    #region Files
    private SortedDictionary<int, AiThreadInfoFile> GetFiles()
    {
        if (Files != null)
            return Files;

        Files = new();
        KnownBlocks.Clear();

        foreach (var entry in Entries)
        {
            if (!int.TryParse(entry.Filename, out var mission) || Files.ContainsKey(mission))
                continue;

            var file = ReadFile(entry.Path);
            Files[mission] = file;

            if (file.Aith == null)
                continue;

            foreach (var map in AiThreadInfoBuilder.GetMaps(file.Aith))
                KnownBlocks.TryAdd(map.MapID, map.BlockIndex);
        }

        return Files;
    }

    private AiThreadInfoFile ReadFile(string path)
    {
        var file = new AiThreadInfoFile { Path = path };

        try
        {
            var data = Project.VFS.FS.ReadFile(path).Value;
            var compression = DCX.Type.None;

            if (DCX.Is(data))
                data = DCX.Decompress(data, out compression);

            file.Aith = AITH.Read(data);
            file.Compression = compression;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_AI_Thread_Info_Failed_Read", path), e);
        }

        return file;
    }

    private void Revert()
    {
        Files = null;
        NewFileStatus = "";
        NewMapStatus = "";
    }

    private void Save()
    {
        if (Files == null)
            return;

        foreach (var file in Files.Values.Where(e => e.Modified && e.Aith != null))
        {
            try
            {
                var data = file.Aith.Write();

                if (file.Compression != DCX.Type.None)
                    data = DCX.Compress(data, file.Compression);

                Project.VFS.ProjectFS.WriteFile(file.Path, data);
                file.Modified = false;

                Smithbox.Log(this, LOC.Get("MAP_AI_Thread_Info_Saved", file.Path));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_AI_Thread_Info_Failed_Write", file.Path), e);
            }
        }
    }

    /// <summary>
    /// Saves the files that have unsaved changes, for the map editor's save.
    /// </summary>
    public void SaveModified()
    {
        if (HasModifiedFiles)
            Save();
    }
    #endregion
}

public class AiThreadInfoFile
{
    public string Path;
    public AITH Aith;
    public DCX.Type Compression;
    public bool Modified;

    // Maps added to the file that have no rows yet
    public List<AiThreadInfoBuilder.MapRef> PendingMaps = new();
}
