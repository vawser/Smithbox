using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// The Data Transfer Import tab's table of stored exports (FmgImporter.ImportSources):
/// name, source container + language, size, date and folder, newest first. Exports from another
/// container or language than the selected one are shown in the warning colour.
/// Also owns the file operations on an export: open its folder, rename, move to the Archive subfolder.
/// </summary>
public class TextStoredExportList
{
    private TextEditorView View;
    private ProjectEntry Project;

    /// <summary>Key (full path) of the selected export, or null.</summary>
    public string SelectedKey;

    // Asked for by a button or the context menu; opened at this ID level, where the popup lives
    private bool _openRenamePopup;
    private bool _focusRenameInput;
    private string _renameKey;
    private string _renameInput = "";

    private const string ArchiveFolderName = "Archive";

    public TextStoredExportList(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool HasSelection => SelectedKey != null && View.FmgImporter.ImportSources.ContainsKey(SelectedKey);

    public StoredFmgContainer Selected =>
        SelectedKey != null && View.FmgImporter.ImportSources.TryGetValue(SelectedKey, out var source) ? source : null;

    #region Table
    public void Display(Action importSelected)
    {
        var sources = View.FmgImporter.ImportSources;
        var dates = View.FmgImporter.ImportSourceDates;

        if (sources.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("TEXT_Importer_No_Exported_Text"));
            DisplayRenamePopup();
            return;
        }

        var storedDir = SafeFullPath(TextUtils.GetStoredTextDirectory(Project));
        var target = View.Selection.SelectedContainerWrapper;

        var tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable |
            ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        if (ImGui.BeginTable("storedExportTable", 5, tableFlags, new Vector2(0, 180 * DPI.UIScale())))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("TEXT_DataTransfer_Column_Name"), ImGuiTableColumnFlags.WidthStretch, 3f);
            ImGui.TableSetupColumn(LOC.Get("TEXT_DataTransfer_Column_Source"), ImGuiTableColumnFlags.WidthStretch, 3f);
            ImGui.TableSetupColumn(LOC.Get("TEXT_DataTransfer_Column_Size"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
            ImGui.TableSetupColumn(LOC.Get("TEXT_DataTransfer_Column_Date"), ImGuiTableColumnFlags.WidthStretch, 2f);
            ImGui.TableSetupColumn(LOC.Get("TEXT_DataTransfer_Column_Folder"), ImGuiTableColumnFlags.WidthStretch, 1.5f);
            ImGui.TableHeadersRow();

            // Newest first: the export just made is at the top
            foreach (var (key, source) in sources.OrderByDescending(s => dates.GetValueOrDefault(s.Key)))
            {
                ImGui.TableNextRow();

                // Name (spans the row, so the whole row selects)
                ImGui.TableSetColumnIndex(0);
                if (ImGui.Selectable($"{source.Name}##storedExport_{key}", key == SelectedKey,
                    ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowOverlap))
                {
                    SelectedKey = key;
                }
                GUI.Tooltip(key);

                // Right-click selects the row too, so the menu acts on the row under the mouse
                if (ImGui.BeginPopupContextItem($"storedExportContext_{key}"))
                {
                    SelectedKey = key;
                    DisplayContextMenu(importSelected);
                    ImGui.EndPopup();
                }

                // Source
                ImGui.TableSetColumnIndex(1);
                var mismatch = SourceMismatch(source, target);
                var sourceText = string.IsNullOrEmpty(source.SourceContainer)
                    ? LOC.Get("TEXT_DataTransfer_Source_Unknown")
                    : $"{source.SourceContainer} ({source.SourceCategory})";

                if (mismatch)
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, sourceText);
                else
                    ImGui.TextUnformatted(sourceText);

                if (mismatch)
                {
                    GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Source_Mismatch_TT",
                        source.SourceContainer, source.SourceCategory,
                        target.FileEntry.Filename, target.ContainerDisplayCategory.ToString()));
                }
                else
                {
                    FmgImporter.ShowSourceTooltip(source);
                }

