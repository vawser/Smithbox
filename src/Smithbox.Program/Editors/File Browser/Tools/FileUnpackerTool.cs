using Andre.IO.VFS;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

public class FileUnpackerTool
{
    // One unpacker per File Browser, shared by its views, so a running job is visible (and blocks a second one)
    // from every view
    public FileBrowserScreen Editor;
    public ProjectEntry Project;

    public string UnpackDirectory = "";
    public volatile bool IsUnpacking = false;
    public int TotalToUnpack = 0;
    public int CurrentUnpacked = 0;

    private List<(string Path, string Error)> FailedUnpackEntries = new();

    private const int MaxConcurrentUnpacks = 6;

    /// <summary>
    /// Written into the unpack directory. Lists every file the unpacker created there, so Delete only ever removes
    /// those files and never anything that was already present (the game's own loose files, mods, user files).
    /// </summary>
    public const string ManifestFileName = ".smithbox_unpack_manifest.txt";
    private const string ManifestHeader = "# Smithbox unpack manifest: files created by the File Browser unpacker";

    // Selective Unpack key for files directly in the data root
    private const string RootFolderKey = "/";

    private FileDictionary BaseFileDictionary = new FileDictionary();
    private Dictionary<string, bool> SelectiveFolderDict = new();
    public List<string> TopFolderList = new();

    public volatile bool IsDeleting = false;
    public int TotalToDelete = 0;
    public int CurrentDeleted = 0;

    // Separate tokens so cancelling or finishing one job never touches the other
    private CancellationTokenSource UnpackCancelToken;
    private CancellationTokenSource DeleteCancelToken;

    // Summary of the last finished job, shown under the actions
    private string LastJobSummary = "";

    private bool IsBusy => IsUnpacking || IsDeleting;

    public FileUnpackerTool(FileBrowserScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        UnpackDirectory = project.Descriptor.DataPath;
        if (CFG.Current.UnpackDirectory != "")
            UnpackDirectory = CFG.Current.UnpackDirectory;

        UpdateBaseFileDictionary();
    }

    public void Display()
    {
        ImGui.BeginChild("GameUnpackerToolSection", ImGuiChildFlags.Borders);

        var windowWidth = ImGui.GetWindowWidth() * 0.95f;

        GUI.WrappedText(LOC.Get("FILE_FileUnpacker_Hint"));

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("FILE_FileUnpacker_Unpack_Dir_Header"),
            LOC.Get("FILE_FileUnpacker_Unpack_Dir_Header_TT"));

