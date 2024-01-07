using SoulsFormats;
using StudioCore.Scene;
using System;

namespace StudioCore.Resource;

public class TextureResource : IResource, IDisposable
{
    private readonly int TPFIndex;

    public TextureResource()
    {
        throw new Exception("Created wrong");
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
        if (TexturePool.TextureHandle.IsTPFCube(Texture.Textures[TPFIndex], Texture.Platform))
        {
            GPUTexture = Renderer.GlobalCubeTexturePool.AllocateTextureDescriptor();
        }
        else
        {
            GPUTexture = Renderer.GlobalTexturePool.AllocateTextureDescriptor();
        }

        if (GPUTexture == null)
        {
            if (FeatureFlags.StrictResourceChecking)
            {
                throw new Exception("Unable to allocate texture descriptor");
            }

            return false;
        }

        if (Texture.Platform == TPF.TPFPlatform.PC || Texture.Platform == TPF.TPFPlatform.PS3)
        {
            Renderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
            {
                if (GPUTexture == null)
                {
                    return;
                }

                GPUTexture.FillWithTPF(d, cl, Texture.Platform, Texture.Textures[TPFIndex],
                    Texture.Textures[TPFIndex].Name);
                Texture = null;
            });
        }
        else if (Texture.Platform == TPF.TPFPlatform.PS4)
        {
            Renderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
            {
                if (GPUTexture == null)
                {
                    return;
                }

                GPUTexture.FillWithPS4TPF(d, cl, Texture.Platform, Texture.Textures[TPFIndex],
                    Texture.Textures[TPFIndex].Name);
                Texture = null;
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

    bool IResource._Load(Memory<byte> bytes, AccessLevel al, GameType type)
    {
        return _LoadTexture(al);
    }

    bool IResource._Load(string file, AccessLevel al, GameType type)
    {
        return _LoadTexture(al);
    }

    #endregion
}
