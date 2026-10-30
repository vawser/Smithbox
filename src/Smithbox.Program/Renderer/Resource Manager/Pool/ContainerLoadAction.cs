#nullable enable
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace StudioCore.Renderer;

internal enum ContainerOrigin
{
    /// <summary>Project-relative path in the virtual file system</summary>
    Vfs,

    /// <summary>Absolute path on disk</summary>
    External,

    /// <summary>Bytes supplied by the caller</summary>
    Memory
}

/// <summary>
///     Loads everything in one container into a pool: reads it, walks its entries (recursing into nested
///     containers), and posts each entry to the loader for its kind. Runs on a job worker thread.
/// </summary>
internal sealed class ContainerLoadAction
{
    private const int MaxNesting = 4;

    private readonly Memory<byte> _data;
    private readonly int _sequence;
    private readonly Action<bool> _onFinished;
    private int _entryCounter;
    private ResourceJob _job = null!;
    private ProjectEntry _project = null!;

    public ContainerLoadAction(ResourcePool pool, string path, ContainerOrigin origin, Memory<byte> data,
        LoadOptions options, int sequence, Action<bool> onFinished)
    {
        Pool = pool;
        Path = path;
        Origin = origin;
        _data = data;
        Options = options;
        _sequence = sequence;
        _onFinished = onFinished;
    }

    public ResourcePool Pool { get; }
    public string Path { get; }
    public ContainerOrigin Origin { get; }
    public LoadOptions Options { get; }

    public void Run(ResourceJob job)
    {
        _job = job;
        var success = false;

        ResourceLoadTracker.SetCurrentFile(ResourceName.FileName(Path));

        try
        {
            _project = Smithbox.Orchestrator.SelectedProject;
            if (_project != null)
            {
                success = LoadRoot();
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("REND_Binder_Load_Failed", Path, Path, Options.AccessLevel), e);
        }

        _onFinished(success);
    }

    private long NextOrder()
    {
        return ((long)_sequence << 32) | (uint)_entryCounter++;
    }

    private bool LoadRoot()
    {
        ContainerFormat format = ResourceClassifier.DetectContainer(Path);

        switch (format)
        {
            case ContainerFormat.Tpf:
            {
                if (!TryRead(Path, out Memory<byte> bytes))
                {
                    return false;
                }

                LoadTpf(TPF.Read(bytes));
                return true;
            }
            case ContainerFormat.Binder:
            {
                if (!TryRead(Path, out Memory<byte> bytes))
                {
                    return false;
                }

                using BinderReader binder = OpenBinder(bytes);
                LoadBinder(binder, Path, 0);
                return true;
            }
            case ContainerFormat.BinderSplit:
            {
                // The header is the container, the data file sits next to it
                var i = Path.LastIndexOf("bhd", StringComparison.OrdinalIgnoreCase);
                var dataPath = string.Concat(Path.AsSpan(0, i), "bdt", Path.AsSpan(i + 3));

                if (!TryRead(Path, out Memory<byte> bhd) || !TryRead(dataPath, out Memory<byte> bdt))
                {
                    return false;
                }

                using BinderReader binder = UsesLegacyBinders() ? new BXF3Reader(bhd, bdt) : new BXF4Reader(bhd, bdt);
                LoadBinder(binder, Path, 0);
                return true;
            }
            default:
                return LoadLooseFile();
        }
    }

    private bool LoadLooseFile()
    {
        ResourceKind kind = ResourceClassifier.Classify(Path, Path, true, Options.Types);
        if (kind == ResourceKind.None)
        {
            Smithbox.Log(this, $"Resource pool '{Pool.Name}': '{Path}' is not a supported container or resource file.");
            return false;
        }

        var ctx = new ResourceLoadContext(Pool, Path, Path, NextOrder(), Options.IsConnectCollision);
        if (!Options.Accepts(ctx.Name) || !WantsLoad(ctx, kind))
        {
            return true;
        }

        IResourceLoadPipeline pipeline = PipelineFor(kind);

        // Files in the project are read by the resource itself, which lets it apply project specific
        // handling of the file. Anything else is read here.
        if (Origin == ContainerOrigin.Vfs)
        {
            if (!_project.VFS.FS.FileExists(Path))
            {
                Smithbox.Log(this, $"Resource pool '{Pool.Name}': '{Path}' does not exist.");
                return false;
            }

            _job.IncrementEstimateTaskSize(1);
            pipeline.LoadFileResourceRequest.Post(new LoadFileResourceRequest(ctx, Path, Options.AccessLevel));
            return true;
        }

        if (!TryRead(Path, out Memory<byte> data))
        {
            return false;
        }

        _job.IncrementEstimateTaskSize(1);
        pipeline.LoadByteResourceBlock.Post(new LoadByteResourceRequest(ctx, data, Options.AccessLevel));
        return true;
    }

