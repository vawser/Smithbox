using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


namespace StudioCore.Editors.FileBrowser;

public class FileListView
{
    public FileEditorView Parent;
    public ProjectEntry Project;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;

    // Only show files the project has (overrides and project-only files)
    private bool ProjectFilesOnly = false;

    public FileListView(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        GUI.TitleHeader(
            LOC.Get("FILE_FileList_Header"),
            LOC.Get("FILE_FileList_Header_TT"));

        DisplayVFS();
    }

    // The tree version this view last drew; a new tree invalidates the search cache and the folder selection
    private int SeenTreeVersion = -1;

    private bool TextFilterActive => !string.IsNullOrWhiteSpace(FileListFilter);
    private bool FilterActive => TextFilterActive || ProjectFilesOnly;

    private void DisplayVFS()
    {
        EditorFilters.DisplayFramedListFilter("fileBrowser_FileList",
            ref FileListFilter, ref ExactFileListFilter);

        ImGui.Checkbox($"{LOC.Get("FILE_FileList_Project_Files_Only")}##projectFilesOnly", ref ProjectFilesOnly);
        GUI.Tooltip(LOC.Get("FILE_FileList_Project_Files_Only_TT"));

        ImGui.SameLine();
        ImGui.TextColored(UI.Current.ImGui_PrimaryChanged_Text, LOC.Get("FILE_FileList_Legend_Override"));
        GUI.Tooltip(LOC.Get("FILE_FileList_Legend_Override_TT"));

        ImGui.SameLine();
        ImGui.TextColored(UI.Current.ImGui_AuxAdded_Text, LOC.Get("FILE_FileList_Legend_Project_Only"));
        GUI.Tooltip(LOC.Get("FILE_FileList_Legend_Project_Only_TT"));

        var tree = Parent.Editor.Tree;
        var root = tree.Root;

        if (root == null)
        {
            ImGui.Text(LOC.Get("FILE_FileList_Building"));
            return;
        }

        if (SeenTreeVersion != tree.Version)
        {
            SeenTreeVersion = tree.Version;
            FolderMatchCache.Clear();

            // Point the selection at the same folder in the new tree, so Export Folder uses its current files
            var oldFolder = Parent.Selection.SelectedVfsFolder;
            if (oldFolder != null)
                Parent.Selection.SelectedVfsFolder = tree.FindFolder(oldFolder.Path);
        }

        DisplayBookmarks();
        DisplayBreadcrumb();

        ImGui.BeginChild($"vfsList", ImGuiChildFlags.Borders);

        DrawFolderNode(root);

        // Files that sit directly in the data root (e.g. ER's /aeg*.bnk sound banks)
        DrawFiles(root);

        ImGui.EndChild();

        // A reveal that wasn't reached (hidden by the filter, say) gives up after a few frames
        if (Parent.Selection.RevealFramesLeft > 0 && --Parent.Selection.RevealFramesLeft == 0)
            Parent.Selection.RevealPath = null;
    }

    /// <summary>
    /// Pinned files and folders. Click to select and reveal, right-click to remove.
    /// </summary>
    private void DisplayBookmarks()
    {
        var bookmarks = Parent.Editor.Bookmarks;

        if (bookmarks.Entries.Count == 0)
            return;

        if (!ImGui.CollapsingHeader($"{LOC.Get("FILE_Bookmarks_Header", bookmarks.Entries.Count)}##fileBookmarks"))
            return;

        string toRemove = null;

        foreach (var key in bookmarks.Entries)
        {
            var isFolder = key.EndsWith('/');

            if (ImGui.Selectable($"{(isFolder ? Icons.Folder : Icons.File)} {key}##bookmark_{key}"))
                OpenBookmark(key);

            if (ImGui.BeginPopupContextItem($"bookmarkContext_{key}"))
            {
                if (ImGui.MenuItem($"{LOC.Get("FILE_Bookmarks_Remove")}##removeBookmark"))
                    toRemove = key;

                ImGui.EndPopup();
            }
        }

        if (toRemove != null)
            bookmarks.Remove(toRemove);
    }

