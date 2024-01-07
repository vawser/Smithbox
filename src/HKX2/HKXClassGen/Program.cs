using System;
using SoulsFormats.XmlExtensions;
using System.Xml;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Markup;

/// <summary>
/// Autogenerates Havok packfile class serializers based on dumped XML reflection data
/// </summary>
namespace HKXClassGen
{
    class Program
    {
        public static StreamWriter CurrentFile;
        public static int CurrentIndent = 0;

        public static Dictionary<string, HKXClass> Classes = new Dictionary<string, HKXClass>();

        public static void PushIndent()
        {
            CurrentIndent++;
        }

        public static void PopIndent()
        {
            CurrentIndent--;
        }

        public static void WriteLine(string s)
        {
            string indent = "";
            for (int i = 0; i < CurrentIndent; i++)
            {
                indent += "    ";
            }
            CurrentFile.WriteLine(indent + s);
        }

        public class HKXClass
        {
            public string ParentName;
            public uint Signature;
            public List<HKXEnum> Enums = new List<HKXEnum>();
            public List<HKXMember> Members = new List<HKXMember>();

            public uint Size;

            public uint LocalSize
            {
                get
                {
                    if (ParentName != null)
                    {
                        return Size - Classes[ParentName].Size;
                    }
                    return Size;
                }
            }

            public uint StartOffset
            {
                get
                {
                    if (ParentName != null)
                    {
                        return Classes[ParentName].Size;
                    }
                    return 0;
                }
            }
        }

        public class HKXMember
        {
            public string Name;
            public uint Offset;
            public VTYPE VType;
            public VTYPE VSubType;
            public string CType;
            public string EType;
            public uint ArraySize;
            public string Flags;
        }

        public class HKXEnum
        {
            public string Name;
            public Dictionary<string, int> EnumValues = new Dictionary<string, int>();
        }

        public static void ParseMembers(XmlNodeList nodes, HKXClass hkclass)
        {
            foreach (XmlNode n in nodes)
            {
                HKXMember m = new HKXMember();
                m.Name = n.ReadStringAttribute("name");
                m.Offset = n.ReadUInt32Attribute("offset");
                m.VType = Enum.Parse<VTYPE>(n.ReadStringAttribute("vtype"));
                m.VSubType = Enum.Parse<VTYPE>(n.ReadStringAttribute("vsubtype"));
                m.CType = n.ReadStringAttributeOrDefault("ctype");
                m.EType = n.ReadStringAttributeOrDefault("etype");
                m.ArraySize = n.ReadUInt32Attribute("arrsize");
                m.Flags = n.ReadStringAttribute("flags");
                if (m.Flags != null && !m.Flags.Contains("SERIALIZE_IGNORED"))
                {
                    hkclass.Members.Add(m);
                }
            }
        }

        public static void ParseEnums(XmlNodeList nodes, HKXClass hkclass)
        {
            foreach (XmlNode n in nodes)
            {
                HKXEnum e = new HKXEnum();
                e.Name = n.ReadStringAttribute("name");
                foreach (XmlNode c in n.ChildNodes)
                {
                    string name = c.ReadStringAttribute("name");
                    int val = c.ReadInt32Attribute("value");
                    e.EnumValues.Add(name, val);
                }
                hkclass.Enums.Add(e);
            }
        }

        public static HKXClass ParseClass(XmlDocument doc)
        {
            var cls = doc.FirstChild;
            var hkclass = new HKXClass();
            hkclass.Signature = cls.ReadUInt32HexAttribute("signature");
            hkclass.ParentName = cls.ReadStringAttributeOrDefault("parent");
            hkclass.Size = cls.ReadUInt32Attribute("size");

            var enums = cls.SelectSingleNode("enums");
            if (enums != null)
            {
                ParseEnums(enums.ChildNodes, hkclass);
            }

            ParseMembers(cls.SelectSingleNode("members").ChildNodes, hkclass);

            return hkclass;
        }

