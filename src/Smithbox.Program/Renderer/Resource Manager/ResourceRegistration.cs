using System.Collections.Generic;

namespace StudioCore.Renderer;

/// <summary>
/// Resource registration
/// </summary>
public class ResourceRegistration
{
    public ResourceRegistration(AccessLevel al)
    {
        AccessLevel = al;
    }

    public IResourceHandle Handle { get; set; } = null;
    public AccessLevel AccessLevel { get; set; }

    public List<AddResourceLoadNotificationRequest> NotificationRequests { get; set; } = new();
}