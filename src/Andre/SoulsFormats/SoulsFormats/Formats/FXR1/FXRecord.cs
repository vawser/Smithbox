using System;

namespace SoulsFormats
{
    public partial class FXR1
    {
        /// <summary>
        /// FXR v2 (Bloodborne): the small record an action-call container can point to, where FXR v1 has ActionData.
        /// Two kinds are seen in every vanilla file (FfxSurvey --bbwalk, FX.md, FFX §19): kind 1, 48 bytes
        /// (i32 1, i32 flags 0/1/256/257, then zeros, -0.0f and 1.0f in vanilla); kind 0, 8 bytes (i32 0, i32 n). Neither
        /// holds pointers, so the bytes are kept as read; their meaning is unknown.
        /// </summary>
        public class FXRecord : XIDable
        {
            public int Kind => Data.Length >= 4 ? BitConverter.ToInt32(Data, 0) : -1;

            /// <summary>The whole record, its kind word first.</summary>
            public byte[] Data;

            public static int SizeOf(int kind) => kind switch
            {
                0 => 8,
                1 => 48,
                _ => throw new NotSupportedException($"FXR v2 record kind {kind} isn't known."),
            };

            internal void Read(BinaryReaderEx br)
            {
                Data = br.ReadBytes(SizeOf(br.GetInt32(br.Position)));
            }

            internal void Write(BinaryWriterEx bw, FxrEnvironment env)
            {
                env.RegisterOffset(bw.Position, this);
                bw.WriteBytes(Data);
            }
        }
    }
}
