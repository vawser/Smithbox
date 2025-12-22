using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class FileDictionary
{
    public List<FileDictionaryEntry> Entries { get; set; }
}
public class FileDictionaryEntry
{
    /// <summary>
    /// The archive this entry belongs to.
    /// </summary>
    public string Archive { get; set; }

    /// <summary>
    /// The relative path for this entry
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// The folder path for this entry (excludes the filename and extension)
    /// </summary>
    public string Folder { get; set; }

    /// <summary>
    /// The file name for this entry (excludes extension)
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    /// The extension for this entry (ignoring .dcx)
    /// </summary>
    public string Extension { get; set; }
}