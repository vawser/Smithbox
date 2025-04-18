using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HKX2
{
    public class PackFileDeserializer
    {
        internal HKXHeader _header;
        internal HKXSection _classSection;
        internal HKXSection _typeSection;
        internal HKXSection _dataSection;

        internal HKXClassNames _classnames;

        internal HKXVariation _variation;

        internal Dictionary<uint, IHavokObject> _deserializedObjects;

        public List<T> ReadClassArray<T>(BinaryReaderEx br) where T : IHavokObject, new()
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            var res = new List<T>();
            if (size > 0)
            {
                // Do a local fixup lookup
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    var cls = new T();
                    cls.Read(this, br);
                    res.Add(cls);
                }
                br.StepOut();
            }
            return res;
        }

        public T ReadClassPointer<T>(BinaryReaderEx br) where T : IHavokObject
        {
            // Do a global fixup lookup
            if (!_dataSection._globalMap.ContainsKey((uint)br.Position))
            {
                br.AssertUInt64(0);
                return default(T);
            }
            var f = _dataSection._globalMap[(uint)br.Position];
            // Consume pointer
            br.AssertUInt64(0);
            return (T)ConstructVirtualClass(br, f.Dst);
        }

        public List<T> ReadClassPointerArray<T>(BinaryReaderEx br) where T : IHavokObject
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            // Do a local fixup lookup
            var res = new List<T>();
            if (size > 0)
            {
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    res.Add(ReadClassPointer<T>(br));
                }
                br.StepOut();
            }
            return res;
        }

        public string ReadStringPointer(BinaryReaderEx br)
        {
            // Do a local fixup lookup
            if (!_dataSection._localMap.ContainsKey((uint)br.Position))
            {
                br.AssertUInt64(0);
                return null;
            }
            var f = _dataSection._localMap[(uint)br.Position];

            // Consume pointer
            br.AssertUInt64(0);
            br.StepIn(f.Dst);
            var ret = br.ReadASCII();
            br.StepOut();
            return ret;
        }

        public List<string> ReadStringPointerArray(BinaryReaderEx br)
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            var res = new List<string>();
            if (size > 0)
            {
                // Do a local fixup lookup
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    res.Add(ReadStringPointer(br));
                }
                br.StepOut();
            }
            return res;
        }

        public List<byte> ReadByteArray(BinaryReaderEx br)
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            var res = new List<byte>();
            if (size > 0)
            {
                // Do a local fixup lookup
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    res.Add(br.ReadByte());
                }
                br.StepOut();
            }
            return res;
        }

        public List<sbyte> ReadSByteArray(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public List<ushort> ReadUInt16Array(BinaryReaderEx br)
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            var res = new List<ushort>();
            if (size > 0)
            {
                // Do a local fixup lookup
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    res.Add(br.ReadUInt16());
                }
                br.StepOut();
            }
            return res;
        }

        public List<short> ReadInt16Array(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public List<uint> ReadUInt32Array(BinaryReaderEx br)
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            var res = new List<uint>();
            if (size > 0)
            {
                // Do a local fixup lookup
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    res.Add(br.ReadUInt32());
                }
                br.StepOut();
            }
            return res;
        }

        public List<int> ReadInt32Array(BinaryReaderEx br)
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            var res = new List<int>();
            if (size > 0)
            {
                // Do a local fixup lookup
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    res.Add(br.ReadInt32());
                }
                br.StepOut();
            }
            return res;
        }

        public List<ulong> ReadUInt64Array(BinaryReaderEx br)
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            var res = new List<ulong>();
            if (size > 0)
            {
                // Do a local fixup lookup
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    res.Add(br.ReadUInt64());
                }
                br.StepOut();
            }
            return res;
        }

        public List<long> ReadInt64Array(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public List<float> ReadSingleArray(BinaryReaderEx br)
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            var res = new List<float>();
            if (size > 0)
            {
                // Do a local fixup lookup
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    res.Add(br.ReadSingle());
                }
                br.StepOut();
            }
            return res;
        }

        public List<bool> ReadBooleanArray(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public Vector4 ReadVector4(BinaryReaderEx br)
        {
            return br.ReadVector4();
        }

        public List<Vector4> ReadVector4Array(BinaryReaderEx br)
        {
            // Consume pointer
            br.AssertUInt64(0);
            uint size = br.ReadUInt32();
            br.ReadUInt32(); // Capacity and flags
            var res = new List<Vector4>();
            if (size > 0)
            {
                // Do a local fixup lookup
                var f = _dataSection._localMap[(uint)br.Position - 16];
                br.StepIn(f.Dst);
                for (int i = 0; i < size; i++)
                {
                    res.Add(ReadVector4(br));
                }
                br.StepOut();
            }
            return res;
        }

        public Matrix4x4 ReadMatrix3(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public List<Matrix4x4> ReadMatrix3Array(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public Matrix4x4 ReadMatrix4(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public List<Matrix4x4> ReadMatrix4Array(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public Matrix4x4 ReadTransform(BinaryReaderEx br)
        {
            // TODO do a proper implementation
            return new Matrix4x4(
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle(),
                br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public List<Matrix4x4> ReadTransformArray(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public Matrix4x4 ReadQSTransform(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public List<Matrix4x4> ReadQSTransformArray(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public Quaternion ReadQuaternion(BinaryReaderEx br)
        {
            return new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        }

        public List<Quaternion> ReadQuaternionArray(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        public IHavokObject ConstructVirtualClass(BinaryReaderEx br, uint offset)
        {
            if (_deserializedObjects.ContainsKey(offset))
            {
                return _deserializedObjects[offset];
            }
            var fixup = _dataSection._virtualMap[offset];
            var cname = _classnames.OffsetClassNamesMap[fixup.NameOffset].ClassName;
            Type klasst = Type.GetType($@"HKX2.{cname}");
            var ret = (IHavokObject)Activator.CreateInstance(klasst);
            br.StepIn(offset);
            ret.Read(this, br);
            br.StepOut();
            _deserializedObjects.Add(offset, ret);
            return ret;
        }

        public IHavokObject Deserialize(BinaryReaderEx br)
        {
            br.BigEndian = false;

            // Peek ahead and read the endian byte
            br.StepIn(0x11);
            br.BigEndian = (br.ReadByte() == 0x0) ? true : false;
            br.StepOut();

            // Read header
            _header = new HKXHeader();
            _header.Magic0 = br.AssertUInt32(0x57E0E057);
            _header.Magic1 = br.AssertUInt32(0x10C0C010);
            //Header.UserTag = br.AssertInt32(0);
            _header.UserTag = br.ReadInt32();
            _header.Version = br.AssertInt32([0x05, 0x08, 0x0B]);
            if (_header.Version == 0x05)
            {
                _variation = HKXVariation.HKXDeS;
            }
            else if (_header.Version == 0x08)
            {
                _variation = HKXVariation.HKXDS1;
            }
            else
            {
                _variation = HKXVariation.HKXDS3;
            }
            _header.PointerSize = br.AssertByte([4, 8]);
            _header.Endian = br.AssertByte([0, 1]);
            _header.PaddingOption = br.AssertByte([0, 1]);
            _header.BaseClass = br.AssertByte(1); // ?
            _header.SectionCount = br.AssertInt32(3); // Always 3 sections pretty sure
            _header.ContentsSectionIndex = br.ReadInt32();
            _header.ContentsSectionOffset = br.ReadInt32();
            _header.ContentsClassNameSectionIndex = br.ReadInt32();
            _header.ContentsClassNameSectionOffset = br.ReadInt32();
            _header.ContentsVersionString = br.ReadFixStr(16); // Should be hk_2014.1.0-r1
            _header.Flags = br.ReadInt32();

            // Later versions of Havok have an extended header
            if (_header.Version >= 0x0B)
            {
                _header.Unk3C = br.ReadInt16();
                _header.SectionOffset = br.ReadInt16();
                if (_header.SectionOffset > 0)
                {
                    _header.Unk40 = br.ReadUInt32();
                    _header.Unk44 = br.ReadUInt32();
                    _header.Unk48 = br.ReadUInt32();
                    _header.Unk4C = br.ReadUInt32();
                }

                // Read the 3 sections in the file
                br.Position = _header.SectionOffset + 0x40;
            }
            else
            {
                // Just padding
                br.AssertUInt32(0xFFFFFFFF);
            }

            _classSection = new HKXSection(br, _variation);
            _classSection.SectionID = 0;
            _typeSection = new HKXSection(br, _variation);
            _typeSection.SectionID = 1;
            _dataSection = new HKXSection(br, _variation);
            _dataSection.SectionID = 2;

            // Process the class names
            _classnames = _classSection.ReadClassnames(this);

            // Deserialize the objects
            _deserializedObjects = new Dictionary<uint, IHavokObject>();
            BinaryReaderEx br2 = new BinaryReaderEx((_header.Endian == 0) ? true : false, _dataSection.SectionData);
            var root = ConstructVirtualClass(br2, 0);
            return root;
        }
    }
}
