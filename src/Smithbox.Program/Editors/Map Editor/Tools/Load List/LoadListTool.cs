using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Edits the LOADLIST files of the project: the worldloadlistlist files that list the MSBs of the world,
/// and ingamestay.loadlist, which lists the files kept loaded throughout play.
/// Edits are kept until they are saved, with the tool's Save button or with the map editor's save.
/// </summary>
public class LoadListTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    // The files opened so far, by their path
    private readonly Dictionary<string, LoadListFile> Files = new();

    private string SelectedPath;

    private bool HideBlankLines = true;
    private string Filter = "";

    private bool RawTextActive = false;
    private string RawText = "";

    public LoadListTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private IEnumerable<FileDictionaryEntry> Entries =>
        Project.Locator.LoadListFiles?.Entries?.OrderBy(e => e.Path) ?? Enumerable.Empty<FileDictionaryEntry>();

    public bool HasModifiedFiles => Files.Values.Any(f => f.Modified);

    public void OnToolWindow()
    {
        ImGui.BeginChild("loadListSection", ImGuiChildFlags.Borders);

        var entries = Entries.ToList();

        if (entries.Count == 0)
        {
            GUI.WrappedText("This project has no load lists.");
            ImGui.EndChild();
            return;
        }

        GUI.WrappedText("Edit the worldloadlistlist files, which list the MSBs of the world, and ingamestay, which lists the files kept loaded throughout play.");
        GUI.WrappedText("Each line loads a file written as device:/path. Everything from a '#' is a comment, so an entry is disabled by commenting it out.");

        GUI.Spacer();
        GUI.SimpleHeader("File", "");

        if (SelectedPath == null || !entries.Any(e => e.Path == SelectedPath))
            SelectedPath = entries[0].Path;

        DisplayFileSelection(entries);

        var file = GetFile(entries.First(e => e.Path == SelectedPath));

        if (file == null)
        {
            GUI.WrappedText("This file couldn't be read. See the log for details.");
            ImGui.EndChild();
            return;
        }

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("loadListActions",
            "loadListSaveButton", "Save",
            "Write this file to the project.",
            () => Save(file),
            "loadListRevertButton", "Revert",
            "Discard the unsaved changes to this file.",
            () => Revert(file));

        GUI.Spacer();

        if (ImGui.BeginTabBar("loadListTabs"))
        {
            if (ImGui.BeginTabItem("Entries##loadListEntriesTab"))
            {
                RawTextActive = false;
                DisplayEntries(file);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Raw Text##loadListRawTextTab"))
            {
                DisplayRawText(file);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.EndChild();
    }

    private void DisplayFileSelection(List<FileDictionaryEntry> entries)
    {
        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##loadListFileCombo", GetFileLabel(SelectedPath)))
        {
            foreach (var entry in entries)
            {
                if (ImGui.Selectable($"{GetFileLabel(entry.Path)}##loadListFile_{entry.Path}", entry.Path == SelectedPath))
                {
                    SelectedPath = entry.Path;
                    RawTextActive = false;
                }
                GUI.Tooltip(entry.Path);
            }

            ImGui.EndCombo();
        }
    }

    private string GetFileLabel(string path)
    {
        var modified = Files.TryGetValue(path, out var file) && file.Modified;

        return modified ? $"{path} *" : path;
    }

    #region Entries
    private void DisplayEntries(LoadListFile file)
    {
        var lines = file.List.Lines;

        ImGui.Checkbox("Hide Blank Lines##loadListHideBlank", ref HideBlankLines);
        GUI.Tooltip("Hide the empty lines that space out the file. They are still written when it is saved.");

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##loadListFilter", "Filter...", ref Filter, 255);

        var visible = new List<int>();

        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (HideBlankLines && line.IsBlank)
                continue;

            if (Filter != "" && !line.ToText().Contains(Filter, StringComparison.OrdinalIgnoreCase))
                continue;

            visible.Add(i);
        }

        Action pendingEdit = null;

        var tblFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;
        var height = ImGui.GetTextLineHeightWithSpacing() * 20;

        if (ImGui.BeginTable("loadListEntryTable", 4, tblFlags, new Vector2(0, height)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Line", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##enabled", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Path", ImGuiTableColumnFlags.WidthStretch, 0.6f);
            ImGui.TableSetupColumn("Comment", ImGuiTableColumnFlags.WidthStretch, 0.4f);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(visible.Count);

            while (clipper.Step())
            {
                for (int v = clipper.DisplayStart; v < clipper.DisplayEnd; v++)
                {
                    var index = visible[v];
                    var line = lines[index];

                    ImGui.PushID(index);
                    ImGui.TableNextRow();

                    // Line number, with the context menu for editing the lines
                    ImGui.TableSetColumnIndex(0);
                    ImGui.Selectable($"{index + 1}##loadListLine");
                    GUI.Tooltip("Right-click to insert, move or delete lines.");

                    if (ImGui.BeginPopupContextItem("loadListLineContext"))
                    {
                        pendingEdit = DisplayLineContextMenu(file, index);
                        ImGui.EndPopup();
                    }

                    // Enabled
                    ImGui.TableSetColumnIndex(1);
                    if (line.IsEntry)
                    {
                        var enabled = line.IsEnabledEntry;
                        if (ImGui.Checkbox("##loadListEnabled", ref enabled))
                        {
                            line.SetEnabled(enabled);
                            file.Modified = true;
                        }
                        GUI.Tooltip(enabled ? "Loaded. Uncheck to comment the entry out." : "Commented out. Check to load it again.");
                    }

                    // Path
                    ImGui.TableSetColumnIndex(2);
                    if (line.IsDisabledEntry)
                    {
                        // Shown as it is written; enabling it makes the path editable
                        ImGui.TextDisabled(line.Comment);
                    }
                    else if (line.IsEnabledEntry || line.IsBlank)
                    {
                        var path = line.Path;
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.InputTextWithHint("##loadListPath", "device:/path", ref path, 512))
                        {
                            // A '#' would start a comment, so it can't be part of the path
                            line.Path = path.Replace("#", "").Trim();
                            if (line.Path != "" && line.Comment != "" && line.Separator == "")
                                line.Separator = " ";
                            file.Modified = true;
                        }
                    }

                    // Comment
                    ImGui.TableSetColumnIndex(3);
                    if (line.IsEnabledEntry || line.IsBlank)
                    {
                        var comment = line.Comment;
                        ImGui.SetNextItemWidth(-1);
                        if (ImGui.InputTextWithHint("##loadListComment", "#comment", ref comment, 512))
                        {
                            line.SetComment(comment);
                            file.Modified = true;
                        }
                    }
                    else if (!line.IsDisabledEntry)
                    {
                        var comment = line.Comment;
                        ImGui.SetNextItemWidth(-1);
                        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
                        var changed = ImGui.InputText("##loadListCommentLine", ref comment, 512);
                        ImGui.PopStyleColor();
                        if (changed)
                        {
                            line.SetComment(comment);
                            file.Modified = true;
                        }
                        GUI.Tooltip("The whole line is a comment.");
                    }

                    ImGui.PopID();
                }
            }

            clipper.End();

            ImGui.EndTable();
        }

        // Lines are added, moved and removed after the table, so its rows don't shift while it is drawn
        if (pendingEdit != null)
        {
            pendingEdit();
            file.Modified = true;
        }

        GUI.MultiButtonInput("loadListAddButtons",
            "loadListAddEntry", "Add Entry",
            "Add an entry at the end of the file.",
            () => { AddLine(file, new LoadListLine(DefaultPath(file))); },
            "loadListAddComment", "Add Comment",
            "Add a comment line at the end of the file.",
            () => { AddLine(file, new LoadListLine("", "#")); });
    }

    private Action DisplayLineContextMenu(LoadListFile file, int index)
    {
        var lines = file.List.Lines;
        Action edit = null;

        if (ImGui.Selectable("Insert Entry Above"))
            edit = () => lines.Insert(index, new LoadListLine(DefaultPath(file)));

        if (ImGui.Selectable("Insert Entry Below"))
            edit = () => lines.Insert(index + 1, new LoadListLine(DefaultPath(file)));

        if (ImGui.Selectable("Insert Comment Below"))
            edit = () => lines.Insert(index + 1, new LoadListLine("", "#"));

        if (ImGui.Selectable("Insert Blank Line Below"))
            edit = () => lines.Insert(index + 1, new LoadListLine());

        ImGui.Separator();

        if (ImGui.Selectable("Duplicate"))
            edit = () => lines.Insert(index + 1, LoadListLine.Parse(lines[index].ToText()));

        if (index > 0 && ImGui.Selectable("Move Up"))
            edit = () => (lines[index - 1], lines[index]) = (lines[index], lines[index - 1]);

        if (index < lines.Count - 1 && ImGui.Selectable("Move Down"))
            edit = () => (lines[index + 1], lines[index]) = (lines[index], lines[index + 1]);

        ImGui.Separator();

        if (ImGui.Selectable("Delete"))
            edit = () => lines.RemoveAt(index);

        return edit;
    }

    private void AddLine(LoadListFile file, LoadListLine line)
    {
        file.List.Lines.Add(line);
        file.Modified = true;
    }

    /// <summary>
    /// The start of the path new entries are given: the device the file's entries use most.
    /// </summary>
    private string DefaultPath(LoadListFile file)
    {
        var device = file.List.Lines
            .Where(l => l.IsEnabledEntry && l.Path.Contains(":/"))
            .GroupBy(l => l.Path[..(l.Path.IndexOf(":/") + 2)])
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        if (device == "map:/")
            return "map:/MapStudio/";

        return device ?? "other:/";
    }
    #endregion

    #region Raw Text
    private void DisplayRawText(LoadListFile file)
    {
        // The text box edits the text of the file, taken from it when the tab is opened
        if (!RawTextActive)
        {
            RawText = file.List.ToText().Replace(file.List.NewLine, "\n");
            RawTextActive = true;
        }

        GUI.WrappedText("Edit the file as text. Changes apply to the Entries tab as they are typed.");

        var maxLength = (uint)(Encoding.UTF8.GetByteCount(RawText) + 65536);

        if (ImGui.InputTextMultiline("##loadListRawText", ref RawText, maxLength, new Vector2(-1, ImGui.GetTextLineHeightWithSpacing() * 24)))
        {
            var list = LoadList.Parse(RawText);
            list.CopyFormat(file.List);

            file.List = list;
            file.Modified = true;
        }
    }
    #endregion

    #region Files
    private LoadListFile GetFile(FileDictionaryEntry entry)
    {
        if (Files.TryGetValue(entry.Path, out var file))
            return file;

        file = ReadFile(entry);
        Files[entry.Path] = file;

        return file;
    }

    private LoadListFile ReadFile(FileDictionaryEntry entry)
    {
        var fs = Project.VFS.FS;

        if (!fs.FileExists(entry.Path))
        {
            Smithbox.LogError(this, LOC.Get("MAP_Load_List_Failed_Find", entry.Path));
            return null;
        }

        try
        {
            var data = fs.ReadFile(entry.Path).Value;
            var compression = DCX.Type.None;

            if (DCX.Is(data))
                data = DCX.Decompress(data, out compression);

            return new LoadListFile
            {
                Entry = entry,
                List = LoadList.Read(data.ToArray()),
                Compression = compression
            };
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Load_List_Failed_Read", entry.Path), e);
            return null;
        }
    }

    private void Revert(LoadListFile file)
    {
        Files.Remove(file.Entry.Path);
        RawTextActive = false;
    }

    private void Save(LoadListFile file)
    {
        try
        {
            var data = file.List.Write();

            if (file.Compression != DCX.Type.None)
                data = DCX.Compress(data, file.Compression);

            Project.VFS.ProjectFS.WriteFile(file.Entry.Path, data);
            file.Modified = false;

            Smithbox.Log(this, LOC.Get("MAP_Load_List_Saved", file.Entry.Path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Load_List_Failed_Write", file.Entry.Path), e);
        }
    }

    /// <summary>
    /// Saves the files with unsaved changes, for the map editor's save.
    /// </summary>
    public void SaveModified()
    {
        foreach (var file in Files.Values.Where(f => f.Modified).ToList())
        {
            Save(file);
        }
    }
    #endregion
}

public class LoadListFile
{
    public FileDictionaryEntry Entry;
    public LoadList List;
    public DCX.Type Compression;
    public bool Modified;
}
