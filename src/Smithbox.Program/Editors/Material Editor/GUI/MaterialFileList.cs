using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The list of each discrete material entry (.MTD or .MATBIN), in binder order, with the actions that add,
/// rename and remove them
/// </summary>
public class MaterialFileList
{
    public MaterialEditorView Parent;
    public ProjectEntry Project;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;

    // File actions: chosen in a row's context menu, opened as a popup outside the list, run after the list
    private enum FileOperation { None, Duplicate, Rename, Delete }

    private FileOperation PendingOperation = FileOperation.None;
    private FileOperation OpenOperation = FileOperation.None;
    private MaterialFileWrapper OperationFile;
    private IMaterialBinder OperationBinder;
    private string NameText = "";
    private Action DeferredAction;

    private const string NamePopupId = "###materialFileNamePopup";
    private const string DeletePopupId = "###materialFileDeletePopup";

    public MaterialFileList(MaterialEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    private MaterialBank Bank => Project.Handler.MaterialData.PrimaryBank;

    public void Draw(float width, float height)
    {
        DisplayTitle();
        DisplayHeader();
        DisplayFileList(width, height);
        DisplayPopups();

        // Structure changes wait until the list is drawn, as they change the collections it walks
        if (DeferredAction != null)
        {
            var action = DeferredAction;
            DeferredAction = null;
            action();
        }
    }

    public void DisplayTitle()
    {
        // File List
        GUI.TitleHeader(
            LOC.Get("MAT_FileList_Header_File_List"),
            LOC.Get("MAT_FileList_Header_File_List_TT"));
    }

    public void DisplayHeader()
    {
        ImGui.BeginChild("MaterialFileListHeaderSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("materialEditor_FileList",
            ref FileListFilter, ref ExactFileListFilter);

        // Display Path
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}"))
        {
            CFG.Current.MaterialEditor_FileList_DisplayFullPath = !CFG.Current.MaterialEditor_FileList_DisplayFullPath;
        }

        var displayPathMode = LOC.Get("MAT_FileList_DisplayFullPath_Toggle_Hide");
        if (CFG.Current.MaterialEditor_FileList_DisplayFullPath)
            displayPathMode = LOC.Get("MAT_FileList_DisplayFullPath_Toggle_Show");

        GUI.Tooltip(LOC.Get("MAT_FileList_DisplayFullPath_Toggle_Hint", displayPathMode));

        // Modified Only: hide the materials that are the same as the game's
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Filter}##modifiedOnlyToggle"))
        {
            CFG.Current.MaterialEditor_FileList_Modified_Only = !CFG.Current.MaterialEditor_FileList_Modified_Only;
        }

        GUI.Tooltip(LOC.Get(CFG.Current.MaterialEditor_FileList_Modified_Only
            ? "MAT_FileList_Modified_Only_On_TT"
            : "MAT_FileList_Modified_Only_Off_TT"));

