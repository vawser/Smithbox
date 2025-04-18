using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Silk.NET.SDL;

namespace Veldrid.Sdl2
{
    internal static class Sdl2WindowRegistry
    {
        public static readonly object Lock = new object();
        private static readonly Dictionary<uint, Sdl2Window> _eventsByWindowID
            = new Dictionary<uint, Sdl2Window>();
        private static bool _firstInit;

        public static void RegisterWindow(Sdl2Window window)
        {
            lock (Lock)
            {
                _eventsByWindowID.Add(window.WindowID, window);
                if (!_firstInit)
                {
                    _firstInit = true;
                    Sdl2Events.Subscribe(ProcessWindowEvent);
                }
            }
        }

        public static void RemoveWindow(Sdl2Window window)
        {
            lock (Lock)
            {
                _eventsByWindowID.Remove(window.WindowID);
            }
        }

        private static unsafe void ProcessWindowEvent(ref Event ev)
        {
            bool handled = false;
            uint windowID = 0;
            switch ((EventType)ev.Type)
            {
                case EventType.Quit:
                case EventType.AppTerminating:
                case EventType.Windowevent:
                    windowID = ev.Window.WindowID;
                    handled = true;
                    break;
                case EventType.Keydown:
                case EventType.Keyup:
                    windowID = ev.Key.WindowID;
                    handled = true;
                    break;
                case EventType.Textediting:
                    windowID = ev.Edit.WindowID;
                    handled = true;
                    break;
                case EventType.Textinput:
                    windowID = ev.Text.WindowID;
                    handled = true;
                    break;
                case EventType.Keymapchanged:
                    windowID = ev.Key.WindowID;
                    handled = true;
                    break;
                case EventType.Mousemotion:
                    windowID = ev.Motion.WindowID;
                    handled = true;
                    break;
                case EventType.Mousebuttondown:
                case EventType.Mousebuttonup:
                    windowID = ev.Button.WindowID;
                    handled = true;
                    break;
                case EventType.Mousewheel:
                    windowID = ev.Wheel.WindowID;
                    handled = true;
                    break;
                case EventType.Dropbegin:
                case EventType.Dropcomplete:
                case EventType.Dropfile:
                case EventType.Droptext:
                    windowID = ev.Drop.WindowID;
                    handled = true;
                    break;
                default:
                    handled = false;
                    break;
            }


            if (handled && _eventsByWindowID.TryGetValue(windowID, out Sdl2Window window))
            {
                window.AddEvent(ev);
            }
        }
    }
}
