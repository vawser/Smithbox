using HKLib.Serialization.hk2018.Binary;
using HKX2;
using SoulsFormats;
using StudioCore.Application;
using System;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorWrapper
{
    public BehaviorContainerWrapper Parent { get; set; }

    public string Name { get; set; }

    public HavokWrapper Havok { get; set; }
    public AnimContainer Container { get; set; }

    public bool Loaded = false;

    public BehaviorWrapper(BehaviorContainerWrapper parent, string name)
    {
        Parent = parent;
        Name = name;

        Havok = new();
    }


    public void Load()
    {
        if (Loaded)
            return;

        try
        {
            var fileData = Parent.TargetFS.ReadFile(Parent.Path);

            if (fileData != null)
            {
                var binder = new BND4Reader(fileData.Value);
                foreach (var file in binder.Files)
                {
                    var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                    var filepath = file.Name.ToLower();

                    // Need to explicitly check the internal folder,
                    // since <id>.hkx is repeated within the binder.
                    if (filepath.Contains("behaviors") && filepath.Contains(".hkx"))
                    {
                        if (filename == Name)
                        {
                            var behaviorData = binder.ReadFile(file);

                            // HKX1
                            if (Parent.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S)
                            {
                                Havok.HKX1_Object = HKX.Read(behaviorData);
                            }
                            else if (Parent.Project.Descriptor.ProjectType is ProjectType.BB)
                            {
                                Havok.HKX1_Object = HKX.Read(behaviorData, HKX.HKXVariation.HKXBloodBorne);
                            }
                            // HKX2
                            else if (Parent.Project.Descriptor.ProjectType is ProjectType.DS3)
                            {
                                var br = new BinaryReaderEx(false, behaviorData);
                                var des = new PackFileDeserializer();

                                Havok.HKX2_Object = (hkRootLevelContainer)des.Deserialize(br);
                            }
                            // HKX3
                            else if (Parent.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
                            {
                                var serializer = new HavokBinarySerializer();

                                using (MemoryStream memoryStream = new MemoryStream(behaviorData.ToArray()))
                                {
                                    try
                                    {
                                        Havok.HKX3_Object = (HKLib.hk2018.hkRootLevelContainer)serializer.Read(memoryStream);
                                    }
                                    catch (Exception ex) { }
                                }
                            }
                        }
                    }
                }
            }

            Loaded = true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[Animation Editor] Failed to read {Parent.Path} during behavior load.", e);
        }
    }


    public void Unload()
    {

    }

    public void Save()
    {
        var containerPath = Parent.Path;
        var project = Parent.Project;
        var fs = Parent.TargetFS;

        try
        {
            var binderData = fs.ReadFile(containerPath);
            if (binderData != null)
            {
                var binder = BND4.Read(binderData.Value);
                foreach (var file in binder.Files)
                {
                    var filename = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                    var filepath = file.Name.ToLower();

                    if (filepath.Contains("behaviors") && filepath.Contains(".hkx"))
                    {
                        if (filename == Name)
                        {
                            // HKX1
                            if (Parent.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S)
                            {
                                file.Bytes = Havok.HKX1_Object.Write();
                            }
                            else if (Parent.Project.Descriptor.ProjectType is ProjectType.BB)
                            {
                                file.Bytes = Havok.HKX1_Object.Write();
                            }
                            // HKX2
                            else if (Parent.Project.Descriptor.ProjectType is ProjectType.DS3)
                            {
                                var bw = new BinaryWriterEx(false);
                                var serializer = new PackFileSerializer();

                                serializer.Serialize(Havok.HKX2_Object, bw);

                                file.Bytes = bw.FinishBytes();
                            }
                            // HKX3
                            else if (Parent.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
                            {
                                var stream = new MemoryStream();
                                var serializer = new HavokBinarySerializer();

                                serializer.Write(Havok.HKX3_Object, stream);

                                file.Bytes = stream.ToArray();
                            }
                        }
                    }
                }

                var outBinderData = binder.Write();
                project.VFS.ProjectFS.WriteFile(containerPath, outBinderData);

                Smithbox.Log(this, $"[Animation Editor] Saved {containerPath}.");
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[Animation Editor] Failed to write {containerPath} during behavior save.", e);
        }
    }
}

public class HavokWrapper
{
    public HKX HKX1_Object { get; set; }
    public HKX2.hkRootLevelContainer HKX2_Object { get; set; }
    public HKLib.hk2018.hkRootLevelContainer HKX3_Object { get; set; }
}