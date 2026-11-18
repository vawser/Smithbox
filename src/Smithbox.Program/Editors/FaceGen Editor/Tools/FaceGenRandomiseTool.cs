#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The Randomise tool: random values for the shown face (or every selected face), and variations of it: copies with
/// small random changes, for crowds of NPCs. What changes is <see cref="FaceRandomiser"/>'s; the groups ticked here
/// are the ones it may change. Every use is one undo step (variations: one for the copies, one for their changes).
/// </summary>
public class FaceGenRandomiseTool
{
    private readonly FaceGenEditorView View;

    private readonly FaceRandomiseOptions Options = new();

    // The seed of the next use, shown so a result can be made again; a new one is drawn after each use
    private int Seed = Environment.TickCount & 0xFFFF;

    private int VariationCount = 5;

    private const int MaxVariations = 100;

    public FaceGenRandomiseTool(FaceGenEditorView view)
    {
        View = view;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("FACEGEN_Tool_Randomise")}##faceGenRandomiseToolHeader"))
            return;

        ImGui.BeginChild("FaceGenRandomiseToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        var editor = View.Editor.ParamEditor;
        var param = View.ActiveParam;
        var row = View.ActiveRow;
        var paramName = View.ParamName;

        if (editor == null || param == null || row == null || paramName == null)
        {
            GUI.WrappedText(LOC.Get("FACEGEN_Properties_No_Face"));
            return;
        }

        GUI.WrappedText(LOC.Get("FACEGEN_Randomise_Hint"));

        // How
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FACEGEN_Randomise_Mode"), LOC.Get("FACEGEN_Randomise_Mode_TT"));

        if (ImGui.RadioButton($"{LOC.Get("FACEGEN_Randomise_Around_Current")}##faceGenRandomAround", Options.AroundCurrent))
            Options.AroundCurrent = true;
        GUI.Tooltip(LOC.Get("FACEGEN_Randomise_Around_Current_TT"));

        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("FACEGEN_Randomise_From_Neutral")}##faceGenRandomNeutral", !Options.AroundCurrent))
            Options.AroundCurrent = false;
        GUI.Tooltip(LOC.Get("FACEGEN_Randomise_From_Neutral_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderFloat("##faceGenRandomStrength", ref Options.Strength, 0f, 1f, LOC.Get("FACEGEN_Randomise_Strength"));

        ImGui.Checkbox($"{LOC.Get("FACEGEN_Randomise_Parts")}##faceGenRandomParts", ref Options.Parts);
        GUI.Tooltip(LOC.Get("FACEGEN_Randomise_Parts_TT"));

        ImGui.SameLine();

        ImGui.Checkbox($"{LOC.Get("FACEGEN_Randomise_Colours")}##faceGenRandomColours", ref Options.Colours);

        // Which groups
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FACEGEN_Randomise_Groups"), LOC.Get("FACEGEN_Randomise_Groups_TT"));

        var groups = FaceGenLayout.Build(View.Project, param);

        foreach (var group in groups)
        {
            var changes = !Options.LockedGroups.Contains(group.Title);

            if (ImGui.Checkbox($"{group.Title}##faceGenRandomGroup{group.Title}", ref changes))
            {
                if (changes)
                    Options.LockedGroups.Remove(group.Title);
                else
                    Options.LockedGroups.Add(group.Title);
            }
        }

        // Seed
        GUI.Spacer();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputInt($"{LOC.Get("FACEGEN_Randomise_Seed")}##faceGenRandomSeed", ref Seed);
        GUI.Tooltip(LOC.Get("FACEGEN_Randomise_Seed_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("FACEGEN_Randomise_New_Seed")}##faceGenRandomNewSeed"))
            NewSeed();

        // Go
        GUI.Spacer();

        var selected = View.ListView.GetSelectedRows();
        var targets = selected.Count > 1 ? selected : new List<Andre.Formats.Param.Row> { row };
        var width = new Vector2(-1, 0);

        var label = targets.Count > 1
            ? LOC.Get("FACEGEN_Randomise_Selected", targets.Count)
            : LOC.Get("FACEGEN_Randomise_Face");

        if (ImGui.Button($"{label}##faceGenRandomise", width))
            Randomise(editor, paramName, param, targets, groups);
        GUI.Tooltip(LOC.Get("FACEGEN_Randomise_Face_TT"));

        // Variations of the shown face
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FACEGEN_Randomise_Variations"), LOC.Get("FACEGEN_Randomise_Variations_TT"));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputInt($"{LOC.Get("FACEGEN_Randomise_Variation_Count")}##faceGenVariationCount", ref VariationCount);
        VariationCount = Math.Clamp(VariationCount, 1, MaxVariations);

        if (ImGui.Button($"{LOC.Get("FACEGEN_Randomise_Make_Variations", VariationCount)}##faceGenMakeVariations", width))
            MakeVariations(editor, paramName, param, row, groups);
        GUI.Tooltip(LOC.Get("FACEGEN_Randomise_Make_Variations_TT"));
    }

    private void NewSeed() => Seed = Random.Shared.Next(0, 0x10000);

    /// <summary>A different random face on each row, in one undo step</summary>
    private void Randomise(ParamEditorScreen editor, string paramName, Andre.Formats.Param param,
        IReadOnlyList<Andre.Formats.Param.Row> rows, List<FaceFieldGroup> groups)
    {
        var random = new Random(Seed);
        var isFemale = View.ViewportWindow.IsFemale();

        var edits = rows
            .Select(r => (r, (IReadOnlyList<(string, object)>)FaceRandomiser.Generate(View.Project, param, r, groups, Options, random, isFemale)))
            .ToList();

        var name = rows.Count == 1
            ? LOC.Get("FACEGEN_Action_Randomise", rows[0].ID)
            : LOC.Get("FACEGEN_Action_Randomise_Rows", rows.Count);

        FaceGenEdits.SetFieldsPerRow(editor, paramName, edits, name);
        NewSeed();
    }

    /// <summary>
    /// Copies of the face at the next free IDs, each then changed around the face's own values (whatever the mode),
    /// and the first one shown
    /// </summary>
    private void MakeVariations(ParamEditorScreen editor, string paramName, Andre.Formats.Param param,
        Andre.Formats.Param.Row source, List<FaceFieldGroup> groups)
    {
        var copies = FaceGenEdits.AddCopies(editor, param, source, VariationCount);

        if (copies.Count == 0)
            return;

        var around = Options.AroundCurrent;
        Options.AroundCurrent = true;

        try
        {
            Randomise(editor, paramName, param, copies, groups);
        }
        finally
        {
            Options.AroundCurrent = around;
        }

        View.Select(paramName, copies[0]);
        View.ListView.SelectMany(copies);
    }
}
