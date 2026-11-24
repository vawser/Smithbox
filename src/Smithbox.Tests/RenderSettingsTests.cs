using StudioCore.Application;
using StudioCore.Renderer;

namespace SmithboxTests;

/// <summary>The settings the renderer reads, handed over from the configuration at start-up</summary>
[Collection(nameof(RenderSettingsTests))]
public class RenderSettingsTests
{
    [Fact]
    public void Use_GivesTheRendererTheConfigurationsOwnSections()
    {
        var previousViewport = RenderSettings.Viewport;
        var previousGraphics = RenderSettings.Graphics;

        try
        {
            var config = new CFG();
            RenderSettings.Use(config.Viewport, config.Graphics);

            Assert.Same(config.Viewport, RenderSettings.Viewport);
            Assert.Same(config.Graphics, RenderSettings.Graphics);

            // A change to the configuration (as the Preferences make) is what the renderer reads
            config.Viewport.Viewport_Frame_Rate = 30.0f;
            Assert.Equal(30.0f, RenderSettings.Viewport.Viewport_Frame_Rate);

            // Resetting to defaults copies into the sections, so the renderer keeps reading the live ones
            CFGHelpers.CopyMembers(new CFG(), config);
            Assert.Same(config.Viewport, RenderSettings.Viewport);
            Assert.Equal(new CFG().Viewport.Viewport_Frame_Rate, RenderSettings.Viewport.Viewport_Frame_Rate);
        }
        finally
        {
            RenderSettings.Use(previousViewport, previousGraphics);
        }
    }

    [Fact]
    public void Use_RefusesMissingSections()
    {
        Assert.Throws<ArgumentNullException>(() => RenderSettings.Use(null!, new GraphicsSettings()));
        Assert.Throws<ArgumentNullException>(() => RenderSettings.Use(new ViewportSettings(), null!));
    }
}
