using SoulsFormats;
using System;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// A .tae file within a source container. Only the bytes are held until it is first selected,
/// as a character can have well over a hundred of these.
/// </summary>
public class TimeActEntry
{
    public string Name { get; }
    public int BinderID { get; }

    public TAE TimeAct { get; private set; }

    public bool LoadFailed { get; private set; }

    private Memory<byte> Data;

    public TimeActEntry(string name, int binderID, Memory<byte> data)
    {
        Name = name;
        BinderID = binderID;
        Data = data;
    }

    public void Load()
    {
        if (TimeAct != null || LoadFailed)
            return;

        try
        {
            TimeAct = TAE.Read(Data);
        }
        catch (Exception e)
        {
            LoadFailed = true;

            Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_TimeAct", Name), e);
        }
    }
}
