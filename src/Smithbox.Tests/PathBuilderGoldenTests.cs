using StudioCore.Application;
using StudioCore.Renderer;
using System.Text;

namespace SmithboxTests;

/// <summary>
///     Runs PathBuilder over every game and a virtual path for each of its branches, and compares the result with
///     Golden/PathBuilder.golden.txt, so a refactor can't change a path unnoticed.
/// </summary>
/// <remarks>When a path is meant to change, regenerate the file (see <see cref="Golden"/>) and review its diff.</remarks>
public class PathBuilderGoldenTests
{
    private static readonly string[] VirtualPaths =
    [
        "loose/flver/c1000.flver",

        "map/tex/m10_00_00_00/0000",
        "map/tex/m10_00_00_00/own",
        "map/tex/m10_00_00_00/gienv",
        "map/tex/m10_00_00_00/tex",

        "map/m10_00_00_00/model/m0000b0",
        "map/m10_00_00_00/hit/lo/h0000b0.hkx",
        "map/m10_00_00_00/hit/hi/h0000b0.hkx",
        "map/m10_00_00_00/hit/fa/h0000b0.hkx",
        "map/m10_00_00_00/connect/lo/h0000b0.hkx",
        "map/m10_00_00_00/nav",
        "map/m10_00_00_00/navvol",
        "map/m10_00_00_00/envmap/m10_00_00_00_envmap_00",

        "chr/c1000/model",
        "chr/c1000/tex",
        "chr/c1000/tex/low",

        "obj/o000100/model",
        "obj/aeg099_001/model",
        "obj/aeg099_001/tex",
        "obj/aeg099_001/collision/aeg099_001_col.hkx",

        "parts/hd_m_1000/model",
        "parts/wp_a_0100/model",
        "parts/common_body/model",
        "parts/hd_m_1000/tex/low",
        "parts/hd_m_1000/tex/tpf",
        "parts/hd_m_1000/tex/other",
        "parts/wp_a_0100/tex/tpf",
        "parts/wp_a_0100_l/tex/tpf",

        "menu/ic_area_01/tex",
        "menu/ic_list_01/tex",
        "menu/map_name_01/tex",
        "menu/vi_01/tex",
        "menu/01_common/tex",

        "aet/aet001_000/tex",
        "aat/aat_0001/tex",
        "systex/systex/tex",
        "sfx/frpg_sfxbnd_c1000/tex",
        "other/default/tex",
        "unknown/thing",
    ];

    private static readonly string[] AssetMapIds = ["m10_01_00_00", "m99_00_00_01", "m29_12_34_56", "m61_44_45_00"];

    private static string Render()
    {
        var text = new StringBuilder();

        foreach (var type in Enum.GetValues<ProjectType>())
        {
            if (type is ProjectType.Undefined)
                continue;

            text.Append("## ").Append(type).Append('\n');

            var project = TestProjects.Of(type);

            foreach (var path in VirtualPaths)
                text.Append(path).Append(" => ").Append(Run(() => PathBuilder.GetRelativePath(project, path))).Append('\n');

            foreach (var mapId in AssetMapIds)
                text.Append("asset map ").Append(mapId).Append(" => ").Append(Run(() => PathBuilder.GetAssetMapID(project, mapId))).Append('\n');
        }

        return text.ToString();
    }

    // Some paths don't apply to some games and throw: that is part of the behaviour too
    private static string Run(Func<string> build)
    {
        try
        {
            return build();
        }
        catch (Exception e)
        {
            return $"<{e.GetType().Name}>";
        }
    }

    [Fact]
    public void PathBuilderGolden_MatchesTheRecordedPaths()
    {
        Golden.Verify("PathBuilder", Render());
    }
}
