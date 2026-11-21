using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.TextureViewer;
using System;

namespace StudioCore.Renderer;

public class TextureResource : IResource, IDisposable
{
    private int TPFIndex;

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

    /// <summary>
    ///     Whether <see cref="TexturePool.TextureHandle.TpfTexture" />, and with it the texture's file data, is kept
    ///     once the texture is on the GPU. True for textures made on their own (the Texture Viewer's, previews), which
    ///     are read back. Textures loaded into a pool drop it, as a map holds thousands of them, which would otherwise
    ///     keep a copy of each in memory as well as on the GPU: read one again with <see cref="TextureSourceReader" />.
    /// </summary>
    public bool KeepSourceData { get; init; } = true;

    private long _budgetBytes;

    /// <summary>
    ///     This texture's share of the <see cref="LoadMemoryBudget" />, given back once it is uploaded (or its upload
    ///     is given up), as its data is held until then.
    /// </summary>
    public long BudgetBytes
    {
        get => Interlocked.Read(ref _budgetBytes);
        init => _budgetBytes = value;
    }

    /// <summary>
    ///     Gives back this texture's share of the load memory budget. Only the first call does anything.
    /// </summary>
    public void ReleaseBudget()
    {
        LoadMemoryBudget.Release(Interlocked.Exchange(ref _budgetBytes, 0));
    }

    /// <summary>
    ///     Called on the render thread once the texture has been filled.
    /// </summary>
    private void OnUploaded(TexturePool.TextureHandle handle)
    {
        if (!KeepSourceData && handle != null)
        {
            handle.TpfTexture = null;
        }
    }

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
        // Switch 2 textures are deswizzled (and ASTC decoded) here on the resource loader's thread, rather than in
        // the upload task on the render thread, and then go through the PC path as a plain DDS
        if (Texture.Platform == TPF.TPFPlatform.NintendoSwitch2)
        {
            var source = Texture.Textures[TPFIndex];
            byte[] dds;

            try
            {
                dds = source.Headerize();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("REND_Texture_Load_Error", source.Name, source.Format), e);
                return false;
            }

            var converted = new TPF.Texture(TPF.TPFPlatform.PC)
            {
                Name = source.Name,
                Format = source.Format,
                Type = source.Type,
                Mipmaps = source.Mipmaps,
                Bytes = dds
            };

            Texture = new TPF { Platform = TPF.TPFPlatform.PC, Textures = { converted } };
            TPFIndex = 0;
        }

        if (TexturePool.TextureHandle.IsTPFCube(Texture.Textures[TPFIndex], Texture.Platform))
        {
            GPUTexture = SceneRenderer.GlobalCubeTexturePool.AllocateTextureDescriptor();
        }
        else if (TexturePool.TextureHandle.IsTPFVolume(Texture.Textures[TPFIndex], Texture.Platform))
        {
            GPUTexture = SceneRenderer.GlobalVolumeTexturePool.AllocateTextureDescriptor();
        }
        else
        {
            GPUTexture = SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();
        }

        // TEMPORARY BB texture diagnostics (BbTextureDiag)
        if (BbTextureDiag.IsTracedTexture(Texture.Textures[TPFIndex].Name))
        {
            BbTextureDiag.Write($"LOAD '{Texture.Textures[TPFIndex].Name}' platform={Texture.Platform} descriptor={(GPUTexture == null ? "NULL" : GPUTexture.TexHandle.ToString())} pool={GPUTexture?.Pool?.GetHashCode()}");
            if (GPUTexture != null)
            {
                BbTextureDiag.TracedSlots[GPUTexture.TexHandle] = Texture.Textures[TPFIndex].Name;
            }
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
                try
                {
                    var gpuTexture = GPUTexture;
                    if (gpuTexture == null)
                    {
                        return;
                    }

                    // Intercept unsupported DDS textures here
                    if (capturedIndex < capturedTexture.Textures.Count)
                    {
                        if (capturedTexture.Textures[capturedIndex] != null)
                        {
                            gpuTexture.FillWithTPF(d, cl, capturedTexture.Platform, capturedTexture.Textures[capturedIndex], capturedTexture.Textures[capturedIndex].Name);
                        }
                    }

                    OnUploaded(gpuTexture);
                }
                finally
                {
                    ReleaseBudget();
                }
            });

            return true;
        }
        else if (Texture.Platform == TPF.TPFPlatform.PS4)
        {
            var capturedTexture = Texture;
            var capturedIndex = TPFIndex;
            Texture = null;

            SceneRenderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
            {
                try
                {
                    var gpuTexture = GPUTexture;
                    if (gpuTexture == null)
                    {
                        return;
                    }

                    // TEMPORARY BB texture diagnostics (BbTextureDiag)
                    var diagTex = capturedTexture.Textures[capturedIndex];
                    var traced = BbTextureDiag.IsTracedTexture(diagTex.Name);
                    if (traced)
                    {
                        BbTextureDiag.Write($"UPLOAD start '{diagTex.Name}' fmt={diagTex.Format} dxgi={diagTex.Header?.DXGIFormat} {diagTex.Header?.Width}x{diagTex.Header?.Height} mips={diagTex.Mipmaps} bytes={diagTex.Bytes?.Length} index={gpuTexture.TexHandle}");
                    }

                    try
                    {
                        gpuTexture.FillWithPS4TPF(d, cl, capturedTexture.Platform, capturedTexture.Textures[capturedIndex],
                            capturedTexture.Textures[capturedIndex].Name);
                    }
                    catch (Exception e) when (traced)
                    {
                        BbTextureDiag.Write($"UPLOAD FAILED '{diagTex.Name}': {e}");
                        throw;
                    }

                    if (traced)
                    {
                        BbTextureDiag.Write($"UPLOAD done '{diagTex.Name}' resident={gpuTexture.Resident} format={gpuTexture.Format}");
                    }

                    OnUploaded(gpuTexture);
                }
                finally
                {
                    ReleaseBudget();
                }
            });

            return true;
        }

        // Nothing was queued for upload, so nothing will give the budget back later
        ReleaseBudget();
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

    bool IResource._Load(Memory<byte> bytes, AccessLevel al, ResourceLoadContext ctx)
    {
        return _LoadTexture(al);
    }

    bool IResource._Load(string file, AccessLevel al, ResourceLoadContext ctx)
    {
        return _LoadTexture(al);
    }

    #endregion
}