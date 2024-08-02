using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.HavokAliasBank;

public class HavokGeneratorAliasBank
{
    public HavokGeneratorAliasResource HavokAliases { get; set; }

    private string TemplateName = "Template.json";

    private string HavokAliasFileName = "";

    private string HavokAliasTitle = "";

    public HavokGeneratorAliasBank(string fileName, string title)
    {
        HavokAliasFileName = fileName;
        HavokAliasTitle = title;
    }

    public void LoadBank()
    {
        try
        {
            HavokAliases = BankUtils.LoadHavokAliasJSON(HavokAliasFileName);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Failed to load: {HavokAliasTitle} Bank: {e.Message}");
        }

        TaskLogs.AddLog($"Havok Alias Bank: Loaded {HavokAliasTitle} Bank");
    }

    public Dictionary<string, HavokAliasReference> GetEntries()
    {
        Dictionary<string, HavokAliasReference> Entries = new Dictionary<string, HavokAliasReference>();

        if (HavokAliases.List != null)
        {
            foreach (var entry in HavokAliases.List)
            {
                if (!Entries.ContainsKey(entry.ID))
                {
                    Entries.Add(entry.ID, entry);
                }
            }
        }

        return Entries;
    }

    public HavokGeneratorAliasResource LoadAliasResource(string path)
    {
        var newResource = new HavokGeneratorAliasResource();

        if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                newResource = JsonSerializer.Deserialize(stream, HavokAliasResourceSerializationContext.Default.HavokGeneratorAliasResource);
            }
        }

        return newResource;
    }
}
