namespace StudioCore.Renderer;

public interface IMeshProviderEventListener
{
    public void OnProviderAvailable();

    public void OnProviderUnavailable();
}