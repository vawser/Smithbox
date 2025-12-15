using Andre.IO.VFS;
using DotNext.Collections.Generic;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.FileBrowserNS;
using StudioCore.Formats.JSON;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.ModelEditor;

public class ModelBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Models = new();

    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> MapPieces = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Characters = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Assets = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Parts = new();
    public Dictionary<FileDictionaryEntry, ModelContainerWrapper> Collisions = new();

    public ModelBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // Map Pieces
        foreach (var entry in Project.ModelData.MapPieceFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);
            newEntry.DeriveMapID();

            if(!MapPieces.ContainsKey(entry))
                MapPieces.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Characters
        foreach (var entry in Project.ModelData.ChrFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);

            if (!Characters.ContainsKey(entry))
                Characters.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Assets
        foreach (var entry in Project.ModelData.AssetFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);

            if (!Assets.ContainsKey(entry))
                Assets.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Parts
        foreach (var entry in Project.ModelData.PartFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);

            if (!Parts.ContainsKey(entry))
                Parts.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        // Collisions
        foreach (var entry in Project.ModelData.CollisionFiles.Entries)
        {
            var newEntry = new ModelContainerWrapper(BaseEditor, Project, entry, TargetFS);

            if (!Collisions.ContainsKey(entry))
                Collisions.Add(entry, newEntry);

            if (!Models.ContainsKey(entry))
                Models.Add(entry, newEntry);
        }

        return true;
    }
}

