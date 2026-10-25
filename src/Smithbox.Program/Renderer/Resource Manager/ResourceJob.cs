using StudioCore.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Renderer;

/// <summary>
///     A named job that loads one or more containers and whose progress will appear in the progress window
/// </summary>
public class ResourceJob
{
    internal const int MaxResourceCommitsPerFrame = 64;

    private readonly ActionBlock<ContainerLoadAction> _loadContainers;
    private readonly BufferBlock<ResourceLoadedReply> _processedResources;
    private int _courseSize;
    private int TotalSize;

    public ResourceJob(string name)
    {
        Name = name;

        ExecutionDataflowBlockOptions options = new() { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded };

        _loadContainers = new ActionBlock<ContainerLoadAction>(a => a.Run(this), options);

        _processedResources = new BufferBlock<ResourceLoadedReply>();

        FlverLoadPipeline = new ResourceLoadPipeline<FlverResource>(_processedResources);
        HavokCollisionLoadPipeline = new ResourceLoadPipeline<HavokCollisionResource>(_processedResources);
        HavokNavmeshLoadPipeline = new ResourceLoadPipeline<HavokNavmeshResource>(_processedResources);
        NVMNavmeshLoadPipeline = new ResourceLoadPipeline<NVMNavmeshResource>(_processedResources);
        TPFTextureLoadPipeline = new TextureLoadPipeline(_processedResources);
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

    public int GetEstimateTaskSize()
    {
        return Math.Max(TotalSize, _courseSize);
    }

    internal void AddContainer(ContainerLoadAction action)
    {
        Interlocked.Increment(ref _courseSize);
        _loadContainers.Post(action);
    }

    /// <summary>
    ///     Hands finished resources to their pools. Runs on the main thread.
    /// </summary>
    public int ProcessLoadedResources()
    {
        var committed = 0;

        while (committed < MaxResourceCommitsPerFrame &&
               _processedResources.TryReceive(out ResourceLoadedReply p))
        {
            p.Context.Pool.Register(p.Context, p.Resource, p.AccessLevel);
            committed++;
        }

        Progress += committed;
        return committed;
    }

    public Task Complete()
    {
        return ResourceManager.JobTaskFactory.StartNew(() =>
        {
            // All containers have been read and their entries posted to the pipelines
            _loadContainers.Complete();
            _loadContainers.Completion.Wait();

            FlverLoadPipeline.LoadByteResourceBlock.Complete();
            FlverLoadPipeline.LoadFileResourceRequest.Complete();

            HavokCollisionLoadPipeline.LoadByteResourceBlock.Complete();
            HavokCollisionLoadPipeline.LoadFileResourceRequest.Complete();

            HavokNavmeshLoadPipeline.LoadByteResourceBlock.Complete();
            HavokNavmeshLoadPipeline.LoadFileResourceRequest.Complete();

            NVMNavmeshLoadPipeline.LoadByteResourceBlock.Complete();
            NVMNavmeshLoadPipeline.LoadFileResourceRequest.Complete();

            TPFTextureLoadPipeline.LoadTPFTextureResourceRequest.Complete();

            FlverLoadPipeline.LoadByteResourceBlock.Completion.Wait();
            FlverLoadPipeline.LoadFileResourceRequest.Completion.Wait();

            HavokCollisionLoadPipeline.LoadByteResourceBlock.Completion.Wait();
            HavokCollisionLoadPipeline.LoadFileResourceRequest.Completion.Wait();

            HavokNavmeshLoadPipeline.LoadByteResourceBlock.Completion.Wait();
            HavokNavmeshLoadPipeline.LoadFileResourceRequest.Completion.Wait();

            NVMNavmeshLoadPipeline.LoadByteResourceBlock.Completion.Wait();
            NVMNavmeshLoadPipeline.LoadFileResourceRequest.Completion.Wait();

            TPFTextureLoadPipeline.LoadTPFTextureResourceRequest.Completion.Wait();

            Smithbox.Log(this, LOC.Get("REND_Job_Finished", Name));
            Finished = true;
        });
    }
}
