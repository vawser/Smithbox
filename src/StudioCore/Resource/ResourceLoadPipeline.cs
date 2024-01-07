using SoulsFormats;
using System;
using System.IO;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Resource;

public readonly record struct LoadByteResourceRequest(
    string VirtualPath,
    Memory<byte> Data,
    AccessLevel AccessLevel,
    GameType GameType);

public readonly record struct LoadFileResourceRequest(
    string VirtualPath,
    string File,
    AccessLevel AccessLevel,
    GameType GameType);

public readonly record struct LoadTPFTextureResourceRequest(
    string VirtualPath,
    TPF Tpf,
    int Index,
    AccessLevel AccessLevel,
    GameType GameType);

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
        _loadByteResourcesTransform = new ActionBlock<LoadByteResourceRequest>(r =>
        {
            var res = new T();
            var success = res._Load(r.Data, r.AccessLevel, r.GameType);
            if (success)
            {
                _loadedResources.Post(new ResourceLoadedReply(r.VirtualPath, r.AccessLevel, res));
            }
        }, options);
        _loadFileResourcesTransform = new ActionBlock<LoadFileResourceRequest>(r =>
        {
            try
            {
                var res = new T();
                var success = res._Load(r.File, r.AccessLevel, r.GameType);
                if (success)
                {
                    _loadedResources.Post(new ResourceLoadedReply(r.VirtualPath, r.AccessLevel, res));
                }
            }
            catch (FileNotFoundException e1) { TaskLogs.AddLog("Resource load error", Microsoft.Extensions.Logging.LogLevel.Warning, TaskLogs.LogPriority.Low, e1); }
            catch (DirectoryNotFoundException e2) { TaskLogs.AddLog("Resource load error", Microsoft.Extensions.Logging.LogLevel.Warning, TaskLogs.LogPriority.Low, e2); }
            // Some DSR FLVERS can't be read due to mismatching layout and vertex sizes
            catch (InvalidDataException e3) { TaskLogs.AddLog("Resource load error", Microsoft.Extensions.Logging.LogLevel.Warning, TaskLogs.LogPriority.Low, e3); }
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
