using SoulsFormats;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Renderer;

public readonly record struct LoadByteResourceRequest(
    ResourceLoadContext Context,
    Memory<byte> Data,
    AccessLevel AccessLevel);

public readonly record struct LoadFileResourceRequest(
    ResourceLoadContext Context,
    string File,
    AccessLevel AccessLevel);

public readonly record struct LoadTPFTextureResourceRequest(
    ResourceLoadContext Context,
    TPF Tpf,
    int Index,
    AccessLevel AccessLevel);

public readonly record struct ResourceLoadedReply(
    ResourceLoadContext Context,
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

        // Byte Requests
        _loadByteResourcesTransform = new ActionBlock<LoadByteResourceRequest>(r =>
        {
            try
            {
                var res = new T();

                var success = res._Load(r.Data, r.AccessLevel, r.Context);

                if (success)
                {
                    var request = new ResourceLoadedReply(r.Context, r.AccessLevel, res);

                    _loadedResources.Post(request);
                }
            }
            catch(Exception ex)
            {
                Smithbox.LogError(this, 
                    LOC.Get("REND_Resource_Pipeline_Load_Error_File_Path",
                    r.Data, r.Context.QualifiedName, r.AccessLevel), ex);
            }

        }, options);

        // File Requests
        _loadFileResourcesTransform = new ActionBlock<LoadFileResourceRequest>(r =>
        {
            try
            {
                var res = new T();

                var success = res._Load(r.File, r.AccessLevel, r.Context);

                if (success)
                {
                    var request = new ResourceLoadedReply(r.Context, r.AccessLevel, res);

                    _loadedResources.Post(request);
                }
            }
            catch (FileNotFoundException e1)
            {
                Smithbox.LogError(this, 
                    LOC.Get("REND_Resource_Pipeline_Load_Error_File_Data", 
                    r.Context.QualifiedName, r.AccessLevel), e1);
            }
            catch (DirectoryNotFoundException e2)
            {
                Smithbox.LogError(this,
                    LOC.Get("REND_Resource_Pipeline_Load_Error_File_Data",
                    r.Context.QualifiedName, r.AccessLevel), e2);
            }
            // Some DSR FLVERS can't be read due to mismatching layout and vertex sizes
            catch (InvalidDataException e3)
            {
                Smithbox.LogError(this,
                    LOC.Get("REND_Resource_Pipeline_Load_Error_File_Data",
                    r.Context.QualifiedName, r.AccessLevel), e3);
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
                _loadedResources.Post(
                    new ResourceLoadedReply(r.Context, r.AccessLevel, res));
            }
        }, options);
    }

    public ITargetBlock<LoadByteResourceRequest> LoadByteResourceBlock => throw new NotImplementedException();
    public ITargetBlock<LoadFileResourceRequest> LoadFileResourceRequest => throw new NotImplementedException();

    public ITargetBlock<LoadTPFTextureResourceRequest> LoadTPFTextureResourceRequest =>
        _loadTPFResourcesTransform;
}
