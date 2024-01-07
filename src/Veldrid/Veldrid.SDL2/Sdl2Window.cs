using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;

using System.ComponentModel;
using Silk.NET.SDL;
using Veldrid;
using Thread = System.Threading.Thread;

namespace Veldrid.Sdl2
{
    public unsafe class Sdl2Window
    {
        private readonly List<Event> _events = new List<Event>();
        private Window* _window;
        internal uint WindowID { get; private set; }
        private bool _exists;

        private SimpleInputSnapshot _publicSnapshot = new SimpleInputSnapshot();
        private SimpleInputSnapshot _privateSnapshot = new SimpleInputSnapshot();
        private SimpleInputSnapshot _privateBackbuffer = new SimpleInputSnapshot();

        // Threaded Sdl2Window flags
        private readonly bool _threadedProcessing;

        private bool _shouldClose;
        public bool LimitPollRate { get; set; }
        public float PollIntervalInMs { get; set; }

        // Current input states
        private int _currentMouseX;
        private int _currentMouseY;
        private bool[] _currentMouseButtonStates = new bool[13];
        private Vector2 _currentMouseDelta;

        // Cached Sdl2Window state (for threaded processing)
        private BufferedValue<Point> _cachedPosition = new BufferedValue<Point>();
        private BufferedValue<Point> _cachedSize = new BufferedValue<Point>();
        private string _cachedWindowTitle;
        private bool _newWindowTitleReceived;
        private bool _firstMouseEvent = true;

        private Sdl SDL;

        public Sdl2Window(string title, int x, int y, int width, int height, WindowFlags flags, bool threadedProcessing)
        {
            SDL = SdlProvider.SDL.Value;
            _threadedProcessing = threadedProcessing;
            if (threadedProcessing)
            {
                using (ManualResetEvent mre = new ManualResetEvent(false))
                {
                    WindowParams wp = new WindowParams()
                    {
                        Title = title,
                        X = x,
                        Y = y,
                        Width = width,
                        Height = height,
                        WindowFlags = flags,
                        ResetEvent = mre
                    };

                    Task.Factory.StartNew(WindowOwnerRoutine, wp, TaskCreationOptions.LongRunning);
                    mre.WaitOne();
                }
            }
            else
            {
                _window = SDL.CreateWindow(title, x, y, width, height, (uint)flags);
                WindowID = SDL.GetWindowID(_window);
                Sdl2WindowRegistry.RegisterWindow(this);
                PostWindowCreated(flags);
            }
        }

        public Sdl2Window(IntPtr windowHandle, bool threadedProcessing)
        {
            _threadedProcessing = threadedProcessing;
            if (threadedProcessing)
            {
                using (ManualResetEvent mre = new ManualResetEvent(false))
                {
                    WindowParams wp = new WindowParams()
                    {
                        WindowHandle = windowHandle,
                        WindowFlags = 0,
                        ResetEvent = mre
                    };

                    Task.Factory.StartNew(WindowOwnerRoutine, wp, TaskCreationOptions.LongRunning);
                    mre.WaitOne();
                }
            }
            else
            {
                _window = SDL.CreateWindowFrom(windowHandle);
                WindowID = SDL.GetWindowID(_window);
                Sdl2WindowRegistry.RegisterWindow(this);
                PostWindowCreated(0);
            }
        }

        public int X { get => _cachedPosition.Value.X; set => SetWindowPosition(value, Y); }
        public int Y { get => _cachedPosition.Value.Y; set => SetWindowPosition(X, value); }

        public int Width { get => GetWindowSize().X; set => SetWindowSize(value, Height); }
        public int Height { get => GetWindowSize().Y; set => SetWindowSize(Width, value); }

        public IntPtr Handle => GetUnderlyingWindowHandle();

        public string Title { get => _cachedWindowTitle; set => SetWindowTitle(value); }

        private void SetWindowTitle(string value)
        {
            _cachedWindowTitle = value;
            _newWindowTitleReceived = true;
        }

