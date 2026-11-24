using StudioCore.Application;
using StudioCore.Editors.TextEditor;
using System.Text;

namespace SmithboxTests;

/// <summary>
///     Runs the Text Editor's FMG naming (display name, internal name, grouping, DLC grouping) over every game, each
///     kind of text container, and the IDs of every game's enum for that kind, and compares the names and what was
///     logged with Golden/TextUtils.golden.txt.
/// </summary>
public class TextUtilsGoldenTests
{
    // A container of each kind, as the path substring the Text Editor tells them apart by, and one of none
    private static readonly (string Kind, string Path)[] Containers =
    [
        ("Item", "/msg/engus/item.msgbnd.dcx"),
        ("Menu", "/msg/engus/menu.msgbnd.dcx"),
        ("Sample", "/msg/engus/sample.msgbnd.dcx"),
        ("NgWord", "/msg/engus/ngword.msgbnd.dcx"),
        ("SellRegion", "/msg/engus/sellregion.msgbnd.dcx"),
        ("Other", "/msg/engus/other.msgbnd.dcx"),
    ];

    private static readonly int[] UndefinedIds = [-1, 999999];

    /// <summary>The IDs of every *_MsgBndID_* enum of the kind, from all games, in order</summary>
    private static IEnumerable<int> IdsOfKind(string kind)
    {
        var enums = typeof(TextUtils).Assembly.GetTypes()
            .Where(t => t.IsEnum && t.Name.StartsWith(kind + "_MsgBndID_", StringComparison.Ordinal));

        return enums.SelectMany(e => Enum.GetValues(e).Cast<object>().Select(Convert.ToInt32))
            .Concat(UndefinedIds)
            .Distinct()
            .Order();
    }

    private static string Render()
    {
        TestLogging.Init();
        TestLogging.Take();

        var text = new StringBuilder();

        foreach (var type in Enum.GetValues<ProjectType>())
        {
            text.Append("## ").Append(type).Append('\n');

            var project = TestProjects.Of(type);

            foreach (var (kind, path) in Containers)
            {
                var info = new TextContainerWrapper(project)
                {
                    FileEntry = new FileDictionaryEntry { Path = path, Folder = "/msg/engus", Filename = kind, Extension = "msgbnd", Archive = "data0" }
                };

                foreach (var id in kind == "Other" ? UndefinedIds : IdsOfKind(kind))
                {
                    var fmgName = $"fmg{id}";

                    var display = TextUtils.GetFmgDisplayName(project, info, id, fmgName);
                    var internalName = TextUtils.GetFmgInternalName(project, info, id, fmgName);
                    var grouping = TextUtils.GetFmgGrouping(project, info, id, fmgName);
                    var dlc = TextUtils.GetFmgDlcGrouping(project, info, id, fmgName);
                    var logged = TestLogging.Take();

                    text.Append($"{kind} {id} => {display} | {internalName} | {grouping} | {dlc}");

                    if (logged.Count > 0)
                        text.Append("  ## ").Append(string.Join("; ", logged));

                    text.Append('\n');
                }
            }
        }

        return text.ToString();
    }

    [Fact]
    public void TextUtilsGolden_MatchesTheRecordedNames()
    {
        Golden.Verify("TextUtils", Render());
    }
}
