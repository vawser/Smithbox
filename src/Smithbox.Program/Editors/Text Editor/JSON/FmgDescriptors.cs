using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;


public class FmgDescriptors
{
    public ProjectType ProjectType { get; set; }

    public List<LanguageDescriptor> Languages { get; set; }
    public List<FmgContainerDescriptor> Containers { get; set; }
    public List<FmgDescriptor> List { get; set; }
}

public class FmgDescriptor
{
    public int ID { get; set; }

    // The name to display this FMG with in the editor
    public string DisplayName { get; set; }

    // The group name for the FMG (i.e. Goods)
    // For ungrouped, it is just the name of the FMG
    public string Domain { get; set; }

    // Used for DS2 secondary groupings
    public string SecondaryDomain { get; set; }

    // The container the FMG is in (Item, Menu, NGWord)
    public string Container { get; set; }

    // The role the FMG has in a group (Title, Description, etc)
    public string Role { get; set; }

    // The variant the FMG is (Vanilla, DLC1, DLC2)
    public string Variant { get; set; }
}

public class LanguageDescriptor
{
    // The folder abbreviation this language is found in
    public string Abbreviation { get; set; }

    // The display name for the language
    public string Language { get; set; }
}

public class FmgContainerDescriptor
{
    // The actual file name of the container
    public string FileName { get; set; }

    // The name used in the FmgDescriptor to associate with this container
    public string ContainerName { get; set; }

    // The variant used in the FmgDescriptor to associate with this container
    public string VariantName { get; set; }

    // The display name for the container
    public string DisplayName { get; set; }

    public bool Obsolete { get; set; } = false;
}