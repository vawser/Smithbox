using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Resources.JSON;

public class ProjectDisplay
{
    public Dictionary<int, Guid> ProjectOrder { get; set; }
}

public class AliasStore
{
    public List<AliasEntry> Assets { get; set; }
    public List<AliasEntry> Characters { get; set; }
    public List<AliasEntry> Cutscenes { get; set; }
    public List<AliasEntry> EventFlags { get; set; }
    public List<AliasEntry> Gparams { get; set; }
    public List<AliasEntry> MapPieces { get; set; }
    public List<AliasEntry> MapNames { get; set; }
    public List<AliasEntry> Movies { get; set; }
    public List<AliasEntry> Particles { get; set; }
    public List<AliasEntry> Parts { get; set; }
    public List<AliasEntry> Sounds { get; set; }
    public List<AliasEntry> TalkScripts { get; set; }
    public List<AliasEntry> TimeActs { get; set; }
}

public class AliasEntry
{
    public string ID { get; set; }
    public string Name { get; set; }
    public List<string> Tags { get; set; }
}

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
