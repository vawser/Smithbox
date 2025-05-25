using Hexa.NET.ImGui;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.FileBrowserNS;

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

        foreach (var root in Project.FileData.Roots)
        {
            Traverse(root, $"File Browser");
        }

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

    public void HandleExtraction(FsEntry entry)
    {

    }
    public void HandleCopy(FsEntry entry)
    {
        
    }
}
