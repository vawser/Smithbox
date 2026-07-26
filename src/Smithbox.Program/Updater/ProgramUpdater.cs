using Hexa.NET.ImGui;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace StudioCore.Application;

public static class ProgramUpdater
{
    private const string Owner = "vawser";
    private const string Repo = "Smithbox";

    private static readonly HttpClient _httpClient = CreateHttpClient();

    public static UpdaterProgress LoadProgress;
    public static Action<UpdaterProgress> ReportProgress = SetProgress;

    private static void SetProgress(UpdaterProgress progress)
    {
        lock (_progressLock)
        {
            LoadProgress = progress;
        }
    }

    public static readonly object _progressLock = new();

    public static bool DisplayModal = false;
    public static bool InitialLayout = false;

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        // GitHub API requires a User-Agent header
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Smithbox-Updater", "1.0"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }

    public static unsafe void Display(float dt, uint mainDockspaceID)
    {
        CheckForUpdate();

        if (Smithbox.Instance._context.Device == null)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        ImGui.SetNextWindowDockID(mainDockspaceID, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_EditorView);

        if (ImGui.Begin($"{LOC.Get("UPDATER_Window_Title")}###ProgramUpdater", GUI.GetInnerWindowFlags()))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            DisplayUpdater();

            ImGui.End();
        }
        else
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }

        DisplayUpdaterModal();
    }

    public static void DisplayUpdater()
    {
        var baseDir = AppContext.BaseDirectory;

        var vulkanExecutable = Path.Join(baseDir, "Smithbox.Vulkan.exe");
        var openGLExecutable = Path.Join(baseDir, "Smithbox.OpenGL.exe");
        var updaterExecutable = Path.Join(baseDir, "Smithbox.Updater.exe");

        ImGui.BeginChild("UpdaterSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("UPDATER_Update_Hint"));

        // --- Out-of-date warning ---
        if (_isOutOfDate)
        {
            GUI.Spacer();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.55f, 0.2f, 1.0f));
            GUI.WrappedText(LOC.Get("UPDATER_OutOfDate_Warning",
                _latestVersionTag, Smithbox.Instance._version));
            ImGui.PopStyleColor(1);
        }
        else if (_hasCheckedForUpdate && string.IsNullOrEmpty(_versionCheckError))
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("UPDATER_UpToDate_Message"));
        }
        else if (!string.IsNullOrEmpty(_versionCheckError))
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("UPDATER_VersionCheck_Failed", _versionCheckError));
        }

        if (File.Exists(updaterExecutable))
        {
            var updaterExeInfo = FileVersionInfo.GetVersionInfo(updaterExecutable);

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("UPDATER_Updater_Version_Header"),
                LOC.Get("UPDATER_Updater_Version_Header_TT"));

            GUI.WrappedText($"{LOC.Get("UPDATER_File_Version", updaterExeInfo.FileVersion)}");
            GUI.WrappedText($"{LOC.Get("UPDATER_Is_Debug_Build", updaterExeInfo.IsDebug)}");
        }


        if (File.Exists(vulkanExecutable))
        {
            var vulkanExeInfo = FileVersionInfo.GetVersionInfo(vulkanExecutable);

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("UPDATER_Vulkan_Version_Header"),
                LOC.Get("UPDATER_Vulkan_Version_Header_TT"));

            GUI.WrappedText($"{LOC.Get("UPDATER_File_Version", vulkanExeInfo.FileVersion)}");
            GUI.WrappedText($"{LOC.Get("UPDATER_Is_Debug_Build", vulkanExeInfo.IsDebug)}");
        }

        if (File.Exists(openGLExecutable))
        {
            var openGLExeInfo = FileVersionInfo.GetVersionInfo(openGLExecutable);

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("UPDATER_OpenGL_Version_Header"),
                LOC.Get("UPDATER_OpenGL_Version_Header_TT"));

            GUI.WrappedText($"{LOC.Get("UPDATER_File_Version", openGLExeInfo.FileVersion)}");
            GUI.WrappedText($"{LOC.Get("UPDATER_Is_Debug_Build", openGLExeInfo.IsDebug)}");
        }

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("UPDATER_Actions_Header"),
            LOC.Get("UPDATER_Actions_Header_TT"));

        GUI.MultiButtonInput("updaterActions",
            "downloadLatest",
            LOC.Get("UPDATER_Download_Latest_Action"),
            LOC.Get("UPDATER_Download_Latest_Action_TT"),
            DownloadLatestRelease);

        ImGui.EndChild();
    }

    public static void DownloadLatestRelease()
    {
        var success = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("DIALOG_Select_Directory"), out var path);

        if (success)
        {
            InitialLayout = false;
            DisplayModal = true;

            ReportProgress(new UpdaterProgress
            {
                PhaseLabel = LOC.Get("UPDATER_Phase_Starting"),
                StepLabel = "",
                Percent = 0f
            });

            _ = DownloadLatestReleaseAsync(path);
        }
    }
    private static async Task DownloadLatestReleaseAsync(string destinationFolder)
    {
        try
        {
            // --- 1. Get latest release metadata ---
            ReportProgress(new UpdaterProgress
            {
                PhaseLabel = LOC.Get("UPDATER_Phase_Checking"),
                StepLabel = LOC.Get("UPDATER_Step_FetchingMetadata"),
                Percent = 0.02f
            });

            var apiUrl = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
            var json = await _httpClient.GetStringAsync(apiUrl);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.GetProperty("tag_name").GetString();
            var assets = root.GetProperty("assets");

            if (assets.GetArrayLength() == 0)
                throw new Exception(LOC.Get("UPDATER_Log_No_Assets_Found"));

            JsonElement? zipAsset = null;
            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString();
                if (name != null && name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    zipAsset = asset;
                    break;
                }
            }

            if (zipAsset == null)
                throw new Exception(LOC.Get("UPDATER_Log_No_Zip_Asset_Found"));

            var downloadUrl = zipAsset.Value.GetProperty("browser_download_url").GetString();
            var assetName = zipAsset.Value.GetProperty("name").GetString();
            var tempZipPath = Path.Combine(Path.GetTempPath(), assetName);

            // --- 2. Download with byte-level progress ---
            // Download phase occupies 0.05 -> 0.80 of the overall bar
            const float downloadStart = 0.05f;
            const float downloadEnd = 0.80f;

            using (var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var totalRead = 0L;
                var buffer = new byte[81920];

                await using var httpStream = await response.Content.ReadAsStreamAsync();
                await using var fs = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None);

                int bytesRead;
                while ((bytesRead = await httpStream.ReadAsync(buffer)) > 0)
                {
                    await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalRead += bytesRead;

                    float percent = downloadStart;
                    string step;

                    if (totalBytes > 0)
                    {
                        var fraction = (float)totalRead / totalBytes;
                        percent = downloadStart + fraction * (downloadEnd - downloadStart);
                        step = LOC.Get("UPDATER_Step_Downloading_Known",
                            FormatBytes(totalRead), FormatBytes(totalBytes));
                    }
                    else
                    {
                        // No Content-Length header: show bytes downloaded without a percent target
                        step = LOC.Get("UPDATER_Step_Downloading_Unknown", FormatBytes(totalRead));
                    }

                    ReportProgress(new UpdaterProgress
                    {
                        PhaseLabel = LOC.Get("UPDATER_Phase_Downloading"),
                        StepLabel = step,
                        Percent = percent
                    });
                }
            }

            // --- 3. Extract with entry-level progress ---
            // Extraction phase occupies 0.80 -> 1.0 of the overall bar
            const float extractStart = 0.80f;
            const float extractEnd = 0.98f;

            if (!Directory.Exists(destinationFolder))
                Directory.CreateDirectory(destinationFolder);

            using (var archive = ZipFile.OpenRead(tempZipPath))
            {
                var totalEntries = archive.Entries.Count;
                var processed = 0;

                foreach (var entry in archive.Entries)
                {
                    var destPath = Path.GetFullPath(Path.Combine(destinationFolder, entry.FullName));

                    // Guard against zip-slip / path traversal
                    if (!destPath.StartsWith(Path.GetFullPath(destinationFolder) + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception(LOC.Get("UPDATER_Log_Entry_Outside_of_Dest_Folder", entry.FullName));
                    }

                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        // Directory entry
                        Directory.CreateDirectory(destPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                        entry.ExtractToFile(destPath, overwrite: true);
                    }

                    processed++;
                    var fraction = totalEntries > 0 ? (float)processed / totalEntries : 1f;

                    ReportProgress(new UpdaterProgress
                    {
                        PhaseLabel = LOC.Get("UPDATER_Phase_Extracting"),
                        StepLabel = LOC.Get("UPDATER_Step_Extracting_File", entry.Name),
                        Percent = extractStart + fraction * (extractEnd - extractStart)
                    });
                }
            }

            // --- 4. Clean up ---
            File.Delete(tempZipPath);

            ReportProgress(new UpdaterProgress
            {
                PhaseLabel = LOC.Get("UPDATER_Phase_Complete"),
                StepLabel = LOC.Get("UPDATER_Step_Complete", tagName),
                Percent = 1f
            });

            // Give the user a moment to see the "complete" state before closing
            await Task.Delay(1000);
            DisplayModal = false;
        }
        catch (Exception ex)
        {
            ReportProgress(new UpdaterProgress
            {
                PhaseLabel = LOC.Get("UPDATER_Phase_Failed"),
                StepLabel = ex.Message,
                Percent = 0f
            });

            // Leave modal open briefly so the error is visible, then close.
            // Swap this for a manual "close" button in DisplayUpdaterModal if you'd rather
            // the user dismiss errors themselves.
            await Task.Delay(3000);
            DisplayModal = false;
        }
    }
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public static void DisplayUpdaterModal()
    {
        if (!DisplayModal)
            return;

        var displayName = LOC.Get("UPDATER_Modal_Title");
        var popupName = $"{displayName}###UpdaterModal";

        ImGui.OpenPopup(popupName);

        if (!InitialLayout)
        {
            GUI.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(popupName,
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            UpdaterProgress progress;
            lock (_progressLock)
                progress = LoadProgress;

            if (!string.IsNullOrEmpty(progress.PhaseLabel))
            {
                ImGui.Text(progress.PhaseLabel);
                ImGui.Spacing();
            }

            ImGui.ProgressBar(
                Math.Clamp(progress.Percent, 0f, 1f),
                new Vector2(400, 0),
                $"{(int)(progress.Percent * 100)}%"
            );

            if (!string.IsNullOrEmpty(progress.StepLabel))
            {
                ImGui.Spacing();
                ImGui.TextDisabled(progress.StepLabel);
            }

            ImGui.EndPopup();
        }
    }

    private static bool _hasCheckedForUpdate = false;
    private static bool _isCheckingForUpdate = false;
    private static bool _isOutOfDate = false;
    private static string _latestVersionTag = "";
    private static string _versionCheckError = "";

    public static void CheckForUpdate()
    {
        if (_hasCheckedForUpdate || _isCheckingForUpdate)
            return;

        _isCheckingForUpdate = true;
        _ = CheckForUpdateAsync();
    }

    private static async Task CheckForUpdateAsync()
    {
        try
        {
            var apiUrl = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
            var json = await _httpClient.GetStringAsync(apiUrl);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var tagName = root.GetProperty("tag_name").GetString() ?? "";
            _latestVersionTag = tagName;

            var curVersion = Smithbox.Instance._version;

            if (TryParseVersion(tagName, out var latestVersion) &&
                TryParseVersion(curVersion, out var currentVersion))
            {
                _isOutOfDate = latestVersion > currentVersion;
            }
            else
            {
                // Couldn't parse one or both versions as a System.Version.
                // Fall back to a simple string inequality so we don't silently hide a mismatch.
                _isOutOfDate = !string.Equals(
                    NormalizeVersionString(tagName),
                    NormalizeVersionString(curVersion),
                    StringComparison.OrdinalIgnoreCase);

                _versionCheckError = LOC.Get("UPDATER_VersionCheck_ParseWarning");
            }
        }
        catch (Exception ex)
        {
            _versionCheckError = ex.Message;
            _isOutOfDate = false;
        }
        finally
        {
            _isCheckingForUpdate = false;
            _hasCheckedForUpdate = true;
        }
    }

    private static string NormalizeVersionString(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return "";

        return version.Trim().TrimStart('v', 'V');
    }

    private static bool TryParseVersion(string raw, out Version version)
    {
        version = null;
        var normalized = NormalizeVersionString(raw);

        if (string.IsNullOrEmpty(normalized))
            return false;

        // System.Version requires at least a Major.Minor format;
        // pad single-number versions like "3" to "3.0"
        var parts = normalized.Split('.');
        if (parts.Length == 1)
            normalized += ".0";

        return Version.TryParse(normalized, out version);
    }
}
