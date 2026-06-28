using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.TextureViewer;
using System;

namespace StudioCore.Renderer;

public class TextureResource : IResource, IDisposable
{
    private readonly int TPFIndex;

    public TextureResource()
    {
        throw new Exception(LOC.Get("REND_TextureResource_Bad_Init"));
    }

    public TextureResource(TPF tex, int index)
    {
        Texture = tex;
        TPFIndex = index;
    }

    public TPF Texture { get; private set; }

    public TexturePool.TextureHandle GPUTexture { get; private set; }

    public bool _LoadTexture(AccessLevel al)
    {
        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
        {
            return LoadVulkanTexture(al);
        }

        return false;
    }

    public bool LoadVulkanTexture(AccessLevel al)
    {
        if (TexturePool.TextureHandle.IsTPFCube(Texture.Textures[TPFIndex], Texture.Platform))
        {
            GPUTexture = SceneRenderer.GlobalCubeTexturePool.AllocateTextureDescriptor();
        }
        else
        {
            GPUTexture = SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();
        }

        if (GPUTexture == null)
        {
            Smithbox.Log(this, LOC.Get("REND_TextureResource_Missing_Texture_Descriptor"));
            return false;
        }

        if (Texture.Platform == TPF.TPFPlatform.PC || Texture.Platform == TPF.TPFPlatform.PS3)
        {
            var capturedTexture = Texture;
            var capturedIndex = TPFIndex;
            Texture = null;

            SceneRenderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
            {
                if (GPUTexture == null)
                {
                    return;
                }

                // Intercept unsupported DDS textures here
                if (capturedIndex < capturedTexture.Textures.Count)
                {
                    if (capturedTexture.Textures[capturedIndex] != null)
                    {
                        GPUTexture.FillWithTPF(d, cl, capturedTexture.Platform, capturedTexture.Textures[capturedIndex], capturedTexture.Textures[capturedIndex].Name);
                    }
                }
            });
        }
        else if (Texture.Platform == TPF.TPFPlatform.PS4)
        {
            var capturedTexture = Texture;
            var capturedIndex = TPFIndex;
            Texture = null;

            SceneRenderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
            {
                if (GPUTexture == null)
                {
                    return;
                }

                GPUTexture.FillWithPS4TPF(d, cl, capturedTexture.Platform, capturedTexture.Textures[capturedIndex],
                    capturedTexture.Textures[capturedIndex].Name);
            });
        }

        return true;
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            Texture = null;

            GPUTexture?.Dispose();
            GPUTexture = null;


            disposedValue = true;
        }
    }

    // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
    ~TextureResource()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    bool IResource._Load(Memory<byte> bytes, AccessLevel al, string virtPath)
    {
        return _LoadTexture(al);
    }

    bool IResource._Load(string file, AccessLevel al, string virtPath)
    {
        return _LoadTexture(al);
    }

    #endregion
}