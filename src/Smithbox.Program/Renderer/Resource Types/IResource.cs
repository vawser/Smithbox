using System;

namespace StudioCore.Renderer;

public interface IResource
{
    public bool _Load(Memory<byte> bytes, AccessLevel al, ResourceLoadContext ctx);
    public bool _Load(string file, AccessLevel al, ResourceLoadContext ctx);
}
