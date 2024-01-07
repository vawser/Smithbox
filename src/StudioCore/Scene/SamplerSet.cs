using Veldrid;
using Vortice.Vulkan;

namespace StudioCore.Scene;

/// <summary>
///     Helper class that contains descriptor sets and layouts for samplers
/// </summary>
public static class SamplerSet
{
    private static Sampler _linearSampler;
    private static Sampler _anisoLinearSampler;
    public static ResourceSet SamplersSet;
    public static ResourceLayout SamplersLayout;

    public static void Initialize(GraphicsDevice d)
    {
        ResourceLayoutDescription layoutdesc = new(
            new ResourceLayoutElementDescription(
                "linearSampler",
                VkDescriptorType.Sampler,
                VkShaderStageFlags.Fragment,
                VkDescriptorBindingFlags.None),
            new ResourceLayoutElementDescription(
                "anisoLinearSampler",
                VkDescriptorType.Sampler,
                VkShaderStageFlags.Fragment,
                VkDescriptorBindingFlags.None));
        SamplersLayout = d.ResourceFactory.CreateResourceLayout(layoutdesc);

        _linearSampler = d.ResourceFactory.CreateSampler(new SamplerDescription(
            VkSamplerAddressMode.Repeat, VkSamplerAddressMode.Repeat, VkSamplerAddressMode.Repeat,
            VkFilter.Linear, VkFilter.Linear, VkSamplerMipmapMode.Linear,
            null, 0, 0, 15, 0, VkBorderColor.FloatOpaqueBlack
        ));
        _anisoLinearSampler = d.ResourceFactory.CreateSampler(new SamplerDescription(
            VkSamplerAddressMode.Repeat, VkSamplerAddressMode.Repeat, VkSamplerAddressMode.Repeat,
            VkFilter.Linear, VkFilter.Linear, VkSamplerMipmapMode.Linear,
            null, 16, 0, 15, 0, VkBorderColor.FloatOpaqueBlack
        ));

        ResourceSetDescription setdesc = new(SamplersLayout, _linearSampler, _anisoLinearSampler);
        SamplersSet = d.ResourceFactory.CreateResourceSet(setdesc);
    }

    public static void Destory()
    {
        SamplersSet.Dispose();
        SamplersLayout.Dispose();
        _linearSampler.Dispose();
    }
}