        ImGui.EndChild();
    }

    public void DisplayFileList(float width, float height)
    {
        ImGui.BeginChild("FileList", new Vector2(width, height), ImGuiChildFlags.Borders);

        var binder = Parent.Selection.SelectedBinder;

        if (binder != null && (Parent.Selection.SourceType is MaterialSourceType.MTD || MaterialUtils.SupportsMATBIN(Project)))
        {
            // Binder order, unreadable files included where they are
            var modifiedOnly = CFG.Current.MaterialEditor_FileList_Modified_Only;
            var vanilla = Parent.Editor.Vanilla;

            var filteredEntries = new List<string>();
            foreach (var key in binder.FileOrder)
            {
                if (!EditorFilters.IsMatch(FileListFilter, key, ExactFileListFilter))
                    continue;

                // Modified Only keeps added and changed materials, and everything while vanilla is loading
                if (modifiedOnly)
                {
                    var file = binder.GetFile(key);
                    if (file == null || vanilla.StateOf(binder, file) == VanillaState.Same)
                        continue;
                }

                filteredEntries.Add(key);
            }

            if (Parent.Selection.ScrollToFile && Parent.Selection.SelectedFileWrapper != null)
            {
                Parent.Selection.ScrollToFile = false;
                MaterialUtils.ScrollToIndex(filteredEntries.IndexOf(Parent.Selection.SelectedFileKey));
            }

            var clipper = new ImGuiListClipper();
            clipper.Begin(filteredEntries.Count);

            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    DisplayFileRow(binder, filteredEntries[i], i);
                }
            }

            clipper.End();
        }

        ImGui.EndChild();
    }

    private void DisplayFileRow(IMaterialBinder binder, string key, int index)
    {
        var curFile = binder.GetFile(key);

        // Files that failed to read: shown, greyed, so they don't look missing
        if (curFile == null)
        {
            ImGui.TextDisabled($"{GetPrettyName(key)} {Icons.ExclamationTriangle}");
            GUI.Tooltip(LOC.Get("MAT_FileList_Unreadable_File_TT"));
            return;
        }

        var displayName = GetPrettyName(key);

        // Mark files with unsaved changes
        if (curFile.IsDirty)
        {
            displayName = $"{displayName}*";
        }

        if (ImGui.Selectable($"{displayName}##materialFileEntry_{key}", key == Parent.Selection.SelectedFileKey, ImGuiSelectableFlags.AllowDoubleClick | ImGuiSelectableFlags.AllowOverlap))
        {
            Parent.Selection.SelectFile(curFile);
            Parent.Selection.ScrollToFile = false;
        }

        // Arrow Selection, on the row's selectable
        if (ImGui.IsItemHovered() && Parent.Selection.SelectFileListEntry)
        {
            Parent.Selection.SelectFileListEntry = false;
            Parent.Selection.SelectFile(curFile);
            Parent.Selection.ScrollToFile = false;
        }

        if (ImGui.IsItemFocused())
        {
            if (InputManager.HasArrowSelection())
            {
                Parent.Selection.SelectFileListEntry = true;
            }
        }

        DisplayContextMenu(binder, curFile, index);

        // Against the game's file (F7): added by the project, or changed
        switch (Parent.Editor.Vanilla.StateOf(binder, curFile))
        {
            case VanillaState.Added:
                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_AuxAdded_Text, $"{Icons.Plus}");
                GUI.Tooltip(LOC.Get("MAT_Vanilla_Added_TT"));
                break;

            case VanillaState.Modified:
                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_PrimaryChanged_Text, $"{Icons.Pencil}");
                GUI.Tooltip(LOC.Get("MAT_Vanilla_Modified"));
                break;
        }
    }

    private void DisplayContextMenu(IMaterialBinder binder, MaterialFileWrapper file, int index)
    {
        if (!ImGui.BeginPopupContextItem($"materialFileContext_{index}"))
            return;

        // Duplicate
        if (ImGui.Selectable($"{LOC.Get("MAT_FileList_Action_Duplicate")}##duplicateFile"))
        {
            StartOperation(FileOperation.Duplicate, binder, file, UniqueName(file, "_copy"));
        }
        GUI.Tooltip(LOC.Get("MAT_FileList_Action_Duplicate_TT"));

        // Rename
        if (ImGui.Selectable($"{LOC.Get("MAT_FileList_Action_Rename")}##renameFile"))
        {
            StartOperation(FileOperation.Rename, binder, file, MaterialBank.ShortNameOf(file.Key));
        }
        GUI.Tooltip(LOC.Get("MAT_FileList_Action_Rename_TT"));

        ImGui.Separator();

        // Delete
        if (ImGui.Selectable($"{LOC.Get("MAT_FileList_Action_Delete")}##deleteFile"))
        {
            StartOperation(FileOperation.Delete, binder, file, "");
        }
        GUI.Tooltip(LOC.Get("MAT_FileList_Action_Delete_TT"));

        ImGui.EndPopup();
    }

    private void StartOperation(FileOperation operation, IMaterialBinder binder, MaterialFileWrapper file, string name)
    {
        PendingOperation = operation;
        OperationBinder = binder;
        OperationFile = file;
        NameText = name;
    }

    /// <summary>
    /// The short name plus a suffix, numbered until no material of this kind has it
    /// </summary>
    private string UniqueName(MaterialFileWrapper file, string suffix)
    {
        var stem = MaterialBank.ShortNameOf(file.Key);
        var candidate = $"{stem}{suffix}";

        for (var n = 2; Bank.IsShortNameTaken(Parent.Selection.SourceType, candidate); n++)
        {
            candidate = $"{stem}{suffix}{n}";
        }

        return candidate;
    }

    #region Popups
    private void DisplayPopups()
    {
        if (PendingOperation != FileOperation.None)
        {
            OpenOperation = PendingOperation;
            PendingOperation = FileOperation.None;
            ImGui.OpenPopup(OpenOperation == FileOperation.Delete ? DeletePopupId : NamePopupId);
        }

        DisplayNamePopup();
        DisplayDeletePopup();
    }

    /// <summary>
    /// Why a short name can't be used, or null if it can
    /// </summary>
    private string NameProblem(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return LOC.Get("MAT_FileList_Name_Empty");

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Contains('/') || name.Contains('\\'))
            return LOC.Get("MAT_FileList_Name_Invalid");

        var isOwnName = OpenOperation == FileOperation.Rename &&
            string.Equals(MaterialBank.ShortNameOf(OperationFile.Key), name, StringComparison.Ordinal);

        if (isOwnName)
            return "";

        // A case-only rename of the file itself is allowed
        var isOwnNameOtherCase = OpenOperation == FileOperation.Rename &&
            string.Equals(MaterialBank.ShortNameOf(OperationFile.Key), name, StringComparison.OrdinalIgnoreCase);

        if (!isOwnNameOtherCase && Bank.IsShortNameTaken(Parent.Selection.SourceType, name))
            return LOC.Get("MAT_FileList_Name_Taken", name);

        return null;
    }

    private void DisplayNamePopup()
    {
        var title = OpenOperation == FileOperation.Rename
            ? LOC.Get("MAT_FileList_Rename_Title")
            : LOC.Get("MAT_FileList_Duplicate_Title");

        ImGui.SetNextWindowSize(new Vector2(460, 0) * DPI.UIScale(), ImGuiCond.Always);

        var open = true;
        if (!ImGui.BeginPopupModal($"{title}{NamePopupId}", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        if (OperationFile == null || OperationBinder?.GetFile(OperationFile.Key) != OperationFile)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get(OpenOperation == FileOperation.Rename ? "MAT_FileList_Rename_Hint" : "MAT_FileList_Duplicate_Hint",
            MaterialBank.ShortNameOf(OperationFile.Key), OperationBinder.Name));

        if (OpenOperation == FileOperation.Rename)
            DisplayUsedByWarning(OperationFile);

        // Name
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAT_FileList_Name_Header"), LOC.Get("MAT_FileList_Name_Header_TT"));

        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##materialFileName", ref NameText, 255);
        GUI.Tooltip(LOC.Get("MAT_FileList_Name_Header_TT"));
        var entered = ImGui.IsItemDeactivatedAfterEdit() && ImGui.IsKeyPressed(ImGuiKey.Enter);

        var name = NameText.Trim();
        var problem = NameProblem(name);

        if (!string.IsNullOrEmpty(problem))
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, problem);
        }
        else if (problem == null)
        {
            GUI.WrappedText(LOC.Get("MAT_FileList_Name_Result", MaterialBank.WithShortName(OperationFile.Key, name)));
        }

        GUI.Spacer();

        var canApply = problem == null;

        ImGui.BeginDisabled(!canApply);
        if (ImGui.Button($"{LOC.Get(OpenOperation == FileOperation.Rename ? "MAT_FileList_Action_Rename" : "MAT_FileList_Action_Duplicate")}##materialFileNameApply")
            || entered && canApply)
        {
            var operation = OpenOperation;
            var binder = OperationBinder;
            var file = OperationFile;

            DeferredAction = () => Apply(operation, binder, file, name);
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("MAT_FileList_Cancel")}##materialFileNameCancel"))
        {
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    private void DisplayDeletePopup()
    {
        ImGui.SetNextWindowSize(new Vector2(460, 0) * DPI.UIScale(), ImGuiCond.Always);

        var open = true;
        if (!ImGui.BeginPopupModal($"{LOC.Get("MAT_FileList_Delete_Title")}{DeletePopupId}", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        if (OperationFile == null || OperationBinder?.GetFile(OperationFile.Key) != OperationFile)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("MAT_FileList_Delete_Confirm", MaterialBank.ShortNameOf(OperationFile.Key), OperationBinder.Name));
        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAT_FileList_Delete_Warning"));
        DisplayUsedByWarning(OperationFile);

        GUI.Spacer();

        if (ImGui.Button($"{LOC.Get("MAT_FileList_Action_Delete")}##materialFileDeleteApply"))
        {
            var binder = OperationBinder;
            var file = OperationFile;

            DeferredAction = () => Apply(FileOperation.Delete, binder, file, "");
            ImGui.CloseCurrentPopup();
        }

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("MAT_FileList_Cancel")}##materialFileDeleteCancel"))
        {
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    /// <summary>
    /// How many models the last Used By scan found using this material, for rename and delete
    /// </summary>
    private void DisplayUsedByWarning(MaterialFileWrapper file)
    {
        var usage = Parent.Editor.Usage;

        if (!usage.HasScan)
        {
            GUI.WrappedText(LOC.Get("MAT_FileList_Used_By_Unknown"));
            return;
        }

        var count = usage.UsesOf(MaterialBank.ShortNameOf(file.Key)).Count;

        if (count > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAT_FileList_Used_By_Warning", count));
        else
            GUI.WrappedText(LOC.Get("MAT_FileList_Used_By_None"));
    }

    private void Apply(FileOperation operation, IMaterialBinder binder, MaterialFileWrapper file, string name)
    {
        var editor = Parent.Editor;

        switch (operation)
        {
            case FileOperation.Duplicate:
                var duplicate = new MatDuplicateFile(editor, Bank, binder, file, name);
                Parent.ActionManager.ExecuteAction(duplicate);
                Parent.Selection.SelectFile(duplicate.NewFile);
                break;

            case FileOperation.Rename:
                Parent.ActionManager.ExecuteAction(new MatRenameFile(editor, Bank, binder, file, name));
                Parent.Selection.ScrollToFile = true;
                break;

            case FileOperation.Delete:
                Parent.ActionManager.ExecuteAction(new MatDeleteFile(editor, Bank, binder, file));
                break;
        }
    }
    #endregion

    public string GetPrettyName(string path)
    {
        var newName = path;

        if(!CFG.Current.MaterialEditor_FileList_DisplayFullPath)
        {
            newName = Path.GetFileNameWithoutExtension(newName);
        }

        return newName;
    }
}
