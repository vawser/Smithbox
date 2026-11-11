using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Palette: every instruction of the game's layouts, by class. Double-click inserts one after the bottom selected
/// block (args at their defaults); drag one onto the Script window to insert it there.
/// </summary>
public class EmevdPaletteTool
{
    private readonly EmevdEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    /// <summary>The instruction being dragged onto the Script window</summary>
    public EmevdInstrDef DragDef;

    public EmevdPaletteTool(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdCatalog Catalog => View.Project.Handler.EmevdData?.Catalog;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("EMEVD_PaletteTool_Header")}##emevdPaletteToolHeader"))
            return;

        ImGui.BeginChild("EmevdPaletteToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private unsafe void Display()
    {
        GUI.WrappedText(LOC.Get("EMEVD_PaletteTool_Hint"));

        var catalog = Catalog;
        if (catalog == null)
        {
            GUI.WrappedText(LOC.Get("EMEVD_PaletteTool_No_Layouts"));
            return;
        }

        var canInsert = View.BlockView.CurrentScript != null;
        if (!canInsert)
            GUI.WrappedText(LOC.Get("EMEVD_PaletteTool_No_Event"));

        // Instructions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_PaletteTool_Instructions_Header"), LOC.Get("EMEVD_PaletteTool_Instructions_Header_TT"));

        EditorFilters.DisplayFramedListFilter("emevdPaletteFilter", ref Filter, ref ExactFilter);

        ImGui.BeginChild("emevdPaletteList", Vector2.Zero, ImGuiChildFlags.Borders);

        foreach (var cls in catalog.Classes.Values.OrderBy(c => c.Bank))
        {
            var instrs = cls.Instructions.Values
                .Where(i => Filter.Length == 0 || EditorFilters.IsMatch(Filter, i.DisplayName, ExactFilter, i.Key))
                .OrderBy(i => i.ID)
                .ToList();

            if (instrs.Count == 0)
                continue;

            if (Filter.Length > 0)
                ImGui.SetNextItemOpen(true, ImGuiCond.Always);

            if (!ImGui.TreeNodeEx($"{cls.DisplayName}##emevdPaletteClass{cls.Bank}", ImGuiTreeNodeFlags.SpanAvailWidth))
                continue;

            foreach (var def in instrs)
            {
                ImGui.Selectable($"{def.DisplayName}##emevdPalette{def.Key}", false, ImGuiSelectableFlags.AllowDoubleClick);

                if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && canInsert)
                    View.Structure.Insert(def);

                if (ImGui.BeginDragDropSource())
                {
                    DragDef = def;
                    byte dummy = 0;
                    ImGui.SetDragDropPayload(EmevdBlockView.PalettePayload, &dummy, 1);
                    ImGui.Text(def.DisplayName);
                    ImGui.EndDragDropSource();
                }

                GUI.Tooltip(LOC.Get("EMEVD_PaletteTool_Item_TT", def.Key, def.Confidence, def.Uses));

                ImGui.SameLine();
                ImGui.TextDisabled(def.Key);
            }

            ImGui.TreePop();
        }

        ImGui.EndChild();
    }
}
