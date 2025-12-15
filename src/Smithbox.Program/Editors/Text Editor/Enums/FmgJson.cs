using System.Collections.Generic;

namespace StudioCore.Editors.TextEditor;

// Languages
public class LanguageDef
{
    public List<LanguageEntry> Entries { get; set; }
}

public class LanguageEntry
{
    public string Language { get; set; }
    public string DisplayName { get; set; }
    public string Path { get; set; }
}

// Containers
public class ContainerDef
{
    public List<FmgContainerEntry> Entries { get; set; }
}

public class FmgContainerEntry
{
    public string DisplayName { get; set; }
    public string Filename { get; set; }
    public bool Obsolete { get; set; }
    public List<FmgFileEntry> Files { get; set; }
}

public class FmgFileEntry
{
    public string Filename { get; set; }
    public int ID { get; set; }
    public string SimpleName { get; set; }
    public string FullName { get; set; }
}

// Associations
public class AssociationDef
{
    public List<FmgAssociationGroup> Entries { get; set; }
}

public class FmgAssociationGroup
{
    public string Name { get; set; }
    public string Filename { get; set; }
    public List<FmgAssociationEntry> Associations { get; set; }
}

public class FmgAssociationEntry
{
    public string Filename { get; set; }
    public string Type { get; set; }
    public int ID { get; set; }
    public int Priority { get; set; }
}