using System;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
        // Deep copies for structure editing (FFX Older Games Plan §21, F7a). Each object is written with its own
        // class table and read back, so the copy has exactly the original's fields and class versions (DES's v3 / v4
        // FXEffect, DS2's v5 …) without a hand-written clone per class.

        /// <summary>A deep copy of an action (its param list and every nested call).</summary>
        public static Action Clone(Action action, StreamFormat format) => Copy(action, format, (br, t) => new Action(br, t));

        /// <summary>A deep copy of a state (its actions and exits).</summary>
        public static State Clone(State state, StreamFormat format) => Copy(state, format, (br, t) => new State(br, t));

        /// <summary>A deep copy of an exit (its target and evaluator tree).</summary>
        public static Trigger Clone(Trigger trigger, StreamFormat format) => Copy(trigger, format, (br, t) => new Trigger(br, t));

        /// <summary>A deep copy of a param (a call's whole param list included).</summary>
        public static Param Clone(Param param, StreamFormat format) => Copy(param, format, Param.Read);

        /// <summary>A deep copy of a whole effect (a DES wrapper's variants included).</summary>
        public static FXEffect Clone(FXEffect effect, StreamFormat format) => Copy(effect, format, (br, t) => new FXEffect(br, t));

        private static T Copy<T>(T obj, StreamFormat format, Func<BinaryReaderEx, ClassTable, T> read) where T : FXSerializable
        {
            if (obj == null)
                return null;
            var classNames = new ClassTable(format);
            obj.AddClassNames(classNames);
            var bw = new BinaryWriterEx(false);
            obj.Write(bw, classNames);
            var br = new BinaryReaderEx(false, bw.FinishBytes());
            return read(br, classNames);
        }
    }
}
