using System.Collections.Generic;
using System.ComponentModel;

namespace SoulsFormats
{
    public partial class DRB
    {
        /// <summary>
        /// An individual UI element.
        /// </summary>
        public class Dlgo
        {
            /// <summary>
            /// The name of the element.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The visual properties of the element.
            /// </summary>
            [Browsable(false)]
            public Shape Shape { get; set; }

            /// <summary>
            /// The behavior of the element.
            /// </summary>
            public Control Control { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk18 { get; set; }

            /// <summary>
            /// Unknown; always 0.
            /// </summary>
            public int Unk1C { get; set; }

            /// <summary>
            /// Creates a Dlgo with default values.
            /// </summary>
            public Dlgo()
            {
                Name = "";
                Shape = new Shape.Null();
                Control = new Control.Static();
            }

            /// <summary>
            /// Creates a Dlgo with the given values and a default control.
            /// </summary>
            public Dlgo(string name, Shape shape)
            {
                Name = name;
                Shape = shape;
                if (shape is Shape.ScrollText)
                    Control = new Control.ScrollTextDummy();
                else
                    Control = new Control.Static();
            }

            /// <summary>
            /// Creates a Dlgo with the given values.
            /// </summary>
            public Dlgo(string name, Shape shape, Control control)
            {
                Name = name;
                Shape = shape;
                Control = control;
            }

            internal Dlgo(BinaryReaderEx br, Dictionary<int, string> strings, Dictionary<int, Shape> shapes, Dictionary<int, Control> controls)
            {
                int nameOffset = br.ReadInt32();
                int shapOffset = br.ReadInt32();
                int ctrlOffset = br.ReadInt32();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Unk18 = br.ReadInt32();
                Unk1C = br.ReadInt32();

                Name = strings[nameOffset];
                Shape = shapes[shapOffset];
                shapes.Remove(shapOffset);
                Control = controls[ctrlOffset];
                controls.Remove(ctrlOffset);
            }

            internal void Write(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> shapOffsets, Queue<int> ctrlOffsets)
            {
                bw.WriteInt32(stringOffsets[Name]);
                bw.WriteInt32(shapOffsets.Dequeue());
                bw.WriteInt32(ctrlOffsets.Dequeue());
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);
                bw.WriteInt32(Unk18);
                bw.WriteInt32(Unk1C);
            }

            /// <summary>
            /// Returns the name, shape type, and control type of the element.
            /// </summary>
            public override string ToString()
            {
                return $"{Name} ({Control.Type} {Shape.Type})";
            }
        }
    }
}
