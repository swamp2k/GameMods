# Subnautica VR Cheat Menu

Small OpenVR/SteamVR overlay for using Subnautica console commands while playing with the Submersed VR mod.

## MVP status

This first slice intentionally does only one command so the VR plumbing can be tested before the menu grows.

- Quest/Oculus Touch default binding
- Toggle overlay: **hold both grips + click the right thumbstick**
- HMD-relative in-game overlay
- Point/click interaction through SteamVR
- **OXYGEN** test button
- **CLOSE** button
- Sends `Shift+Enter`, the command text, then `Enter` to `Subnautica.exe`

## Build

Requirements:

- Windows x64
- .NET 8 SDK
- SteamVR

From this directory:

```powershell
dotnet restore
dotnet build -c Release
```

Run:

```powershell
dotnet run -c Release
```

Or launch the built `SubnauticaVRCheats.exe` while SteamVR is running.

## Test

1. Start SteamVR and connect the Quest/Touch controllers.
2. Start this app.
3. Start Subnautica with Submersed.
4. Hold both grip buttons and click the **right thumbstick**.
5. The overlay should appear in front of the headset.
6. Point at **OXYGEN** and click it.
7. The overlay hides, Subnautica is focused, and the app sends `oxygen` through the game's developer console.

If SteamVR does not pick up the bundled binding automatically, open SteamVR Controller Bindings for **Subnautica VR Cheats** and select the supplied Quest Touch binding.

## Safety / known limitations

- Console commands disable achievements and can affect saves. Save first.
- The command sender refuses to type unless it can verify that `Subnautica.exe` is the foreground process.
- Only Quest/Oculus Touch has a bundled binding in the MVP.
- Overlay placement, texture orientation and pointer hit-testing still need a real headset test.
- The current OpenVR wrapper is OVRSharp 1.2.0. It bundles `openvr_api.dll`; if current SteamVR exposes a compatibility issue, replacing the wrapper with Valve's current generated C# binding is the next move.

## Next slice

Once the toggle and click path work on Balder's setup, add a real command grid: `nocost`, `nodamage`, `day`, `night`, `invisible`, vehicle spawns, and configurable commands.
