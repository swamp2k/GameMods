using OVRSharp;

namespace GameMods.Subnautica.VRCheats;

internal static class Program
{
    private static readonly ManualResetEventSlim ExitRequested = new(false);

    public static int Main()
    {
        Application? vrApplication = null;
        VrOverlayMenu? menu = null;

        StartupLog.Reset();
        StartupLog.Write("Process started.");

        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            ExitRequested.Set();
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) => ExitRequested.Set();

        try
        {
            StartupLog.Write("Initializing OpenVR as overlay application...");
            vrApplication = new Application(Application.ApplicationType.Overlay);
            StartupLog.Write("OpenVR initialized.");

            var manifestPath = Path.Combine(AppContext.BaseDirectory, "bindings", "actions.json");
            StartupLog.Write($"Loading SteamVR action manifest: {manifestPath}");
            var input = new SteamVrInput(manifestPath);
            StartupLog.Write("SteamVR input initialized.");

            StartupLog.Write("Creating VR overlay...");
            menu = new VrOverlayMenu();
            StartupLog.Write("VR overlay created.");

            menu.CommandRequested += command =>
            {
                menu.Hide();

                if (!ConsoleCommandSender.TrySend(command, out var message))
                {
                    Console.Error.WriteLine(message);
                    StartupLog.Write(message);
                }
                else
                {
                    Console.WriteLine(message);
                    StartupLog.Write(message);
                }
            };

            Console.WriteLine("Subnautica VR Cheats is running.");
            Console.WriteLine("Toggle menu: hold both grips + click the right thumbstick.");
            Console.WriteLine($"Log: {StartupLog.Path}");
            StartupLog.Write("Startup complete; entering main loop.");

            while (!ExitRequested.IsSet)
            {
                if (input.ToggleMenuPressed())
                    menu.Toggle();

                menu.PollEvents();
                Thread.Sleep(10);
            }

            StartupLog.Write("Exit requested.");
            return 0;
        }
        catch (Exception ex)
        {
            StartupLog.Write($"FATAL: {ex}");
            Console.Error.WriteLine();
            Console.Error.WriteLine("Subnautica VR Cheats failed to start.");
            Console.Error.WriteLine(ex);
            Console.Error.WriteLine();
            Console.Error.WriteLine($"Log written to: {StartupLog.Path}");
            Console.Error.WriteLine("Press Enter to close...");
            Console.ReadLine();
            return 1;
        }
        finally
        {
            try
            {
                menu?.Dispose();
            }
            catch (Exception ex)
            {
                StartupLog.Write($"Overlay shutdown warning: {ex}");
            }

            try
            {
                vrApplication?.Shutdown();
            }
            catch (Exception ex)
            {
                StartupLog.Write($"OpenVR shutdown warning: {ex}");
            }
        }
    }
}