        public WindowState WindowState
        {
            get
            {
                WindowFlags flags = (WindowFlags)SDL.GetWindowFlags(_window);
                if (((flags & WindowFlags.FullscreenDesktop) == WindowFlags.FullscreenDesktop)
                    || ((flags & (WindowFlags.Borderless | WindowFlags.Fullscreen)) == (WindowFlags.Borderless | WindowFlags.Fullscreen)))
                {
                    return WindowState.BorderlessFullScreen;
                }
                else if ((flags & WindowFlags.Minimized) == WindowFlags.Minimized)
                {
                    return WindowState.Minimized;
                }
                else if ((flags & WindowFlags.Fullscreen) == WindowFlags.Fullscreen)
                {
                    return WindowState.FullScreen;
                }
                else if ((flags & WindowFlags.Maximized) == WindowFlags.Maximized)
                {
                    return WindowState.Maximized;
                }
                else if ((flags & WindowFlags.Hidden) == WindowFlags.Hidden)
                {
                    return WindowState.Hidden;
                }

                return WindowState.Normal;
            }
            set
            {
                switch (value)
                {
                    case WindowState.Normal:
                        SDL.SetWindowFullscreen(_window, (uint)WindowFlags.None);
                        break;
                    case WindowState.FullScreen:
                        SDL.SetWindowFullscreen(_window, (uint)WindowFlags.Fullscreen);
                        break;
                    case WindowState.Maximized:
                        SDL.MaximizeWindow(_window);
                        break;
                    case WindowState.Minimized:
                        SDL.MinimizeWindow(_window);
                        break;
                    case WindowState.BorderlessFullScreen:
                        SDL.SetWindowFullscreen(_window, (uint)WindowFlags.FullscreenDesktop);
                        break;
                    case WindowState.Hidden:
                        SDL.HideWindow(_window);
                        break;
                    default:
                        throw new InvalidOperationException("Illegal WindowState value: " + value);
                }
            }
        }

        public bool Exists => _exists;

        public bool Visible
        {
            get => ((WindowFlags)SDL.GetWindowFlags(_window) & WindowFlags.Shown) != 0;
            set
            {
                if (value)
                {
                    SDL.ShowWindow(_window);
                }
                else
                {
                    SDL.HideWindow(_window);
                }
            }
        }

        public Vector2 ScaleFactor => Vector2.One;

        public Rectangle Bounds => new Rectangle(_cachedPosition, GetWindowSize());

        public bool CursorVisible
        {
            get
            {
                return SDL.ShowCursor(-1) == 1;
            }
            set
            {
                int toggle = value ? Sdl.Enable : Sdl.Disable;
                SDL.ShowCursor(toggle);
            }
        }

        public float Opacity
        {
            get
            {
                float opacity = float.NaN;
                if (SDL.GetWindowOpacity(_window, &opacity) == 0)
                {
                    return opacity;
                }
                return float.NaN;
            }
            set
            {
                SDL.SetWindowOpacity(_window, value);
            }
        }

        public bool Focused => ((WindowFlags)SDL.GetWindowFlags(_window) & WindowFlags.InputFocus) != 0;

        public bool Resizable
        {
            get => ((WindowFlags)SDL.GetWindowFlags(_window) & WindowFlags.Resizable) != 0;
            set => SDL.SetWindowResizable(_window, value ? SdlBool.True : SdlBool.False);
        }

        public bool BorderVisible
        {
            get => ((WindowFlags)SDL.GetWindowFlags(_window) & WindowFlags.Borderless) == 0;
            set => SDL.SetWindowBordered(_window, value ? SdlBool.True : SdlBool.False);
        }

        public Window* SdlWindowHandle => _window;

        public event Action Resized;
        public event Action Closing;
        public event Action Closed;
        public event Action FocusLost;
        public event Action FocusGained;
        public event Action Shown;
        public event Action Hidden;
        public event Action MouseEntered;
        public event Action MouseLeft;
        public event Action Exposed;
        public event Action<Point> Moved;
        public event Action<MouseWheelEventArgs> MouseWheel;
        public event Action<MouseMoveEventArgs> MouseMove;
        public event Action<MouseEvent> MouseDown;
        public event Action<MouseEvent> MouseUp;
        public event Action<KeyEvent> KeyDown;
        public event Action<KeyEvent> KeyUp;
        public event Action<DragDropEvent> DragDrop;

        public Point ClientToScreen(Point p)
        {
            Point position = _cachedPosition;
            return new Point(p.X + position.X, p.Y + position.Y);
        }

        public void SetMousePosition(Vector2 position) => SetMousePosition((int)position.X, (int)position.Y);
        public void SetMousePosition(int x, int y)
        {
            if (_exists)
            {
                SDL.WarpMouseInWindow(_window, x, y);
                _currentMouseX = x;
                _currentMouseY = y;
            }
        }

