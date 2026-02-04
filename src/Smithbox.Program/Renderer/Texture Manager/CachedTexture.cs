using Silk.NET.OpenGL;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Renderer;
public class CachedTexture
{
    public SubTexture SubTexture { get; set; }

    public ITextureHandle Handle { get; set; }

    public CachedTexture(SubTexture subTexture)
    {
        SubTexture = subTexture;
    }

    public void Load(TPF tpf, int index)
    {
        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
        {
            Handle = new VulkanTextureHandle();
            Handle.Load(tpf, index);
        }

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
        {
            // TODO: need to handle BC7 properly.
            //Handle = new OpenGLTextureHandle();
            //Handle.Load(tpf, index);
        }
    }

    public void Dispose()
    {
        Handle.Dispose();
    }
}
