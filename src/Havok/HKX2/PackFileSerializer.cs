using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace HKX2
{
    public class PackFileSerializer
    {
        private void WriteArrayBase<T>(BinaryWriterEx bw, IList<T> l, Action<T> perElement, bool pad=false)
        {
            uint size = (l != null) ? (uint)l.Count : 0;
            bw.WriteUInt64(0);
            bw.WriteUInt32(size);
            bw.WriteUInt32(size | (((uint)0x80) << 24));
            if (size > 0)
            {
                LocalFixup lfu = new LocalFixup();
                lfu.Src = (uint)bw.Position - 16;
                _localFixups.Add(lfu);
                _localWriteQueues[_currWriteQueue].Enqueue(() =>
                {
                    lfu.Dst = (uint)bw.Position;
                    // This ensures any writes the array elements may have are top priority
                    PushWriteQueue();
                    for (int i = 0; i < size; i++)
                    {
                        perElement.Invoke(l[i]);
                    }
                    PopWriteQueue(bw);
                });
                if (pad)
                {
                    //PushWriteQueue();
                    _localWriteQueues[_currWriteQueue].Enqueue(() =>
                    {
                        bw.Pad(16);
                    });
                    //PopWriteQueue(bw);
                }
            }
        }

        public void WriteClassArray<T>(BinaryWriterEx bw, List<T> d) where T : IHavokObject
        {
            WriteArrayBase(bw, d, (e) =>
            {
                e.Write(this, bw);
            }, true);
        }

        public void WriteClassPointer<T>(BinaryWriterEx bw, T d) where T : IHavokObject
        {
            if (d == null)
            {
                bw.WriteUInt64(0);
                return;
            }

            // If we're referencing an already serialized object, add a global ref
            if (_globalLookup.ContainsKey(d))
            {
                GlobalFixup gfu = new GlobalFixup();
                gfu.Src = (uint)bw.Position;
                gfu.DstSectionIndex = 2;
                gfu.Dst = _globalLookup[d];
                _globalFixups.Add(gfu);
            }
            // Otherwise need to add a pending reference and mark the object for serialization
            else
            {
                if (!_pendingGlobals.ContainsKey(d))
                {
                    _pendingGlobals.Add(d, new List<uint>());
                    PushSerializationQueue();
                    _serializationQueues[_currSerializationQueue].Enqueue(d);
                    PopSerializationQueue(bw);
                    _pendingVirtuals.Add(d);
                }
                _pendingGlobals[d].Add((uint)bw.Position);
            }
            bw.WriteUInt64(0);
        }

        public void WriteClassPointerArray<T>(BinaryWriterEx bw, List<T> d) where T : IHavokObject
        {
            WriteArrayBase(bw, d, (e) =>
            {
                WriteClassPointer(bw, e);
            });
        }

        public void WriteStringPointer(BinaryWriterEx bw, string d)
        {
            bw.WriteUInt64(0);
            if (d != null)
            {
                var lfu = new LocalFixup();
                lfu.Src = (uint)bw.Position - 8;
                _localFixups.Add(lfu);
                _localWriteQueues[_currWriteQueue].Enqueue(() =>
                {
                    lfu.Dst = (uint)bw.Position;
                    bw.WriteASCII(d, true);
                    bw.Pad(16);
                });
            }
        }

        public void WriteStringPointerArray(BinaryWriterEx bw, List<string> d)
        {
            throw new NotImplementedException();
        }

        public void WriteByteArray(BinaryWriterEx bw, List<byte> d)
        {
            WriteArrayBase(bw, d, (e) =>
            {
                bw.WriteByte(e);
            });
        }

        public void WriteSByteArray(BinaryWriterEx bw, List<sbyte> d)
        {
            throw new NotImplementedException();
        }

        public void WriteUInt16Array(BinaryWriterEx bw, List<ushort> d)
        {
            WriteArrayBase(bw, d, (e) =>
            {
                bw.WriteUInt16(e);
            });
        }

        public void WriteInt16Array(BinaryWriterEx bw, List<short> d)
        {
            throw new NotImplementedException();
        }

        public void WriteUInt32Array(BinaryWriterEx bw, List<uint> d)
        {
            WriteArrayBase(bw, d, (e) =>
            {
                bw.WriteUInt32(e);
            });
        }

        public void WriteInt32Array(BinaryWriterEx bw, List<int> d)
        {
            WriteArrayBase(bw, d, (e) =>
            {
                bw.WriteInt32(e);
            });
        }

        public void WriteUInt64Array(BinaryWriterEx bw, List<ulong> d)
        {
            WriteArrayBase(bw, d, (e) =>
            {
                bw.WriteUInt64(e);
            });
        }

        public void WriteInt64Array(BinaryWriterEx bw, List<long> d)
        {
            throw new NotImplementedException();
        }

        public void WriteSingleArray(BinaryWriterEx bw, List<float> d)
        {
            throw new NotImplementedException();
        }

        public void WriteBooleanArray(BinaryWriterEx bw, List<bool> d)
        {
            throw new NotImplementedException();
        }

        public void WriteVector4(BinaryWriterEx bw, Vector4 d)
        {
            bw.WriteVector4(d);
        }

        public void WriteVector4Array(BinaryWriterEx bw, List<Vector4> d)
        {
            WriteArrayBase(bw, d, (e) =>
            {
                bw.WriteVector4(e);
            });
        }

        public void WriteMatrix3(BinaryWriterEx bw, Matrix4x4 d)
        {
            throw new NotImplementedException();
        }

        public void WriteMatrix3Array(BinaryWriterEx bw, List<Matrix4x4> d)
        {
            throw new NotImplementedException();
        }

        public void WriteMatrix4(BinaryWriterEx bw, Matrix4x4 d)
        {
            throw new NotImplementedException();
        }

        public void WriteMatrix4Array(BinaryWriterEx bw, List<Matrix4x4> d)
        {
            throw new NotImplementedException();
        }

        public void WriteTransform(BinaryWriterEx bw, Matrix4x4 d)
        {
            throw new NotImplementedException();
        }

        public void WriteTransformArray(BinaryWriterEx bw, List<Matrix4x4> d)
        {
            throw new NotImplementedException();
        }

        public void WriteQSTransform(BinaryWriterEx bw, Matrix4x4 d)
        {
            throw new NotImplementedException();
        }

        public void WriteQSTransformArray(BinaryWriterEx bw, List<Matrix4x4> d)
        {
            throw new NotImplementedException();
        }

        public void WriteQuaternion(BinaryWriterEx bw, Quaternion d)
        {
            bw.WriteSingle(d.X);
            bw.WriteSingle(d.Y);
            bw.WriteSingle(d.Z);
            bw.WriteSingle(d.W);
        }

        public void WriteQuaternionArray(BinaryWriterEx bw, List<Quaternion> d)
        {
            throw new NotImplementedException();
        }

        //private Queue<IHavokObject> _serializationQueue = new Queue<IHavokObject>();
        private int _currSerializationQueue = 0;
        private List<Queue<IHavokObject>> _serializationQueues = new List<Queue<IHavokObject>>();

        private HashSet<IHavokObject> _serializedObjects = new HashSet<IHavokObject>();

        private HashSet<IHavokObject> _pendingVirtuals = new HashSet<IHavokObject>();
        private Dictionary<string, uint> _virtualTableLookup = new Dictionary<string, uint>();
        private List<VirtualFixup> _virtualFixups = new List<VirtualFixup>();

        private Dictionary<IHavokObject, List<uint>> _pendingGlobals = new Dictionary<IHavokObject, List<uint>>();
        private Dictionary<IHavokObject, uint> _globalLookup = new Dictionary<IHavokObject, uint>();
        private List<GlobalFixup> _globalFixups = new List<GlobalFixup>();

        private int _currWriteQueue = 0;
        private List<Queue<Action>> _localWriteQueues = new List<Queue<Action>>();
        private List<LocalFixup> _localFixups = new List<LocalFixup>();


        internal void PushWriteQueue()
        {
            _currWriteQueue++;
            if (_currWriteQueue == _localWriteQueues.Count)
            {
                _localWriteQueues.Add(new Queue<Action>());
            }
        }

        internal void PopWriteQueue(BinaryWriterEx bw)
        {
            // Enqueue a padding operation
            /*_localWriteQueues[_currWriteQueue].Enqueue(() =>
            {
                bw.Pad(16);
            });*/
            _currWriteQueue--;
        }

        internal void PushSerializationQueue()
        {
            _currSerializationQueue++;
            if (_currSerializationQueue == _serializationQueues.Count)
            {
                _serializationQueues.Add(new Queue<IHavokObject>());
            }
        }

        internal void PopSerializationQueue(BinaryWriterEx bw)
        {
            // Enqueue a padding operation
            /*_localWriteQueues[_currWriteQueue].Enqueue(() =>
            {
                bw.Pad(16);
            });*/
            _currSerializationQueue--;
        }

        public void Serialize(IHavokObject rootObject, BinaryWriterEx bw)
        {
            // Hardcoded for DS3 for now
            bw.BigEndian = false;
            bw.WriteUInt32(0x57E0E057); // magic 1
            bw.WriteUInt32(0x10C0C010); // magic 2
            bw.WriteInt32(0); // User tag
            bw.WriteInt32(0x0B); // DS3 havok
            bw.WriteByte(8); // Pointer size
            bw.WriteByte(1); // Little endian
            bw.WriteByte(0); // Padding option
            bw.WriteByte(1); // Base class?
            bw.WriteInt32(3); // Always 3 sections

            // Hardcoded content stuff for now?
            bw.WriteInt32(2); // Content section index
            bw.WriteInt32(0); // Content section offset
            bw.WriteInt32(0); // Content class name section index
            bw.WriteInt32(0x4B); // Content class name section offset
            bw.WriteFixStr("hk_2014.1.0-r1", 16, 0xFF); // Version string
            bw.WriteInt32(0); // Flags
            bw.WriteInt16(21); // Unk
            bw.WriteInt16(16); // section offset
            bw.WriteInt32(20); // Unk
            bw.WriteInt32(0); // Unk
            bw.WriteInt32(0); // Unk
            bw.WriteInt32(0); // Unk

            // Initialize bookkeeping structures
            //_serializationQueue = new Queue<IHavokObject>();
            //_serializationQueue.Enqueue(rootObject);
            _serializationQueues = new List<Queue<IHavokObject>>();
            _serializationQueues.Add(new Queue<IHavokObject>());
            _serializationQueues[0].Enqueue(rootObject);
            _serializedObjects = new HashSet<IHavokObject>();
            _pendingVirtuals = new HashSet<IHavokObject>();
            _pendingVirtuals.Add(rootObject);
            _virtualTableLookup = new Dictionary<string, uint>();
            _virtualFixups = new List<VirtualFixup>();
            _localWriteQueues = new List<Queue<Action>>();
            _localWriteQueues.Add(new Queue<Action>());
            _localFixups = new List<LocalFixup>();

            // Memory stream for writing all the class definitions
            MemoryStream classms = new MemoryStream();
            BinaryWriterEx classbw = new BinaryWriterEx(false, classms);

            // Data memory stream for havok objects
            MemoryStream datams = new MemoryStream();
            BinaryWriterEx databw = new BinaryWriterEx(false, datams);

            // Populate class names with some stuff havok always has
            HKXClassName hkclass = new HKXClassName();
            hkclass.ClassName = "hkClass";
            hkclass.Signature = 0x33D42383;
            hkclass.Write(classbw);
            hkclass.ClassName = "hkClassMember";
            hkclass.Signature = 0xB0EFA719;
            hkclass.Write(classbw);
            hkclass.ClassName = "hkClassEnum";
            hkclass.Signature = 0x8A3609CF;
            hkclass.Write(classbw);
            hkclass.ClassName = "hkClassEnumItem";
            hkclass.Signature = 0xCe6F8A6C;
            hkclass.Write(classbw);

            while (_serializationQueues.Count > 1 || _serializationQueues[0].Count > 0)
            {
                var sq = _serializationQueues.Last();
                while (sq != null && sq.Count() == 0 && _serializationQueues.Count > 1)
                {
                    if (_serializationQueues.Count > 1)
                    {
                        _serializationQueues.RemoveAt(_serializationQueues.Count - 1);
                    }
                    sq = _serializationQueues.Last();
                }
                if (sq.Count == 0)
                {
                    continue;
                }
                var obj = sq.Dequeue();
                _currSerializationQueue = _serializationQueues.Count - 1;

                if (_serializedObjects.Contains(obj))
                {
                    continue;
                }
                // See if we need to add virtual bookkeeping
                if (_pendingVirtuals.Contains(obj))
                {
                    _pendingVirtuals.Remove(obj);
                    var classname = obj.GetType().Name;
                    if (!_virtualTableLookup.ContainsKey(classname))
                    {
                        // Need to create a new class name entry and record the position
                        HKXClassName cname = new HKXClassName();
                        cname.ClassName = classname;
                        cname.Signature = obj.Signature;
                        uint offset = (uint)classbw.Position;
                        cname.Write(classbw);
                        _virtualTableLookup.Add(classname, offset + 5);
                    }
                    // Create a new Virtual fixup for this object
                    var vfu = new VirtualFixup();
                    vfu.Src = (uint)databw.Position;
                    vfu.SectionIndex = 0;
                    vfu.NameOffset = _virtualTableLookup[classname];
                    _virtualFixups.Add(vfu);

                    // See if we have any pending global references to this object
                    if (_pendingGlobals.ContainsKey(obj))
                    {
                        // If so, create all the needed global fixups
                        foreach (var src in _pendingGlobals[obj])
                        {
                            var gfu = new GlobalFixup();
                            gfu.Src = src;
                            gfu.Dst = (uint)databw.Position;
                            gfu.DstSectionIndex = 2;
                            _globalFixups.Add(gfu);
                        }
                        _pendingGlobals.Remove(obj);
                    }

                    // Add global lookup
                    _globalLookup.Add(obj, (uint)databw.Position);
                }
                obj.Write(this, databw);
                _serializedObjects.Add(obj);
                databw.Pad(16);

                // Write local data (such as array contents and strings)
                while (_localWriteQueues.Count > 1 || _localWriteQueues[0].Count > 0)
                {
                    var q = _localWriteQueues.Last();
                    while (q != null && q.Count() == 0 && _localWriteQueues.Count > 1)
                    {
                        if (_localWriteQueues.Count > 1)
                        {
                            _localWriteQueues.RemoveAt(_localWriteQueues.Count - 1);
                        }
                        q = _localWriteQueues.Last();

                        // Do alignment at the popping of a queue frame
                        databw.Pad(16);
                    }
                    if (q.Count == 0)
                    {
                        continue;
                    }
                    var act = q.Dequeue();
                    _currWriteQueue = _localWriteQueues.Count - 1;
                    act.Invoke();
                }
                databw.Pad(16);
            }

            HKXSection classNames = new HKXSection();
            classNames.SectionID = 0;
            classNames.SectionTag = "__classnames__";
            classNames.SectionData = classms.ToArray();
            classNames.WriteHeader(bw, HKX.HKXVariation.HKXDS3);

            HKXSection types = new HKXSection();
            types.SectionID = 1;
            types.SectionTag = "__types__";
            types.SectionData = new byte[0];
            types.WriteHeader(bw, HKX.HKXVariation.HKXDS3);

            HKXSection data = new HKXSection();
            data.SectionID = 2;
            data.SectionTag = "__data__";
            data.SectionData = datams.ToArray();
            data.VirtualFixups = _virtualFixups;
            data.GlobalFixups = _globalFixups.OrderBy((x) => x.Src).ToList();
            data.LocalFixups = _localFixups.OrderBy((x) => x.Dst).ToList();
            data.WriteHeader(bw, HKX.HKXVariation.HKXDS3);

            classNames.WriteData(bw);
            types.WriteData(bw);
            data.WriteData(bw);

            classms.Close();
            datams.Close();
        }
    }
}