        public Vector2 MouseDelta => _currentMouseDelta;

        public void Close()
        {
            if (_threadedProcessing)
            {
                _shouldClose = true;
            }
            else
            {
                CloseCore();
            }
        }

        private void CloseCore()
        {
            Sdl2WindowRegistry.RemoveWindow(this);
            Closing?.Invoke();
            SDL.DestroyWindow(_window);
            _exists = false;
            Closed?.Invoke();
        }

        private void WindowOwnerRoutine(object state)
        {
            WindowParams wp = (WindowParams)state;
            _window = wp.Create();
            WindowID = SDL.GetWindowID(_window);
            Sdl2WindowRegistry.RegisterWindow(this);
            PostWindowCreated(wp.WindowFlags);
            wp.ResetEvent.Set();

            double previousPollTimeMs = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (_exists)
            {
                if (_shouldClose)
                {
                    CloseCore();
                    return;
                }

                double currentTick = sw.ElapsedTicks;
                double currentTimeMs = sw.ElapsedTicks * (1000.0 / Stopwatch.Frequency);
                if (LimitPollRate && currentTimeMs - previousPollTimeMs < PollIntervalInMs)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    previousPollTimeMs = currentTimeMs;
                    ProcessEvents(null);
                }
            }
        }

        private void PostWindowCreated(WindowFlags flags)
        {
            RefreshCachedPosition();
            RefreshCachedSize();
            if ((flags & WindowFlags.Shown) == WindowFlags.Shown)
            {
                SDL.ShowWindow(_window);
            }

            _exists = true;
        }

        // Called by Sdl2EventProcessor when an event for this window is encountered.
        internal void AddEvent(Event ev)
        {
            _events.Add(ev);
        }

        public InputSnapshot PumpEvents()
        {
            _currentMouseDelta = new Vector2();
            if (_threadedProcessing)
            {
                SimpleInputSnapshot snapshot = Interlocked.Exchange(ref _privateSnapshot, _privateBackbuffer);
                snapshot.CopyTo(_publicSnapshot);
                snapshot.Clear();
            }
            else
            {
                ProcessEvents(null);
                _privateSnapshot.CopyTo(_publicSnapshot);
                _privateSnapshot.Clear();
            }

            return _publicSnapshot;
        }

        private void ProcessEvents(SDLEventHandler eventHandler)
        {
            CheckNewWindowTitle();

            Sdl2Events.ProcessEvents();
            for (int i = 0; i < _events.Count; i++)
            {
                Event ev = _events[i];
                if (eventHandler == null)
                {
                    HandleEvent(&ev);
                }
                else
                {
                    eventHandler(ref ev);
                }
            }
            _events.Clear();
        }

        public void PumpEvents(SDLEventHandler eventHandler)
        {
            ProcessEvents(eventHandler);
        }

        private unsafe void HandleEvent(Event* ev)
        {
            switch ((EventType)ev->Type)
            {
                case EventType.Quit:
                    Close();
                    break;
                case EventType.AppTerminating:
                    Close();
                    break;
                case EventType.Windowevent:
                    HandleWindowEvent(ev->Window);
                    break;
                case EventType.Keydown:
                case EventType.Keyup:
                    HandleKeyboardEvent(ev->Key);
                    break;
                case EventType.Textediting:
                    break;
                case EventType.Textinput:
                    HandleTextInputEvent(ev->Text);
                    break;
                case EventType.Keymapchanged:
                    break;
                case EventType.Mousemotion:
                    HandleMouseMotionEvent(ev->Motion);
                    break;
                case EventType.Mousebuttondown:
                case EventType.Mousebuttonup:
                    HandleMouseButtonEvent(ev->Button);
                    break;
                case EventType.Mousewheel:
                    HandleMouseWheelEvent(ev->Wheel);
                    break;
                case EventType.Dropfile:
                case EventType.Dropbegin:
                case EventType.Droptext:
                    HandleDropEvent(ev->Drop);
                    break;
                default:
                    // Ignore
                    break;
            }
        }

        private void CheckNewWindowTitle()
        {
            if (WindowState != WindowState.Minimized && _newWindowTitleReceived)
            {
                _newWindowTitleReceived = false;
                SDL.SetWindowTitle(_window, _cachedWindowTitle);
            }
        }

