using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextEditor;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// What an arg's value refers to (EmevdArgDef.Role, from the layouts' roles), described and linked to the editor that
/// owns it:
/// - Entity → the Map Editor, in the event file's own map (common files have no map, so no link: use Search);
/// - Param → the Param Editor row (the param can depend on another arg, e.g. Item Type for Item ID);
/// - Text → the Text Editor entry;
/// - EventFlag → the Search tool, for every use of the flag;
/// - MapId / MapPart → the Map Editor, loading the map.
/// Names are looked up in the other editors' data when it's loaded, and left out when it isn't.
/// </summary>
public class EmevdRoleLinks
{
    private readonly EmevdEditorView View;

    public EmevdRoleLinks(EmevdEditorView view)
    {
        View = view;
    }

    private ProjectEntry Project => View.Project;

    /// <summary>The value an arg holds, as used by its role (-1 for "none" in either signedness)</summary>
    private static long ValueOf(EmevdArg arg) => arg.Type == EmevdArgType.U32 && arg.Int == uint.MaxValue ? -1 : arg.Int;

    /// <summary>The param an arg's value is a row of, given the line (for args whose param another arg picks)</summary>
    public static string ParamOf(EmevdLine line, EmevdArg arg)
    {
        var def = arg.Def;
        if (def.Role != EmevdArgRole.Param)
            return null;

        if (def.RoleSelector < 0)
            return def.RoleType;

        var selector = line.Decoded.Args.FirstOrDefault(a => a.Offset == def.RoleSelector);
        return selector.Def != null ? def.ParamFor(selector.Int) : null;
    }

    /// <summary>The map an EMEVD file belongs to (its name for a map file), or null for common files</summary>
    public static string MapOf(EmevdFileWrapper file)
    {
        return file != null && Regex.IsMatch(file.Name, @"^m\d\d_\d\d_\d\d_\d\d$") ? file.Name : null;
    }

    /// <summary>The map named by a MapId value (AABBCCDD) or by a line's Area/Block/Region/Index args</summary>
    public static string MapName(EmevdLine line, EmevdArg arg)
    {
        if (arg.Def.Role == EmevdArgRole.MapId)
        {
            var v = ValueOf(arg);
            return v < 0 ? null : $"m{v / 1000000 % 100:D2}_{v / 10000 % 100:D2}_{v / 100 % 100:D2}_{v % 100:D2}";
        }

        if (arg.Def.Role == EmevdArgRole.MapPart)
        {
            long Part(string name)
            {
                var a = line.Decoded.Args.FirstOrDefault(x => x.Def.Role == EmevdArgRole.MapPart && x.Def.RoleType == name);
                return a.Def != null ? ValueOf(a) : 0;
            }

            var area = Part("Area");
            return area < 0 ? null : $"m{area:D2}_{Math.Max(0, Part("Block")):D2}_{Math.Max(0, Part("Region")):D2}_{Math.Max(0, Part("Index")):D2}";
        }

        return null;
    }

    /// <summary>A short description of what the value refers to, for tooltips and the Properties window; null if none</summary>
    public string Describe(EmevdLine line, EmevdArg arg)
    {
        var def = arg.Def;
        if (def.Role == EmevdArgRole.None)
            return null;

        if (line.Params.ContainsKey(arg.Offset))
            return LOC.Get("EMEVD_Role_Set_By_Parameter", RoleName(def.Role));

        var v = ValueOf(arg);

        switch (def.Role)
        {
            case EmevdArgRole.Entity:
                if (v == 0 || v == -1)
                    return LOC.Get("EMEVD_Role_Entity_None");
                return def.RoleType != null && def.RoleSelector < 0
                    ? LOC.Get("EMEVD_Role_Entity_Kinds", def.RoleType.Replace(",", ", "))
                    : LOC.Get("EMEVD_Role_Entity");

            case EmevdArgRole.EventFlag:
                return LOC.Get("EMEVD_Role_EventFlag");

            case EmevdArgRole.Param:
            {
                var param = ParamOf(line, arg);
                if (param == null)
                    return LOC.Get("EMEVD_Role_Param_Unknown");
                if (v < 0)
                    return LOC.Get("EMEVD_Role_Param_None", param);

                var row = FindRow(param, v);
                return row == null ? LOC.Get("EMEVD_Role_Param_Row", param, v)
                    : string.IsNullOrEmpty(row.Name) ? LOC.Get("EMEVD_Role_Param_Row", param, v)
                    : LOC.Get("EMEVD_Role_Param_Named", param, v, row.Name);
            }

            case EmevdArgRole.Text:
            {
                if (v < 0)
                    return LOC.Get("EMEVD_Role_Text_None", def.RoleType);

                var text = FindText(def.RoleType, v);
                return text?.Entry?.Text is { Length: > 0 } t
                    ? LOC.Get("EMEVD_Role_Text_Entry", def.RoleType, Shorten(t))
                    : LOC.Get("EMEVD_Role_Text_Id", def.RoleType, v);
            }

            case EmevdArgRole.MapId:
            case EmevdArgRole.MapPart:
                return MapName(line, arg) is { } map ? LOC.Get("EMEVD_Role_Map", map) : null;
        }

        return null;
    }

