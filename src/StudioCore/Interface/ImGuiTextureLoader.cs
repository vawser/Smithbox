namespace StudioCore.Interface;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using StudioCore.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Veldrid;
using Vortice.Vulkan;

public class ImGuiTextureLoader : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly IImguiRenderer _imguiRenderer;

    // Cache: maps file paths to loaded textures
    private readonly Dictionary<string, LoadedImGuiTexture> _textureCache = new();

    public ImGuiTextureLoader(GraphicsDevice graphicsDevice, IImguiRenderer imguiRenderer)
    {
        _graphicsDevice = graphicsDevice;
        _imguiRenderer = imguiRenderer;
    }

    public LoadedImGuiTexture LoadTextureFromFile(string filePath)
    {
        if (Smithbox.LowRequirementsMode)
            return null;

        var vulkanImGuiRenderer = (VulkanImGuiRenderer)_imguiRenderer;

        if (_textureCache.TryGetValue(filePath, out var cachedTexture))
        {
            return cachedTexture;
        }

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Image file not found: {filePath}");

        using var image = Image.Load<Rgba32>(filePath);
        int width = image.Width;
        int height = image.Height;

        byte[] pixelData = new byte[width * height * 4];
        image.CopyPixelDataTo(pixelData);

        TextureDescription desc = new();
        desc.Width = (uint)width;
        desc.Height = (uint)height;
        desc.MipLevels = 1;
        desc.SampleCount = VkSampleCountFlags.Count1;
        desc.ArrayLayers = 1;
        desc.Depth = 1;
        desc.Type = VkImageType.Image2D;
        desc.Usage = VkImageUsageFlags.Sampled;
        desc.CreateFlags = VkImageCreateFlags.None;
        desc.Tiling = VkImageTiling.Linear;
        desc.Format = VkFormat.R8G8B8A8Unorm;

        var texture = _graphicsDevice.ResourceFactory.CreateTexture(desc);

        _graphicsDevice.UpdateTexture(texture, pixelData, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);

        var textureView = _graphicsDevice.ResourceFactory.CreateTextureView(texture);
        IntPtr imguiBinding = vulkanImGuiRenderer.GetOrCreateImGuiBinding(_graphicsDevice.ResourceFactory, textureView);

        var loadedTexture = new LoadedImGuiTexture
        {
            Texture = texture,
            TextureView = textureView,
            ImGuiBinding = imguiBinding,
            Width = width,
            Height = height,
            Path = filePath
        };

        _textureCache[filePath] = loadedTexture;

        return loadedTexture;
    }

    public void RemoveTexture(string filePath)
    {
        if (_textureCache.TryGetValue(filePath, out var texture))
        {
            texture.Dispose();
            _textureCache.Remove(filePath);
        }
    }

    public void Dispose()
    {
        foreach (var kv in _textureCache)
        {
            kv.Value.Dispose();
        }
        _textureCache.Clear();
    }
}

public class LoadedImGuiTexture : IDisposable
{
    public Texture Texture { get; set; }
    public TextureView TextureView { get; set; }
    public IntPtr ImGuiBinding { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string Path { get; set; }

    public void Dispose()
    {
        TextureView.Dispose();
        Texture.Dispose();
    }
}