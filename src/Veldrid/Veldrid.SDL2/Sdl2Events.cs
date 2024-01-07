using System.Collections.Generic;

using static Silk.NET.SDL.SdlProvider;
using Silk.NET.SDL;

namespace Veldrid.Sdl2
{
    public static class Sdl2Events
    {
        private static readonly object s_lock = new object();
        private static readonly List<SDLEventHandler> s_processors = new List<SDLEventHandler>();
        public static void Subscribe(SDLEventHandler processor)
        {
            lock (s_lock)
            {
                s_processors.Add(processor);
            }
        }

        public static void Unsubscribe(SDLEventHandler processor)
        {
            lock (s_lock)
            {
                s_processors.Remove(processor);
            }
        }

        /// <summary>
        /// Pumps the SDL2 event loop, and calls all registered event processors for each event.
        /// </summary>
        public static unsafe void ProcessEvents()
        {
            lock (s_lock)
            {
                Event ev;
                while (SDL.Value.PollEvent(&ev) == 1)
                {
                    foreach (SDLEventHandler processor in s_processors)
                    {
                        processor(ref ev);
                    }
                }
            }
        }
    }
}
