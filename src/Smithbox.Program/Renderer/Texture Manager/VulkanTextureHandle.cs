using SoulsFormats;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

public class VulkanTextureHandle : ITextureHandle
{
    public TexturePool.TextureHandle Handle { get; set; }
    public uint Width { get; set; }
    public uint Height { get; set; }

    public VulkanTextureHandle() { }

    public void Load(TPF tpf, int index)
    {
        if (TexturePool.TextureHandle.IsTPFCube(tpf.Textures[index], tpf.Platform))
        {
            Handle = SceneRenderer.GlobalCubeTexturePool.AllocateTextureDescriptor();
        }
        else
        {
            Handle = SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();
        }

        if (Handle == null)
        {
            ResourceLog.AddLog("Unable to allocate texture descriptor");
            return;
        }

        if (tpf.Platform == TPF.TPFPlatform.PC || tpf.Platform == TPF.TPFPlatform.PS3)
        {
            SceneRenderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
            {
                if (Handle == null)
                {
                    return;
                }

                if (index < tpf.Textures.Count)
                {
                    if (tpf.Textures[index] != null)
                    {
                        try
                        {
                            Handle.FillWithTPF(
                                d, cl, tpf.Platform, tpf.Textures[index], tpf.Textures[index].Name);

                            Width = Handle.Width;
                            Height = Handle.Height;
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddError("Failed to fill TPF", ex);
                        }
                    }
                }
            });
        }
    }

    public void Dispose()
    {
        Handle.Dispose();
    }
}
