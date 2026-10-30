using SoulsFormats;
using StudioCore.Logger;

namespace StudioCore.Editors.MapEditor;

public class LightAtlasBank
{
    public ProjectEntry Project;

    public Dictionary<string, BTAB> Files = new();

    public LightAtlasBank(ProjectEntry project)
    {
        Project = project;

        Setup();
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB or ProjectType.DS2 or ProjectType.DS2S)
            return true;

        return false;
    }

    public void Setup()
    {
        if (!CanUse())
            return;

        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            foreach (var entry in Project.Locator.LightAtlasFiles.Entries)
            {
                var bhdPath = entry.Path;
                var bdtPath = $"{bhdPath}".Replace(".gibhd", ".gibdt");

                if (fs.FileExists(bdtPath) && fs.FileExists(bdtPath))
                {
                    try
                    {
                        var bdtFile = (Memory<byte>)fs.ReadFile(bdtPath);
                        var bhdFile = (Memory<byte>)fs.ReadFile(bhdPath);

                        using var bdt = BXF4.Read(bhdFile, bdtFile);
                        BinderFile file = bdt.Files.Find(f => f.Name.EndsWith("atlasinfo.btab.dcx"));

                        if (file != null)
                        {
                            var btabData = BTAB.Read(file.Bytes);

                            Files.Add(entry.Filename, btabData);
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_GI", bhdPath), e);
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_GI_File", bhdPath));
                }
            }
        }
        else
        {
            foreach (var entry in Project.Locator.LightAtlasFiles.Entries)
            {
                if (fs.FileExists(entry.Path))
                {
                    var fileData = fs.ReadFile(entry.Path);

                    if (fileData != null)
                    {
                        try
                        {
                            var btabData = BTAB.Read(fileData.Value);

                            Files.Add(entry.Filename, btabData);
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_BTAB", entry.Path), e);
                        }
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_BTAB", entry.Path));
                }
            }
        }
    }

    public void LoadBTAB(MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Files)
        {
            if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                var worldBlock = map.Name.Substring(1);

                if (entry.Key.Contains(worldBlock))
                {
                    var btab = entry.Value;

                    if (btab != null)
                    {
                        map.LoadBTAB(entry.Key, btab);
                    }
                }
            }
            else
            {
                // File will be: m30_00_00_00_0001, so we match loosely
                if (entry.Key.Contains(map.Name))
                {
                    var btab = entry.Value;

                    if (btab != null)
                    {
                        map.LoadBTAB(entry.Key, btab);
                    }
                }
            }
        }
    }

    public bool SaveBTAB(MapContainer map)
    {
        var success = true;

        if (!CanUse())
            return success;

        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            foreach (var entry in Project.Locator.LightAtlasFiles.Entries)
            {
                var worldBlock = map.Name.Substring(1);

                if (!entry.Filename.Contains(worldBlock))
                    continue;

                var bhdPath = entry.Path;
                var bdtPath = $"{bhdPath}".Replace(".gibhd", ".gibdt");

                if (fs.FileExists(bdtPath) && fs.FileExists(bhdPath))
                {
                    try
                    {
                        var bdtFile = (Memory<byte>)fs.ReadFile(bdtPath);
                        var bhdFile = (Memory<byte>)fs.ReadFile(bhdPath);

                        using var packedBinder = BXF4.Read(bhdFile, bdtFile);
                        BinderFile file = packedBinder.Files.Find(f => f.Name.EndsWith("atlasinfo.btab.dcx"));

                        if (file != null)
                        {
                            var applyEdit = false;

                            foreach (var parent in map.LightAtlasParents)
                            {
                                if (parent.WrappedObject.ToString() == entry.Filename)
                                {
                                    var btabData = BTAB.Read(file.Bytes);

                                    btabData.Entries.Clear();

                                    foreach (var btabEntry in parent.Children)
                                    {
                                        var curEntry = (BTAB.Entry)btabEntry.WrappedObject;

                                        btabData.Entries.Add(curEntry);
                                    }

                                    var fileOutput = btabData.Write();

                                    if (!BytePerfectHelper.Md5Equal(file.Bytes.Span, fileOutput))
                                    {
                                        applyEdit = true;
                                    }

                                    if (applyEdit)
                                    {
                                        file.Bytes = fileOutput;
                                    }

                                    UpdateCachedEntries(entry.Filename, btabData);
                                }
                            }

                            if (applyEdit)
                            {
                                packedBinder.Write(out var writtenBhdBytes, out var writtenBdtBytes);

                                Project.VFS.ProjectFS.WriteFile(bhdPath, writtenBhdBytes);
                                Project.VFS.ProjectFS.WriteFile(bdtPath, writtenBdtBytes);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_GI", bhdPath), e);
                        success = false;
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_GI_File", bhdPath));
                    success = false;
                }
            }
        }
        else
        {
            foreach (var entry in Project.Locator.LightAtlasFiles.Entries)
            {
                // File will be: m30_00_00_00_0001, so we match loosely
                if (!entry.Filename.Contains(map.Name))
                    continue;

                if (fs.FileExists(entry.Path))
                {
                    var fileData = fs.ReadFile(entry.Path);

                    if (fileData != null)
                    {
                        var applyEdit = false;

                        try
                        {
                            foreach (var parent in map.LightAtlasParents)
                            {
                                if (parent.WrappedObject.ToString() == entry.Filename)
                                {
                                    var btabData = BTAB.Read(fileData.Value);

                                    btabData.Entries.Clear();

                                    foreach (var btabEntry in parent.Children)
                                    {
                                        var curEntry = (BTAB.Entry)btabEntry.WrappedObject;

                                        btabData.Entries.Add(curEntry);
                                    }

                                    var fileOutput = btabData.Write();

                                    if (!BytePerfectHelper.Md5Equal(fileData.Value.Span, fileOutput))
                                    {
                                        applyEdit = true;
                                    }

                                    if (applyEdit)
                                    {
                                        Project.VFS.ProjectFS.WriteFile(entry.Path, fileOutput);
                                    }

                                    UpdateCachedEntries(entry.Filename, btabData);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_BTAB", entry.Path), e);
                            success = false;
                        }
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_BTAB", entry.Path));
                    success = false;
                }
            }
        }

        return success;
    }

    // Maps are loaded from Files, so added and removed entries must be reflected here to survive a map reload
    private void UpdateCachedEntries(string filename, BTAB saved)
    {
        if (Files.TryGetValue(filename, out var cached))
        {
            cached.Entries = saved.Entries;
        }
    }
}
