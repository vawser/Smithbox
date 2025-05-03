using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools.Validation;

public class MapValidationTool
{
    private Smithbox BaseEditor;

    public MapValidationTool(Smithbox editor)
    {
        BaseEditor = editor;
    }

    public bool HasFinished = false;

    public bool TargetProject = false;

    public List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();

    public void ValidateMSB()
    {
        var curProject = BaseEditor.ProjectManager.SelectedProject;

        // Disable this since it ignores asserts if on.
        CFG.Current.System_IgnoreAsserts = false;
        HasFinished = false;

        var mapDir = $"{curProject.DataPath}/map/mapstudio/";

        if (TargetProject)
        {
            mapDir = $"{curProject.ProjectPath}/map/mapstudio/";
        }

        foreach (var entry in Directory.EnumerateFiles(mapDir))
        {
            if (entry.Contains(".msb.dcx"))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                ResourceDescriptor ad = MapLocator.GetMapMSB(curProject, name);
                if (ad.AssetPath != null)
                {
                    resMaps.Add(ad);
                }
            }
        }

        if (curProject.ProjectType == ProjectType.DES)
        {
            foreach (var res in resMaps)
            {
                var msb = MSBD.Read(res.AssetPath);
            }
        }
        if (curProject.ProjectType == ProjectType.DS1 || curProject.ProjectType == ProjectType.DS1R)
        {
            foreach (var res in resMaps)
            {
                var msb = MSB1.Read(res.AssetPath);
            }
        }
        if (curProject.ProjectType == ProjectType.DS2 || curProject.ProjectType == ProjectType.DS2S)
        {
            foreach (var res in resMaps)
            {
                var msb = MSB2.Read(res.AssetPath);
            }
        }
        if (curProject.ProjectType == ProjectType.DS3)
        {
            foreach (var res in resMaps)
            {
                var msb = MSB3.Read(res.AssetPath);
            }
        }
        if (curProject.ProjectType == ProjectType.BB)
        {
            foreach (var res in resMaps)
            {
                var msb = MSBB.Read(res.AssetPath);
            }
        }
        if (curProject.ProjectType == ProjectType.SDT)
        {
            foreach (var res in resMaps)
            {
                var msb = MSBS.Read(res.AssetPath);
            }
        }
        if (curProject.ProjectType == ProjectType.ER)
        {
            foreach (var res in resMaps)
            {
                var msb = MSBE.Read(res.AssetPath);
            }
        }
        if (curProject.ProjectType == ProjectType.AC6)
        {
            foreach (var res in resMaps)
            {
                var msb = MSB_AC6.Read(res.AssetPath);
            }
        }

        HasFinished = true;
    }
}
