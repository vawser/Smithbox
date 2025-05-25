namespace StudioCore.Resource;

/// <summary>
///     Implementors of this interface can subscribe to a resource handle to be notified of resource load/unload events
/// </summary>
public interface IResourceEventListener
{
    public void OnResourceLoaded(IResourceHandle handle, int tag);
    public void OnResourceUnloaded(IResourceHandle handle, int tag);
}
