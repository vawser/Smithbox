using StudioCore.UserProject;
using System;

namespace StudioCore.Resource;

public interface IResource
{
    public bool _Load(Memory<byte> bytes, AccessLevel al);
    public bool _Load(string file, AccessLevel al);
}
