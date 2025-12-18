using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;


namespace StudioCore.Editors.FileBrowser;

public class FileListView
{
    public FileBrowserScreen Editor;
    public ProjectEntry Project;

    public FileListView(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.Begin($"Browser List##BrowserList");

        if (Project.FileData.Roots.Count == 0)
        {
            ImGui.Text("No File System roots available. Load a project.");

            return;
        }

        // VFS
        BuildFolderNodes();
        DisplayVFS();

        // File Roots
#if DEBUG
        foreach (var root in Project.FileData.Roots)
        {
            Traverse(root, $"File Browser");
        }
#endif

        ImGui.End();
    }

    /// <summary>
    /// Ignore the bdt files since clicking them crashes Smithbox, and the bucket viewing is done via the bhd.
    /// </summary>
    private List<string> ignoredFiles = new()
    {
        ".bdt"
    };

    private void Traverse(FsEntry e, string parentIdStr)
    {
        var extension = Path.GetExtension(e.Name);
        if (ignoredFiles.Contains(extension))
            return;

        string id = $"{parentIdStr}##{e.Name}";
        var flags = ImGuiTreeNodeFlags.OpenOnDoubleClick;

        if (e is VirtualFileSystemFsEntry)
            flags |= ImGuiTreeNodeFlags.CollapsingHeader;

        if (!e.CanHaveChildren)
            flags |= ImGuiTreeNodeFlags.Leaf;

        if (Editor.Selection.SelectedEntry == e)
            flags |= ImGuiTreeNodeFlags.Selected;

        bool shouldBeOpen = e.CanHaveChildren && (e.IsInitialized || e.IsLoading);

        ImGui.SetNextItemOpen(shouldBeOpen);
        bool isOpen = ImGui.TreeNodeEx(id, flags, e.Name);

        if (ImGui.IsItemClicked())
        {
            if (!e.IsInitialized)
            {
                e.LoadAsync(id, e.Name, Project);
                Select(id, e);
            }
            else if (!e.CanHaveChildren)
            {
                Select(id, e);
            }
            else
            {
                e.Unload();
            }

        }

        if (ImGui.BeginPopupContextItem($"Context##FileContext{e.Name}"))
        {
            //if (ImGui.Selectable("Copy to Project"))
            //{
            //    HandleCopy(e);
            //}

            //if (ImGui.Selectable("Extract"))
            //{
            //    HandleExtraction(e);
            //}

            ImGui.EndPopup();
        }

        if (isOpen)
        {
            //if (!e.IsInitialized && e.CanHaveChildren) e.LoadAsync();
            //IsInitialized may have changed, so re-check
            shouldBeOpen = e.CanHaveChildren && (e.IsInitialized || e.IsLoading);

            //nodes that have CanHaveChildren = false will be set to be leaf nodes, but leaf nodes always
            //return true from ImGui.TreeNode, so we need to double-check shouldBeOpen here so that
            //we don't erroneously display children, such as in the case of a BHD file, since they
            //override CanHaveChildren to be false but still populate the Children list.
            if (shouldBeOpen)
            {
                if (e.IsLoading)
                {
                    ImGui.TreeNodeEx("Loading...", ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.Leaf);
                }
                else
                {
                    foreach (var child in e.Children.Order(fsEntryComparer))
                    {
                        Traverse(child, id);
                    }
                }
            }
            if (!flags.HasFlag(ImGuiTreeNodeFlags.NoTreePushOnOpen)) ImGui.TreePop();
        }
    }

    private void Select(string id, FsEntry e)
    {
        if (Editor.Selection.SelectedEntry == e)
        {
            return;
        }

        if (Editor.Selection.SelectedEntry != null)
        {
            Editor.Selection.SelectedEntry.onUnload = null;
        }

        Editor.Selection.SelectedEntry = e;
        Editor.Selection.SelectedEntryID = id;
        e.onUnload = TryDeselect;
    }

    private void TryDeselect(FsEntry e)
    {
        if (Editor.Selection.SelectedEntry == e)
        {
            Editor.Selection.SelectedEntry = null;
        }
    }

    public IComparer<FsEntry> fsEntryComparer = Comparer<FsEntry>.Create((a, b) =>
    {
        bool aIsVfsDir = a is VirtualFileSystemDirectoryFsEntry;
        bool bIsVfsDir = b is VirtualFileSystemDirectoryFsEntry;

        if (aIsVfsDir && !bIsVfsDir)
            return -1;

        if (bIsVfsDir && !aIsVfsDir)
            return 1;

        return string.Compare(a.Name, b.Name, StringComparison.CurrentCulture);
    });

    private bool BuiltFolderNodes = false;
    private FolderNode _root = new() { Name = "/" };

