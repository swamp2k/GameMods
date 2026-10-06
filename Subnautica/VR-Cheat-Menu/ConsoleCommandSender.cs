using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameMods.Subnautica.VRCheats;

internal static class ConsoleCommandSender
{
    private const ushort VkReturn = 0x0D;
    private const ushort VkRightShift = 0xA1;
    private const uint InputKeyboard = 1;
    private const uint KeyEventKeyUp = 0x0002;
    private const uint KeyEventUnicode = 0x0004;

    public static bool TrySend(string command, out string message)
    {
        var process = Process.GetProcessesByName("Subnautica")
            .FirstOrDefault(p => !p.HasExited);

        if (process is null)
        {
            message = "Subnautica.exe was not found. Command not sent.";
            return false;
        }

        if (!TryFocus(process))
        {
            message = "Could not safely focus Subnautica. Command not sent.";
            return false;
        }

        // Current Subnautica builds open the developer console with Shift+Enter.
        KeyDown(VkRightShift);
        KeyDown(VkReturn);
        KeyUp(VkReturn);
        KeyUp(VkRightShift);

        Thread.Sleep(80);

        foreach (var ch in command)
            UnicodeKey(ch);

        KeyDown(VkReturn);
        KeyUp(VkReturn);

        message = $"Sent Subnautica console command: {command}";
        return true;
    }

    private static bool TryFocus(Process process)
    {
        if (IsForegroundProcess(process.Id))
            return true;

        var window = process.MainWindowHandle;
        if (window == IntPtr.Zero)
            return false;

        _ = SetForegroundWindow(window);
        Thread.Sleep(100);

        return IsForegroundProcess(process.Id);
    }

    private static bool IsForegroundProcess(int processId)
    {
        var foreground = GetForegroundWindow();
        if (foreground == IntPtr.Zero)
            return false;

        _ = GetWindowThreadProcessId(foreground, out var foregroundProcessId);
        return foregroundProcessId == (uint)processId;
    }

    private static void KeyDown(ushort virtualKey) => SendVirtualKey(virtualKey, false);
    private static void KeyUp(ushort virtualKey) => SendVirtualKey(virtualKey, true);

    private static void SendVirtualKey(ushort virtualKey, bool keyUp)
    {
        var input = new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    VirtualKey = virtualKey,
                    Flags = keyUp ? KeyEventKeyUp : 0
                }
            }
        };

        Send(input);
    }

    private static void UnicodeKey(char ch)
    {
        var down = new Input
        {
            Type = InputKeyboard,
            Data = new InputUnion
            {
                Keyboard = new KeyboardInput
                {
                    ScanCode = ch,
                    Flags = KeyEventUnicode
                }
            }
        };

        var up = down;
        up.Data.Keyboard.Flags = KeyEventUnicode | KeyEventKeyUp;

        Send(down, up);
    }

    private static void Send(params Input[] inputs)
    {
        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        if (sent != inputs.Length)
            throw new InvalidOperationException($"SendInput sent {sent} of {inputs.Length} events.");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public KeyboardInput Keyboard;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int size);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr window);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);
}