        public static void WriteEnum(HKXEnum e)
        {
            WriteLine("public enum " + e.Name);
            WriteLine("{");
            PushIndent();

            foreach (var m in e.EnumValues)
            {
                WriteLine($@"{m.Key} = {m.Value},");
            }

            PopIndent();
            WriteLine("}");
        }

        public const int POINTER_SIZE = 8;

        public enum VTYPE
        {
            TYPE_VOID,
            TYPE_ENUM,
            TYPE_CSTRING,
            TYPE_STRINGPTR,
            TYPE_UINT8,
            TYPE_INT8,
            TYPE_CHAR,
            TYPE_UINT16,
            TYPE_INT16,
            TYPE_HALF,
            TYPE_UINT32,
            TYPE_INT32,
            TYPE_ULONG,
            TYPE_UINT64,
            TYPE_LONG,
            TYPE_INT64,
            TYPE_BOOL,
            TYPE_FLAGS,
            TYPE_REAL,
            TYPE_QUATERNION,
            TYPE_ROTATION,
            TYPE_VECTOR4,
            TYPE_MATRIX4,
            TYPE_MATRIX3,
            TYPE_TRANSFORM,
            TYPE_QSTRANSFORM,
            TYPE_POINTER,
            TYPE_ARRAY,
            TYPE_RELARRAY,
            TYPE_SIMPLEARRAY,
            TYPE_STRUCT,
            TYPE_VARIANT,
        }

        public static uint MemberSize(HKXMember m, VTYPE t, bool v = false)
        {
            var adjarrsize = m.ArraySize;
            if (v || adjarrsize == 0)
            {
                adjarrsize = 1;
            }
            if (t == VTYPE.TYPE_ENUM)
            {
                return MemberSize(m, m.VSubType, true);
                //return 4; // Almost always 4 bytes regardless of vsubtype?
            }
            else if (t == VTYPE.TYPE_CSTRING)
            {
                return POINTER_SIZE;
            }
            else if (t == VTYPE.TYPE_STRINGPTR)
            {
                return POINTER_SIZE;
            }
            else if (t == VTYPE.TYPE_UINT8)
            {
                return 1 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_INT8)
            {
                return 1 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_CHAR)
            {
                return 1 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_UINT16)
            {
                return 2 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_INT16)
            {
                return 2 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_HALF)
            {
                return 2 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_UINT32)
            {
                return 4 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_INT32)
            {
                return 4 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_ULONG)
            {
                return 8 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_UINT64)
            {
                return 8 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_LONG)
            {
                return 8 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_INT64)
            {
                return 8 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_INT32)
            {
                return 4 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_BOOL)
            {
                return 1 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_FLAGS)
            {
                return MemberSize(m, m.VSubType, true);
            }
            else if (t == VTYPE.TYPE_REAL)
            {
                return 4 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_QUATERNION)
            {
                return 16 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_ROTATION)
            {
                return 48 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_VECTOR4)
            {
                return 16 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_MATRIX4)
            {
                return 64 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_MATRIX3)
            {
                return 48 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_TRANSFORM)
            {
                return 64 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_QSTRANSFORM)
            {
                return 64 * adjarrsize;
            }
            else if (t == VTYPE.TYPE_POINTER)
            {
                return POINTER_SIZE * adjarrsize;
            }
            else if (t == VTYPE.TYPE_ARRAY)
            {
                return 16;
            }
            else if (t == VTYPE.TYPE_RELARRAY)
            {
                return 4;
            }
            else if (t == VTYPE.TYPE_SIMPLEARRAY)
            {
                return 16; // ?
            }
            else if (t == VTYPE.TYPE_STRUCT)
            {
                return Classes[m.CType].Size * adjarrsize;
            }
            else if (t == VTYPE.TYPE_VARIANT)
            {
                return 8; // Don't really care for this
            }
            throw new Exception("Unknown type");
        }

