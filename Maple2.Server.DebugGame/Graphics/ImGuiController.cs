using ImGuiNET;
using Maple2.Server.DebugGame.Graphics.Ui.Windows;
using Serilog;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;

namespace Maple2.Server.DebugGame.Graphics;

public enum ImGuiWindowType {
    NoToolbar,
    Main,
    Field
}

public class ImGuiController {
    public static readonly ILogger Logger = Log.Logger.ForContext<ImGuiController>();

    public DebugGraphicsContext Context { get; init; }
    public IWindow? ParentWindow { get; private set; }
    public IInputContext Input { get; init; }
    public IntPtr ImGuiContext { get; private set; }
    public ImGuiWindowType WindowType { get; init; }
    private List<char> pressedCharacters;

    private List<IUiWindow> uiWindows = new();
    private Dictionary<Type, IUiWindow> uiWindowMap = new();
    private static bool _initializedReflectedTypes = false;
    private static IEnumerable<Type> _uiWindowTypes = Array.Empty<Type>();

    public ImGuiController(DebugGraphicsContext context, IInputContext input, ImGuiWindowType windowType = ImGuiWindowType.NoToolbar) {
        Context = context;
        Input = input;
        pressedCharacters = new List<char>();
        WindowType = windowType;

        InitializeWindows(null);
    }

    public ImGuiController(DebugGraphicsContext context, IInputContext input, DebugFieldWindow fieldWindow) {
        Context = context;
        Input = input;
        pressedCharacters = new List<char>();
        WindowType = ImGuiWindowType.Field;

        InitializeWindows(fieldWindow);
    }

    private void InitializeWindows(DebugFieldWindow? fieldWindow) {
        if (WindowType == ImGuiWindowType.NoToolbar) {
            return;
        }

        if (!_initializedReflectedTypes) {
            _initializedReflectedTypes = true;

            _uiWindowTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(t => t.GetInterfaces().Contains(typeof(IUiWindow)));
        }

        foreach (Type type in _uiWindowTypes) {
            var windowObject = Activator.CreateInstance(type);

            if (windowObject is not IUiWindow window) {
                throw new InvalidCastException($"Type {type.Name} doesn't implement IUiWindow");
            }

            bool windowIsAllowed = WindowType switch {
                ImGuiWindowType.NoToolbar => false,
                ImGuiWindowType.Main => window.AllowMainWindow,
                ImGuiWindowType.Field => window.AllowFieldWindow,
                _ => false
            };

            if (!windowIsAllowed) {
                continue;
            }

            uiWindows.Add(window);
            uiWindowMap.Add(((dynamic) window).GetType(), window);
        }

        foreach (IUiWindow window in uiWindows) {
            window.Initialize(Context, this, fieldWindow);
        }
    }

    public unsafe void Initialize(IWindow window) {
        ParentWindow = window;

        ImGuiContext = ImGui.CreateContext();
        ImGui.SetCurrentContext(ImGuiContext);

        ImGuiNative.igImGui_ImplDX11_Init(Context.DxDevice, Context.DxDeviceContext);

        SetImGuiWindowData(1.0f / 60.0f);

        Input.ConnectionChanged += OnConnectionChanged;

        foreach (IKeyboard keyboard in Input.Keyboards) {
            RegisterKeyboard(keyboard);
        }
    }

    public void CleanUp() {
        if (ImGui.GetCurrentContext() != ImGuiContext) {
            ImGui.SetCurrentContext(ImGuiContext);
        }

        ImGuiNative.igImGui_ImplDX11_Shutdown();
        ImGui.DestroyContext(ImGuiContext);

        ImGuiContext = default;

        Input.ConnectionChanged -= OnConnectionChanged;

        foreach (IKeyboard keyboard in Input.Keyboards) {
            UnregisterKeyboard(keyboard);
        }
    }

    private void SetImGuiWindowData(float delta) {
        Vector2D<int> frameSize = ParentWindow!.FramebufferSize;

        ImGuiIOPtr io = ImGui.GetIO();
        io.DisplaySize = new Vector2(frameSize.X, frameSize.Y);
        io.DisplayFramebufferScale = new Vector2(1, 1);
        io.DeltaTime = delta;
    }

