using StudioCore.Application;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Renderer;

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
    /// Loading files within binder (bnd, bhd/bdt)
    /// </summary>
    /// <param name="virtualPath"></param>
    /// <param name="al"></param>
    /// <param name="populateOnly"></param>
    /// <param name="assets"></param>
    /// <param name="isPersistent"></param>
    public void AddLoadArchiveTask(string virtualPath, AccessLevel al, bool populateOnly,
        HashSet<string> assets = null, bool isPersistent = false)
    {
        if (ResourceManager.InFlightFiles.Contains(virtualPath))
        {
            return;
        }

        ResourceManager.InFlightFiles.Add(virtualPath);

        if (virtualPath == "null")
        {
            return;
        }

        if (!archivesToLoad.Contains(virtualPath))
        {
            archivesToLoad.Add(virtualPath);
            _job.AddLoadBinderResources(
                new LoadBinderResourcesAction(_job, virtualPath, al, populateOnly, ResourceType.All, assets));
        }
    }

    /// <summary>
    /// Loading files within binder (bnd, bhd/bdt), with resource filter
    /// </summary>
    /// <param name="virtualPath"></param>
    /// <param name="al"></param>
    /// <param name="populateOnly"></param>
    /// <param name="filter"></param>
    /// <param name="assets"></param>
    /// <param name="isPersistent"></param>
    public void AddLoadArchiveTask(string virtualPath, AccessLevel al, bool populateOnly, ResourceType filter, HashSet<string> assets = null, bool isPersistent = false)
    {
        if (ResourceManager.InFlightFiles.Contains(virtualPath))
        {
            return;
        }

        ResourceManager.InFlightFiles.Add(virtualPath);

        if (virtualPath == "null")
        {
            return;
        }

        if (!archivesToLoad.Contains(virtualPath))
        {
            archivesToLoad.Add(virtualPath);
            _job.AddLoadBinderResources(
                new LoadBinderResourcesAction(_job, virtualPath, al, populateOnly, filter, assets));
        }
    }

    /// <summary>
    /// Loading singular file (no binder) via VFS
    /// </summary>
    /// <param name="virtualPath"></param>
    /// <param name="al"></param>
    /// <param name="isPersistent"></param>
    public void AddLoadFileTask(string virtualPath, AccessLevel al, bool isPersistent = false)
    {
        if (ResourceManager.InFlightFiles.Contains(virtualPath))
        {
            return;
        }

        ResourceManager.InFlightFiles.Add(virtualPath);

        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;
        var relativePath = ResourceLocator.GetRelativePath(curProject, virtualPath);

        IResourceLoadPipeline pipeline;

        if (relativePath == null || virtualPath == "null")
        {
            return;
        }

        if (!curProject.FS.FileExists(relativePath) && !virtualPath.Contains("direct"))
        {
            return;
        }

        if (virtualPath.EndsWith(".hkx"))
        {
            pipeline = _job.HavokCollisionLoadPipeline;
        }
        else if (relativePath.EndsWith(".tpf") || relativePath.EndsWith(".tpf.dcx"))
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

            _job.AddLoadTPFResources(new LoadTPFResourcesAction(
                _job, virt, relativePath, al));

            return;
        }
        else
        {
            pipeline = _job.FlverLoadPipeline;
        }

        pipeline.LoadFileResourceRequest.Post(new LoadFileResourceRequest(
            virtualPath, relativePath, al));
    }

    /// <summary>
    /// Loading singular file (no binder) via external path
    /// </summary>
    /// <param name="virtualPath"></param>
    /// <param name="al"></param>
    /// <param name="isPersistent"></param>
    public void AddExternalFileTask(string virtualPath, AccessLevel al, bool isPersistent = false)
    {
        // PIPELINE: resource is not already being loaded
        if (ResourceManager.InFlightFiles.Contains(virtualPath))
        {
            return;
        }

        ResourceManager.InFlightFiles.Add(virtualPath);

        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;
        var absPath = ResourceLocator.GetAbsolutePath(curProject, virtualPath);

        IResourceLoadPipeline pipeline;

        // PIPELINE: resource path is not invalid
        if (absPath == null || virtualPath == "null")
        {
            return;
        }

        // If file doesn't exist, return so we don't hang the resource loader.
        if (!File.Exists(absPath))
        {
            return;
        }

        if (absPath.EndsWith(".tpf") || absPath.EndsWith(".tpf.dcx"))
        {
            var virt = virtualPath;

            _job.AddLoadTPFResources(new LoadTPFResourcesAction(
                _job, virt, absPath, al));

            return;
        }
        else
        {
            pipeline = _job.FlverLoadPipeline;
        }

        pipeline.LoadFileResourceRequest.Post(new LoadFileResourceRequest(virtualPath, absPath, al));
    }

    /// <summary>
    /// Attempts to load unloaded resources (with active references) via UDSFM textures
    /// </summary>
    public void AddPostTextureLoadTask()
    {
        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        foreach (KeyValuePair<string, IResourceHandle> r in ResourceManager.ResourceDatabase)
        {
            if (!r.Value.IsLoaded())
            {
                var virtPath = r.Key;

                string path = null;

                // DS1 
                if (CFG.Current.MapEditor_TextureLoad_MapPieces)
                {
                    if (curProject.ProjectType is ProjectType.DS1)
                    {
                        if (virtPath.StartsWith("map/tex"))
                        {
                            if (curProject != null)
                            {
                                path = Path.Join(curProject.DataPath, "map", "tx", $"{Path.GetFileName(virtPath)}.tpf");
                            }
                        }

                        if (path != null && File.Exists(path))
                        {
                            _job.AddLoadTPFResources(new LoadTPFResourcesAction(_job,
                                Path.GetDirectoryName(r.Key.Replace('\\', Path.DirectorySeparatorChar)).Replace(Path.DirectorySeparatorChar, '/'),
                                path, AccessLevel.AccessGPUOptimizedOnly));
                        }
                    }
                }

                // AET loading after the flvers have been processed
                // This is required since lots of the AET listeners refer to AET ids
                // that aren't included in the default AEG texture load lists,
                // as some models make use of textures from others.
                if (CFG.Current.MapEditor_TextureLoad_Objects)
                {
                    if (curProject.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                    {
                        if (virtPath.Contains("aet"))
                        {
                            var parts = virtPath.Split('/');
                            if (parts.Length > 1)
                            {
                                var id = parts[1];

                                AddLoadFileTask($"aet/{id}/tex", AccessLevel.AccessGPUOptimizedOnly);
                            }
                        }
                    }
                }

                // Fill listeners that point to texbnd resources that aren't aligned with the chr model
                // e.g. chrbnd is c2025, texbnd is c2029
                if (CFG.Current.MapEditor_TextureLoad_Characters)
                {
                    if (virtPath.Contains("chr"))
                    {
                        var parts = virtPath.Split('/');
                        if (parts.Length > 1)
                        {
                            var id = parts[1];

                            AddLoadArchiveTask($"chr/{id}/tex", AccessLevel.AccessGPUOptimizedOnly, false);
                        }
                    }
                }
            }
        }
    }

    public void AddWorldMapLoadTask()
    {
        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        if (curProject == null)
            return;

        foreach (KeyValuePair<string, IResourceHandle> r in ResourceManager.ResourceDatabase)
        {
            if (!r.Value.IsLoaded())
            {
                var virtPath = r.Key;

                // Smithbox
                // For loading the world map TPFs
                if (curProject.ProjectType is ProjectType.ER or ProjectType.NR)
                {
                    if (virtPath.Contains("smithbox"))
                    {
                        AddExternalFileTask($"smithbox/world_map", AccessLevel.AccessGPUOptimizedOnly);
                    }
                }
            }
        }
    }


    public Task Complete()
    {
        // Build the job, register it with the task manager, and start it
        ResourceManager.ActiveJobProgress[_job] = 0;
        Task jobtask = _job.Complete();
        return jobtask;
    }
}