public class ModelContainerWrapper
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;

    public string MapID { get; set; }

    public string Name { get; set; }
    public string Path { get; set; }

    public List<ModelWrapper> Models { get; set; }

    public ModelContainerWrapper(Smithbox baseEditor, ProjectEntry project, FileDictionaryEntry dictEntry, VirtualFileSystem targetFS)
    {
        BaseEditor = baseEditor;
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

        if (Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
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
            }
        }
    }

    public void PopulateModelList()
    {
        Models.Clear();

        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, Path);

        if (binderType is ResourceContainerType.None)
        {
            var modelWrapper = new ModelWrapper(this, Name);
            Models.Add(modelWrapper);
        }

        if (binderType is ResourceContainerType.BND)
        {
            if (Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                try
                {
                    var fileData = TargetFS.ReadFile(Path);
                    if (fileData != null)
                    {
                        var binder = new BND3Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
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
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {Path} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            else
            {
                try
                {
                    var fileData = TargetFS.ReadFile(Path);
                    if (fileData != null)
                    {
                        var binder = new BND4Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
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
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {Path} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }

        if (binderType is ResourceContainerType.BXF)
        {
            Memory<byte> bhd = new Memory<byte>();
            Memory<byte> bdt = new Memory<byte>();

            var targetBhdPath = Path;
            var targetBdtPath = Path.Replace("bhd", "bdt");

            try
            {
                bhd = (Memory<byte>)TargetFS.ReadFile(targetBhdPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {targetBhdPath} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            try
            {
                bdt = (Memory<byte>)TargetFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {targetBdtPath} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            if (bhd.Length != 0 && bdt.Length != 0)
            {
                if (Project.ProjectType is ProjectType.DES
                    or ProjectType.DS1
                    or ProjectType.DS1R)
                {
                    var binder = new BXF3Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            var modelWrapper = new ModelWrapper(this, filename);
                            Models.Add(modelWrapper);
                        }
                    }
                }
                else
                {
                    var binder = new BXF4Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            var modelWrapper = new ModelWrapper(this, filename);
                            Models.Add(modelWrapper);
                        }
                    }
                }
            }
        }
    }
}

public class ModelWrapper
{
    public ModelContainerWrapper Parent { get; set; }

    public string Name { get; set; }

    public FLVER2 FLVER { get; set; }

    public ModelContainer Container { get; set; }

    public ModelWrapper(ModelContainerWrapper parent, string name)
    {
        Parent = parent;
        Name = name;
    }

    public void Load()
    {
        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Parent.Project, Parent.Path);

        if (binderType is ResourceContainerType.None)
        {
            var fileData = Parent.TargetFS.ReadFile(Parent.Path);
            if (fileData != null)
            {
                FLVER = FLVER2.Read(fileData.Value);
            }
        }

        if (binderType is ResourceContainerType.BND)
        {
            if (Parent.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                try
                {
                    var fileData = Parent.TargetFS.ReadFile(Parent.Path);
                    if (fileData != null)
                    {
                        var binder = new BND3Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                            {
                                if (filename == Name)
                                {
                                    var flverData = binder.ReadFile(file);
                                    FLVER = FLVER2.Read(flverData);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {Parent.Path} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            else
            {
                try
                {
                    var fileData = Parent.TargetFS.ReadFile(Parent.Path);
                    if (fileData != null)
                    {
                        var binder = new BND4Reader(fileData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                            {
                                if (filename == Name)
                                {
                                    var flverData = binder.ReadFile(file);
                                    FLVER = FLVER2.Read(flverData);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {Parent.Path} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }

        if (binderType is ResourceContainerType.BXF)
        {
            Memory<byte> bhd = new Memory<byte>();
            Memory<byte> bdt = new Memory<byte>();

            var targetBhdPath = Parent.Path;
            var targetBdtPath = Parent.Path.Replace("bhd", "bdt");

            try
            {
                bhd = (Memory<byte>)Parent.TargetFS.ReadFile(targetBhdPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {targetBhdPath} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            try
            {
                bdt = (Memory<byte>)Parent.TargetFS.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to read {targetBdtPath} during model load.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            if (bhd.Length != 0 && bdt.Length != 0)
            {
                if (Parent.Project.ProjectType is ProjectType.DES
                    or ProjectType.DS1
                    or ProjectType.DS1R)
                {
                    var binder = new BXF3Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            if (filename == Name)
                            {
                                var flverData = binder.ReadFile(file);
                                FLVER = FLVER2.Read(flverData);
                            }
                        }
                    }
                }
                else
                {
                    var binder = new BXF4Reader(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            if (filename == Name)
                            {
                                var flverData = binder.ReadFile(file);
                                FLVER = FLVER2.Read(flverData);
                            }
                        }
                    }
                }
            }
        }

        if (FLVER != null)
        {
            Parent.Project.ModelEditor.Universe.LoadModel(this);
        }
    }


    public void Unload()
    {
        Parent.Project.ModelEditor.Universe.UnloadModel(this);
    }

    public void Save()
    {
        var containerPath = Parent.Path;
        var project = Parent.Project;
        var fs = Parent.TargetFS;

        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(project, containerPath);

        // Place the container data in the FLVER
        // Only replaces these elements currently 

        // Dummies
        FLVER.Dummies.Clear();
        foreach (var entry in Container.Dummies)
        {
            var obj = (FLVER.Dummy)entry.WrappedObject;
            FLVER.Dummies.Add(obj);
        }

        // Nodes
        FLVER.Nodes.Clear();
        foreach (var entry in Container.Nodes)
        {
            var obj = (FLVER.Node)entry.WrappedObject;
            FLVER.Nodes.Add(obj);
        }

        // Materials
        FLVER.Materials.Clear();
        foreach (var entry in Container.Materials)
        {
            var obj = (FLVER2.Material)entry.WrappedObject;
            FLVER.Materials.Add(obj);
        }

        // Meshes
        FLVER.Meshes.Clear();
        foreach (var entry in Container.Meshes)
        {
            var obj = (FLVER2.Mesh)entry.WrappedObject;
            FLVER.Meshes.Add(obj);
        }

        // SkeletonSet
        var newSkeletonSet = Container.Skeletons.First();
        FLVER.Skeletons = (FLVER2.SkeletonSet)newSkeletonSet.WrappedObject;

        var flverData = FLVER.Write();

        if (binderType is ResourceContainerType.None)
        {
            try
            {
                project.ProjectFS.WriteFile(containerPath, flverData);
                TaskLogs.AddLog($"[Smithbox:Model Editor] Saved {containerPath}.");
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to write {containerPath} during model save.", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        if (binderType is ResourceContainerType.BND)
        {
            if (project.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                try
                {
                    var binderData = fs.ReadFile(containerPath);
                    if (binderData != null)
                    {
                        var binder = BND3.Read(binderData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                            {
                                if (filename == Name)
                                {
                                    file.Bytes = flverData;
                                }
                            }
                        }

                        var outBinderData = binder.Write();
                        project.ProjectFS.WriteFile(containerPath, outBinderData);
                        TaskLogs.AddLog($"[Smithbox:Model Editor] Saved {containerPath}.");
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to write {containerPath} during model save.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            else
            {
                try
                {
                    var binderData = fs.ReadFile(containerPath);
                    if (binderData != null)
                    {
                        var binder = BND4.Read(binderData.Value);
                        foreach (var file in binder.Files)
                        {
                            var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                            var filepath = file.Name.ToLower();

                            if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                            {
                                if (filename == Name)
                                {
                                    file.Bytes = flverData;
                                }
                            }
                        }

                        var outBinderData = binder.Write();
                        project.ProjectFS.WriteFile(containerPath, outBinderData);
                        TaskLogs.AddLog($"[Smithbox:Model Editor] Saved {containerPath}.");
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to write {containerPath} during model save.", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
        }

        if (binderType is ResourceContainerType.BXF)
        {
            Memory<byte> bhd = new Memory<byte>();
            Memory<byte> bdt = new Memory<byte>();

            var targetBhdPath = containerPath;
            var targetBdtPath = containerPath.Replace("bhd", "bdt");

            var writePathBhd = Path.Combine(project.ProjectPath, targetBhdPath);
            var writePathBdt = Path.Combine(project.ProjectPath, targetBdtPath);

            try
            {
                bhd = (Memory<byte>)fs.ReadFile(targetBhdPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to write {targetBhdPath} during model save.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            try
            {
                bdt = (Memory<byte>)fs.ReadFile(targetBdtPath);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:Model Editor] Failed to write {targetBdtPath} during model save.", LogLevel.Error, Tasks.LogPriority.High, e);
            }

            if (bhd.Length != 0 && bdt.Length != 0)
            {
                if (project.ProjectType is ProjectType.DES
                    or ProjectType.DS1
                    or ProjectType.DS1R)
                {
                    var binder = BXF3.Read(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            if (filename == Name)
                            {
                                file.Bytes = flverData;
                            }
                        }
                    }

                    byte[] bhdData;
                    byte[] bdtData;

                    binder.Write(out bhdData, out bdtData);

                    project.ProjectFS.WriteFile(writePathBhd, bhdData);
                    project.ProjectFS.WriteFile(writePathBhd, bdtData);
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Saved {containerPath}.");
                }
                else
                {
                    var binder = BXF4.Read(bhd, bdt);
                    foreach (var file in binder.Files)
                    {
                        var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                        var filepath = file.Name.ToLower();

                        if (filepath.Contains(".flver") || filepath.Contains(".flv"))
                        {
                            if (filename == Name)
                            {
                                file.Bytes = flverData;
                            }
                        }
                    }

                    byte[] bhdData;
                    byte[] bdtData;

                    binder.Write(out bhdData, out bdtData);

                    project.ProjectFS.WriteFile(writePathBhd, bhdData);
                    project.ProjectFS.WriteFile(writePathBhd, bdtData);
                    TaskLogs.AddLog($"[Smithbox:Model Editor] Saved {containerPath}.");
                }
            }
        }
    }
}