using StudioCore.ProjectCore;
using System;

namespace StudioCore.Resource;

public interface IResource
{
    public bool _Load(Memory<byte> bytes, AccessLevel al, ProjectType type);
    public bool _Load(string file, AccessLevel al, ProjectType type);
}
