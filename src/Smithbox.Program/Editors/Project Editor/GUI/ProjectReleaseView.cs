using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// The Release tab of the Project Editor: packs the project folder into a ZIP for players, holding only the files
/// the game reads. Works from the project folder on disk, so the project needn't be loaded.
/// </summary>
public class ProjectReleaseView
{
    public ProjectScreen Editor;

    private ProjectEntry _project;
    private ProjectReleaseSettings _settings = new();

    // The edit buffers of the text fields; applied to the settings when a field is left
    private string _outputFolder = "";
    private string _archiveName = "";
    private string _rootFolder = "";
    private string _patterns = "";

    // Scan
    private volatile ReleaseScanResult _scan;
    private volatile bool _isScanning;
    private CancellationTokenSource _scanToken;

    // Build
    private volatile bool _isBuilding;
    private int _buildWritten;
    private int _buildTotal;
    private CancellationTokenSource _buildToken;

    // The outcome of the last build, shown under the button
    private string _lastResult = "";
    private bool _lastResultFailed;
    private string _lastZipPath = "";

    public ProjectReleaseView(ProjectScreen editor)
    {
        Editor = editor;
    }

    public void Display(ProjectEntry project)
    {
        if (project != _project)
            SetProject(project);

        if (string.IsNullOrWhiteSpace(project.Descriptor.ProjectPath) || !Directory.Exists(project.Descriptor.ProjectPath))
        {
            GUI.EmptyState(LOC.Get("PROJECT_Release_No_Folder_Title"), LOC.Get("PROJECT_Release_No_Folder_Body"));
            return;
        }

        GUI.WrappedText(LOC.Get("PROJECT_Release_Intro"));
        GUI.Spacer();

        DisplayUnsavedWarning(project);

        DisplayArchiveSettings(project);
        GUI.Spacer();

        DisplayExclusions();
        GUI.Spacer();

        DisplayContents();
        GUI.Spacer();

        DisplayBuild(project);
    }

    private void SetProject(ProjectEntry project)
    {
        _scanToken?.Cancel();

        _project = project;
        _settings = ProjectReleaseBuilder.LoadSettings(project);

        _outputFolder = _settings.OutputFolder;
        _archiveName = _settings.ArchiveName;
        _rootFolder = _settings.RootFolder;
        _patterns = string.Join('\n', _settings.ExcludePatterns);

        _lastResult = "";
        _lastZipPath = "";
        _scan = null;

        StartScan();
    }

    private void SaveSettings()
    {
        _settings.OutputFolder = _outputFolder.Trim();
        _settings.ArchiveName = _archiveName.Trim();
        _settings.RootFolder = _rootFolder.Trim();
        _settings.ExcludePatterns = _patterns
            .Split('\n')
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        ProjectReleaseBuilder.SaveSettings(_project, _settings);
    }

    #region Sections

