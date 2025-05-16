using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Handles the load and save processes for the EMEVD files and their containers, 
/// as well as applying the EMEDF templates to the EMEVD Files.
/// </summary>
public class EmevdBank
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, EMEVD> Scripts = new();

    public EMEDF InfoBank { get; private set; } = new();

    public bool IsSupported = false;

    public EmevdBank(string name, Smithbox baseEditor, ProjectEntry project, VirtualFileSystem targetFs)
    {
        BaseEditor = baseEditor;
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        // EMEDF
        Task<bool> emedfTask = LoadEMEDF();
        bool emedfTaskResult = await emedfTask;

        // EMEVD
        Task<bool> emevdTask = SetupEMEVD();
        bool emevdTaskResult = await emevdTask;

        return true;
    }

    public async Task<bool> LoadEMEDF()
    {
        await Task.Yield();

        IsSupported = false;

        var path = "";
        switch(Project.ProjectType)
        {
            case ProjectType.DS1:
            case ProjectType.DS1R:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds1-common.emedf.json";
                break;
            case ProjectType.DS2:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds2-common.emedf.json";
                break;
            case ProjectType.DS2S:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds2scholar-common.emedf.json";
                break;
            case ProjectType.BB:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//bb-common.emedf.json";
                break;
            case ProjectType.DS3:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ds3-common.emedf.json";
                break;
            case ProjectType.SDT:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//sekiro-common.emedf.json";
                break;
            case ProjectType.ER:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//er-common.emedf.json";
                break;
            case ProjectType.AC6:
                IsSupported = true;
                path = $"{AppDomain.CurrentDomain.BaseDirectory}//Assets//EMEVD//ac6-common.emedf.json";
                break;
            default: break;
        }

        if (IsSupported)
        {
            try
            {
                InfoBank = EMEDF.ReadFile(path);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read EMEDF at: {path}", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    public async Task<bool> SetupEMEVD()
    {
        await Task.Yield();

        Scripts = new();

        foreach (var entry in Project.EmevdData.EmevdFiles.Entries)
        {
            Scripts.Add(entry, null);
        }

        return true;
    }

    public async Task<bool> LoadScript(FileDictionaryEntry fileEntry)
    {
        await Task.Yield();

        // If already loaded, just ignore
        if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
        {
            return true;
        }

        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename))
            {
                var scriptEntry = Scripts.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

                if (scriptEntry.Key != null)
                {
                    var key = scriptEntry.Key;

                    try
                    {
                        var regulation = Project.FS.GetFileOrThrow("enc_regulation.bnd.dcx").GetData();

                        try
                        {
                            var binder = BND4.Read(regulation);
                            foreach (var entry in binder.Files)
                            {
                                if (!entry.Name.EndsWith("emevd"))
                                    continue;

                                if (Path.GetFileNameWithoutExtension(entry.Name) == fileEntry.Filename)
                                {
                                    var emevdData = entry.Bytes;

                                    try
                                    {
                                        var emevd = EMEVD.Read(emevdData);

                                        Scripts[key] = emevd;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read {entry.Name} as EMEVD", LogLevel.Error, Tasks.LogPriority.High, e);
                                        return false;
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read enc_regulation as BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read enc_regulation.bnd.dcx from VFS", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
            }
        }
        else
        {
            if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename))
            {
                var scriptEntry = Scripts.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

                if (scriptEntry.Key != null)
                {
                    var key = scriptEntry.Key;

                    try
                    {
                        var emevdData = TargetFS.ReadFileOrThrow(key.Path);

                        try
                        {
                            var emevd = EMEVD.Read(emevdData);

                            Scripts[key] = emevd;
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read {key.Filename} as EMEVD", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read {key.Filename} from VFS", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public async Task<bool> SaveAllScripts()
    {
        await Task.Yield();

        foreach (var entry in Scripts)
        {
            await SaveScript(entry.Key, entry.Value);
        }

        return true;
    }

    public async Task<bool> SaveScript(FileDictionaryEntry fileEntry, EMEVD curScript)
    {
        await Task.Yield();

        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            try
            {
                var regulation = Project.FS.GetFileOrThrow("enc_regulation.bnd.dcx").GetData();

                try
                {
                    var binder = BND4.Read(regulation);
                    foreach (var entry in binder.Files)
                    {
                        if (!entry.Name.EndsWith("emevd"))
                            continue;

                        if (Path.GetFileNameWithoutExtension(entry.Name) == fileEntry.Filename)
                        {
                            try
                            {
                                entry.Bytes = curScript.Write();
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to write {entry.Name} as EMEVD", LogLevel.Error, Tasks.LogPriority.High, e);
                                return false;
                            }
                        }
                    }

                    try
                    {
                        var newRegulation = binder.Write();

                        try
                        {
                            Project.ProjectFS.WriteFile("enc_regulation.bnd.dcx", newRegulation);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to write enc_regulation.bnd.dcx", LogLevel.Error, Tasks.LogPriority.High, e);
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to write enc_regulation to BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read enc_regulation as BND4", LogLevel.Error, Tasks.LogPriority.High, e);
                    return false;
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to read enc_regulation.bnd.dcx from VFS", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }
        else
        {
            if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
            {
                var emevd = curScript;

                try
                {
                    var bytes = emevd.Write();

                    try
                    {
                        Project.ProjectFS.WriteFile(fileEntry.Path, bytes);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to write {fileEntry.Filename}.emevd.dcx", LogLevel.Error, Tasks.LogPriority.High, e);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to write {fileEntry.Filename} as EMEVD", LogLevel.Error, Tasks.LogPriority.High, e);
                    return false;
                }
            }
        }

        return true;
    }
}
