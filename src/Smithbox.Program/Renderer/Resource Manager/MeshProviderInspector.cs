using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

public static class MeshProviderInspector
{
    public static Dictionary<int, MeshProviderContext> Providers = new();

    public static void Add(string virtPath, MeshProvider provider)
    {
        var hash = virtPath.GetHashCode();

        if(!Providers.ContainsKey(hash))
        {
            var context = new MeshProviderContext(virtPath, provider);

            Providers.Add(hash, context);
        }
    }
    public static void Remove(string virtPath)
    {
        var hash = virtPath.GetHashCode();

        if (Providers.ContainsKey(hash))
        {
            Providers.Remove(hash);
        }
    }
}

public class MeshProviderContext
{
    public string VirtualPath { get; set; }

    public MeshProvider Provider { get; set; }

    public MeshProviderContext(string virtualPath, MeshProvider provider)
    {
        VirtualPath = virtualPath;
        Provider = provider;
    }
}