        private void HandleTextInputEvent(TextInputEvent textInputEvent)
        {
            uint byteCount = 0;
            // Loop until the null terminator is found or the max size is reached.
            while (byteCount < 32 && textInputEvent.Text[byteCount++] != 0)
            { }

            if (byteCount > 1)
            {
                // We don't want the null terminator.
                byteCount -= 1;
                int charCount = Encoding.UTF8.GetCharCount(textInputEvent.Text, (int)byteCount);
                char* charsPtr = stackalloc char[charCount];
                Encoding.UTF8.GetChars(textInputEvent.Text, (int)byteCount, charsPtr, charCount);
                for (int i = 0; i < charCount; i++)
                {
                    _privateSnapshot.KeyCharPressesList.Add(charsPtr[i]);
                }
            }
        }

        private void HandleMouseWheelEvent(MouseWheelEvent mouseWheelEvent)
        {
            _privateSnapshot.WheelDelta += mouseWheelEvent.Y;
            MouseWheel?.Invoke(new MouseWheelEventArgs(GetCurrentMouseState(), (float)mouseWheelEvent.Y));
        }

        private void HandleDropEvent(DropEvent dropEvent)
        {
            string file = Utilities.GetString(dropEvent.File);
            SDL.Free(dropEvent.File);

            if ((EventType)dropEvent.Type == EventType.Dropfile)
            {
                DragDrop?.Invoke(new DragDropEvent(file));
            }
        }

        private void HandleMouseButtonEvent(MouseButtonEvent mouseButtonEvent)
        {
            MouseButton button = MapMouseButton(mouseButtonEvent.Button);
            bool down = mouseButtonEvent.State == 1;
            _currentMouseButtonStates[(int)button] = down;
            _privateSnapshot.MouseDown[(int)button] = down;
            MouseEvent mouseEvent = new MouseEvent(button, down);
            _privateSnapshot.MouseEventsList.Add(mouseEvent);
            if (down)
            {
                MouseDown?.Invoke(mouseEvent);
            }
            else
            {
                MouseUp?.Invoke(mouseEvent);
            }
        }

        private MouseButton MapMouseButton(int button)
        {
            switch (button)
            {
                case Sdl.ButtonLeft:
                    return MouseButton.Left;
                case Sdl.ButtonMiddle:
                    return MouseButton.Middle;
                case Sdl.ButtonRight:
                    return MouseButton.Right;
                case Sdl.ButtonX1:
                    return MouseButton.Button1;
                case Sdl.ButtonX2:
                    return MouseButton.Button2;
                default:
                    return MouseButton.Left;
            }
        }

        private void HandleMouseMotionEvent(MouseMotionEvent mouseMotionEvent)
        {
            Vector2 mousePos = new Vector2(mouseMotionEvent.X, mouseMotionEvent.Y);
            Vector2 delta = new Vector2(mouseMotionEvent.Xrel, mouseMotionEvent.Yrel);
            _currentMouseX = (int)mousePos.X;
            _currentMouseY = (int)mousePos.Y;
            _privateSnapshot.MousePosition = mousePos;

            if (!_firstMouseEvent)
            {
                _currentMouseDelta += delta;
                MouseMove?.Invoke(new MouseMoveEventArgs(GetCurrentMouseState(), mousePos));
            }

            _firstMouseEvent = false;
        }

        private void HandleKeyboardEvent(KeyboardEvent keyboardEvent)
        {
            SimpleInputSnapshot snapshot = _privateSnapshot;
            KeyEvent keyEvent = new KeyEvent(MapKey(keyboardEvent.Keysym), keyboardEvent.State == 1,
                MapModifierKeys((Keymod)keyboardEvent.Keysym.Mod));
            snapshot.KeyEventsList.Add(keyEvent);
            if (keyboardEvent.State == 1)
            {
                KeyDown?.Invoke(keyEvent);
            }
            else
            {
                KeyUp?.Invoke(keyEvent);
            }
        }

