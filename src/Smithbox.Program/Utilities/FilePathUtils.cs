using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

public class FilePathUtils
{
    /// <summary>
    /// Checks if the filename is valid for Windows and doesn't contain non-ASCII characters.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static bool IsValidFileName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        // Forbidden characters in Windows filenames
        char[] invalidChars = Path.GetInvalidFileNameChars();

        foreach (char c in input)
        {
            if (c > 0x7F)
                return false; // non-ASCII character

            if (Array.IndexOf(invalidChars, c) >= 0)
                return false; // invalid file character
        }

        return true;
    }
}
