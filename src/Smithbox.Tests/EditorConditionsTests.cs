using StudioCore.Application;
using StudioCore.Editors.Common;

namespace SmithboxTests;

/// <summary>
///     Pins which editors each game gets. A failure means a game gained or lost an editor: check that it was meant to.
/// </summary>
public class EditorConditionsTests
{
    [Theory]
    [InlineData(ProjectType.DS2)]
    [InlineData(ProjectType.DS2S)]
    public void TimeActEditor_IsNotSupported_ForDS2(ProjectType type)
    {
        Assert.False(EditorConditions.SupportsTimeActEditor(type));
    }

    [Theory]
    [InlineData(ProjectType.DES)]
    [InlineData(ProjectType.DS1)]
    [InlineData(ProjectType.DS3)]
    [InlineData(ProjectType.ER)]
    public void TimeActEditor_IsSupported_ForOtherGames(ProjectType type)
    {
        Assert.True(EditorConditions.SupportsTimeActEditor(type));
    }

    [Theory]
    [InlineData(ProjectType.DS3)]
    [InlineData(ProjectType.SDT)]
    [InlineData(ProjectType.ER)]
    [InlineData(ProjectType.AC6)]
    [InlineData(ProjectType.NR)]
    public void FxPlayback_ImpliesFxEditor(ProjectType type)
    {
        Assert.True(EditorConditions.SupportsFxPlayback(type));
        Assert.True(EditorConditions.SupportsFxEditor(type));
    }

    [Theory]
    [InlineData(ProjectType.DES)]
    [InlineData(ProjectType.DS1)]
    [InlineData(ProjectType.DS2S)]
    [InlineData(ProjectType.BB)]
    public void OlderGames_HaveFxEditor_WithoutPlayback(ProjectType type)
    {
        Assert.False(EditorConditions.SupportsFxPlayback(type));
        Assert.True(EditorConditions.SupportsFxEditor(type));
    }

    [Fact]
    public void Wwise_IsOnlyUsed_BySoundEditorGames()
    {
        foreach (var type in Enum.GetValues<ProjectType>())
        {
            if (EditorConditions.UsesWwise(type))
                Assert.True(EditorConditions.SupportsSoundEditor(type), $"{type} uses Wwise but has no Sound Editor");
        }
    }

    [Fact]
    public void Bhv_IsOnlyUsed_ByBehaviorEditorGames()
    {
        foreach (var type in Enum.GetValues<ProjectType>())
        {
            if (EditorConditions.UsesBhvBehaviors(type))
                Assert.True(EditorConditions.SupportsBehaviorEditor(type), $"{type} uses .bhv but has no Behavior Editor");
        }
    }

    [Fact]
    public void SoundEditor_IsNotSupported_ForDES()
    {
        Assert.False(EditorConditions.SupportsSoundEditor(ProjectType.DES));
    }

    [Theory]
    [InlineData(ProjectType.DES)]
    [InlineData(ProjectType.DS1)]
    [InlineData(ProjectType.DS1R)]
    public void GraphicsParamEditor_IsNotSupported_ForEarlyGames(ProjectType type)
    {
        Assert.False(EditorConditions.SupportsGraphicsParamEditor(type));
    }
}
