#nullable enable
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// Named groups of bones per animation set (the Bones tool's Selection Sets): one click selects the group, so W10's
/// multi-bone keying, Clear Keys, Copy Offset and the gizmo act on all of it. Kept in
/// <c>.smithbox/Animations/&lt;set&gt;/Selections.json</c> by bone name. A set without the file gets defaults made from
/// the skeleton's names (arms, legs, spine, head, fingers), written to the file on the first change.
/// </summary>
public class AnimationSelectionSets
{
    private readonly AnimationEditorView View;

    private const string FileName = "Selections.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    // The sets of the open work folder (null: not read yet), and the folder they were read from
    private List<SetDto>? Sets;
    private string? SetsFolder;
    private HavokSkeletonData? SetsSkeleton;

    private string NewName = "";
    private int Renaming = -1;
    private string RenameText = "";
    private bool RenameFocus;

    public AnimationSelectionSets(AnimationEditorView view)
    {
        View = view;
    }

    public void Display(HavokSkeletonData skeleton)
    {
        var folder = View.Bank.WorkFolder;
        if (folder == null)
            return;

        if (Sets == null || SetsFolder != folder || !ReferenceEquals(SetsSkeleton, skeleton))
            Load(folder, skeleton);

        var sets = Sets!;
        if (!ImGui.TreeNodeEx($"{LOC.Get("ANIM_Sets_Header", sets.Count)}##animationSelectionSets"))
            return;

        var nameToBone = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var b = 0; b < skeleton.BoneCount; b++)
            nameToBone.TryAdd(skeleton.Names[b], b);

        for (var i = 0; i < sets.Count; i++)
        {
            var set = sets[i];
            var bones = set.Bones.Select(n => nameToBone.TryGetValue(n, out var b) ? b : -1).Where(b => b >= 0).ToList();

            if (Renaming == i)
            {
                ImGui.SetNextItemWidth(-1);
                if (RenameFocus)
                {
                    ImGui.SetKeyboardFocusHere();
                    RenameFocus = false;
                }

                ImGui.InputText($"##animationSetRename_{i}", ref RenameText, 64);
                if (GUI.InputCommitted())
                {
                    var name = RenameText.Trim();
                    if (name.Length > 0 && name != set.Name)
                    {
                        set.Name = name;
                        Save(folder);
                    }

                    Renaming = -1;
                }
                else if (ImGui.IsItemDeactivated())
                {
                    // Left without an edit, or with Escape
                    Renaming = -1;
                }

                continue;
            }

            var selected = bones.Count > 0 && bones.All(View.IsBoneSelected);
            if (ImGui.Selectable($"{set.Name} ({bones.Count})##animationSet_{i}", selected))
                Select(bones, ImGui.GetIO().KeyCtrl);
            GUI.Tooltip(LOC.Get("ANIM_Sets_Set_TT", string.Join(", ", set.Bones.Take(12)) + (set.Bones.Count > 12 ? ", ..." : "")));

            if (ImGui.BeginPopupContextItem($"##animationSetMenu_{i}"))
            {
                ImGui.BeginDisabled(View.SelectedBones.Count == 0);
                if (ImGui.MenuItem($"{LOC.Get("ANIM_Sets_Update")}##animationSetUpdate_{i}"))
                {
                    set.Bones = View.SelectedBones.Select(b => skeleton.Names[b]).ToList();
                    Save(folder);
                }
                ImGui.EndDisabled();

                if (ImGui.MenuItem($"{LOC.Get("ANIM_Sets_Rename")}##animationSetRenameItem_{i}"))
                {
                    Renaming = i;
                    RenameText = set.Name;
                    RenameFocus = true;
                }

                if (ImGui.MenuItem($"{LOC.Get("ANIM_Sets_Remove")}##animationSetRemove_{i}"))
                {
                    sets.RemoveAt(i);
                    Save(folder);
                    ImGui.EndPopup();
                    break;
                }

                ImGui.EndPopup();
            }
        }

        if (sets.Count == 0)
            ImGui.TextDisabled(LOC.Get("ANIM_Sets_None"));

        // A new set from the selected bones: named in the field, added with the button
        var addWidth = ImGui.CalcTextSize(LOC.Get("ANIM_Sets_Add")).X + ImGui.GetStyle().FramePadding.X * 2;
        ImGui.SetNextItemWidth(-addWidth - ImGui.GetStyle().ItemSpacing.X);
        ImGui.InputTextWithHint("##animationSetNewName", LOC.Get("ANIM_Sets_New_Hint"), ref NewName, 64);

        ImGui.SameLine();
        ImGui.BeginDisabled(View.SelectedBones.Count == 0 || NewName.Trim().Length == 0);
        if (ImGui.Button($"{LOC.Get("ANIM_Sets_Add")}##animationSetAdd"))
        {
            sets.Add(new SetDto { Name = NewName.Trim(), Bones = View.SelectedBones.Select(b => skeleton.Names[b]).ToList() });
            NewName = "";
            Save(folder);
        }
        ImGui.EndDisabled();
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(LOC.Get("ANIM_Sets_Add_TT"));

        if (ImGui.SmallButton($"{LOC.Get("ANIM_Sets_Defaults")}##animationSetDefaults"))
        {
            sets.Clear();
            sets.AddRange(Defaults(skeleton));
            Save(folder);
        }
        GUI.Tooltip(LOC.Get("ANIM_Sets_Defaults_TT"));

