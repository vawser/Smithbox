using Andre.IO.VFS;
using Hexa.NET.ImGui;
using Smithbox.Core.FileBrowserNS.Entries;
using StudioCore;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Smithbox.Core.FileBrowserNS;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
public class FileBrowser : IEditor
{
    private Project Project;

    public int ID = 0;

    private List<VirtualFileSystemFsEntry> roots = [];

    public FsEntry? selected = null;
    public string selectedId = "";

    public bool Initialized = false;

    public FileBrowser(int id, Project projectOwner)
    {
        Project = projectOwner;
        ID = id;

        Initialize();
    }
    public async void Initialize()
    {
        if (Initialized)
            return;

        // VFS Roots
        Task<bool> vfsRootsTask = LoadVfsRoots();
        bool vfsRootsLoaded = await vfsRootsTask;

        if (vfsRootsLoaded)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:File Browser] Loaded VFS roots.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:File Browser] Failed to load VFS roots.");
        }

        Initialized = true;
    }

    private async Task<bool> LoadVfsRoots()
    {
        await Task.Delay(1000);

        roots.Clear();
        bool anyFs = false;
        bool vanillaFs = false;

        if (Project.VanillaRealFS is not EmptyVirtualFileSystem)
        {
            roots.Add(new VirtualFileSystemFsEntry(Project, Project.VanillaRealFS, "Game Directory"));
            anyFs = true;
            vanillaFs = true;
        }

        if (Project.VanillaBinderFS is not EmptyVirtualFileSystem)
        {
            roots.Add(new VirtualFileSystemFsEntry(Project, Project.VanillaBinderFS, "Vanilla Dvdbnds"));
            anyFs = true;
            vanillaFs = true;
        }

        if (vanillaFs && Project.VanillaFS is not EmptyVirtualFileSystem)
        {
            roots.Add(new VirtualFileSystemFsEntry(Project, Project.VanillaFS, "Full Vanilla FS"));
        }

        if (Project.ProjectFS is not EmptyVirtualFileSystem)
        {
            roots.Add(new VirtualFileSystemFsEntry(Project, Project.ProjectFS, "Project Directory"));
            anyFs = true;
        }

        if (anyFs && Project.FS is not EmptyVirtualFileSystem)
        {
            roots.Add(new VirtualFileSystemFsEntry(Project, Project.FS, "Full Combined FS"));
        }

        return true;
    }

    public void OnGUI(float dt, string[] cmd)
    {
        ImGui.Begin($"File Browser##FileBrowser", Project.BaseEditor.MainWindowFlags);

        uint dockspaceID = ImGui.GetID("FileBrowserDockspace");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"Browser List##BrowserList", Project.BaseEditor.SubWindowFlags);

        DisplayFileBrowser();

        ImGui.End();

        ImGui.Begin($"Item Viewer##ItemViewer", Project.BaseEditor.SubWindowFlags);

        DisplayItemViewer();

        ImGui.End();
    }

    private void Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Edit"))
            {

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    private void Shortcuts()
    {
        if (ImGui.IsWindowHovered())
        {

        }
    }

    private void DisplayFileBrowser()
    {
        if (!Initialized)
            return;

        if (roots.Count == 0)
        {
            ImGui.Text("No File System roots available. Load a project.");

            return;
        }

        foreach (var root in roots)
        {
            Traverse(root, $"File Browser");
        }
    }

    private void DisplayItemViewer()
    {
        if (!Initialized)
            return;

        if (selected == null)
        {
            ImGui.Text("Nothing selected");
        }
        else
        {
            if (selected.CanView)
            {
                if (!selected.IsInitialized && !selected.IsLoading)
                {
                    selected.LoadAsync(selectedId, selected.Name, Project);
                }

                if (selected.IsInitialized)
                {
                    selected.OnGui();
                }
                else
                {
                    ImGui.Text("Loading...");
                }
            }
            else
            {
                ImGui.Text($"Selected: {selected.Name}");
                ImGui.Text("This file has no Item Viewer.");
            }
        }
    }

    private void Traverse(FsEntry e, string parentIdStr)
    {
        string id = $"{parentIdStr}##{e.Name}";
        var flags = ImGuiTreeNodeFlags.OpenOnDoubleClick;

        if (e is VirtualFileSystemFsEntry)
            flags |= ImGuiTreeNodeFlags.CollapsingHeader;

        if (!e.CanHaveChildren)
            flags |= ImGuiTreeNodeFlags.Leaf;

        if (selected == e)
            flags |= ImGuiTreeNodeFlags.Selected;

        bool shouldBeOpen = e.CanHaveChildren && (e.IsInitialized || e.IsLoading);

        ImGui.SetNextItemOpen(shouldBeOpen);
        bool isOpen = ImGui.TreeNodeEx(id, flags, e.Name);

        if (ImGui.IsItemClicked())
        {
            Console.WriteLine(id);
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

    private void TryDeselect(FsEntry e)
    {
        if (selected == e)
        {
            selected = null;
        }
    }

    private void Select(string id, FsEntry e)
    {
        if (selected == e)
        {
            return;
        }

        if (selected != null)
        {
            selected.onUnload = null;
        }

        selected = e;
        selectedId = id;
        e.onUnload = TryDeselect;
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
}
