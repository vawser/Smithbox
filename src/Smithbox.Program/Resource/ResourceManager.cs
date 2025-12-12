using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Resource.Locators;
using StudioCore.Resource.Types;
using StudioCore.Scene;
using StudioCore.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Resource;

/// <summary>
/// Manages resources (mainly GPU) such as textures and models, and can be used to unload and reload them at will.
/// A background thread will manage the unloading and streaming in of assets. This is designed to map closely to
/// the souls asset system, but in a more abstract way
/// </summary>
public static class ResourceManager
{
    public static Smithbox BaseEditor;

    public static QueuedTaskScheduler JobScheduler = new(4, "JobMaster");
    public static readonly TaskFactory JobTaskFactory = new(JobScheduler);

    public static readonly Dictionary<string, IResourceHandle> ResourceDatabase = new();
    public static readonly ConcurrentDictionary<ResourceJob, int> ActiveJobProgress = new();
    public static readonly HashSet<string> InFlightFiles = new();

    public static readonly BufferBlock<AddResourceLoadNotificationRequest> _notificationRequests = new();

    public static readonly BufferBlock<UnloadResourceRequest> _unloadRequests = new();


    public static int _prevCount;

    public static bool _schedulePostTextureLoad;
    public static bool _scheduleWorldMapLoad;

    public static Dictionary<string, IResourceHandle> GetResourceDatabase()
    {
        return ResourceDatabase;
    }

    public static ConcurrentDictionary<ResourceJob, int> GetActiveJobProgress()
    {
        return ActiveJobProgress;
    }

    public static IResourceHandle ConstructHandle(Type t, string virtualpath, bool isPersistent = false)
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
            return new ResourceHandle<TextureResource>(virtualpath, isPersistent);
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

    public static bool IsResourceLoaded(string resourceName, AccessLevel al)
    {
        var lResourceName = resourceName.ToLower();
        return ResourceDatabase.ContainsKey(lResourceName) &&
               CheckAccessLevel(al, ResourceDatabase[lResourceName].AccessLevel);
    }

    public static void RemoveResource(string resourceName)
    {
        if(ResourceDatabase.ContainsKey(resourceName))
        {
            ResourceDatabase[resourceName].Release(true);
        }
    }

    public static void UnloadResource(IResourceHandle resource, bool unloadOnlyIfUnused)
    {
        _unloadRequests.Post(new UnloadResourceRequest(resource, unloadOnlyIfUnused));
    }

    public static void SchedulePostTextureRefresh()
    {
        _schedulePostTextureLoad = true;
    }

    public static void ScheduleWorldMapRefresh()
    {
        _scheduleWorldMapLoad = true;
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

            if (_schedulePostTextureLoad)
            {
                ResourceJobBuilder job = CreateNewJob(@"Loading additional textures");
                job.AddPostTextureLoadTask();
                job.Complete();
                _schedulePostTextureLoad = false;
            }

            if (_scheduleWorldMapLoad)
            {
                ResourceJobBuilder job = CreateNewJob(@"Loading world map textures");
                job.AddWorldMapLoadTask();
                job.Complete();
                _scheduleWorldMapLoad = false;
            }
        }

        // If there were active jobs last frame but none this frame, clear out unused resources
        //if (_prevCount > 0 && ActiveJobProgress.Count == 0)
        //{
        //    UnloadUnusedResources();
        //}

        _prevCount = ActiveJobProgress.Count;
    }

    public static void ClearUnusedResources()
    {
        if (ActiveJobProgress.Count == 0)
        {
            UnloadUnusedResources();
        }
    }

    public static void Shutdown()
    {
        JobScheduler.Dispose();
        JobScheduler = null;
    }
}