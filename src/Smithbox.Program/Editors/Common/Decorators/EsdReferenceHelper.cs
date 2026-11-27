using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.EsdEditor;
using StudioCore.Keybinds;
using System.Globalization;

namespace StudioCore.Editors.Common;

/// <summary>
/// Quick links from a field that names a talk script to it in the ESD Editor, as <see cref="ParamReferenceHelper"/>
/// does for param rows: the script the value resolves to, a "Go to" entry in the field's context menu, and Ctrl+click.
/// The jump is the esd/select/&lt;container&gt;/&lt;esd&gt; command, which EsdEditorScreen handles.
/// <list type="bullet">
/// <item><see cref="TalkRef"/>: a talk ID (the fields the meta tags with the TalkAlias), the ESD t&lt;id&gt;. Looked for in
/// the map's own talkesdbnd, then its area's (mXX_00_00_00), then any talk script already read; the context menu can
/// read the rest.</item>
/// <item><see cref="MapGimmickRef"/>: a MapGimmickParam row ID, whose gimmickType AABB names m02_AA_BB_00.talkesdbnd
/// (AC6, as the game builds it).</item>
/// </list>
/// </summary>
public static class EsdReferenceHelper
{
    public const string TalkRef = "Talk";
    public const string MapGimmickRef = "MapGimmickParam";

    /// <summary>Where a value points: the container and ESD to select (File may be null), what to show, and whether it exists.</summary>
    public sealed record Target(string Container, string File, string Display, bool Exists);

    /// <summary>Whether the project has an ESD Editor with its files listed.</summary>
    public static bool IsAvailable(ProjectEntry project) =>
        project?.Descriptor != null
        && project.Descriptor.EnableEsdEditor
        && EditorConditions.SupportsEsdEditor(project.Descriptor.ProjectType)
        && project.Handler?.EsdEditor != null
        && project.Handler.EsdData?.PrimaryBank != null;

