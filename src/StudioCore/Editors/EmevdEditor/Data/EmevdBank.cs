using Andre.IO.VFS;
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
        await Task.Delay(1);

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
        await Task.Delay(1);

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
            InfoBank = EMEDF.ReadFile(path);
        else
            return false;

        return true;
    }

    public async Task<bool> SetupEMEVD()
    {
        await Task.Delay(1);

        Scripts = new();

        foreach (var entry in Project.EmevdData.EmevdFiles.Entries)
        {
            Scripts.Add(entry, null);
        }

        return true;
    }

    public async Task<bool> LoadScript(FileDictionaryEntry fileEntry)
    {
        await Task.Delay(1);

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

                    var regulation = Project.FS.GetFileOrThrow("enc_regulation.bnd.dcx").GetData();
                    var binder = BND4.Read(regulation);
                    foreach (var entry in binder.Files)
                    {
                        if (!entry.Name.EndsWith("emevd"))
                            continue;

                        if (Path.GetFileNameWithoutExtension(entry.Name) == fileEntry.Filename)
                        {
                            var emevdData = entry.Bytes;
                            var emevd = EMEVD.Read(emevdData);

                            Scripts[key] = emevd;
                        }
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

                    var emevdData = TargetFS.ReadFileOrThrow(key.Path);
                    var emevd = EMEVD.Read(emevdData);

                    Scripts[key] = emevd;
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
        await Task.Delay(1);

        foreach(var entry in Scripts)
        {
            var dictEntry = entry.Key;
            await SaveScript(dictEntry);
        }

        return true;
    }

    public async Task<bool> SaveScript(FileDictionaryEntry fileEntry)
    {
        await Task.Delay(1);

        if (Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var targetScript = Scripts.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

            var regulation = Project.FS.GetFileOrThrow("enc_regulation.bnd.dcx").GetData();
            var binder = BND4.Read(regulation);
            foreach (var entry in binder.Files)
            {
                if (!entry.Name.EndsWith("emevd"))
                    continue;

                if (Path.GetFileNameWithoutExtension(entry.Name) == fileEntry.Filename)
                {
                    entry.Bytes = targetScript.Value.Write();
                }
            }

            var newRegulation = binder.Write();

            Project.ProjectFS.WriteFile("enc_regulation.bnd.dcx", newRegulation);
        }
        else
        {
            if (Scripts.Any(e => e.Key.Filename == fileEntry.Filename && e.Value != null))
            {
                var scriptEntry = Scripts.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);
                var fileInfo = scriptEntry.Key;

                var emevd = scriptEntry.Value;
                var bytes = emevd.Write();

                Project.ProjectFS.WriteFile(fileInfo.Path, bytes);

                //var filePath = @$"{Project.ProjectPath}\{fileInfo.Path}";
                //File.WriteAllBytes(filePath, bytes);
            }
        }

        return true;
    }
}