        GUI.HintTextInput("##unpackDirectory", ref UnpackDirectory, LOC.Get("FILE_FileUnpacker_Unpack_Hint"));
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            CFG.Current.UnpackDirectory = UnpackDirectory;
        }

        ImGui.SliderInt($"{LOC.Get("FILE_FileUnpacker_Threads")}##unpackThreads", ref CFG.Current.FileBrowser_Unpack_Threads, 1, 32);
        GUI.Tooltip(LOC.Get("FILE_FileUnpacker_Threads_TT"));

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("FILE_FileUnpacker_Actions_Header"),
            LOC.Get("FILE_FileUnpacker_Actions_Header_TT"));

        GUI.MultiButtonInput("unpackActions",
            "setUnpackDirectory", 
            LOC.Get("FILE_FileUnpacker_Set_Unpack_Dir"),
            LOC.Get("FILE_FileUnpacker_Set_Unpack_Dir_TT"),
            ConfigureUnpackDirectory,

            "unpackGame",
            LOC.Get("FILE_FileUnpacker_Unpack"),
            LOC.Get("FILE_FileUnpacker_Unpack_TT"), 
            UnpackGameAction,

            "deleteUnpackedFiles",
            LOC.Get("FILE_FileUnpacker_Delete"),
            LOC.Get("FILE_FileUnpacker_Delete_TT"), 
            DeleteUnpackedFilesAction);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("FILE_FileUnpacker_Selective_Unpack_Header"),
            LOC.Get("FILE_FileUnpacker_Selective_Unpack_Header_TT"));

        GUI.MultiButtonInput("selectiveUnpackActions",
            "selectAllOptions",
            LOC.Get("FILE_FileUnpacker_Selective_Select_All"),
            LOC.Get("FILE_FileUnpacker_Selective_Select_All_TT"),
            () => SetAllSelectiveUnpackOptions(true),

            "selectNoneOptions",
            LOC.Get("FILE_FileUnpacker_Selective_Select_None"),
            LOC.Get("FILE_FileUnpacker_Selective_Select_None_TT"),
            () => SetAllSelectiveUnpackOptions(false));

        // Toggles
        ImGui.BeginChild("ToggleSection", new Vector2(0, 400), ImGuiChildFlags.Borders);
        foreach (var entry in SelectiveFolderDict)
        {
            var curToggle = entry.Value;
            var label = entry.Key == RootFolderKey ? LOC.Get("FILE_FileUnpacker_Root_Files") : entry.Key;
            ImGui.Checkbox($"{label}##toggleFolder_{entry.Key}", ref curToggle);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                SelectiveFolderDict[entry.Key] = curToggle;
            }
        }
        ImGui.EndChild();

        // Progress
        if (IsUnpacking)
        {
            float progress = TotalToUnpack > 0 ? (float)CurrentUnpacked / TotalToUnpack : 0f;
            string label = LOC.Get("FILE_FileUnpacker_Unpacking_Status", CurrentUnpacked, TotalToUnpack);
            ImGui.ProgressBar(progress, DPI.WholeWidthButton(windowWidth, 24), label);

            GUI.MultiButtonInput("cancelUnpackActions",
                "cancelUnpack",
                LOC.Get("FILE_FileUnpacker_Cancel_Unpack"),
                LOC.Get("FILE_FileUnpacker_Cancel_Unpack_TT"),
                CancelUnpack);
        }

        if (IsDeleting)
        {
            float progress = TotalToDelete > 0 ? (float)CurrentDeleted / TotalToDelete : 0f;
            string label = LOC.Get("FILE_FileUnpacker_Deleting_Status", CurrentDeleted, TotalToDelete);
            ImGui.ProgressBar(progress, DPI.WholeWidthButton(windowWidth, 24), label);

            GUI.MultiButtonInput("cancelDeleteActions",
                "cancelDelete",
                LOC.Get("FILE_FileUnpacker_Cancel_Delete"),
                LOC.Get("FILE_FileUnpacker_Cancel_Delete_TT"),
                CancelDelete);
        }

        if (!IsBusy && LastJobSummary != "")
        {
            GUI.WrappedText(LastJobSummary);
        }

        if (!IsUnpacking && FailedUnpackEntries.Count > 0)
        {
            ImGui.BeginChild("FailedFilesChild", new Vector2(0, 0), ImGuiChildFlags.Borders);

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("FILE_FileUnpacker_Failed_Unpack_Header"),
                LOC.Get("FILE_FileUnpacker_Failed_Unpack_Header_TT"));

            List<(string Path, string Error)> failed;
            lock (FailedUnpackEntries)
                failed = FailedUnpackEntries.ToList();

            foreach (var (path, error) in failed)
            {
                ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), path);
                ImGui.PushTextWrapPos();
                ImGui.TextWrapped($"  - {error}");
                ImGui.PopTextWrapPos();
            }

            ImGui.EndChild();
        }

        ImGui.EndChild();
    }

    public void CancelUnpack()
    {
        // The job may finish and dispose its token between the read and the call
        try { UnpackCancelToken?.Cancel(); } catch (ObjectDisposedException) { }
    }

    public void CancelDelete()
    {
        try { DeleteCancelToken?.Cancel(); } catch (ObjectDisposedException) { }
    }

    /// <summary>
    /// The directory a job writes to or deletes from: the unpack directory, or the game directory when none is set
    /// </summary>
    private string GetUnpackRoot()
    {
        var root = UnpackDirectory != "" ? UnpackDirectory : Project.Descriptor.DataPath;
        return Path.GetFullPath(root);
    }

    /// <summary>
    /// Absolute path for a dictionary path ("/chr/c0000.chrbnd.dcx") under root, or null if it would land outside root
    /// </summary>
    private static string ResolveUnderRoot(string root, string relativePath)
    {
        var rel = relativePath.Replace('\\', '/').TrimStart('/');
        if (rel == "")
            return null;

        var full = Path.GetFullPath(Path.Combine(root, rel));
        var rootWithSep = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;

        if (!full.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase))
            return null;

        return full;
    }

    private static HashSet<string> ReadManifest(string manifestPath)
    {
        var entries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (!File.Exists(manifestPath))
            return entries;

        foreach (var line in File.ReadAllLines(manifestPath))
        {
            var trimmed = line.Trim();
            if (trimmed == "" || trimmed.StartsWith('#'))
                continue;

            entries.Add(trimmed);
        }

        return entries;
    }

    private static void WriteManifest(string manifestPath, IEnumerable<string> entries)
    {
        var list = entries.OrderBy(e => e, StringComparer.OrdinalIgnoreCase).ToList();

        if (list.Count == 0)
        {
            if (File.Exists(manifestPath))
                File.Delete(manifestPath);
            return;
        }

        File.WriteAllLines(manifestPath, new[] { ManifestHeader }.Concat(list));
    }

    public void ConfigureUnpackDirectory()
    {
        var unpackDirectory = "";
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("FILE_FileUnpacker_Select_Unpack_Dir"), out unpackDirectory, UnpackDirectory);

        if (result)
        {
            UnpackDirectory = unpackDirectory;
            CFG.Current.UnpackDirectory = unpackDirectory;
        }
    }

    /// <summary>
    /// Ticks or unticks every Selective Unpack folder. (The old Toggle All inverted each one, so a mixed selection
    /// stayed mixed.)
    /// </summary>
    public void SetAllSelectiveUnpackOptions(bool selected)
    {
        foreach (var key in SelectiveFolderDict.Keys.ToList())
        {
            SelectiveFolderDict[key] = selected;
        }
    }

    public void UnpackGameAction()
    {
        if(UnpackDirectory == "")
        {
            Smithbox.Log<FileUnpackerTool>(
                LOC.Get("FILE_FileUnpacker_Unpack_Dir_Not_Set"));
            return;
        }

        if (IsUnpacking)
        {
            Smithbox.Log<FileUnpackerTool>(
                LOC.Get("FILE_FileUnpacker_Game_Data_Already_Unpacked"));
            return;
        }

        if (IsDeleting)
        {
            Smithbox.Log<FileUnpackerTool>(
                LOC.Get("FILE_FileUnpacker_Game_Files_Being_Deleted"));
            return;
        }

        IsUnpacking = true;

        lock (FailedUnpackEntries)
            FailedUnpackEntries.Clear();

        // Exact top-folder match: "/map" must not also pick up a "/mapstudio" folder, and "/" means root files only
        bool IsFolderSelected(string folder)
        {
            return SelectiveFolderDict.TryGetValue(GetTopFolder(folder), out var selected) && selected;
        }

        var newFileDictionary = new FileDictionary();
        newFileDictionary.Entries = BaseFileDictionary.Entries
            .Where(e => IsFolderSelected(e.Folder)).ToHashSet();

        // Off the UI thread from the start: planning 100k+ entries and creating their folders takes a moment
        var root = GetUnpackRoot();
        _ = Task.Run(() => UnpackGameAsync(newFileDictionary, root));
    }

    public void DeleteUnpackedFilesAction()
    {
        if (IsDeleting)
        {
            Smithbox.Log<FileUnpackerTool>(
                LOC.Get("FILE_FileUnpacker_Game_Files_Being_Deleted"));
            return;
        }

        if (IsUnpacking)
        {
            Smithbox.Log<FileUnpackerTool>(
                LOC.Get("FILE_FileUnpacker_Game_Data_Already_Unpacked"));
            return;
        }

        var root = GetUnpackRoot();
        var manifestPath = Path.Combine(root, ManifestFileName);

        HashSet<string> manifest;
        try
        {
            manifest = ReadManifest(manifestPath);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_FileUnpacker_Manifest_Read_Failed", manifestPath), e);
            return;
        }

        // Only files this tool created are ever deleted. Without a manifest there is nothing known to be ours.
        if (manifest.Count == 0)
        {
            Smithbox.Log<FileUnpackerTool>(LOC.Get("FILE_FileUnpacker_No_Manifest", root));
            return;
        }

        var dialog = PlatformUtils.Instance.MessageBox(
            LOC.Get("FILE_FileUnpacker_Delete_Confirm", manifest.Count, root),
            LOC.Get("SYS_Warning_Header"),
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (dialog is not DialogResult.Yes)
            return;

        IsDeleting = true;

        _ = DeleteUnpackedDataAsync(root, manifestPath, manifest);
    }

    public bool HasUnpackedGame()
    {
        bool anyExist = false;

        var unpackPath = Project.Descriptor.DataPath;
        if (UnpackDirectory != "")
            unpackPath = UnpackDirectory;

        foreach (var folderName in TopFolderList)
        {
            // The root always exists, so it says nothing about an unpack
            if (folderName == RootFolderKey)
                continue;

            string fullPath = $@"{unpackPath}/{folderName}";

            if (Directory.Exists(fullPath))
            {
                anyExist = true;
                break;
            }
        }

        return anyExist;
    }

    /// <summary>
    /// Writes every entry under root. Existing files are left alone and not claimed, so the manifest only ever lists
    /// files this tool created. Every failure is caught per entry, and the state is always reset when the job ends.
    /// </summary>
    public async Task UnpackGameAsync(FileDictionary targetFileDictionary, string root)
    {
        IsUnpacking = true;
        LastJobSummary = "";

        var cts = new CancellationTokenSource();
        UnpackCancelToken = cts;
        var token = cts.Token;

        TotalToUnpack = targetFileDictionary.Entries.Count;
        CurrentUnpacked = 0;

        var written = 0;
        var skipped = 0;
        var failed = 0;
        var cancelled = false;

        var manifestPath = Path.Combine(root, ManifestFileName);
        var manifestLock = new object();
        StreamWriter manifestWriter = null;

        var archiveHandles = new Dictionary<string, SafeFileHandle>(StringComparer.OrdinalIgnoreCase);
        var stopwatch = Stopwatch.StartNew();

        void AddFailure(string path, string error)
        {
            Interlocked.Increment(ref failed);

            lock (FailedUnpackEntries)
                FailedUnpackEntries.Add((path, error));
        }

        try
        {
            Directory.CreateDirectory(root);

            var newManifest = !File.Exists(manifestPath);
            manifestWriter = new StreamWriter(manifestPath, append: true);
            if (newManifest)
                manifestWriter.WriteLine(ManifestHeader);

            // Plan: resolve every entry once, up front
            var work = new List<UnpackWork>(targetFileDictionary.Entries.Count);
            var folders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in targetFileDictionary.Entries)
            {
                var absPath = ResolveUnderRoot(root, entry.Path);
                if (absPath == null)
                {
                    AddFailure(entry.Path, LOC.Get("FILE_FileUnpacker_Invalid_Path"));
                    Interlocked.Increment(ref CurrentUnpacked);
                    continue;
                }

                var archiveFile = FindArchiveFile(entry.Path);
                var bdtPath = archiveFile?.Bdt?.Name;

                work.Add(new UnpackWork
                {
                    Path = entry.Path,
                    AbsPath = absPath,
                    BdtPath = bdtPath,
                    Header = bdtPath != null ? archiveFile.FileHeader : null
                });

                folders.Add(Path.GetDirectoryName(absPath));
            }

            // Folders once, instead of a CreateDirectory call per file. A failure here shows up on the files.
            foreach (var folder in folders)
            {
                try { Directory.CreateDirectory(folder); }
                catch { }
            }

            // Archive order: each .bdt is read front to back instead of seeking around it at random.
            // Loose files (no archive) go last.
            work.Sort((a, b) =>
            {
                var c = string.CompareOrdinal(a.BdtPath ?? "￿", b.BdtPath ?? "￿");
                return c != 0 ? c : (a.Header?.FileOffset ?? 0).CompareTo(b.Header?.FileOffset ?? 0);
            });

            // A handle of our own per archive. Positional reads on it need no lock, so workers read the same
            // archive in parallel; the VFS's shared stream serialises every read behind one lock.
            foreach (var bdtPath in work.Where(w => w.BdtPath != null).Select(w => w.BdtPath).Distinct())
            {
                archiveHandles[bdtPath] = File.OpenHandle(bdtPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }

            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Clamp(CFG.Current.FileBrowser_Unpack_Threads, 1, 64),
                CancellationToken = token
            };

            // Items are handed out in list order, so the workers move through each archive together
            await Parallel.ForEachAsync(work, options, (item, ct) =>
            {
                try
                {
                    switch (UnpackOne(item, archiveHandles))
                    {
                        case UnpackResult.Written:
                            lock (manifestLock)
                                manifestWriter.WriteLine(item.Path);

                            Interlocked.Increment(ref written);
                            break;

                        case UnpackResult.Skipped:
                            Interlocked.Increment(ref skipped);
                            break;

                        case UnpackResult.NotFound:
                            AddFailure(item.Path, LOC.Get("FILE_FileUnpacker_Failed_Unpack_Entry"));
                            break;
                    }
                }
                catch (Exception e)
                {
                    AddFailure(item.Path, e.Message);
                }
                finally
                {
                    Interlocked.Increment(ref CurrentUnpacked);
                }

                return ValueTask.CompletedTask;
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
            Smithbox.Log(this,
                LOC.Get("FILE_FileUnpacker_Unpack_Cancelled"), LogLevel.Warning);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_FileUnpacker_Unpack_Failed", root), e);
        }
        finally
        {
            try
            {
                lock (manifestLock)
                    manifestWriter?.Dispose();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FILE_FileUnpacker_Manifest_Write_Failed", manifestPath), e);
            }

            foreach (var handle in archiveHandles.Values)
                handle.Dispose();

            stopwatch.Stop();

            LastJobSummary = LOC.Get(cancelled ? "FILE_FileUnpacker_Unpack_Summary_Cancelled" : "FILE_FileUnpacker_Unpack_Summary",
                written, skipped, failed, root, stopwatch.Elapsed.ToString(@"hh\:mm\:ss"));

            Smithbox.Log(this, LastJobSummary);

            UnpackCancelToken = null;
            cts.Dispose();
            IsUnpacking = false;
        }
    }

    private sealed class UnpackWork
    {
        public string Path;
        public string AbsPath;

        // Set when the file lives in a game archive; otherwise it is read through VanillaFS (loose files)
        public string BdtPath;
        public BHD5.FileHeader Header;
    }

    private enum UnpackResult
    {
        Written,
        Skipped,
        NotFound
    }

    private ArchiveBinderVirtualFileSystem.BinderVirtualFile FindArchiveFile(string path)
        => FileContainerReader.FindArchiveFile(Project, path);

    /// <summary>
    /// Writes one file. The output is created first with CreateNew, so an existing file is skipped without reading
    /// any archive data, and a half-written file from a failed read is removed again.
    /// </summary>
    private UnpackResult UnpackOne(UnpackWork item, Dictionary<string, SafeFileHandle> archiveHandles)
    {
        var header = item.Header;
        var length = 0L;

        if (header != null)
        {
            // The archive stores files padded to the AES block size; the real size is kept from DS3 onwards
            length = header.UnpaddedFileSize > 0 && header.UnpaddedFileSize <= header.PaddedFileSize
                ? header.UnpaddedFileSize
                : header.PaddedFileSize;
        }

        FileStream output;
        try
        {
            output = new FileStream(item.AbsPath, new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.Write,
                Share = FileShare.None,
                BufferSize = 0, // one write of the whole file, no extra buffer copy
                PreallocationSize = length
            });
        }
        catch (IOException) when (File.Exists(item.AbsPath))
        {
            return UnpackResult.Skipped;
        }

        var complete = false;

        try
        {
            using (output)
            {
                if (header != null)
                {
                    var padded = header.PaddedFileSize;
                    var buffer = ArrayPool<byte>.Shared.Rent(Math.Max(padded, 1));

                    try
                    {
                        ReadExactlyAt(archiveHandles[item.BdtPath], buffer.AsSpan(0, padded), header.FileOffset);

                        if (header.AESKey != null)
                            Decrypt(header.AESKey, buffer);

                        output.Write(buffer, 0, (int)length);
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(buffer);
                    }
                }
                else
                {
                    var data = Project.VFS.VanillaFS.ReadFile(item.Path);
                    if (data == null)
                        return UnpackResult.NotFound;

                    output.Write(data.Value.Span);
                }
            }

            complete = true;
            return UnpackResult.Written;
        }
        finally
        {
            if (!complete)
            {
                // We created this file a moment ago, so removing it never touches anything that was already there
                try { File.Delete(item.AbsPath); }
                catch { }
            }
        }
    }

    private static void ReadExactlyAt(SafeFileHandle handle, Span<byte> buffer, long offset)
    {
        while (buffer.Length > 0)
        {
            var read = RandomAccess.Read(handle, buffer, offset);
            if (read <= 0)
                throw new EndOfStreamException($"Archive ended early at offset {offset}");

            buffer = buffer[read..];
            offset += read;
        }
    }

    // One AES instance per worker thread: BHD5.AesKey.Decrypt shares a single static instance across all threads
    [ThreadStatic]
    private static Aes ThreadAes;

    /// <summary>
    /// The same in-place ECB decryption of the key's ranges as BHD5.AesKey.Decrypt
    /// </summary>
    private static void Decrypt(BHD5.AesKey key, byte[] bytes)
    {
        var aes = ThreadAes;
        if (aes == null)
        {
            aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            aes.KeySize = 128;
            ThreadAes = aes;
        }

        using var decryptor = aes.CreateDecryptor(key.Key, new byte[16]);

        foreach (var range in key.Ranges)
        {
            if (range.StartOffset == -1 || range.EndOffset == -1 || range.StartOffset == range.EndOffset)
                continue;

            var start = (int)range.StartOffset;
            var count = (int)(range.EndOffset - range.StartOffset);
            decryptor.TransformBlock(bytes, start, count, bytes, start);
        }
    }

    /// <summary>
    /// Deletes only the files listed in the manifest, then removes folders that became empty, deepest first, without
    /// going above root. Files that could not be deleted stay in the manifest so a later Delete can retry them.
    /// </summary>
    public async Task DeleteUnpackedDataAsync(string root, string manifestPath, HashSet<string> manifest)
    {
        IsDeleting = true;
        LastJobSummary = "";

        var cts = new CancellationTokenSource();
        DeleteCancelToken = cts;
        var token = cts.Token;

        TotalToDelete = manifest.Count;
        CurrentDeleted = 0;

        var deleted = new System.Collections.Concurrent.ConcurrentBag<string>();
        var touchedFolders = new System.Collections.Concurrent.ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        var failed = 0;
        var cancelled = false;

        try
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = MaxConcurrentUnpacks,
                CancellationToken = token
            };

            await Parallel.ForEachAsync(manifest, options, (relPath, ct) =>
            {
                try
                {
                    var absPath = ResolveUnderRoot(root, relPath);
                    if (absPath == null)
                    {
                        // Never act on a manifest line that points outside the unpack directory
                        Interlocked.Increment(ref failed);
                        Smithbox.LogError(this, LOC.Get("FILE_FileUnpacker_Invalid_Path_Delete", relPath));
                        return ValueTask.CompletedTask;
                    }

                    if (File.Exists(absPath))
                        File.Delete(absPath);

                    deleted.Add(relPath);
                    touchedFolders.TryAdd(Path.GetDirectoryName(absPath), 0);
                }
                catch (Exception e)
                {
                    Interlocked.Increment(ref failed);
                    Smithbox.LogError(this, LOC.Get("FILE_FileUnpacker_Failed_File_Delete", relPath), e);
                }
                finally
                {
                    Interlocked.Increment(ref CurrentDeleted);
                }

                return ValueTask.CompletedTask;
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
            Smithbox.Log(this,
                LOC.Get("FILE_FileUnpacker_Delete_Cancelled"), LogLevel.Warning);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FILE_FileUnpacker_Delete_Failed", root), e);
        }
        finally
        {
            var removedFolders = RemoveEmptyFolders(root, touchedFolders.Keys);

            try
            {
                var remaining = new HashSet<string>(manifest, StringComparer.OrdinalIgnoreCase);
                remaining.ExceptWith(deleted);
                WriteManifest(manifestPath, remaining);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FILE_FileUnpacker_Manifest_Write_Failed", manifestPath), e);
            }

            LastJobSummary = LOC.Get(cancelled ? "FILE_FileUnpacker_Delete_Summary_Cancelled" : "FILE_FileUnpacker_Delete_Summary",
                deleted.Count, removedFolders, failed, root);

            Smithbox.Log(this, LastJobSummary);

            DeleteCancelToken = null;
            cts.Dispose();
            IsDeleting = false;
        }
    }

    /// <summary>
    /// Removes each folder, and then its parents, while they are empty. Stops at root, which is never removed.
    /// Folders holding anything else (game files, mods) are left as they are.
    /// </summary>
    private int RemoveEmptyFolders(string root, IEnumerable<string> folders)
    {
        var removed = 0;
        var rootFull = Path.TrimEndingDirectorySeparator(root);

        // Deepest first, so a parent is checked after its children are gone
        foreach (var folder in folders.OrderByDescending(f => f.Length))
        {
            var current = folder;

            while (current != null
                && current.Length > rootFull.Length
                && current.StartsWith(rootFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (!Directory.Exists(current) || Directory.EnumerateFileSystemEntries(current).Any())
                        break;

                    Directory.Delete(current, false);
                    removed++;
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("FILE_FileUnpacker_Failed_Folder_Delete", current), e);
                    break;
                }

                current = Path.GetDirectoryName(current);
            }
        }

        return removed;
    }

    public void UpdateBaseFileDictionary()
    {
        // Get the unmerged base file dictionary
        var folder = Path.Join(AppContext.BaseDirectory, "Assets", "File Dictionaries");
        var file = "";

        switch (Project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                file = "DES-File-Dictionary.json"; break;
            case ProjectType.DS1:
                file = "DS1-File-Dictionary.json"; break;
            case ProjectType.DS1R:
                file = "DS1R-File-Dictionary.json"; break;
            case ProjectType.DS2:
                file = "DS2-File-Dictionary.json"; break;
            case ProjectType.DS2S:
                file = "DS2S-File-Dictionary.json"; break;
            case ProjectType.DS3:
                file = "DS3-File-Dictionary.json"; break;
            case ProjectType.BB:
                file = "BB-File-Dictionary.json"; break;
            case ProjectType.SDT:
                file = "SDT-File-Dictionary.json"; break;
            case ProjectType.ER:
                file = "ER-File-Dictionary.json"; break;
            case ProjectType.AC6:
                file = "AC6-File-Dictionary.json"; break;
            case ProjectType.NR:
                file = "NR-File-Dictionary.json"; break;
            case ProjectType.DB_NT:
                file = "DB-NT-File-Dictionary.json"; break;
            case ProjectType.DB:
                file = "DB-File-Dictionary.json"; break;
            default: break;
        }

        var filepath = Path.Join(folder, file);

        var baseFileDictionary = new FileDictionary
        {
            Entries = new()
        };

        if (File.Exists(filepath))
        {
            try
            {
                var filestring = File.ReadAllText(filepath);

                try
                {
                    baseFileDictionary = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FileDictionary);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, 
                        LOC.Get("FILE_FileUnpacker_Failed_Deserialize_File_Dict", filepath), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this,
                    LOC.Get("FILE_FileUnpacker_Failed_Read_File_Dict", filepath), e);
            }
        }

        // Only validated entries exist in the game archives (the project locator applies the same filter);
        // the others can never be read and would all end up as failures
        baseFileDictionary.Entries = baseFileDictionary.Entries
            .Where(e => e.Validated)
            .ToHashSet();

        BaseFileDictionary = baseFileDictionary;

        SelectiveFolderDict.Clear();
        TopFolderList = new();

        foreach (var entry in baseFileDictionary.Entries)
        {
            var topFolder = GetTopFolder(entry.Folder);

            if (!TopFolderList.Contains(topFolder))
            {
                TopFolderList.Add(topFolder);

                if (!SelectiveFolderDict.ContainsKey(topFolder))
                {
                    SelectiveFolderDict.Add(topFolder, true);
                }
            }
        }

        // Root first, then the folders by name, so the toggle list reads like the File List
        TopFolderList.Sort(StringComparer.OrdinalIgnoreCase);
        SelectiveFolderDict = SelectiveFolderDict
            .OrderBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(e => e.Key, e => e.Value);
    }

    /// <summary>
    /// "/" for files directly in the data root, otherwise the first folder ("/chr/c0000" -> "/chr")
    /// </summary>
    private static string GetTopFolder(string folder)
    {
        var parts = (folder ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length == 0 ? RootFolderKey : $"/{parts[0]}";
    }
}