    private void OpenBookmark(string key)
    {
        var tree = Parent.Editor.Tree;

        if (key.EndsWith('/'))
        {
            var folder = tree.FindFolder(key.TrimEnd('/'));
            if (folder == null)
            {
                Smithbox.Log(this, LOC.Get("FILE_Bookmarks_Missing", key));
                return;
            }

            Parent.Selection.SelectedVfsFolder = folder;
            Parent.Selection.Reveal(folder.Path);
            return;
        }

        var file = tree.FindFile(key);
        if (file == null)
        {
            Smithbox.Log(this, LOC.Get("FILE_Bookmarks_Missing", key));
            return;
        }

        Parent.Selection.SelectAndReveal(file);
    }

    /// <summary>
    /// The selected file's path as clickable folders: a click selects that folder and scrolls to it
    /// </summary>
    private void DisplayBreadcrumb()
    {
        var file = Parent.Selection.SelectedVfsFile;
        if (file == null)
            return;

        var parts = (file.Folder ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (ImGui.SmallButton($"{Icons.Crosshairs}##revealSelected"))
            Parent.Selection.Reveal(file.Path);
        GUI.Tooltip(LOC.Get("FILE_FileList_Reveal_TT"));

        ImGui.SameLine();

        if (ImGui.SmallButton("/##crumbRoot"))
            Parent.Selection.SelectedVfsFolder = Parent.Editor.Tree.Root;

        var path = "";

        for (var i = 0; i < parts.Length; i++)
        {
            path += "/" + parts[i];

            ImGui.SameLine(0, 2);
            ImGui.TextDisabled(">");
            ImGui.SameLine(0, 2);

            if (ImGui.SmallButton($"{parts[i]}##crumb_{i}"))
            {
                var folder = Parent.Editor.Tree.FindFolder(path);
                if (folder != null)
                {
                    Parent.Selection.SelectedVfsFolder = folder;
                    Parent.Selection.Reveal(folder.Path);
                }
            }
        }

        ImGui.SameLine(0, 2);
        ImGui.TextDisabled(">");
        ImGui.SameLine(0, 2);
        ImGui.Text($"{file.Filename}.{file.Extension}");
    }

    /// <summary>
    /// Whether a folder lies on the way to the path being revealed (so it should be opened)
    /// </summary>
    private bool IsOnRevealPath(FolderNode folder)
    {
        var reveal = Parent.Selection.RevealPath;

        return reveal != null && reveal.StartsWith(folder.Path + "/", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsRevealTarget(string path)
    {
        return Parent.Selection.RevealPath != null &&
               string.Equals(Parent.Selection.RevealPath, path, StringComparison.OrdinalIgnoreCase);
    }

    private void DrawFolderNode(FolderNode node)
    {
        foreach (var folder in node.Children)
        {
            bool hasMatch = FilterActive && FolderHasMatch(folder);

            // Skip non-matching folders while filtering. A folder whose own name matches stays, unless only
            // project files are shown (a name says nothing about where its files come from).
            if (FilterActive && !hasMatch && !FolderNameMatches(folder))
                continue;

            bool folderSelected = ReferenceEquals(Parent.Selection.SelectedVfsFolder, folder);

            ImGuiTreeNodeFlags flags =
                ImGuiTreeNodeFlags.OpenOnArrow |
                ImGuiTreeNodeFlags.SpanFullWidth;

            if (folderSelected)
                flags |= ImGuiTreeNodeFlags.Selected;

            // Auto-expand when filtering, and on the way to a revealed path
            if (FilterActive && hasMatch)
                ImGui.SetNextItemOpen(true, ImGuiCond.Always);
            else if (IsOnRevealPath(folder))
                ImGui.SetNextItemOpen(true);

            bool open = ImGui.TreeNodeEx(
                $"{folder.Name}##folder_{folder.GetHashCode()}",
                flags
            );

            if (IsRevealTarget(folder.Path))
            {
                ImGui.SetScrollHereY(0.5f);
                Parent.Selection.RevealPath = null;
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left) || ImGui.IsItemClicked(ImGuiMouseButton.Right))
                Parent.Selection.SelectedVfsFolder = folder;

            FolderContextMenu(folder);

            if (!open)
                continue;

            DrawFiles(folder);

            DrawFolderNode(folder);
            ImGui.TreePop();
        }
    }

    private void DrawFiles(FolderNode folder)
    {
        var tree = Parent.Editor.Tree;
        var selection = Parent.Selection;

        // The rows this folder shows, in order, so Shift-click can select the range between two of them
        var visible = new List<(FileDictionaryEntry File, FileSource Source)>();
        foreach (var file in folder.Files)
        {
            var source = tree.GetSource(file);

            if (FilePasses(file, source))
                visible.Add((file, source));
        }

        for (var index = 0; index < visible.Count; index++)
        {
            var (file, source) = visible[index];

            string filename = $"{file.Filename}.{file.Extension}";

            // By path, so the selection survives a tree rebuild (the entries are new objects then)
            bool fileSelected = selection.SelectedPaths.Contains(file.Path);

            ImGuiTreeNodeFlags fileFlags =
                ImGuiTreeNodeFlags.Leaf |
                ImGuiTreeNodeFlags.NoTreePushOnOpen |
                ImGuiTreeNodeFlags.SpanFullWidth;

            if (fileSelected)
                fileFlags |= ImGuiTreeNodeFlags.Selected;

            // Colour by source: the project's own files stand out from the game's
            var colored = source is not FileSource.Vanilla;

            if (colored)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, source is FileSource.ProjectOverride
                    ? UI.Current.ImGui_PrimaryChanged_Text
                    : UI.Current.ImGui_AuxAdded_Text);
            }

            ImGui.TreeNodeEx(
                $"{filename}##file_{file.GetHashCode()}",
                fileFlags
            );

            if (colored)
                ImGui.PopStyleColor();

            if (IsRevealTarget(file.Path))
            {
                ImGui.SetScrollHereY(0.5f);
                selection.RevealPath = null;
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                var io = ImGui.GetIO();

                if (io.KeyShift && selection.SelectionAnchor != null)
                {
                    // Range from the anchor to here, when the anchor is in this folder's rows
                    var anchorIndex = visible.FindIndex(v =>
                        string.Equals(v.File.Path, selection.SelectionAnchor, StringComparison.OrdinalIgnoreCase));

                    if (anchorIndex >= 0)
                    {
                        if (!io.KeyCtrl)
                            selection.SelectedPaths.Clear();

                        for (var i = Math.Min(anchorIndex, index); i <= Math.Max(anchorIndex, index); i++)
                            selection.SelectedPaths.Add(visible[i].File.Path);

                        selection.UpdateVfsFileSelection(file);
                    }
                    else
                    {
                        SelectSingle(file);
                    }
                }
                else if (io.KeyCtrl)
                {
                    // Toggle this file; the Item Viewer follows the last file added
                    if (selection.SelectedPaths.Remove(file.Path))
                    {
                        if (selection.SelectedVfsFile != null &&
                            string.Equals(selection.SelectedVfsFile.Path, file.Path, StringComparison.OrdinalIgnoreCase) &&
                            selection.SelectedPaths.Count > 0)
                        {
                            var next = tree.FindFile(selection.SelectedPaths.First());
                            if (next != null)
                                selection.UpdateVfsFileSelection(next);
                        }
                    }
                    else
                    {
                        selection.SelectedPaths.Add(file.Path);
                        selection.SelectionAnchor = file.Path;
                        selection.UpdateVfsFileSelection(file);
                    }
                }
                else
                {
                    SelectSingle(file);
                }
            }
            // Right-click on an unselected row selects it alone, so the context menu acts on the row it was opened on
            else if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && !fileSelected)
            {
                SelectSingle(file);
            }

            FileContextMenu(file, source);
        }
    }

    private void SelectSingle(FileDictionaryEntry file)
    {
        var selection = Parent.Selection;

        selection.SelectedPaths.Clear();
        selection.SelectedPaths.Add(file.Path);
        selection.SelectionAnchor = file.Path;
        selection.UpdateVfsFileSelection(file);
    }

    private void FileContextMenu(FileDictionaryEntry file, FileSource source)
    {
        if (!ImGui.BeginPopupContextItem($"fileContext_{file.GetHashCode()}"))
            return;

        var actions = Parent.Editor.Actions;
        var filename = $"{file.Filename}.{file.Extension}";

        // Open in Editor
        var command = actions.GetOpenCommand(file, "", out var editorName);
        if (command != null)
        {
            if (ImGui.MenuItem($"{LOC.Get("FILE_Actions_Open_In_Editor", editorName)}##openInEditor"))
            {
                actions.OpenInEditor(file, Parent.Selection.SelectedInternalTexFile);
            }
            GUI.Tooltip(LOC.Get("FILE_Actions_Open_In_Editor_TT"));

            ImGui.Separator();
        }

        if (ImGui.MenuItem($"{LOC.Get("FILE_Actions_Copy_Path")}##copyPath"))
        {
            ImGui.SetClipboardText(file.Path);
        }

        if (ImGui.MenuItem($"{LOC.Get("FILE_Actions_Copy_Name")}##copyName"))
        {
            ImGui.SetClipboardText(filename);
        }

        var bookmarks = Parent.Editor.Bookmarks;
        var isBookmarked = bookmarks.Contains(file.Path);

        if (ImGui.MenuItem($"{LOC.Get(isBookmarked ? "FILE_Bookmarks_Remove" : "FILE_Bookmarks_Add")}##toggleBookmark"))
        {
            bookmarks.Toggle(file.Path);
        }

        ImGui.Separator();

        // Copy to Project: only offered for game files the project doesn't have yet
        if (ImGui.MenuItem($"{LOC.Get("FILE_Actions_Copy_To_Project")}##copyToProject", "", false, source is FileSource.Vanilla))
        {
            actions.CopyToProject(file);
        }
        GUI.Tooltip(LOC.Get("FILE_Actions_Copy_To_Project_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FILE_Actions_Export")}##exportFile"))
        {
            Parent.ToolView.ExtractTool.ExportContainerFile();
        }
        GUI.Tooltip(LOC.Get("FILE_Actions_Export_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FILE_Actions_Show_In_Explorer")}##showInExplorer", "", false, actions.ExistsOnDisk(file.Path, false)))
        {
            actions.ShowInExplorer(file.Path, false);
        }
        GUI.Tooltip(LOC.Get("FILE_Actions_Show_In_Explorer_TT"));

        ImGui.EndPopup();
    }

    private void FolderContextMenu(FolderNode folder)
    {
        if (!ImGui.BeginPopupContextItem($"folderContext_{folder.GetHashCode()}"))
            return;

        var actions = Parent.Editor.Actions;

        if (ImGui.MenuItem($"{LOC.Get("FILE_Actions_Copy_Path")}##copyFolderPath"))
        {
            ImGui.SetClipboardText(folder.Path);
        }

        var bookmarks = Parent.Editor.Bookmarks;
        var bookmarkKey = FileBookmarks.FolderKey(folder.Path);

        if (ImGui.MenuItem($"{LOC.Get(bookmarks.Contains(bookmarkKey) ? "FILE_Bookmarks_Remove" : "FILE_Bookmarks_Add")}##toggleFolderBookmark"))
        {
            bookmarks.Toggle(bookmarkKey);
        }

        ImGui.Separator();

        if (ImGui.MenuItem($"{LOC.Get("FILE_Actions_Export_Folder")}##exportFolder"))
        {
            Parent.Selection.SelectedVfsFolder = folder;
            Parent.ToolView.ExtractTool.ExportSelectedFolder();
        }
        GUI.Tooltip(LOC.Get("FILE_FileExporter_Export_Folder_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FILE_Actions_Show_In_Explorer")}##showFolderInExplorer", "", false, actions.ExistsOnDisk(folder.Path, true)))
        {
            actions.ShowInExplorer(folder.Path, true);
        }
        GUI.Tooltip(LOC.Get("FILE_Actions_Show_In_Explorer_TT"));

        ImGui.EndPopup();
    }

    private enum SearchMode
    {
        Name,
        Extension,
        Path
    }

    /// <summary>
    /// "ext:flver,tpf" matches by extension (DCX ignored), "path:/chr/c2" by full path; anything else by name with
    /// the usual filter rules
    /// </summary>
    private string ParsedFilterText = null;
    private (SearchMode Mode, string[] Terms) ParsedFilter;

    private (SearchMode Mode, string[] Terms) ParseFilter()
    {
        // Called per row, so parsed once per filter change
        if (ParsedFilterText == FileListFilter)
            return ParsedFilter;

        ParsedFilterText = FileListFilter;
        ParsedFilter = ParseFilterText();
        return ParsedFilter;
    }

    private (SearchMode Mode, string[] Terms) ParseFilterText()
    {
        var filter = FileListFilter.Trim();

        static string[] Split(string value) => value
            .Split([',', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (filter.StartsWith("ext:", StringComparison.OrdinalIgnoreCase))
            return (SearchMode.Extension, Split(filter[4..]).Select(t => t.TrimStart('.')).ToArray());

        if (filter.StartsWith("path:", StringComparison.OrdinalIgnoreCase))
            return (SearchMode.Path, Split(filter[5..]));

        return (SearchMode.Name, Array.Empty<string>());
    }

    private bool FilePasses(FileDictionaryEntry file, FileSource source)
    {
        if (ProjectFilesOnly && source is FileSource.Vanilla)
            return false;

        var (mode, terms) = ParseFilter();

        switch (mode)
        {
            case SearchMode.Extension:
                return terms.Length == 0 || terms.Any(t => string.Equals(file.Extension, t, StringComparison.OrdinalIgnoreCase));

            case SearchMode.Path:
                return terms.Length == 0 || terms.Any(t => file.Path.Contains(t, StringComparison.OrdinalIgnoreCase));
        }

        return EditorFilters.IsMatch(FileListFilter, $"{file.Filename}.{file.Extension}", ExactFileListFilter);
    }

    private bool FolderNameMatches(FolderNode folder)
    {
        if (!TextFilterActive || ProjectFilesOnly)
            return false;

        var (mode, terms) = ParseFilter();

        // An extension says nothing about a folder; a path search shows folders whose path contains it
        return mode switch
        {
            SearchMode.Extension => false,
            SearchMode.Path => terms.Any(t => folder.Path.Contains(t, StringComparison.OrdinalIgnoreCase)),
            _ => EditorFilters.IsMatch(FileListFilter, folder.Name, ExactFileListFilter)
        };
    }

    // Per-folder filter results, so each folder is checked once per filter change instead of every frame
    private string MatchCacheFilter = null;
    private bool MatchCacheExact = false;
    private bool MatchCacheProjectOnly = false;
    private readonly Dictionary<FolderNode, bool> FolderMatchCache = new();

    /// <summary>
    /// Whether any file below this folder passes the filter, or any subfolder's name matches it. Uses the same
    /// EditorFilters rules as the file rows, so OR terms and Exact behave the same for folders and files.
    /// </summary>
    private bool FolderHasMatch(FolderNode node)
    {
        if (MatchCacheFilter != FileListFilter || MatchCacheExact != ExactFileListFilter ||
            MatchCacheProjectOnly != ProjectFilesOnly)
        {
            FolderMatchCache.Clear();
            MatchCacheFilter = FileListFilter;
            MatchCacheExact = ExactFileListFilter;
            MatchCacheProjectOnly = ProjectFilesOnly;
        }

        if (FolderMatchCache.TryGetValue(node, out var cached))
            return cached;

        var tree = Parent.Editor.Tree;
        var result = false;

        foreach (var file in node.Files)
        {
            if (FilePasses(file, tree.GetSource(file)))
            {
                result = true;
                break;
            }
        }

        if (!result)
        {
            foreach (var child in node.Children)
            {
                if (FolderNameMatches(child) || FolderHasMatch(child))
                {
                    result = true;
                    break;
                }
            }
        }

        FolderMatchCache[node] = result;

        return result;
    }
}
