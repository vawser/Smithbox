#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The project's cutscenes, with their aliases where the project has them (by the EMEVD cutscene ID: s10_00_0020 is
/// 10000020), and the cuts of the selected one.
/// </summary>
public class CutsceneListView
{
    private readonly CutsceneEditorView Editor;

    private string Filter = "";
    public bool ScrollToSelection;

    private static readonly Regex NamePattern = new(@"^s(\d{2})_(\d{2})_(\d{4})$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private Dictionary<string, string>? Aliases;

    public CutsceneListView(CutsceneEditorView editor)
    {
        Editor = editor;
    }

    /// <summary>The EMEVD cutscene ID of a cutscene name, or null for a name of another shape.</summary>
    public static int? CutsceneId(string name)
    {
        var m = NamePattern.Match(name);
        if (!m.Success)
            return null;

        return int.Parse(m.Groups[1].Value) * 1000000 + int.Parse(m.Groups[2].Value) * 10000 + int.Parse(m.Groups[3].Value);
    }

    private string? AliasOf(string name)
    {
        if (Aliases == null)
        {
            Aliases = new(StringComparer.OrdinalIgnoreCase);
            if (Editor.Project.Handler.ProjectData?.Aliases.TryGetValue(ProjectAliasType.Cutscenes, out var entries) == true)
            {
                foreach (var entry in entries)
                    Aliases.TryAdd(entry.ID, entry.Name);
            }
        }

        return CutsceneId(name) is { } id && Aliases.TryGetValue(id.ToString(), out var alias) ? alias : null;
    }

    private string MapFilter = "";

    /// <summary>
    /// The map behind the selected cutscene: the one its name implies (sAA_BB → mAA_BB_00_00), or one picked here,
    /// which open world cutscenes need, as nothing in their files says which tile they are on.
    /// </summary>
    private void DisplayMapPicker(CutsceneFile file)
    {
        ImGui.SeparatorText(LOC.Get("CUTSCENE_List_Map"));

        var backdrop = Editor.Backdrop;
        if (!backdrop.CanLoadMaps())
        {
            ImGui.TextDisabled(LOC.Get("CUTSCENE_List_Map_Unavailable"));
            return;
        }

        var current = Editor.CurrentMapFor(file);
        var implied = CutsceneMapBackdrop.ImpliedMap(file.Name);

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##cutsceneMap", current ?? LOC.Get("CUTSCENE_List_Map_None")))
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##cutsceneMapFilter", LOC.Get("CUTSCENE_List_Filter_Hint"), ref MapFilter, 64);

            if (ImGui.Selectable(LOC.Get("CUTSCENE_List_Map_None"), current == null))
                Editor.SetMapFor(file, null);

            foreach (var map in backdrop.ListMaps())
            {
                if (MapFilter.Length > 0 && !map.Contains(MapFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                var label = map.Equals(implied, StringComparison.OrdinalIgnoreCase) ? $"{map}  ({LOC.Get("CUTSCENE_List_Map_Implied")})" : map;
                if (ImGui.Selectable($"{label}##map_{map}", map.Equals(current, StringComparison.OrdinalIgnoreCase)))
                    Editor.SetMapFor(file, map);
            }

            ImGui.EndCombo();
        }

        if (implied == null)
            ImGui.TextDisabled(LOC.Get("CUTSCENE_List_Map_Open_World"));

        if (backdrop.LoadingMapId != null)
            ImGui.TextDisabled(LOC.Get("CUTSCENE_List_Map_Loading", backdrop.LoadingMapId));
        else if (backdrop.LoadedMapId != null)
            ImGui.TextDisabled(LOC.Get("CUTSCENE_List_Map_Loaded", backdrop.LoadedMapId, backdrop.PieceCount, backdrop.PartCount));
        else if (!CFG.Current.CutsceneEditor.CutsceneEditor_Load_Map)
            ImGui.TextDisabled(LOC.Get("CUTSCENE_List_Map_Off"));
    }

    public void Display()
    {
        var files = Editor.Data?.Files;
        if (files == null)
            return;

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##cutsceneFilter", LOC.Get("CUTSCENE_List_Filter_Hint"), ref Filter, 128);

        var selected = Editor.SelectedFile;

        // The cuts of the selected cutscene, above the list so they stay in view
        if (selected?.Mqb is { } mqb)
        {
            ImGui.SeparatorText(LOC.Get("CUTSCENE_List_Cuts", selected.Name));
            for (var i = 0; i < mqb.Cuts.Count; i++)
            {
                var cut = mqb.Cuts[i];
                var seconds = cut.Duration / (mqb.Framerate > 0 ? mqb.Framerate : 30);
                if (ImGui.Selectable($"{cut.Name}  ({cut.Duration} f, {seconds:0.0} s)##cut{i}", i == Editor.SelectedCut))
                {
                    Editor.SelectCut(i);
                    Section = ListSection.Cuts;
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    Section = ListSection.Cuts;

                if (ImGui.BeginPopupContextItem($"##cutMenu{i}"))
                {
                    CutMenu(selected, i);
                    ImGui.EndPopup();
                }
            }

            DisplayMapPicker(selected);
        }
        else if (selected?.LoadError is { } error)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, error);
        }

        ImGui.SeparatorText(LOC.Get("CUTSCENE_List_Cutscenes", files.Count));

        if (ImGui.BeginChild("##cutsceneListChild"))
        {
            foreach (var file in files)
            {
                var alias = AliasOf(file.Name);

                // A search with "modified" / "added" goes through the shared filter (its + and | too); others keep the
                // plain name / alias match
                if (EditorFilters.HasStatusTerm(Filter))
                {
                    var match = EditorFilters.IsMatch(Filter, file.Name, false,
                        () => file.IsNew ? VanillaStatus.Added : Editor.Project.VFS.FileStatus(file.Entry.Path), alias ?? "");
                    if (!match)
                        continue;
                }
                else if (Filter.Length > 0
                    && !file.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase)
                    && !(alias?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false))
                    continue;

                var isSelected = file == selected;

                // Against the game's file: a new (unsaved) cutscene is Added, else the saved file's status
                var vanillaStatus = file.IsNew ? VanillaStatus.Added : Editor.Project.VFS.FileStatus(file.Entry.Path);
                var coloured = VanillaMarkers.PushColour(vanillaStatus);

                // The ## suffix keeps the ID the same as the changed marker comes and goes
                var clicked = ImGui.Selectable($"{file.Name}{(file.IsDirty || file.PendingRevert ? "*" : "")}##cutscene_{file.Name}", isSelected);

                VanillaMarkers.PopColour(coloured);
                if (coloured)
                    GUI.Tooltip(file.IsNew ? LOC.Get("CUTSCENE_File_New_TT") : VanillaMarkers.TooltipText(vanillaStatus) ?? "");

                if (clicked)
                {
                    Editor.Select(file, 0);
                    Section = ListSection.Files;
                }

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    Section = ListSection.Files;

                if (ImGui.BeginPopupContextItem($"##cutsceneMenu_{file.Name}"))
                {
                    FileMenu(file);
                    ImGui.EndPopup();
                }

                if (isSelected && ScrollToSelection)
                {
                    ImGui.SetScrollHereY();
                    ScrollToSelection = false;
                }

                Markers(file);

                if (alias != null)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);
                }
            }
        }

        ImGui.EndChild();

        DrawNamePopup();
    }

    /// <summary>A pending revert gets an icon; new / modified / added files are shown by the label's colour</summary>
    private void Markers(CutsceneFile file)
    {
        if (file.PendingRevert)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, Icons.Undo);
            GUI.Tooltip(LOC.Get("CUTSCENE_File_Reverting_TT"));
        }
    }

    // ---- structure: cutscenes and cuts added, renamed, deleted, copied and pasted (undoable, written on save)

    /// <summary>Which list the last click was in, for the Copy, Paste and Delete shortcuts</summary>
    private enum ListSection { Files, Cuts }

    private ListSection Section = ListSection.Files;

    private CutsceneData Data => Editor.Data;

    private void FileMenu(CutsceneFile file)
    {
        if (ImGui.MenuItem(LOC.Get("CUTSCENE_File_New")))
            AskName(NameMode.NewFile, file, -1, Data.SuggestName(file));
        GUI.Tooltip(LOC.Get("CUTSCENE_File_New_TT2"));

        var renameRefusal = CutsceneFileOps.RenameRefusal(file);
        if (ImGui.MenuItem(LOC.Get("CUTSCENE_File_Rename"), "", false, renameRefusal == null))
            AskName(NameMode.RenameFile, file, -1, file.Name);
        GUI.Tooltip(LOC.Get(renameRefusal ?? "CUTSCENE_File_Rename_TT"));

        var deleteRefusal = CutsceneFileOps.DeleteRefusal(Data, file);
        if (ImGui.MenuItem(LOC.Get("CUTSCENE_File_Delete"), InputManager.GetHint(KeybindID.Delete), false, deleteRefusal == null))
            DeleteFile(file);
        GUI.Tooltip(LOC.Get(CutsceneFileOps.DeleteTooltip(Data, file)));

        ImGui.Separator();

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_File_Copy"), InputManager.GetHint(KeybindID.Copy)))
            CopyFile(file);
        GUI.Tooltip(LOC.Get("CUTSCENE_File_Copy_TT"));

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_File_Paste"), InputManager.GetHint(KeybindID.Paste), false, Editor.CopiedFile != null))
            PasteFile();
        GUI.Tooltip(Editor.CopiedFile != null ? LOC.Get("CUTSCENE_File_Paste_TT", Editor.CopiedFile.Name) : LOC.Get("CUTSCENE_File_Paste_None_TT"));

        ImGui.Separator();

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_File_Copy_Name")))
            ImGui.SetClipboardText(file.Name);
    }

    private void CutMenu(CutsceneFile file, int index)
    {
        var mqb = file.Mqb!;
        var cut = mqb.Cuts[index];

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Cut_Add")))
            AskName(NameMode.NewCut, file, index, CutsceneCutOps.SuggestName(file, index));
        GUI.Tooltip(LOC.Get("CUTSCENE_Cut_Add_TT"));

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Cut_Rename")))
            AskName(NameMode.RenameCut, file, index, cut.Name);
        GUI.Tooltip(LOC.Get("CUTSCENE_Cut_Rename_TT"));

        var deleteRefusal = CutsceneCutOps.DeleteRefusal(file);
        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Cut_Delete"), InputManager.GetHint(KeybindID.Delete), false, deleteRefusal == null))
            DeleteCut(file, index);
        GUI.Tooltip(LOC.Get(deleteRefusal ?? "CUTSCENE_Cut_Delete_TT"));

        ImGui.Separator();

        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Cut_Copy"), InputManager.GetHint(KeybindID.Copy)))
            CopyCut(file, index);
        GUI.Tooltip(LOC.Get("CUTSCENE_Cut_Copy_TT"));

        var clip = Editor.CopiedCut;
        if (ImGui.MenuItem(LOC.Get("CUTSCENE_Cut_Paste"), InputManager.GetHint(KeybindID.Paste), false, clip != null))
            PasteCut(file, index);
        GUI.Tooltip(clip != null ? LOC.Get("CUTSCENE_Cut_Paste_TT", clip.Cut.Name, clip.SourceName) : LOC.Get("CUTSCENE_Cut_Paste_None_TT"));
    }

    /// <summary>Ctrl+C, Ctrl+V and Delete in the list: on the cut or the cutscene, whichever list was clicked last</summary>
    public void Shortcuts()
    {
        if (ImGui.GetIO().WantTextInput || Editor.SelectedFile is not { } file)
            return;

        if (Section == ListSection.Cuts && file.Mqb is { } mqb && Editor.SelectedCut >= 0 && Editor.SelectedCut < mqb.Cuts.Count)
        {
            if (InputManager.IsPressed(KeybindID.Copy))
                CopyCut(file, Editor.SelectedCut);
            else if (InputManager.IsPressed(KeybindID.Paste) && Editor.CopiedCut != null)
                PasteCut(file, Editor.SelectedCut);
            else if (InputManager.IsPressed(KeybindID.Delete) && CutsceneCutOps.DeleteRefusal(file) == null)
                DeleteCut(file, Editor.SelectedCut);
        }
        else if (Section == ListSection.Files)
        {
            if (InputManager.IsPressed(KeybindID.Copy))
                CopyFile(file);
            else if (InputManager.IsPressed(KeybindID.Paste) && Editor.CopiedFile != null)
                PasteFile();
            else if (InputManager.IsPressed(KeybindID.Delete) && CutsceneFileOps.DeleteRefusal(Data, file) == null)
                DeleteFile(file);
        }
    }

    /// <summary>A new cutscene beside the selected one (else the first), its name asked first</summary>
    public void NewFile()
    {
        var beside = Editor.SelectedFile ?? Data.Files.FirstOrDefault();
        if (beside != null)
            AskName(NameMode.NewFile, beside, -1, Data.SuggestName(beside));
    }

    private void CopyFile(CutsceneFile file)
    {
        Editor.CopiedFile = file;
        Smithbox.Log(this, LOC.Get("CUTSCENE_File_Copied", file.Name));
    }

    private void PasteFile()
    {
        if (Editor.CopiedFile is { } source)
            AskName(NameMode.PasteFile, source, -1, Data.SuggestName(source));
    }

    private void DeleteFile(CutsceneFile file) => Editor.Execute(CutsceneFileOps.Delete(Data, file));

    private void CopyCut(CutsceneFile file, int index)
    {
        Editor.CopiedCut = CutsceneCutOps.Copy(Editor.Project, file, file.Mqb!.Cuts[index]);
        Smithbox.Log(this, LOC.Get("CUTSCENE_Cut_Copied", file.Mqb.Cuts[index].Name, file.Name));
    }

    private void PasteCut(CutsceneFile file, int index)
    {
        if (Editor.CopiedCut != null)
            AskName(NameMode.PasteCut, file, index, CutsceneCutOps.SuggestName(file, index));
    }

    private void DeleteCut(CutsceneFile file, int index) =>
        Editor.Execute(CutsceneCutOps.Delete(Editor.Project, file, file.Mqb!.Cuts[index]));

    // ---- the name popup

    private enum NameMode { NewFile, RenameFile, PasteFile, NewCut, RenameCut, PasteCut }

    // What the popup is for, the cutscene (or, pasting one, the copied cutscene) and the cut it acts on, and the name
    private NameMode Mode;
    private CutsceneFile? NameFile;
    private int NameCut = -1;
    private string NameText = "";
    private bool OpenNamePopup;

    private void AskName(NameMode mode, CutsceneFile file, int cut, string suggestion)
    {
        Mode = mode;
        NameFile = file;
        NameCut = cut;
        NameText = suggestion;
        OpenNamePopup = true;
    }

    private void DrawNamePopup()
    {
        if (OpenNamePopup)
        {
            ImGui.OpenPopup("##cutsceneNamePopup");
            OpenNamePopup = false;
        }

        if (!ImGui.BeginPopup("##cutsceneNamePopup"))
            return;

        var file = NameFile;
        var isCut = Mode is NameMode.NewCut or NameMode.RenameCut or NameMode.PasteCut;
        var cut = isCut && file?.Mqb is { } m && NameCut >= 0 && NameCut < m.Cuts.Count ? m.Cuts[NameCut] : null;
        if (file == null || isCut && cut == null || Mode == NameMode.PasteCut && Editor.CopiedCut == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        ImGui.TextDisabled(Mode switch
        {
            NameMode.NewFile => LOC.Get("CUTSCENE_Name_New_File", file.Name),
            NameMode.RenameFile => LOC.Get("CUTSCENE_Name_Rename_File", file.Name),
            NameMode.PasteFile => LOC.Get("CUTSCENE_Name_Paste_File", file.Name),
            NameMode.NewCut => LOC.Get("CUTSCENE_Name_New_Cut", cut!.Name),
            NameMode.RenameCut => LOC.Get("CUTSCENE_Name_Rename_Cut", cut!.Name),
            _ => LOC.Get("CUTSCENE_Name_Paste_Cut", Editor.CopiedCut!.Cut.Name, Editor.CopiedCut.SourceName, cut!.Name)
        });

        ImGui.SetNextItemWidth(260f * DPI.UIScale());
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.InputText("##cutsceneNameText", ref NameText, 64);

        // Leaving the field after an edit applies it, unless the click that left it was on Cancel (checked below)
        var committed = GUI.InputCommitted();

        var name = NameText.Trim();
        var refusal = Mode switch
        {
            NameMode.NewFile or NameMode.PasteFile => Data.NameRefusal(file, name),
            NameMode.RenameFile => CutsceneFileOps.RenameRefusal(file) ?? Data.NameRefusal(file, name, file),
            NameMode.RenameCut => CutsceneCutOps.NameRefusal(file, name, cut),
            _ => CutsceneCutOps.NameRefusal(file, name)
        };

        if (refusal != null && refusal != "CUTSCENE_File_No_Change")
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get(refusal, name));
        else if (!isCut && CutsceneId(name) is { } id)
            ImGui.TextDisabled(LOC.Get("CUTSCENE_Name_Id", id));

        var label = Mode switch
        {
            NameMode.RenameFile or NameMode.RenameCut => LOC.Get("CUTSCENE_Name_Rename"),
            NameMode.PasteFile or NameMode.PasteCut => LOC.Get("CUTSCENE_Name_Paste"),
            _ => LOC.Get("CUTSCENE_Name_Create")
        };

        ImGui.BeginDisabled(refusal != null);
        var go = ImGui.Button($"{label}##cutsceneNameGo");
        ImGui.EndDisabled();

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("CUTSCENE_Name_Cancel")}##cutsceneNameCancel"))
            ImGui.CloseCurrentPopup();
        else if (go || committed && refusal == null && !ImGui.IsItemActive())
        {
            Apply(file, NameCut, name);
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    private void Apply(CutsceneFile file, int cutIndex, string name)
    {
        var project = Editor.Project;
        switch (Mode)
        {
            case NameMode.NewFile:
            case NameMode.PasteFile:
            {
                var action = Mode == NameMode.NewFile ? CutsceneFileOps.AddBlank(Data, file, name) : CutsceneFileOps.Paste(Data, file, name);
                Editor.Execute(action);
                if (action != null && Data.Files.FirstOrDefault(f => f.Name == name) is { } made)
                {
                    Editor.Select(made, 0);
                    ScrollToSelection = true;
                }
                break;
            }
            case NameMode.RenameFile:
                Editor.Execute(CutsceneFileOps.Rename(Data, file, name));
                ScrollToSelection = true;
                break;
            case NameMode.NewCut:
            case NameMode.PasteCut:
            {
                var action = Mode == NameMode.NewCut
                    ? CutsceneCutOps.Add(file, cutIndex + 1, name)
                    : CutsceneCutOps.Paste(project, file, cutIndex + 1, name, Editor.CopiedCut!);
                Editor.Execute(action);
                if (action != null && file == Editor.SelectedFile)
                    Editor.SelectCut(cutIndex + 1);
                break;
            }
            case NameMode.RenameCut:
                Editor.Execute(CutsceneCutOps.Rename(project, file, file.Mqb!.Cuts[cutIndex], name));
                break;
        }
    }
}
