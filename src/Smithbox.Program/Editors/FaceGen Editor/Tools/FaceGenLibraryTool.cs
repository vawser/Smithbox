#nullable enable
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.FileBrowser;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The Face Library tool: faces saved as files (<see cref="FaceFile"/>), in two libraries, the shared one (every
/// project and game) and the project's own, or anywhere on disk. A face is saved from the shown face, and imported
/// onto the shown face or as a new face, by <see cref="FaceImport"/>: exactly within a game, fitted and converted
/// between games.
/// <para>
/// The name field is applied only by the Save button, as in a dialog. Lists are read when the tool opens and on
/// Refresh, Save or import from file.
/// </para>
/// </summary>
public class FaceGenLibraryTool
{
    private enum Library
    {
        Shared,
        Project
    }

    private readonly FaceGenEditorView View;

    private Library SaveTo = Library.Shared;
    private string SaveName = "";
    private Andre.Formats.Param.Row? SaveNameFor;

    private Library Shown = Library.Shared;
    private string Filter = "";

    private readonly Dictionary<Library, List<FaceLibrary.Entry>> Lists = new();

    // The face picked to import: from a library, or a file opened from disk
    private FaceLibrary.Entry? Picked;

    private readonly FaceImportOptions Options = new();

    // What the last import did
    private List<string> LastNotes = new();

    public FaceGenLibraryTool(FaceGenEditorView view)
    {
        View = view;
    }

    private string Folder(Library library) =>
        library == Library.Shared ? FaceLibrary.SharedFolder : FaceLibrary.ProjectFolder(View.Project);

    private List<FaceLibrary.Entry> Entries(Library library)
    {
        if (!Lists.TryGetValue(library, out var list))
            Lists[library] = list = FaceLibrary.List(Folder(library));

        return list;
    }

    private void Refresh() => Lists.Clear();

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("FACEGEN_Tool_Face_Library")}##faceGenLibraryToolHeader"))
            return;

        ImGui.BeginChild("FaceGenLibraryToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        var param = View.ActiveParam;
        var row = View.ActiveRow;
        var paramName = View.ParamName;

        GUI.WrappedText(LOC.Get("FACEGEN_Library_Hint"));

        DisplaySave(paramName, param, row);
        DisplayLibraries();
        DisplayImport(paramName, param, row);
    }

    // ---- save

    private void DisplaySave(string? paramName, Andre.Formats.Param? param, Andre.Formats.Param.Row? row)
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FACEGEN_Library_Save_Header"), LOC.Get("FACEGEN_Library_Save_Header_TT"));

        if (param == null || row == null || paramName == null)
        {
            ImGui.TextDisabled(LOC.Get("FACEGEN_Properties_No_Face"));
            return;
        }

        // The name starts as the face's, and is the user's to change until another face is shown
        if (SaveNameFor != row)
        {
            SaveName = string.IsNullOrWhiteSpace(row.Name) ? $"{paramName} {row.ID}" : row.Name;
            SaveNameFor = row;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##faceGenLibrarySaveName", LOC.Get("FACEGEN_Properties_Name_Hint"), ref SaveName, 256);

        LibraryRadio("faceGenLibrarySaveTo", ref SaveTo);

        if (ImGui.Button($"{LOC.Get("FACEGEN_Library_Save")}##faceGenLibrarySave"))
            Save(paramName, param, row, FaceFile.NewPath(Folder(SaveTo), SaveName));
        GUI.Tooltip(LOC.Get("FACEGEN_Library_Save_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("FACEGEN_Library_Export_File")}##faceGenLibraryExport") &&
            PlatformUtils.Instance.SaveFileDialog(LOC.Get("FACEGEN_Library_Export_File"), new[] { "json" }, Folder(SaveTo), out var path))
        {
            if (!path.EndsWith(FaceFile.Extension, StringComparison.OrdinalIgnoreCase))
                path = Path.ChangeExtension(path, null) + FaceFile.Extension;

            Save(paramName, param, row, path);
        }
        GUI.Tooltip(LOC.Get("FACEGEN_Library_Export_File_TT"));
    }

    private void Save(string paramName, Andre.Formats.Param param, Andre.Formats.Param.Row row, string path)
    {
        try
        {
            FaceFile.FromRow(View.Project, paramName, param, row, SaveName.Trim()).Save(path);
            Smithbox.Log(this, LOC.Get("FACEGEN_Library_Saved", path));
            Refresh();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Smithbox.LogError(this, LOC.Get("FACEGEN_Library_Save_Failed", path), e);
        }
    }

    private static void LibraryRadio(string id, ref Library library)
    {
        if (ImGui.RadioButton($"{LOC.Get("FACEGEN_Library_Shared")}##{id}Shared", library == Library.Shared))
            library = Library.Shared;
        GUI.Tooltip(LOC.Get("FACEGEN_Library_Shared_TT"));

        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("FACEGEN_Library_Project")}##{id}Project", library == Library.Project))
            library = Library.Project;
        GUI.Tooltip(LOC.Get("FACEGEN_Library_Project_TT"));
    }

    // ---- libraries

    private void DisplayLibraries()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FACEGEN_Library_List_Header"), LOC.Get("FACEGEN_Library_List_Header_TT"));

        LibraryRadio("faceGenLibraryShown", ref Shown);

        if (ImGui.SmallButton($"{LOC.Get("FACEGEN_Library_Refresh")}##faceGenLibraryRefresh"))
            Refresh();

        ImGui.SameLine();

        if (ImGui.SmallButton($"{LOC.Get("FACEGEN_Library_Open_Folder")}##faceGenLibraryOpenFolder"))
        {
            Directory.CreateDirectory(Folder(Shown));
            FileBrowserScreen.OpenInExplorer(Folder(Shown));
        }

        ImGui.SameLine();

        if (ImGui.SmallButton($"{LOC.Get("FACEGEN_Library_Import_File")}##faceGenLibraryImportFile") &&
            PlatformUtils.Instance.OpenFileDialog(LOC.Get("FACEGEN_Library_Import_File"), new[] { "json" }, Folder(Shown), out var path))
        {
            var file = FaceFile.Load(path, out var error);

            if (file == null)
                Smithbox.Log(this, LOC.Get("FACEGEN_Library_Load_Failed", path, error ?? ""), LogLevel.Warning);
            else
                Picked = new FaceLibrary.Entry(path, file, null);
        }
        GUI.Tooltip(LOC.Get("FACEGEN_Library_Import_File_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##faceGenLibraryFilter", LOC.Get("FACEGEN_Library_Filter_Hint"), ref Filter, 256);

        var entries = Entries(Shown);
        var filter = Filter.Trim();
        var game = ProjectUtils.GetGameDirectory(View.Project);

        if (ImGui.BeginChild("faceGenLibraryList", new Vector2(0, 160 * DPI.UIScale()), ImGuiChildFlags.Borders))
        {
            if (entries.Count == 0)
                ImGui.TextDisabled(LOC.Get("FACEGEN_Library_Empty"));

            var index = 0;

            foreach (var entry in entries)
            {
                var name = entry.File?.Name ?? Path.GetFileName(entry.Path);
                var detail = entry.File != null ? $"{entry.File.Game} {entry.File.Param} {entry.File.SourceId}" : LOC.Get("FACEGEN_Library_Unreadable");

                if (filter.Length > 0 && !name.Contains(filter, StringComparison.OrdinalIgnoreCase) &&
                    !detail.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var otherGame = entry.File != null && !entry.File.Game.Equals(game, StringComparison.OrdinalIgnoreCase);

                ImGui.BeginDisabled(entry.File == null);

                if (ImGui.Selectable($"{name}##faceGenLibraryEntry{index++}", Picked?.Path == entry.Path))
                    Picked = entry;

                ImGui.EndDisabled();

                GUI.Tooltip(entry.Error ?? entry.Path);

                // The game it came from, at the end of the line (another game's in the warning colour: it is converted)
                ImGui.SameLine();
                var room = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(detail).X;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0f, room));

                if (otherGame)
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, detail);
                else
                    ImGui.TextDisabled(detail);
            }
        }

        ImGui.EndChild();
    }

    // ---- import

    private void DisplayImport(string? paramName, Andre.Formats.Param? param, Andre.Formats.Param.Row? row)
    {
        if (Picked?.File is not { } file)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("FACEGEN_Library_Import_Header", file.Name), LOC.Get("FACEGEN_Library_Import_Header_TT"));

        var game = ProjectUtils.GetGameDirectory(View.Project);
        var sameGame = file.Game.Equals(game, StringComparison.OrdinalIgnoreCase);
        var sameParam = sameGame && paramName != null && file.Param.Equals(paramName, StringComparison.OrdinalIgnoreCase);

        ImGui.TextDisabled(LOC.Get("FACEGEN_Library_From", file.Game, file.Param, file.SourceId, file.Saved.ToString("yyyy-MM-dd HH:mm")));

        if (!sameParam)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                sameGame ? LOC.Get("FACEGEN_Library_Other_Param", file.Param) : LOC.Get("FACEGEN_Library_Other_Game", file.Game));

        ImGui.Checkbox($"{LOC.Get("FACEGEN_Import_Shape")}##faceGenImportShape", ref Options.Shape);
        GUI.Tooltip(LOC.Get("FACEGEN_Import_Shape_TT"));

        ImGui.Checkbox($"{LOC.Get("FACEGEN_Import_Colours")}##faceGenImportColours", ref Options.Colours);
        GUI.Tooltip(LOC.Get("FACEGEN_Import_Colours_TT"));

        ImGui.BeginDisabled(!sameGame);
        ImGui.Checkbox($"{LOC.Get("FACEGEN_Import_Parts")}##faceGenImportParts", ref Options.Parts);
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get(sameGame ? "FACEGEN_Import_Parts_TT" : "FACEGEN_Import_Parts_Other_Game_TT"));

        ImGui.BeginDisabled(!sameParam);
        ImGui.Checkbox($"{LOC.Get("FACEGEN_Import_Other")}##faceGenImportOther", ref Options.Other);
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("FACEGEN_Import_Other_TT"));

        var editor = View.Editor.ParamEditor;
        var canImport = editor != null && param != null && row != null && paramName != null;

        ImGui.BeginDisabled(!canImport);

        if (ImGui.Button($"{LOC.Get("FACEGEN_Import_Apply")}##faceGenImportApply"))
        {
            var plan = FaceImport.Build(View.Project, paramName!, param!, row!, file, Options);
            LastNotes = plan.Notes;

            if (!FaceGenEdits.SetFields(editor!, paramName!, row!, plan.Values, LOC.Get("FACEGEN_Action_Import", file.Name, row!.ID)))
                LastNotes.Add(LOC.Get("FACEGEN_Import_Note_Nothing"));
        }
        GUI.Tooltip(LOC.Get("FACEGEN_Import_Apply_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("FACEGEN_Import_New")}##faceGenImportNew"))
        {
            var plan = FaceImport.Build(View.Project, paramName!, param!, row!, file, Options);
            LastNotes = plan.Notes;

            if (FaceGenEdits.AddFace(editor!, param!, row!, plan.Values, file.Name) is { } created)
                View.Select(paramName!, created);
        }
        GUI.Tooltip(LOC.Get("FACEGEN_Import_New_TT"));

        ImGui.EndDisabled();

        foreach (var note in LastNotes)
            GUI.WrappedText($"- {note}");
    }
}
