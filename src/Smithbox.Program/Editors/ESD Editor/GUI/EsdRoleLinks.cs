using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// What an arg's value refers to (Framework/EsdRoles), described and linked to the editor that owns it (P5), as the
/// EMEVD Editor's EmevdRoleLinks does:
/// - EventFlag → this editor's Search (every ESD use) and the EMEVD Editor's Search (every event use);
/// - Param / Item → the Param Editor row (an item's param from its call's item type);
/// - Text → the Text Editor entry;
/// - Entity → the Map Editor, in the talk file's own map;
/// - Machine → the machine.
/// Names come from the other editors' data when it's loaded, and are left out when it isn't.
/// </summary>
public class EsdRoleLinks
{
    private readonly EsdEditorView View;

    public EsdRoleLinks(EsdEditorView view)
    {
        View = view;
    }

    private ProjectEntry Project => View.Project;

    public string Game => Project.Descriptor.ProjectType.ToString();

    public EsdRole RoleOf(EsdCallDef def, int index) => EsdRoles.Of(Game, def, index);

    /// <summary>
    /// The role of the literal at a path of an expression: an arg of the function call around it, else (for a command
    /// arg that is just this literal) the command's arg. With the call's def and its item-type value when it has one.
    /// </summary>
    public (EsdRole Role, EsdCallDef Def, long? Selector) RoleAt(byte[] code, IReadOnlyList<int> path, SoulsFormats.ESD.CommandCall cmd, int arg, EsdKindCatalog cat)
    {
        var root = code != null ? EsdExpr.Decode(code) : null;
        if (root == null || path == null)
            return (EsdRole.None, null, null);

        // Up from the literal, past markers, to the node holding it
        var at = path.Count;
        while (at > 0)
        {
            var parent = EsdEdits.At(root, path.Take(at - 1).ToList());
            if (parent == null)
                break;
            if (parent.Kind == EsdExprKind.Unary && parent.Op is 0xA6 or 0xB7)
            {
                at--;
                continue;
            }
            if (parent.Kind == EsdExprKind.Call && parent.FunctionId is long fid && path[at - 1] >= 1)
            {
                var def = cat?.Function(fid);
                var role = RoleOf(def, path[at - 1] - 1);
                long? sel = role.Kind == EsdRoleKind.Item && role.Selector + 1 < parent.Children.Count ? EsdExpr.Constant(parent.Children[role.Selector + 1]) : null;
                return (role, def, sel);
            }
            return (EsdRole.None, null, null);
        }

        // The whole arg is this literal: the command's own arg
        if (cmd != null && EsdLayout.KindOf(cmd) == EsdCommandKind.Game)
        {
            var def = cat?.Command(cmd.CommandBank, cmd.CommandID);
            var role = RoleOf(def, arg);
            long? sel = role.Kind == EsdRoleKind.Item && role.Selector < cmd.Arguments.Count ? EsdExpr.Constant(EsdExpr.Decode(cmd.Arguments[role.Selector])) : null;
            return (role, def, sel);
        }
        return (EsdRole.None, null, null);
    }

    /// <summary>Every int literal of an expression that has a role, with its path (Properties' References)</summary>
    public List<(List<int> Path, long Value, EsdRole Role, EsdCallDef Def, long? Selector)> References(byte[] code, SoulsFormats.ESD.CommandCall cmd, int arg, EsdKindCatalog cat)
    {
        var list = new List<(List<int>, long, EsdRole, EsdCallDef, long?)>();
        var root = code != null ? EsdExpr.Decode(code) : null;
        if (root == null)
            return list;
        foreach (var (lit, path) in EsdEdits.Literals(root))
        {
            if (lit.Kind != EsdExprKind.Int)
                continue;
            var (role, def, sel) = RoleAt(code, path, cmd, arg, cat);
            if (role.Kind != EsdRoleKind.None)
                list.Add((path, lit.IntValue, role, def, sel));
        }
        return list;
    }

    /// <summary>The map a talk binder belongs to (m10_00_00_00.talkesdbnd → m10_00_00_00), or null</summary>
    public string MapOfFile()
    {
        var name = View.Selection.SelectedContainer?.Name;
        var m = name != null ? Regex.Match(name, @"^(m\d\d_\d\d_\d\d_\d\d)") : null;
        return m?.Success == true ? m.Groups[1].Value : null;
    }

    /// <summary>The param a Param or Item role's value is a row of: the first candidate the project has</summary>
    public string ParamOf(EsdRole role, EsdCallDef def, long? selector, EsdKindCatalog cat)
    {
        IEnumerable<string> candidates = role.Kind switch
        {
            EsdRoleKind.Param => role.Params,
            EsdRoleKind.Item when selector is long s && def != null && role.Selector < def.Args.Count =>
                new[] { EsdRoles.ItemParam(cat?.FindEnum(def.Args[role.Selector].Enum)?.ValueName(s)) }.Where(p => p != null),
            _ => Enumerable.Empty<string>()
        };

        var bank = ParamBank;
        foreach (var p in candidates)
            if (bank?.Params == null || bank.Params.ContainsKey(p))
                return p;
        return null;
    }

    private Andre.Formats.Param.Row FindRow(string param, long id)
    {
        var bank = ParamBank;
        if (bank?.Params == null || id < int.MinValue || id > int.MaxValue || !bank.Params.TryGetValue(param, out var p))
            return null;
        return p[(int)id];
    }

