using StudioCore.Application;
using System.Text;
using System.Text.Json;

namespace SmithboxTests;

/// <summary>
///     Configuration.json must keep its shape as CFG's settings move into sections: one flat object with the same
///     keys, so existing files load and older Smithbox versions can still read new ones.
/// </summary>
public class CfgFormatTests
{
    /// <summary>Every top-level key of a default configuration with its value, sorted by key</summary>
    private static string RenderKeys(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var text = new StringBuilder();

        foreach (var property in doc.RootElement.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
            text.Append(property.Name).Append(" = ").Append(property.Value.GetRawText().ReplaceLineEndings(" ")).Append('\n');

        return text.ToString();
    }

    [Fact]
    public void CfgGolden_DefaultFileHasTheRecordedKeysAndValues()
    {
        Golden.Verify("CfgKeys", RenderKeys(CFG.ToJson(new CFG())));
    }

    /// <summary>A file written before the settings moved into a section still loads them</summary>
    [Fact]
    public void FlatFile_LoadsIntoTheSections()
    {
        var json = """
            {
              "GFX_Display_Width": 2560,
              "GFX_Renderable_LightProbeSphere_BaseColor": { "X": 0.25, "Y": 0.5, "Z": 0.75 },
              "Viewport_Frame_Rate": 144.0,
              "Viewport_Filter_Preset_1": { "Name": "Mine", "Filters": 1 },
              "Interface_UI_Scale": 1.5
            }
            """;

        var config = CFG.FromJson(json)!;

        Assert.Equal(144.0f, config.Viewport.Viewport_Frame_Rate);
        Assert.Equal("Mine", config.Viewport.Viewport_Filter_Preset_1.Name);
        Assert.Equal(2560, config.Graphics.GFX_Display_Width);
        Assert.Equal(0.5f, config.Graphics.GFX_Renderable_LightProbeSphere_BaseColor.Y);
        Assert.Equal(1.5f, config.Interface.Interface_UI_Scale);

        // What the file doesn't hold keeps its default
        Assert.Equal(new CFG().Graphics.GFX_Display_Height, config.Graphics.GFX_Display_Height);
    }

    [Fact]
    public void Sections_AreSavedFlat_UnderTheirOwnKeys()
    {
        var config = new CFG();
        config.Graphics.GFX_Display_Width = 3840;

        using var doc = JsonDocument.Parse(CFG.ToJson(config));

        Assert.Equal(3840, doc.RootElement.GetProperty("GFX_Display_Width").GetInt32());
        Assert.False(doc.RootElement.TryGetProperty(nameof(CFG.Graphics), out _));
    }

    [Fact]
    public void SaveThenLoad_KeepsEverySetting()
    {
        var config = new CFG();
        config.Graphics.GFX_Display_X = 123;
        config.Interface.Interface_UI_Scale = 2.0f;

        var json = CFG.ToJson(config);

        Assert.Equal(json, CFG.ToJson(CFG.FromJson(json)!));
    }

    /// <summary>
    ///     A file from before the grid sets still has its Map Editor grid settings moved into them, now that the grid
    ///     set lives in the Map Editor section
    /// </summary>
    [Fact]
    public void OldGridSettings_MigrateIntoTheMapEditorSection()
    {
        var json = """{ "MapEditor_PrimaryGrid_Size": 42, "MapEditor_DisplayPrimaryGrid": true }""";
        var config = CFG.FromJson(json)!;

        StudioCore.Editors.Viewport.ViewportGridSettingsMigration.Migrate(config, json);

        var grid = config.MapEditor.MapEditor_Grids.Get(StudioCore.Editors.Viewport.ViewportGridType.Primary);
        Assert.Equal(42, grid.Size);
        Assert.True(grid.Display);
    }

    /// <summary>Copying defaults copies into the sections, so two configurations never share one</summary>
    [Fact]
    public void CopyMembers_KeepsEachConfigurationsOwnSections()
    {
        var defaults = new CFG();
        var target = new CFG();
        var section = target.Graphics;

        target.Graphics.GFX_Display_Width = 1;
        CFGHelpers.CopyMembers(defaults, target);

        Assert.Same(section, target.Graphics);
        Assert.NotSame(defaults.Graphics, target.Graphics);
        Assert.Equal(defaults.Graphics.GFX_Display_Width, target.Graphics.GFX_Display_Width);

        target.Graphics.GFX_Display_Width = 2;
        Assert.NotEqual(2, defaults.Graphics.GFX_Display_Width);
    }
}