        public static string ReduceType(HKXMember m, VTYPE t, bool v = false)
        {
            string r;
            if (t == VTYPE.TYPE_ENUM)
            {
                r = m.EType;
            }
            else if (t == VTYPE.TYPE_CSTRING)
            {
                r = "string";
            }
            else if (t == VTYPE.TYPE_STRINGPTR)
            {
                r = "string";
            }
            else if (t == VTYPE.TYPE_UINT8)
            {
                r = "byte";
            }
            else if (t == VTYPE.TYPE_INT8)
            {
                r = "sbyte";
            }
            else if (t == VTYPE.TYPE_CHAR)
            {
                r = "sbyte";
            }
            else if (t == VTYPE.TYPE_UINT16)
            {
                r = "ushort";
            }
            else if (t == VTYPE.TYPE_INT16)
            {
                r = "short";
            }
            else if (t == VTYPE.TYPE_HALF)
            {
                r = "short";
            }
            else if (t == VTYPE.TYPE_UINT32)
            {
                r = "uint";
            }
            else if (t == VTYPE.TYPE_INT32)
            {
                r = "int";
            }
            else if (t == VTYPE.TYPE_ULONG)
            {
                r = "ulong";
            }
            else if (t == VTYPE.TYPE_UINT64)
            {
                r = "ulong";
            }
            else if (t == VTYPE.TYPE_LONG)
            {
                r = "long";
            }
            else if (t == VTYPE.TYPE_INT64)
            {
                r = "long";
            }
            else if (t == VTYPE.TYPE_INT32)
            {
                r = "int";
            }
            else if (t == VTYPE.TYPE_BOOL)
            {
                r = "bool";
            }
            else if (t == VTYPE.TYPE_FLAGS)
            {
                r = ReduceType(m, m.VSubType, true);
            }
            else if (t == VTYPE.TYPE_REAL)
            {
                r = "float";
            }
            else if (t == VTYPE.TYPE_QUATERNION)
            {
                r = "Quaternion";
            }
            else if (t == VTYPE.TYPE_ROTATION)
            {
                r = "Matrix4x4";
            }
            else if (t == VTYPE.TYPE_VECTOR4)
            {
                r = "Vector4";
            }
            else if (t == VTYPE.TYPE_MATRIX4)
            {
                r = "Matrix4x4";
            }
            else if (t == VTYPE.TYPE_MATRIX3)
            {
                r = "Matrix4x4";
            }
            else if (t == VTYPE.TYPE_TRANSFORM)
            {
                r = "Matrix4x4";
            }
            else if (t == VTYPE.TYPE_QSTRANSFORM)
            {
                r = "Matrix4x4";
            }
            else if (t == VTYPE.TYPE_POINTER)
            {
                if (v)
                {
                    if (m.CType != null)
                    {
                        r = m.CType;
                    }
                    else
                    {
                        r = "void*";
                    }
                    return r;
                }
                r = ReduceType(m, m.VSubType, true);
            }
            else if (t == VTYPE.TYPE_ARRAY)
            {
                r = "List<" + ReduceType(m, m.VSubType, true) + ">";
            }
            else if (t == VTYPE.TYPE_RELARRAY)
            {
                r = "List<" + ReduceType(m, m.VSubType, true) + ">";
            }
            else if (t == VTYPE.TYPE_SIMPLEARRAY)
            {
                r = "List<" + ReduceType(m, m.VSubType, true) + ">";
            }
            else if (t == VTYPE.TYPE_STRUCT)
            {
                r = m.CType;
            }
            else if (t == VTYPE.TYPE_VARIANT)
            {
                r = "ulong";
            }
            else
            {
                throw new Exception("Unknown type");
            }
            return r;
        }

        public static void WriteMember(HKXMember m)
        {
            if (m.CType != null && m.CType == "hkxMesh")
            {
                return;
            }
            var n = "m_" + m.Name;

            string type = ReduceType(m, m.VType);
            if (m.VType != VTYPE.TYPE_ARRAY && m.VType != VTYPE.TYPE_RELARRAY && m.VType != VTYPE.TYPE_SIMPLEARRAY && m.ArraySize > 0)
            {
                for (int i = 0; i < m.ArraySize; i++)
                {
                    WriteLine("public " + type + " " + n + $@"_{i}" + ";");
                }
            }
            else
            {
                WriteLine("public " + type + " " + n + ";");
            }
        }

