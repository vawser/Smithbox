using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static StudioCore.Resource.ResourceManager;

namespace StudioCore.Resource;

public class LoadBinderResourcesAction
{
    public readonly object ProgressLock = new();
    public ResourceJob _job;
    public AccessLevel AccessLevel = AccessLevel.AccessGPUOptimizedOnly;
    public HashSet<string> AssetWhitelist;
    public RefCount<BinderReader> Binder;
    public HashSet<int> BinderLoadMask = null;
    public string BinderVirtualPath;
    public string BinderRelativePath;
    public List<Task> LoadingTasks = new();

    public List<Tuple<IResourceLoadPipeline, string, RefCount<BinderFileHeader>>> PendingResources = new();
    public List<Tuple<string, RefCount<BinderFileHeader>, bool>> PendingTPFs = new();
    public bool PopulateResourcesOnly;
    public ResourceType ResourceMask = ResourceType.All;
    public List<int> TaskProgress = new();
    public List<int> TaskSizes = new();
    public int TotalSize = 0;

    public bool PersistentTPF = false;

    public LoadBinderResourcesAction(ResourceJob job, string virtpath, AccessLevel accessLevel, bool populateOnly, ResourceType mask, HashSet<string> whitelist, bool isPersistentTPF = false)
    {
        _job = job;
        BinderVirtualPath = virtpath;
        PopulateResourcesOnly = populateOnly;
        ResourceMask = mask;
        AssetWhitelist = whitelist;
        AccessLevel = accessLevel;
        PersistentTPF = isPersistentTPF;
    }

