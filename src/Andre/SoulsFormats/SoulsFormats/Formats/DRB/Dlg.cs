using System.Collections.Generic;
using System.ComponentModel;

namespace SoulsFormats
{
    public partial class DRB
    {
        /// <summary>
        /// A group of UI elements, which is itself a UI element.
        /// </summary>
        public class Dlg : Dlgo
        {
            /// <summary>
            /// The child elements attached to this group.
            /// </summary>
            [Browsable(false)]
            public List<Dlgo> Dlgos { get; set; }

            /// <summary>
            /// Left edge of the group.
            /// </summary>
            public short LeftEdge { get; set; }

            /// <summary>
            /// Top edge of the group.
            /// </summary>
            public short TopEdge { get; set; }

            /// <summary>
            /// Right edge of the group.
            /// </summary>
            public short RightEdge { get; set; }

            /// <summary>
            /// Bottom edge of the group.
            /// </summary>
            public short BottomEdge { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public short[] Unk30 { get; private set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public short Unk3A { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk3C { get; set; }

            /// <summary>
            /// Creates a Dlg with default values.
            /// </summary>
            public Dlg() : base()
            {
                Dlgos = new List<Dlgo>();
                Unk30 = new short[5] { -1, -1, -1, -1, -1 };
            }

            /// <summary>
            /// Creates a Dlg with the given values.
            /// </summary>
            public Dlg(string name, Shape shape, Control control) : base(name, shape, control)
            {
                Dlgos = new List<Dlgo>();
                Unk30 = new short[5] { -1, -1, -1, -1, -1 };
            }

            internal Dlg(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Shape> shapes, Dictionary<int, Control> controls, Dictionary<int, Dlgo> dlgos) : base(br, strings, shapes, controls)
            {
                int dlgoCount = br.ReadInt32();
                int dlgoOffset = br.ReadInt32();
                LeftEdge = br.ReadInt16();
                TopEdge = br.ReadInt16();
                RightEdge = br.ReadInt16();
                BottomEdge = br.ReadInt16();
                Unk30 = br.ReadInt16s(5);
                Unk3A = br.ReadInt16();
                Unk3C = br.ReadInt32();

                Dlgos = new List<Dlgo>(dlgoCount);
                for (int i = 0; i < dlgoCount; i++)
                {
                    int offset = dlgoOffset + DLGO_SIZE * i;
                    Dlgos.Add(dlgos[offset]);
                    dlgos.Remove(offset);
                }
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> shapOffsets, Queue<int> ctrlOffsets, Queue<int> dlgoOffsets)
            {
                Write(bw, stringOffsets, shapOffsets, ctrlOffsets);
                bw.WriteInt32(Dlgos.Count);
                bw.WriteInt32(dlgoOffsets.Dequeue());
                bw.WriteInt16(LeftEdge);
                bw.WriteInt16(TopEdge);
                bw.WriteInt16(RightEdge);
                bw.WriteInt16(BottomEdge);
                bw.WriteInt16s(Unk30);
                bw.WriteInt16(Unk3A);
                bw.WriteInt32(Unk3C);
            }

            /// <summary>
            /// Returns the child element with the given name, or null if not found.
            /// </summary>
            public Dlgo this[string name] => Dlgos.Find(dlgo => dlgo.Name == name);

            /// <summary>
            /// Returns the name, number of child elements, shape type, and control type of this group.
            /// </summary>
            public override string ToString()
            {
                return $"{Name} ({Control.Type} {Shape.Type} [{Dlgos.Count}])";
            }
        }
    }
}