        public static string GetSimpleType(VTYPE t)
        {
            switch (t)
            {
                case VTYPE.TYPE_BOOL:
                    return "Boolean";
                case VTYPE.TYPE_CHAR:
                case VTYPE.TYPE_INT8:
                    return "SByte";
                case VTYPE.TYPE_UINT8:
                    return "Byte";
                case VTYPE.TYPE_INT16:
                case VTYPE.TYPE_HALF:
                    return "Int16";
                case VTYPE.TYPE_UINT16:
                    return "UInt16";
                case VTYPE.TYPE_INT32:
                    return "Int32";
                case VTYPE.TYPE_UINT32:
                    return "UInt32";
                case VTYPE.TYPE_INT64:
                case VTYPE.TYPE_LONG:
                    return "Int64";
                case VTYPE.TYPE_UINT64:
                case VTYPE.TYPE_ULONG:
                    return "UInt64";
                case VTYPE.TYPE_REAL:
                    return "Single";
                default:
                    return null;
            }
        }

        public static string GetComplexType(VTYPE t)
        {
            switch (t)
            {
                case VTYPE.TYPE_QUATERNION:
                    return "Quaternion";
                case VTYPE.TYPE_ROTATION:
                    return "Matrix3";
                case VTYPE.TYPE_VECTOR4:
                    return "Vector4";
                case VTYPE.TYPE_MATRIX4:
                    return "Matrix4";
                case VTYPE.TYPE_MATRIX3:
                    return "Matrix3";
                case VTYPE.TYPE_TRANSFORM:
                    return "Transform";
                case VTYPE.TYPE_QSTRANSFORM:
                    return "QSTransform";
                case VTYPE.TYPE_CSTRING:
                case VTYPE.TYPE_STRINGPTR:
                    return "StringPointer";
                default:
                    return null;
            }
        }

        public static string GetEnumType(VTYPE t, string ename)
        {
            switch (t)
            {
                case VTYPE.TYPE_INT8:
                    return $@"SByte";
                case VTYPE.TYPE_UINT8:
                    return $@"Byte";
                case VTYPE.TYPE_INT16:
                    return $@"Int16";
                case VTYPE.TYPE_UINT16:
                    return $@"UInt16";
                case VTYPE.TYPE_INT32:
                    return $@"Int32";
                case VTYPE.TYPE_UINT32:
                    return $@"UInt32";
                default:
                    throw new Exception("Unimplemented type");
            }
        }

        public static void WritePadding(uint pad, bool isWriter)
        {
            uint toWrite = pad;
            while (toWrite > 0)
            {
                if (toWrite >= 8)
                {
                    if (isWriter)
                    {
                        WriteLine("bw.WriteUInt64(0);");
                    }
                    else
                    {
                        //WriteLine("br.AssertUInt64(0);");
                        WriteLine("br.ReadUInt64();");
                    }
                    toWrite -= 8;
                }
                else if (toWrite >= 4)
                {
                    if (isWriter)
                    {
                        WriteLine("bw.WriteUInt32(0);");
                    }
                    else
                    {
                        //WriteLine("br.AssertUInt32(0);");
                        WriteLine("br.ReadUInt32();");
                    }
                    toWrite -= 4;
                }
                else if (toWrite >= 2)
                {
                    if (isWriter)
                    {
                        WriteLine("bw.WriteUInt16(0);");
                    }
                    else
                    {
                        //WriteLine("br.AssertUInt16(0);");
                        WriteLine("br.ReadUInt16();");
                    }
                    toWrite -= 2;
                }
                else
                {
                    if (isWriter)
                    {
                        WriteLine("bw.WriteByte(0);");
                    }
                    else
                    {
                        //WriteLine("br.AssertByte(0);");
                        WriteLine("br.ReadByte();");
                    }
                    toWrite -= 1;
                }
            }
        }

