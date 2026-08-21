using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;

namespace StudioCore.Editors.HavokEditor;

// Cutscene HKX within cutscene/ folder
// <x>.cutscenebnd.dcx
// -> cut<xxxx>/<x>.hkx
public class HavokCutsceneBank : IDisposable
{
    public ProjectEntry Project;

    public Dictionary<HavokFileLocation, hkRootLevelContainer> Files = new();

    public HavokCutsceneBank(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        foreach (var entry in Project.Locator.HavokCutsceneFiles.Entries)
        {
            PopulateFiles(entry);
        }

        return true;
    }

    // Populate the files dictionary when a source binder is loaded
    public void PopulateFiles(FileDictionaryEntry fileEntry)
    {
        var binderData = Project.VFS.FS.ReadFile(fileEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileEntry.Path);
        }

        if (binderData == null)
            return;

        var binder = new BND4Reader(binderData.Value);
        foreach (var file in binder.Files)
        {
            var newFileLocation = new HavokFileLocation()
            {
                FileEntry = fileEntry,
                InternalFilename = Path.GetFileNameWithoutExtension(file.Name),
                InternalFilePath = file.Name
            };

            if (!Files.ContainsKey(newFileLocation))
            {
                Files.Add(newFileLocation, null);
            }
        }
    }

    public void LoadCutsceneFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        var curFileLocation = Files.FirstOrDefault(e => e.Key.FileEntry.Path == fileEntry.Path && e.Key.InternalFilePath == internalFilePath);

        var binderData = Project.VFS.FS.ReadFile(fileEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileEntry.Path);
        }

        if (binderData == null)
            return;

        HavokBinarySerializer serializer = new HavokBinarySerializer();

        var binder = new BND4Reader(binderData.Value);
        foreach (var file in binder.Files)
        {
            var name = Path.GetFileNameWithoutExtension(file.Name);

            if (file.Name != internalFilePath)
                continue;

            try
            {
                var fileBytes = binder.ReadFile(file).ToArray();

                using (MemoryStream memoryStream = new MemoryStream(fileBytes))
                {
                    hkRootLevelContainer fileHkx;

                    try
                    {
                        fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                        Files[curFileLocation.Key] = fileHkx;
                    }
                    catch (InvalidDataException ex)
                    {
                        Smithbox.LogError(this,
                            LOC.Get("HAVOK_Data_Failed_to_Read_Cutscene_HKX", name), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this,
                    LOC.Get("HAVOK_Data_Failed_to_Read_Cutscene_Binder_File", name), ex);
            }
        }
    }

    public void SaveCutsceneFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        var curFileLocation = Files.FirstOrDefault(e => e.Key.FileEntry.Path == fileEntry.Path && e.Key.InternalFilePath == internalFilePath);

        var binderData = Project.VFS.FS.ReadFile(fileEntry.Path);

        if (Project.VFS.ProjectFS.FileExists(fileEntry.Path))
        {
            binderData = Project.VFS.ProjectFS.ReadFile(fileEntry.Path);
        }

        if (binderData == null)
            return;

        bool anyWritten = false;

        HavokBinarySerializer serializer = new HavokBinarySerializer();

        var binder = BND4.Read(binderData.Value);

        foreach (var file in binder.Files)
        {
            var name = Path.GetFileNameWithoutExtension(file.Name);

            if (file.Name != internalFilePath)
                continue;

            if (!Files.ContainsKey(curFileLocation.Key))
                continue;

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(file.Bytes.ToArray()))
                {
                    serializer.Write(Files[curFileLocation.Key], memoryStream);

                    file.Bytes = memoryStream.ToArray();
                    anyWritten = true;
                }
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_Data_Failed_to_Write_Cutscene_HKX", name), ex);
            }
        }

        if (!anyWritten)
            return;

        try
        {
            var writtenBinder = binder.Write();

            Project.VFS.ProjectFS.WriteFile(fileEntry.Path, writtenBinder);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Failed_to_Write_Cutscene_HKX", fileEntry.Path), ex);
        }
    }
    #region
    public void Dispose()
    {
    }
    #endregion
}