        private Key MapKey(Keysym keysym)
        {
            switch (keysym.Scancode)
            {
                case Scancode.ScancodeA:
                    return Key.A;
                case Scancode.ScancodeB:
                    return Key.B;
                case Scancode.ScancodeC:
                    return Key.C;
                case Scancode.ScancodeD:
                    return Key.D;
                case Scancode.ScancodeE:
                    return Key.E;
                case Scancode.ScancodeF:
                    return Key.F;
                case Scancode.ScancodeG:
                    return Key.G;
                case Scancode.ScancodeH:
                    return Key.H;
                case Scancode.ScancodeI:
                    return Key.I;
                case Scancode.ScancodeJ:
                    return Key.J;
                case Scancode.ScancodeK:
                    return Key.K;
                case Scancode.ScancodeL:
                    return Key.L;
                case Scancode.ScancodeM:
                    return Key.M;
                case Scancode.ScancodeN:
                    return Key.N;
                case Scancode.ScancodeO:
                    return Key.O;
                case Scancode.ScancodeP:
                    return Key.P;
                case Scancode.ScancodeQ:
                    return Key.Q;
                case Scancode.ScancodeR:
                    return Key.R;
                case Scancode.ScancodeS:
                    return Key.S;
                case Scancode.ScancodeT:
                    return Key.T;
                case Scancode.ScancodeU:
                    return Key.U;
                case Scancode.ScancodeV:
                    return Key.V;
                case Scancode.ScancodeW:
                    return Key.W;
                case Scancode.ScancodeX:
                    return Key.X;
                case Scancode.ScancodeY:
                    return Key.Y;
                case Scancode.ScancodeZ:
                    return Key.Z;
                case Scancode.Scancode1:
                    return Key.Number1;
                case Scancode.Scancode2:
                    return Key.Number2;
                case Scancode.Scancode3:
                    return Key.Number3;
                case Scancode.Scancode4:
                    return Key.Number4;
                case Scancode.Scancode5:
                    return Key.Number5;
                case Scancode.Scancode6:
                    return Key.Number6;
                case Scancode.Scancode7:
                    return Key.Number7;
                case Scancode.Scancode8:
                    return Key.Number8;
                case Scancode.Scancode9:
                    return Key.Number9;
                case Scancode.Scancode0:
                    return Key.Number0;
                case Scancode.ScancodeReturn:
                    return Key.Enter;
                case Scancode.ScancodeEscape:
                    return Key.Escape;
                case Scancode.ScancodeBackspace:
                    return Key.BackSpace;
                case Scancode.ScancodeTab:
                    return Key.Tab;
                case Scancode.ScancodeSpace:
                    return Key.Space;
                case Scancode.ScancodeMinus:
                    return Key.Minus;
                case Scancode.ScancodeEquals:
                    return Key.Plus;
                case Scancode.ScancodeLeftbracket:
                    return Key.BracketLeft;
                case Scancode.ScancodeRightbracket:
                    return Key.BracketRight;
                case Scancode.ScancodeBackslash:
                    return Key.BackSlash;
                case Scancode.ScancodeSemicolon:
                    return Key.Semicolon;
                case Scancode.ScancodeApostrophe:
                    return Key.Quote;
                case Scancode.ScancodeGrave:
                    return Key.Grave;
                case Scancode.ScancodeComma:
                    return Key.Comma;
                case Scancode.ScancodePeriod:
                    return Key.Period;
                case Scancode.ScancodeSlash:
                    return Key.Slash;
                case Scancode.ScancodeCapslock:
                    return Key.CapsLock;
                case Scancode.ScancodeF1:
                    return Key.F1;
                case Scancode.ScancodeF2:
                    return Key.F2;
                case Scancode.ScancodeF3:
                    return Key.F3;
                case Scancode.ScancodeF4:
                    return Key.F4;
                case Scancode.ScancodeF5:
                    return Key.F5;
                case Scancode.ScancodeF6:
                    return Key.F6;
                case Scancode.ScancodeF7:
                    return Key.F7;
                case Scancode.ScancodeF8:
                    return Key.F8;
                case Scancode.ScancodeF9:
                    return Key.F9;
                case Scancode.ScancodeF10:
                    return Key.F10;
                case Scancode.ScancodeF11:
                    return Key.F11;
                case Scancode.ScancodeF12:
                    return Key.F12;
                case Scancode.ScancodePrintscreen:
                    return Key.PrintScreen;
                case Scancode.ScancodeScrolllock:
                    return Key.ScrollLock;
                case Scancode.ScancodePause:
                    return Key.Pause;
                case Scancode.ScancodeInsert:
                    return Key.Insert;
                case Scancode.ScancodeHome:
                    return Key.Home;
                case Scancode.ScancodePageup:
                    return Key.PageUp;
                case Scancode.ScancodeDelete:
                    return Key.Delete;
                case Scancode.ScancodeEnd:
                    return Key.End;
                case Scancode.ScancodePagedown:
                    return Key.PageDown;
                case Scancode.ScancodeRight:
                    return Key.Right;
                case Scancode.ScancodeLeft:
                    return Key.Left;
                case Scancode.ScancodeDown:
                    return Key.Down;
                case Scancode.ScancodeUp:
                    return Key.Up;
                case Scancode.ScancodeNumlockclear:
                    return Key.NumLock;
                case Scancode.ScancodeKPDivide:
                    return Key.KeypadDivide;
                case Scancode.ScancodeKPMultiply:
                    return Key.KeypadMultiply;
                case Scancode.ScancodeKPMinus:
                    return Key.KeypadMinus;
                case Scancode.ScancodeKPPlus:
                    return Key.KeypadPlus;
                case Scancode.ScancodeKPEnter:
                    return Key.KeypadEnter;
                case Scancode.ScancodeKP1:
                    return Key.Keypad1;
                case Scancode.ScancodeKP2:
                    return Key.Keypad2;
                case Scancode.ScancodeKP3:
                    return Key.Keypad3;
                case Scancode.ScancodeKP4:
                    return Key.Keypad4;
                case Scancode.ScancodeKP5:
                    return Key.Keypad5;
                case Scancode.ScancodeKP6:
                    return Key.Keypad6;
                case Scancode.ScancodeKP7:
                    return Key.Keypad7;
                case Scancode.ScancodeKP8:
                    return Key.Keypad8;
                case Scancode.ScancodeKP9:
                    return Key.Keypad9;
                case Scancode.ScancodeKP0:
                    return Key.Keypad0;
                case Scancode.ScancodeKPPeriod:
                    return Key.KeypadPeriod;
                case Scancode.ScancodeNonusbackslash:
                    return Key.NonUSBackSlash;
                case Scancode.ScancodeKPEquals:
                    return Key.KeypadPlus;
                case Scancode.ScancodeF13:
                    return Key.F13;
                case Scancode.ScancodeF14:
                    return Key.F14;
                case Scancode.ScancodeF15:
                    return Key.F15;
                case Scancode.ScancodeF16:
                    return Key.F16;
                case Scancode.ScancodeF17:
                    return Key.F17;
                case Scancode.ScancodeF18:
                    return Key.F18;
                case Scancode.ScancodeF19:
                    return Key.F19;
                case Scancode.ScancodeF20:
                    return Key.F20;
                case Scancode.ScancodeF21:
                    return Key.F21;
                case Scancode.ScancodeF22:
                    return Key.F22;
                case Scancode.ScancodeF23:
                    return Key.F23;
                case Scancode.ScancodeF24:
                    return Key.F24;
                case Scancode.ScancodeMenu:
                    return Key.Menu;
                case Scancode.ScancodeLctrl:
                    return Key.ControlLeft;
                case Scancode.ScancodeLshift:
                    return Key.ShiftLeft;
                case Scancode.ScancodeLalt:
                    return Key.AltLeft;
                case Scancode.ScancodeRctrl:
                    return Key.ControlRight;
                case Scancode.ScancodeRshift:
                    return Key.ShiftRight;
                case Scancode.ScancodeRalt:
                    return Key.AltRight;
                default:
                    return Key.Unknown;
            }
        }

