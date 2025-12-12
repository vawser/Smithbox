using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.ModelEditor;
using StudioCore.Resource.Types;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Resource;

/// <summary>
///     A named job that runs many tasks and whose progress will appear in the progress window
/// </summary>
public class ResourceJob
{
    // PIPELINE: holds all the Load Binder Resources actions for processing.
    private readonly ActionBlock<LoadBinderResourcesAction> _loadBinderResources;

    private readonly TransformManyBlock<LoadTPFResourcesAction, LoadTPFTextureResourceRequest>
        _loadTPFResources;

    private readonly BufferBlock<ResourceLoadedReply> _processedResources;
    private int _courseSize;
    private int TotalSize;

    public ResourceJob(string name)
    {
        Name = name;
        IsPersistent = false;

        ExecutionDataflowBlockOptions options = new() { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded };

        _loadTPFResources = new TransformManyBlock<LoadTPFResourcesAction, LoadTPFTextureResourceRequest>(LoadTPFResources, options);

        _loadBinderResources = new ActionBlock<LoadBinderResourcesAction>(LoadBinderResources, options);

        _processedResources = new BufferBlock<ResourceLoadedReply>();

        FlverLoadPipeline = new ResourceLoadPipeline<FlverResource>(_processedResources);

        HavokCollisionLoadPipeline = new ResourceLoadPipeline<HavokCollisionResource>(_processedResources);

        HavokNavmeshLoadPipeline = new ResourceLoadPipeline<HavokNavmeshResource>(_processedResources);
        NVMNavmeshLoadPipeline = new ResourceLoadPipeline<NVMNavmeshResource>(_processedResources);
        TPFTextureLoadPipeline = new TextureLoadPipeline(_processedResources);
        _loadTPFResources.LinkTo(TPFTextureLoadPipeline.LoadTPFTextureResourceRequest);
    }

    public string Name { get; }
    public int Progress { get; private set; }

    public bool IsPersistent { get; private set; }

    // Asset load pipelines
    internal IResourceLoadPipeline FlverLoadPipeline { get; }
    internal IResourceLoadPipeline HavokCollisionLoadPipeline { get; }
    internal IResourceLoadPipeline HavokNavmeshLoadPipeline { get; }
    internal IResourceLoadPipeline NVMNavmeshLoadPipeline { get; }
    internal IResourceLoadPipeline TPFTextureLoadPipeline { get; }

    public bool Finished { get; private set; }

    internal void IncrementEstimateTaskSize(int size)
    {
        Interlocked.Add(ref TotalSize, size);
    }

    internal void IncrementCourseEstimateTaskSize(int size)
    {
        Interlocked.Add(ref _courseSize, size);
    }

    public int GetEstimateTaskSize()
    {
        return Math.Max(TotalSize, _courseSize);
    }

    #region TPF Resource
    internal void AddLoadTPFResources(LoadTPFResourcesAction action, bool isPersistent = false)
    {
        IsPersistent = isPersistent;
        _loadTPFResources.Post(action);
    }

