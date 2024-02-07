
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.AssetLocator;
public static class TextAssetLocator
{
    /// <summary>
    /// Get folders with msgbnds used in-game
    /// </summary>
    /// <returns>Dictionary with language name and path</returns>
    public static Dictionary<string, string> GetMsgLanguages()
    {
        Dictionary<string, string> dict = new();
        List<string> folders = new();
        try
        {
            if (Project.Type == ProjectType.DES)
            {
                folders = Directory.GetDirectories(Project.GameRootDirectory + @"\msg").ToList();
                // Japanese uses root directory
                if (File.Exists(Project.GameRootDirectory + @"\msg\menu.msgbnd.dcx") ||
                    File.Exists(Project.GameRootDirectory + @"\msg\item.msgbnd.dcx"))
                    dict.Add("Japanese", "");
            }
            else if (Project.Type == ProjectType.DS2S)
                folders = Directory.GetDirectories(Project.GameRootDirectory + @"\menu\text").ToList();
            else
                // Exclude folders that don't have typical msgbnds
                folders = Directory.GetDirectories(Project.GameRootDirectory + @"\msg")
                    .Where(x => !"common,as,eu,jp,na,uk,japanese".Contains(x.Split("\\").Last())).ToList();

            foreach (var path in folders)
                dict.Add(path.Split("\\").Last(), path);
        }
        catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
        {
        }

        return dict;
    }

    /// <summary>
    /// Get path of item.msgbnd (english by default)
    /// </summary>
    public static AssetDescription GetItemMsgbnd(string langFolder, bool writemode = false)
    {
        return GetMsgbnd("item", langFolder, writemode);
    }

    /// <summary>
    /// Get path of menu.msgbnd (english by default)
    /// </summary>
    public static AssetDescription GetMenuMsgbnd(string langFolder, bool writemode = false)
    {
        return GetMsgbnd("menu", langFolder, writemode);
    }

    public static AssetDescription GetMsgbnd(string msgBndType, string langFolder, bool writemode = false)
    {
        AssetDescription ad = new();
        var path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
        if (Project.Type == ProjectType.DES)
        {
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
            // Demon's Souls has msgbnds directly in the msg folder
            if (!File.Exists($@"{Project.GameRootDirectory}\{path}"))
                path = $@"msg\{msgBndType}.msgbnd.dcx";
        }
        else if (Project.Type == ProjectType.DS1)
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd";
        else if (Project.Type == ProjectType.DS1R)
            path = $@"msg\{langFolder}\{msgBndType}.msgbnd.dcx";
        else if (Project.Type == ProjectType.DS2S)
        {
            // DS2 does not have an msgbnd but loose fmg files instead
            path = $@"menu\text\{langFolder}";
            AssetDescription ad2 = new();
            ad2.AssetPath = writemode ? path : $@"{Project.GameRootDirectory}\{path}";
            //TODO: doesn't support project files
            return ad2;
        }
        else if (Project.Type == ProjectType.DS3)
            path = $@"msg\{langFolder}\{msgBndType}_dlc2.msgbnd.dcx";

        if (writemode)
        {
            ad.AssetPath = path;
            return ad;
        }

        if (Project.GameModDirectory != null && File.Exists($@"{Project.GameModDirectory}\{path}") ||
            writemode && Project.GameModDirectory != null)
            ad.AssetPath = $@"{Project.GameModDirectory}\{path}";
        else if (File.Exists($@"{Project.GameRootDirectory}\{path}"))
            ad.AssetPath = $@"{Project.GameRootDirectory}\{path}";

        return ad;
    }
}
