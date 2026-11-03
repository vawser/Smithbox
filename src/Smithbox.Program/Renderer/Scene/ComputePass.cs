using Veldrid;

namespace StudioCore.Renderer;

/// <summary>
///     A compute shader and the pipeline that runs it, for the post-processing passes that work on whole textures or
///     volumes (auto exposure, volumetric fog, depth mips). The shader is Shaders/<c>name</c>.comp.spv, with its
///     local size matching <see cref="GroupSize" />. Veldrid moves the images a dispatch binds into the layouts it
///     needs; a pass that reads what an earlier dispatch wrote, other than through a resource set, adds its own barrier.
/// </summary>
public class ComputePass
{
    public string Name { get; }

    /// <summary>The shader's local size, which <see cref="Dispatch" /> divides the work by</summary>
    public (uint X, uint Y, uint Z) GroupSize { get; }

    public ResourceLayout[] Layouts { get; }

    private Pipeline Pipeline;

    public ComputePass(string name, ResourceLayout[] layouts, uint groupX, uint groupY = 1, uint groupZ = 1)
    {
        Name = name;
        Layouts = layouts;
        GroupSize = (groupX, groupY, groupZ);
    }

    /// <summary>
    ///     Runs the shader over <paramref name="width" /> x <paramref name="height" /> x <paramref name="depth" />
    ///     invocations, rounded up to whole groups, with <paramref name="sets" /> bound in the order of
    ///     <see cref="Layouts" />. Must be called outside a render pass, so not between a framebuffer's draws.
    /// </summary>
    public void Dispatch(GraphicsDevice gd, CommandList cl, ResourceSet[] sets, uint width, uint height = 1, uint depth = 1)
    {
        if (width == 0 || height == 0 || depth == 0)
        {
            return;
        }

        cl.SetPipeline(GetPipeline(gd));

        for (var i = 0; i < sets.Length; i++)
        {
            cl.SetComputeResourceSet((uint)i, sets[i]);
        }

        cl.Dispatch(DivideUp(width, GroupSize.X), DivideUp(height, GroupSize.Y), DivideUp(depth, GroupSize.Z));
    }

    private static uint DivideUp(uint count, uint size) => (count + size - 1) / size;

    private Pipeline GetPipeline(GraphicsDevice gd)
    {
        if (Pipeline != null)
        {
            return Pipeline;
        }

        Shader shader = ShaderHelper.LoadComputeSPIRV(gd, gd.ResourceFactory, Name);

        ComputePipelineDescription description = new(shader, Layouts, GroupSize.X, GroupSize.Y, GroupSize.Z)
        {
            Specializations = ShaderHelper.GetSpecializations(gd)
        };

        Pipeline = gd.ResourceFactory.CreateComputePipeline(ref description);
        Pipeline.Name = Name;

        return Pipeline;
    }
}
