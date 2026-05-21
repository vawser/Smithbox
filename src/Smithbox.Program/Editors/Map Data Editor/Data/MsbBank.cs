using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class MsbBank : IDisposable
{
    public ProjectEntry Project;

    public VirtualFileSystem TargetFS = EmptyVirtualFileSystem.Instance;

    public string Name;

    public Dictionary<FileDictionaryEntry, IMsb> Maps = new();

    public MsbBank(string name, ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        Name = name;
        TargetFS = targetFs;
    }

    public async Task<bool> Setup()
    {
        foreach(var entry in Project.Locator.MapFiles.Entries)
        {
            if (!Maps.ContainsKey(entry))
            {
                Maps.Add(entry, null);
            }
        }

        return true;
    }

    public async Task<bool> LoadMap(FileDictionaryEntry fileEntry)
    {
        IMsb msb;

        if(CFG.Current.MapDataEditor_CacheLoadedMaps)
        {
            if (Maps.ContainsKey(fileEntry))
            {
                if(Maps[fileEntry] != null)
                {
                    return true;
                }
            }
        }

        try
        {
            var mapData = TargetFS.ReadFileOrThrow(fileEntry.Path);

            switch (Project.Descriptor.ProjectType)
            {
                case ProjectType.DES:
                    try
                    {
                        msb = MSBD.Read(mapData);

                        if (Maps.ContainsKey(fileEntry))
                        {
                            Maps[fileEntry] = msb;
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as MSB", e);
                        return false;
                    }
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    try
                    {
                        msb = MSB1.Read(mapData);

                        if (Maps.ContainsKey(fileEntry))
                        {
                            Maps[fileEntry] = msb;
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as MSB", e);
                        return false;
                    }
                    break;
                case ProjectType.DS2:
                case ProjectType.DS2S:
                    try
                    {
                        msb = MSB2.Read(mapData);

                        if (Maps.ContainsKey(fileEntry))
                        {
                            Maps[fileEntry] = msb;
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as MSB", e);
                        return false;
                    }
                    break;
                case ProjectType.DS3:
                    try
                    {
                        msb = MSB3.Read(mapData);

                        if (Maps.ContainsKey(fileEntry))
                        {
                            Maps[fileEntry] = msb;
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as MSB", e);
                        return false;
                    }
                    break;
                case ProjectType.BB:
                    try
                    {
                        msb = MSBB.Read(mapData);

                        if (Maps.ContainsKey(fileEntry))
                        {
                            Maps[fileEntry] = msb;
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as MSB", e);
                        return false;
                    }
                    break;
                case ProjectType.SDT:
                    try
                    {
                        msb = MSBS.Read(mapData);

                        if (Maps.ContainsKey(fileEntry))
                        {
                            Maps[fileEntry] = msb;
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as MSB", e);
                        return false;
                    }
                    break;
                case ProjectType.ER:
                    try
                    {
                        msb = MSBE.Read(mapData);

                        if (Maps.ContainsKey(fileEntry))
                        {
                            Maps[fileEntry] = msb;
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as MSB", e);
                        return false;
                    }
                    break;
                case ProjectType.AC6:
                    try
                    {
                        msb = MSB_AC6.Read(mapData);

                        if (Maps.ContainsKey(fileEntry))
                        {
                            Maps[fileEntry] = msb;
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as MSB", e);
                        return false;
                    }
                    break;
                case ProjectType.NR:
                    try
                    {
                        msb = MSB_NR.Read(mapData);

                        if (Maps.ContainsKey(fileEntry))
                        {
                            Maps[fileEntry] = msb;
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} as MSB", e);

                        return false;
                    }
                    break;
                default: break;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[Map Data Editor] Failed to read {fileEntry.Path} from VFS", e);
            return false;
        }

        return true;
    }

    public async Task<bool> SaveMap(MapDataEditorView view, FileDictionaryEntry fileEntry)
    {
        if (Maps.ContainsKey(fileEntry))
        {
            var msb = Maps[fileEntry];

            try
            {
                var mapData = msb.Write();

                Project.VFS.ProjectFS.WriteFile(fileEntry.Path, mapData);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Data Editor] Failed to write {fileEntry.Path}", e);

                return false;
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        Maps.Clear();
        Maps = null;
    }
    #endregion
}
