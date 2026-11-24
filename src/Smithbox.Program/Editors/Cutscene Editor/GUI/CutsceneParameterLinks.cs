#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>What an MQB parameter's value names.</summary>
public enum CutsceneLinkKind
{
    None,

    // A TalkParam row, and the subtitle text its message field names
    Talk,

    // A row of another param
    Param,

    // An effect of the FX Editor
    Effect,

    // An event flag, searched for in the EMEVD Editor
    EventFlag,

    // A value ID of a GPARAM group: the cutscene's own file (bank 0) or its map's (bank 1)
    Gparam,

    // A sound of the Sound Editor: seId, of the category its event's seType gives (TAE SoundType numbering)
    Sound
}

/// <summary>
/// Links from MQB event and timeline parameters to what their values name: described under the value in the
/// Properties panel, and opened in the editor that owns it from a right-click menu on the value or the description,
/// as the Param Editor's field references are (Cutscene Editor.md, parameter links).
/// Roles go by parameter name, which is the same in every game:
/// <list type="bullet">
/// <item>talkParamId: the TalkParam row and its subtitle text (the row's field with an FmgRef, else msgId in
/// Talk_Messages).</item>
/// <item>phantomParamId, materialExParamId, wetParamId, decalId, partsDrawId, overlayMaterialParamId: rows of their
/// params.</item>
/// <item>sfxId: the FX Editor. EventFlagId: an EMEVD Editor search.</item>
/// <item>seId: a sound, of the category seType gives (the TAE SoundType numbering: 4 is (p) Cutscene SE), with a play
/// button, its alias or Play event, and the Sound Editor's Play / Go to entries.</item>
/// <item>A ...Bank parameter followed by an ...Id one (beginToneMapBank, beginToneMapId): a value ID of the GPARAM group
/// the name gives. Bank 0 (or -1) is the cutscene's own sAA_BB_CCCC.gparam and bank 1 its map's mAA_BB_0000.gparam,
/// found by Claude/Tools/CutsceneGparamProbe in all five games.</item>
/// </list>
/// The timeline's areaNo/blockNo/mapNo/altNo name a map, opened in the Map Editor. Names come from the other editors'
/// data when it is loaded, and are left out when it isn't.
/// </summary>
public class CutsceneParameterLinks
{
    private readonly CutsceneEditorView Editor;

    // GPARAMs read for descriptions, by file name; null when the project has no such file
    private readonly Dictionary<string, GPARAM?> GparamCache = new(StringComparer.OrdinalIgnoreCase);

    public CutsceneParameterLinks(CutsceneEditorView editor)
    {
        Editor = editor;
        Talk = new CutsceneTalk(editor.Project);
    }

    private ProjectEntry Project => Editor.Project;

