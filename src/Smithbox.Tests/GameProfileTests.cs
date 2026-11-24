using StudioCore.Application;

namespace SmithboxTests;

public class GameProfileTests
{
    public static TheoryData<ProjectType> AllTypes()
    {
        var data = new TheoryData<ProjectType>();

        foreach (var type in Enum.GetValues<ProjectType>())
            data.Add(type);

        return data;
    }

    [Theory]
    [MemberData(nameof(AllTypes))]
    public void EveryGame_HasItsOwnProfile(ProjectType type)
    {
        Assert.Equal(type, GameProfile.For(type).Type);
    }

    [Fact]
    public void Ds2Layout_IsDs2AndScholar()
    {
        Assert.Equal([ProjectType.DS2S, ProjectType.DS2], Where(p => p.Ds2Layout));
    }

    /// <summary>
    ///     The games whose map folders sit in area folders. Checked against game data on 2026-11-24 for the Duskbloods
    ///     network test (romfs/map/m80_00_00_00/*.mapbnd.dcx, the area folders holding only colour grading).
    /// </summary>
    [Fact]
    public void MapAreaFolders_AreErNrAndAc6()
    {
        Assert.Equal([ProjectType.ER, ProjectType.AC6, ProjectType.NR], Where(p => p.MapAreaFolders));
    }

    /// <summary>Every game but Undefined names a file dictionary, and the file is in Smithbox.Data's assets</summary>
    [Theory]
    [MemberData(nameof(AllTypes))]
    public void FileDictionary_ExistsInTheDataAssets(ProjectType type)
    {
        var name = GameProfile.For(type).FileDictionaryName;

        if (type is ProjectType.Undefined)
        {
            Assert.Null(name);
            return;
        }

        Assert.NotNull(name);

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "Smithbox.Data")))
            dir = dir.Parent;

        Assert.NotNull(dir);
        Assert.True(File.Exists(Path.Combine(dir.FullName, "Smithbox.Data", "Assets", "File Dictionaries", name)),
            $"{type}: {name} isn't in Smithbox.Data/Assets/File Dictionaries");
    }

    [Fact]
    public void AssetFolders_ArePerMap()
    {
        foreach (var type in Enum.GetValues<ProjectType>())
        {
            var profile = GameProfile.For(type);

            if (profile.AssetLayout != GameAssetLayout.None)
                Assert.True(profile.AssetsPerMap, $"{type} has asset folders but shares assets per area");
        }
    }

    private static IEnumerable<ProjectType> Where(Func<GameProfile, bool> test)
    {
        return Enum.GetValues<ProjectType>().Where(t => test(GameProfile.For(t)));
    }
}