    public void BeginFrame(float delta) {
        if (ImGui.GetCurrentContext() != ImGuiContext) {
            ImGui.SetCurrentContext(ImGuiContext);
        }

        SetImGuiWindowData(delta);

        UpdateInput();

        ImGuiNative.igImGui_ImplDX11_NewFrame();
        ImGui.NewFrame();

        RenderWindows();
    }

    public unsafe void EndFrame() {
        if (ImGui.GetCurrentContext() != ImGuiContext) {
            ImGui.SetCurrentContext(ImGuiContext);
        }

        ImGui.Render();
        ImGuiNative.igImGui_ImplDX11_RenderDrawData(ImGui.GetDrawData().NativePtr);
    }

    public T? GetUiWindow<T>() where T : IUiWindow {
        Type type = typeof(T);

        uiWindowMap.TryGetValue(type, out IUiWindow? window);

        if (window is not T value) {
            return default;
        }

        return value;
    }

    public void RenderWindows() {
        if (WindowType == ImGuiWindowType.NoToolbar) {
            return;
        }

        float height = ImGui.CalcTextSize("M").Y;// + 2 * ImGui.GetStyle().FramePadding.Y;
        float width = ParentWindow!.FramebufferSize.X;

        ImGui.SetNextWindowPos(new Vector2(0, 0));
        ImGui.SetNextWindowSize(new Vector2(width, height));

        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoTitleBar;
        windowFlags |= ImGuiWindowFlags.NoResize;
        windowFlags |= ImGuiWindowFlags.NoMove;
        windowFlags |= ImGuiWindowFlags.NoScrollbar;
        windowFlags |= ImGuiWindowFlags.NoSavedSettings;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);

        height = ImGui.CalcTextSize("M").Y;// + 2 * ImGui.GetStyle().FramePadding.Y;

        ImGui.SetWindowSize(new Vector2(width, height));

        ImGui.Begin("##TOOLBAR", windowFlags);

        ImGuiComboFlags comboFlags = ImGuiComboFlags.NoArrowButton;
        comboFlags |= ImGuiComboFlags.HeightLargest;
        comboFlags |= ImGuiComboFlags.WidthFitPreview;