                // Size: text files / entries
                ImGui.TableSetColumnIndex(2);
                var fmgCount = source.FmgWrappers.Count;
                var entryCount = source.FmgWrappers.Sum(w => w?.Fmg?.Entries?.Count ?? 0);
                ImGui.TextUnformatted(LOC.Get("TEXT_DataTransfer_Size_Value", fmgCount, entryCount));

                // Date
                ImGui.TableSetColumnIndex(3);
                ImGui.TextUnformatted(dates.TryGetValue(key, out var date) ? date.ToString("yyyy-MM-dd HH:mm") : "");

                // Folder
                ImGui.TableSetColumnIndex(4);
                var inProject = string.Equals(SafeFullPath(Path.GetDirectoryName(key)), storedDir, StringComparison.OrdinalIgnoreCase);
                ImGui.TextUnformatted(LOC.Get(inProject ? "TEXT_DataTransfer_Folder_Project" : "TEXT_DataTransfer_Folder_Custom"));
                GUI.Tooltip(Path.GetDirectoryName(key) ?? "");
            }

            ImGui.EndTable();
        }

        DisplayRenamePopup();
    }

    private void DisplayContextMenu(Action importSelected)
    {
        if (ImGui.Selectable(LOC.Get("TEXT_DataTransfer_Action_Import_Selected")))
            importSelected();

        ImGui.Separator();

        if (ImGui.Selectable(LOC.Get("TEXT_DataTransfer_Action_Show_In_Folder")))
            ShowSelectedInFolder();

        if (ImGui.Selectable(LOC.Get("TEXT_DataTransfer_Action_Rename_Export")))
            BeginRenameSelected();

        if (ImGui.Selectable(LOC.Get("TEXT_DataTransfer_Action_Archive_Export")))
            ArchiveSelected();
        GUI.Tooltip(LOC.Get("TEXT_DataTransfer_Action_Archive_Export_TT"));
    }

    /// <summary>
    /// True when the export names another container or language than the selected one.
    /// The import still runs (moving text between e.g. DLC containers is legitimate); this is a heads-up.
    /// </summary>
    private static bool SourceMismatch(StoredFmgContainer source, TextContainerWrapper target)
    {
        if (target == null || string.IsNullOrEmpty(source.SourceContainer))
            return false;

        return source.SourceContainer != target.FileEntry.Filename ||
            (!string.IsNullOrEmpty(source.SourceCategory) && source.SourceCategory != target.ContainerDisplayCategory.ToString());
    }

    private static string SafeFullPath(string path)
    {
        try
        {
            return string.IsNullOrEmpty(path) ? "" : Path.GetFullPath(path);
        }
        catch (Exception)
        {
            return path ?? "";
        }
    }
    #endregion

    #region Open
    public void ShowSelectedInFolder()
    {
        if (!HasSelection)
            return;

        try
        {
            // Opens the folder with the file selected
            Process.Start("explorer.exe", $"/select,\"{SelectedKey}\"");
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_DataTransfer_Log_Open_Directory_Failed", Path.GetDirectoryName(SelectedKey)), ex);
        }
    }
    #endregion

    #region Rename
    public void BeginRenameSelected()
    {
        if (!HasSelection)
            return;

        _renameKey = SelectedKey;
        _renameInput = Path.GetFileNameWithoutExtension(SelectedKey);
        _openRenamePopup = true;
        _focusRenameInput = true;
    }

    /// <summary>Why the new name can't be used, or null. "" = unchanged (no message, nothing to do).</summary>
    private string RenameRefusal(string name)
    {
        if (name == "")
            return LOC.Get("TEXT_DataTransfer_Rename_Empty");

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return LOC.Get("TEXT_DataTransfer_Rename_Invalid");

        if (name == Path.GetFileNameWithoutExtension(_renameKey))
            return "";

        var newPath = Path.Combine(Path.GetDirectoryName(_renameKey) ?? "", $"{name}.json");

        // A case-only rename targets the same file
        if (File.Exists(newPath) && !string.Equals(newPath, _renameKey, StringComparison.OrdinalIgnoreCase))
            return LOC.Get("TEXT_DataTransfer_Rename_Exists", name);

        return null;
    }

    private void DisplayRenamePopup()
    {
        var popupId = $"{LOC.Get("TEXT_DataTransfer_Rename_Popup")}##textExportRenamePopup";

        if (_openRenamePopup)
        {
            ImGui.OpenPopup(popupId);
            _openRenamePopup = false;
        }

        if (!ImGui.BeginPopup(popupId, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        // The export was archived, renamed or the folder changed while the popup was open
        if (_renameKey == null || !View.FmgImporter.ImportSources.ContainsKey(_renameKey))
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        if (_focusRenameInput)
        {
            ImGui.SetKeyboardFocusHere();
            _focusRenameInput = false;
        }

        ImGui.SetNextItemWidth(300f * DPI.UIScale());
        ImGui.InputText("##textExportRenameInput", ref _renameInput, 255);

        // Leaving the field after an edit renames, unless the click that left it was on Cancel
        var committed = GUI.InputCommitted();

        var name = _renameInput.Trim();
        var refusal = RenameRefusal(name);

        if (!string.IsNullOrEmpty(refusal))
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, refusal);

        ImGui.BeginDisabled(refusal != null);
        var go = ImGui.Button($"{LOC.Get("TEXT_DataTransfer_Rename_Apply")}##textExportRenameGo");
        ImGui.EndDisabled();

        ImGui.SameLine();
        var cancel = ImGui.Button($"{LOC.Get("TEXT_DataTransfer_Rename_Cancel")}##textExportRenameCancel");
        var cancelClicked = ImGui.IsItemActive();

        if (cancel)
        {
            ImGui.CloseCurrentPopup();
        }
        else if (refusal == null && (go || committed && !cancelClicked))
        {
            Rename(_renameKey, name);
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    private void Rename(string oldPath, string name)
    {
        if (!View.FmgImporter.ImportSources.TryGetValue(oldPath, out var source))
            return;

        var newPath = Path.Combine(Path.GetDirectoryName(oldPath) ?? "", $"{name}.json");

        try
        {
            File.Move(oldPath, newPath);

            // The import lists show the stored name, so it follows the file name (as on export)
            source.Name = name;
            StoredFmgJson.Write(newPath, source);

            Smithbox.Log(this, LOC.Get("TEXT_DataTransfer_Log_Renamed", Path.GetFileName(oldPath), Path.GetFileName(newPath)));

            SelectedKey = newPath;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_DataTransfer_Log_Rename_Failed", oldPath), ex);
        }

        View.FmgImporter.InvalidateWrapperCache();
    }
    #endregion

    #region Archive
    /// <summary>
    /// Move the selected export into an Archive subfolder of its folder. It leaves the import lists
    /// (only the folder itself is scanned) but nothing is deleted: move it back to list it again.
    /// </summary>
    public void ArchiveSelected()
    {
        if (!HasSelection)
            return;

        var path = SelectedKey;
        var archiveDir = Path.Combine(Path.GetDirectoryName(path) ?? "", ArchiveFolderName);
        var target = Path.Combine(archiveDir, Path.GetFileName(path));

        try
        {
            Directory.CreateDirectory(archiveDir);

            // Never overwrite an earlier archived copy
            if (File.Exists(target))
            {
                target = Path.Combine(archiveDir,
                    $"{Path.GetFileNameWithoutExtension(path)}-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            }

            File.Move(path, target);

            Smithbox.Log(this, LOC.Get("TEXT_DataTransfer_Log_Archived", Path.GetFileName(path), archiveDir));

            SelectedKey = null;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("TEXT_DataTransfer_Log_Archive_Failed", path), ex);
        }

        View.FmgImporter.InvalidateWrapperCache();
    }
    #endregion
}
