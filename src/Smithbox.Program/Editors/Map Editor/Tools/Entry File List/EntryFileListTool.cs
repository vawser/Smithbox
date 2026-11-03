using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Edits the ENTRYFILELIST files of the project, which list the files the game preloads when the player enters a map.
/// A file named e{ID} belongs to the SpawnPoint region with that entity ID, and e{map}999 to the map itself.
/// A file named i{map}{index} belongs to the auto invade point at that index of the map's AIP.
/// The files are shown apart from the maps, with the map and region each one belongs to resolved from its ID.
/// Edits are kept until they are saved, with the tool's Save button or with the map editor's save.
/// </summary>
public partial class EntryFileListTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    // The files opened so far, by their path
    private readonly Dictionary<string, EntryFileListFile> Files = new();

    // The descriptions of the files in the locator, rebuilt when files are added
    private List<EntryFileListInfo> Infos;

    private string SelectedPath;

    private string ListFilter = "";
    private string EntryFilter = "";

    private string NewEntryPath = "";
    private string AddEntryStatus = "";

    private string NewListName = "";
    private string NewListStatus = "";

    public EntryFileListTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool CanUse()
    {
        return Project.Locator.EntryFileListFiles?.Entries?.Count > 0;
    }

    public bool HasModifiedFiles => Files.Values.Any(f => f.Modified);

    public void OnToolWindow()
    {
        ImGui.BeginChild("entryFileListSection", ImGuiChildFlags.Borders);

        var infos = GetInfos();

        if (infos.Count == 0)
        {
            GUI.WrappedText("This project has no entry file lists.");
            ImGui.EndChild();
            return;
        }

        GUI.WrappedText("Edit the entry file lists, which list the files the game preloads when the player enters a map.");
        GUI.WrappedText("A list named e{ID} is used when the player spawns at the SpawnPoint region with that entity ID, and e{map}999 is the map's default list. A list named i{map}{index} is used when the player invades at the auto invade point with that index.");

        GUI.Spacer();
        GUI.SimpleHeader("Lists", "");

        if (SelectedPath == null || !infos.Any(e => e.Entry.Path == SelectedPath))
            SelectedPath = infos[0].Entry.Path;

        DisplayListSelection(infos);

        var info = infos.First(e => e.Entry.Path == SelectedPath);
        var file = GetFile(info.Entry);

        if (file == null)
        {
            GUI.WrappedText("This file couldn't be read. See the log for details.");
            ImGui.EndChild();
            return;
        }

        GUI.Spacer();
        GUI.SimpleHeader("Owner", "");

        DisplayOwner(info);

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("entryFileListActions",
            "entryFileListSaveButton", "Save",
            "Write this list to the project.",
            () => Save(file),
            "entryFileListRevertButton", "Revert",
            "Discard the unsaved changes to this list. A list that hasn't been saved yet is removed.",
            () => Revert(file));

        DisplayNewList(file);

        GUI.Spacer();
        GUI.SimpleHeader("Entries", "");

        DisplayEntries(file);

        ImGui.EndChild();
    }

    #region Lists
    private void DisplayListSelection(List<EntryFileListInfo> infos)
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##entryFileListListFilter", "Filter by name or map...", ref ListFilter, 255);

        var visible = infos
            .Where(e => ListFilter == ""
                || e.Name.Contains(ListFilter, StringComparison.OrdinalIgnoreCase)
                || (e.MapID?.Contains(ListFilter, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        var height = ImGui.GetTextLineHeightWithSpacing() * 10;

        ImGui.BeginChild("entryFileListListChild", new Vector2(0, height), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(visible.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var info = visible[i];

                if (ImGui.Selectable($"{GetListLabel(info)}##entryFileList_{info.Entry.Path}", info.Entry.Path == SelectedPath))
                {
                    SelectedPath = info.Entry.Path;
                    AddEntryStatus = "";
                    NewListStatus = "";
                }
                GUI.Tooltip(info.Entry.Path);

                if (info.MapID != null)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled(info.MapID);
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private string GetListLabel(EntryFileListInfo info)
    {
        var modified = Files.TryGetValue(info.Entry.Path, out var file) && file.Modified;

        return modified ? $"{info.Name} *" : info.Name;
    }

    private List<EntryFileListInfo> GetInfos()
    {
        var entries = Project.Locator.EntryFileListFiles?.Entries;

        if (entries == null)
            return new();

        if (Infos == null || Infos.Count != entries.Count)
        {
            var maps = Project.Handler.MapData.PrimaryBank.Maps.Keys
                .Select(e => e.Filename)
                .ToHashSet();

            Infos = entries
                .Select(e => EntryFileListInfo.Create(e, Project.Descriptor.ProjectType, maps))
                .OrderBy(e => e.Name)
                .ToList();
        }

        return Infos;
    }
    #endregion

    #region Owner
    private void DisplayOwner(EntryFileListInfo info)
    {
        if (info.ID == null)
        {
            GUI.WrappedText("The name of this list doesn't hold an ID.");
            return;
        }

        if (info.MapID == null)
        {
            GUI.WrappedText($"ID {info.ID} doesn't belong to a map of this project.");
            return;
        }

        GUI.WrappedText($"Map: {info.MapID}");

        var mapName = AliasHelper.GetMapNameAlias(Project, info.MapID);
        if (!string.IsNullOrEmpty(mapName))
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, mapName);
        }

        if (info.Kind is not ('e' or 'i'))
            return;

        // The last 4 digits of an i list's ID are the index of its point in the map's AIP
        var aipIndex = (int)(info.ID.Value % 10000);

        // DS3 has no AIP; its i lists are taken to index the InvasionPoint regions instead.
        // This is unconfirmed: the number of lists matches the number of regions in only some maps.
        var usesInvasionPoints = Project.Descriptor.ProjectType is ProjectType.DS3;

        if (info.Kind == 'i')
        {
            if (usesInvasionPoints)
            {
                GUI.WrappedText($"Invasion point {aipIndex} of {info.MapID}, counting the InvasionPoint regions in the order of the MSB.");
                GUI.WrappedText("This link is assumed: the number of i lists matches the number of InvasionPoint regions in only some maps.");
            }
            else
            {
                GUI.WrappedText($"Auto invade point {aipIndex} of {info.MapID}.");
            }
        }

        var map = View.Selection.GetMapContainerFromMapID(info.MapID);

        if (map == null)
        {
            GUI.WrappedText(info.Kind == 'e'
                ? "Load the map to find the region this list belongs to."
                : usesInvasionPoints
                    ? "Load the map to find the invasion point this list belongs to."
                    : "Load the map to find the auto invade point this list belongs to.");
            return;
        }

        Entity owner;

        if (info.Kind == 'e')
        {
            owner = FindOwner(map, info.ID.Value);

            if (owner == null)
            {
                if (info.ID % 1000 == 999)
                    GUI.WrappedText("No region has this ID, so this is the map's default list.");
                else
                    GUI.WrappedText($"No object in {info.MapID} has the entity ID {info.ID}.");

                return;
            }

            GUI.WrappedText($"Owner: {owner.Name} ({owner.WrappedObject.GetType().Name})");
        }
        else if (usesInvasionPoints)
        {
            var points = map.Regions
                .Where(e => e.WrappedObject is MSB3.Region.InvasionPoint)
                .ToList();

            if (aipIndex >= points.Count)
            {
                GUI.WrappedText($"{info.MapID} has {points.Count} InvasionPoint regions, so it has no invasion point {aipIndex}.");
                return;
            }

            owner = points[aipIndex];

            GUI.WrappedText($"Owner: {owner.Name} (InvasionPoint)");
        }
        else
        {
            var points = map.AutoInvadeParent?.Children;

            if (points == null)
            {
                GUI.WrappedText("The auto invade points of this map aren't loaded. They are only supported for Elden Ring.");
                return;
            }

            if (aipIndex >= points.Count)
            {
                GUI.WrappedText($"{info.MapID} has {points.Count} auto invade points, so it has no point {aipIndex}.");
                return;
            }

            owner = points[aipIndex];

            var point = (AIP.AutoInvadePointInstance)owner.WrappedObject;
            GUI.WrappedText($"Position: {point.Position.X:F2}, {point.Position.Y:F2}, {point.Position.Z:F2}");
        }

        GUI.MultiButtonInput("entryFileListOwnerActions",
            "entryFileListSelectOwner", "Select Owner",
            "Select the region or auto invade point this list belongs to and frame it in the viewport.",
            () =>
            {
                View.ViewportSelection.ClearSelection();
                View.ViewportSelection.AddSelection(owner);
                View.FrameAction.ApplyViewportFrame();
            });
    }

    private static Entity FindOwner(MapContainer map, long id)
    {
        foreach (var e in map.Objects)
        {
            var value = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);

            if (value is int entInt && entInt == id)
                return e;

            if (value is uint entUint && entUint == id)
                return e;
        }

        return null;
    }
    #endregion

    #region Entries
    private void DisplayEntries(EntryFileListFile file)
    {
        var list = file.List;

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##entryFileListEntryFilter", "Filter...", ref EntryFilter, 255);

        var visible = new List<int>();

        for (int i = 0; i < list.Strings.Count; i++)
        {
            if (EntryFilter != "" && !list.Strings[i].Contains(EntryFilter, StringComparison.OrdinalIgnoreCase))
                continue;

            visible.Add(i);
        }

        ImGui.Text($"{list.Strings.Count} entries");

        Action pendingEdit = null;

        var tblFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;
        var height = ImGui.GetTextLineHeightWithSpacing() * 20;

        if (ImGui.BeginTable("entryFileListEntryTable", 3, tblFlags, new Vector2(0, height)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Path", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Hash", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(visible.Count);

            while (clipper.Step())
            {
                for (int v = clipper.DisplayStart; v < clipper.DisplayEnd; v++)
                {
                    var index = visible[v];
                    var path = list.Strings[index];
                    var value = list.Struct2s[index].Unk1;

                    ImGui.PushID(index);
                    ImGui.TableNextRow();

                    // Index, with the context menu for editing the entries
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Selectable($"{index}##entryFileListEntry");
                    GUI.Tooltip("Right-click to copy or delete the entry.");

                    if (ImGui.BeginPopupContextItem("entryFileListEntryContext"))
                    {
                        if (ImGui.Selectable("Copy Path"))
                            PlatformUtils.Instance.SetClipboardText(path);

                        ImGui.Separator();

                        if (ImGui.Selectable("Delete"))
                            pendingEdit = () => RemoveEntry(file, index);

                        ImGui.EndPopup();
                    }

                    // Path
                    ImGui.TableSetColumnIndex(1);
                    if (file.CopiedEntries.TryGetValue(path, out var source))
                    {
                        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, path);
                        GUI.Tooltip($"Added with the hash this path has in {source}.");
                    }
                    else if (file.BlankEntries.Contains(path))
                    {
                        ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, path);
                        GUI.Tooltip("Added with a blank hash, which doesn't match the path.");
                    }
                    else
                    {
                        ImGui.Text(path);
                    }

                    // Hash
                    ImGui.TableSetColumnIndex(2);
                    ImGui.TextDisabled($"{(uint)value:X8}");
                    GUI.Tooltip($"Full value: 0x{value:X16}\nThe low 32 bits are the hash of the path. The high bits differ between lists and aren't understood.");

                    ImGui.PopID();
                }
            }

            clipper.End();

            ImGui.EndTable();
        }

        // Entries are removed after the table, so its rows don't shift while it is drawn
        if (pendingEdit != null)
        {
            pendingEdit();
            file.Modified = true;
        }

        DisplayAddEntry(file);
    }

    private void DisplayAddEntry(EntryFileListFile file)
    {
        GUI.Spacer();

        GUI.WrappedText("Add an entry by its path. The hash of a path can't be computed yet, so it is taken from another list that has the path, or left blank.");

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##entryFileListNewEntryPath", "device:/path", ref NewEntryPath, 512);

        GUI.MultiButtonInput("entryFileListAddEntryButtons",
            "entryFileListAddEntry", "Add Entry",
            "Add the path to this list, with its hash taken from the first other list found that has it. Lists of the same map are searched first. Searching every list can take a while for Elden Ring.",
            () => AddEntry(file, NewEntryPath.Trim()),
            "entryFileListAddBlankEntry", "Add With Blank Hash",
            "Add the path to this list with a hash of 0, without searching the other lists. The game may not find the entry, as its hash won't match its path.",
            () => AddBlankEntry(file, NewEntryPath.Trim()));

        if (AddEntryStatus != "")
            GUI.WrappedText(AddEntryStatus);
    }

    private bool CanAddEntry(EntryFileListFile file, string path)
    {
        if (path == "")
        {
            AddEntryStatus = "Enter the path of the file to add.";
            return false;
        }

        if (file.List.Strings.Any(e => e.Equals(path, StringComparison.OrdinalIgnoreCase)))
        {
            AddEntryStatus = $"This list already has {path}.";
            return false;
        }

        return true;
    }

    private void AddBlankEntry(EntryFileListFile file, string path)
    {
        if (!CanAddEntry(file, path))
            return;

        file.List.Strings.Add(path);
        file.List.Struct2s.Add(new ENFL.Struct2(0));
        file.BlankEntries.Add(path);
        file.Modified = true;

        AddEntryStatus = $"Added {path}, with a blank hash.";
        NewEntryPath = "";
    }

    private void AddEntry(EntryFileListFile file, string path)
    {
        if (!CanAddEntry(file, path))
            return;

        var found = FindHash(file, path);

        if (found == null)
        {
            AddEntryStatus = $"No other list has {path}, so its hash isn't known. Use Add With Blank Hash to add it anyway.";
            return;
        }

        var (value, sourceName, foundPath) = found.Value;

        file.List.Strings.Add(foundPath);
        file.List.Struct2s.Add(new ENFL.Struct2(value));
        file.CopiedEntries[foundPath] = sourceName;
        file.Modified = true;

        AddEntryStatus = $"Added {foundPath}, with its hash from {sourceName}.";
        NewEntryPath = "";
    }

    private void RemoveEntry(EntryFileListFile file, int index)
    {
        file.CopiedEntries.Remove(file.List.Strings[index]);
        file.BlankEntries.Remove(file.List.Strings[index]);
        file.List.Strings.RemoveAt(index);
        file.List.Struct2s.RemoveAt(index);
    }

    /// <summary>
    /// Finds the value a path has in another list: the lists already opened, then the lists of the same map, then the rest.
    /// The path is returned as it is written there, as the hash may depend on its case.
    /// </summary>
    private (long value, string source, string path)? FindHash(EntryFileListFile target, string path)
    {
        var infos = GetInfos();
        var targetInfo = infos.FirstOrDefault(e => e.Entry.Path == target.Entry.Path);

        var candidates = infos
            .Where(e => e.Entry.Path != target.Entry.Path)
            .OrderByDescending(e => Files.ContainsKey(e.Entry.Path))
            .ThenByDescending(e => targetInfo?.MapID != null && e.MapID == targetInfo.MapID)
            .ToList();

        foreach (var info in candidates)
        {
            // Opened lists are searched as edited; the others are read without being kept
            var list = Files.TryGetValue(info.Entry.Path, out var opened)
                ? opened?.List
                : ReadList(info.Entry, false);

            if (list == null)
                continue;

            var index = list.Strings.FindIndex(e => e.Equals(path, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
                return (list.Struct2s[index].Unk1, info.Name, list.Strings[index]);
        }

        return null;
    }
    #endregion

    #region New List
    private void DisplayNewList(EntryFileListFile file)
    {
        GUI.Spacer();

        GUI.WrappedText("Create a list as a copy of this one, such as for a new SpawnPoint region (e followed by its entity ID) or auto invade point (i followed by the map's ID prefix and the point's index).");

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##entryFileListNewListName", $"{GetInfos().First(e => e.Entry.Path == file.Entry.Path).Name[0]}ID", ref NewListName, 64);

        GUI.MultiButtonInput("entryFileListNewListButtons",
            "entryFileListDuplicate", "Duplicate List",
            "Create a new list with the entries of this one. It is written to the project when it is saved.",
            () => DuplicateList(file, NewListName.Trim()));

        if (NewListStatus != "")
            GUI.WrappedText(NewListStatus);
    }

    private void DuplicateList(EntryFileListFile file, string name)
    {
        if (!ListNameRegex().IsMatch(name))
        {
            NewListStatus = "The name must be e or i followed by an ID, such as e3002960.";
            return;
        }

        var source = file.Entry;
        var folder = GetListFolder(source, name);
        var path = $"{folder}/{name}.{source.Extension}";

        if (Project.Locator.EntryFileListFiles.Entries.Any(e => e.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
        {
            NewListStatus = $"A list named {name} already exists.";
            return;
        }

        var entry = source.Clone();
        entry.Path = path;
        entry.Folder = folder;
        entry.Filename = name;

        var list = new ENFL
        {
            Struct1s = file.List.Struct1s.Select(e => new ENFL.Struct1 { Step = e.Step, Index = e.Index }).ToList(),
            Struct2s = file.List.Struct2s.Select(e => new ENFL.Struct2(e.Unk1)).ToList(),
            Strings = file.List.Strings.ToList()
        };

        Project.Locator.EntryFileListFiles.Entries.Add(entry);

        Files[path] = new EntryFileListFile
        {
            Entry = entry,
            List = list,
            CopiedEntries = new(file.CopiedEntries),
            BlankEntries = new(file.BlankEntries),
            Modified = true,
            IsNew = true
        };

        Infos = null;
        SelectedPath = path;
        NewListName = "";
        NewListStatus = $"Created {name}. Save it to write it to the project.";
    }

    /// <summary>
    /// The folder a new list goes in. Elden Ring and later keep the lists in folders named after the first 7 characters of their names.
    /// </summary>
    private static string GetListFolder(FileDictionaryEntry source, string name)
    {
        var folder = source.Folder.TrimEnd('/');
        var lastPart = folder[(folder.LastIndexOf('/') + 1)..];

        if (lastPart.Equals("entryfilelist", StringComparison.OrdinalIgnoreCase))
            return folder;

        var parent = folder[..folder.LastIndexOf('/')];

        return name.Length >= 7 ? $"{parent}/{name[..7]}" : folder;
    }

    [GeneratedRegex(@"^[ei]\d+(_\d+)?$")]
    private static partial Regex ListNameRegex();
    #endregion

    #region Files
    private EntryFileListFile GetFile(FileDictionaryEntry entry)
    {
        if (Files.TryGetValue(entry.Path, out var file))
            return file;

        var list = ReadList(entry, true);

        file = list == null ? null : new EntryFileListFile
        {
            Entry = entry,
            List = list
        };

        Files[entry.Path] = file;

        return file;
    }

    private ENFL ReadList(FileDictionaryEntry entry, bool logErrors)
    {
        var fs = Project.VFS.FS;

        if (!fs.FileExists(entry.Path))
        {
            if (logErrors)
                Smithbox.LogError(this, LOC.Get("MAP_Entry_File_List_Failed_Find", entry.Path));
            return null;
        }

        try
        {
            return ENFL.Read(fs.ReadFile(entry.Path).Value);
        }
        catch (Exception e)
        {
            if (logErrors)
                Smithbox.LogError(this, LOC.Get("MAP_Entry_File_List_Failed_Read", entry.Path), e);
            return null;
        }
    }

    private void Revert(EntryFileListFile file)
    {
        Files.Remove(file.Entry.Path);
        AddEntryStatus = "";

        // A list that was never written has nothing to go back to
        if (file.IsNew)
        {
            Project.Locator.EntryFileListFiles.Entries.Remove(file.Entry);
            Infos = null;
            SelectedPath = null;
        }
    }

    private void Save(EntryFileListFile file)
    {
        try
        {
            Project.VFS.ProjectFS.WriteFile(file.Entry.Path, file.List.Write());
            file.Modified = false;
            file.IsNew = false;

            Smithbox.Log(this, LOC.Get("MAP_Entry_File_List_Saved", file.Entry.Path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Entry_File_List_Failed_Write", file.Entry.Path), e);
        }
    }

    /// <summary>
    /// Saves the lists with unsaved changes, for the map editor's save.
    /// </summary>
    public void SaveModified()
    {
        foreach (var file in Files.Values.Where(f => f != null && f.Modified).ToList())
        {
            Save(file);
        }
    }
    #endregion
}

public class EntryFileListFile
{
    public FileDictionaryEntry Entry;
    public ENFL List;
    public bool Modified;

    // Set for a list created in the tool that hasn't been written yet
    public bool IsNew;

    // The paths added with a hash taken from another list, with the name of that list
    public Dictionary<string, string> CopiedEntries = new();

    // The paths added with a blank hash
    public HashSet<string> BlankEntries = new();
}

/// <summary>
/// What the name of an entry file list says: its kind (e or i), its ID, and the map the ID belongs to.
/// </summary>
public class EntryFileListInfo
{
    public FileDictionaryEntry Entry;
    public string Name;
    public char Kind;
    public long? ID;

    // Null if the ID doesn't belong to a map of the project
    public string MapID;

    public static EntryFileListInfo Create(FileDictionaryEntry entry, ProjectType type, HashSet<string> maps)
    {
        var info = new EntryFileListInfo
        {
            Entry = entry,
            Name = entry.Filename,
            Kind = entry.Filename.Length > 0 ? entry.Filename[0] : ' '
        };

        // BB names have a suffix: e002102950_00
        var digits = entry.Filename.Length > 1 ? entry.Filename[1..].Split('_')[0] : "";

        if (long.TryParse(digits, out var id))
        {
            info.ID = id;

            var mapID = GetMapID(id, type);

            if (mapID != null && maps.Contains(mapID))
                info.MapID = mapID;
        }

        return info;
    }

    /// <summary>
    /// The map an entity ID belongs to, from the layout of the game's IDs.
    /// The result is only used if the project has a map of that name.
    /// </summary>
    private static string GetMapID(long id, ProjectType type)
    {
        if (type is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT)
        {
            // AABxxxx: m{AA}_{0B}_00_00
            if (id < 1_000_000)
                return null;

            return $"m{id / 100_000:D2}_{id / 10_000 % 10:D2}_00_00";
        }

        if (id >= 1_000_000_000)
        {
            var area = id / 100_000_000;

            // The overworld IDs start with 10 for m60 and 20 for m61: 10XXYYxxxx is m60_XX_YY_00
            if (type is ProjectType.ER or ProjectType.NR)
            {
                if (area == 10)
                    area = 60;
                else if (area == 20)
                    area = 61;
            }

            return $"m{area:D2}_{id / 1_000_000 % 100:D2}_{id / 10_000 % 100:D2}_00";
        }

        // AABBxxxx: m{AA}_{BB}_00_00
        if (id < 1_000_000)
            return null;

        return $"m{id / 1_000_000:D2}_{id / 10_000 % 100:D2}_00_00";
    }
}
