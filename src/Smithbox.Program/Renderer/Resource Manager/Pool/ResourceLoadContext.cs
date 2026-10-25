namespace StudioCore.Renderer;

/// <summary>
///     Describes where a single resource came from while it is being loaded. This replaces the
///     "virtual path" string that used to be threaded through the loaders; a resource that needs to
///     know about its origin (e.g. a model that has to look up its textures) reads it from here.
/// </summary>
public sealed class ResourceLoadContext
{
    public ResourceLoadContext(ResourcePool pool, string entryPath, string containerPath, long order,
        bool isConnectCollision = false)
    {
        Pool = pool;
        EntryPath = entryPath ?? "";
        ContainerPath = containerPath ?? "";
        FileName = ResourceName.FileName(EntryPath);
        Name = ResourceName.Normalize(FileName);
        Order = order;
        IsConnectCollision = isConnectCollision;
    }

    /// <summary>The pool the resource is being loaded into. Dependencies (e.g. textures) are looked up here.</summary>
    public ResourcePool Pool { get; }

    /// <summary>Lookup name (see <see cref="ResourceName" />), e.g. "c1234".</summary>
    public string Name { get; }

    /// <summary>File name exactly as stored in the container, e.g. "h0000B0A10.hkx.dcx".</summary>
    public string FileName { get; }

    /// <summary>Path of the entry inside its container, or the file path for a loose file.</summary>
    public string EntryPath { get; }

    /// <summary>Path of the container that was passed to the pool. Empty for in-memory containers.</summary>
    public string ContainerPath { get; }

    /// <summary>
    ///     Priority of this resource inside the pool. Lower wins when two resources share a name:
    ///     containers loaded earlier beat containers loaded later, and earlier entries beat later
    ///     entries within one container.
    /// </summary>
    public long Order { get; }

    /// <summary>
    ///     The caller asked for this to be loaded as connect collision (see <see cref="LoadOptions.IsConnectCollision" />).
    /// </summary>
    public bool IsConnectCollision { get; }

    /// <summary>Human readable origin, used for logging and tooling.</summary>
    public string QualifiedName => $"{Pool?.Name}:{EntryPath}";
}