    /// <summary>
    /// The link button for an arg's role, if it has one: a small icon button with a tooltip, as on list rows
    /// </summary>
    public void Button(string id, EmevdLine line, EmevdArg arg)
    {
        var def = arg.Def;
        if (def.Role == EmevdArgRole.None || line.Params.ContainsKey(arg.Offset))
            return;

        var v = ValueOf(arg);
        string tooltip = null;
        Action go = null;

        switch (def.Role)
        {
            case EmevdArgRole.Entity when v != 0 && v != -1:
            {
                var map = MapOf(View.Selection.SelectedFile);
                if (map != null && Project.Handler.MapEditor != null)
                {
                    var kind = def.RoleType?.Contains("Enemy") == true ? "enemy" : "any";
                    tooltip = LOC.Get("EMEVD_Role_Link_Entity_TT", v, map);
                    go = () =>
                    {
                        EditorCommandQueue.AddCommand($"map/load/{map}");
                        EditorCommandQueue.AddCommand($"map/idselect/{kind}/{map}/{v}");
                    };
                }
                else
                {
                    tooltip = LOC.Get("EMEVD_Role_Link_Entity_Search_TT", v);
                    go = () => View.ToolWindow.SearchTool.SearchValue(v);
                }
                break;
            }

            case EmevdArgRole.EventFlag when v > 0:
                tooltip = LOC.Get("EMEVD_Role_Link_Flag_TT", v);
                go = () => View.ToolWindow.SearchTool.SearchValue(v);
                break;

            case EmevdArgRole.Param when v >= 0 && Project.Handler.ParamEditor != null:
            {
                var param = ParamOf(line, arg);
                if (param != null)
                {
                    tooltip = LOC.Get("EMEVD_Role_Link_Param_TT", param, v);
                    go = () => EditorCommandQueue.AddCommand($"param/select/-1/{param}/{v}");
                }
                break;
            }

            case EmevdArgRole.Text when v >= 0:
            {
                var text = FindText(def.RoleType, v);
                if (text?.ContainerWrapper != null)
                {
                    tooltip = LOC.Get("EMEVD_Role_Link_Text_TT", def.RoleType, v);
                    go = () => EditorCommandQueue.AddCommand(
                        $"text/select/{text.ContainerWrapper.ContainerDisplayCategory}/{text.ContainerWrapper.FileEntry.Filename}/{text.FmgName}/{text.Entry.ID}");
                }
                break;
            }

            case EmevdArgRole.MapId:
            case EmevdArgRole.MapPart:
            {
                // One button per line for a split map ID: on its Area arg
                if (def.Role == EmevdArgRole.MapPart && def.RoleType != "Area")
                    break;

                var map = MapName(line, arg);
                if (map != null && Project.Handler.MapEditor != null)
                {
                    tooltip = LOC.Get("EMEVD_Role_Link_Map_TT", map);
                    go = () => EditorCommandQueue.AddCommand($"map/load/{map}");
                }
                break;
            }
        }

        if (go == null)
            return;

        if (ImGui.Button($"{Icons.ExternalLink}##{id}"))
            go();
        GUI.Tooltip(tooltip);
    }

    public static string RoleName(EmevdArgRole role) => LOC.Get($"EMEVD_Role_{role}");

    private Andre.Formats.Param.Row FindRow(string param, long id)
    {
        var bank = Project.Handler.ParamEditor?.ViewHandler?.ActiveView?.GetPrimaryBank();
        if (bank?.Params == null || id > int.MaxValue || !bank.Params.TryGetValue(param, out var p))
            return null;

        return p[(int)id];
    }

    private TextResult FindText(string fmg, long id)
    {
        var view = Project.Handler.TextEditor?.ViewHandler?.ActiveView;
        if (view == null || fmg == null || id > int.MaxValue)
            return null;

        return TextFinder.GetTextResult(view, fmg, (int)id);
    }

    private static string Shorten(string text)
    {
        var oneLine = text.Replace("\r", "").Replace('\n', ' ');
        return oneLine.Length > 60 ? oneLine[..57] + "..." : oneLine;
    }
}
