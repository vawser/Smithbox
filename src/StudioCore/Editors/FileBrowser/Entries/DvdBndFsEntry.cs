#nullable enable
using Andre.Core.Util;
using Andre.Formats;
using Andre.IO.VFS;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.FileBrowserNS;

public class DvdBndFsEntry : SoulsFileFsEntry
{
    internal bool isInitialized = false;
    public override bool IsInitialized => isInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => getBdtStream != null;
    public override List<FsEntry> Children => innerFsEntry?.Children ?? [];
    public override bool CanView => true;
    internal Func<FileStream>? getBdtStream;
    internal FileStream? bdtStream;
    private Func<Memory<byte>> getBhdFunc;
    private Memory<byte>? bhdData = null;
    private byte[]? decryptedBhdData = null;

    private BinderArchive? archive = null;
    private BHD5? bhd = null;
    private BhdDictionary? dictionary = null;
    private ArchiveBinderVirtualFileSystem? innerFs = null;
    private VirtualFileSystemFsEntry? innerFsEntry = null;
    private bool wasEncrypted = false;

    public DvdBndFsEntry(string name, Func<FileStream>? getBdtStream, Func<Memory<byte>> getBhdFunc)
    {
        this.name = name;
        this.getBdtStream = getBdtStream;
        this.getBhdFunc = getBhdFunc;
    }

    internal override void Load(ProjectEntry ownerProject)
    {
        bhdData = getBhdFunc();
        wasEncrypted = false;

        var andreGame = ownerProject.ProjectType.AsAndreGame();
        var bhdGame = ownerProject.ProjectType.AsBhdGame();

        if (BinderArchive.IsBhdEncrypted(bhdData.Value))
        {
            wasEncrypted = true;

            //TaskLogs.AddVerboseLog($"[File Browser] DVDBND {name} appears encrypted. Decrypting (this may take a while)...");

            string path = name.Replace(".bdt", ".bhd");

            if (ownerProject.ProjectType.AsBhdGame() >= BHD5.Game.EldenRing && name.Contains("sd"))
            {
                path = $"sd\\{name}";
            }

            if (andreGame != null)
            {
                decryptedBhdData = BinderArchive.Decrypt(bhdData.Value, path, andreGame.Value);
                bhdData = new(decryptedBhdData);
            }
        }

        if (andreGame != null && bhdGame != null)
        {
            dictionary = ArchiveBinderVirtualFileSystem.GetDictionaryForGame(andreGame.Value);

            bhd = BHD5.Read(bhdData.Value, bhdGame.Value);

            if (getBdtStream != null)
            {
                bdtStream = getBdtStream();
                archive = new(bhd, bdtStream, wasEncrypted);
                innerFs = new ArchiveBinderVirtualFileSystem([archive], dictionary);
                innerFsEntry = new(ownerProject, innerFs, name);
                innerFsEntry.Load(ownerProject);
            }
        }

        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        innerFsEntry?.Unload();
        innerFsEntry = null;
        innerFs = null;
        dictionary = null;
        archive = null;
        bhd = null;
        bhdData = null;
        decryptedBhdData = null;
        bdtStream?.Close();
        bdtStream = null;
        isInitialized = false;
    }

    private void FileHeaderGui(string id, BHD5.FileHeader h)
    {
        PropertyTable(id + "##PropertyTable", (row) =>
        {
            row("FileNameHash", h.FileNameHash.ToString());
            row("FileOffset", h.FileOffset.ToString());
            row("PaddedFileSize", h.PaddedFileSize.ToString());
            row("UnpaddedFileSize", h.UnpaddedFileSize.ToString());
            row("SHAHash", h.SHAHash == null ? "" : BitConverter.ToString(h.SHAHash.Hash).Replace("-", ""));
            row("AESKey", h.AESKey == null ? "" : BitConverter.ToString(h.AESKey.Key).Replace("-", ""));
            foreach (var (i, r) in h.SHAHash?.Ranges?.Select((r, i) => (i, r)) ?? [])
            {
                row($"SHAHash.Ranges[{i}]", $"{r.StartOffset}..{r.EndOffset}");
            }
            foreach (var (i, r) in h.AESKey?.Ranges?.Select((r, i) => (i, r)) ?? [])
            {
                row($"AESKey.Ranges[{i}]", $"{r.StartOffset}..{r.EndOffset}");
            }
        });
    }

    public override void OnGui()
    {
        ImGui.Text($"DVDBND File {name}");

        if (getBdtStream == null)
            ImGui.Text($"Note: This is the bhd file. For more information, see the bdt file that should be at {name.Replace(".bhd", ".bdt")}");

        if (bhd != null)
        {
            PropertyTable("DVDBND Properties", (row) =>
            {
                row("Encrypted", wasEncrypted.ToString());
                row("BigEndian", bhd.BigEndian.ToString());
                row("Salt", bhd.Salt ?? "");
                row("Unk05", bhd.Unk05.ToString());
            });
            if (innerFs != null)
            {
                if (ImGui.CollapsingHeader("Files##DVDBND_Files"))
                {
                    ImGui.TreePush("Files##DVDBND_Files");
                    foreach (var (fpath, file) in innerFs.FileHeaders)
                    {
                        if (ImGui.CollapsingHeader(fpath))
                        {
                            ImGui.TreePush(fpath);
                            FileHeaderGui(fpath, file);
                            ImGui.TreePop();
                        }
                    }
                    ImGui.TreePop();
                }
            }

            if (ImGui.CollapsingHeader("Buckets##DVDBND_Buckets"))
            {
                ImGui.TreePush("Buckets##DVDBND_Buckets");
                foreach (var (i, bucket) in bhd.Buckets.Select((b, i) => (i, b)))
                {
                    if (ImGui.CollapsingHeader($"Bucket {i}##{name}"))
                    {
                        ImGui.TreePush($"Bucket {i}##{name}");
                        foreach (var f in bucket)
                        {
                            string headerStr;

                            if (dictionary != null && dictionary.GetPath(f.FileNameHash, out var path))
                            {
                                headerStr = $"File hash {f.FileNameHash} ({path})";
                            }
                            else
                            {
                                headerStr = $"File hash {f.FileNameHash}";
                            }

                            if (ImGui.CollapsingHeader(headerStr))
                            {
                                ImGui.TreePush(headerStr);
                                FileHeaderGui(headerStr, f);
                                ImGui.TreePop();
                            }
                        }
                        ImGui.TreePop();
                    }
                }
                ImGui.TreePop();
            }
        }
    }
}