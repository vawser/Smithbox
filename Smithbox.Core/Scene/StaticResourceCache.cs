using System.Collections.Generic;
using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Scene;

internal static class StaticResourceCache
{
    private static readonly Dictionary<GraphicsPipelineDescription, Pipeline> s_pipelines = new();

    private static readonly Dictionary<ResourceLayoutDescription, ResourceLayout> s_layouts = new();

    private static readonly Dictionary<ShaderSetCacheKey, (Shader, Shader)> s_shaderSets = new();

    private static readonly Dictionary<Texture, TextureView> s_textureViews = new();

    private static readonly Dictionary<ResourceSetDescription, ResourceSet> s_resourceSets = new();

    public static readonly ResourceLayoutDescription SceneParamLayoutDescription = new(
        new ResourceLayoutElementDescription("SceneParam", VkDescriptorType.UniformBuffer,
            VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment));

    public static readonly ResourceLayoutDescription PickingResultDescription = new(
        new ResourceLayoutElementDescription("PickingResult", VkDescriptorType.StorageBuffer,
            VkShaderStageFlags.Vertex | VkShaderStageFlags.Fragment));

    public static Pipeline GetPipeline(ResourceFactory factory, ref GraphicsPipelineDescription desc)
    {
        if (!s_pipelines.TryGetValue(desc, out Pipeline p))
        {
            p = factory.CreateGraphicsPipeline(ref desc);
            s_pipelines.Add(desc, p);
        }

        return p;
    }

    public static ResourceLayout GetResourceLayout(ResourceFactory factory, ResourceLayoutDescription desc)
    {
        if (!s_layouts.TryGetValue(desc, out ResourceLayout p))
        {
            p = factory.CreateResourceLayout(ref desc);
            s_layouts.Add(desc, p);
        }

        return p;
    }

    public static (Shader vs, Shader fs) GetShaders(
        GraphicsDevice gd,
        ResourceFactory factory,
        string name)
    {
        SpecializationConstant[] constants = ShaderHelper.GetSpecializations(gd);
        ShaderSetCacheKey cacheKey = new(name, constants);
        if (!s_shaderSets.TryGetValue(cacheKey, out (Shader vs, Shader fs) set))
        {
            set = ShaderHelper.LoadSPIRV(gd, factory, name);
            s_shaderSets.Add(cacheKey, set);
        }

        return set;
    }

    public static void DestroyAllDeviceObjects()
    {
        foreach (KeyValuePair<GraphicsPipelineDescription, Pipeline> kvp in s_pipelines)
        {
            kvp.Value.Dispose();
        }

        s_pipelines.Clear();

        foreach (KeyValuePair<ResourceLayoutDescription, ResourceLayout> kvp in s_layouts)
        {
            kvp.Value.Dispose();
        }

        s_layouts.Clear();

        foreach (KeyValuePair<ShaderSetCacheKey, (Shader, Shader)> kvp in s_shaderSets)
        {
            kvp.Value.Item1.Dispose();
            kvp.Value.Item2.Dispose();
        }

        s_shaderSets.Clear();

        foreach (KeyValuePair<Texture, TextureView> kvp in s_textureViews)
        {
            kvp.Value.Dispose();
        }

        s_textureViews.Clear();

        foreach (KeyValuePair<ResourceSetDescription, ResourceSet> kvp in s_resourceSets)
        {
            kvp.Value.Dispose();
        }

        s_resourceSets.Clear();
    }

    internal static TextureView GetTextureView(ResourceFactory factory, Texture texture)
    {
        if (!s_textureViews.TryGetValue(texture, out TextureView view))
        {
            view = factory.CreateTextureView(texture);
            s_textureViews.Add(texture, view);
        }

        return view;
    }

    internal static ResourceSet GetResourceSet(ResourceFactory factory, ResourceSetDescription description)
    {
        if (!s_resourceSets.TryGetValue(description, out ResourceSet ret))
        {
            ret = factory.CreateResourceSet(ref description);
            s_resourceSets.Add(description, ret);
        }

        return ret;
    }
}