    /// <summary>The ID a field value holds. Talk IDs of 0 and below are no script; a MapGimmickParam row can be 0.</summary>
    public static bool TryGetId(object value, bool allowZero, out int id)
    {
        id = 0;
        if (value == null)
            return false;

        try
        {
            var n = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            if (n < 0 || (n == 0 && !allowZero) || n > int.MaxValue)
                return false;
            id = (int)n;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// The talk script a field value points to, or null when the field names none. <paramref name="mapName"/> is the
    /// map of the selected entity (for talk IDs). <paramref name="searchAll"/> reads every talk script not yet read.
    /// </summary>
    public static Target Resolve(ProjectEntry project, string kind, object value, string mapName, bool searchAll = false)
    {
        if (!IsAvailable(project))
            return null;

        var bank = project.Handler.EsdData.PrimaryBank;

        return kind switch
        {
            MapGimmickRef when TryGetId(value, true, out var row) => ResolveMapGimmick(project, bank, row),
            TalkRef when TryGetId(value, false, out var talk) => ResolveTalk(project, bank, talk, mapName, searchAll),
            _ => null
        };
    }

    private static Target ResolveMapGimmick(ProjectEntry project, EsdBank bank, int rowId)
    {
        var paramBank = project.Handler.ParamEditor?.ViewHandler?.ActiveView?.GetPrimaryBank();
        if (paramBank?.Params == null || !paramBank.Params.TryGetValue("MapGimmickParam", out var param))
            return null;

        var row = param[rowId];
        if (row == null)
            return new Target(null, null, LOC.Get("ESD_Reference_No_Param_Row", "MapGimmickParam", rowId), false);

        var cell = row["gimmickType"];
        if (cell == null)
            return null;

        // As the game builds it: gimmickType AABB -> m02_AA_BB_00
        var type = Convert.ToInt32(cell.Value.Value, CultureInfo.InvariantCulture) % 10000;
        var stem = $"m02_{type / 100:D2}_{type % 100:D2}_00";
        var container = bank.FindContainer($"{stem}.talkesdbnd");

        if (container == null)
            return new Target(null, null, LOC.Get("ESD_Reference_Missing", stem), false);

        // The ESD inside, when the binder holds just one
        var file = container.EnsureLoaded() && container.Files.Count == 1 ? container.Files[0] : null;

        return Found(project, container, file);
    }

    private static Target ResolveTalk(ProjectEntry project, EsdBank bank, int id, string mapName, bool searchAll)
    {
        // The map's own talk scripts first, then its area's, then a loose tXXXX.esd (DES)
        var first = new List<EsdContainer>();
        void Add(string name)
        {
            var c = bank.FindContainer(name);
            if (c != null && !first.Contains(c))
                first.Add(c);
        }

        if (!string.IsNullOrEmpty(mapName))
        {
            Add($"{mapName}.talkesdbnd");
            if (mapName.Length >= 3)
                Add($"{mapName[..3]}_00_00_00.talkesdbnd");
        }
        Add($"t{id}");

        foreach (var c in first)
        {
            if (c.EnsureLoaded() && FindTalkFile(c, id) is { } f)
                return Found(project, c, f);
        }

        var allRead = true;
        foreach (var c in bank.Containers)
        {
            if (c.Kind != EsdScriptKind.Talk || first.Contains(c))
                continue;

            if (!c.IsLoaded && !(searchAll && c.EnsureLoaded()))
            {
                allRead &= c.LoadError != null;
                continue;
            }

            if (FindTalkFile(c, id) is { } f)
                return Found(project, c, f);
        }

        return new Target(null, null, LOC.Get(allRead ? "ESD_Reference_Missing_All" : "ESD_Reference_Missing", $"t{id}"), false);
    }

    /// <summary>An ESD named t&lt;id&gt;, also with zero padding</summary>
    private static EsdFileWrapper FindTalkFile(EsdContainer container, int id)
    {
        foreach (var f in container.Files)
        {
            if (f.Name.Length > 1 && (f.Name[0] == 't' || f.Name[0] == 'T')
                && int.TryParse(f.Name.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n == id)
            {
                return f;
            }
        }

        return null;
    }

    private static Target Found(ProjectEntry project, EsdContainer container, EsdFileWrapper file)
    {
        var aliases = project.Handler.EsdData.Aliases;
        var stem = container.IsBinder ? Path.GetFileNameWithoutExtension(container.Name) : container.Name;

        string display;
        string alias;
        if (file != null && container.IsBinder)
        {
            display = $"{stem} / {file.Name}";
            alias = aliases.FileAlias(file);
        }
        else
        {
            display = stem;
            alias = aliases.ContainerAlias(container);
        }

        if (!string.IsNullOrEmpty(alias))
            display = $"{display}: {alias}";

        return new Target(container.Name, file != null && container.IsBinder ? file.Name : null, display, true);
    }

    /// <summary>
    /// The context menu entries: go to the script, or (for a talk ID not found yet) read every talk script to find it.
    /// Call inside an open popup.
    /// </summary>
    public static void ContextMenu(ProjectEntry project, string kind, object value, string mapName, Target target)
    {
        if (target == null)
            return;

        if (target.Exists)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
            if (ImGui.Selectable($"{LOC.Get("ESD_Reference_Go_To", target.Display)}##esdRefGoTo"))
                JumpTo(target);
            ImGui.PopStyleColor();
            GUI.Tooltip(LOC.Get("ESD_Reference_Go_To_TT"));
            return;
        }

        ImGui.TextDisabled(target.Display);

        if (kind == TalkRef && !AllTalkRead(project))
        {
            if (ImGui.Selectable($"{LOC.Get("ESD_Reference_Search_All")}##esdRefSearchAll"))
            {
                if (Resolve(project, kind, value, mapName, true) is { Exists: true } found)
                    JumpTo(found);
            }
            GUI.Tooltip(LOC.Get("ESD_Reference_Search_All_TT"));
        }
    }

    /// <summary>Whether every talk script has been read (or failed to read)</summary>
    private static bool AllTalkRead(ProjectEntry project)
    {
        var bank = project.Handler.EsdData?.PrimaryBank;
        return bank == null || bank.Containers.All(c => c.Kind != EsdScriptKind.Talk || c.IsLoaded || c.LoadError != null);
    }

    /// <summary>Ctrl+click on the last item jumps to the script. Call right after the item it applies to.</summary>
    public static void Click(Target target)
    {
        if (target is { Exists: true } && ImGui.IsItemClicked(ImGuiMouseButton.Left) && InputManager.HasCtrlDown())
            JumpTo(target);
    }

    public static void JumpTo(Target target) =>
        EditorCommandQueue.AddCommand($"esd/select/{target.Container}/{target.File ?? ""}");
}
