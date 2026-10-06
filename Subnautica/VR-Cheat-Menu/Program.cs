using OVRSharp;

namespace GameMods.Subnautica.VRCheats;

internal static class Program
{
    private static readonly ManualResetEventSlim ExitRequested = new(false);

    public static int Main()
    {
        Application? vrApplication = null;
        VrOverlayMenu? menu = null;

        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            ExitRequested.Set();
        };

        AppDomain.CurrentDomain.ProcessExit += (_, _) => ExitRequested.Set();

        try
        {
            vrApplication = new Application(Application.ApplicationType.Overlay);

            var manifestPath = Path.Combine(AppContext.BaseDirectory, "bindings", "actions.json");
            var input = new SteamVrInput(manifestPath);

            menu = new VrOverlayMenu();
            menu.CommandRequested += command =>
            {
                menu.Hide();

                if (!ConsoleCommandSender.TrySend(command, out var message))
                    Console.Error.WriteLine(message);
                else
                    Console.WriteLine(message);
            };

            Console.WriteLine("Subnautica VR Cheats is running.");
            Console.WriteLine("Toggle menu: hold both grips + click the right thumbstick.");

            while (!ExitRequested.IsSet)
            {
                if (input.ToggleMenuPressed())
                    menu.Toggle();

                menu.PollEvents();
                Thread.Sleep(10);
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex}");
            return 1;
        }
        finally
        {
            try
            {
                menu?.Dispose();
            }
            catch
            {
                // Best-effort shutdown.
            }

            vrApplication?.Shutdown();
        }
    }
}
