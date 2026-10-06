namespace GameMods.Subnautica.VRCheats;

internal static class StartupLog
{
    private static readonly object Sync = new();

    public static string Path { get; } =
        System.IO.Path.Combine(AppContext.BaseDirectory, "SubnauticaVRCheats.log");

    public static void Reset()
    {
        lock (Sync)
        {
            try
            {
                File.WriteAllText(
                    Path,
                    $"Subnautica VR Cheats startup log{Environment.NewLine}");
            }
            catch
            {
                // Logging must never stop the app from attempting startup.
            }
        }
    }

    public static void Write(string message)
    {
        lock (Sync)
        {
            try
            {
                File.AppendAllText(
                    Path,
                    $"[{DateTimeOffset.Now:O}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Logging must never become a new failure mode.
            }
        }
    }
}
