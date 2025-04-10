using SoulsFormats;

namespace StudioCore.Editors.TimeActEditor.Bank;

/// <summary>
/// Wrapper class for the top-level binder. Holds the actual file container, and then the internal file container (if applicable).
/// </summary>
public class TimeActBinderWrapper
{
    public IBinder ContainerBinder { get; set; }
    public IBinder InternalBinder { get; set; }
    public string InternalBinderName { get; set; }

    public TimeActBinderWrapper(IBinder containerBinder, IBinder internalBinder, string internalBinderName)
    {
        ContainerBinder = containerBinder;
        InternalBinder = internalBinder;
        InternalBinderName = internalBinderName;
    }
}