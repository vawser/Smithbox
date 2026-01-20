using SoulsFormats;
using StudioCore.Editors.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Renderer;
public class CachedTexture
{
    public SubTexture SubTexture { get; set; }
    public nint Handle { get; set; }

    // TEMP
    public TexturePool.TextureHandle OldHandle { get; set; }

    public CachedTexture(SubTexture subTexture)
    {
        SubTexture = subTexture;
    }
    public void Load(TPF tpf, int index)
    {
        if (TexturePool.TextureHandle.IsTPFCube(tpf.Textures[index], tpf.Platform))
        {
            OldHandle = SceneRenderer.GlobalCubeTexturePool.AllocateTextureDescriptor();
        }
        else
        {
            OldHandle = SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();
        }

        if (OldHandle == null)
        {
            ResourceLog.AddLog("Unable to allocate texture descriptor");
            return;
        }

        if (tpf.Platform == TPF.TPFPlatform.PC || tpf.Platform == TPF.TPFPlatform.PS3)
        {
            SceneRenderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
            {
                if (OldHandle == null)
                {
                    return;
                }

                if (index < tpf.Textures.Count)
                {
                    if (tpf.Textures[index] != null)
                    {
                        try
                        {
                            OldHandle.FillWithTPF(
                                d, cl, tpf.Platform, tpf.Textures[index], tpf.Textures[index].Name);
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
}
