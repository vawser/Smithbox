using Andre.IO.VFS;
using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using System;
using System.Collections.Generic;

namespace Smithbox.Core.FileBrowserNS.Entries;

public abstract class FsEntry
{
    public abstract bool IsInitialized { get; }

    public abstract string Name { get; }

    public abstract bool CanHaveChildren { get; }

    public abstract bool CanView { get; }

    public abstract List<FsEntry> Children { get; }

    public Action<FsEntry>? onUnload;

    private TaskManager.LiveTask? loadingTask = null;
    public bool IsLoading => loadingTask != null;

    public TaskManager.LiveTask LoadAsync(string id, string name, Project ownerProject)
    {
        if (loadingTask != null) 
            return loadingTask;

        loadingTask = new TaskManager.LiveTask($"{id}", $"{name}", $"Loaded {name}", $"Failed to load {name}", TaskManager.RequeueType.WaitThenRequeue, false,
            () =>
            {
                Load(ownerProject);
                loadingTask = null;
            });

        TaskManager.Run(loadingTask);

        return loadingTask;
    }

    internal void Load(Project ownerProject, Action<FsEntry> onUnload)
    {
        this.onUnload = onUnload;
        Load(ownerProject);
    }

    internal abstract void Load(Project ownerProject);

    internal abstract void UnloadInner();

    public void Unload()
    {
        if (!IsInitialized) return;
        UnloadInner();
        if (onUnload != null)
        {
            onUnload(this);
            onUnload = null;
        }
    }

    public virtual void OnGui()
    {
        throw new NotImplementedException($"Viewer not implemented for {GetType().FullName}");
    }

    internal static bool PropertyTable(string name, Action<Action<string, string>> rowsFunc)
    {
        if (!ImGui.BeginTable(name, 2))
            return false;
        ImGui.TableSetupColumn("Property");
        ImGui.TableSetupColumn("Value");
        ImGui.TableHeadersRow();
        void Row(string a, string b)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(a);
            ImGui.TableNextColumn();
            ImGui.Text(b);
        }
        rowsFunc(Row);
        ImGui.EndTable();
        return true;
    }

    public static FsEntry? TryGetFor(Project ownerProject, string fileName, Func<Memory<byte>> getDataFunc, VirtualFileSystem? vfs = null, string? path = null)
    {
        if (fileName.EndsWith(".txt"))
        {
            return new TextFsEntry(ownerProject, fileName, getDataFunc);
        }

        return SoulsFileFsEntry.TryGetFor(ownerProject, fileName, getDataFunc, vfs, path);
    }
}