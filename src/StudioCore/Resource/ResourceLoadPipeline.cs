using SoulsFormats;
using StudioCore.Resource.Types;
using StudioCore.Tasks;
using System;
using System.IO;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Resource;

public readonly record struct LoadByteResourceRequest(
    string VirtualPath,
    RefCount<Memory<byte>> Data,
    AccessLevel AccessLevel);

public readonly record struct LoadFileResourceRequest(
    string VirtualPath,
    string File,
    AccessLevel AccessLevel);

public readonly record struct LoadTPFTextureResourceRequest(
    string VirtualPath,
    TPF Tpf,
    int Index,
    AccessLevel AccessLevel);

public readonly record struct ResourceLoadedReply(
    string VirtualPath,
    AccessLevel AccessLevel,
    IResource Resource);

public interface IResourceLoadPipeline
{
    public ITargetBlock<LoadByteResourceRequest> LoadByteResourceBlock { get; }
    public ITargetBlock<LoadFileResourceRequest> LoadFileResourceRequest { get; }
    public ITargetBlock<LoadTPFTextureResourceRequest> LoadTPFTextureResourceRequest { get; }
}

// PIPELINE: resolve processed resources for:
// - FlverResource
// - HavokCollisionResource
// - HavokNavmeshResource
// - NVMNavmeshResource
public class ResourceLoadPipeline<T> : IResourceLoadPipeline where T : class, IResource, new()
{
    private readonly ActionBlock<LoadByteResourceRequest> _loadByteResourcesTransform;

    private readonly ITargetBlock<ResourceLoadedReply> _loadedResources;
    private readonly ActionBlock<LoadFileResourceRequest> _loadFileResourcesTransform;

    public ResourceLoadPipeline(ITargetBlock<ResourceLoadedReply> target)
    {
        var options = new ExecutionDataflowBlockOptions();
        options.MaxDegreeOfParallelism = 6;
        _loadedResources = target;

        // PIPELINE: Byte Requests
        _loadByteResourcesTransform = new ActionBlock<LoadByteResourceRequest>(r =>
        {
            try
            {
                var res = new T();

                // PIPELINE: Load the byte resource (as the <T> type)
                var success = res._Load(r.Data.Value, r.AccessLevel, r.VirtualPath);

                // PIPELINE: If resource is loaded successful, add reply to Loaded Resource block
                if (success)
                {
                    var request = new ResourceLoadedReply(r.VirtualPath, r.AccessLevel, res);

                    _loadedResources.Post(request);
                }
            }
            catch(Exception ex)
            {
                TaskLogs.AddLog($"Resource pipeline load error:\nFile path request\nValue: {r.Data.Value}\nAccess Level: {r.AccessLevel}\nVirtual Path: {r.VirtualPath}\n{ex}", Microsoft.Extensions.Logging.LogLevel.Warning, LogPriority.Low);
            }
            r.Data.Dispose();
        }, options);

        // PIPELINE: File Requests
        _loadFileResourcesTransform = new ActionBlock<LoadFileResourceRequest>(r =>
        {
            try
            {
                var res = new T();

                // PIPELINE: Load the byte resource (as the <T> type)
                var success = res._Load(r.File, r.AccessLevel, r.VirtualPath);

                // PIPELINE: If resource is loaded successful, add reply to Loaded Resource block
                if (success)
                {
                    var request = new ResourceLoadedReply(r.VirtualPath, r.AccessLevel, res);

                    _loadedResources.Post(request);
                }
            }
            catch (FileNotFoundException e1)
            {
                TaskLogs.AddLog($"Resource pipeline load error:\nFile bytes request\nAccess Level: {r.AccessLevel}\nVirtual Path: {r.VirtualPath}\n{e1}", Microsoft.Extensions.Logging.LogLevel.Warning, LogPriority.Low);
            }
            catch (DirectoryNotFoundException e2)
            {
                TaskLogs.AddLog($"Resource pipeline load error:\nFile bytes request\nAccess Level: {r.AccessLevel}\nVirtual Path: {r.VirtualPath}\n{e2}", Microsoft.Extensions.Logging.LogLevel.Warning, LogPriority.Low);
            }
            // Some DSR FLVERS can't be read due to mismatching layout and vertex sizes
            catch (InvalidDataException e3)
            {
                TaskLogs.AddLog($"Resource pipeline load error:\nFile bytes request\nAccess Level: {r.AccessLevel}\nVirtual Path: {r.VirtualPath}\n{e3}", Microsoft.Extensions.Logging.LogLevel.Warning, LogPriority.Low);
            }
        }, options);

    }

    public ITargetBlock<LoadByteResourceRequest> LoadByteResourceBlock => _loadByteResourcesTransform;
    public ITargetBlock<LoadFileResourceRequest> LoadFileResourceRequest => _loadFileResourcesTransform;

    public ITargetBlock<LoadTPFTextureResourceRequest> LoadTPFTextureResourceRequest =>
        throw new NotImplementedException();
}

public class TextureLoadPipeline : IResourceLoadPipeline
{
    private readonly ITargetBlock<ResourceLoadedReply> _loadedResources;

    private readonly ActionBlock<LoadTPFTextureResourceRequest> _loadTPFResourcesTransform;

    public TextureLoadPipeline(ITargetBlock<ResourceLoadedReply> target)
    {
        var options = new ExecutionDataflowBlockOptions();
        options.MaxDegreeOfParallelism = 6;
        _loadedResources = target;
        _loadTPFResourcesTransform = new ActionBlock<LoadTPFTextureResourceRequest>(r =>
        {
            var res = new TextureResource(r.Tpf, r.Index);
            var success = res._LoadTexture(r.AccessLevel);
            if (success)
            {
                _loadedResources.Post(new ResourceLoadedReply(r.VirtualPath, r.AccessLevel, res));
            }
        }, options);
    }

    public ITargetBlock<LoadByteResourceRequest> LoadByteResourceBlock => throw new NotImplementedException();
    public ITargetBlock<LoadFileResourceRequest> LoadFileResourceRequest => throw new NotImplementedException();

    public ITargetBlock<LoadTPFTextureResourceRequest> LoadTPFTextureResourceRequest =>
        _loadTPFResourcesTransform;
}
