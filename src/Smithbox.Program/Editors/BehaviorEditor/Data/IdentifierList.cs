using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class IdentifierList
{
    private Memory<byte> _data;

    private string Text;

    public IdentifierList(Memory<byte> data)
    {
        _data = data;
        Text = ReadText();
    }

    public void UpdateIdentifierList(string newText)
    {
        Text = newText;
        _data = WriteText();
    }

    public string ReadText()
    {
        Encoding shiftJis = Encoding.GetEncoding("shift_jis");
        return shiftJis.GetString(_data.ToArray());
    }

    public byte[] WriteText()
    {
        Encoding shiftJis = Encoding.GetEncoding("shift_jis");
        return shiftJis.GetBytes(Text);
    }

    public byte[] GetData()
    {
        return _data.ToArray();
    }
}
