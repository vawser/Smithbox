using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using static SoulsFormats.NVA;


namespace StudioCore.Editors.MapEditor;

public class LightProbeBank
{
    public MapEditorView View;
    public ProjectEntry Project;

    public Dictionary<string, BTPB> Files = new();

    public LightProbeBank(MapEditorView view, ProjectEntry project)
    {
        View = view;
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

        var fs = View.Project.Handler.MapData.PrimaryBank.TargetFS;

        if (View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            foreach (var entry in Project.Locator.LightProbeFiles.Entries)
            {
                var bhdPath = entry.Path;
                var bdtPath = $"{bhdPath}".Replace(".gibhd", ".gibdt");

                if (fs.FileExists(bdtPath) && fs.FileExists(bhdPath))
                {
                    try
                    {
                        var bdtFile = (Memory<byte>)fs.ReadFile(bdtPath);
                        var bhdFile = (Memory<byte>)fs.ReadFile(bhdPath);

                        using var bdt = BXF4.Read(bhdFile, bdtFile);
                        BinderFile file = bdt.Files.Find(f => f.Name.EndsWith("pointcloud.btpb.dcx"));

                        if (file != null)
                        {
                            var btpbData = BTPB.Read(file.Bytes);

                            Files.Add(entry.Filename, btpbData);
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
            foreach (var entry in Project.Locator.LightProbeFiles.Entries)
            {
                if (fs.FileExists(entry.Path))
                {
                    var fileData = fs.ReadFile(entry.Path);

                    if (fileData != null)
                    {
                        try
                        {
                            var btpbData = BTPB.Read(fileData.Value);

                            Files.Add(entry.Filename, btpbData);
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_BTPB", entry.Path), e);
                        }
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_BTPB", entry.Path));
                }
            }
        }
    }

    public void LoadBTPB(MapContainer map)
    {
        if (!CanUse())
            return;

        foreach (var entry in Files)
        {
            if (View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                var worldBlock = map.Name.Substring(1);

                if (entry.Key.Contains(worldBlock))
                {
                    var btpb = entry.Value;

                    if (btpb != null)
                    {
                        map.LoadBTPB(entry.Key, btpb);
                    }
                }
            }
            else
            {
                // File will be: m30_00_00_00_0001, so we match loosely
                if (entry.Key.Contains(map.Name))
                {
                    var btpb = entry.Value;

                    if (btpb != null)
                    {
                        map.LoadBTPB(entry.Key, btpb);
                    }
                }
            }
        }
    }

    public void SaveBTPB(MapContainer map)
    {
        if (!CanUse())
            return;

        var fs = View.Project.Handler.MapData.PrimaryBank.TargetFS;

        if (View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            foreach (var entry in Project.Locator.LightProbeFiles.Entries)
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

                        using var bdt = BXF4.Read(bhdFile, bdtFile);
                        BinderFile file = bdt.Files.Find(f => f.Name.EndsWith("pointcloud.btpb.dcx"));

                        if (file != null)
                        {
                            var applyEdit = false;

                            foreach (var parent in map.LightProbeParents)
                            {
                                // Match the root object name to the filename
                                if (parent.WrappedObject.ToString() == entry.Filename)
                                {
                                    try
                                    {
                                        var btpbData = BTPB.Read(file.Bytes);

                                        // Clear groups and then re-fill from the map container hierarchy
                                        btpbData.Groups.Clear();

                                        foreach (var btpbEntry in parent.Children)
                                        {
                                            var group = (BTPB.Group)btpbEntry.WrappedObject;

                                            btpbData.Groups.Add(group);
                                        }

                                        var fileOutput = btpbData.Write();

                                        if (!BytePerfectHelper.Md5Equal(file.Bytes.Span, fileOutput))
                                        {
                                            applyEdit = true;
                                        }

                                        if (applyEdit)
                                        {
                                            file.Bytes = fileOutput;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_BTPB", entry.Filename), e);
                                    }
                                }
                            }

                            if (applyEdit)
                            {
                                Project.VFS.ProjectFS.WriteFile(bhdPath, bhdFile.ToArray());
                                Project.VFS.ProjectFS.WriteFile(bdtPath, bdtFile.ToArray());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_GI", bhdPath), e);
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
            foreach (var entry in Project.Locator.LightProbeFiles.Entries)
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
                            foreach (var parent in map.LightProbeParents)
                            {
                                // Match the root object name to the filename
                                if (parent.WrappedObject.ToString() == entry.Filename)
                                {
                                    try
                                    {
                                        var btpbData = BTPB.Read(fileData.Value);

                                        // Clear groups and then re-fill from the map container hierarchy
                                        btpbData.Groups.Clear();

                                        foreach (var btpbEntry in parent.Children)
                                        {
                                            var group = (BTPB.Group)btpbEntry.WrappedObject;

                                            btpbData.Groups.Add(group);
                                        }

                                        var fileOutput = btpbData.Write();

                                        if (!BytePerfectHelper.Md5Equal(fileData.Value.Span, fileOutput))
                                        {
                                            applyEdit = true;
                                        }

                                        if (applyEdit)
                                        {
                                            Project.VFS.ProjectFS.WriteFile(entry.Path, fileOutput);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_BTPB", entry.Path), e);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_BTPB", entry.Path), e);
                        }
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_BTPB", entry.Path));
                }
            }
        }
    }
}