    private void LoadBinder(BinderReader binder, string containerPath, int depth)
    {
        for (var i = 0; i < binder.Files.Count(); i++)
        {
            BinderFileHeader header = binder.Files[i];
            var entryPath = header.Name ?? "";
            var fileName = ResourceName.FileName(entryPath);

            // Every entry takes a slot in the ordering, loaded or not, so priorities don't shift between loads
            var order = NextOrder();

            if (fileName.Length == 0)
            {
                continue;
            }

            try
            {
                switch (ResourceClassifier.DetectContainer(fileName))
                {
                    case ContainerFormat.Tpf:
                        if (Options.Types.HasFlag(ResourceType.Texture))
                        {
                            LoadTpf(TPF.Read(binder.ReadFile(header)));
                        }

                        continue;
                    case ContainerFormat.Binder when depth < MaxNesting:
                        using (BinderReader nested = OpenBinder(binder.ReadFile(header)))
                        {
                            LoadBinder(nested, containerPath, depth + 1);
                        }

                        continue;
                    case ContainerFormat.None:
                        break;
                    default:
                        continue;
                }

                ResourceKind kind = ResourceClassifier.Classify(fileName, containerPath, false, Options.Types);
                if (kind == ResourceKind.None)
                {
                    continue;
                }

                var ctx = new ResourceLoadContext(Pool, entryPath, containerPath, order, Options.IsConnectCollision);
                if (!Options.Accepts(ctx.Name) || !WantsLoad(ctx, kind))
                {
                    continue;
                }

                // Reading decompresses the entry, so it is only done for entries that are going to be used
                Memory<byte> data = binder.ReadFile(header);
                _job.IncrementEstimateTaskSize(1);
                PipelineFor(kind).LoadByteResourceBlock.Post(new LoadByteResourceRequest(ctx, data, Options.AccessLevel));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("REND_Binder_TPF_Load_Failed", fileName, containerPath, Path,
                    Options.AccessLevel), e);
            }
        }
    }

    private void LoadTpf(TPF tpf)
    {
        if (!Options.Types.HasFlag(ResourceType.Texture))
        {
            return;
        }

        for (var i = 0; i < tpf.Textures.Count; i++)
        {
            // Textures are named by the texture, not by the TPF they are packed in
            var ctx = new ResourceLoadContext(Pool, tpf.Textures[i].Name, Path, NextOrder());
            if (!Options.Accepts(ctx.Name) ||
                !Pool.ShouldLoad(ctx.Name, typeof(TextureResource), ctx.Order, Options.AccessLevel, Options.Reload))
            {
                continue;
            }

            _job.IncrementEstimateTaskSize(1);
            _job.TPFTextureLoadPipeline.LoadTPFTextureResourceRequest.Post(
                new LoadTPFTextureResourceRequest(ctx, tpf, i, Options.AccessLevel));
        }
    }

    private bool WantsLoad(ResourceLoadContext ctx, ResourceKind kind)
    {
        return Pool.ShouldLoad(ctx.Name, ResourceClassifier.ResourceTypeOf(kind)!, ctx.Order, Options.AccessLevel,
            Options.Reload);
    }

    private IResourceLoadPipeline PipelineFor(ResourceKind kind)
    {
        return kind switch
        {
            ResourceKind.Flver => _job.FlverLoadPipeline,
            ResourceKind.Collision => _job.HavokCollisionLoadPipeline,
            ResourceKind.NavmeshNVM => _job.NVMNavmeshLoadPipeline,
            ResourceKind.NavmeshHKX => _job.HavokNavmeshLoadPipeline,
            _ => throw new InvalidOperationException()
        };
    }

    private bool UsesLegacyBinders()
    {
        return _project.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R;
    }

    private BinderReader OpenBinder(Memory<byte> bytes)
    {
        return UsesLegacyBinders() ? new BND3Reader(bytes) : new BND4Reader(bytes);
    }

    private bool TryRead(string path, out Memory<byte> data)
    {
        data = default;

        try
        {
            switch (Origin)
            {
                case ContainerOrigin.Memory:
                    data = _data;
                    return true;
                case ContainerOrigin.External:
                    if (!File.Exists(path))
                    {
                        Smithbox.Log(this, $"Resource pool '{Pool.Name}': '{path}' does not exist.");
                        return false;
                    }

                    data = File.ReadAllBytes(path);
                    return true;
                default:
                    if (!_project.VFS.FS.FileExists(path))
                    {
                        Smithbox.Log(this, $"Resource pool '{Pool.Name}': '{path}' does not exist.");
                        return false;
                    }

                    var fileData = _project.VFS.FS.ReadFile(path);

                    if (fileData != null)
                    {
                        data = (Memory<byte>)fileData;
                    }
                    return true;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("REND_Resource_Load_Failed", path), e);
            return false;
        }
    }
}
