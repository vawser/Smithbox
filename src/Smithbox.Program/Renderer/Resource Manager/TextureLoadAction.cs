using SoulsFormats;

namespace StudioCore.Renderer;

public struct LoadTPFResourcesAction
{
    /// <summary>
    /// Job this action belongs to.
    /// </summary>
    public ResourceJob _job;

    /// <summary>
    /// Virtual resource path used for this TPF
    /// </summary>
    public string _virtualPath = null;

    /// <summary>
    /// TPF container
    /// </summary>
    public TPF _tpf = null;

    /// <summary>
    /// Absolute resource path used for this TPF
    /// </summary>
    public string _filePath = null;

    /// <summary>
    /// Resource access level
    /// </summary>
    public AccessLevel _accessLevel = AccessLevel.AccessGPUOptimizedOnly;

    public LoadTPFResourcesAction(ResourceJob job, string virtualPath, TPF tpf, AccessLevel al)
    {
        _job = job;
        _virtualPath = virtualPath;
        _tpf = tpf;
        _accessLevel = al;
    }

    public LoadTPFResourcesAction(ResourceJob job, string virtualPath, string filePath, AccessLevel al)
    {
        _job = job;
        _virtualPath = virtualPath;
        _filePath = filePath;
        _accessLevel = al;
    }
}