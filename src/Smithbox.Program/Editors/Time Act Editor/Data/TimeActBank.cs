using Andre.IO.VFS;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;
    public string Name;

    private List<TimeActContainerWrapper> Characters = new();
    private List<TimeActContainerWrapper> Assets = new();

    // The sources are added by the scan while the source list is displayed
    private readonly object SourceLock = new();
    private CancellationTokenSource ScanCancellation = new();

    public int ScanProgress => _scanProgress;
    public int ScanTotal => _scanTotal;
    public bool IsScanning => _scanProgress < _scanTotal;

    private volatile int _scanProgress;
    private volatile int _scanTotal;

    public TimeActBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        var characterFiles = Project.Locator.TimeActFiles.Entries
            .Where(IsCharacterSource)
            .ToList();
        var assetFiles = Project.Locator.AssetFiles.Entries.ToList();

        _scanTotal = characterFiles.Count + assetFiles.Count;

        var token = ScanCancellation.Token;

        _ = Task.Run(() => ScanSources(characterFiles, assetFiles, token), token);

        return true;
    }

    /// <summary>
    /// The files named after a character variant only hold HKX files split from the main anibnd
    /// </summary>
    private bool IsCharacterSource(FileDictionaryEntry entry)
    {
        return entry.Folder.StartsWith("/chr") && !entry.Filename.Contains('_');
    }

    /// <summary>
    /// Whether a container holds time acts can't be told from the file dictionary: objects and assets
    /// hold them in a nested anibnd that most of them don't have, and some anibnds don't have any.
    /// The containers have to be read to find out, which takes too long to block the project load for.
    /// </summary>
    private void ScanSources(List<FileDictionaryEntry> characterFiles, List<FileDictionaryEntry> assetFiles, CancellationToken token)
    {
        var modelFiles = new Dictionary<string, FileDictionaryEntry>();

        foreach (var entry in Project.Locator.ChrFiles.Entries)
        {
            modelFiles.TryAdd(entry.Filename, entry);
        }

        var scannedFiles = new HashSet<string>();

        foreach (var entry in characterFiles)
        {
            modelFiles.TryGetValue(entry.Filename, out var modelEntry);

            ScanSource(entry, modelEntry, Characters, scannedFiles, token);
        }

        scannedFiles.Clear();

        foreach (var entry in assetFiles)
        {
            ScanSource(entry, entry, Assets, scannedFiles, token);
        }
    }

    private void ScanSource(FileDictionaryEntry sourceEntry, FileDictionaryEntry modelEntry,
        List<TimeActContainerWrapper> sources, HashSet<string> scannedFiles, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;

        // Some projects hold both the compressed and uncompressed version of a file
        if (scannedFiles.Add(sourceEntry.Filename))
        {
            try
            {
                var wrapper = new TimeActContainerWrapper(Project, sourceEntry, modelEntry, TargetFS);

                if (wrapper.ReadTimeActs().Count > 0)
                {
                    lock (SourceLock)
                    {
                        if (!token.IsCancellationRequested)
                        {
                            sources.Add(wrapper);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Source", sourceEntry.Path), e);
            }
        }

        _scanProgress++;
    }

    public List<TimeActContainerWrapper> GetCharacters()
    {
        lock (SourceLock)
        {
            return Characters.ToList();
        }
    }

    public List<TimeActContainerWrapper> GetAssets()
    {
        lock (SourceLock)
        {
            return Assets.ToList();
        }
    }

    #region Dispose
    public void Dispose()
    {
        ScanCancellation.Cancel();
        ScanCancellation.Dispose();

        lock (SourceLock)
        {
            Characters.Clear();
            Assets.Clear();
        }
    }
    #endregion
}
