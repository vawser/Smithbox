using System;
using System.Collections.Generic;
using System.Globalization;
using LuaDecompilerCore.Utilities;

namespace LuaDecompilerCore
{
    public class LuaFile
    {
        public enum LuaVersion
        {
            Lua50,
            Lua51Hks,
            Lua53Smash,
        }

        public readonly LuaVersion Version;

        private int _functionCount = 0;

        private int GetNextFunctionId()
        {
            return _functionCount++;
        }

        private static string? ReadLuaString(BinaryReaderEx br, LuaVersion version, bool sizeminusone=true)
        {
            ulong length;
            if (version == LuaVersion.Lua53Smash)
            {
                length = br.ReadByte();
                if (!sizeminusone)
                {
                    length++;
                }
            }
            else
            {
                length = br.ReadUInt64();
            }
            
            if (length <= 0)
                return null;
            
            string ret;
            if (version == LuaVersion.Lua50)
            {
                ret = br.ReadShiftJIS((int)length - 1);
            }
            else
            {
                ret = br.ReadUTF8((int)length - 1);
            }
            if (version != LuaVersion.Lua53Smash)
            {
                br.AssertByte(0); // Eat null terminator
            }
            return ret;
        }

        public class Header
        {
            public readonly LuaVersion LuaVersion;
            public readonly byte Version;
            public readonly byte Endianess;
            public readonly byte IntSize;
            public readonly byte LongSize;
            public readonly byte InstructionSize;
            public readonly byte OpSize;
            public readonly byte ASize;
            public readonly byte BSize;
            public readonly byte CSize;
            public readonly byte LuaNumberSize;
            public readonly byte FloatSize;

            public Header(BinaryReaderEx br)
            {
                br.ReadUInt32(); // Magic
                Version = br.ReadByte();
                if (Version == 0x50)
                {
                    LuaVersion = LuaVersion.Lua50;
                }
                else if (Version == 0x51)
                {
                    var format = br.AssertByte(0x0E); // HKS
                    LuaVersion = LuaVersion.Lua51Hks;
                }
                else if (Version == 0x53)
                {
                    LuaVersion = LuaVersion.Lua53Smash;
                    br.ReadBytes(7); //unk
                }
                if (LuaVersion == LuaVersion.Lua53Smash)
                {
                    Endianess = 0;
                }
                else
                {
                    Endianess = br.ReadByte();
                }
                IntSize = br.ReadByte();
                LongSize = br.ReadByte();
                InstructionSize = br.ReadByte();
                if (LuaVersion == LuaVersion.Lua50)
                {
                    OpSize = br.ReadByte();
                    ASize = br.ReadByte();
                    BSize = br.ReadByte();
                    CSize = br.ReadByte();
                }
                LuaNumberSize = br.ReadByte();
                if (LuaVersion == LuaVersion.Lua53Smash)
                {
                    FloatSize = br.ReadByte();
                }
                if (LuaVersion == LuaVersion.Lua50)
                {
                    br.ReadDouble(); // test number
                }
                else if (LuaVersion == LuaVersion.Lua53Smash)
                {
                    br.ReadUInt64(); // test int
                    br.ReadSingle(); // test number
                }
                else
                {
                    br.ReadByte(); // Isintegral
                }
            }
        }

        public class Constant
        {
            public enum ConstantType
            {
                TypeBoolean,
                TypeString,
                TypeNumber,
                TypeInt,
                TypeNil
            }

            public readonly ConstantType Type;
            public readonly bool BoolValue;
            public readonly string? StringValue;
            public readonly double NumberValue;
            public readonly ulong IntValue;

            public Constant(LuaFile file, BinaryReaderEx br, LuaVersion version)
            {
                var type = br.ReadByte();

                if (version == LuaVersion.Lua51Hks)
                {
                    switch (type)
                    {
                        case 0:
                            Type = ConstantType.TypeNil;
                            break;
                        case 1:
                            Type = ConstantType.TypeBoolean;
                            BoolValue = br.ReadBoolean();
                            break;
                        case 3:
                            NumberValue = file.LuaHeader.LuaNumberSize == 8 ? br.ReadDouble() : br.ReadSingle();
                            Type = ConstantType.TypeNumber;
                            break;
                        case 4:
                            StringValue = ReadLuaString(br, LuaVersion.Lua51Hks);
                            Type = ConstantType.TypeString;
                            break;
                        default:
                            throw new Exception("Unimplemented HKS type");
                    }
                    return;
                }
                
                if (type == 1)
                {
                    Type = ConstantType.TypeBoolean;
                    BoolValue = br.ReadBoolean();
                }
                if (type == 3)
                {
                    if (version == LuaVersion.Lua50)
                    {
                        NumberValue = br.ReadDouble();
                    }
                    else
                    {
                        NumberValue = br.ReadSingle();
                    }
                    Type = ConstantType.TypeNumber;
                }
                else if (type == 4 || type == 0x14)
                {
                    StringValue = ReadLuaString(br, version);
                    Type = ConstantType.TypeString;
                }
                else if (type == 0x13)
                {
                    IntValue = br.ReadUInt64();
                    Type = ConstantType.TypeInt;
                }
                else
                {
                    Type = ConstantType.TypeNil;
                }
            }

