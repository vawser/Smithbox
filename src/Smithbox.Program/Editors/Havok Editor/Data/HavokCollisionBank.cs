using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;

namespace StudioCore.Editors.HavokEditor;

// Map Collision HKX within map/m<xx>/m<xx_xx_xx_xx>/ folders
// <x>.hkxbdt + <x>.hkxbhd
// -> <x>.hkx.dcx (uses compendium)

// Asset Collision HKX within asset/aeg/aeg<xxx>/
// <x>_h.geomhkxbnd.dcx
// <x>_l.geomhkxbnd.dcx
public class HavokCollisionBank : IDisposable
{
    public ProjectEntry Project;
    public Dictionary<HavokFileLocation, hkRootLevelContainer> MapCollisionFiles = new();
    public Dictionary<HavokFileLocation, hkRootLevelContainer> AssetCollisionFiles = new();
    public bool SetupBank = false;

    public HavokCollisionBank(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        foreach(var entry in Project.Locator.HavokCollisionFiles.Entries)
        {
            PopulateMapFiles(entry);
        }

        foreach (var entry in Project.Locator.HavokAssetFiles.Entries)
        {
            PopulateAssetFiles(entry);
        }

        SetupBank = true;

        return true;
    }

    public void PopulateMapFiles(FileDictionaryEntry fileEntry)
    {
        var bdtPath = fileEntry.Path;
        var bhdPath = fileEntry.Path.Replace("bdt", "bhd");

        var name = Path.GetFileNameWithoutExtension(fileEntry.Path);

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            foreach (var file in packedBinder.Files)
            {
                var newFileLocation = new HavokFileLocation()
                {
                    FileEntry = fileEntry,
                    InternalFilename = Path.GetFileNameWithoutExtension(file.Name),
                    InternalFilePath = file.Name
                };

                if (!MapCollisionFiles.ContainsKey(newFileLocation))
                {
                    MapCollisionFiles.Add(newFileLocation, null);
                }
            }
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this,
                LOC.Get("HAVOK_Data_Failed_to_Read_Collision_Binder_File", name), ex);
        }
    }
    public void PopulateAssetFiles(FileDictionaryEntry fileEntry)
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

            if (!AssetCollisionFiles.ContainsKey(newFileLocation))
            {
                AssetCollisionFiles.Add(newFileLocation, null);
            }
        }
    }

    public void LoadMapCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        // bdt 
        var curFileLocation = MapCollisionFiles.FirstOrDefault(e => e.Key.FileEntry.Path == fileEntry.Path && e.Key.InternalFilePath == internalFilePath);

        var bdtPath = fileEntry.Path;
        var bhdPath = fileEntry.Path.Replace("bdt", "bhd");

        var name = Path.GetFileNameWithoutExtension(internalFilePath);

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            HavokBinarySerializer serializer = new HavokBinarySerializer();

            // Get compendium
            byte[] compendiumFileBytes = null;

            foreach (var file in packedBinder.Files)
            {
                if (file.Name.Contains(".compendium.dcx"))
                {
                    compendiumFileBytes = DCX.Decompress(file.Bytes).ToArray();
                }
            }

            if (compendiumFileBytes != null)
            {
                using MemoryStream memoryStream = new MemoryStream(compendiumFileBytes);
                serializer.LoadCompendium(memoryStream);
            }

            foreach (var file in packedBinder.Files)
            {
                if (file.Name != internalFilePath)
                    continue;

                using (MemoryStream memoryStream = new MemoryStream(file.Bytes.ToArray()))
                {
                    hkRootLevelContainer fileHkx;

                    try
                    {
                        fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                        MapCollisionFiles[curFileLocation.Key] = fileHkx;
                    }
                    catch (InvalidDataException ex)
                    {
                        Smithbox.LogError(this,
                            LOC.Get("HAVOK_Data_Failed_to_Read_Collision_HKX", name), ex);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this,
                LOC.Get("HAVOK_Data_Failed_to_Read_Collision_Binder_File", name), ex);
        }
    }

    public void LoadAssetCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        var curFileLocation = AssetCollisionFiles.FirstOrDefault(e => e.Key.FileEntry.Path == fileEntry.Path && e.Key.InternalFilePath == internalFilePath);

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

                        AssetCollisionFiles[curFileLocation.Key] = fileHkx;
                    }
                    catch (InvalidDataException ex)
                    {
                        Smithbox.LogError(this,
                            LOC.Get("HAVOK_Data_Failed_to_Read_Collision_HKX", name), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this,
                    LOC.Get("HAVOK_Data_Failed_to_Read_Collision_Binder_File", name), ex);
            }
        }
    }


    public void SaveMapCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        var curFileLocation = MapCollisionFiles.FirstOrDefault(e => e.Key.FileEntry.Path == fileEntry.Path && e.Key.InternalFilePath == internalFilePath);

        var bdtPath = fileEntry.Path;
        var bhdPath = fileEntry.Path.Replace("bdt", "bhd");

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
            }
            if (Project.VFS.ProjectFS.FileExists(bhdPath))
            {
                bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            if (bdtData == null || bhdData == null)
                return;

            bool anyWritten = false;

            HavokBinarySerializer serializer = new HavokBinarySerializer();

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            foreach (var file in packedBinder.Files)
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);

                if (file.Name != internalFilePath)
                    continue;

                if (!MapCollisionFiles.ContainsKey(curFileLocation.Key))
                    continue;

                try
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        serializer.Write(MapCollisionFiles[curFileLocation.Key], memoryStream);

                        // NOTE: assumes DCX_KRAK to match ER/NR collision packaging.
                        // Swap this for whatever DCX.Type the project actually uses if different.
                        var compressedBytes = DCX.Compress(memoryStream.ToArray(), DCX.Type.DCX_KRAK);

                        file.Bytes = compressedBytes;
                        anyWritten = true;
                    }
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, LOC.Get("HAVOK_Data_Failed_to_Write_Collision_HKX", name), ex);
                }
            }

            if (!anyWritten)
                return;

            packedBinder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

            Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
            Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Failed_to_Write_Collision_Binder_File", fileEntry.Path), ex);
        }
    }
    public void SaveAssetCollisionFile(FileDictionaryEntry fileEntry, string internalFilePath)
    {
        var curFileLocation = AssetCollisionFiles.FirstOrDefault(e => e.Key.FileEntry.Path == fileEntry.Path && e.Key.InternalFilePath == internalFilePath);

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

            if (!AssetCollisionFiles.ContainsKey(curFileLocation.Key))
                continue;

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(file.Bytes.ToArray()))
                {
                    serializer.Write(AssetCollisionFiles[curFileLocation.Key], memoryStream);

                    file.Bytes = memoryStream.ToArray();
                    anyWritten = true;
                }
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_Data_Failed_to_Write_Collision_HKX", name), ex);
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
            Smithbox.LogError(this, LOC.Get("HAVOK_Data_Failed_to_Write_Collision_Binder_File", fileEntry.Path), ex);
        }
    }
    #region
    public void Dispose()
    {
    }
    #endregion
}
