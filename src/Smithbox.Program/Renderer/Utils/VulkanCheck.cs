using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Renderer;

public static class VulkanCheck
{
    public static bool IsVulkanSupported()
    {
        try
        {
            GraphicsDeviceOptions options = new GraphicsDeviceOptions(
                debug: false,
                swapchainDepthFormat: null,
                syncToVerticalBlank: true,
                preferStandardClipSpaceYDirection: true,
                preferDepthRangeZeroToOne: true);

            using GraphicsDevice gd = GraphicsDevice.CreateVulkan(options);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
