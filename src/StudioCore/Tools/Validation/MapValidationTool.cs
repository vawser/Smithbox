using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools.Validation
{
    public static class MapValidationTool
    {

        public static bool HasFinished = false;

        public static bool TargetProject = false;

        public static List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();

        public static void ValidateMSB()
        {
            // Disable this since it ignores asserts if on.
            CFG.Current.System_IgnoreAsserts = false;
            HasFinished = false;

            var mapDir = $"{Smithbox.GameRoot}/map/mapstudio/";

            if (TargetProject)
            {
                mapDir = $"{Smithbox.ProjectRoot}/map/mapstudio/";
            }

            foreach (var entry in Directory.EnumerateFiles(mapDir))
            {
                if (entry.Contains(".msb.dcx"))
                {
                    var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                    ResourceDescriptor ad = MapLocator.GetMapMSB(name);
                    if (ad.AssetPath != null)
                    {
                        resMaps.Add(ad);
                    }
                }
            }

            if (Smithbox.ProjectType == ProjectType.DES)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSBD.Read(res.AssetPath);
                }
            }
            if (Smithbox.ProjectType == ProjectType.DS1 || Smithbox.ProjectType == ProjectType.DS1R)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSB1.Read(res.AssetPath);
                }
            }
            if (Smithbox.ProjectType == ProjectType.DS2 || Smithbox.ProjectType == ProjectType.DS2S)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSB2.Read(res.AssetPath);
                }
            }
            if (Smithbox.ProjectType == ProjectType.DS3)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSB3.Read(res.AssetPath);
                }
            }
            if (Smithbox.ProjectType == ProjectType.BB)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSBB.Read(res.AssetPath);
                }
            }
            if (Smithbox.ProjectType == ProjectType.SDT)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSBS.Read(res.AssetPath);
                }
            }
            if (Smithbox.ProjectType == ProjectType.ER)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSBE.Read(res.AssetPath);
                }
            }
            if (Smithbox.ProjectType == ProjectType.AC6)
            {
                foreach (var res in resMaps)
                {
                    var msb = MSB_AC6.Read(res.AssetPath);
                }
            }

            HasFinished = true;
        }
    }
}