        if (ImGui.BeginCombo("##TOOLBAR COMBO", "View", comboFlags)) {
            foreach (IUiWindow window in uiWindows) {
                bool toggle = ImGui.Selectable(window.TypeName, window.Enabled);

                if (toggle) {
                    window.Enabled ^= true;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.PopStyleVar(2); // WindowBorderSize, WindowPadding

        ImGui.End();


        foreach (IUiWindow window in uiWindows) {
            if (!window.Enabled) {
                continue;
            }

            window.Render();
        }
    }

    private void RegisterKeyboard(IKeyboard keyboard) {
        keyboard.KeyUp += OnKeyUp;
        keyboard.KeyDown += OnKeyDown;
        keyboard.KeyChar += OnKeyChar;
    }

    private void UnregisterKeyboard(IKeyboard keyboard) {
        keyboard.KeyUp -= OnKeyUp;
        keyboard.KeyDown -= OnKeyDown;
        keyboard.KeyChar -= OnKeyChar;
    }

    private void OnConnectionChanged(IInputDevice device, bool connected) {
        if (ImGui.GetCurrentContext() != ImGuiContext) {
            ImGui.SetCurrentContext(ImGuiContext);
        }

        if (device is not IKeyboard keyboard) {
            return;
        }

        if (connected) {
            RegisterKeyboard(keyboard);
        } else {
            UnregisterKeyboard(keyboard);
        }
    }

    private static void OnKeyEvent(IKeyboard keyboard, Key keyCode, int scanCode, bool isDown) {
        ImGuiIOPtr io = ImGui.GetIO();
        ImGuiKey key = SilkKeyToImGui(keyCode);
        io.AddKeyEvent(key, isDown);
    }

    private void OnKeyDown(IKeyboard keyboard, Key keyCode, int scanCode) {
        OnKeyEvent(keyboard, keyCode, scanCode, isDown: true);
    }

    private void OnKeyUp(IKeyboard keyboard, Key keyCode, int scanCode) {
        OnKeyEvent(keyboard, keyCode, scanCode, isDown: false);
    }

    private void OnKeyChar(IKeyboard keyboard, char character) {
        pressedCharacters.Add(character);
    }

    private void UpdateInput() {
        ImGuiIOPtr io = ImGui.GetIO();

        io.MouseDown[0] = false;
        io.MouseDown[1] = false;
        io.MouseDown[2] = false;

        foreach (IMouse currentMouse in Input.Mice) {
            io.MouseDown[0] |= currentMouse.IsButtonPressed(MouseButton.Left);
            io.MouseDown[1] |= currentMouse.IsButtonPressed(MouseButton.Right);
            io.MouseDown[2] |= currentMouse.IsButtonPressed(MouseButton.Middle);
        }

        IMouse? mouse = Input.Mice.Count > 0 ? Input.Mice[0] : null;

        if (mouse is not null) {
            Point mousePos = new Point((int) mouse.Position.X, (int) mouse.Position.Y);

            io.MousePos = new Vector2(mousePos.X, mousePos.Y);

            ScrollWheel? wheel = mouse.ScrollWheels.Count > 0 ? mouse.ScrollWheels[0] : null;

            if (wheel is not null) {
                io.MouseWheel = wheel.Value.Y;
                io.MouseWheelH = wheel.Value.X;
            }
        }

        foreach (char character in pressedCharacters) {
            io.AddInputCharacter(character);
        }

        pressedCharacters.Clear();

        io.KeyCtrl = false;
        io.KeyAlt = false;
        io.KeyShift = false;
        io.KeySuper = false;

        foreach (IKeyboard keyboard in Input.Keyboards) {
            io.KeyCtrl |= keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight);
            io.KeyAlt |= keyboard.IsKeyPressed(Key.AltLeft) || keyboard.IsKeyPressed(Key.AltRight);
            io.KeyShift |= keyboard.IsKeyPressed(Key.ShiftLeft) || keyboard.IsKeyPressed(Key.ShiftRight);
            io.KeySuper |= keyboard.IsKeyPressed(Key.SuperLeft) || keyboard.IsKeyPressed(Key.SuperRight);
        }
    }

    public static ImGuiKey SilkKeyToImGui(Key key) {
        // from Silk.NET.OpenGL.Extensions.ImGui sample
        return key switch {
            Key.Tab => ImGuiKey.Tab,
            Key.Left => ImGuiKey.LeftArrow,
            Key.Right => ImGuiKey.RightArrow,
            Key.Up => ImGuiKey.UpArrow,
            Key.Down => ImGuiKey.DownArrow,
            Key.PageUp => ImGuiKey.PageUp,
            Key.PageDown => ImGuiKey.PageDown,
            Key.Home => ImGuiKey.Home,
            Key.End => ImGuiKey.End,
            Key.Insert => ImGuiKey.Insert,
            Key.Delete => ImGuiKey.Delete,
            Key.Backspace => ImGuiKey.Backspace,
            Key.Space => ImGuiKey.Space,
            Key.Enter => ImGuiKey.Enter,
            Key.Escape => ImGuiKey.Escape,
            Key.Apostrophe => ImGuiKey.Apostrophe,
            Key.Comma => ImGuiKey.Comma,
            Key.Minus => ImGuiKey.Minus,
            Key.Period => ImGuiKey.Period,
            Key.Slash => ImGuiKey.Slash,
            Key.Semicolon => ImGuiKey.Semicolon,
            Key.Equal => ImGuiKey.Equal,
            Key.LeftBracket => ImGuiKey.LeftBracket,
            Key.BackSlash => ImGuiKey.Backslash,
            Key.RightBracket => ImGuiKey.RightBracket,
            Key.GraveAccent => ImGuiKey.GraveAccent,
            Key.CapsLock => ImGuiKey.CapsLock,
            Key.ScrollLock => ImGuiKey.ScrollLock,
            Key.NumLock => ImGuiKey.NumLock,
            Key.PrintScreen => ImGuiKey.PrintScreen,
            Key.Pause => ImGuiKey.Pause,
            Key.Keypad0 => ImGuiKey.Keypad0,
            Key.Keypad1 => ImGuiKey.Keypad1,
            Key.Keypad2 => ImGuiKey.Keypad2,
            Key.Keypad3 => ImGuiKey.Keypad3,
            Key.Keypad4 => ImGuiKey.Keypad4,
            Key.Keypad5 => ImGuiKey.Keypad5,
            Key.Keypad6 => ImGuiKey.Keypad6,
            Key.Keypad7 => ImGuiKey.Keypad7,
            Key.Keypad8 => ImGuiKey.Keypad8,
            Key.Keypad9 => ImGuiKey.Keypad9,
            Key.KeypadDecimal => ImGuiKey.KeypadDecimal,
            Key.KeypadDivide => ImGuiKey.KeypadDivide,
            Key.KeypadMultiply => ImGuiKey.KeypadMultiply,
            Key.KeypadSubtract => ImGuiKey.KeypadSubtract,
            Key.KeypadAdd => ImGuiKey.KeypadAdd,
            Key.KeypadEnter => ImGuiKey.KeypadEnter,
            Key.KeypadEqual => ImGuiKey.KeypadEqual,
            Key.ShiftLeft => ImGuiKey.LeftShift,
            Key.ControlLeft => ImGuiKey.LeftCtrl,
            Key.AltLeft => ImGuiKey.LeftAlt,
            Key.SuperLeft => ImGuiKey.LeftSuper,
            Key.ShiftRight => ImGuiKey.RightShift,
            Key.ControlRight => ImGuiKey.RightCtrl,
            Key.AltRight => ImGuiKey.RightAlt,
            Key.SuperRight => ImGuiKey.RightSuper,
            Key.Menu => ImGuiKey.Menu,
            Key.Number0 => ImGuiKey._0,
            Key.Number1 => ImGuiKey._1,
            Key.Number2 => ImGuiKey._2,
            Key.Number3 => ImGuiKey._3,
            Key.Number4 => ImGuiKey._4,
            Key.Number5 => ImGuiKey._5,
            Key.Number6 => ImGuiKey._6,
            Key.Number7 => ImGuiKey._7,
            Key.Number8 => ImGuiKey._8,
            Key.Number9 => ImGuiKey._9,
            Key.A => ImGuiKey.A,
            Key.B => ImGuiKey.B,
            Key.C => ImGuiKey.C,
            Key.D => ImGuiKey.D,
            Key.E => ImGuiKey.E,
            Key.F => ImGuiKey.F,
            Key.G => ImGuiKey.G,
            Key.H => ImGuiKey.H,
            Key.I => ImGuiKey.I,
            Key.J => ImGuiKey.J,
            Key.K => ImGuiKey.K,
            Key.L => ImGuiKey.L,
            Key.M => ImGuiKey.M,
            Key.N => ImGuiKey.N,
            Key.O => ImGuiKey.O,
            Key.P => ImGuiKey.P,
            Key.Q => ImGuiKey.Q,
            Key.R => ImGuiKey.R,
            Key.S => ImGuiKey.S,
            Key.T => ImGuiKey.T,
            Key.U => ImGuiKey.U,
            Key.V => ImGuiKey.V,
            Key.W => ImGuiKey.W,
            Key.X => ImGuiKey.X,
            Key.Y => ImGuiKey.Y,
            Key.Z => ImGuiKey.Z,
            Key.F1 => ImGuiKey.F1,
            Key.F2 => ImGuiKey.F2,
            Key.F3 => ImGuiKey.F3,
            Key.F4 => ImGuiKey.F4,
            Key.F5 => ImGuiKey.F5,
            Key.F6 => ImGuiKey.F6,
            Key.F7 => ImGuiKey.F7,
            Key.F8 => ImGuiKey.F8,
            Key.F9 => ImGuiKey.F9,
            Key.F10 => ImGuiKey.F10,
            Key.F11 => ImGuiKey.F11,
            Key.F12 => ImGuiKey.F12,
            Key.F13 => ImGuiKey.F13,
            Key.F14 => ImGuiKey.F14,
            Key.F15 => ImGuiKey.F15,
            Key.F16 => ImGuiKey.F16,
            Key.F17 => ImGuiKey.F17,
            Key.F18 => ImGuiKey.F18,
            Key.F19 => ImGuiKey.F19,
            Key.F20 => ImGuiKey.F20,
            Key.F21 => ImGuiKey.F21,
            Key.F22 => ImGuiKey.F22,
            Key.F23 => ImGuiKey.F23,
            Key.F24 => ImGuiKey.F24,
            _ => throw new NotImplementedException(),
        };
    }
}
