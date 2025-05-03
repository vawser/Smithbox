using System;

namespace StudioCore.Resource;

public interface IResource
{
    public bool _Load(Memory<byte> bytes, AccessLevel al, string virtPath);
    public bool _Load(string file, AccessLevel al, string virtPath);
}