    public static LoadTPFTextureResourceRequest[] LoadTPFResources(LoadTPFResourcesAction action)
    {
        var project = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        // If tpf is null this is a loose file load.
        if (action._tpf == null)
        {
            // External
            if (action._virtpathbase.Contains("smithbox"))
            {
                if (File.Exists(action._filePath))
                {
                    try
                    {
                        var fileData = File.ReadAllBytes(action._filePath);
                        action._tpf = TPF.Read(fileData);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"Failed to load TPF:\nFile path: {action._filePath}\nVirtual path: {action._virtpathbase}\nAccess Level: {action._accessLevel}\n{e}", LogLevel.Warning, LogPriority.Normal);

                        return new LoadTPFTextureResourceRequest[] { };
                    }
                }
            }
            // VFS
            else
            {
                if (project.FS.FileExists(action._filePath))
                {
                    try
                    {
                        var fileData = project.FS.ReadFile(action._filePath);
                        action._tpf = TPF.Read(fileData.Value);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"Failed to load TPF:\nFile path: {action._filePath}\nVirtual path: {action._virtpathbase}\nAccess Level: {action._accessLevel}\n{e}", LogLevel.Warning, LogPriority.Normal);

                        return new LoadTPFTextureResourceRequest[] { };
                    }
                }
            }
        }

        var tpf = action._tpf;

        action._job.IncrementEstimateTaskSize(tpf.Textures.Count);
        var ret = new LoadTPFTextureResourceRequest[tpf.Textures.Count];

        for (var i = 0; i < tpf.Textures.Count; i++)
        {
            TPF.Texture tex = tpf.Textures[i];

            if (project != null)
            {
                ret[i] = new LoadTPFTextureResourceRequest(
                    $@"{action._virtpathbase}/{tex.Name}", 
                    tpf, i, action._accessLevel);
            }
        }

        action._tpf = null;

        return ret;
    }
    #endregion

    #region Binder Resource
    internal void AddLoadBinderResources(LoadBinderResourcesAction action, bool isPersistent = false)
    {
        IsPersistent = isPersistent;
        _courseSize++;
        _loadBinderResources.Post(action);
    }

    public static void LoadBinderResources(LoadBinderResourcesAction action)
    {
        try
        {
            action.ProcessBinder();
            var b = action.Binder;
            if (!action.PopulateResourcesOnly)
            {
                var doasync = action.PendingResources.Count() + action.PendingTPFs.Count() > 1;
                var i = 0;

                // PIPELINE: non-TPF files within the binder
                foreach (var p in action.PendingResources)
                {
                    var pipeline = p.Item1;
                    var virtualPath = p.Item2;
                    var binder = p.Item3;

                    i++;

                    Memory<byte> binderData = action.Binder.Value.ReadFile(binder.Value);

                    var child = new ChildResource<BinderReader, Memory<byte>>(action.Binder.Ref(), binderData);

                    // PIPELINE: create request to load resource from bytes
                    var request = new LoadByteResourceRequest(virtualPath, child, action.AccessLevel);

                    // PIPELINE: add request to pipeline action block
                    pipeline.LoadByteResourceBlock.Post(request);


                    action._job.IncrementEstimateTaskSize(1);

                }

                // PIPELINE: TPF files within the binder
                foreach (var t in action.PendingTPFs)
                {
                    var tpfName = t.Item1;
                    var binder = t.Item2;
                    var isPersistent = t.Item3;
                    i++;

                    try
                    {
                        TPF tpf = TPF.Read(action.Binder.Value.ReadFile(binder.Value));

                        var request = new LoadTPFResourcesAction(action._job, tpfName, tpf, action.AccessLevel);

                        action._job.AddLoadTPFResources(request, isPersistent);
                    }
                    catch (Exception e)
                    {

                        TaskLogs.AddLog("" +
                            $"Failed to load TPF:\nName: {tpfName}" +
                            $"\nBinder Path: {action.BinderRelativePath}" +
                            $"\nBinder Virtual Path: {action.BinderVirtualPath}" +
                            $"\nAccess Level: {action.AccessLevel}" +
                            $"\n{e}",
                            LogLevel.Warning, LogPriority.Normal);

                        i--;
                    }

                }
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Failed to load binder: {action.BinderVirtualPath}" +
                            $"\nBinder Virtual Path: {action.BinderVirtualPath}" +
                            $"\nAccess Level: {action.AccessLevel}" +
                            $"\n{e}",
                            LogLevel.Warning, LogPriority.Normal);
        }

        if (action.Binder != null)
        {
            // We always want to dispose when using the Model Editor (so we can save the container), so we ignore the refcount logic

            var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

            if (curProject != null)
            {
                if (curProject.FocusedEditor is ModelEditorScreen)
                {
                    action.Binder.Dispose(true);
                }
                else
                {
                    action.Binder.Dispose();
                }
            }
        }

        action.PendingResources.Clear();
        action.Binder = null;
    }
    #endregion

    public void ProcessLoadedResources()
    {
        if (_processedResources.TryReceiveAll(out IList<ResourceLoadedReply> processed))
        {
            Progress += processed.Count;

            foreach (ResourceLoadedReply p in processed)
            {
                var lPath = p.VirtualPath.ToLower();

                if (!ResourceManager.ResourceDatabase.ContainsKey(lPath))
                {
                    ResourceManager.ResourceDatabase.Add(
                        lPath,
                        ResourceManager.ConstructHandle(
                            p.Resource.GetType(),
                            p.VirtualPath,
                            IsPersistent)
                        );
                }

                IResourceHandle reg = ResourceManager.ResourceDatabase[lPath];
                reg._ResourceLoaded(p.Resource, p.AccessLevel);
            }
        }
    }

    public Task Complete()
    {
        return ResourceManager.JobTaskFactory.StartNew(() =>
        {
            // HINT:
            // Add timeout duration to Wait() if hang issues occur to allow exception to filter up

            // PIPELINE: complete all Load Binder Resources actions within the ActionBlock
            _loadBinderResources.Complete();

            // PIPELINE: wait for compleition
            _loadBinderResources.Completion.Wait();

            // FLVER
            FlverLoadPipeline.LoadByteResourceBlock.Complete();
            FlverLoadPipeline.LoadFileResourceRequest.Complete();

            // HAVOK COLLISION
            HavokCollisionLoadPipeline.LoadByteResourceBlock.Complete();
            HavokCollisionLoadPipeline.LoadFileResourceRequest.Complete();

            // HAVOK NAVMESH
            HavokNavmeshLoadPipeline.LoadByteResourceBlock.Complete();
            HavokNavmeshLoadPipeline.LoadFileResourceRequest.Complete();

            // TPF
            _loadTPFResources.Complete();
            _loadTPFResources.Completion.Wait();
            TPFTextureLoadPipeline.LoadTPFTextureResourceRequest.Complete();

            // FLVER
            FlverLoadPipeline.LoadByteResourceBlock.Completion.Wait();
            FlverLoadPipeline.LoadFileResourceRequest.Completion.Wait();

            // HAVOK COLLISION
            HavokCollisionLoadPipeline.LoadByteResourceBlock.Completion.Wait();
            HavokCollisionLoadPipeline.LoadFileResourceRequest.Completion.Wait();

            // HAVOK NAVMESH
            HavokNavmeshLoadPipeline.LoadByteResourceBlock.Completion.Wait();
            HavokNavmeshLoadPipeline.LoadFileResourceRequest.Completion.Wait();

            // TPF
            TPFTextureLoadPipeline.LoadTPFTextureResourceRequest.Completion.Wait();

            ResourceLog.AddLog($"Job: {Name} - Finished");
            Finished = true;
        });
    }
}
