using System.Runtime.InteropServices;
using Valve.VR;

namespace GameMods.Subnautica.VRCheats;

internal sealed class SteamVrInput
{
    private const string ActionSetPath = "/actions/global";
    private const string ToggleActionPath = "/actions/global/in/togglemenu";

    private readonly ulong _actionSetHandle;
    private readonly ulong _toggleActionHandle;
    private readonly uint _activeActionSetSize;
    private readonly uint _digitalActionDataSize;

    public SteamVrInput(string actionManifestPath)
    {
        if (!File.Exists(actionManifestPath))
            throw new FileNotFoundException("SteamVR action manifest was not found.", actionManifestPath);

        ThrowIfError(
            OpenVR.Input.SetActionManifestPath(Path.GetFullPath(actionManifestPath)),
            "SetActionManifestPath");

        ulong actionSet = 0;
        ThrowIfError(OpenVR.Input.GetActionSetHandle(ActionSetPath, ref actionSet), "GetActionSetHandle");
        _actionSetHandle = actionSet;

        ulong toggleAction = 0;
        ThrowIfError(OpenVR.Input.GetActionHandle(ToggleActionPath, ref toggleAction), "GetActionHandle");
        _toggleActionHandle = toggleAction;

        _activeActionSetSize = (uint)Marshal.SizeOf<VRActiveActionSet_t>();
        _digitalActionDataSize = (uint)Marshal.SizeOf<InputDigitalActionData_t>();
    }

    public bool ToggleMenuPressed()
    {
        var activeSets = new[]
        {
            new VRActiveActionSet_t
            {
                ulActionSet = _actionSetHandle,
                ulRestrictedToDevice = OpenVR.k_ulInvalidInputValueHandle,
                ulSecondaryActionSet = 0,
                unPadding = 0,
                nPriority = 100
            }
        };

        var updateError = OpenVR.Input.UpdateActionState(activeSets, _activeActionSetSize);
        if (updateError != EVRInputError.None)
            return false;

        var data = new InputDigitalActionData_t();
        var dataError = OpenVR.Input.GetDigitalActionData(
            _toggleActionHandle,
            ref data,
            _digitalActionDataSize,
            OpenVR.k_ulInvalidInputValueHandle);

        return dataError == EVRInputError.None &&
               data.bActive &&
               data.bChanged &&
               data.bState;
    }

    private static void ThrowIfError(EVRInputError error, string operation)
    {
        if (error != EVRInputError.None)
            throw new InvalidOperationException($"{operation} failed: {error}");
    }
}