    /// <summary>
    /// Edits not yet saved are not in the files on disk, so not in the release
    /// </summary>
    private void DisplayUnsavedWarning(ProjectEntry project)
    {
        if (!project.IsActive)
            return;

        var unsaved = project.GetUnsavedChanges();

        if (unsaved.Count == 0)
            return;

        var names = string.Join(", ", unsaved.Select(e => e.Name));

        GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("PROJECT_Release_Unsaved_Warning", names));
        GUI.Spacer();
    }

    private void DisplayArchiveSettings(ProjectEntry project)
    {
        GUI.SimpleHeader(LOC.Get("PROJECT_Release_Header_Archive"), LOC.Get("PROJECT_Release_Header_Archive_TT"));

        // Output folder
        ImGui.TextUnformatted(LOC.Get("PROJECT_Release_Output_Folder"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##releaseOutputFolder",
            ProjectReleaseBuilder.GetOutputFolder(project, new ProjectReleaseSettings()), ref _outputFolder, 1024);
        if (GUI.InputCommitted())
            SaveSettings();
        GUI.Tooltip(LOC.Get("PROJECT_Release_Output_Folder_TT"));

        var outputFolder = ProjectReleaseBuilder.GetOutputFolder(project, _settings);

        GUI.ConditionalMultiButtonInput("releaseOutputActions",
            "selectOutput",
            LOC.Get("PROJECT_Release_Select_Output_Folder"),
            LOC.Get("PROJECT_Release_Select_Output_Folder_TT"),
            SelectOutputFolder, !_isBuilding,

            "openOutput",
            LOC.Get("PROJECT_Release_Open_Output_Folder"),
            LOC.Get("PROJECT_Release_Open_Output_Folder_TT"),
            () => ProjectConfigureMenu.OpenDirectory(outputFolder), Directory.Exists(outputFolder));

        // Archive name
        ImGui.TextUnformatted(LOC.Get("PROJECT_Release_Archive_Name"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##releaseArchiveName", project.Descriptor.ProjectName, ref _archiveName, 255);
        if (GUI.InputCommitted())
            SaveSettings();
        GUI.Tooltip(LOC.Get("PROJECT_Release_Archive_Name_TT"));

        // Folder in the ZIP
        ImGui.TextUnformatted(LOC.Get("PROJECT_Release_Root_Folder"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##releaseRootFolder", LOC.Get("PROJECT_Release_Root_Folder_Hint"), ref _rootFolder, 255);
        if (GUI.InputCommitted())
            SaveSettings();
        GUI.Tooltip(LOC.Get("PROJECT_Release_Root_Folder_TT"));
    }

    private void DisplayExclusions()
    {
        GUI.SimpleHeader(LOC.Get("PROJECT_Release_Header_Exclusions"), LOC.Get("PROJECT_Release_Header_Exclusions_TT"));

        GUI.WrappedText(LOC.Get("PROJECT_Release_Builtin_Rules"));
        GUI.Spacer();

        ImGui.TextUnformatted(LOC.Get("PROJECT_Release_Patterns"));
        GUI.HelpButton("releasePatternsHelp", LOC.Get("PROJECT_Release_Patterns"),
            LOC.Get("PROJECT_Release_Patterns_Help_1"),
            LOC.Get("PROJECT_Release_Patterns_Help_2"),
            LOC.Get("PROJECT_Release_Patterns_Help_3"));

        ImGui.InputTextMultiline("##releasePatterns", ref _patterns, 8192,
            new Vector2(-1, 90f * DPI.UIScale()), ImGuiInputTextFlags.None);

        // Applied when the field is left, then the contents are found again with the new patterns
        if (GUI.InputCommitted())
        {
            SaveSettings();
            StartScan();
        }
        GUI.Tooltip(LOC.Get("PROJECT_Release_Patterns_TT"));
    }

    private void DisplayContents()
    {
        GUI.SimpleHeader(LOC.Get("PROJECT_Release_Header_Contents"), LOC.Get("PROJECT_Release_Header_Contents_TT"));

        var scan = _scan;

        if (_isScanning)
        {
            GUI.WrappedText(LOC.Get("PROJECT_Release_Scanning"));
            return;
        }

        if (scan == null)
        {
            GUI.WrappedText(LOC.Get("PROJECT_Release_Not_Scanned"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("PROJECT_Release_Summary",
                scan.Included.Count, FormatSize(scan.IncludedSize),
                scan.ExcludedFileCount, FormatSize(scan.ExcludedSize)));
        }

        GUI.MultiButtonInput("releaseScanActions",
            "refresh",
            LOC.Get("PROJECT_Release_Refresh"),
            LOC.Get("PROJECT_Release_Refresh_TT"),
            StartScan);

        if (scan == null)
            return;

        // Left out: open at first, as it is the list to check
        if (ImGui.TreeNodeEx($"{LOC.Get("PROJECT_Release_Excluded_List", scan.Excluded.Count)}###releaseExcluded",
            ImGuiTreeNodeFlags.DefaultOpen))
        {
            DisplayEntryTable("releaseExcludedTable", scan.Excluded, true);
            ImGui.TreePop();
        }

        // Included
        if (ImGui.TreeNodeEx($"{LOC.Get("PROJECT_Release_Included_List", scan.Included.Count)}###releaseIncluded",
            ImGuiTreeNodeFlags.None))
        {
            DisplayEntryTable("releaseIncludedTable", scan.Included, false);
            ImGui.TreePop();
        }
    }

    private void DisplayEntryTable(string id, List<ReleaseEntry> entries, bool showReason)
    {
        if (entries.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PROJECT_Release_List_Empty"));
            return;
        }

        var columns = showReason ? 3 : 2;
        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;

        // Tall enough for a good look, without pushing the Create button far down
        var rows = Math.Min(entries.Count, 12) + 1;
        var height = rows * ImGui.GetTextLineHeightWithSpacing() + ImGui.GetStyle().CellPadding.Y * 2;

        if (!ImGui.BeginTable(id, columns, flags, new Vector2(-1, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("PROJECT_Release_Column_Path"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("PROJECT_Release_Column_Size"), ImGuiTableColumnFlags.WidthFixed, 110f * DPI.UIScale());

        if (showReason)
            ImGui.TableSetupColumn(LOC.Get("PROJECT_Release_Column_Reason"), ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableHeadersRow();

        var clipper = new ImGuiListClipper();
        clipper.Begin(entries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var entry = entries[i];

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(entry.RelativePath);
                GUI.Tooltip(entry.FullPath);

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(entry.IsFolder
                    ? LOC.Get("PROJECT_Release_Folder_Size", FormatSize(entry.Size), entry.FileCount)
                    : FormatSize(entry.Size));

                if (showReason)
                {
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted(GetReasonText(entry));
                }
            }
        }

        clipper.End();

        ImGui.EndTable();
    }

    private void DisplayBuild(ProjectEntry project)
    {
        GUI.SimpleHeader(LOC.Get("PROJECT_Release_Header_Create"), "");

        var zipPath = GetZipPath(project);

        GUI.WrappedText(LOC.Get("PROJECT_Release_Target", zipPath));

        if (_isBuilding)
        {
            var total = Math.Max(1, _buildTotal);
            var written = Volatile.Read(ref _buildWritten);

            ImGui.ProgressBar(written / (float)total, new Vector2(-1, 24f * DPI.UIScale()),
                LOC.Get("PROJECT_Release_Progress", written, _buildTotal));

            if (GUI.ExplainedButton($"{LOC.Get("PROJECT_Release_Cancel")}##releaseCancel",
                LOC.Get("PROJECT_Release_Cancel_TT"), true))
            {
                _buildToken?.Cancel();
            }

            return;
        }

        var scan = _scan;
        var canBuild = !_isScanning && scan != null && scan.Included.Count > 0;

        var tooltip = canBuild
            ? LOC.Get("PROJECT_Release_Create_TT")
            : LOC.Get("PROJECT_Release_Create_Disabled_TT");

        if (GUI.ExplainedButton($"{LOC.Get("PROJECT_Release_Create")}##releaseCreate", tooltip, canBuild))
            StartBuild(project, zipPath);

        if (!string.IsNullOrEmpty(_lastResult))
        {
            var color = _lastResultFailed ? UI.Current.ImGui_Invalid_Text_Color : UI.Current.ImGui_Benefit_Text_Color;

            GUI.WrappedTextColored(color, _lastResult);

            if (!_lastResultFailed && File.Exists(_lastZipPath))
            {
                if (GUI.ExplainedButton($"{LOC.Get("PROJECT_Release_Show_Zip")}##releaseShowZip",
                    LOC.Get("PROJECT_Release_Show_Zip_TT"), true))
                {
                    ProjectConfigureMenu.OpenDirectory(Path.GetDirectoryName(_lastZipPath));
                }
            }
        }
    }

    #endregion

    #region Actions

    private void SelectOutputFolder()
    {
        var start = ProjectReleaseBuilder.GetOutputFolder(_project, _settings);

        if (PlatformUtils.Instance.OpenFolderDialog(LOC.Get("PROJECT_Release_Dialog_Select_Output_Folder"),
            out var path, start))
        {
            _outputFolder = path;
            SaveSettings();
        }
    }

    private string GetZipPath(ProjectEntry project)
    {
        var folder = ProjectReleaseBuilder.GetOutputFolder(project, _settings);
        var name = ProjectReleaseBuilder.GetArchiveFileName(project, _settings);

        try
        {
            return Path.GetFullPath(Path.Join(folder, name));
        }
        catch (Exception)
        {
            return Path.Join(folder, name);
        }
    }

    private void StartScan()
    {
        if (_project == null)
            return;

        _scanToken?.Cancel();

        var token = new CancellationTokenSource();
        _scanToken = token;

        var root = _project.Descriptor.ProjectPath;
        var patterns = _settings.ExcludePatterns.ToList();

        _isScanning = true;

        Task.Run(() =>
        {
            try
            {
                var result = ProjectReleaseBuilder.Scan(root, patterns, token.Token);

                // A newer scan (another project, new patterns) replaced this one
                if (_scanToken == token)
                    _scan = result;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PROJECT_Release_Scan_Failed", root, e.Message), e);
            }
            finally
            {
                if (_scanToken == token)
                    _isScanning = false;
            }
        });
    }

    private void StartBuild(ProjectEntry project, string zipPath)
    {
        var scan = _scan;

        if (scan == null || _isBuilding)
            return;

        // The ZIP is never written inside the project folder, where the next release would have to skip it
        if (ProjectUtils.IsSameOrInsideDirectory(Path.GetDirectoryName(zipPath), project.Descriptor.ProjectPath))
        {
            var inside = PlatformUtils.Instance.MessageBox(
                LOC.Get("PROJECT_Release_Dialog_Inside_Project", zipPath),
                LOC.Get("SYS_Warning_Header"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (inside is not DialogResult.OK)
                return;
        }

        if (File.Exists(zipPath))
        {
            var overwrite = PlatformUtils.Instance.MessageBox(
                LOC.Get("PROJECT_Release_Dialog_Overwrite", zipPath),
                LOC.Get("SYS_Warning_Header"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            if (overwrite is not DialogResult.OK)
                return;
        }

        var rootFolder = ProjectReleaseBuilder.GetRootFolder(_settings);
        var token = new CancellationTokenSource();

        _buildToken = token;
        _buildTotal = scan.Included.Count;
        _buildWritten = 0;
        _lastResult = "";
        _isBuilding = true;

        Task.Run(() =>
        {
            try
            {
                ProjectReleaseBuilder.WriteZip(scan, zipPath, rootFolder,
                    written => Volatile.Write(ref _buildWritten, written), token.Token);

                var size = new FileInfo(zipPath).Length;

                _lastZipPath = zipPath;
                _lastResultFailed = false;
                _lastResult = LOC.Get("PROJECT_Release_Created", scan.Included.Count, FormatSize(size), zipPath);

                Smithbox.Log(this, _lastResult);
            }
            catch (OperationCanceledException)
            {
                _lastResultFailed = true;
                _lastResult = LOC.Get("PROJECT_Release_Cancelled");
            }
            catch (Exception e)
            {
                _lastResultFailed = true;
                _lastResult = LOC.Get("PROJECT_Release_Failed", zipPath, e.Message);

                Smithbox.LogError(this, _lastResult, e);
            }
            finally
            {
                _isBuilding = false;
            }
        });
    }

    #endregion

    #region Text

    private static string GetReasonText(ReleaseEntry entry)
    {
        return entry.Reason switch
        {
            ReleaseExclusionReason.HiddenData => LOC.Get("PROJECT_Release_Reason_Hidden"),
            ReleaseExclusionReason.Backup => LOC.Get("PROJECT_Release_Reason_Backup"),
            ReleaseExclusionReason.UnpackedFolder => LOC.Get("PROJECT_Release_Reason_Unpacked"),
            ReleaseExclusionReason.SerializedCopy => LOC.Get("PROJECT_Release_Reason_Serialized"),
            ReleaseExclusionReason.Archive => LOC.Get("PROJECT_Release_Reason_Archive"),
            ReleaseExclusionReason.SystemFile => LOC.Get("PROJECT_Release_Reason_System"),
            ReleaseExclusionReason.ProjectFile => LOC.Get("PROJECT_Release_Reason_Project_File"),
            ReleaseExclusionReason.UserPattern => LOC.Get("PROJECT_Release_Reason_Pattern", entry.Pattern),
            _ => ""
        };
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];

        double size = bytes;
        var unit = 0;

        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} {units[0]}" : $"{size:0.##} {units[unit]}";
    }

    #endregion
}