        ImGui.TreePop();
    }

    /// <summary>The sets of the open character with their bones, read first if they haven't been (for the Layers tool's masks)</summary>
    public List<(string Name, List<int> Bones)> Resolved(HavokSkeletonData skeleton)
    {
        if (View.Bank.WorkFolder is not { } folder)
            return new();

        if (Sets == null || SetsFolder != folder || !ReferenceEquals(SetsSkeleton, skeleton))
            Load(folder, skeleton);

        var nameToBone = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var b = 0; b < skeleton.BoneCount; b++)
            nameToBone.TryAdd(skeleton.Names[b], b);

        return Sets!.Select(s => (s.Name, s.Bones.Select(n => nameToBone.TryGetValue(n, out var b) ? b : -1).Where(b => b >= 0).ToList()))
            .ToList();
    }

    /// <summary>
    /// Selects the bones, the first as the primary one (the gizmo's); with <paramref name="add" /> they join the
    /// selection instead (bones already in it stay)
    /// </summary>
    private void Select(List<int> bones, bool add)
    {
        if (bones.Count == 0)
            return;

        if (add)
        {
            foreach (var bone in bones.AsEnumerable().Reverse())
            {
                if (!View.IsBoneSelected(bone))
                    View.SelectBone(bone, true);
            }

            return;
        }

        // Each added bone becomes the primary one, so the first is added last
        View.SelectBone(bones[^1]);
        for (var i = bones.Count - 2; i >= 0; i--)
            View.SelectBone(bones[i], true);
    }

    // ---- file

    private void Load(string folder, HavokSkeletonData skeleton)
    {
        SetsFolder = folder;
        SetsSkeleton = skeleton;
        Renaming = -1;

        var path = Path.Join(folder, FileName);
        try
        {
            if (File.Exists(path) && JsonSerializer.Deserialize<FileDto>(File.ReadAllText(path), JsonOptions) is { } file)
            {
                Sets = file.Sets.Where(s => s.Name.Length > 0).ToList();
                return;
            }
        }
        catch (Exception e)
        {
            Smithbox.Log(this, LOC.Get("ANIM_Sets_Read_Failed", path), LogLevel.Warning, e);
        }

        Sets = Defaults(skeleton);
    }

    private void Save(string folder)
    {
        var path = Path.Join(folder, FileName);
        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(path, JsonSerializer.Serialize(new FileDto { Sets = Sets ?? new() }, JsonOptions));
        }
        catch (Exception e)
        {
            Smithbox.Log(this, LOC.Get("ANIM_Sets_Write_Failed", path), LogLevel.Warning, e);
        }
    }

    // ---- defaults

    /// <summary>
    /// Sets made from the skeleton's names: each arm and leg down its main chain, the spine, neck and head, each
    /// hand's fingers. Names are matched ignoring case, spaces and underscores (L_Thigh = L Thigh). A set none of
    /// whose bones exist is left out.
    /// </summary>
    public static List<SetDto> Defaults(HavokSkeletonData skeleton)
    {
        var byKey = new Dictionary<string, string>();
        for (var b = 0; b < skeleton.BoneCount; b++)
            byKey.TryAdd(Key(skeleton.Names[b]), skeleton.Names[b]);

        List<string> Named(params string[] names) =>
            names.Select(n => byKey.TryGetValue(Key(n), out var found) ? found : null).OfType<string>().ToList();

        List<string> Matching(Func<string, bool> match) =>
            skeleton.Names.Where(n => match(Key(n))).ToList();

        var sets = new List<SetDto>();
        void Add(string locKey, List<string> bones)
        {
            if (bones.Count > 0)
                sets.Add(new SetDto { Name = LOC.Get(locKey), Bones = bones });
        }

        Add("ANIM_Sets_Default_Spine", Matching(k => k.StartsWith("spine")));
        Add("ANIM_Sets_Default_Head", Named("Neck", "Neck1", "Head"));
        Add("ANIM_Sets_Default_Left_Arm", Named("L_Clavicle", "L_UpperArm", "L_Forearm", "L_Hand"));
        Add("ANIM_Sets_Default_Right_Arm", Named("R_Clavicle", "R_UpperArm", "R_Forearm", "R_Hand"));
        Add("ANIM_Sets_Default_Left_Leg", Named("L_Thigh", "L_Calf", "L_Foot", "L_Toe0"));
        Add("ANIM_Sets_Default_Right_Leg", Named("R_Thigh", "R_Calf", "R_Foot", "R_Toe0"));
        Add("ANIM_Sets_Default_Left_Fingers", Matching(k => k.StartsWith("lfinger")));
        Add("ANIM_Sets_Default_Right_Fingers", Matching(k => k.StartsWith("rfinger")));
        return sets;
    }

    private static string Key(string name) => name.Replace(" ", "").Replace("_", "").ToLowerInvariant();

    public sealed class FileDto
    {
        public int Version { get; set; } = 1;
        public List<SetDto> Sets { get; set; } = new();
    }

    public sealed class SetDto
    {
        public string Name { get; set; } = "";
        public List<string> Bones { get; set; } = new();
    }
}
