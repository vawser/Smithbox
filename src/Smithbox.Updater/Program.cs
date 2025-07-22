using System.Diagnostics;
using System.IO;

namespace Smithbox.Updater;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Invalid usage.");
            Console.ReadKey();
            return;
        }

        string rootDir = args[0];

        if(!Directory.Exists(rootDir))
        {
            Console.WriteLine("Invalid usage.");
            Console.ReadKey();
            return;
        }

        string updateDir = $"{rootDir}/_update";
        string smithboxExe = $"{rootDir}/Smithbox.exe";

        // Wait for main app to exit
        Thread.Sleep(1000);
        while (IsFileLocked(smithboxExe))
        {
            Thread.Sleep(500);
        }

        var excluded_folders = new List<string>()
        {
            "_update",
            "Crash Logs"
        };

        // Delete Folders
        foreach(var folder in Directory.EnumerateDirectories(rootDir))
        {
            string lastDir = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

            if (!excluded_folders.Contains(lastDir))
            {
                Console.WriteLine(folder);
                Directory.Delete(folder, true);
            }
        }
        // Delete Files
        foreach (var file in Directory.EnumerateFiles(rootDir))
        {
            var filename = Path.GetFileName(file);

            Console.WriteLine(file);
            File.Delete(file);
        }

        if (!Directory.Exists(updateDir))
        {
            Console.WriteLine($"Update directory does not exist: {updateDir}");
            return;
        }

        CopyAll(new DirectoryInfo(updateDir), new DirectoryInfo(rootDir));
        Console.WriteLine("Update files copied successfully.");

        // Remove _update folder
        Directory.Delete(updateDir, recursive: true);

        //// Relaunch app
        Process.Start(smithboxExe);
    }

    static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        // Create all directories in target
        foreach (DirectoryInfo dir in source.GetDirectories())
        {
            DirectoryInfo targetSubDir = target.CreateSubdirectory(dir.Name);
            CopyAll(dir, targetSubDir);
        }

        // Copy all files to target
        foreach (FileInfo file in source.GetFiles())
        {
            string targetFilePath = Path.Combine(target.FullName, file.Name);
            file.CopyTo(targetFilePath, overwrite: true);
        }
    }

    static bool IsFileLocked(string filePath)
    {
        try
        {
            using var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch
        {
            return true;
        }
    }
}
