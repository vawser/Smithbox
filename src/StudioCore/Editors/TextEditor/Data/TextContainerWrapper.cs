using SoulsFormats;
using StudioCore.Core.Project;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// A wrapper class that holds the information relating to the Text container
/// and any associated sub-classes, such as FmgWrappers that belong to it.
/// </summary>
public class TextContainerWrapper : IComparable<TextContainerWrapper>
{
    /// <summary>
    /// File name of the container (includes extensions)
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    /// Read path of the container
    /// </summary>
    public string ReadPath { get; set; }

    /// <summary>
    /// The relative path of the container (excluding filename)
    /// </summary>
    public string RelativePath { get; set; }

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

    public TextContainerWrapper() 
    {
        IsModified = false;
    }

    public int CompareTo(TextContainerWrapper other)
    {
        return string.Compare(Filename, other.Filename);
    }

    /// <summary>
    /// Returns the write out path when saving this container.
    /// </summary>
    public string GetWritePath()
    {
        return $"{Smithbox.ProjectRoot}//{RelativePath}//{Filename}";
    }

    /// <summary>
    /// Returns whether the container is considered 'unused'.
    /// This is used to hide the non-DLC2 containers for games that
    /// have redundant containers.
    /// </summary>
    public bool IsContainerUnused()
    {
        // Hide Base and DLC1 containers as they are not used
        if (Smithbox.ProjectType is ProjectType.ER)
        {
            if (Filename == "item.msgbnd.dcx" || 
                Filename == "menu.msgbnd.dcx" ||
                Filename == "item_dlc01.msgbnd.dcx" || 
                Filename == "menu_dlc01.msgbnd.dcx")
            {
                return true;
            }
        }
        // Hide Base and DLC1 containers as they are not used
        if (Smithbox.ProjectType is ProjectType.DS3)
        {
            if (Filename == "item_dlc1.msgbnd.dcx" || 
                Filename == "menu_dlc1.msgbnd.dcx")
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
        var name = Filename;
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

        if (Smithbox.ProjectType is ProjectType.DES)
        {
            if (name.Contains("sample"))
                prettyName = "Sample";

            // DES has compressed and uncompressed versions, so add some extra text so it is more obvious which is which
            if (name.Contains(".dcx"))
            {
                prettyName = $"{prettyName} [Compressed]";
            }
        }

        return prettyName;
    }

}