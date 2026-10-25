using System;

namespace StudioCore.Renderer;

/// <summary>
/// Request to be told (on the main thread) when a resource is loaded
/// </summary>
/// <param name="Handle"></param>
/// <param name="Listener"></param>
/// <param name="AccessLevel"></param>
/// <param name="tag"></param>
public readonly record struct AddResourceLoadNotificationRequest(
        IResourceHandle Handle,
        IResourceEventListener Listener,
        AccessLevel AccessLevel,
        int tag);

/// <summary>
/// Unload Request
/// </summary>
/// <param name="Resource"></param>
/// <param name="UnloadOnlyIfUnused"></param>
public readonly record struct UnloadResourceRequest(
        IResourceHandle Resource,
        bool UnloadOnlyIfUnused);