        private ModifierKeys MapModifierKeys(Keymod mod)
        {
            ModifierKeys mods = ModifierKeys.None;
            if ((mod & (Keymod.Lshift | Keymod.Rshift)) != 0)
            {
                mods |= ModifierKeys.Shift;
            }
            if ((mod & (Keymod.Lalt | Keymod.Ralt)) != 0)
            {
                mods |= ModifierKeys.Alt;
            }
            if ((mod & (Keymod.Lctrl | Keymod.Rctrl)) != 0)
            {
                mods |= ModifierKeys.Control;
            }

            return mods;
        }

        private void HandleWindowEvent(WindowEvent windowEvent)
        {
            switch ((WindowEventID)windowEvent.Event)
            {
                case WindowEventID.Resized:
                case WindowEventID.SizeChanged:
                case WindowEventID.Minimized:
                case WindowEventID.Maximized:
                case WindowEventID.Restored:
                    HandleResizedMessage();
                    break;
                case WindowEventID.FocusGained:
                    FocusGained?.Invoke();
                    break;
                case WindowEventID.FocusLost:
                    FocusLost?.Invoke();
                    break;
                case WindowEventID.Close:
                    Close();
                    break;
                case WindowEventID.Shown:
                    Shown?.Invoke();
                    break;
                case WindowEventID.Hidden:
                    Hidden?.Invoke();
                    break;
                case WindowEventID.Enter:
                    MouseEntered?.Invoke();
                    break;
                case WindowEventID.Leave:
                    MouseLeft?.Invoke();
                    break;
                case WindowEventID.Exposed:
                    Exposed?.Invoke();
                    break;
                case WindowEventID.Moved:
                    _cachedPosition.Value = new Point(windowEvent.Data1, windowEvent.Data2);
                    Moved?.Invoke(new Point(windowEvent.Data1, windowEvent.Data2));
                    break;
                default:
                    Debug.WriteLine("Unhandled SDL WindowEvent: " + windowEvent.Event);
                    break;
            }
        }

