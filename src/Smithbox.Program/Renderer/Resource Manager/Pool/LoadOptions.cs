using System.Collections.Generic;

namespace StudioCore.Renderer;

/// <summary>
///     Optional settings for loading a container into a <see cref="ResourcePool" />.
///     The defaults load everything in the container for GPU use.
/// </summary>
public sealed class LoadOptions
{
    private HashSet<string> _onlyNames;

    /// <summary>How the loaded resources will be accessed.</summary>
    public AccessLevel AccessLevel { get; set; } = AccessLevel.AccessGPUOptimizedOnly;

    /// <summary>Restrict loading to certain kinds of resource.</summary>
    public ResourceType Types { get; set; } = ResourceType.All;

    /// <summary>
    ///     If set, only entries whose name is in this set are loaded. Useful for very large containers
    ///     (e.g. a map's model archive) when only a few models are needed. Names are normalized
    ///     with <see cref="ResourceName.Normalize" />, so file names or paths can be supplied as they are.
    /// </summary>
    public IEnumerable<string> OnlyNames
    {
        get => _onlyNames;
        set
        {
            if (value == null)
            {
                _onlyNames = null;
                return;
            }

            _onlyNames = new HashSet<string>();
            foreach (var n in value)
            {
                _onlyNames.Add(ResourceName.Normalize(n));
            }
        }
    }

    /// <summary>
    ///     By default a resource that is already loaded in the pool (from this container, or from one that
    ///     was added earlier) is not loaded again. Set to true to replace resources that are already loaded.
    /// </summary>
    public bool Reload { get; set; }

    /// <summary>
    ///     Load collision as connect collision. Connect collisions are stored in the same files as normal low
    ///     collisions, so nothing in the file says which one it is. Load them into a separate pool
    ///     (<see cref="ResourceManager.ConnectCollisionPool" />) so they don't share names with normal collision.
    /// </summary>
    public bool IsConnectCollision { get; set; }

    /// <summary>Name shown in the resource load window. Defaults to the container path.</summary>
    public string JobName { get; set; }

    public LoadOptions Clone()
    {
        var clone = (LoadOptions)MemberwiseClone();
        clone.OnlyNames = _onlyNames;
        return clone;
    }

    internal bool Accepts(string normalizedName)
    {
        return _onlyNames == null || _onlyNames.Contains(normalizedName);
    }
}
