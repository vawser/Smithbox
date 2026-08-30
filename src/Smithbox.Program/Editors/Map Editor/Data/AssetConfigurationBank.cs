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

            if (!fs.FileExists(bhdPath) || !fs.FileExists(bdtPath))
                continue;

            var bdtFile = (Memory<byte>)fs.ReadFile(bdtPath);
            var bhdFile = (Memory<byte>)fs.ReadFile(bhdPath);

            if(View.Project.VFS.ProjectFS.FileExists(bhdPath) && View.Project.VFS.ProjectFS.FileExists(bdtPath))
            {
                bdtFile = (Memory<byte>)View.Project.VFS.ProjectFS.ReadFile(bdtPath);
                bhdFile = (Memory<byte>)View.Project.VFS.ProjectFS.ReadFile(bhdPath);
            }

            try
            {
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

                if (!fs.FileExists(bhdPath) || !fs.FileExists(bdtPath))
                    continue;

                var bdtFile = (Memory<byte>)fs.ReadFile(bdtPath);
                var bhdFile = (Memory<byte>)fs.ReadFile(bhdPath);

                if (View.Project.VFS.ProjectFS.FileExists(bhdPath) && View.Project.VFS.ProjectFS.FileExists(bdtPath))
                {
                    bdtFile = (Memory<byte>)View.Project.VFS.ProjectFS.ReadFile(bdtPath);
                    bhdFile = (Memory<byte>)View.Project.VFS.ProjectFS.ReadFile(bhdPath);
                }

                var applyEdit = false;

                try
                {
                    using var packedBinder = BXF4.Read(bhdFile, bdtFile);
                    foreach(var binderFile in packedBinder.Files)
                    {
                        if (!binderFile.Name.EndsWith(".acb"))
                            continue;

                        if (map.AssetConfigurationParent.WrappedObject.ToString() == entry.Filename)
                        {
                            try
                            {
                                var acbData = ACB.Read(binderFile.Bytes);

                                acbData.Assets.Clear();

                                foreach (var assetEntry in map.AssetConfigurationParent.Children)
                                {
                                    var curEntry = (ACB.Asset)assetEntry.WrappedObject;

                                    acbData.Assets.Add(curEntry);
                                }

                                var fileOutput = acbData.Write();

                                if (!BytePerfectHelper.Md5Equal(binderFile.Bytes.Span, fileOutput))
                                {
                                    applyEdit = true;
                                }

                                if (applyEdit)
                                {
                                    binderFile.Bytes = fileOutput;
                                }
                            }
                            catch (Exception e)
                            {
                                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_ACB", binderFile.Name), e);
                            }
                        }
                    }

                    if (applyEdit)
                    {
                        packedBinder.Write(out var writtenBhdBytes, out var writtenBdtBytes);

                        Project.VFS.ProjectFS.WriteFile(bhdPath, writtenBhdBytes);
                        Project.VFS.ProjectFS.WriteFile(bdtPath, writtenBdtBytes);

                        Smithbox.Log(this, LOC.Get("MAP_Data_Write_ACB_Log", bhdPath));
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_MAPBND", bhdPath), e);
                }
            }
        }
    }
}