    private static readonly Dictionary<string, string[]> ParamRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["phantomParamId"] = ["PhantomParam"],
        ["dstPhantomParamId"] = ["PhantomParam"],
        ["materialExParamId"] = ["MaterialExParam"],
        ["wetParamId"] = ["WetAspectParam"],
        ["decalId"] = ["DecalParam"],
        ["partsDrawId"] = ["PartsDrawParam"],
        ["overlayMaterialParamId"] = ["OverlayMaterialParam"]
    };

    /// <summary>The role of parameter <paramref name="index" />, and for a GPARAM pair its bank and group stem</summary>
    public static (CutsceneLinkKind Kind, int Bank, string Stem) RoleOf(IReadOnlyList<MQB.Parameter> parameters, int index)
    {
        var name = parameters[index].Name;

        if (name.Equals("talkParamId", StringComparison.OrdinalIgnoreCase))
            return (CutsceneLinkKind.Talk, 0, "");
        if (ParamRoles.ContainsKey(name))
            return (CutsceneLinkKind.Param, 0, "");
        if (name.Equals("sfxId", StringComparison.OrdinalIgnoreCase))
            return (CutsceneLinkKind.Effect, 0, "");
        if (name.Equals("EventFlagId", StringComparison.OrdinalIgnoreCase))
            return (CutsceneLinkKind.EventFlag, 0, "");

        // The sound's category is the event's seType (4 = (p) Cutscene SE, the usual one); -1 when it has none
        if (name.Equals("seId", StringComparison.OrdinalIgnoreCase))
        {
            var type = parameters.FirstOrDefault(p => p.Name.Equals("seType", StringComparison.OrdinalIgnoreCase));
            return (CutsceneLinkKind.Sound, type != null ? ToInt(type.Value) ?? -1 : -1, "");
        }

        // An Id straight after a Bank: begin/end/dst + group + Id
        if (index > 0 && Regex.IsMatch(name, "I[dD]$") &&
            parameters[index - 1].Name.EndsWith("Bank", StringComparison.OrdinalIgnoreCase))
        {
            var stem = Regex.Replace(name, "^(begin|end|dst)|I[dD]$", "");
            return (CutsceneLinkKind.Gparam, ToInt(parameters[index - 1].Value) ?? 0, stem);
        }

        return (CutsceneLinkKind.None, 0, "");
    }

    // What the parameter being shown names (lines under its value), and where it can go (its right-click menu)
    private readonly List<(string Text, bool Quote)> Lines = new();
    private readonly List<(string Label, Action Go)> Menu = new();

    /// <summary>
    /// Call straight after a parameter's value widget: what the value names, as lines under it, and a right-click
    /// menu on the value and on those lines that goes to it in the editor that owns it (as the Param Editor's field
    /// references do). Nothing for a parameter with no role or a value of none (-1, or 0 for rows and effects).
    /// </summary>
    public void Display(string id, IReadOnlyList<MQB.Parameter> parameters, int index)
    {
        Lines.Clear();
        Sound = null;
        Menu.Clear();

        // A timeline's map numbers: the map's menu on each (its line is under the table, DisplayTimelineMap)
        if (IsMapNumber(parameters[index].Name))
        {
            if (MapOf(parameters) is { } map)
                LinkMap(map);
            Render(id, onValue: true);
            return;
        }

        var (kind, bank, stem) = RoleOf(parameters, index);
        if (kind == CutsceneLinkKind.None || ToInt(parameters[index].Value) is not { } value)
            return;

        switch (kind)
        {
            case CutsceneLinkKind.Talk:
                DescribeTalk(value);
                break;

            case CutsceneLinkKind.Param:
                DescribeParamRow(ParamRoles[parameters[index].Name], value);
                break;

            case CutsceneLinkKind.Effect when value > 0:
                Line(LOC.Get("CUTSCENE_Link_Effect", value));
                if (Project.Descriptor.EnableFxEditor)
                    Link(LOC.Get("CUTSCENE_Link_Go_Effect", value), () => EditorCommandQueue.AddCommand($"fx/select/{value}"));
                break;

            case CutsceneLinkKind.EventFlag when value > 0:
                Line(LOC.Get("CUTSCENE_Link_Flag", value));
                if (Project.Descriptor.EnableEmevdEditor)
                    Link(LOC.Get("CUTSCENE_Link_Go_Flag", value), () => EditorCommandQueue.AddCommand($"emevd/search/{value}"));
                break;

            case CutsceneLinkKind.Gparam when value >= 0:
                DescribeGparam(bank, stem, value);
                break;

            case CutsceneLinkKind.Sound when value > 0:
                DescribeSound(value, bank);
                break;
        }

        Render(id, onValue: true);
    }

    private void Line(string text, bool quote = false) => Lines.Add((text, quote));
    private void Link(string label, Action go) => Menu.Add((label, go));

    // A sound to show with a play button in front of the first line, and its entries in the menu
    private (int Id, string? Type, int Line)? Sound;

    // ---- sound

    /// <summary>
    /// A sound: its alias (Sounds aliases, by the bare ID as the Time Act Editor looks them up), else the Play event
    /// the Sound Editor's index finds for it, with the bank. The type letter comes from seType through the game's
    /// SoundType enum, falling back to every letter when it finds nothing.
    /// </summary>
    private void DescribeSound(int value, int seType)
    {
        var letter = seType >= 0 ? SoundReferenceHelper.TypeLetter(Project, seType) : null;
        Sound = (value, letter, Lines.Count);

        var label = letter != null ? $"{letter}{value:000000000}" : value.ToString();
        var alias = Project.Handler.ProjectData?.Aliases != null &&
                    Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Sounds, out var aliases) && aliases != null
            ? AliasHelper.GetAliasName(aliases, $"{value}")
            : "";

        if (!string.IsNullOrEmpty(alias))
        {
            Line(LOC.Get("CUTSCENE_Link_Sound_Named", label, alias));
            return;
        }

        if (!SoundReferenceHelper.IsAvailable(Project))
        {
            Line(LOC.Get("CUTSCENE_Link_Sound", label));
            return;
        }

        var index = Project.Handler.SoundData!.Index!;
        var matches = SoundReferenceHelper.Find(Project, value.ToString(), letter);
        Line(!index.IsBuilt
            ? LOC.Get(index.Error != null ? "CUTSCENE_Link_Sound" : "CUTSCENE_Link_Sound_Indexing", label)
            : matches.Count > 0
                ? LOC.Get("CUTSCENE_Link_Sound_Event", matches[0].Label)
                : LOC.Get("CUTSCENE_Link_Sound_Missing", label));
    }

    /// <summary>
    /// Draws the collected lines, and the menu on them; <paramref name="onValue" /> also puts it on the item drawn
    /// just before (the value widget), which has to come before any line is drawn.
    /// </summary>
    private void Render(string id, bool onValue)
    {
        var popup = $"##cutsceneLinks_{id}";
        var sound = Sound is { } s && SoundReferenceHelper.IsAvailable(Project) ? s : ((int Id, string? Type, int Line)?)null;
        var hasMenu = Menu.Count > 0 || sound != null;

        if (hasMenu && onValue)
            ImGui.OpenPopupOnItemClick(popup, ImGuiPopupFlags.MouseButtonRight);

        for (var i = 0; i < Lines.Count; i++)
        {
            var (text, quote) = Lines[i];

            // A sound plays from a button in front of its line, as the Time Act Editor's sound aliases do
            var isSoundLine = sound is { } at && at.Line == i;
            if (isSoundLine)
                SoundReferenceHelper.PlayButton(Project, sound!.Value.Id, $"cutscene_{id}", sound.Value.Type);

            if (quote)
                ImGui.TextWrapped($"\"{text}\"");
            else if (isSoundLine)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
                ImGui.TextUnformatted(text);
                ImGui.PopStyleColor();
            }
            else
                ImGui.TextDisabled(text);

            if (hasMenu)
            {
                ImGui.OpenPopupOnItemClick(popup, ImGuiPopupFlags.MouseButtonRight);
                GUI.Tooltip(LOC.Get(sound != null ? "CUTSCENE_Link_Sound_Hint" : "CUTSCENE_Link_Menu_Hint"));
            }

            if (isSoundLine && sound is { } clickable)
                SoundReferenceHelper.Click(Project, clickable.Id, clickable.Type);
        }

        if (!hasMenu || !ImGui.BeginPopup(popup))
            return;

        // Play / Go to for each event of the sound, from the shared helper
        if (sound is { } menuSound)
        {
            SoundReferenceHelper.ContextMenu(Project, menuSound.Id, menuSound.Type);
            if (Menu.Count > 0)
                ImGui.Separator();
        }

        // Coloured like the Param Editor's references
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        for (var i = 0; i < Menu.Count; i++)
        {
            if (ImGui.Selectable($"{Menu[i].Label}##{id}_link{i}"))
                Menu[i].Go();
        }
        ImGui.PopStyleColor();

        ImGui.EndPopup();
    }

    // ---- params and text

    private ParamBank? ParamBank => Project.Handler.ParamEditor?.ViewHandler?.ActiveView?.GetPrimaryBank();

    private (string? Param, Andre.Formats.Param.Row? Row) FindRow(IEnumerable<string> candidates, int id)
    {
        var bank = ParamBank;
        if (bank?.Params == null)
            return (candidates.FirstOrDefault(), null);

        foreach (var name in candidates)
        {
            if (bank.Params.TryGetValue(name, out var param))
                return (name, param[id]);
        }

        return (null, null);
    }

    private void DescribeParamRow(IEnumerable<string> candidates, int value)
    {
        if (value is 0 or -1)
            return;

        var (param, row) = FindRow(candidates, value);
        if (param == null)
            return;

        Line(row != null && !string.IsNullOrEmpty(row.Name)
            ? LOC.Get("CUTSCENE_Link_Param_Named", param, value, row.Name)
            : LOC.Get(row != null || ParamBank == null ? "CUTSCENE_Link_Param_Row" : "CUTSCENE_Link_Param_Missing", param, value));

        if (Project.Handler.ParamEditor != null && row != null)
        {
            var label = string.IsNullOrEmpty(row.Name) ? $"{param} {value}" : $"{param} {value}: {row.Name}";
            Link(LOC.Get("CUTSCENE_Link_Go_Param", label), () => EditorCommandQueue.AddCommand($"param/select/-1/{param}/{value}"));
            Link(LOC.Get("CUTSCENE_Link_Go_Param_New_View", label), () => EditorCommandQueue.AddCommand($"param/select/new/{param}/{value}"));
        }
    }

    private void DescribeTalk(int value)
    {
        if (value <= 0)
            return;

        DescribeParamRow(["TalkParam"], value);

        var row = Talk.Row(value);
        if (row == null)
            return;

        if (Talk.Text(row) is { } text)
        {
            if (text.Entry?.Text is { Length: > 0 } t)
                Line(Shorten(t), quote: true);
            else
                Line(LOC.Get("CUTSCENE_Link_Text_Empty"));

            if (text.ContainerWrapper != null && text.Entry != null)
                Link(LOC.Get("CUTSCENE_Link_Go_Text", text.Entry.ID), () => EditorCommandQueue.AddCommand(
                    $"text/select/{text.ContainerWrapper.ContainerDisplayCategory}/{text.ContainerWrapper.FileEntry.Filename}/{text.FmgName}/{text.Entry.ID}"));
        }

        // The voice the line plays (and the viewport plays with the subtitle): a sound of type (v)
        if (Talk.Voice(row) is { } voice)
        {
            Sound = (voice.Id, CutsceneTalk.VoiceLetter, Lines.Count);
            Line(LOC.Get("CUTSCENE_Link_Voice", voice.Field, $"{CutsceneTalk.VoiceLetter}{voice.Id:000000000}"));
        }
    }

    private readonly CutsceneTalk Talk;

    // ---- GPARAM

    /// <summary>The GPARAM file a bank names for the selected cutscene: its own (0, -1) or its map's (1)</summary>
    public string? GparamFileFor(int bank)
    {
        var file = Editor.SelectedFile;
        if (file == null)
            return null;

        if (bank is 0 or -1)
            return file.Name;

        if (bank != 1)
            return null;

        // The map's own file is mAA_BB_0000 (map GPARAMs are not per tile: ER has m60_00_0000 for every m60 tile)
        var map = Editor.CurrentMapFor(file) ?? CutsceneMapBackdrop.ImpliedMap(file.Name);
        var m = Regex.Match(map ?? file.Name, @"^[ms](\d\d)_(\d\d)");
        if (!m.Success)
            return null;

        var name = $"m{m.Groups[1].Value}_{m.Groups[2].Value}_0000";
        return Gparam(name) != null ? name : $"m{m.Groups[1].Value}_00_0000";
    }

    /// <summary>A GPARAM by file name: the Gparam Editor's copy if it has the file open (with its edits), else read here once</summary>
    private GPARAM? Gparam(string name)
    {
        var bank = Project.Handler.GparamData?.PrimaryBank;
        if (bank == null)
            return null;

        var entry = bank.Entries.FirstOrDefault(e => e.Key.Filename.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (entry.Key == null)
            return null;

        if (entry.Value.IsLoaded())
            return entry.Value.Gparam;

        if (!GparamCache.TryGetValue(name, out var gparam))
        {
            if (!bank.TryReadGraphicsParam(entry.Key, out gparam))
                gparam = null;
            GparamCache[name] = gparam;
        }

        return gparam;
    }

    private static string Norm(string s) => Regex.Replace(s.ToLowerInvariant(), "[^a-z0-9]", "");

    // Stems whose group has another name. LightSetAll and LightSetModel are the LightSet group, WetAll is Wet.
    private static readonly Dictionary<string, string> GroupAliases = new()
    {
        ["light"] = "lightset",
        ["fog"] = "fogparam",
        ["scattering"] = "lightscattering"
    };

    /// <summary>The group of a GPARAM a stem names (ToneMap → "Tone Map[Yebis]"): one starting with it before one containing it</summary>
    public static GPARAM.Param? GroupOf(GPARAM gparam, string stem)
    {
        var n = Regex.Replace(Norm(stem), "(all|model)$", "");
        if (n.Length == 0)
            return null;
        n = GroupAliases.GetValueOrDefault(n, n);

        return gparam.Params.FirstOrDefault(p => Norm(p.Key).StartsWith(n)) ??
               gparam.Params.FirstOrDefault(p => Norm(p.Key).Contains(n));
    }

    private void DescribeGparam(int bank, string stem, int value)
    {
        var fileName = GparamFileFor(bank);
        if (fileName == null)
        {
            Line(LOC.Get("CUTSCENE_Link_Gparam_Bank_Unknown", bank));
            return;
        }

        var gparam = Gparam(fileName);
        if (gparam == null)
        {
            Line(LOC.Get(Project.Handler.GparamData == null ? "CUTSCENE_Link_Gparam_No_Data" : "CUTSCENE_Link_Gparam_No_File", fileName));
            return;
        }

        // A pair with no group name (InGameDrawingParameterSetting's beginBank/beginId) is the whole file at that ID
        var group = stem.Length > 0 ? GroupOf(gparam, stem) : null;
        IEnumerable<GPARAM.Param> groups = group != null ? [group] : stem.Length == 0 ? gparam.Params : [];
        var fields = groups.SelectMany(g => g.Fields.Where(f => f.Values.Any(v => v.ID == value)).Select(f => (Group: g, Field: f))).ToList();

        if (stem.Length > 0 && group == null)
            Line(LOC.Get("CUTSCENE_Link_Gparam_No_Group", fileName, stem));
        else if (fields.Count == 0)
            Line(LOC.Get("CUTSCENE_Link_Gparam_No_Id", fileName, group?.Key ?? "", value));
        else
            Line(group != null
                ? LOC.Get("CUTSCENE_Link_Gparam_Group", fileName, group.Key, value, fields.Count)
                : LOC.Get("CUTSCENE_Link_Gparam_File", fileName, value, fields.Select(f => f.Group).Distinct().Count()));

        if (!Project.Descriptor.EnableGparamEditor)
            return;

        // gparam/view/<file>/<group>/<field>/<id>: the first field holding the ID, else the group, else the file
        var target = fields.Count > 0
            ? $"gparam/view/{fileName}/{fields[0].Group.Key}/{fields[0].Field.Key}/{value}"
            : group != null ? $"gparam/view/{fileName}/{group.Key}" : $"gparam/view/{fileName}";
        Link(LOC.Get(fields.Count > 0 ? "CUTSCENE_Link_Go_Gparam_Value" : "CUTSCENE_Link_Go_Gparam", fileName, group?.Key ?? "", value),
            () => EditorCommandQueue.AddCommand(target));
    }

    // ---- the timeline's map

    /// <summary>The map a timeline's areaNo/blockNo/mapNo/altNo name (mAA_BB_CC_DD, unset parts as 00), or null</summary>
    public static string? MapOf(IReadOnlyList<MQB.Parameter> parameters)
    {
        int? Get(string name) => parameters.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is { } p ? ToInt(p.Value) : null;

        var area = Get("areaNo");
        var block = Get("blockNo");
        if (area is not >= 0 || block is not >= 0)
            return null;

        static int Part(int? v) => v is >= 0 ? v.Value : 0;
        return $"m{area:00}_{block:00}_{Part(Get("mapNo")):00}_{Part(Get("altNo")):00}";
    }

    private static bool IsMapNumber(string name) =>
        name.Equals("areaNo", StringComparison.OrdinalIgnoreCase) || name.Equals("blockNo", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("mapNo", StringComparison.OrdinalIgnoreCase) || name.Equals("altNo", StringComparison.OrdinalIgnoreCase);

    // The Map Editor link for a timeline's map, if the project has the map
    private void LinkMap(string map)
    {
        if (Project.Handler.MapEditor != null && Editor.Backdrop.HasMap(map))
            Link(LOC.Get("CUTSCENE_Link_Go_Map", map), () => EditorCommandQueue.AddCommand($"map/load/{map}"));
    }

    /// <summary>Under a timeline's parameters: the map they name with its alias, and the menu that loads it</summary>
    public void DisplayTimelineMap(string id, IReadOnlyList<MQB.Parameter> parameters)
    {
        Lines.Clear();
        Sound = null;
        Menu.Clear();

        if (MapOf(parameters) is not { } map)
            return;

        var alias = AliasHelper.GetMapNameAlias(Project, map);
        Line(string.IsNullOrEmpty(alias) ? LOC.Get("CUTSCENE_Link_Map", map) : LOC.Get("CUTSCENE_Link_Map_Named", map, alias));
        LinkMap(map);
        Render(id, onValue: false);
    }

    // ---- helpers

    private static int? ToInt(object? value) => value switch
    {
        sbyte v => v,
        byte v => v,
        short v => v,
        ushort v => v,
        int v => v,
        uint v => v <= int.MaxValue ? (int)v : null,
        long v => v is >= int.MinValue and <= int.MaxValue ? (int)v : null,
        _ => null
    };

    private static string Shorten(string text)
    {
        var oneLine = text.Replace("\r", "").Replace('\n', ' ');
        return oneLine.Length > 80 ? oneLine[..77] + "..." : oneLine;
    }
}
