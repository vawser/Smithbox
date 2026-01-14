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

        BuildFolderNodes();

        ImGui.BeginTabBar("sourceTabs");

        if (ImGui.BeginTabItem($"VFS"))
        {
            DisplayVFS();

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();

        ImGui.End();
    }

    private bool BuiltFolderNodes = false;
    private FolderNode _root = new() { Name = "/" };

    private string _search = string.Empty;

    private void DisplayVFS()
    {
        ImGui.InputText("Search##FileSearch",  ref _search, 256);

        if (BuiltFolderNodes)
        {
            ImGui.BeginChild($"vfsList");

            DrawFolderNode(_root);

            ImGui.EndChild();
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
        var dict = Project.Locator.FileDictionary.Entries;

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