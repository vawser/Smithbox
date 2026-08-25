using SoulsFormats;
using StudioCore.Logger;

namespace StudioCore.Editors.MapEditor;

public class AssetConfigurationBank
{
    public MapEditorView View;
    public ProjectEntry Project;

    public Dictionary<string, ACB> Files = new();

    public AssetConfigurationBank(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Setup();
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            return true;

        return false;
    }

    public void Setup()
    {
        if (!CanUse())
            return;

        var fs = View.Project.Handler.MapData.PrimaryBank.TargetFS;

        // ACB in mapbhd (the gibhd one isn't used by the game)
        foreach (var entry in Project.Locator.MapPieceFiles.Entries)
        {
            var bhdPath = entry.Path;
            var bdtPath = $"{bhdPath}".Replace(".mapbhd", ".mapbdt");

            if (fs.FileExists(bhdPath) && fs.FileExists(bhdPath))
            {
                try
                {
                    var bdtFile = (Memory<byte>)fs.ReadFile(bdtPath);
                    var bhdFile = (Memory<byte>)fs.ReadFile(bhdPath);

                    using var bdt = BXF4.Read(bhdFile, bdtFile);
                    BinderFile file = bdt.Files.Find(f => f.Name.EndsWith(".acb"));

                    if (file != null)
                    {
                        try
                        {
                            var acbData = ACB.Read(file.Bytes);

                            Files.Add(entry.Filename, acbData);
                        }
                        catch (Exception ex)
                        {
                            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_ACB", bhdPath), ex);
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_MAPBND", bhdPath), e);
                }
            }
            else
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_MAPBND", bhdPath));
            }
        }
    }

    public void LoadACB(MapContainer map)
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
                    var acb = entry.Value;

                    if (acb != null)
                    {
                        map.LoadACB(entry.Key, acb);
                    }
                }
            }
        }
    }

    public void SaveACB(MapContainer map)
    {
        if (!CanUse())
            return;

        var fs = View.Project.Handler.MapData.PrimaryBank.TargetFS;

        if (View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            foreach (var entry in Project.Locator.MapPieceFiles.Entries)
            {
                var worldBlock = map.Name.Substring(1);

                if (!entry.Filename.Contains(worldBlock))
                    continue;

                var bhdPath = entry.Path;
                var bdtPath = $"{bhdPath}".Replace(".mapbhd", ".mapbdt");

                if (fs.FileExists(bdtPath) && fs.FileExists(bhdPath))
                {
                    try
                    {
                        var bdtFile = (Memory<byte>)fs.ReadFile(bdtPath);
                        var bhdFile = (Memory<byte>)fs.ReadFile(bhdPath);

                        using var bdt = BXF4.Read(bhdFile, bdtFile);
                        BinderFile file = bdt.Files.Find(f => f.Name.EndsWith(".acb"));

                        if (file != null)
                        {
                            var applyEdit = false;

                            if (map.AssetConfigurationParent.WrappedObject.ToString() == entry.Filename)
                            {
                                try
                                {
                                    var acbData = ACB.Read(file.Bytes);

                                    acbData.Assets.Clear();

                                    foreach (var assetEntry in map.AssetConfigurationParent.Children)
                                    {
                                        var curEntry = (ACB.Asset)assetEntry.WrappedObject;

                                        acbData.Assets.Add(curEntry);
                                    }

                                    var fileOutput = acbData.Write();

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
                                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_ACB", file.Name), e);
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
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_MAPBND", bhdPath), e);
                    }
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_MAPBND", bhdPath));
                }
            }
        }
    }
}