        public static void WriteWriterReader(HKXClass cls, bool isWriter)
        {
            string virtAbstract = cls.ParentName != null ? "override" : "virtual";
            if (isWriter)
            {
                WriteLine("public " + virtAbstract + " void Write(PackFileSerializer s, BinaryWriterEx bw)");
            }
            else
            {
                WriteLine("public " + virtAbstract + " void Read(PackFileDeserializer des, BinaryReaderEx br)");
            }
            WriteLine("{");
            PushIndent();

            if (cls.ParentName != null && isWriter)
            {
                WriteLine("base.Write(s, bw);");
            }
            else if (cls.ParentName != null)
            {
                WriteLine("base.Read(des, br);");
            }

            HKXMember prev = null;
            for (int i = 0; i < cls.Members.Count; i++)
            {
                if (cls.Members[i].Flags.Contains("SERIALIZE_IGNORED"))
                {
                    continue;
                }
                if (cls.Members[i].CType != null && cls.Members[i].CType == "hkxMesh")
                {
                    continue;
                }

                uint prevoffset = prev == null ? cls.StartOffset : (prev.Offset + MemberSize(prev, prev.VType));
                if (prevoffset < cls.Members[i].Offset)
                {
                    WritePadding(cls.Members[i].Offset - prevoffset, isWriter);
                }

                prev = cls.Members[i];

                var m = cls.Members[i];

                // If it's a simple primitive, emit a direct read/write
                string brPrimitive = GetSimpleType(m.VType);
                if (brPrimitive != null)
                {
                    if (m.ArraySize > 0)
                    {
                        for (int j = 0; j < m.ArraySize; j++)
                        {
                            if (isWriter)
                            {
                                WriteLine($@"bw.Write{brPrimitive}(m_{m.Name}_{j});");
                            }
                            else
                            {
                                WriteLine($@"m_{m.Name}_{j} = br.Read{brPrimitive}();");
                            }
                        }
                    }
                    else
                    {
                        if (isWriter)
                        {
                            WriteLine($@"bw.Write{brPrimitive}(m_{m.Name});");
                        }
                        else
                        {
                            WriteLine($@"m_{m.Name} = br.Read{brPrimitive}();");
                        }
                    }
                    continue;
                }

                brPrimitive = GetComplexType(m.VType);
                if (brPrimitive != null)
                {
                    if (m.ArraySize > 0)
                    {
                        for (int j = 0; j < m.ArraySize; j++)
                        {
                            if (isWriter)
                            {
                                WriteLine($@"s.Write{brPrimitive}(bw, m_{m.Name}_{j});");
                            }
                            else
                            {
                                WriteLine($@"m_{m.Name}_{j} = des.Read{brPrimitive}(br);");
                            }
                        }
                    }
                    else
                    {
                        if (isWriter)
                        {
                            WriteLine($@"s.Write{brPrimitive}(bw, m_{m.Name});");
                        }
                        else
                        {
                            WriteLine($@"m_{m.Name} = des.Read{brPrimitive}(br);");
                        }
                    }
                    continue;
                }

                // Arrays
                if (m.VType == VTYPE.TYPE_ARRAY)
                {
                    brPrimitive = GetSimpleType(m.VSubType);
                    if (brPrimitive == null)
                    {
                        brPrimitive = GetComplexType(m.VSubType);
                    }
                    if (brPrimitive != null)
                    {
                        if (isWriter)
                        {
                            WriteLine($@"s.Write{brPrimitive}Array(bw, m_{m.Name});");
                        }
                        else
                        {
                            WriteLine($@"m_{m.Name} = des.Read{brPrimitive}Array(br);");
                        }
                        continue;
                    }
                    else if (m.VSubType == VTYPE.TYPE_STRUCT)
                    {
                        if (isWriter)
                        {
                            WriteLine($@"s.WriteClassArray<{m.CType}>(bw, m_{m.Name});");
                        }
                        else
                        {
                            WriteLine($@"m_{m.Name} = des.ReadClassArray<{m.CType}>(br);");
                        }
                        continue;
                    }
                    else if (m.VSubType == VTYPE.TYPE_POINTER)
                    {
                        if (isWriter)
                        {
                            WriteLine($@"s.WriteClassPointerArray<{m.CType}>(bw, m_{m.Name});");
                        }
                        else
                        {
                            WriteLine($@"m_{m.Name} = des.ReadClassPointerArray<{m.CType}>(br);");
                        }
                        continue;
                    }
                    WriteLine($@"// Read {m.VSubType} array");
                    continue;
                }

                // Enum
                if (m.VType == VTYPE.TYPE_ENUM)
                {
                    brPrimitive = GetEnumType(m.VSubType, m.EType);
                    if (isWriter)
                    {
                        WriteLine($@"bw.Write{brPrimitive}(({ReduceType(m, m.VSubType, true)})m_{m.Name});");
                    }
                    else
                    {
                        WriteLine($@"m_{m.Name} = ({m.EType})br.Read{brPrimitive}();");
                    }
                    continue;
                }
                // Flags
                if (m.VType == VTYPE.TYPE_FLAGS)
                {
                    brPrimitive = GetEnumType(m.VSubType, m.EType);
                    if (isWriter)
                    {
                        WriteLine($@"bw.Write{brPrimitive}(m_{m.Name});");
                    }
                    else
                    {
                        WriteLine($@"m_{m.Name} = br.Read{brPrimitive}();");
                    }
                    continue;
                }

                // Read inline class
                if (m.VType == VTYPE.TYPE_STRUCT)
                {
                    if (m.ArraySize > 0)
                    {
                        for (int j = 0; j < m.ArraySize; j++)
                        {
                            if (isWriter)
                            {
                                WriteLine($@"m_{m.Name}_{j}.Write(s, bw);");
                            }
                            else
                            {
                                WriteLine($@"m_{m.Name}_{j} = new {m.CType}();");
                                WriteLine($@"m_{m.Name}_{j}.Read(des, br);");
                            }
                        }
                    }
                    else
                    {
                        if (isWriter)
                        {
                            WriteLine($@"m_{m.Name}.Write(s, bw);");
                        }
                        else
                        {
                            WriteLine($@"m_{m.Name} = new {m.CType}();");
                            WriteLine($@"m_{m.Name}.Read(des, br);");
                        }
                    }
                    continue;
                }

                // Read class pointer
                if (m.VType == VTYPE.TYPE_POINTER)
                {
                    if (m.VSubType != VTYPE.TYPE_STRUCT)
                    {
                        throw new Exception("bruh");
                    }
                    if (m.ArraySize > 0)
                    {
                        for (int j = 0; j < m.ArraySize; j++)
                        {
                            if (isWriter)
                            {
                                WriteLine($@"s.WriteClassPointer<{m.CType}>(bw, m_{m.Name}_{j});");
                            }
                            else
                            {
                                WriteLine($@"m_{m.Name}_{j} = des.ReadClassPointer<{m.CType}>(br);");
                            }
                        }
                    }
                    else
                    {
                        if (isWriter)
                        {
                            WriteLine($@"s.WriteClassPointer<{m.CType}>(bw, m_{m.Name});");
                        }
                        else
                        {
                            WriteLine($@"m_{m.Name} = des.ReadClassPointer<{m.CType}>(br);");
                        }
                    }
                    continue;
                }

                WriteLine($@"// Read {m.VType}");
            }
            var writeEnd = (cls.Members.Count > 0) ? cls.Members.Last().Offset + MemberSize(cls.Members.Last(), cls.Members.Last().VType) : cls.Size - cls.LocalSize;
            if (writeEnd < cls.Size)
            {
                WritePadding(cls.Size - writeEnd, isWriter);
            }

            PopIndent();
            WriteLine("}");
        }

