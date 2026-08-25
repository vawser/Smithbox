using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.ModelEditor;

public class ModelBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Models = new();

    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> MapPieces = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Characters = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Assets = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Parts = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Collisions = new();

    public ModelBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // Map Pieces
        foreach (var entry in Project.Locator.MapPieceFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);
            newEntry.DeriveMapID();

            if(!MapPieces.ContainsKey(entry))
                MapPieces.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Characters
        foreach (var entry in Project.Locator.ChrFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);

            if (!Characters.ContainsKey(entry))
                Characters.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Assets
        foreach (var entry in Project.Locator.AssetFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);

            if (!Assets.ContainsKey(entry))
                Assets.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Parts
        foreach (var entry in Project.Locator.PartFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);

            if (!Parts.ContainsKey(entry))
                Parts.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Collisions
        foreach (var entry in Project.Locator.CollisionFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(Project, entry, TargetFS);

            if (!Collisions.ContainsKey(entry))
                Collisions.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        Models.Clear();
        MapPieces.Clear();
        Characters.Clear();
        Assets.Clear();
        Parts.Clear();
        Collisions.Clear();

        Models = null;
        MapPieces = null;
        Characters = null;
        Assets = null;
        Parts = null;
        Collisions = null;
    }
    #endregion
}

public class ModelContainerWrapper
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;

    public string MapID { get; set; }

    public string Name { get; set; }
    public string Path { get; set; }

    public List<ModelWrapper> Models { get; set; }

    public ModelContainerWrapper(ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        Project = project;
        TargetFS = targetFS;
        Name = dictEntry.Filename;
        Path = dictEntry.Path;

        Models = new();
    }

    /// <summary>
    /// Used to store the map ID for map pieces and collision wrappers
    /// </summary>
    public void DeriveMapID()
    {
        var parts = Path.Split("/");

        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            // '/map/m30/m30_00_00_00/...'
            if(parts.Length >= 3)
            {
                MapID = parts[3];
            }
        }
        else
        {
            // '/map/m30_00_00_00/...'
            if (parts.Length >= 2)
            {
                MapID = parts[2];

                if(Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    MapID = FilePathUtils.GetPureFilename(parts[3]);
                }
            }
        }
    }

    public void PopulateModelList()
    {
        Models.Clear();

        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, Path);

        if (binderType is ResourceContainerType.None)
        {
            PopulateDirect();
        }
        else if (binderType is ResourceContainerType.BND)
        {
            if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                PopulateBND3();
            }
            else
            {
                PopulateBND4();
            }
        }
        else if (binderType is ResourceContainerType.BXF)
        {
            PopulateBXF();
        }
    }

    public void PopulateDirect()
    {
        var modelWrapper = new ModelWrapper(this, Name);
        Models.Add(modelWrapper);
    }

    public void PopulateBND3()
    {
        var fs = TargetFS;

        if (fs.FileExists(Path))
        {
            try
            {
                var fileData = fs.ReadFile(Path);

                if (fileData != null)
                {
                    var binder = new BND3Reader(fileData.Value);

                    foreach (var file in binder.Files)
                    {
                        var filename = FilePathUtils.GetPureFilename(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            var modelWrapper = new ModelWrapper(this, filename);
                            Models.Add(modelWrapper);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Path), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Path));
        }
    }

    public void PopulateBND4()
    {
        var fs = TargetFS;

        if (fs.FileExists(Path))
        {
            try
            {
                var fileData = fs.ReadFile(Path);

                if (fileData != null)
                {
                    var binder = new BND4Reader(fileData.Value);

                    foreach (var file in binder.Files)
                    {
                        var filename = FilePathUtils.GetPureFilename(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            var modelWrapper = new ModelWrapper(this, filename);
                            Models.Add(modelWrapper);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Path), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Path));
        }
    }

    public void PopulateBXF()
    {
        var fs = TargetFS;

        Memory<byte> bhd = new Memory<byte>();
        Memory<byte> bdt = new Memory<byte>();

        var targetBhdPath = Path;
        var targetBdtPath = Path.Replace("bhd", "bdt");

        if (fs.FileExists(targetBhdPath) && fs.FileExists(targetBhdPath))
        {
            bhd = (Memory<byte>)fs.ReadFile(targetBhdPath);
            bdt = (Memory<byte>)fs.ReadFile(targetBdtPath);

            if (bhd.Length == 0 || bdt.Length == 0)
                return;

            if (Project.Descriptor.ProjectType is ProjectType.DES
                or ProjectType.DS1
                or ProjectType.DS1R)
            {
                PopulateBXF3(bhd, bdt);
            }
            else
            {
                PopulateBXF4(bhd, bdt);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", targetBhdPath));
        }
    }

    public void PopulateBXF3(Memory<byte> bhdData, Memory<byte> bdtData)
    {
        var binder = new BXF3Reader(bhdData, bdtData);

        foreach (var file in binder.Files)
        {
            var filename = FilePathUtils.GetPureFilename(file.Name);
            var filepath = file.Name.ToLower();

            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
            {
                var modelWrapper = new ModelWrapper(this, filename);
                Models.Add(modelWrapper);
            }
        }
    }

    public void PopulateBXF4(Memory<byte> bhdData, Memory<byte> bdtData)
    {
        var binder = new BXF4Reader(bhdData, bdtData);

        foreach (var file in binder.Files)
        {
            var filename = FilePathUtils.GetPureFilename(file.Name);
            var filepath = file.Name.ToLower();

            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
            {
                var modelWrapper = new ModelWrapper(this, filename);
                Models.Add(modelWrapper);
            }
        }
    }
}

public class ModelWrapper
{
    public ModelContainerWrapper Parent { get; set; }

    public string Name { get; set; }

    public ModelContainer Container { get; set; }

    public ModelWrapper(ModelContainerWrapper parent, string name)
    {
        Parent = parent;
        Name = name;
    }

    // FLVER
    public FLVER2 FLVER { get; set; }

    // HKXPWV
    public HKXPWV HKXPWV { get; set; }

    // CLM2
    public CLM2 CLM2 { get; set; }

    // EDGE
    public EDGE EDGE { get; set; }

    // GRASS
    public GRASS GRASS { get; set; }


    public void Load()
    {
        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Parent.Project, Parent.Path);

        if (binderType is ResourceContainerType.None)
        {
            LoadDirect();
        }
        else if (binderType is ResourceContainerType.BND)
        {
            if (Parent.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                LoadBND3();
            }
            else
            {
                LoadBND4();
            }
        }
        else if (binderType is ResourceContainerType.BXF)
        {
            LoadBXF();
        }

        // FLVER
        if (FLVER != null)
        {
            var modelEditor = Parent.Project.Handler.ModelEditor;

            var activeView = modelEditor.ViewHandler.ActiveView;

            if (activeView != null)
            {
                activeView.Universe.LoadModel(this);
            }
        }
    }

    public void LoadDirect()
    {
        var fileData = Parent.TargetFS.ReadFile(Parent.Path);
        if (fileData != null)
        {
            FLVER = FLVER2.Read(fileData.Value);
        }
    }

    public void LoadBND3()
    {
        var fs = Parent.TargetFS;

        if (fs.FileExists(Parent.Path))
        {
            try
            {
                var fileData = fs.ReadFile(Parent.Path);

                if (fileData != null)
                {
                    var binder = new BND3Reader(fileData.Value);

                    foreach (var file in binder.Files)
                    {
                        var filename = FilePathUtils.GetPureFilename(file.Name);
                        var filepath = file.Name.ToLower();

                        var data = binder.ReadFile(file);

                        ReadFile(filepath, filename, data);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Parent.Path), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Parent.Path));
        }
    }

    public void LoadBND4()
    {
        var fs = Parent.TargetFS;

        if (fs.FileExists(Parent.Path))
        {
            try
            {
                var fileData = fs.ReadFile(Parent.Path);

                if (fileData != null)
                {
                    var binder = new BND4Reader(fileData.Value);

                    foreach (var file in binder.Files)
                    {
                        var filename = FilePathUtils.GetPureFilename(file.Name);
                        var filepath = file.Name.ToLower();

                        var data = binder.ReadFile(file);

                        ReadFile(filepath, filename, data);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Parent.Path), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Parent.Path));
        }
    }

    public void LoadBXF()
    {
        var fs = Parent.TargetFS;

        Memory<byte> bhd = new Memory<byte>();
        Memory<byte> bdt = new Memory<byte>();

        var targetBhdPath = Parent.Path;
        var targetBdtPath = Parent.Path.Replace("bhd", "bdt");

        if (fs.FileExists(targetBhdPath) && fs.FileExists(targetBhdPath))
        {
            bhd = (Memory<byte>)fs.ReadFile(targetBhdPath);
            bdt = (Memory<byte>)fs.ReadFile(targetBdtPath);

            if (bhd.Length == 0 || bdt.Length == 0)
                return;

            if (Parent.Project.Descriptor.ProjectType is ProjectType.DES
                or ProjectType.DS1
                or ProjectType.DS1R)
            {
                LoadBXF3(bhd, bdt);
            }
            else
            {
                LoadBXF4(bhd, bdt);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", targetBhdPath));
        }
    }

    public void LoadBXF3(Memory<byte> bhdData, Memory<byte> bdtData)
    {
        var binder = new BXF3Reader(bhdData, bdtData);

        foreach (var file in binder.Files)
        {
            var filename = FilePathUtils.GetPureFilename(file.Name);
            var filepath = file.Name.ToLower();

            var data = binder.ReadFile(file);

            ReadFile(filepath, filename, data);
        }
    }

    public void LoadBXF4(Memory<byte> bhdData, Memory<byte> bdtData)
    {
        var binder = new BXF4Reader(bhdData, bdtData);

        foreach (var file in binder.Files)
        {
            var filename = FilePathUtils.GetPureFilename(file.Name);
            var filepath = file.Name.ToLower();

            var data = binder.ReadFile(file);

            ReadFile(filepath, filename, data);
        }
    }

    public void ReadFile(string filepath, string filename, Memory<byte> data)
    {
        // FLVER
        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
        {
            if (filename == Name)
            {

                try
                {
                    FLVER = FLVER2.Read(data);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_FLVER", filepath), e);
                }
            }
        }

        // CLM2
        if (filepath.Contains(".clm2"))
        {
            if (filename.Contains(Name))
            {
                try
                {
                    CLM2 = CLM2.Read(data);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_CLM2", filepath), e);
                }
            }
        }

        // HKXPWV
        if (filepath.Contains(".hkxpwv"))
        {
            if (filename == Name)
            {
                try
                {
                    HKXPWV = HKXPWV.Read(data);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_HKXPWV", filepath), e);
                }
            }
        }

        // GRASS
        if (Parent.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (filepath.Contains(".grass"))
            {
                if (filename == Name)
                {
                    try
                    {
                        GRASS = GRASS.Read(data);
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_GRASS", filepath), e);
                    }
                }
            }
        }

        // EDGE
        if (Parent.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (filepath.Contains(".edge"))
            {
                if (filename == Name)
                {
                    try
                    {
                        EDGE = EDGE.Read(data);
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_EDGE", filepath), e);
                    }
                }
            }
        }
    }

    public void Save()
    {
        var containerPath = Parent.Path;
        var project = Parent.Project;
        var fs = Parent.TargetFS;

        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(project, containerPath);

        // Updates the FLVER object with changes from the ModelContainer
        UpdateFLVER();

        if (binderType is ResourceContainerType.None)
        {
            SaveDirect();
        }
        else if (binderType is ResourceContainerType.BND)
        {
            if (Parent.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                SaveBND3();
            }
            else
            {
                SaveBND4();
            }
        }
        else if (binderType is ResourceContainerType.BXF)
        {
            if (Parent.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                SaveBXF3();
            }
            else
            {
                SaveBXF4();
            }
        }
    }

    public void SaveDirect()
    {
        var containerPath = Parent.Path;

        try
        {
            var data = FLVER.Write();
            Parent.Project.VFS.ProjectFS.WriteFile(containerPath, data);

            Smithbox.Log(this, LOC.Get("MODEL_Data_Write_FLVER_Log", containerPath));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_FLVER", containerPath), e);
        }
    }

    public void SaveBND3()
    {
        var fs = Parent.TargetFS;

        if (fs.FileExists(Parent.Path))
        {
            try
            {
                var fileData = fs.ReadFile(Parent.Path);

                var anyWritten = false;

                if (fileData != null)
                {
                    var binder = BND3.Read(fileData.Value);

                    foreach (var file in binder.Files)
                    {
                        var filename = FilePathUtils.GetPureFilename(file.Name);
                        var filepath = file.Name.ToLower();

                        WriteFile(filepath, filename, file);
                        anyWritten = true;
                    }

                    if (anyWritten)
                    {
                        var outBinderData = binder.Write();
                        Parent.Project.VFS.ProjectFS.WriteFile(Parent.Path, outBinderData);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Parent.Path), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Parent.Path));
        }
    }

    public void SaveBND4()
    {
        var fs = Parent.TargetFS;

        if (fs.FileExists(Parent.Path))
        {
            try
            {
                var fileData = fs.ReadFile(Parent.Path);

                var anyWritten = false;

                if (fileData != null)
                {
                    var binder = BND4.Read(fileData.Value);

                    foreach (var file in binder.Files)
                    {
                        var filename = FilePathUtils.GetPureFilename(file.Name);
                        var filepath = file.Name.ToLower();

                        WriteFile(filepath, filename, file);
                        anyWritten = true;
                    }

                    if (anyWritten)
                    {
                        var outBinderData = binder.Write();
                        Parent.Project.VFS.ProjectFS.WriteFile(Parent.Path, outBinderData);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Parent.Path), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Parent.Path));
        }
    }

    public void SaveBXF3()
    {
        var fs = Parent.TargetFS;

        var bhdData = new Memory<byte>?();
        var bdtData = new Memory<byte>?();

        var targetBhdPath = Parent.Path;
        var targetBdtPath = Parent.Path.Replace("bhd", "bdt");

        var writePathBhd = Path.Combine(Parent.Project.Descriptor.ProjectPath, targetBhdPath);
        var writePathBdt = Path.Combine(Parent.Project.Descriptor.ProjectPath, targetBdtPath);

        if (fs.FileExists(targetBhdPath) && fs.FileExists(writePathBdt))
        {
            try
            {
                bhdData = fs.ReadFile(targetBhdPath);
                bdtData = fs.ReadFile(targetBdtPath);

                var anyWritten = false;

                if (bhdData != null && bdtData != null)
                {
                    var binder = BXF3.Read(bhdData.Value, bdtData.Value);

                    foreach (var file in binder.Files)
                    {
                        var filename = FilePathUtils.GetPureFilename(file.Name);
                        var filepath = file.Name.ToLower();

                        WriteFile(filepath, filename, file);
                        anyWritten = true;
                    }

                    if (anyWritten)
                    {
                        binder.Write(out byte[] newBhdData, out byte[] newBdtData);

                        Parent.Project.VFS.ProjectFS.WriteFile(writePathBhd, newBhdData);
                        Parent.Project.VFS.ProjectFS.WriteFile(writePathBdt, newBdtData);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Parent.Path), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Parent.Path));
        }
    }

    public void SaveBXF4()
    {
        var fs = Parent.TargetFS;

        var bhdData = new Memory<byte>?();
        var bdtData = new Memory<byte>?();

        var targetBhdPath = Parent.Path;
        var targetBdtPath = Parent.Path.Replace("bhd", "bdt");

        var writePathBhd = Path.Combine(Parent.Project.Descriptor.ProjectPath, targetBhdPath);
        var writePathBdt = Path.Combine(Parent.Project.Descriptor.ProjectPath, targetBdtPath);

        if (fs.FileExists(targetBhdPath) && fs.FileExists(writePathBdt))
        {
            try
            {
                bhdData = fs.ReadFile(targetBhdPath);
                bdtData = fs.ReadFile(targetBdtPath);

                var anyWritten = false;

                if (bhdData != null && bdtData != null)
                {
                    var binder = BXF4.Read(bhdData.Value, bdtData.Value);

                    foreach (var file in binder.Files)
                    {
                        var filename = FilePathUtils.GetPureFilename(file.Name);
                        var filepath = file.Name.ToLower();

                        WriteFile(filepath, filename, file);
                        anyWritten = true;
                    }

                    if (anyWritten)
                    {
                        binder.Write(out byte[] newBhdData, out byte[] newBdtData);

                        Parent.Project.VFS.ProjectFS.WriteFile(writePathBhd, newBhdData);
                        Parent.Project.VFS.ProjectFS.WriteFile(writePathBdt, newBdtData);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", Parent.Path), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Find_Binder", Parent.Path));
        }
    }

    public void WriteFile(string filepath, string filename, BinderFile file)
    {
        // FLVER
        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
        {
            if (filename == Name)
            {
                try
                {
                    file.Bytes = FLVER.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_FLVER_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_FLVER", filepath), e);
                }
            }
        }

        // CLM2
        if (filepath.Contains(".clm2"))
        {
            if (filename.Contains(Name))
            {
                try
                {
                    file.Bytes = CLM2.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_CLM2_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_CLM2", filepath), e);
                }
            }
        }

        // HKXPWV
        if (filepath.Contains(".hkxpwv"))
        {
            if (filename == Name)
            {
                try
                {
                    file.Bytes = HKXPWV.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_HKXPWV_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_HKXPWV", filepath), e);
                }
            }
        }

        // GRASS
        if (Parent.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (filepath.Contains(".grass"))
            {
                try
                {
                    file.Bytes = GRASS.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_GRASS_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_GRASS", filepath), e);
                }
            }
        }

        // EDGE
        if (Parent.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (filepath.Contains(".edge"))
            {
                try
                {
                    file.Bytes = EDGE.Write();

                    Smithbox.Log(this, LOC.Get("MODEL_Data_Write_EDGE_Log", filepath));
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Write_EDGE", filepath), e);
                }
            }
        }
    }

    public void Unload()
    {
        var modelEditor = Parent.Project.Handler.ModelEditor;

        var activeView = modelEditor.ViewHandler.ActiveView;

        if (activeView != null)
        {
            activeView.ViewportActionManager.Clear();
            activeView.ActionManager.Clear();

            activeView.EntityTypeCache.InvalidateCache();

            activeView.Universe.UnloadModel(this);
        }
    }

    public void UpdateFLVER()
    {
        // Dummies
        FLVER.Dummies.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER.Dummy entDummy)
            {
                FLVER.Dummies.Add(entDummy);
            }
        }

        // Nodes
        FLVER.Nodes.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER.Node entNode)
            {
                FLVER.Nodes.Add(entNode);
            }
        }

        // Materials
        FLVER.Materials.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER2.Material entMat)
            {
                FLVER.Materials.Add(entMat);
            }
        }

        // Meshes
        FLVER.Meshes.Clear();
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER2.Mesh entMesh)
            {
                FLVER.Meshes.Add(entMesh);
            }
        }

        // SkeletonSet
        FLVER.Skeletons = null;
        foreach (var entry in Container.Objects)
        {
            if (entry.WrappedObject is FLVER2.SkeletonSet entSkeletonSet)
            {
                FLVER.Skeletons = entSkeletonSet;
            }
        }
    }

}