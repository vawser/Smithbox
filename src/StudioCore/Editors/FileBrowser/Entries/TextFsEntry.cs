#nullable enable
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StudioCore.FileBrowserNS;

public class TextFsEntry : FsEntry
{
    private ProjectEntry Project;

    internal bool isInitialized = false;
    public override bool IsInitialized => isInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => false;
    public override bool CanView => true;
    public override List<FsEntry> Children => [];

    internal Func<Memory<byte>> getDataFunc;
    internal Memory<byte>? data = null;
    internal string? contents = null;

    private static Encoding[] encoders = [Encoding.UTF8, Encoding.Unicode, Encoding.GetEncoding("shift-jis")];
    private Encoding encoding = Encoding.UTF8;
    private bool issueDecoding = false;

    public TextFsEntry(ProjectEntry ownerProject, string name, Func<Memory<byte>> getDataFunc)
    {
        Project = ownerProject;
        this.name = name;
        this.getDataFunc = getDataFunc;
    }

    private void DecodeText(Encoding e)
    {
        if(data != null)
            contents = e.GetString(data.Value.Span);
    }

    internal override void Load(ProjectEntry ownerProject)
    {
        data = getDataFunc();
        bool ok = false;
        //we have no way of knowing what encoding this text file uses, so we'll try each one in our list of
        //possible encodings until we get output that looks right.
        List<(Encoding, Exception)>? errors = null;
        foreach (var e in encoders)
        {
            try
            {
                DecodeText(e);
            }
            catch (Exception e1)
            {
                if (errors == null)
                    errors = [];
                errors.Add((e, e1));
                //TaskLogs.AddVerboseLog($"[File Browser] Failed to decode file {name} as {e.EncodingName}, trying next encoding...", LogLevel.Debug, e1);
                continue;
            }
            encoding = e;
            if (contents?.Contains('�') ?? false)
            {
                //TaskLogs.AddVerboseLog($"[File Browser] Decoding text in file {name} as {e.EncodingName} yielded error characters, trying next encoding...", LogLevel.Debug);
            }
            else
            {
                ok = true;
                break;
            }
        }

        if (!ok)
        {
            //every encoding we know to try either threw an exception or produced output that didn't
            //look right.
            issueDecoding = true;
            if (contents == null)
            {
                contents = "ERROR: COULD NOT DECODE FILE CONTENTS. DISPLAYING AS LIST OF BYTES.\n";
                contents += string.Join(", ", data.Value.ToArray());
            }
            Exception? e = null;
            if (errors != null)
                e = new AggregateException(
                    "Failed to find encoding for text",
                    errors.Select(t =>
                        new Exception($"Error decoding text as {t.Item1.EncodingName}", t.Item2))
                );
            //TaskLogs.AddVerboseLog($"[File Browser] Failed to find encoding for text in file {name}.", LogLevel.Warning, ex: e);
        }

        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        contents = null;
        data = null;
        isInitialized = false;
    }

    public override void OnGui()
    {
        ImGui.Text($"Text file: {name}");
        if (issueDecoding)
        {
            ImGui.Text("Warning: we failed to automatically find the encoding of this text.");
            ImGui.Text("If any text is displayed, it may be malformed or incorrect.");
        }
        ImGui.Text("Encoding:");
        foreach (var e in encoders)
        {
            if (ImGui.RadioButton(e.EncodingName, encoding.Equals(e)))
            {
                encoding = e;
                try
                {
                    DecodeText(e);
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"[File Browser] Failed to decode text of file {name} with selected encoding {e.EncodingName}", LogLevel.Error, Tasks.LogPriority.High, ex);
                }
            }
        }

        var size = ImGui.GetWindowSize() - new Vector2(50, 50);

        if (size.X < 50) 
            size.X = 50;

        if (size.Y < 50) 
            size.Y = 50;

        if (contents != null)
        {
            int byteCount = Encoding.UTF8.GetByteCount(contents) + 1;

            ImGui.InputTextMultiline("Text File Contents", ref contents, (nuint)byteCount, size, ImGuiInputTextFlags.ReadOnly);
        }
    }
}
