using Andre.IO.Asset;
using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Formats.JSON;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

public static class ProgramUpdater
{
    public static bool IsUpdateAvaliable = false;
    public static bool UpdateProcessActive = false;

    public static Octokit.Release LatestRelease = null;
    public static GithubAsset Asset = null;

    public static string OutputDir = "";

    public static bool IsConnectedToInternet = false;

    public static async Task<bool> HasInternetConnectionAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            using var response = await client.GetAsync("https://www.google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public static async void CheckForUpdate(string version)
    {
        if (!CFG.Current.System_Check_Program_Update)
            return;

        IsConnectedToInternet = await HasInternetConnectionAsync();

        if (IsConnectedToInternet)
        {
            Octokit.GitHubClient gitHubClient = new(new Octokit.ProductHeaderValue("Smithbox"));
            Octokit.Release release = gitHubClient.Repository.Release.GetLatest("vawser", "Smithbox").Result;

            LatestRelease = release;

            if (release.TagName != version)
            {
                var curVersion = version.Replace(".", "");
                var releaseVersion = release.TagName.Replace(".", "");

                var curVersionNum = -1;
                var releaseVersionNum = -1;

                var curSuccess = int.TryParse(curVersion, out curVersionNum);
                var releaseSuccess = int.TryParse(releaseVersion, out releaseVersionNum);

                if (curSuccess && releaseSuccess)
                {
                    if (curVersionNum != -1 && releaseVersionNum != -1)
                    {
                        if (curVersionNum < releaseVersionNum)
                        {
                            IsUpdateAvaliable = true;
                        }
                    }
                }
            }
        }
    }

    public static void DisplayUpdateHint(Smithbox baseEditor)
    {
        if (!CFG.Current.System_Check_Program_Update)
            return;

        if (LatestRelease == null)
            return;

        if (!IsConnectedToInternet)
            return;

        if (IsUpdateAvaliable && !UpdateProcessActive)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Warning_Text_Color);
            if (ImGui.Button("Update Smithbox to Latest Release"))
            {
                var dialog = PlatformUtils.Instance.MessageBox("This will delete your current Smithbox install and replace it with the latest release. You must let this process finish without interruption.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dialog == DialogResult.Yes)
                {
                    UpdateSmithbox(baseEditor);
                }
            }

            ImGui.PopStyleColor();
        }

        if (UpdateProcessActive)
        {
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, "Download in progres...");
        }
    }

    public async static void UpdateSmithbox(Smithbox baseEditor)
    {
        if (!CFG.Current.System_Check_Program_Update)
            return;

        if (LatestRelease == null)
            return;

        if (!IsConnectedToInternet)
            return;

        if (UpdateProcessActive)
        {
            TaskLogs.AddLog("[Smithbox] Download in progress.");

            return;
        }

        await Task.Yield();

        UpdateProcessActive = true;

        OutputDir = $"{AppContext.BaseDirectory}";
        if (!Directory.Exists(OutputDir))
        {
            Directory.CreateDirectory(OutputDir);
        }

        Task<bool> releaseAssetTask = GetReleaseAsset();
        Task.WaitAll(releaseAssetTask);

        Task<bool> zipTask = GetReleaseZIP();
        Task.WaitAll(zipTask);

        Task<bool> extractTask = ExtractLatestRelease();
        Task.WaitAll(extractTask);

        // Trigger Smithbox.Updater.exe and shutdown Smithbox.exe
        var extractDir = $"{OutputDir}/_update";
        string updaterPath = Path.Combine(extractDir, "Smithbox.Updater.exe");

        Process.Start(new ProcessStartInfo
        {
            FileName = updaterPath,
            Arguments = $"{OutputDir}",
            UseShellExecute = false
        });

        Environment.Exit(0);
    }

    public async static Task<bool> ExtractLatestRelease()
    {
        await Task.Yield();

        var extractDir = $"{OutputDir}/_update";
        if (!Directory.Exists(extractDir))
        {
            Directory.CreateDirectory(extractDir);
        }

        string sourcePath = Path.Combine(OutputDir, $"{Asset.name}");

        ZipFile.ExtractToDirectory(sourcePath, extractDir, true);

        return true;
    }

    public async static Task<bool> GetReleaseZIP()
    {
        if (Asset != null)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; gael@ringedcity.com)");

                using var assetResponse = await httpClient.GetAsync(Asset.browser_download_url);
                assetResponse.EnsureSuccessStatusCode();

                string filePath = Path.Combine(OutputDir, $"{Asset.name}");

                await using var fs = new FileStream(filePath, System.IO.FileMode.Create);
                await assetResponse.Content.CopyToAsync(fs);

                TaskLogs.AddLog("[Smithbox] Downloaded latest release ZIP.");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog("[Smithbox] Failed to download latest release ZIP.", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, ex);
            }
        }


        return true;
    }

    public async static Task<bool> GetReleaseAsset()
    {
        await Task.Yield();

        var mainAsset = LatestRelease.Assets[0];

        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; gael@ringedcity.com)");

            using var response = await httpClient.GetAsync(mainAsset.Url);
            response.EnsureSuccessStatusCode();

            try
            {
                var options = new JsonSerializerOptions();
                Asset = JsonSerializer.Deserialize(response.Content.ReadAsStream(), SmithboxSerializerContext.Default.GithubAsset);

                TaskLogs.AddLog("[Smithbox] Downloaded github asset information.");
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to seralize github asset information.", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog("[Smithbox] Failed to download latest github asset information.", Microsoft.Extensions.Logging.LogLevel.Error, Tasks.LogPriority.High, ex);
        }

        return true;
    }
}