            public override string ToString()
            {
                return Type switch
                {
                    ConstantType.TypeString when StringValue != null => StringValue,
                    ConstantType.TypeNumber => NumberValue.ToString(CultureInfo.InvariantCulture),
                    ConstantType.TypeBoolean => BoolValue ? "true" : "false",
                    ConstantType.TypeNil => "nil",
                    _ => "NULL"
                };
            }
        }

        public class Local
        {
            public readonly string? Name;
            public readonly int Start;
            public readonly int End;

            public Local(BinaryReaderEx br, LuaVersion version)
            {
                Name = ReadLuaString(br, version, false);
                Start = br.ReadInt32();
                End = br.ReadInt32();
            }
        }

        /// <summary>
        /// Lua 5.3 upvalue definition
        /// </summary>
        public class UpValue
        {
            public readonly bool InStack;
            public readonly byte Id;

            public UpValue(BinaryReaderEx br)
            {
                InStack = br.ReadBoolean();
                Id = br.ReadByte();
            }
        }

        public class UpValueName
        {
            public readonly string? Name;

            public UpValueName(BinaryReaderEx br, LuaVersion version)
            {
                Name = ReadLuaString(br, version);
            }
        }

        public class Function
        {
            public string? Name;
            public string? Path;
            public int LineDefined;
            public byte NumUpValues;
            public uint NumParams;
            public uint NumSlots; // HKS
            public byte IsVarArg;
            public byte MaxStackSize;
            public int SizeLineInfo;
            public int LocalVarsCount;
            public int UpValuesCount;
            public Constant[] Constants = Array.Empty<Constant>();
            public Local[] Locals = Array.Empty<Local>();
            public Dictionary<int, List<Local>> LocalMap = new();
            public UpValue[] UpValues = Array.Empty<UpValue>();
            public UpValueName[] UpValueNames = Array.Empty<UpValueName>();
            public Function[] ChildFunctions = Array.Empty<Function>();
            public byte[] Bytecode = Array.Empty<byte>();
            public readonly int FunctionId;

            private void ReadLua53Smash(LuaFile file, BinaryReaderEx br)
            {
                Name = ReadLuaString(br, LuaVersion.Lua53Smash);
                LineDefined = br.ReadInt32();
                br.ReadInt32(); // last line
                NumParams = br.ReadByte();
                IsVarArg = br.ReadByte();
                MaxStackSize = br.ReadByte();
                var bytecodeCount = br.ReadInt32();
                Bytecode = br.ReadBytes(bytecodeCount * 4);
                var constantsCount = br.ReadInt32();
                Constants = new Constant[constantsCount];
                for (var i = 0; i < constantsCount; i++)
                {
                    Constants[i] = new Constant(file, br, LuaVersion.Lua53Smash);
                }
                var upValueCount = br.ReadInt32();
                UpValues = new UpValue[upValueCount];
                for (var i = 0; i < upValueCount; i++)
                {
                    UpValues[i] = new UpValue(br);
                }
                var functionCount = br.ReadInt32();
                ChildFunctions = new Function[functionCount];
                for (var i = 0; i < functionCount; i++)
                {
                    ChildFunctions[i] = new Function(file, LuaVersion.Lua53Smash, br);
                }

                SizeLineInfo = br.ReadInt32();
                // Eat line numbers
                br.ReadInt32s(SizeLineInfo);

                LocalVarsCount = br.ReadInt32();
                Locals = new Local[LocalVarsCount];
                LocalMap = new Dictionary<int, List<Local>>();
                for (var i = 0; i < LocalVarsCount; i++)
                {
                    Locals[i] = new Local(br, LuaVersion.Lua53Smash);
                    var name = Locals[i].Name;
                    if (name != null && !name.StartsWith("("))
                    {
                        if (!LocalMap.ContainsKey(Locals[i].Start))
                        {
                            LocalMap[Locals[i].Start] = new List<Local>();
                        }
                        LocalMap[Locals[i].Start].Add(Locals[i]);
                    }
                }

                //br.ReadUInt32(); // upval names
                UpValuesCount = br.ReadInt32();
                UpValueNames = new UpValueName[UpValuesCount];
                for (var i = 0; i < UpValuesCount; i++)
                {
                    UpValueNames[i] = new UpValueName(br, LuaVersion.Lua53Smash);
                }
            }

