using System;
using System.Collections.Generic;
using System.Text;
namespace StudioCore.Application;

public class HavokObjectAliases
{
    public List<HavokAliasFileEntry> Files = new();
}

public class HavokAliasFileEntry
{
    public string Path { get; set; } = "";

    public List<HavokAliasEntry> Aliases = new();
}

public class HavokAliasEntry
{
    public string ID { get; set; } = "";
    public string Name { get; set; } = "";
}