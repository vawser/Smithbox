using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smithbox.Core.JSON;

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