            public Function(LuaFile file, LuaVersion version, BinaryReaderEx br)
            {
                FunctionId = file.GetNextFunctionId();
                if (version == LuaVersion.Lua53Smash)
                {
                    ReadLua53Smash(file, br);
                }
                else if (version == LuaVersion.Lua50)
                {
                    Name = ReadLuaString(br, version);
                    LineDefined = br.ReadInt32();
                    NumUpValues = br.ReadByte();
                    NumParams = br.ReadByte();
                    IsVarArg = br.ReadByte();
                    MaxStackSize = br.ReadByte();
                    SizeLineInfo = br.ReadInt32();
                    // Eat line numbers
                    br.ReadInt32s(SizeLineInfo);
                    LocalVarsCount = br.ReadInt32();
                    Locals = new Local[LocalVarsCount];
                    LocalMap = new Dictionary<int, List<Local>>();
                    for (var i = 0; i < LocalVarsCount; i++)
                    {
                        Locals[i] = new Local(br, version);
                        var name = Locals[i].Name;
                        if (name != null && !name.StartsWith("("))
                        {
                            if (!LocalMap.ContainsKey(Locals[i].Start))
                            {
                                LocalMap[Locals[i].Start] = new List<Local>();
                            }
                            LocalMap[Locals[i].Start].Add(Locals[i]);
                        }
                    }
                    UpValuesCount = br.ReadInt32();
                    UpValueNames = new UpValueName[UpValuesCount];
                    for (var i = 0; i < UpValuesCount; i++)
                    {
                        UpValueNames[i] = new UpValueName(br, version);
                    }
                    var constantsCount = br.ReadInt32();
                    Constants = new Constant[constantsCount];
                    for (var i = 0; i < constantsCount; i++)
                    {
                        Constants[i] = new Constant(file, br, version);
                    }
                    var funcCount = br.ReadInt32();
                    ChildFunctions = new Function[funcCount];
                    for (var i = 0; i < funcCount; i++)
                    {
                        ChildFunctions[i] = new Function(file, version, br);
                    }
                    var bytecodeCount = br.ReadInt32();
                    Bytecode = br.ReadBytes(bytecodeCount * 4);
                }
                else if (version == LuaVersion.Lua51Hks)
                {
                    // Thanks @horkrux for reverse engineering this
                    br.ReadInt32();
                    NumParams = br.ReadUInt32();
                    br.ReadByte(); // Unk
                    NumSlots = br.ReadUInt32();
                    br.ReadUInt32(); // unk
                    var bytecodeCount = br.ReadInt32();
                    br.Pad(4);
                    Bytecode = br.ReadBytes(bytecodeCount * 4);
                    var constantsCount = br.ReadInt32();
                    Constants = new Constant[constantsCount];
                    for (var i = 0; i < constantsCount; i++)
                    {
                        Constants[i] = new Constant(file, br, LuaVersion.Lua51Hks);
                    }
                    br.ReadInt32(); // unk
                    br.ReadInt32(); // unk
                    LocalVarsCount = br.ReadInt32();
                    UpValuesCount = br.ReadInt32();
                    br.ReadInt32(); // line begin
                    br.ReadInt32(); // line end
                    Path = ReadLuaString(br, version);
                    Name = ReadLuaString(br, version);
                    // Eat line numbers
                    br.ReadInt32s(bytecodeCount);
                    Locals = new Local[LocalVarsCount];
                    LocalMap = new Dictionary<int, List<Local>>();
                    for (var i = 0; i < LocalVarsCount; i++)
                    {
                        Locals[i] = new Local(br, version);
                        var name = Locals[i].Name;
                        if (name != null && !name.StartsWith("("))
                        {
                            if (!LocalMap.ContainsKey(Locals[i].Start))
                            {
                                LocalMap[Locals[i].Start] = new List<Local>();
                            }
                            LocalMap[Locals[i].Start].Add(Locals[i]);
                        }
                    }
                    UpValueNames = new UpValueName[UpValuesCount];
                    for (var i = 0; i < UpValuesCount; i++)
                    {
                        UpValueNames[i] = new UpValueName(br, version);
                    }
                    var funcCount = br.ReadInt32();
                    ChildFunctions = new Function[funcCount];
                    for (var i = 0; i < funcCount; i++)
                    {
                        ChildFunctions[i] = new Function(file, version, br);
                    }
                }
            }

            public List<Local>? LocalsAt(int i)
            {
                return LocalMap.TryGetValue(i, out var at) ? at : null;
            }

            public override string? ToString()
            {
                return !string.IsNullOrEmpty(Name) ? Name : base.ToString();
            }
        }

        public readonly Header LuaHeader;
        public readonly Function MainFunction;

        public LuaFile(BinaryReaderEx br)
        {
            LuaHeader = new Header(br);
            Version = LuaHeader.LuaVersion;
            if (Version == LuaVersion.Lua51Hks)
            {
                br.BigEndian = true;
                br.Position = 0xee; // lel
            }
            else if (Version == LuaVersion.Lua53Smash)
            {
                // read "upval size"
                br.ReadByte();
            }
            MainFunction = new Function(this, Version, br);
        }
    }
}
