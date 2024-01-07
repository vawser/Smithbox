using System.Collections.Generic;
using System.ComponentModel;
using System.IO;

namespace SoulsFormats
{
    public partial class DRB
    {
        /// <summary>
        /// Indicates the behavior of a UI element.
        /// </summary>
        public enum ControlType
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            DmeCtrlScrollText,

            /// <summary>
            /// Unknown.
            /// </summary>
            FrpgMenuDlgObjContentsHelpItem,

            /// <summary>
            /// Unknown.
            /// </summary>
            Static
        }

        /// <summary>
        /// Determines the behavior of a UI element.
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public abstract class Control
        {
            /// <summary>
            /// The type of this control.
            /// </summary>
            public abstract ControlType Type { get; }

            internal static Control Read(BinaryReaderEx br, Dictionary<int, string> strings, long ctprStart)
            {
                int typeOffset = br.ReadInt32();
                int ctprOffset = br.ReadInt32();
                string type = strings[typeOffset];

                Control result;
                br.StepIn(ctprStart + ctprOffset);
                {
                    if (type == "DmeCtrlScrollText")
                        result = new ScrollTextDummy(br);
                    else if (type == "FrpgMenuDlgObjContentsHelpItem")
                        result = new HelpItem(br);
                    else if (type == "Static")
                        result = new Static(br);
                    else
                        throw new InvalidDataException($"Unknown control type: {type}");
                }
                br.StepOut();
                return result;
            }

            internal abstract void WriteData(BinaryWriterEx bw);

            internal void WriteHeader(BinaryWriterEx bw, Dictionary<string, int> stringOffsets, Queue<int> ctprOffsets)
            {
                bw.WriteInt32(stringOffsets[Type.ToString()]);
                bw.WriteInt32(ctprOffsets.Dequeue());
            }

            /// <summary>
            /// Returns the type of the control.
            /// </summary>
            public override string ToString()
            {
                return $"{Type}";
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class ScrollTextDummy : Control
            {
                /// <summary>
                /// ControlType.DmeCtrlScrollText
                /// </summary>
                public override ControlType Type => ControlType.DmeCtrlScrollText;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Creates a ScrollTextDummy with default values.
                /// </summary>
                public ScrollTextDummy() : base() { }

                internal ScrollTextDummy(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                internal override void WriteData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class HelpItem : Control
            {
                /// <summary>
                /// ControlType.FrpgMenuDlgObjContentsHelpItem
                /// </summary>
                public override ControlType Type => ControlType.FrpgMenuDlgObjContentsHelpItem;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// An FMG ID.
                /// </summary>
                public int TextID { get; set; }

                /// <summary>
                /// Creates a HelpItem with default values.
                /// </summary>
                public HelpItem() : base()
                {
                    TextID = -1;
                }

                internal HelpItem(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    TextID = br.ReadInt32();
                }

                internal override void WriteData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(TextID);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Static : Control
            {
                /// <summary>
                /// ControlType.Static
                /// </summary>
                public override ControlType Type => ControlType.Static;

                /// <summary>
                /// Unknown; always 0.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Creates a Static with default values.
                /// </summary>
                public Static() : base() { }

                internal Static(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                internal override void WriteData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                }
            }
        }
    }
}