        private void HandleResizedMessage()
        {
            RefreshCachedSize();
            Resized?.Invoke();
        }

        private void RefreshCachedSize()
        {
            int w, h;
            SDL.GetWindowSize(_window, &w, &h);
            _cachedSize.Value = new Point(w, h);
        }

        private void RefreshCachedPosition()
        {
            int x, y;
            SDL.GetWindowPosition(_window, &x, &y);
            _cachedPosition.Value = new Point(x, y);
        }

        private MouseState GetCurrentMouseState()
        {
            return new MouseState(
                _currentMouseX, _currentMouseY,
                _currentMouseButtonStates[0], _currentMouseButtonStates[1],
                _currentMouseButtonStates[2], _currentMouseButtonStates[3],
                _currentMouseButtonStates[4], _currentMouseButtonStates[5],
                _currentMouseButtonStates[6], _currentMouseButtonStates[7],
                _currentMouseButtonStates[8], _currentMouseButtonStates[9],
                _currentMouseButtonStates[10], _currentMouseButtonStates[11],
                _currentMouseButtonStates[12]);
        }

        public Point ScreenToClient(Point p)
        {
            Point position = _cachedPosition;
            return new Point(p.X - position.X, p.Y - position.Y);
        }

        private void SetWindowPosition(int x, int y)
        {
            SDL.SetWindowPosition(_window, x, y);
            _cachedPosition.Value = new Point(x, y);
        }

        private Point GetWindowSize()
        {
            return _cachedSize;
        }

        private void SetWindowSize(int width, int height)
        {
            SDL.SetWindowSize(_window, width, height);
            _cachedSize.Value = new Point(width, height);
        }

        private IntPtr GetUnderlyingWindowHandle()
        {
            SysWMInfo wmInfo;
            SDL.GetVersion(&wmInfo.Version);
            SDL.GetWindowWMInfo(_window, &wmInfo);
            if (wmInfo.Subsystem == SysWMType.Windows)
            {
                var win32Info = wmInfo.Info.Win;
                return win32Info.Hwnd;
            }

            return (IntPtr)_window;
        }

        private class SimpleInputSnapshot : InputSnapshot
        {
            public List<KeyEvent> KeyEventsList { get; private set; } = new List<KeyEvent>();
            public List<MouseEvent> MouseEventsList { get; private set; } = new List<MouseEvent>();
            public List<char> KeyCharPressesList { get; private set; } = new List<char>();

            public IReadOnlyList<KeyEvent> KeyEvents => KeyEventsList;

            public IReadOnlyList<MouseEvent> MouseEvents => MouseEventsList;

            public IReadOnlyList<char> KeyCharPresses => KeyCharPressesList;

            public Vector2 MousePosition { get; set; }

            private bool[] _mouseDown = new bool[13];
            public bool[] MouseDown => _mouseDown;
            public float WheelDelta { get; set; }

            public bool IsMouseDown(MouseButton button)
            {
                return _mouseDown[(int)button];
            }

            internal void Clear()
            {
                KeyEventsList.Clear();
                MouseEventsList.Clear();
                KeyCharPressesList.Clear();
                WheelDelta = 0f;
            }