        public static bool IsSubEnum(string e)
        {
            return (e == "ScaleNormalBehavior" ||
                e == "Type" ||
                e == "Flags" ||
                e == "FlagValues" ||
                e == "FlagsEnum" ||
                e == "Axis" ||
                //e == "ScaleNormalBehaviour" ||
                e == "ControlByte" ||
                e == "SetAngleMethod" ||
                e == "RotationAxisCoordinates" ||
                e == "Component" ||
                e == "InternalFlags" ||
                e == "HandleChangeMode" ||
                e == "OffsetType" ||
                e == "AnimeEndEventType" ||
                e == "FaceFlagBits" ||
                e == "Constants" ||
                e == "LineOfSightFlags" ||
                e == "FlagBits" ||
                e == "ClipFlags" ||
                e == "FadeState" ||
                e == "MovementSpeedsEnum" ||
                e == "TriangleMaterial" ||
                //e == "PrimitiveType" ||
                e == "KeyboardKey" ||
                e == "Config" ||
                e == "SolverType" ||
                e == "BroadPhaseType" ||
                e == "PlaybackMode" ||
                e == "SimulationType" ||
                e == "GainState");
        }

        public static void WriteClass(string name, HKXClass cls)
        {
            WriteLine("namespace HKX2\r\n{");
            PushIndent();

            foreach (var e in cls.Enums)
            {
                if (!IsSubEnum(e.Name) && name != "hknpShapeType" && name != "hkpBvCompressedMeshShape" &&
                    name != "hclObjectSpaceMeshMeshDeformOperator" && name != "hclMeshMeshDeformOperator")
                {
                    WriteEnum(e);
                    WriteLine("");
                }
            }

            WriteLine("public partial class " + name + (cls.ParentName != null ? " : " + cls.ParentName : " : IHavokObject"));
            WriteLine("{");
            PushIndent();

            string virtAbstract = cls.ParentName != null ? "override" : "virtual";
            WriteLine($@"public {virtAbstract} uint Signature " + "{ get => " + cls.Signature + "; }");
            WriteLine("");

            foreach (var e in cls.Enums)
            {
                if (IsSubEnum(e.Name) || name == "hknpShapeType" || name == "hkpBvCompressedMeshShape" ||
                    name == "hclObjectSpaceMeshMeshDeformOperator" || name == "hclMeshMeshDeformOperator")
                {
                    WriteEnum(e);
                    WriteLine("");
                }
            }

            foreach (var m in cls.Members)
            {
                if (m.Flags != null && !m.Flags.Contains("SERIALIZE_IGNORED"))
                {
                    WriteMember(m);
                }
            }

            WriteLine("");
            WriteWriterReader(cls, false);
            WriteLine("");
            WriteWriterReader(cls, true);

            PopIndent();
            WriteLine("}");

            PopIndent();
            WriteLine("}");
        }