    private ParamBank ParamBank => Project.Handler.ParamEditor?.ViewHandler?.ActiveView?.GetPrimaryBank();

    private TextResult FindText(string fmg, long id)
    {
        var view = Project.Handler.TextEditor?.ViewHandler?.ActiveView;
        if (view == null || fmg == null || id < 0 || id > int.MaxValue)
            return null;
        return TextFinder.GetTextResult(view, fmg, (int)id);
    }

    /// <summary>A short description of what a value refers to, for tooltips and Properties; null if it has no role</summary>
    public string Describe(EsdRole role, long value, EsdCallDef def, long? selector, EsdKindCatalog cat)
    {
        switch (role.Kind)
        {
            case EsdRoleKind.EventFlag:
                return value <= 0 ? LOC.Get("ESD_Role_Flag_None") : LOC.Get("ESD_Role_Flag", value);

            case EsdRoleKind.Param:
            case EsdRoleKind.Item:
                {
                    var param = ParamOf(role, def, selector, cat);
                    if (param == null)
                        return LOC.Get("ESD_Role_Param_Unknown");
                    if (value is 0 or -1)
                        return LOC.Get("ESD_Role_Param_None", param);
                    var row = FindRow(param, value);
                    return row != null && !string.IsNullOrEmpty(row.Name)
                        ? LOC.Get("ESD_Role_Param_Named", param, value, row.Name)
                        : LOC.Get("ESD_Role_Param_Row", param, value);
                }

            case EsdRoleKind.Text:
                {
                    var text = FindText(role.Fmg, value);
                    return text?.Entry?.Text is { Length: > 0 } t
                        ? LOC.Get("ESD_Role_Text_Entry", Shorten(t))
                        : LOC.Get("ESD_Role_Text_Id", value);
                }

            case EsdRoleKind.Entity:
                return value == 10000 ? LOC.Get("ESD_Role_Entity_Player") : LOC.Get("ESD_Role_Entity", value);

            case EsdRoleKind.Machine:
                return View.Selection.SelectedFile?.Esd?.StateGroups.ContainsKey(value) == true
                    ? LOC.Get("ESD_Role_Machine", EsdLayout.MachineLabel(value))
                    : LOC.Get("ESD_Role_Machine_Missing", value);
        }
        return null;
    }

    /// <summary>The link buttons for a value's role (small icon buttons on one line); nothing when it has none</summary>
    public void Buttons(string id, EsdRole role, long value, EsdCallDef def, long? selector, EsdKindCatalog cat)
    {
        void Button(string icon, string key, string tooltip, Action go)
        {
            ImGui.SameLine();
            if (ImGui.SmallButton($"{icon}##{id}{key}"))
                go();
            GUI.Tooltip(tooltip);
        }

        switch (role.Kind)
        {
            case EsdRoleKind.EventFlag when value > 0:
                Button(Icons.Search, "esd", LOC.Get("ESD_Role_Link_Flag_Esd_TT", value), () => View.ToolWindow.SearchTool.SearchValue(value, true));
                if (Project.Descriptor.EnableEmevdEditor)
                    Button(Icons.ExternalLink, "emevd", LOC.Get("ESD_Role_Link_Flag_Emevd_TT", value), () => EditorCommandQueue.AddCommand($"emevd/search/{value}"));
                break;

            case EsdRoleKind.Param or EsdRoleKind.Item when value is not (0 or -1) && Project.Handler.ParamEditor != null:
                {
                    var param = ParamOf(role, def, selector, cat);
                    if (param != null)
                        Button(Icons.ExternalLink, "param", LOC.Get("ESD_Role_Link_Param_TT", param, value), () => EditorCommandQueue.AddCommand($"param/select/-1/{param}/{value}"));
                    break;
                }

            case EsdRoleKind.Text when value >= 0:
                {
                    var text = FindText(role.Fmg, value);
                    if (text?.ContainerWrapper != null)
                        Button(Icons.ExternalLink, "text", LOC.Get("ESD_Role_Link_Text_TT", value), () => EditorCommandQueue.AddCommand(
                            $"text/select/{text.ContainerWrapper.ContainerDisplayCategory}/{text.ContainerWrapper.FileEntry.Filename}/{text.FmgName}/{text.Entry.ID}"));
                    break;
                }

            case EsdRoleKind.Entity when value is > 0 and not 10000 && Project.Handler.MapEditor != null:
                {
                    var map = MapOfFile();
                    if (map != null)
                        Button(Icons.ExternalLink, "entity", LOC.Get("ESD_Role_Link_Entity_TT", value, map), () =>
                        {
                            EditorCommandQueue.AddCommand($"map/load/{map}");
                            EditorCommandQueue.AddCommand($"map/idselect/any/{map}/{value}");
                        });
                    break;
                }

            case EsdRoleKind.Machine when View.Selection.SelectedFile?.Esd?.StateGroups.ContainsKey(value) == true:
                Button(Icons.ArrowRight, "machine", LOC.Get("ESD_Properties_Go_To_Machine_TT"), () => View.Selection.GoToMachine(value));
                break;
        }
    }

    private static string Shorten(string text)
    {
        var oneLine = text.Replace("\r", "").Replace('\n', ' ');
        return oneLine.Length > 60 ? oneLine[..57] + "..." : oneLine;
    }
}
