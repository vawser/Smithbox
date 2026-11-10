using Andre.Core;
using Andre.IO.VFS;

// NavBaselineCheck overlay|stitchedit [regex] [max] : see NavBaselineCheck.csproj
var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "Claude", "game_data.txt"))
    .Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());
var root = gamePaths["ER"];
using var fs = new CompundVirtualFileSystem([ArchiveBinderVirtualFileSystem.FromGameFolder(root, Game.ER), new RealVirtualFileSystem(root, false)]);
var dict = File.ReadAllText(Path.Combine("Assets", "File Dictionaries", "ER-File-Dictionary.json"));
Console.WriteLine("4685 paths");
if (args[0] == "overlay") Overlay.Run(fs, dict, args);
// stitchedit was compared for N1; NavSurvey's StitchEdit.cs now passes a map's profile, which the pre-N1 sources lack
