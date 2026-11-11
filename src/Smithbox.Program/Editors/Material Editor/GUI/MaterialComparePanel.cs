using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Above a material's parameters: how it compares with the game's file (F7). Which models use it (F5) is the
/// Used By tool.
/// </summary>
public class MaterialComparePanel
{
    public MaterialEditorView View;
    public ProjectEntry Project;

    public MaterialComparePanel(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MaterialVanilla Vanilla => View.Editor.Vanilla;

    #region Vanilla
    /// <summary>
    /// The vanilla entries of the selected material by list (Params, Samplers, Textures) and name, for the
    /// Properties list's markers. Null when there is no vanilla file to compare with.
    /// </summary>
    public Dictionary<(string list, string name), object> VanillaEntries(IMaterialBinder binder, MaterialFileWrapper file)
    {
        var vanilla = file == null ? null : Vanilla.FindVanilla(binder, file.Key);
        if (vanilla == null)
            return null;

        var entries = new Dictionary<(string, string), object>();

        void Add(string list, IEnumerable items)
        {
            foreach (var item in items)
                entries.TryAdd((list, MaterialVanilla.EntryName(item) ?? ""), item);
        }

        switch (MaterialVanilla.MaterialOf(vanilla))
        {
            case MTD m:
                Add(nameof(MTD.Params), m.Params);
                Add(nameof(MTD.Textures), m.Textures);
                break;

            case MATBIN m:
                Add(nameof(MATBIN.Params), m.Params);
                Add(nameof(MATBIN.Samplers), m.Samplers);
                break;
        }

        return entries;
    }

    /// <summary>
    /// One line on how the material compares with the game's, with Revert to Vanilla when it differs, and the
    /// vanilla entries it no longer has, with Restore
    /// </summary>
    public void DrawVanilla(IMaterialBinder binder, MaterialFileWrapper file, object material)
    {
        var state = Vanilla.StateOf(binder, file);

        switch (state)
        {
            case VanillaState.Unknown:
                ImGui.TextDisabled(LOC.Get("MAT_Vanilla_Loading"));
                return;

            case VanillaState.Same:
                ImGui.TextDisabled(LOC.Get("MAT_Vanilla_Same"));
                return;

            case VanillaState.Added:
                ImGui.TextColored(UI.Current.ImGui_AuxAdded_Text, $"{Icons.Plus} {LOC.Get("MAT_Vanilla_Added")}");
                GUI.Tooltip(LOC.Get("MAT_Vanilla_Added_TT"));
                return;
        }

        var vanillaFile = Vanilla.FindVanilla(binder, file.Key);
        var vanilla = MaterialVanilla.MaterialOf(vanillaFile);

        ImGui.TextColored(UI.Current.ImGui_PrimaryChanged_Text, $"{Icons.Pencil} {LOC.Get("MAT_Vanilla_Modified")}");
        ImGui.SameLine();

        if (ImGui.SmallButton($"{LOC.Get("MAT_Vanilla_Revert_Material")}##revertMaterial"))
        {
            View.ActionManager.ExecuteAction(new MatRevertMaterial(file, material, MaterialVanilla.Copy(vanilla)));
            return;
        }
        GUI.Tooltip(LOC.Get("MAT_Vanilla_Revert_Material_TT"));

        DrawRemoved(file, material, vanilla);
    }

    /// <summary>
    /// The params and samplers/textures vanilla has that this material doesn't
    /// </summary>
    private void DrawRemoved(MaterialFileWrapper file, object material, object vanilla)
    {
        var removed = new List<(IList list, System.Reflection.PropertyInfo prop, object owner, object entry)>();

        void Compare(object owner, string propName, IList current, IList original)
        {
            var have = current.Cast<object>().Select(MaterialVanilla.EntryName).ToHashSet();
            var prop = owner.GetType().GetProperty(propName);

            foreach (var entry in original)
            {
                if (!have.Contains(MaterialVanilla.EntryName(entry)))
                    removed.Add((current, prop, owner, entry));
            }
        }

        switch (material, vanilla)
        {
            case (MTD m, MTD v):
                Compare(m, nameof(MTD.Params), m.Params, v.Params);
                Compare(m, nameof(MTD.Textures), m.Textures, v.Textures);
                break;

            case (MATBIN m, MATBIN v):
                Compare(m, nameof(MATBIN.Params), m.Params, v.Params);
                Compare(m, nameof(MATBIN.Samplers), m.Samplers, v.Samplers);
                break;
        }

        if (removed.Count == 0)
            return;

        GUI.WrappedText(LOC.Get("MAT_Vanilla_Removed", removed.Count));
        GUI.Tooltip(string.Join("\n", removed.Select(r => MaterialVanilla.EntryName(r.entry))));

        ImGui.SameLine();
        if (ImGui.SmallButton($"{LOC.Get("MAT_Vanilla_Restore")}##restoreRemoved"))
        {
            var actions = new List<EditorAction>();
            var next = new Dictionary<IList, int>();

            foreach (var (list, prop, owner, entry) in removed)
            {
                var index = next.TryGetValue(list, out var n) ? n : list.Count;
                next[list] = index + 1;
                actions.Add(new MatAddListEntry(file, prop, owner, MaterialVanilla.CopyEntry(entry), index));
            }

            View.ActionManager.ExecuteAction(new CompoundAction(actions));
        }
        GUI.Tooltip(LOC.Get("MAT_Vanilla_Restore_TT"));
    }
    #endregion
}
