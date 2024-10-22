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

    private string DescriptorPath = $"{Smithbox.SmithboxDataRoot}\\GX Items\\GXItemDescriptors.json";

    public GXDescriptorList Descriptors = new GXDescriptorList();

    public GxDescriptorBank(ModelEditorScreen screen)
    {
        Screen = screen;

        try
        {
            Descriptors = LoadJson(DescriptorPath);
        }
        catch (Exception e)
        {
#if DEBUG
            TaskLogs.AddLog($"Failed to load: GX Item Descriptors Bank: {e.Message}");
#endif
        }

        TaskLogs.AddLog($"GX Item Descriptors: Loaded Bank");
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
