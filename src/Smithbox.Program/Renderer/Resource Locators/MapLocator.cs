using StudioCore.Application;
using System;

namespace StudioCore.Renderer;

public static class MapLocator
{
    /// <summary>
    /// Gets the adjusted map ID that contains all the map assets
    /// </summary>
    /// <param name="mapid">The msb map ID to adjust</param>
    /// <returns>The map ID for the purpose of asset storage</returns>
    public static string GetAssetMapID(ProjectEntry project, string mapid)
    {
        if (project.ProjectType is ProjectType.DES or ProjectType.ER or ProjectType.NR or ProjectType.AC6)
        {
            return mapid;
        }
        else if (project.ProjectType is ProjectType.DS1R)
        {
            if (mapid.StartsWith("m99"))
            {
                // DSR m99 maps contain their own assets
                return mapid;
            }
        }
        else if (project.ProjectType is ProjectType.BB)
        {
            if (mapid.StartsWith("m29"))
            {
                // Special case for chalice dungeon assets
                return "m29_00_00_00";
            }
        }
        else if (project.ProjectType is ProjectType.ACFA)
        {
            return mapid[..4];
        }
        else if (project.ProjectType is ProjectType.ACV)
        {
            return mapid[..5];
        }
        else if (project.ProjectType is ProjectType.ACVD)
        {
            if (mapid.Length == 12 && mapid.StartsWith("ch"))
            {
                return string.Concat(mapid.AsSpan(7, 4), "0");
            }

            return mapid[..4] + '0';
        }

        // Default
        return mapid.Substring(0, 6) + "_00_00";
    }

}
