namespace Hexa.NET.ImGui.Widgets
{
    using System;
    using System.Diagnostics;

    internal static class TryUtils
    {
        public static TryCapture<T> Try<T>(this T data, Action<T> action)
        {
            try
            {
                action(data);
            }
            catch (Exception ex)
            {
                return new TryCapture<T>(data, ex);
            }

            return default;
        }

        public static TryCapture<T, TReturn> TryReturn<T, TReturn>(this T data, Func<T, TReturn> action)
        {
            try
            {
                action(data);
            }
            catch (Exception ex)
            {
                return new(data, default, ex);
            }

            return new(data, default, null);
        }

        public static TryCapture<T> Try<T>(Action<T> action, T data)
        {
            try
            {
                action(data);
            }
            catch (Exception ex)
            {
                return new TryCapture<T>(data, ex);
            }

            return default;
        }

        public readonly struct TryCapture<T>
        {
            private readonly T value;
            private readonly Exception? exception;

            public TryCapture(T value, Exception? exception)
            {
                this.value = value;
                this.exception = exception;
            }

            public readonly void Catch(Action<T, Exception> callback)
            {
                if (exception == null) return;
                callback(value, exception);
            }

            public readonly bool Failed => exception != null;
            public readonly bool Success => exception == null;
        }

        public readonly struct TryCapture<T, TReturn>
        {
            private readonly T value;
            private readonly TReturn @return;
            private readonly Exception? exception;

            public TryCapture(T value, TReturn @return, Exception? exception)
            {
                this.value = value;
                this.@return = @return;
                this.exception = exception;
            }

            public readonly TryCapture<T, TReturn> Catch(Action<T, Exception> callback)
            {
                if (exception == null) return this;
                callback(value, exception);
                return this;
            }

            public readonly TryCapture<T, TReturn> LogFail(string message)
            {
                if (exception == null) return this;
#if DEBUG
                Trace.WriteLine(message);
#endif
                return this;
            }

            public readonly bool Failed => exception != null;

            public readonly bool Success => exception == null;

            public static implicit operator TReturn(TryCapture<T, TReturn> capture) => capture.@return;
        }
    }
}