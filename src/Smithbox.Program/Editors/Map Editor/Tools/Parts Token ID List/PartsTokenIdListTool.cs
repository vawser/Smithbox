using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Edits the parts token ID lists of the project (AC6): the PartsTokenParam IDs of the enemies of each map,
/// kept as one PTIDL file per map in partstokenidlist.ptidlbnd.dcx.
/// The entries of a list follow the enemies of its map, so a list can be compared with a loaded map and generated from it.
/// Edits are kept until they are saved, with the tool's Save button or with the map editor's save.
/// </summary>
public partial class PartsTokenIdListTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    // The binder, once opened
    private PartsTokenIdListBinder Binder;
    private bool ReadFailed = false;

    private string SelectedMap;

    private string ListFilter = "";

    private string NewListName = "";
    private string NewListStatus = "";

    // The name and flags new lists are given when no list of the binder is left to copy them from
    private const string DefaultFileFolder = @"W:\FNR\data\Target\INTERROOT_win64\map\PartsTokenIdList\List\";
    private const string FileExtension = ".ptidl";

    public PartsTokenIdListTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private FileDictionaryEntry Entry =>
        Project.Locator.PartsTokenIdListFiles?.Entries?.OrderBy(e => e.Path).FirstOrDefault();

    public bool CanUse()
    {
        return Project.Descriptor.ProjectType is ProjectType.AC6 && Entry != null;
    }

    public bool HasModifiedFiles => Binder?.Modified ?? false;

    public void OnToolWindow()
    {
        ImGui.BeginChild("partsTokenIdListSection", ImGuiChildFlags.Borders);

        var binder = GetBinder();

        if (binder == null)
        {
            GUI.WrappedText("The parts token ID lists couldn't be read. See the log for details.");
            ImGui.EndChild();
            return;
        }

        GUI.WrappedText("Edit the parts token ID lists, which hold the PartsTokenParam IDs of the enemies of each map.");
        GUI.WrappedText("A list holds the PartsTokenParamID of each Enemy, then each DummyEnemy, whose ID isn't -1, in the order of the map's MSB. Generate a list from its map after changing the enemies or their PartsTokenParamIDs.");

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("partsTokenIdListActions",
            "partsTokenIdListSaveButton", "Save",
            "Write the lists to the project.",
            () => Save(),
            "partsTokenIdListRevertButton", "Revert",
            "Discard the unsaved changes to all of the lists.",
            () => Revert());

        GUI.Spacer();
        GUI.SimpleHeader("Lists", "");

        if (SelectedMap == null || !binder.Lists.ContainsKey(SelectedMap))
            SelectedMap = binder.Lists.Keys.FirstOrDefault();

        DisplayListSelection(binder);
        DisplayNewList(binder);

        if (SelectedMap == null)
        {
            ImGui.EndChild();
            return;
        }

        var list = binder.Lists[SelectedMap];

        GUI.Spacer();
        GUI.SimpleHeader("Map", "");

        DisplayMap(binder, list);

        GUI.Spacer();
        GUI.SimpleHeader("Entries", "");

        DisplayEntries(binder, list);

        ImGui.EndChild();
    }

    #region Lists
    private void DisplayListSelection(PartsTokenIdListBinder binder)
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##partsTokenIdListListFilter", "Filter by map...", ref ListFilter, 255);

        var maps = GetProjectMaps();

        var visible = binder.Lists.Keys
            .Where(e => ListFilter == ""
                || e.Contains(ListFilter, StringComparison.OrdinalIgnoreCase)
                || (AliasHelper.GetMapNameAlias(Project, e)?.Contains(ListFilter, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();

        var height = ImGui.GetTextLineHeightWithSpacing() * 10;

        ImGui.BeginChild("partsTokenIdListListChild", new Vector2(0, height), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(visible.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var mapID = visible[i];
                var label = binder.ModifiedLists.Contains(mapID) ? $"{mapID} *" : mapID;

                if (ImGui.Selectable($"{label}##partsTokenIdList_{mapID}", mapID == SelectedMap))
                {
                    SelectedMap = mapID;
                    NewListStatus = "";
                }

                var alias = AliasHelper.GetMapNameAlias(Project, mapID);
                if (!string.IsNullOrEmpty(alias))
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);
                }

                if (!maps.Contains(mapID))
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled("(no map)");
                }
                else if (!MatchesMap(binder.Lists[mapID], mapID))
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, "(differs from map)");
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private void DisplayNewList(PartsTokenIdListBinder binder)
    {
        GUI.WrappedText("Add a list for a map that has none, or remove the selected list. A new list is generated from its map if the map is loaded.");

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##partsTokenIdListNewListName", "mXX_XX_XX_XX", ref NewListName, 64);

        GUI.MultiButtonInput("partsTokenIdListNewListButtons",
            "partsTokenIdListAddList", "Add List",
            "Add a list for the map. It is written to the project when the lists are saved.",
            () => AddList(binder, NewListName.Trim()),
            "partsTokenIdListRemoveList", "Remove List",
            "Remove the selected list. It is removed from the project when the lists are saved.",
            () => RemoveList(binder));

        if (NewListStatus != "")
            GUI.WrappedText(NewListStatus);
    }

    private void AddList(PartsTokenIdListBinder binder, string mapID)
    {
        if (!MapNameRegex().IsMatch(mapID))
        {
            NewListStatus = "The name must be a map ID, such as m01_03_70_00.";
            return;
        }

        if (binder.Lists.ContainsKey(mapID))
        {
            NewListStatus = $"{mapID} already has a list.";
            return;
        }

        var list = new PartsTokenIdList();
        var map = View.Selection.GetMapContainerFromMapID(mapID);

        if (map != null)
            list.IDs = PartsTokenIdList.GetMapIDs(map);

        binder.Lists[mapID] = list;
        binder.ModifiedLists.Add(mapID);
        binder.Modified = true;

        SelectedMap = mapID;
        NewListName = "";

        if (map != null)
            NewListStatus = $"Added a list for {mapID}, generated from the loaded map. Save the lists to write it to the project.";
        else if (GetProjectMaps().Contains(mapID))
            NewListStatus = $"Added an empty list for {mapID}. Load the map to generate it.";
        else
            NewListStatus = $"Added an empty list for {mapID}. No map of the project has this name, so the game won't use it.";
    }

    private void RemoveList(PartsTokenIdListBinder binder)
    {
        if (SelectedMap == null)
            return;

        binder.Lists.Remove(SelectedMap);
        binder.ModifiedLists.Remove(SelectedMap);
        binder.Modified = true;

        NewListStatus = $"Removed the list of {SelectedMap}. Save the lists to remove it from the project.";
        SelectedMap = null;
    }

    private HashSet<string> GetProjectMaps()
    {
        return Project.Handler.MapData.PrimaryBank.Maps.Keys
            .Select(e => e.Filename)
            .ToHashSet();
    }

    [GeneratedRegex(@"^m\d{2}_\d{2}_\d{2}_\d{2}$")]
    private static partial Regex MapNameRegex();
    #endregion

    #region Map
    private void DisplayMap(PartsTokenIdListBinder binder, PartsTokenIdList list)
    {
        GUI.WrappedText($"Map: {SelectedMap}");

        var mapName = AliasHelper.GetMapNameAlias(Project, SelectedMap);
        if (!string.IsNullOrEmpty(mapName))
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, mapName);
        }

        if (!GetProjectMaps().Contains(SelectedMap))
        {
            GUI.WrappedText("No map of the project has this name, so the game doesn't use this list.");
            return;
        }

        var map = View.Selection.GetMapContainerFromMapID(SelectedMap);

        if (map == null)
        {
            GUI.WrappedText("Load the map to compare this list with its enemies and generate it from them.");
            return;
        }

        var ids = PartsTokenIdList.GetMapIDs(map);

        if (ids.SequenceEqual(list.IDs))
        {
            GUI.WrappedText($"This list matches the {ids.Count} enemies of the loaded map that have a PartsTokenParamID.");
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);
            GUI.WrappedText($"This list differs from the loaded map, which has {ids.Count} enemies with a PartsTokenParamID.");
            ImGui.PopStyleColor();
        }

        GUI.MultiButtonInput("partsTokenIdListMapActions",
            "partsTokenIdListGenerate", "Generate from Map",
            "Replace the entries of this list with the PartsTokenParamIDs of the loaded map's enemies, including edits that haven't been saved.",
            () =>
            {
                list.IDs = ids;
                binder.ModifiedLists.Add(SelectedMap);
                binder.Modified = true;
            });
    }

    private bool MatchesMap(PartsTokenIdList list, string mapID)
    {
        var map = View.Selection.GetMapContainerFromMapID(mapID);

        // A map that isn't loaded can't be compared, so it isn't marked
        if (map == null)
            return true;

        return PartsTokenIdList.GetMapIDs(map).SequenceEqual(list.IDs);
    }
    #endregion

    #region Entries
    private void DisplayEntries(PartsTokenIdListBinder binder, PartsTokenIdList list)
    {
        var ids = list.IDs;

        var map = View.Selection.GetMapContainerFromMapID(SelectedMap);
        var enemies = map != null ? PartsTokenIdList.GetTokenEnemies(map) : null;

        var param = GetPartsTokenParam();

        ImGui.Text($"{ids.Count} entries");

        Action pendingEdit = null;

        var tblFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;
        var height = ImGui.GetTextLineHeightWithSpacing() * 20;

        if (ImGui.BeginTable("partsTokenIdListEntryTable", 4, tblFlags, new Vector2(0, height)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Index", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthStretch, 0.3f);
            ImGui.TableSetupColumn("Row Name", ImGuiTableColumnFlags.WidthStretch, 0.35f);
            ImGui.TableSetupColumn("Enemy", ImGuiTableColumnFlags.WidthStretch, 0.35f);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(ids.Count);

            while (clipper.Step())
            {
                for (int index = clipper.DisplayStart; index < clipper.DisplayEnd; index++)
                {
                    ImGui.PushID(index);
                    ImGui.TableNextRow();

                    // Index, with the context menu for editing the entries
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Selectable($"{index}##partsTokenIdListEntry");
                    GUI.Tooltip("Right-click to insert, move or delete entries.");

                    if (ImGui.BeginPopupContextItem("partsTokenIdListEntryContext"))
                    {
                        pendingEdit = DisplayEntryContextMenu(ids, index);
                        ImGui.EndPopup();
                    }

                    // ID
                    ImGui.TableSetColumnIndex(1);
                    var id = ids[index];
                    ImGui.SetNextItemWidth(-1);
                    if (ImGui.InputInt("##partsTokenIdListID", ref id))
                    {
                        ids[index] = id;
                        binder.ModifiedLists.Add(SelectedMap);
                        binder.Modified = true;
                    }

                    // Row Name
                    ImGui.TableSetColumnIndex(2);
                    var row = param?[ids[index]];
                    if (row == null)
                    {
                        ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, param == null ? "-" : "No row");
                        GUI.Tooltip(param == null ? "PartsTokenParam isn't loaded." : "PartsTokenParam has no row with this ID.");
                    }
                    else
                    {
                        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, row.Name ?? "");
                    }

                    // Enemy, from the loaded map: the entry at an index belongs to the enemy at that index
                    ImGui.TableSetColumnIndex(3);
                    if (enemies == null)
                    {
                        ImGui.TextDisabled("-");
                        GUI.Tooltip("Load the map to see the enemy each entry belongs to.");
                    }
                    else if (index >= enemies.Count)
                    {
                        ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, "None");
                        GUI.Tooltip("The loaded map has fewer enemies with a PartsTokenParamID than this list has entries.");
                    }
                    else
                    {
                        var enemy = enemies[index];
                        var enemyID = ((MSB_AC6.Part.EnemyBase)enemy.WrappedObject).PartsTokenParamID;

                        if (enemyID != ids[index])
                            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

                        if (ImGui.Selectable($"{enemy.Name}##partsTokenIdListEnemy"))
                        {
                            View.ViewportSelection.ClearSelection();
                            View.ViewportSelection.AddSelection(enemy);
                            View.FrameAction.ApplyViewportFrame();
                        }

                        if (enemyID != ids[index])
                        {
                            ImGui.PopStyleColor();
                            GUI.Tooltip($"This enemy's PartsTokenParamID is {enemyID}. Click to select it.");
                        }
                        else
                        {
                            GUI.Tooltip("Click to select this enemy.");
                        }
                    }

                    ImGui.PopID();
                }
            }

            clipper.End();

            ImGui.EndTable();
        }

        // Entries are added, moved and removed after the table, so its rows don't shift while it is drawn
        if (pendingEdit != null)
        {
            pendingEdit();
            binder.ModifiedLists.Add(SelectedMap);
            binder.Modified = true;
        }

        GUI.MultiButtonInput("partsTokenIdListAddButtons",
            "partsTokenIdListAddEntry", "Add Entry",
            "Add an entry at the end of the list.",
            () =>
            {
                ids.Add(0);
                binder.ModifiedLists.Add(SelectedMap);
                binder.Modified = true;
            });
    }

    private static Action DisplayEntryContextMenu(List<int> ids, int index)
    {
        Action edit = null;

        if (ImGui.Selectable("Insert Entry Above"))
            edit = () => ids.Insert(index, 0);

        if (ImGui.Selectable("Insert Entry Below"))
            edit = () => ids.Insert(index + 1, 0);

        if (ImGui.Selectable("Duplicate"))
            edit = () => ids.Insert(index + 1, ids[index]);

        ImGui.Separator();

        if (index > 0 && ImGui.Selectable("Move Up"))
            edit = () => (ids[index - 1], ids[index]) = (ids[index], ids[index - 1]);

        if (index < ids.Count - 1 && ImGui.Selectable("Move Down"))
            edit = () => (ids[index + 1], ids[index]) = (ids[index], ids[index + 1]);

        ImGui.Separator();

        if (ImGui.Selectable("Delete"))
            edit = () => ids.RemoveAt(index);

        return edit;
    }

    private Param GetPartsTokenParam()
    {
        var bank = Project.Handler.ParamData?.PrimaryBank;

        if (bank?.Params == null)
            return null;

        return bank.Params.TryGetValue("PartsTokenParam", out var param) ? param : null;
    }
    #endregion

    #region Files
    private PartsTokenIdListBinder GetBinder()
    {
        if (Binder == null && !ReadFailed)
        {
            Binder = ReadBinder(Entry);
            ReadFailed = Binder == null;
        }

        return Binder;
    }

    private PartsTokenIdListBinder ReadBinder(FileDictionaryEntry entry)
    {
        var fs = Project.VFS.FS;

        if (!fs.FileExists(entry.Path))
        {
            Smithbox.LogError(this, LOC.Get("MAP_Parts_Token_ID_List_Failed_Find", entry.Path));
            return null;
        }

        try
        {
            var data = fs.ReadFile(entry.Path).Value;
            var compression = DCX.Type.None;

            if (DCX.Is(data))
                data = DCX.Decompress(data, out compression);

            var bnd = BND4.Read(data);

            var binder = new PartsTokenIdListBinder
            {
                Entry = entry,
                Bnd = bnd,
                Compression = compression
            };

            foreach (var file in bnd.Files)
            {
                binder.Lists[GetMapID(file.Name)] = PartsTokenIdList.Read(file.Bytes.ToArray());
            }

            return binder;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Parts_Token_ID_List_Failed_Read", entry.Path), e);
            return null;
        }
    }

    /// <summary>
    /// The map a file of the binder belongs to: its name, without its folder and extension.
    /// </summary>
    private static string GetMapID(string fileName)
    {
        var name = fileName[(fileName.LastIndexOfAny(['\\', '/']) + 1)..];

        if (name.EndsWith(FileExtension, StringComparison.OrdinalIgnoreCase))
            name = name[..^FileExtension.Length];

        return name;
    }

    private void Revert()
    {
        Binder = null;
        ReadFailed = false;
        NewListStatus = "";
    }

    private void Save()
    {
        var binder = Binder;

        if (binder == null)
            return;

        try
        {
            var bnd = binder.Bnd;

            // New files take the folder and flags of the files the binder has
            var template = bnd.Files.FirstOrDefault();
            var folder = template != null
                ? template.Name[..(template.Name.LastIndexOfAny(['\\', '/']) + 1)]
                : DefaultFileFolder;
            var flags = template?.Flags ?? SoulsFormats.Binder.FileFlags.Flag1;

            var existing = bnd.Files.ToDictionary(e => GetMapID(e.Name), e => e);
            var files = new List<BinderFile>();

            // The files are kept in the order of their names, numbered from 0, as they are in the base game
            foreach (var (mapID, list) in binder.Lists)
            {
                if (!existing.TryGetValue(mapID, out var file))
                    file = new BinderFile(flags, 0, $"{folder}{mapID}{FileExtension}", Array.Empty<byte>());

                file.ID = files.Count;
                file.Bytes = list.Write();
                files.Add(file);
            }

            bnd.Files = files;

            var data = bnd.Write();

            if (binder.Compression != DCX.Type.None)
                data = DCX.Compress(data, binder.Compression);

            Project.VFS.ProjectFS.WriteFile(binder.Entry.Path, data);
            binder.Modified = false;
            binder.ModifiedLists.Clear();

            Smithbox.Log(this, LOC.Get("MAP_Parts_Token_ID_List_Saved", binder.Entry.Path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Parts_Token_ID_List_Failed_Write", binder.Entry.Path), e);
        }
    }

    /// <summary>
    /// Saves the lists if they have unsaved changes, for the map editor's save.
    /// </summary>
    public void SaveModified()
    {
        if (HasModifiedFiles)
            Save();
    }
    #endregion
}

public class PartsTokenIdListBinder
{
    public FileDictionaryEntry Entry;
    public BND4 Bnd;
    public DCX.Type Compression;

    // The lists, by the map they belong to, in the order of their names
    public SortedDictionary<string, PartsTokenIdList> Lists = new(StringComparer.Ordinal);

    // The maps whose lists have unsaved changes
    public HashSet<string> ModifiedLists = new();

    public bool Modified;
}