    private string _search = string.Empty;

    private void DisplayVFS()
    {
        if (ImGui.CollapsingHeader("VFS", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.InputTextWithHint(
                "##FileSearch",
                "Search files...",
                ref _search,
                256
            );

            if (BuiltFolderNodes)
            {
                DrawFolderNode(_root);
            }
        }
    }

    private void DrawFolderNode(FolderNode node)
    {
        foreach (var folder in node.Children)
        {
            bool hasMatch = FolderHasMatch(folder, _search);
            bool searchActive = !string.IsNullOrWhiteSpace(_search);

            // Skip non-matching folders during search
            if (searchActive && !hasMatch &&
                !folder.Name.Contains(_search, StringComparison.OrdinalIgnoreCase))
                continue;

            bool folderSelected = ReferenceEquals(Editor.Selection.SelectedVfsFolder, folder);

            ImGuiTreeNodeFlags flags =
                ImGuiTreeNodeFlags.OpenOnArrow |
                ImGuiTreeNodeFlags.SpanFullWidth;

            if (folderSelected)
                flags |= ImGuiTreeNodeFlags.Selected;

            // Auto-expand when searching
            if (searchActive && hasMatch)
                ImGui.SetNextItemOpen(true, ImGuiCond.Always);

            bool open = ImGui.TreeNodeEx(
                $"{folder.Name}##folder_{folder.GetHashCode()}",
                flags
            );

            if (ImGui.IsItemClicked())
                Editor.Selection.SelectedVfsFolder = folder;

            if (!open)
                continue;

            // Files
            foreach (var file in folder.Files)
            {
                string filename = $"{file.Filename}.{file.Extension}";
                bool fileMatches =
                    string.IsNullOrWhiteSpace(_search) ||
                    filename.Contains(_search, StringComparison.OrdinalIgnoreCase);

                if (!fileMatches)
                    continue;

                bool fileSelected = ReferenceEquals(Editor.Selection.SelectedVfsFile, file);

                ImGuiTreeNodeFlags fileFlags =
                    ImGuiTreeNodeFlags.Leaf |
                    ImGuiTreeNodeFlags.NoTreePushOnOpen |
                    ImGuiTreeNodeFlags.SpanFullWidth;

                if (fileSelected)
                    fileFlags |= ImGuiTreeNodeFlags.Selected;

                // Highlight matching files
                if (searchActive && filename.Contains(_search, StringComparison.OrdinalIgnoreCase))
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1f, 1f, 0.4f, 1f));

                ImGui.TreeNodeEx(
                    $"{filename}##file_{file.GetHashCode()}",
                    fileFlags
                );

                if (ImGui.IsItemClicked())
                {
                    Editor.Selection.UpdateVfsFileSelection(file);
                }

                if (searchActive && filename.Contains(_search, StringComparison.OrdinalIgnoreCase))
                    ImGui.PopStyleColor();
            }

            DrawFolderNode(folder);
            ImGui.TreePop();
        }
    }

    private bool FolderHasMatch(FolderNode node, string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return false;

        search = search.ToLowerInvariant();

        // Check files in this folder
        foreach (var file in node.Files)
        {
            if ($"{file.Filename}.{file.Extension}"
                .ToLowerInvariant()
                .Contains(search))
                return true;
        }

        // Check subfolders recursively
        foreach (var child in node.Children)
        {
            if (child.Name.ToLowerInvariant().Contains(search))
                return true;

            if (FolderHasMatch(child, search))
                return true;
        }

        return false;
    }

    private void BuildFolderNodes()
    {
        var dict = Project.FileDictionary.Entries;

        if (!BuiltFolderNodes)
        {
            _root = new FolderNode { Name = "/" };

            foreach (var entry in dict)
            {
                var parts = entry.Folder
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);

                FolderNode current = _root;

                foreach (var part in parts)
                {
                    var child = current.Children
                        .FirstOrDefault(c => c.Name == part);

                    if (child == null)
                    {
                        child = new FolderNode { Name = part };
                        current.Children.Add(child);
                    }

                    current = child;
                }

                current.Files.Add(entry);
            }

            SortTree(_root);

            BuiltFolderNodes = true;
        }
    }

    private void SortTree(FolderNode node)
    {
        node.Children.Sort((a, b) =>
            string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));

        node.Files.Sort((a, b) =>
            string.Compare(
                $"{a.Filename}.{a.Extension}",
                $"{b.Filename}.{b.Extension}",
                StringComparison.OrdinalIgnoreCase));

        foreach (var child in node.Children)
            SortTree(child);
    }
}

public class FolderNode
{
    public string Name;
    public List<FolderNode> Children = new();
    public List<FileDictionaryEntry> Files = new();
}