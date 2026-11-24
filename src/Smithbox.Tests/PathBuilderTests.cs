using StudioCore.Application;
using StudioCore.Renderer;

namespace SmithboxTests;

/// <summary>
///     Pins the virtual-path → VFS-relative-path mapping per game. These cases avoid the branches that query the VFS
///     (DES .dcx preference, DS1 loose textures, map folder lookups), so they run without game data.
/// </summary>
public class PathBuilderTests
{
    private static string Rel(ProjectType type, string virtPath)
    {
        // Already in the VFS's own form (leading slash, forward slashes, lower case), so independent of the OS separator
        return PathBuilder.GetRelativePath(TestProjects.Of(type), virtPath);
    }

    [Theory]
    [InlineData(ProjectType.DS1, "/map/m10_00_00_00/m0000b0.flver")]
    [InlineData(ProjectType.DS1R, "/map/m10_00_00_00/m0000b0.flver.dcx")]
    [InlineData(ProjectType.BB, "/map/m10_00_00_00/m0000b0.flver.dcx")]
    [InlineData(ProjectType.DS3, "/map/m10_00_00_00/m0000b0.mapbnd.dcx")]
    [InlineData(ProjectType.SDT, "/map/m10_00_00_00/m0000b0.mapbnd.dcx")]
    [InlineData(ProjectType.ER, "/map/m10/m10_00_00_00/m0000b0.mapbnd.dcx")]
    [InlineData(ProjectType.AC6, "/map/m10/m10_00_00_00/m0000b0.mapbnd.dcx")]
    [InlineData(ProjectType.DS2S, "/model/map/m10_00_00_00.mapbhd")]
    public void MapModel(ProjectType type, string expected)
    {
        Assert.Equal(expected, Rel(type, "map/m10_00_00_00/model/m0000b0"));
    }

    [Theory]
    [InlineData(ProjectType.DS1R, "lo", "/map/m10_00_00_00/l10_00_00_00.hkxbhd")]
    [InlineData(ProjectType.DS1R, "hi", "/map/m10_00_00_00/h10_00_00_00.hkxbhd")]
    [InlineData(ProjectType.DS3, "lo", "/map/m10_00_00_00/l10_00_00_00.hkxbhd")]
    [InlineData(ProjectType.DS2S, "lo", "/model/map/l10_00_00_00.hkxbhd")]
    [InlineData(ProjectType.DS2S, "hi", "/model/map/h10_00_00_00.hkxbhd")]
    // DS2 (non-SotFS) has no separate low collision binder, so both resolve to the high one
    [InlineData(ProjectType.DS2, "lo", "/model/map/h10_00_00_00.hkxbhd")]
    public void MapCollision(ProjectType type, string hitType, string expected)
    {
        Assert.Equal(expected, Rel(type, $"map/m10_00_00_00/hit/{hitType}/h0000b0.hkx"));
    }

    [Theory]
    [InlineData(ProjectType.DS1, "/map/m10_00_00_00/m10_00_00_00.nvmbnd")]
    [InlineData(ProjectType.DS1R, "/map/m10_00_00_00/m10_00_00_00.nvmbnd.dcx")]
    public void MapNavmesh(ProjectType type, string expected)
    {
        Assert.Equal(expected, Rel(type, "map/m10_00_00_00/nav"));
    }

    [Theory]
    [InlineData(ProjectType.DS1, "/chr/c1000.chrbnd")]
    [InlineData(ProjectType.DS2S, "/model/chr/c1000.bnd")]
    [InlineData(ProjectType.DS3, "/chr/c1000.chrbnd.dcx")]
    [InlineData(ProjectType.ER, "/chr/c1000.chrbnd.dcx")]
    public void CharacterModel(ProjectType type, string expected)
    {
        Assert.Equal(expected, Rel(type, "chr/c1000/model"));
    }

    [Theory]
    [InlineData(ProjectType.DS2S, "/model/chr/c1000.texbnd")]
    [InlineData(ProjectType.DS1R, "/chr/c1000.chrbnd.dcx")]
    [InlineData(ProjectType.DS3, "/chr/c1000.texbnd.dcx")]
    [InlineData(ProjectType.SDT, "/chr/c1000.texbnd.dcx")]
    public void CharacterTextures(ProjectType type, string expected)
    {
        Assert.Equal(expected, Rel(type, "chr/c1000/tex"));
    }

    [Fact]
    public void LooseFlver_ReturnsTheSegmentAfterThePrefix()
    {
        Assert.Equal("c1000.flver", PathBuilder.GetRelativePath(TestProjects.Of(ProjectType.ER), "loose/flver/c1000.flver"));
    }

    [Theory]
    [InlineData(ProjectType.ER, "m10_01_00_00", "m10_01_00_00")]
    [InlineData(ProjectType.DS3, "m30_01_00_00", "m30_01_00_00")]
    [InlineData(ProjectType.DS1R, "m10_01_00_00", "m10_01_00_00")]
    [InlineData(ProjectType.DS1R, "m99_00_00_01", "m99_00_00_01")]
    [InlineData(ProjectType.BB, "m29_12_34_56", "m29_00_00_00")]
    public void AssetMapId(ProjectType type, string mapId, string expected)
    {
        Assert.Equal(expected, PathBuilder.GetAssetMapID(TestProjects.Of(type), mapId));
    }

    [Fact]
    public void SanitiseRelativePath_UsesForwardSlashesAndLowerCase()
    {
        Assert.Equal("/map/m10_00_00_00/a.flver", PathBuilder.SanitiseRelativePath(@"Map\M10_00_00_00\A.FLVER"));
    }
}