    public void ProcessBinder()
    {
        var curProject = BaseEditor.ProjectManager.SelectedProject;

        // Read binder
        if (Binder == null)
        {
            BinderRelativePath = ResourceLocator.GetRelativePath(curProject, BinderVirtualPath);

            var load = true;

            var targetPath = BinderRelativePath;

            if (targetPath == null || targetPath == "")
                return;

            if (targetPath.EndsWith("bhd"))
            {
                Memory<byte> bhd = new Memory<byte>();
                Memory<byte> bdt = new Memory<byte>();

                var targetBhdPath = targetPath;
                var targetBdtPath = targetPath.Replace("bhd", "bdt");

                foreach (var entry in curProject.FileDictionary.Entries)
                {
                    if (entry.Path == targetBhdPath)
                    {
                        try
                        {
                            bhd = (Memory<byte>)curProject.FS.ReadFile(entry.Path);
                        }
                        catch (Exception e)
                        {
                            load = false;
                            ResourceLog.AddLog($"[Smithbox:DEBUG] Failed to read {entry.Path} during resource load.", LogLevel.Error);
                        }

                        break;
                    }
                }

                foreach (var entry in curProject.FileDictionary.Entries)
                {
                    if (entry.Path == targetBdtPath)
                    {
                        try
                        {
                            bdt = (Memory<byte>)curProject.FS.ReadFile(entry.Path);
                        }
                        catch (Exception e)
                        {
                            load = false;
                            ResourceLog.AddLog($"[Smithbox:DEBUG] Failed to read {entry.Path} during resource load.", LogLevel.Error);
                        }

                        break;
                    }
                }

                if (bhd.Length != 0 && bdt.Length != 0)
                {
                    if (curProject.ProjectType is ProjectType.DES
                        or ProjectType.DS1
                        or ProjectType.DS1R)
                    {
                        Binder = new(new BXF3Reader(bhd, bdt));
                    }
                    else
                    {
                        Binder = new(new BXF4Reader(bhd, bdt));
                    }
                }
            }
            else
            {
                Memory<byte> binder = new Memory<byte>();

                foreach (var entry in curProject.FileDictionary.Entries)
                {
                    if (entry.Path == targetPath)
                    {
                        try
                        {
                            binder = (Memory<byte>)curProject.FS.ReadFile(entry.Path);
                        }
                        catch (Exception e)
                        {
                            load = false;
                            ResourceLog.AddLog($"[Smithbox:DEBUG] Failed to read {entry.Path} during resource load.", LogLevel.Error);
                        }

                        break;
                    }
                }

                if (load && binder.Length != 0)
                {
                    if (curProject.ProjectType is ProjectType.DES
                        or ProjectType.DS1
                        or ProjectType.DS1R)
                    {
                        Binder = new(new BND3Reader(binder));
                    }
                    else
                    {
                        Binder = new(new BND4Reader(binder));
                    }
                }
            }
        }

        if (Binder == null)
            return;

        var b = Binder.Value;

        if (b == null)
            return;

        // Iterate through each file in the binder
        for (var i = 0; i < b.Files.Count(); i++)
        {
            var f = new ChildResource<BinderReader, BinderFileHeader>(Binder.Ref(), b.Files[i]);

            // Skip entry if entry ID is not in binder load mask (if defined)
            if (BinderLoadMask != null && !BinderLoadMask.Contains(i))
            {
                continue;
            }

            // Append internal filename to the BinderVirtualPath
            var curFileBinderPath = BinderVirtualPath;
            var curBinderFilename = Path.GetFileNameWithoutExtension($@"{f.Value.Name.Replace('\\', Path.DirectorySeparatorChar)}.blah");

            if (curBinderFilename.Length > 0)
                curFileBinderPath = $@"{BinderVirtualPath}/{curBinderFilename}";

            // Skip entry if entry Path is not in AssetWhitelist
            if (AssetWhitelist != null && !AssetWhitelist.Contains(curFileBinderPath))
            {
                continue;
            }

            IResourceLoadPipeline pipeline = null;

            // TPF
            if (LocatorUtils.IsTPF(curFileBinderPath))
            {
                var bndvirt = BinderVirtualPath;

                // Handles the 4 TPFBHDs used to hold map textures
                // e.g. map/tex/m10_00_00_00/0001
                if (bndvirt.StartsWith(@"map/tex"))
                {
                    Regex regex = new(@"\d{4}$");
                    if (regex.IsMatch(bndvirt))
                    {
                        bndvirt = bndvirt.Substring(0, bndvirt.Length - 5);
                    }
                    else if (bndvirt.EndsWith("tex"))
                    {
                        bndvirt = bndvirt.Substring(0, bndvirt.Length - 4);
                    }
                }

                PendingTPFs.Add((bndvirt, (RefCount<BinderFileHeader>)f, PersistentTPF).ToTuple());

                //ResourceLog.AddLog($"ProcessBinder - PendingTPFs: {curFileBinderPath}");
            }
            else
            {
                // FLVER
                if (ResourceMask.HasFlag(ResourceType.Flver))
                {
                    if (LocatorUtils.IsFLVER(curFileBinderPath))
                    {
                        //handle = new ResourceHandle<FlverResource>();
                        pipeline = _job.FlverLoadPipeline;

                        //ResourceLog.AddLog($"ProcessBinder - FlverLoadPipeline: {curFileBinderPath}");
                    }
                }

                // NAVMESH
                if (ResourceMask.HasFlag(ResourceType.Navmesh))
                {
                    if (LocatorUtils.IsNavmesh(curFileBinderPath))
                    {
                        pipeline = _job.NVMNavmeshLoadPipeline;

                        //ResourceLog.AddLog($"ProcessBinder - NVMNavmeshLoadPipeline: {curFileBinderPath}");
                    }
                }

                // HAVOK NAVMESH
                if (ResourceMask.HasFlag(ResourceType.NavmeshHKX))
                {
                    if (LocatorUtils.IsHavokNavmesh(BinderVirtualPath, curFileBinderPath))
                    {
                        pipeline = _job.HavokNavmeshLoadPipeline;

                        //ResourceLog.AddLog($"ProcessBinder - HavokNavmeshLoadPipeline: {curFileBinderPath}");
                    }
                }

                // HAVOK COLLISION
                if (ResourceMask.HasFlag(ResourceType.CollisionHKX))
                {
                    if (LocatorUtils.IsHavokCollision(BinderVirtualPath, curFileBinderPath))
                    {
                        pipeline = _job.HavokCollisionLoadPipeline;

                        //ResourceLog.AddLog($"ProcessBinder - HavokCollisionLoadPipeline: {curFileBinderPath}");
                    }
                }

                // Send pipeline (if valid)
                if (pipeline != null)
                {

                    PendingResources.Add((pipeline, curFileBinderPath, (RefCount<BinderFileHeader>)f).ToTuple());

                    //ResourceLog.AddLog($"ProcessBinder - PendingResources: {curFileBinderPath}");
                }
            }
        }

        Binder.Dispose();
    }
}