            public void CopyTo(SimpleInputSnapshot other)
            {
                Debug.Assert(this != other);

                other.MouseEventsList.Clear();
                foreach (var me in MouseEventsList) { other.MouseEventsList.Add(me); }

                other.KeyEventsList.Clear();
                foreach (var ke in KeyEventsList) { other.KeyEventsList.Add(ke); }

                other.KeyCharPressesList.Clear();
                foreach (var kcp in KeyCharPressesList) { other.KeyCharPressesList.Add(kcp); }

                other.MousePosition = MousePosition;
                other.WheelDelta = WheelDelta;
                _mouseDown.CopyTo(other._mouseDown, 0);
            }
        }

        private class WindowParams
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string Title { get; set; }
            public WindowFlags WindowFlags { get; set; }

            public IntPtr WindowHandle { get; set; }

            public ManualResetEvent ResetEvent { get; set; }

            public Window* Create()
            {
                if (WindowHandle != IntPtr.Zero)
                {
                    return SdlProvider.SDL.Value.CreateWindowFrom(WindowHandle);
                }
                else
                {
                    return SdlProvider.SDL.Value.CreateWindow(Title, X, Y, Width, Height, (uint)WindowFlags);
                }
            }
        }
    }

    public struct MouseState
    {
        public readonly int X;
        public readonly int Y;

        private bool _mouseDown0;
        private bool _mouseDown1;
        private bool _mouseDown2;
        private bool _mouseDown3;
        private bool _mouseDown4;
        private bool _mouseDown5;
        private bool _mouseDown6;
        private bool _mouseDown7;
        private bool _mouseDown8;
        private bool _mouseDown9;
        private bool _mouseDown10;
        private bool _mouseDown11;
        private bool _mouseDown12;

        public MouseState(
            int x, int y,
            bool mouse0, bool mouse1, bool mouse2, bool mouse3, bool mouse4, bool mouse5, bool mouse6,
            bool mouse7, bool mouse8, bool mouse9, bool mouse10, bool mouse11, bool mouse12)
        {
            X = x;
            Y = y;
            _mouseDown0 = mouse0;
            _mouseDown1 = mouse1;
            _mouseDown2 = mouse2;
            _mouseDown3 = mouse3;
            _mouseDown4 = mouse4;
            _mouseDown5 = mouse5;
            _mouseDown6 = mouse6;
            _mouseDown7 = mouse7;
            _mouseDown8 = mouse8;
            _mouseDown9 = mouse9;
            _mouseDown10 = mouse10;
            _mouseDown11 = mouse11;
            _mouseDown12 = mouse12;
        }

        public bool IsButtonDown(MouseButton button)
        {
            uint index = (uint)button;
            switch (index)
            {
                case 0:
                    return _mouseDown0;
                case 1:
                    return _mouseDown1;
                case 2:
                    return _mouseDown2;
                case 3:
                    return _mouseDown3;
                case 4:
                    return _mouseDown4;
                case 5:
                    return _mouseDown5;
                case 6:
                    return _mouseDown6;
                case 7:
                    return _mouseDown7;
                case 8:
                    return _mouseDown8;
                case 9:
                    return _mouseDown9;
                case 10:
                    return _mouseDown10;
                case 11:
                    return _mouseDown11;
                case 12:
                    return _mouseDown12;
            }

            throw new ArgumentOutOfRangeException(nameof(button));
        }
    }

    public struct MouseWheelEventArgs
    {
        public MouseState State { get; }
        public float WheelDelta { get; }
        public MouseWheelEventArgs(MouseState mouseState, float wheelDelta)
        {
            State = mouseState;
            WheelDelta = wheelDelta;
        }
    }

    public struct MouseMoveEventArgs
    {
        public MouseState State { get; }
        public Vector2 MousePosition { get; }
        public MouseMoveEventArgs(MouseState mouseState, Vector2 mousePosition)
        {
            State = mouseState;
            MousePosition = mousePosition;
        }
    }

    [DebuggerDisplay("{DebuggerDisplayString,nq}")]
    public class BufferedValue<T> where T : struct
    {
        public T Value
        {
            get => Current.Value;
            set
            {
                Back.Value = value;
                Back = Interlocked.Exchange(ref Current, Back);
            }
        }

        private ValueHolder Current = new ValueHolder();
        private ValueHolder Back = new ValueHolder();

        public static implicit operator T(BufferedValue<T> bv) => bv.Value;

        private string DebuggerDisplayString => $"{Current.Value}";

        private class ValueHolder
        {
            public T Value;
        }
    }

    public delegate void SDLEventHandler(ref Event ev);
}
