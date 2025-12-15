using System;

namespace StudioCore.Renderer;

/// <summary>
/// Load Request
/// </summary>
/// <param name="ResourceVirtualPath"></param>
/// <param name="Type"></param>
/// <param name="Listener"></param>
/// <param name="AccessLevel"></param>
/// <param name="tag"></param>
public readonly record struct AddResourceLoadNotificationRequest(
        string ResourceVirtualPath,
        Type Type,
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