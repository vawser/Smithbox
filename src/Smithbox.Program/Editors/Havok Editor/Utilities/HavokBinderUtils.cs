using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public static class HavokBinderUtils
{
    public static BinderFile GetInsertFile(HavokFileView.FileAction fileAction, BinderFile sourceFile, string filename, byte[] fileData)
    {
        var binderEntry = fileAction.BankDict[fileAction.BinderEntry];

        var sourceName = Path.GetFileName(sourceFile.Name);
        var newFilePath = sourceFile.Name.Replace(sourceName, filename);

        var newFile = new BinderFile
        {
            // Remain the same
            Flags = sourceFile.Flags,
            Bytes = fileData,
            CompressionType = sourceFile.CompressionType,

            // Increment ID to +1 from last ID
            ID = sourceFile.ID + 1,

            Name = newFilePath
        };

        return newFile;
    }

    public static BinderFile GetPasteFile(HavokFileView.FileAction fileAction, BinderFile sourceFile, BinderFile idFile, string primaryExtension)
    {
        var binderEntry = fileAction.BankDict[fileAction.BinderEntry];

        var newFile = new BinderFile
        {
            // Remain the same
            Flags = sourceFile.Flags,
            Bytes = sourceFile.Bytes,
            CompressionType = sourceFile.CompressionType,

            // Increment ID to +1 from last ID
            ID = idFile.ID + 1,

            Name = GetUniqueFileName(sourceFile.Name, binderEntry.Keys, "hkx")
        };

        return newFile;
    }

    private static string GetUniqueFileName(string baseName, IEnumerable<string> existingNames, string primaryExtension)
    {
        // Use a HashSet for O(1) lookups instead of scanning the collection each time
        var nameSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

        if (!nameSet.Contains(baseName))
            return baseName;

        int lastSlash = baseName.LastIndexOf('\\');
        string directory = lastSlash >= 0 ? baseName.Substring(0, lastSlash + 1) : string.Empty;
        string fileName = Path.GetFileName(baseName);

        if(baseName.Contains(".dcx"))
        {
            fileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(baseName));
        }
        else
        {
            fileName = Path.GetFileNameWithoutExtension(baseName);
        }

        int suffix = 1;
        string candidate;
        do
        {
            if (baseName.Contains(".dcx"))
            {
                candidate = $"{directory}{fileName}_{suffix}.{primaryExtension}.dcx";
            }
            else
            {
                candidate = $"{directory}{fileName}_{suffix}.{primaryExtension}";
            }

            suffix++;
        }
        while (nameSet.Contains(candidate));

        return candidate;
    }

    public static string ReplaceFileName(string path, string newName)
    {
        string directory = Path.GetDirectoryName(path);
        string fileName = Path.GetFileName(path);

        var (baseName, extensions) = SplitKnownExtensions(fileName);

        string newFileName = newName + extensions;
        return Path.Combine(directory ?? string.Empty, newFileName);
    }

    public static readonly string[] KnownExtensions = { "hkx", "dcx" };

    public static (string baseName, string extensions) SplitKnownExtensions(string fileName)
    {
        string remaining = fileName;
        string extensions = "";

        while (true)
        {
            int lastDot = remaining.LastIndexOf('.');
            if (lastDot == -1)
                break;

            string candidateExt = remaining.Substring(lastDot + 1);

            if (KnownExtensions.Contains(candidateExt, StringComparer.OrdinalIgnoreCase))
            {
                extensions = "." + candidateExt + extensions;
                remaining = remaining.Substring(0, lastDot);
            }
            else
            {
                break;
            }
        }

        return (remaining, extensions);
    }
}
