using Microsoft.Extensions.Logging;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class GxDescriptorBank
{
    private ModelEditorScreen Screen;

    private string DescriptorPath = $"{AppContext.BaseDirectory}\\Assets\\GX Items\\GXItemDescriptors.json";

    public GXDescriptorList Descriptors = new GXDescriptorList();

    public GxDescriptorBank(ModelEditorScreen screen)
    {
        Screen = screen;

        try
        {
            Descriptors = LoadJson(DescriptorPath);
            TaskLogs.AddLog($"Banks: setup GX item descriptor bank.");
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Banks: failed to setup GX item descriptor bank: {DescriptorPath}\n{e}", LogLevel.Error);
        }
    }

    public GXDescriptorList LoadJson(string path)
    {
        using (var stream = File.OpenRead(path))
        {
            var resource = JsonSerializer.Deserialize(stream, GXDescriptorListContext.Default.GXDescriptorList);

            return resource;
        }
    }

    public GX00ItemDescriptor GetEntry(string id )
    {
        if(Descriptors.List.Any(e => e.ID == id))
        {
            return Descriptors.List.Where(e => e.ID == id).FirstOrDefault();
        }

        return null;
    }
}
