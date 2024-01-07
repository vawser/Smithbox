using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Scene;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks.Schedulers;

namespace StudioCore.Resource;

/// <summary>
///     Manages resources (mainly GPU) such as textures and models, and can be used to unload and reload them at will.
///     A background thread will manage the unloading and streaming in of assets. This is designed to map closely to
///     the souls asset system, but in a more abstract way
/// </summary>
public static class ResourceManager
{
    [Flags]
    public enum ResourceType
    {
        Flver = 1,
        Texture = 2,
        CollisionHKX = 4,
        Navmesh = 8,
        NavmeshHKX = 16,
        All = 0xFFFFFFF
    }

    private static QueuedTaskScheduler JobScheduler = new(4, "JobMaster");
    private static readonly TaskFactory JobTaskFactory = new(JobScheduler);

    public static AssetLocator Locator;

    private static readonly Dictionary<string, IResourceHandle> ResourceDatabase = new();
    private static readonly ConcurrentDictionary<ResourceJob, int> ActiveJobProgress = new();
    private static readonly HashSet<string> InFlightFiles = new();

    private static readonly BufferBlock<AddResourceLoadNotificationRequest> _notificationRequests = new();

    private static readonly BufferBlock<UnloadResourceRequest> _unloadRequests = new();

    private static int Pending = 0;
    private static int Finished = 0;
    private static int _prevCount;

    private static object AddResourceLock = new();
    private static bool AddingResource = false;

    private static bool _scheduleUDSFMLoad;
    private static bool _scheduleUnloadedTexturesLoad;

    private static bool TaskWindowOpen = true;

    private static IResourceHandle InstantiateResource(ResourceType type, string path)
    {
        switch (type)
        {
            case ResourceType.Flver:
                return new ResourceHandle<FlverResource>(path);
            //case ResourceType.Texture:
            //    return new ResourceHandle<TextureResource>(path);
        }

        return null;
    }

    private static LoadTPFTextureResourceRequest[] LoadTPFResources(LoadTPFResourcesAction action)
    {
        Tracy.___tracy_c_zone_context ctx =
            Tracy.TracyCZoneN(1, $@"LoadTPFResourcesTask::Run {action._virtpathbase}");
        if (!CFG.Current.EnableTexturing)
        {
            action._tpf = null;
            Tracy.TracyCZoneEnd(ctx);
            return new LoadTPFTextureResourceRequest[] { };
        }

        // If tpf is null this is a loose file load.
        if (action._tpf == null)
        {
            try
            {
                action._tpf = TPF.Read(action._filePath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to load TPF \"{action._filePath}\": {e.Message}",
                    LogLevel.Warning, TaskLogs.LogPriority.Normal, e);
                return new LoadTPFTextureResourceRequest[] { };
            }
        }

        action._job.IncrementEstimateTaskSize(action._tpf.Textures.Count);
        var ret = new LoadTPFTextureResourceRequest[action._tpf.Textures.Count];
        for (var i = 0; i < action._tpf.Textures.Count; i++)
        {
            TPF.Texture tex = action._tpf.Textures[i];
            ret[i] = new LoadTPFTextureResourceRequest($@"{action._virtpathbase}/{tex.Name}", action._tpf, i,
                action._accessLevel, action._game);
        }

        action._tpf = null;
        Tracy.TracyCZoneEnd(ctx);
        return ret;
    }

    private static void LoadBinderResources(LoadBinderResourcesAction action)
    {
        try
        {
            action.ProcessBinder();
            if (!action.PopulateResourcesOnly)
            {
                var doasync = action.PendingResources.Count() + action.PendingTPFs.Count() > 1;
                var i = 0;
                foreach (Tuple<IResourceLoadPipeline, string, BinderFileHeader> p in action.PendingResources)
                {
                    Memory<byte> f = action.Binder.ReadFile(p.Item3);
                    p.Item1.LoadByteResourceBlock.Post(new LoadByteResourceRequest(p.Item2, f, action.AccessLevel,
                        Locator.Type));
                    action._job.IncrementEstimateTaskSize(1);
                    i++;
                }

                foreach (Tuple<string, BinderFileHeader> t in action.PendingTPFs)
                {
                    try
                    {
                        TPF f = TPF.Read(action.Binder.ReadFile(t.Item2));
                        action._job.AddLoadTPFResources(new LoadTPFResourcesAction(action._job, t.Item1, f,
                            action.AccessLevel, Locator.Type));
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"Failed to load TPF \"{t.Item1}\"",
                            LogLevel.Warning, TaskLogs.LogPriority.Normal, e);
                    }

                    i++;
                }
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Failed to load binder \"{action.BinderVirtualPath}\"",
                LogLevel.Warning, TaskLogs.LogPriority.Normal, e);
        }

