using HKLib.hk2018.hkAsyncThreadPool;
using Octokit;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// A wrapper class that holds the information relating to the Text container
/// and any associated sub-classes, such as FmgWrappers that belong to it.
/// </summary>
public class TextContainerWrapper : IComparable<TextContainerWrapper>
{
    public FileDictionaryEntry FileEntry { get; set; }

    private ProjectEntry Project;

    /// <summary>
    /// Whether the contents of this container has been modified
    /// </summary>
    public bool IsModified { get; set; }

    /// <summary>
    /// The DCX compression type used by this container
    /// </summary>
    public DCX.Type CompressionType { get; set; }

    /// <summary>
    /// The container storage type (Loose or Binder)
    /// </summary>
    public TextContainerType ContainerType { get; set; }

    /// <summary>
    /// The category in which to display this container in the File List
    /// </summary>
    public TextContainerCategory ContainerDisplayCategory { get; set; }

    /// <summary>
    /// The sub category which to display this container in the File List
    /// Relevant to DS2 only
    /// </summary>
    public ContainerSubCategory ContainerDisplaySubCategory { get; set; }

    /// <summary>
    /// The associated FMG wrappers that belong to this container
    /// </summary>
    public List<TextFmgWrapper> FmgWrappers { get; set; }

    public TextContainerWrapper(ProjectEntry project) 
    {
        Project = project;
        IsModified = false;
    }

    public int CompareTo(TextContainerWrapper other)
    {
        return string.Compare(FileEntry.Filename, other.FileEntry.Filename);
    }

    /// <summary>
    /// Returns whether the container is considered 'unused'.
    /// This is used to hide the non-DLC2 containers for games that
    /// have redundant containers.
    /// </summary>
    public bool IsContainerUnused()
    {
        // Hide Base and DLC1 containers as they are not used
        if (Project.ProjectType is ProjectType.ER)
        {
            if (FileEntry.Filename == "item" ||
                FileEntry.Filename == "menu" ||
                FileEntry.Filename == "item_dlc01" ||
                FileEntry.Filename == "menu_dlc01")
            {
                return true;
            }
        }
        // Hide Base and DLC1 containers as they are not used
        if (Project.ProjectType is ProjectType.DS3)
        {
            if (FileEntry.Filename == "item_dlc1" ||
                FileEntry.Filename == "menu_dlc1")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get the 'community' name for the container file
    /// </summary>
    public string GetContainerDisplayName()
    {
        var name = FileEntry.Filename;
        var prettyName = name;

        if (name.Contains("item"))
            prettyName = "Item";

        if (name.Contains("menu"))
            prettyName = "Menu";

        if (name.Contains("sellregion"))
            prettyName = "Sell Region";

        if (name.Contains("ngword"))
            prettyName = "Blocked Words";

        // Only show DLC type in non-Simple mode
        if (!CFG.Current.TextEditor_SimpleFileList)
        {
            if (name.Contains("dlc01") || name.Contains("dlc1"))
                prettyName = $"{prettyName} - DLC 1";

            if (name.Contains("dlc02") || name.Contains("dlc2"))
                prettyName = $"{prettyName} - DLC 2";
        }

        if (Project.ProjectType is ProjectType.DES)
        {
            if (name.Contains("sample"))
                prettyName = "Sample";

            // DES has compressed and uncompressed versions, so add some extra text so it is more obvious which is which
            if (FileEntry.Extension.Contains("dcx"))
            {
                prettyName = $"{prettyName} [Compressed]";
            }
        }

        return prettyName;
    }
}