        static void Main(string[] args)
        {
            var classfiles = Directory.GetFileSystemEntries($@"classxmlds3", @"*.xml").ToList();
            foreach (var f in classfiles)
            {
                var xml = new XmlDocument();
                xml.Load(f);
                var c = ParseClass(xml);
                string name = Path.GetFileNameWithoutExtension(f);
                if (name.IndexOf('_') != -1)
                {
                    name = name.Substring(0, name.IndexOf('_'));
                }

                // Exclude some problematic classes that aren't needed
                if (name == "hkaiTraversalAnalysis" ||
                    name == "hkaiTraversalAnnotationLibraryAnnotation" ||
                    name == "hkaiTraversalAnnotationLibrary" ||
                    name == "hkaiTraversalAnalysisOutputSection" ||
                    name == "hkaiTraversalAnalysisOutput" ||
                    name == "hkbnpPhysicsInterface")
                {
                    continue;
                }

                Classes.Add(name, c);
            }

            foreach (var c in Classes)
            {
                Directory.CreateDirectory("out");
                using (CurrentFile = new StreamWriter($@"out\{c.Key}.cs"))
                {
                    WriteLine("using SoulsFormats;");
                    WriteLine("using System.Collections.Generic;");
                    WriteLine("using System.Numerics;");
                    WriteLine("");
                    WriteClass(c.Key, c.Value);
                }
            }
        }
    }
}