        action.PendingResources.Clear();
        action.Binder = null;
    }

    private static IResourceHandle ConstructHandle(Type t, string virtualpath)
    {
        if (t == typeof(FlverResource))
        {
            return new ResourceHandle<FlverResource>(virtualpath);
        }

        if (t == typeof(HavokCollisionResource))
        {
            return new ResourceHandle<HavokCollisionResource>(virtualpath);
        }

        if (t == typeof(HavokNavmeshResource))
        {
            return new ResourceHandle<HavokNavmeshResource>(virtualpath);
        }

        if (t == typeof(NVMNavmeshResource))
        {
            return new ResourceHandle<NVMNavmeshResource>(virtualpath);
        }

        if (t == typeof(TextureResource))
        {
            return new ResourceHandle<TextureResource>(virtualpath);
        }

        throw new Exception("Unhandled resource type");
    }

    /// <summary>
    ///     See if you can use a target resource's access level with a given access level
    /// </summary>
    /// <param name="src">The access level you want to use a resource at</param>
    /// <param name="target">The access level the resource is at</param>
    /// <returns></returns>
    public static bool CheckAccessLevel(AccessLevel src, AccessLevel target)
    {
        // Full access level means anything can use it
        if (target == AccessLevel.AccessFull)
        {
            return true;
        }

        return src == target;
    }

    public static BinderReader InstantiateBinderReaderForFile(string filePath, GameType type)
    {
        if (filePath == null || !File.Exists(filePath))
        {
            return null;
        }

        if (type == GameType.DemonsSouls || type == GameType.DarkSoulsPTDE || type == GameType.DarkSoulsRemastered)
        {
            if (filePath.ToUpper().EndsWith("BHD"))
            {
                return new BXF3Reader(filePath, filePath.Substring(0, filePath.Length - 3) + "bdt");
            }

            return new BND3Reader(filePath);
        }

        if (filePath.ToUpper().EndsWith("BHD"))
        {
            return new BXF4Reader(filePath, filePath.Substring(0, filePath.Length - 3) + "bdt");
        }

        return new BND4Reader(filePath);
    }

    public static void UnloadUnusedResources()
    {
        foreach (KeyValuePair<string, IResourceHandle> r in ResourceDatabase)
        {
            if (r.Value.IsLoaded() && r.Value.GetReferenceCounts() == 0)
            {
                r.Value.UnloadIfUnused();
            }
        }
    }

    public static ResourceJobBuilder CreateNewJob(string name)
    {
        return new ResourceJobBuilder(name);
    }

    /// <summary>
    ///     The primary way to get a handle to the resource, this will call the provided listener once the requested
    ///     resource is available and loaded. This will be called on the main UI thread.
    /// </summary>
    /// <param name="resourceName"></param>
    /// <param name="listener"></param>
    public static void AddResourceListener<T>(string resourceName, IResourceEventListener listener, AccessLevel al,
        int tag = 0) where T : IResource
    {
        _notificationRequests.Post(
            new AddResourceLoadNotificationRequest(resourceName.ToLower(), typeof(T), listener, al, tag));
    }

    public static void MarkResourceInFlight(string resourceName, AccessLevel al)
    {
        // TODO is this needed?
        /*var lResourceName = resourceName.ToLower();
        if (!ResourceDatabase.ContainsKey(lResourceName))
            ResourceDatabase.Add(lResourceName, new ResourceRegistration(al));
        ResourceDatabase[lResourceName].AccessLevel = al;*/
    }

    public static bool IsResourceLoadedOrInFlight(string resourceName, AccessLevel al)
    {
        var lResourceName = resourceName.ToLower();
        return ResourceDatabase.ContainsKey(lResourceName) &&
               CheckAccessLevel(al, ResourceDatabase[lResourceName].AccessLevel);
    }

    public static void UnloadResource(IResourceHandle resource, bool unloadOnlyIfUnused)
    {
        _unloadRequests.Post(new UnloadResourceRequest(resource, unloadOnlyIfUnused));
    }

    public static void ScheduleUDSMFRefresh()
    {
        _scheduleUDSFMLoad = true;
    }

    public static void ScheduleUnloadedTexturesRefresh()
    {
        _scheduleUnloadedTexturesLoad = true;
    }

    public static void UpdateTasks()
    {
        // Process any resource notification requests
        var res = _notificationRequests.TryReceiveAll(out IList<AddResourceLoadNotificationRequest> requests);
        if (res)
        {
            foreach (AddResourceLoadNotificationRequest r in requests)
            {
                var lResourceName = r.ResourceVirtualPath.ToLower();
                if (!ResourceDatabase.ContainsKey(lResourceName))
                {
                    ResourceDatabase.Add(lResourceName, ConstructHandle(r.Type, r.ResourceVirtualPath));
                }

                IResourceHandle registration = ResourceDatabase[lResourceName];
                registration.AddResourceEventListener(r.Listener, r.AccessLevel, r.tag);
            }
        }

        // If no loading job is currently in flight, process any unload requests
        var count = ActiveJobProgress.Count;
        if (count == 0)
        {
            InFlightFiles.Clear();
            if (_unloadRequests.TryReceiveAll(out IList<UnloadResourceRequest> toUnload))
            {
                foreach (UnloadResourceRequest r in toUnload)
                {
                    if (r.UnloadOnlyIfUnused && r.Resource.GetReferenceCounts() > 0)
                    {
                        continue;
                    }

                    r.Resource.Unload();
                    if (r.Resource.GetReferenceCounts() > 0)
                    {
                        continue;
                    }

                    ResourceDatabase.Remove(r.Resource.AssetVirtualPath.ToLower());
                }
            }
        }

        if (count > 0)
        {
            HashSet<ResourceJob> toRemove = new();
            foreach (KeyValuePair<ResourceJob, int> job in ActiveJobProgress)
            {
                job.Key.ProcessLoadedResources();
                if (job.Key.Finished)
                {
                    toRemove.Add(job.Key);
                }
            }

            foreach (ResourceJob rm in toRemove)
            {
                int o;
                ActiveJobProgress.TryRemove(rm, out o);
            }
        }
        else
        {
            if (Renderer.GeometryBufferAllocator != null &&
                Renderer.GeometryBufferAllocator.HasStagingOrPending())
            {
                Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, "Flush Staging buffer");
                Renderer.GeometryBufferAllocator.FlushStaging(true);
                Tracy.TracyCZoneEnd(ctx);
            }

            if (_scheduleUDSFMLoad)
            {
                ResourceJobBuilder job = CreateNewJob(@"Loading UDSFM textures");
                job.AddLoadUDSFMTexturesTask();
                job.Complete();
                _scheduleUDSFMLoad = false;
            }

            if (_scheduleUnloadedTexturesLoad)
            {
                ResourceJobBuilder job = CreateNewJob(@"Loading other textures");
                job.AddLoadUnloadedTextures();
                job.Complete();
                _scheduleUnloadedTexturesLoad = false;
            }
        }

        // If there were active jobs last frame but none this frame, clear out unused resources
        if (_prevCount > 0 && ActiveJobProgress.Count == 0)
        {
            UnloadUnusedResources();
        }

        _prevCount = ActiveJobProgress.Count;
    }

    public static void OnGuiDrawTasks(float w, float h)
    {
        var scale = MapStudioNew.GetUIScale();

        if (ActiveJobProgress.Count() > 0)
        {
            ImGui.SetNextWindowSize(new Vector2(400, 310) * scale);
            ImGui.SetNextWindowPos(new Vector2(w - (100 * scale), h - (300 * scale)));
            if (!ImGui.Begin("Resource Loading Tasks", ref TaskWindowOpen, ImGuiWindowFlags.NoDecoration))
            {
                ImGui.End();
                return;
            }

            foreach (KeyValuePair<ResourceJob, int> job in ActiveJobProgress)
            {
                if (!job.Key.Finished)
                {
                    var completed = job.Key.Progress;
                    var size = job.Key.GetEstimateTaskSize();
                    ImGui.Text(job.Key.Name);
                    if (size == 0)
                    {
                        ImGui.ProgressBar(0.0f);
                    }
                    else
                    {
                        ImGui.ProgressBar(completed / (float)size, new Vector2(386.0f, 20.0f) * scale);
                    }
                }
            }

            ImGui.End();
        }
    }

    public static void OnGuiDrawResourceList()
    {
        if (!ImGui.Begin("Resource List"))
        {
            ImGui.End();
            return;
        }

        ImGui.Text("List of Resources Loaded & Unloaded");
        ImGui.Columns(4);
        ImGui.Separator();
        var id = 0;
        foreach (KeyValuePair<string, IResourceHandle> item in ResourceDatabase)
        {
            ImGui.PushID(id);
            ImGui.AlignTextToFramePadding();
            ImGui.Text(item.Key);
            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(item.Value.IsLoaded() ? "Loaded" : "Unloaded");
            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(item.Value.AccessLevel.ToString());
            ImGui.NextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.Text(item.Value.GetReferenceCounts().ToString());
            ImGui.NextColumn();
            ImGui.PopID();
        }

        ImGui.Columns(1);
        ImGui.Separator();
        ImGui.End();
    }

    public static void Shutdown()
    {
        JobScheduler.Dispose();
        JobScheduler = null;
    }

    public interface IResourceTask
    {
        public void Run();
        public Task RunAsync(IProgress<int> progress);

        /// <summary>
        ///     Get an estimate of the size of a task (i.e. how many files to load)
        /// </summary>
        /// <returns></returns>
        public int GetEstimateTaskSize();
    }

    internal struct LoadTPFResourcesAction
    {
        public ResourceJob _job;
        public string _virtpathbase = null;
        public TPF _tpf = null;
        public string _filePath = null;
        public AccessLevel _accessLevel = AccessLevel.AccessGPUOptimizedOnly;
        public GameType _game;

        public LoadTPFResourcesAction(ResourceJob job, string virtpathbase, TPF tpf, AccessLevel al, GameType type)
        {
            _job = job;
            _virtpathbase = virtpathbase;
            _tpf = tpf;
            _accessLevel = al;
            _game = type;
        }

        public LoadTPFResourcesAction(ResourceJob job, string virtpathbase, string filePath, AccessLevel al,
            GameType type)
        {
            _job = job;
            _virtpathbase = virtpathbase;
            _filePath = filePath;
            _accessLevel = al;
            _game = type;
        }
    }

    internal class LoadBinderResourcesAction
    {
        public readonly object ProgressLock = new();
        public ResourceJob _job;
        public AccessLevel AccessLevel = AccessLevel.AccessGPUOptimizedOnly;
        public HashSet<string> AssetWhitelist;
        public BinderReader Binder;
        public HashSet<int> BinderLoadMask = null;
        public string BinderVirtualPath;
        public List<Task> LoadingTasks = new();

        public List<Tuple<IResourceLoadPipeline, string, BinderFileHeader>> PendingResources = new();
        public List<Tuple<string, BinderFileHeader>> PendingTPFs = new();
        public bool PopulateResourcesOnly;
        public ResourceType ResourceMask = ResourceType.All;
        public List<int> TaskProgress = new();
        public List<int> TaskSizes = new();
        public int TotalSize = 0;

        public LoadBinderResourcesAction(ResourceJob job, string virtpath, AccessLevel accessLevel,
            bool populateOnly, ResourceType mask, HashSet<string> whitelist)
        {
            _job = job;
            BinderVirtualPath = virtpath;
            PopulateResourcesOnly = populateOnly;
            ResourceMask = mask;
            AssetWhitelist = whitelist;
            AccessLevel = accessLevel;
        }

        public void ProcessBinder()
        {
            if (Binder == null)
            {
                string o;
                var path = Locator.VirtualToRealPath(BinderVirtualPath, out o);
                Binder = InstantiateBinderReaderForFile(path, Locator.Type);
                if (Binder == null)
                {
                    return;
                }
            }

            for (var i = 0; i < Binder.Files.Count(); i++)
            {
                BinderFileHeader f = Binder.Files[i];
                if (BinderLoadMask != null && !BinderLoadMask.Contains(i))
                {
                    continue;
                }

                var binderpath = f.Name;
                var filevirtpath = Locator.GetBinderVirtualPath(BinderVirtualPath, binderpath);
                if (AssetWhitelist != null && !AssetWhitelist.Contains(filevirtpath))
                {
                    continue;
                }

                IResourceLoadPipeline pipeline = null;
                if (filevirtpath.ToUpper().EndsWith(".TPF") || filevirtpath.ToUpper().EndsWith(".TPF.DCX"))
                {
                    var virt = BinderVirtualPath;
                    if (virt.StartsWith(@"map/tex"))
                    {
                        Regex regex = new(@"\d{4}$");
                        if (regex.IsMatch(virt))
                        {
                            virt = virt.Substring(0, virt.Length - 5);
                        }
                        else if (virt.EndsWith("tex"))
                        {
                            virt = virt.Substring(0, virt.Length - 4);
                        }
                    }

                    PendingTPFs.Add(new Tuple<string, BinderFileHeader>(virt, f));
                }
                else
                {
                    if (ResourceMask.HasFlag(ResourceType.Flver) &&
                        (filevirtpath.ToUpper().EndsWith(".FLVER") ||
                         filevirtpath.ToUpper().EndsWith(".FLV") ||
                         filevirtpath.ToUpper().EndsWith(".FLV.DCX")))
                    {
                        //handle = new ResourceHandle<FlverResource>();
                        pipeline = _job.FlverLoadPipeline;
                    }
                    else if (ResourceMask.HasFlag(ResourceType.CollisionHKX) &&
                             (filevirtpath.ToUpper().EndsWith(".HKX") ||
                              filevirtpath.ToUpper().EndsWith(".HKX.DCX")))
                    {
                        pipeline = _job.HavokCollisionLoadPipeline;
                    }
                    else if (ResourceMask.HasFlag(ResourceType.Navmesh) && filevirtpath.ToUpper().EndsWith(".NVM"))
                    {
                        pipeline = _job.NVMNavmeshLoadPipeline;
                    }
                    else if (ResourceMask.HasFlag(ResourceType.NavmeshHKX) &&
                             (filevirtpath.ToUpper().EndsWith(".HKX") ||
                              filevirtpath.ToUpper().EndsWith(".HKX.DCX")))
                    {
                        pipeline = _job.HavokNavmeshLoadPipeline;
                    }

                    if (pipeline != null)
                    {
                        PendingResources.Add(
                            new Tuple<IResourceLoadPipeline, string, BinderFileHeader>(pipeline, filevirtpath, f));
                    }
                }
            }
        }
    }

    /// <summary>
    ///     A named job that runs many tasks and whose progress will appear in the progress window
    /// </summary>
    public class ResourceJob
    {
        private readonly ActionBlock<LoadBinderResourcesAction> _loadBinderResources;

        private readonly TransformManyBlock<LoadTPFResourcesAction, LoadTPFTextureResourceRequest>
            _loadTPFResources;

        private readonly BufferBlock<ResourceLoadedReply> _processedResources;
        private int _courseSize;
        private int TotalSize;

        public ResourceJob(string name)
        {
            ExecutionDataflowBlockOptions options = new() { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded };
            Name = name;
            _loadTPFResources =
                new TransformManyBlock<LoadTPFResourcesAction, LoadTPFTextureResourceRequest>(LoadTPFResources,
                    options);

            //options.MaxDegreeOfParallelism = 4;
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

        internal void AddLoadTPFResources(LoadTPFResourcesAction action)
        {
            _loadTPFResources.Post(action);
        }

        internal void AddLoadBinderResources(LoadBinderResourcesAction action)
        {
            _courseSize++;
            _loadBinderResources.Post(action);
        }

        public Task Complete()
        {
            return JobTaskFactory.StartNew(() =>
            {
                _loadBinderResources.Complete();
                _loadBinderResources.Completion.Wait();
                FlverLoadPipeline.LoadByteResourceBlock.Complete();
                FlverLoadPipeline.LoadFileResourceRequest.Complete();
                HavokCollisionLoadPipeline.LoadByteResourceBlock.Complete();
                HavokCollisionLoadPipeline.LoadFileResourceRequest.Complete();
                HavokNavmeshLoadPipeline.LoadByteResourceBlock.Complete();
                HavokNavmeshLoadPipeline.LoadFileResourceRequest.Complete();
                _loadTPFResources.Complete();
                _loadTPFResources.Completion.Wait();
                TPFTextureLoadPipeline.LoadTPFTextureResourceRequest.Complete();
                FlverLoadPipeline.LoadByteResourceBlock.Completion.Wait();
                FlverLoadPipeline.LoadFileResourceRequest.Completion.Wait();
                HavokCollisionLoadPipeline.LoadByteResourceBlock.Completion.Wait();
                HavokCollisionLoadPipeline.LoadFileResourceRequest.Completion.Wait();
                HavokNavmeshLoadPipeline.LoadByteResourceBlock.Completion.Wait();
                HavokNavmeshLoadPipeline.LoadFileResourceRequest.Completion.Wait();
                TPFTextureLoadPipeline.LoadTPFTextureResourceRequest.Completion.Wait();
                Finished = true;
            });
        }

        public void ProcessLoadedResources()
        {
            if (_processedResources.TryReceiveAll(out IList<ResourceLoadedReply> processed))
            {
                Progress += processed.Count;
                foreach (ResourceLoadedReply p in processed)
                {
                    var lPath = p.VirtualPath.ToLower();
                    if (!ResourceDatabase.ContainsKey(lPath))
                    {
                        ResourceDatabase.Add(lPath, ConstructHandle(p.Resource.GetType(), p.VirtualPath));
                    }

                    IResourceHandle reg = ResourceDatabase[lPath];
                    reg._ResourceLoaded(p.Resource, p.AccessLevel);
                }
            }
        }
    }

    public class ResourceJobBuilder
    {
        private readonly ResourceJob _job;
        private readonly HashSet<string> archivesToLoad = new();
        private string Name;

        public ResourceJobBuilder(string name)
        {
            _job = new ResourceJob(name);
            Name = name;
        }

        /// <summary>
        ///     Loads an entire archive in this virtual path
        /// </summary>
        /// <param name="virtualPath"></param>
        public void AddLoadArchiveTask(string virtualPath, AccessLevel al, bool populateOnly,
            HashSet<string> assets = null)
        {
            if (InFlightFiles.Contains(virtualPath))
            {
                return;
            }

            InFlightFiles.Add(virtualPath);
            if (virtualPath == "null")
            {
                return;
            }

            if (!archivesToLoad.Contains(virtualPath))
            {
                archivesToLoad.Add(virtualPath);
                _job.AddLoadBinderResources(new LoadBinderResourcesAction(_job, virtualPath, al, populateOnly,
                    ResourceType.All, assets));
            }
        }

        public void AddLoadArchiveTask(string virtualPath, AccessLevel al, bool populateOnly, ResourceType filter,
            HashSet<string> assets = null)
        {
            if (InFlightFiles.Contains(virtualPath))
            {
                return;
            }

            InFlightFiles.Add(virtualPath);
            if (virtualPath == "null")
            {
                return;
            }

            if (!archivesToLoad.Contains(virtualPath))
            {
                archivesToLoad.Add(virtualPath);
                _job.AddLoadBinderResources(new LoadBinderResourcesAction(_job, virtualPath, al, populateOnly,
                    filter, assets));
            }
        }

        /// <summary>
        ///     Loads a loose virtual file
        /// </summary>
        /// <param name="virtualPath"></param>
        public void AddLoadFileTask(string virtualPath, AccessLevel al)
        {
            if (InFlightFiles.Contains(virtualPath))
            {
                return;
            }

            InFlightFiles.Add(virtualPath);

            string bndout;
            var path = Locator.VirtualToRealPath(virtualPath, out bndout);

            IResourceLoadPipeline pipeline;
            if (path == null || virtualPath == "null")
            {
                return;
            }

            if (virtualPath.EndsWith(".hkx"))
            {
                pipeline = _job.HavokCollisionLoadPipeline;
            }
            else if (path.ToUpper().EndsWith(".TPF") || path.ToUpper().EndsWith(".TPF.DCX"))
            {
                var virt = virtualPath;
                if (virt.StartsWith(@"map/tex"))
                {
                    Regex regex = new(@"\d{4}$");
                    if (regex.IsMatch(virt))
                    {
                        virt = virt.Substring(0, virt.Length - 5);
                    }
                    else if (virt.EndsWith("tex"))
                    {
                        virt = virt.Substring(0, virt.Length - 4);
                    }
                }

                _job.AddLoadTPFResources(new LoadTPFResourcesAction(_job, virt, path, al, Locator.Type));
                return;
            }
            else
            {
                pipeline = _job.FlverLoadPipeline;
            }

            pipeline.LoadFileResourceRequest.Post(new LoadFileResourceRequest(virtualPath, path, al, Locator.Type));
        }

        /// <summary>
        ///     Attempts to load unloaded resources (with active references) via UDSFM textures
        /// </summary>
        public void AddLoadUDSFMTexturesTask()
        {
            foreach (KeyValuePair<string, IResourceHandle> r in ResourceDatabase)
            {
                if (!r.Value.IsLoaded())
                {
                    var texpath = r.Key;
                    string path = null;
                    if (texpath.StartsWith("map/tex"))
                    {
                        path = $@"{Locator.GameRootDirectory}\map\tx\{Path.GetFileName(texpath)}.tpf";
                    }

                    if (path != null && File.Exists(path))
                    {
                        _job.AddLoadTPFResources(new LoadTPFResourcesAction(_job,
                            Path.GetDirectoryName(r.Key).Replace('\\', '/'),
                            path, AccessLevel.AccessGPUOptimizedOnly, Locator.Type));
                    }
                }
            }
        }

        /// <summary>
        ///     Looks for unloaded textures and queues them up for loading. References to parts and Elden Ring AETs depend on this
        /// </summary>
        public void AddLoadUnloadedTextures()
        {
            HashSet<string> assetTpfs = new();
            foreach (KeyValuePair<string, IResourceHandle> r in ResourceDatabase)
            {
                if (!r.Value.IsLoaded())
                {
                    var texpath = r.Key;
                    string path = null;
                    if (texpath.StartsWith("aet/"))
                    {
                        var splits = texpath.Split('/');
                        var aetid = splits[1];
                        var aetname = splits[2];
                        var fullaetid = aetname.Substring(0, 10);
                        path =
                            $@"{Locator.GameRootDirectory}\asset\aet\{aetid.Substring(0, 6)}\{fullaetid}.tpf.dcx";
                        if (assetTpfs.Contains(fullaetid))
                        {
                            continue;
                        }

                        assetTpfs.Add(fullaetid);
                    }

                    if (path != null && File.Exists(path))
                    {
                        _job.AddLoadTPFResources(new LoadTPFResourcesAction(_job,
                            Path.GetDirectoryName(r.Key).Replace('\\', '/'), path,
                            AccessLevel.AccessGPUOptimizedOnly, Locator.Type));
                    }
                }
            }
        }

        public Task Complete()
        {
            // Build the job, register it with the task manager, and start it
            ActiveJobProgress[_job] = 0;
            Task jobtask = _job.Complete();
            return jobtask;
        }
    }

    private class ResourceRegistration
    {
        public ResourceRegistration(AccessLevel al)
        {
            AccessLevel = al;
        }

        public IResourceHandle? Handle { get; set; } = null;
        public AccessLevel AccessLevel { get; set; }

        public List<AddResourceLoadNotificationRequest> NotificationRequests { get; set; } = new();
    }

    private readonly record struct AddResourceLoadNotificationRequest(
        string ResourceVirtualPath,
        Type Type,
        IResourceEventListener Listener,
        AccessLevel AccessLevel,
        int tag);

    private readonly record struct UnloadResourceRequest(
        IResourceHandle Resource,
        bool UnloadOnlyIfUnused);
}
